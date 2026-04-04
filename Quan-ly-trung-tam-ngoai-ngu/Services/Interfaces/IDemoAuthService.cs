using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface IDemoAuthService
{
    IReadOnlyList<DemoAccount> GetDemoAccounts();
    DemoAccount? ValidateLogin(string email, string password);
    StudentRegistrationResult RegisterStudent(string fullName, string email, string phone, string password);
}
