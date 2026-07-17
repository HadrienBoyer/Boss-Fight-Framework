using UnityEngine;

namespace TappyTale.BossFight.Player
{
    [CreateAssetMenu(menuName = "Tappy Tale/Boss Fight/Player Definition", fileName = "Player_")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [Header("Locomotion")]
        [SerializeField, Min(0f)] private float moveSpeed = 8f;
        [SerializeField, Min(0f)] private float acceleration = 55f;
        [SerializeField, Min(0f)] private float deceleration = 70f;
        [SerializeField, Min(0f)] private float airControl = 0.8f;
        [SerializeField, Min(0f)] private float rotationSharpness = 24f;

        [Header("Jump")]
        [SerializeField] private float gravity = -32f;
        [SerializeField, Min(0f)] private float jumpHeight = 2.2f;
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;
        [SerializeField, Min(0)] private int additionalAirJumps = 1;

        [Header("Dash")]
        [SerializeField, Min(0f)] private float dashSpeed = 20f;
        [SerializeField, Min(0.01f)] private float dashDuration = 0.18f;
        [SerializeField, Min(0f)] private float dashCooldown = 0.35f;
        [SerializeField, Min(0f)] private float dashBufferTime = 0.15f;
        [SerializeField] private bool allowAirDash = true;
        [SerializeField] private bool invulnerableDuringDash = true;

        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirControl => airControl;
        public float RotationSharpness => rotationSharpness;
        public float Gravity => gravity;
        public float JumpHeight => jumpHeight;
        public float CoyoteTime => coyoteTime;
        public float JumpBufferTime => jumpBufferTime;
        public int AdditionalAirJumps => additionalAirJumps;
        public float DashSpeed => dashSpeed;
        public float DashDuration => dashDuration;
        public float DashCooldown => dashCooldown;
        public float DashBufferTime => dashBufferTime;
        public bool AllowAirDash => allowAirDash;
        public bool InvulnerableDuringDash => invulnerableDuringDash;
    }
}
