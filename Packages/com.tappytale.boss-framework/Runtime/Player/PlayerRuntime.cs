using System;
using UnityEngine;

namespace TappyTale.BossFight.Player
{
    public sealed class PlayerRuntime
    {
        private readonly PlayerDefinition _definition;
        private readonly PlayerMotor _motor;
        private readonly Transform _camera;
        private readonly PlayerInputBuffer _buffer;
        private float _dashRemaining;
        private Vector3 _dashDirection;
        private bool _airDashConsumed;

        public PlayerRuntime(PlayerDefinition definition, PlayerMotor motor, Transform camera, PlayerInputBuffer buffer)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
            _motor = motor != null ? motor : throw new ArgumentNullException(nameof(motor));
            _camera = camera;
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            Blackboard = new PlayerBlackboard
            {
                AirJumpsRemaining = definition.AdditionalAirJumps
            };
        }

        public PlayerBlackboard Blackboard { get; }
        public event Action<PlayerState, PlayerState> StateChanged;
        public event Action Jumped;
        public event Action DashStarted;
        public event Action DashEnded;

        public void Tick(Vector2 moveInput, float deltaTime)
        {
            Blackboard.MoveInput = moveInput;
            Blackboard.DashCooldownRemaining = Mathf.Max(0f, Blackboard.DashCooldownRemaining - deltaTime);
            UpdateGrounding(deltaTime);

            if (Blackboard.State == PlayerState.Dashing)
            {
                TickDash(deltaTime);
                _buffer.Tick(deltaTime);
                return;
            }

            Vector3 moveDirection = ResolveMoveDirection(moveInput);
            Blackboard.MoveDirection = moveDirection;

            if (_buffer.HasDash && CanDash())
            {
                _buffer.ConsumeDash();
                BeginDash(moveDirection);
                TickDash(deltaTime);
                _buffer.Tick(deltaTime);
                return;
            }

            TryJump();
            TickLocomotion(moveDirection, deltaTime);
            _buffer.Tick(deltaTime);
        }

        public void SetDisabled(bool disabled)
        {
            SetState(disabled ? PlayerState.Disabled : (Blackboard.IsGrounded ? PlayerState.Grounded : PlayerState.Airborne));
        }

        private void UpdateGrounding(float deltaTime)
        {
            Blackboard.IsGrounded = _motor.IsGrounded;
            if (Blackboard.IsGrounded)
            {
                Blackboard.TimeSinceGrounded = 0f;
                Blackboard.AirJumpsRemaining = _definition.AdditionalAirJumps;
                _airDashConsumed = false;
                if (Blackboard.VerticalVelocity < 0f)
                {
                    Blackboard.VerticalVelocity = -2f;
                }

                if (Blackboard.State != PlayerState.Dashing && Blackboard.State != PlayerState.Disabled)
                {
                    SetState(PlayerState.Grounded);
                }
            }
            else
            {
                Blackboard.TimeSinceGrounded += deltaTime;
                if (Blackboard.State != PlayerState.Dashing && Blackboard.State != PlayerState.Disabled)
                {
                    SetState(PlayerState.Airborne);
                }
            }
        }

        private void TickLocomotion(Vector3 direction, float deltaTime)
        {
            if (Blackboard.State == PlayerState.Disabled)
            {
                return;
            }

            Vector3 targetVelocity = direction * _definition.MoveSpeed;
            float control = Blackboard.IsGrounded ? 1f : _definition.AirControl;
            float rate = targetVelocity.sqrMagnitude > Blackboard.HorizontalVelocity.sqrMagnitude
                ? _definition.Acceleration
                : _definition.Deceleration;
            Blackboard.HorizontalVelocity = Vector3.MoveTowards(
                Blackboard.HorizontalVelocity,
                targetVelocity,
                rate * control * deltaTime);

            Blackboard.VerticalVelocity += _definition.Gravity * deltaTime;
            Vector3 velocity = Blackboard.HorizontalVelocity + Vector3.up * Blackboard.VerticalVelocity;
            _motor.Move(velocity, deltaTime);
            _motor.Face(direction, _definition.RotationSharpness, deltaTime);
        }

        private void TryJump()
        {
            if (!_buffer.HasJump || Blackboard.State == PlayerState.Disabled)
            {
                return;
            }

            bool canUseCoyote = Blackboard.IsGrounded || Blackboard.TimeSinceGrounded <= _definition.CoyoteTime;
            bool canUseAirJump = !canUseCoyote && Blackboard.AirJumpsRemaining > 0;
            if (!canUseCoyote && !canUseAirJump)
            {
                return;
            }

            _buffer.ConsumeJump();
            if (canUseAirJump)
            {
                Blackboard.AirJumpsRemaining--;
            }

            Blackboard.VerticalVelocity = Mathf.Sqrt(2f * _definition.JumpHeight * -_definition.Gravity);
            Blackboard.IsGrounded = false;
            Blackboard.TimeSinceGrounded = _definition.CoyoteTime + 1f;
            SetState(PlayerState.Airborne);
            Jumped?.Invoke();
        }

        private bool CanDash()
        {
            if (Blackboard.State == PlayerState.Disabled || Blackboard.DashCooldownRemaining > 0f)
            {
                return false;
            }

            if (Blackboard.IsGrounded)
            {
                return true;
            }

            return _definition.AllowAirDash && !_airDashConsumed;
        }

        private void BeginDash(Vector3 moveDirection)
        {
            _dashDirection = moveDirection.sqrMagnitude > 0.0001f ? moveDirection : _motor.Body.forward;
            _dashDirection.y = 0f;
            _dashDirection.Normalize();
            _dashRemaining = _definition.DashDuration;
            Blackboard.HorizontalVelocity = Vector3.zero;
            Blackboard.VerticalVelocity = 0f;
            Blackboard.DashCooldownRemaining = _definition.DashCooldown;
            Blackboard.IsInvulnerable = _definition.InvulnerableDuringDash;
            if (!Blackboard.IsGrounded)
            {
                _airDashConsumed = true;
            }

            SetState(PlayerState.Dashing);
            DashStarted?.Invoke();
        }

        private void TickDash(float deltaTime)
        {
            _dashRemaining -= deltaTime;
            _motor.Move(_dashDirection * _definition.DashSpeed, deltaTime);
            _motor.Face(_dashDirection, _definition.RotationSharpness * 2f, deltaTime);
            if (_dashRemaining > 0f)
            {
                return;
            }

            Blackboard.IsInvulnerable = false;
            SetState(_motor.IsGrounded ? PlayerState.Grounded : PlayerState.Airborne);
            DashEnded?.Invoke();
        }

        private Vector3 ResolveMoveDirection(Vector2 input)
        {
            Vector3 forward = _camera != null ? _camera.forward : Vector3.forward;
            Vector3 right = _camera != null ? _camera.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void SetState(PlayerState next)
        {
            if (Blackboard.State == next)
            {
                return;
            }

            PlayerState previous = Blackboard.State;
            Blackboard.State = next;
            StateChanged?.Invoke(previous, next);
        }
    }
}
