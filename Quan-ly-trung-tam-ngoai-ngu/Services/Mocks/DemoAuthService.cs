using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Mocks;

public class DemoAuthService : IDemoAuthService
{
    private readonly MockDataService _mockDataService;

    public DemoAuthService(MockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    public IReadOnlyList<DemoAccount> GetDemoAccounts()
    {
        return _mockDataService.GetAccounts();
    }

    public DemoAccount? ValidateLogin(string email, string password)
    {
        return _mockDataService
            .GetAccounts()
            .FirstOrDefault(x =>
                (x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) ||
                 x.Username.Equals(email, StringComparison.OrdinalIgnoreCase)) &&
                (x.PasswordHash == password || x.Password == password));
    }

    public StudentRegistrationResult RegisterStudent(string fullName, string email, string phone, string password)
    {
        return _mockDataService.RegisterStudent(fullName, email, phone);
    }
}
