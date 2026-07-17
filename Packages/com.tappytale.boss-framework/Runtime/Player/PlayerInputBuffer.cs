namespace TappyTale.BossFight.Player
{
    public sealed class PlayerInputBuffer
    {
        private float _jumpRemaining;
        private float _dashRemaining;

        public void Tick(float deltaTime)
        {
            if (_jumpRemaining > 0f) _jumpRemaining -= deltaTime;
            if (_dashRemaining > 0f) _dashRemaining -= deltaTime;
        }

        public void BufferJump(float duration)
        {
            _jumpRemaining = duration;
        }

        public void BufferDash(float duration)
        {
            _dashRemaining = duration;
        }

        public bool ConsumeJump()
        {
            if (_jumpRemaining <= 0f) return false;
            _jumpRemaining = 0f;
            return true;
        }

        public bool ConsumeDash()
        {
            if (_dashRemaining <= 0f) return false;
            _dashRemaining = 0f;
            return true;
        }

        public bool HasJump => _jumpRemaining > 0f;
        public bool HasDash => _dashRemaining > 0f;

        public void Clear()
        {
            _jumpRemaining = 0f;
            _dashRemaining = 0f;
        }
    }
}
