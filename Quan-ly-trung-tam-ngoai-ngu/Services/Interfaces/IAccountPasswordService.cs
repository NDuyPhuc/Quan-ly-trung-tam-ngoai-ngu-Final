using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface IAccountPasswordService
{
    string HashPassword(AccountEntity account, string password);
    PasswordVerificationResult VerifyPassword(AccountEntity account, string password);
    bool NeedsMigration(string storedValue);
}
