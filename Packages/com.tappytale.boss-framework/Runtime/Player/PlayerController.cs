using UnityEngine;

namespace TappyTale.BossFight.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerDefinition definition;
        [SerializeField] private PlayerInputSource inputSource;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private Transform cameraTransform;

        private PlayerInputBuffer _inputBuffer;
        private PlayerDefinition _runtimeFallbackDefinition;

        public PlayerRuntime Runtime { get; private set; }
        public PlayerBlackboard Blackboard => Runtime?.Blackboard;
        public PlayerDefinition Definition => definition;

        private void Reset()
        {
            motor = GetComponent<PlayerMotor>();
            inputSource = GetComponent<PlayerInputSource>();
        }

        private void Awake()
        {
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (inputSource == null) inputSource = GetComponent<PlayerInputSource>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            BuildRuntime();
        }

        private void Update()
        {
            if (Runtime == null || inputSource == null)
            {
                return;
            }

            if (inputSource.JumpPressed)
            {
                _inputBuffer.BufferJump(RuntimeDefinition.JumpBufferTime);
            }

            if (inputSource.DashPressed)
            {
                _inputBuffer.BufferDash(RuntimeDefinition.DashBufferTime);
            }

            Runtime.Tick(inputSource.Move, Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (_runtimeFallbackDefinition != null)
            {
                Destroy(_runtimeFallbackDefinition);
            }

            Runtime = null;
        }

        public void SetInputSource(PlayerInputSource source)
        {
            inputSource = source;
        }

        public void SetCamera(Transform newCamera)
        {
            cameraTransform = newCamera;
            BuildRuntime();
        }

        public void SetDefinition(PlayerDefinition newDefinition)
        {
            definition = newDefinition;
            BuildRuntime();
        }

        public void SetDisabled(bool disabled)
        {
            Runtime?.SetDisabled(disabled);
        }

        private PlayerDefinition RuntimeDefinition
        {
            get
            {
                if (definition != null)
                {
                    return definition;
                }

                if (_runtimeFallbackDefinition == null)
                {
                    _runtimeFallbackDefinition = ScriptableObject.CreateInstance<PlayerDefinition>();
                    _runtimeFallbackDefinition.hideFlags = HideFlags.HideAndDontSave;
                }

                return _runtimeFallbackDefinition;
            }
        }

        private void BuildRuntime()
        {
            if (motor == null)
            {
                return;
            }

            _inputBuffer = new PlayerInputBuffer();
            Runtime = new PlayerRuntime(RuntimeDefinition, motor, cameraTransform, _inputBuffer);
        }
    }
}
