using System.Collections.Generic;

namespace TappyTale.BossFight.Editor.Validation
{
    public enum BossValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    public readonly struct BossValidationIssue
    {
        public BossValidationIssue(BossValidationSeverity severity, string message, string assetPath)
        {
            Severity = severity;
            Message = message;
            AssetPath = assetPath;
        }

        public BossValidationSeverity Severity { get; }
        public string Message { get; }
        public string AssetPath { get; }
    }

    public sealed class BossValidationReport
    {
        private readonly List<BossValidationIssue> _issues = new();

        public IReadOnlyList<BossValidationIssue> Issues => _issues;
        public bool HasErrors { get; private set; }
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public void Add(BossValidationSeverity severity, string message, string assetPath = "")
        {
            _issues.Add(new BossValidationIssue(severity, message, assetPath));
            if (severity == BossValidationSeverity.Error)
            {
                HasErrors = true;
                ErrorCount++;
            }
            else if (severity == BossValidationSeverity.Warning)
            {
                WarningCount++;
            }
        }
    }
}
