namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class PasswordVerificationResult
{
    public bool Succeeded { get; init; }
    public bool NeedsUpgrade { get; init; }
    public bool UsedLegacyPlainText { get; init; }
    public string? UpgradedHash { get; init; }

    public static PasswordVerificationResult Success(string? upgradedHash = null, bool usedLegacyPlainText = false)
    {
        return new PasswordVerificationResult
        {
            Succeeded = true,
            NeedsUpgrade = !string.IsNullOrWhiteSpace(upgradedHash),
            UsedLegacyPlainText = usedLegacyPlainText,
            UpgradedHash = upgradedHash
        };
    }

    public static PasswordVerificationResult Fail()
    {
        return new PasswordVerificationResult
        {
            Succeeded = false
        };
    }
}
