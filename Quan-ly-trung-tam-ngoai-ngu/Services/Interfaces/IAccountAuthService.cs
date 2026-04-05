using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface IAccountAuthService
{
    Task<IReadOnlyList<DemoAccount>> GetDemoAccountsAsync();
    Task<DemoAccount?> ValidateLoginAsync(string email, string password);
    Task<StudentRegistrationResult> RegisterStudentAsync(string fullName, string email, string phone, string password);
}
