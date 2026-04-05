namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class StudentRegistrationResult
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? StudentCode { get; init; }

    public static StudentRegistrationResult Success(string message, string? studentCode = null)
    {
        return new StudentRegistrationResult
        {
            Succeeded = true,
            Message = message,
            StudentCode = studentCode
        };
    }

    public static StudentRegistrationResult Fail(string message)
    {
        return new StudentRegistrationResult
        {
            Succeeded = false,
            Message = message
        };
    }
}

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
