using Microsoft.EntityFrameworkCore;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Ef;

public sealed class EfAuthService : IAccountAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAccountPasswordService _passwordService;
    private readonly ILogger<EfAuthService> _logger;

    public EfAuthService(
        ApplicationDbContext dbContext,
        IAccountPasswordService passwordService,
        ILogger<EfAuthService> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DemoAccount>> GetDemoAccountsAsync()
    {
        var teacherSpecializations = await _dbContext.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Email != null)
            .ToDictionaryAsync(x => x.Email!, x => x.Specialization ?? string.Empty, StringComparer.OrdinalIgnoreCase);

        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .Select(x => new DemoAccount
            {
                Id = x.Id,
                Username = x.Username,
                FullName = x.FullName,
                Email = x.Email ?? string.Empty,
                Phone = x.Phone ?? string.Empty,
                Role = x.Role,
                Department = ResolveDepartment(x.Role, x.Email, teacherSpecializations),
                Status = EfServiceMapper.MapAccountStatus(x.IsActive, x.Status),
                Password = string.IsNullOrWhiteSpace(x.PasswordHash) ? string.Empty : "********",
                PasswordHash = x.PasswordHash
            })
            .ToListAsync();
    }

    public async Task<DemoAccount?> ValidateLoginAsync(string email, string password)
    {
        var login = email.Trim();
        var normalizedLogin = login.ToLowerInvariant();

        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                ((x.Email != null && x.Email.ToLower() == normalizedLogin) ||
                 x.Username.ToLower() == normalizedLogin));

        if (account is null)
        {
            return null;
        }

        if (!account.IsActive || account.Status != 1)
        {
            return null;
        }

        var verification = _passwordService.VerifyPassword(account, password);
        if (!verification.Succeeded)
        {
            _logger.LogWarning("Password verification failed for account {AccountId}.", account.Id);
            return null;
        }

        if (verification.NeedsUpgrade && !string.IsNullOrWhiteSpace(verification.UpgradedHash))
        {
            account.PasswordHash = verification.UpgradedHash;
            account.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "Upgraded password storage for account {AccountId}. LegacyPlainText={LegacyPlainText}.",
                account.Id,
                verification.UsedLegacyPlainText);
        }

        var teacherSpecialization = await _dbContext.Teachers
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Email == account.Email)
            .Select(x => x.Specialization)
            .FirstOrDefaultAsync();

        return new DemoAccount
        {
            Id = account.Id,
            Username = account.Username,
            FullName = account.FullName,
            Email = account.Email ?? string.Empty,
            Phone = account.Phone ?? string.Empty,
            Role = account.Role,
            Department = ResolveDepartment(account.Role, account.Email, teacherSpecialization),
            Status = EfServiceMapper.MapAccountStatus(account.IsActive, account.Status),
            Password = "********",
            PasswordHash = account.PasswordHash
        };
    }

    private static string ResolveDepartment(string role, string? email, IReadOnlyDictionary<string, string> teacherSpecializations)
    {
        if (!string.IsNullOrWhiteSpace(email) &&
            teacherSpecializations.TryGetValue(email, out var specialization) &&
            !string.IsNullOrWhiteSpace(specialization))
        {
            return specialization;
        }

        return role switch
        {
            "Admin" => "Ban quản trị",
            "Staff" => "Giáo vụ",
            "Teacher" => "Giảng viên",
            _ => "Hệ thống"
        };
    }

    private static string ResolveDepartment(string role, string? email, string? teacherSpecialization)
    {
        if (!string.IsNullOrWhiteSpace(teacherSpecialization))
        {
            return teacherSpecialization;
        }

        return ResolveDepartment(role, email, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }
}
