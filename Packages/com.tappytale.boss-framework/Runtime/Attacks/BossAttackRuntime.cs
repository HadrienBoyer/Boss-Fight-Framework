using System;

namespace TappyTale.BossFight.Attacks
{
    public sealed class BossAttackRuntime
    {
        public BossAttackRuntime(BossAttackDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public BossAttackDefinition Definition { get; }
        public float CooldownRemaining { get; private set; }
        public bool IsReady => CooldownRemaining <= 0f;

        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= deltaTime;
            }
        }

        public void CommitCooldown()
        {
            CooldownRemaining = Definition.Cooldown;
        }

        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
        }
    }
}
