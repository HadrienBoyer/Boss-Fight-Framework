using UnityEngine;

namespace TappyTale.BossFight.Player
{
    public sealed class PlayerBlackboard
    {
        public PlayerState State { get; internal set; } = PlayerState.Airborne;
        public Vector2 MoveInput { get; internal set; }
        public Vector3 MoveDirection { get; internal set; }
        public Vector3 HorizontalVelocity { get; internal set; }
        public float VerticalVelocity { get; internal set; }
        public bool IsGrounded { get; internal set; }
        public bool IsInvulnerable { get; internal set; }
        public float TimeSinceGrounded { get; internal set; }
        public float DashCooldownRemaining { get; internal set; }
        public int AirJumpsRemaining { get; internal set; }
        public bool IsDashing => State == PlayerState.Dashing;
    }
}
