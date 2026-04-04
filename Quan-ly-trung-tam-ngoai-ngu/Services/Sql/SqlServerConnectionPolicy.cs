using Microsoft.Data.SqlClient;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

internal static class SqlServerConnectionPolicy
{
    private static readonly object SyncRoot = new();
    private static readonly TimeSpan UnavailableCooldown = TimeSpan.FromSeconds(30);

    private static DateTimeOffset _blockedUntilUtc = DateTimeOffset.MinValue;

    public static bool ShouldBypassDatabase()
    {
        lock (SyncRoot)
        {
            return DateTimeOffset.UtcNow < _blockedUntilUtc;
        }
    }

    public static void ReportSuccess()
    {
        lock (SyncRoot)
        {
            _blockedUntilUtc = DateTimeOffset.MinValue;
        }
    }

    public static void ReportFailure()
    {
        lock (SyncRoot)
        {
            _blockedUntilUtc = DateTimeOffset.UtcNow.Add(UnavailableCooldown);
        }
    }

    public static string? PrepareConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 3,
            ConnectRetryCount = 0
        };

        return builder.ConnectionString;
    }
}
