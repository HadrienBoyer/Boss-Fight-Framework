using UnityEngine;

namespace TappyTale.BossFight.Player
{
    public abstract class PlayerInputSource : MonoBehaviour
    {
        public abstract Vector2 Move { get; }
        public abstract bool JumpPressed { get; }
        public abstract bool DashPressed { get; }
    }
}
