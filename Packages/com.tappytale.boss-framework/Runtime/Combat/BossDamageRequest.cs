namespace TappyTale.BossFight.Combat
{
    public readonly struct BossDamageRequest
    {
        public BossDamageRequest(float amount, string sourceId = null, bool ignoreInvulnerability = false)
        {
            Amount = amount;
            SourceId = sourceId ?? string.Empty;
            IgnoreInvulnerability = ignoreInvulnerability;
        }

        public float Amount { get; }
        public string SourceId { get; }
        public bool IgnoreInvulnerability { get; }
    }
}
