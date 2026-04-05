using Microsoft.AspNetCore.Identity;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Security;

public sealed class AccountPasswordService : IAccountPasswordService
{
    private readonly PasswordHasher<AccountEntity> _passwordHasher = new();

    public string HashPassword(AccountEntity account, string password)
    {
        return _passwordHasher.HashPassword(account, password.Trim());
    }

    public Models.PasswordVerificationResult VerifyPassword(AccountEntity account, string password)
    {
        if (string.IsNullOrWhiteSpace(account.PasswordHash) || string.IsNullOrWhiteSpace(password))
        {
            return Models.PasswordVerificationResult.Fail();
        }

        try
        {
            var verification = _passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);
            return verification switch
            {
                Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded => Models.PasswordVerificationResult.Success(HashPassword(account, password)),
                Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success => Models.PasswordVerificationResult.Success(),
                _ => VerifyLegacyPlainText(account, password)
            };
        }
        catch (FormatException)
        {
            return VerifyLegacyPlainText(account, password);
        }
    }

    private Models.PasswordVerificationResult VerifyLegacyPlainText(AccountEntity account, string password)
    {
        if (!string.Equals(account.PasswordHash, password, StringComparison.Ordinal))
        {
            return Models.PasswordVerificationResult.Fail();
        }

        return Models.PasswordVerificationResult.Success(HashPassword(account, password), usedLegacyPlainText: true);
    }

    public bool NeedsMigration(string storedValue)
    {
        return !string.IsNullOrWhiteSpace(storedValue) && !storedValue.StartsWith("AQAAAA", StringComparison.Ordinal);
    }
}
