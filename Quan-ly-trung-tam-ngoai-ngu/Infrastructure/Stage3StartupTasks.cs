using Microsoft.EntityFrameworkCore;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Infrastructure;

public static class Stage3StartupTasks
{
    public static async Task ApplySecurityBackfillAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IAccountPasswordService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Stage3StartupTasks");

        var legacyAccounts = await dbContext.Accounts
            .Where(x => !x.IsDeleted && x.PasswordHash != null && !x.PasswordHash.StartsWith("AQAAAA"))
            .ToListAsync();

        if (legacyAccounts.Count == 0)
        {
            logger.LogInformation("Stage 3 security backfill found no legacy account passwords.");
            return;
        }

        foreach (var account in legacyAccounts)
        {
            account.PasswordHash = passwordService.HashPassword(account, account.PasswordHash);
            account.UpdatedAt = DateTime.Now;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Stage 3 security backfill migrated {Count} legacy account passwords.", legacyAccounts.Count);
    }
}
