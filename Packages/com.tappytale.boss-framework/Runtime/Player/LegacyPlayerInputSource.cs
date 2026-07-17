using UnityEngine;

namespace TappyTale.BossFight.Player
{
    [DisallowMultipleComponent]
    public sealed class LegacyPlayerInputSource : PlayerInputSource
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

        public override Vector2 Move => Vector2.ClampMagnitude(
            new Vector2(Input.GetAxisRaw(horizontalAxis), Input.GetAxisRaw(verticalAxis)), 1f);

        public override bool JumpPressed => Input.GetKeyDown(jumpKey);
        public override bool DashPressed => Input.GetKeyDown(dashKey);
    }
}
