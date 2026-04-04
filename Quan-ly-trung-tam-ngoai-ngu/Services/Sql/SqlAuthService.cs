using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.Services.Mocks;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public class SqlAuthService : IDemoAuthService
{
    private const string ActiveStatus = "Đang hoạt động";
    private const string LockedStatus = "Tạm khóa";

    private readonly string? _connectionString;
    private readonly MockDataService _fallback;
    private readonly ILogger<SqlAuthService> _logger;
    private bool _databaseUnavailable;

    public SqlAuthService(
        IConfiguration configuration,
        MockDataService fallback,
        ILogger<SqlAuthService> logger)
    {
        _connectionString = SqlServerConnectionPolicy.PrepareConnectionString(
            configuration.GetConnectionString("LanguageCenterDb"));
        _fallback = fallback;
        _logger = logger;
    }

    public IReadOnlyList<DemoAccount> GetDemoAccounts()
    {
        if (ShouldUseFallback())
        {
            return _fallback.GetAccounts();
        }

        try
        {
            const string sql = """
                               SELECT
                                   a.Id,
                                   a.Username,
                                   a.PasswordHash,
                                   a.FullName,
                                   a.Email,
                                   a.Phone,
                                   a.Role,
                                   a.IsActive,
                                   a.Status,
                                   COALESCE(NULLIF(t.Specialization, N''), CASE a.Role
                                       WHEN N'Admin' THEN N'Ban quản trị'
                                       WHEN N'Staff' THEN N'Giáo vụ'
                                       WHEN N'Teacher' THEN N'Giảng viên'
                                       ELSE N'Hệ thống'
                                   END) AS Department
                               FROM dbo.Accounts a
                               LEFT JOIN dbo.Teachers t ON t.Email = a.Email AND t.IsDeleted = 0
                               WHERE a.IsDeleted = 0
                               ORDER BY a.Id;
                               """;

            var accounts = Query(sql, Array.Empty<SqlParameter>(), MapAccount);
            SqlServerConnectionPolicy.ReportSuccess();
            return accounts;
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or PlatformNotSupportedException)
        {
            MarkDatabaseUnavailable(ex, "Không thể tải danh sách tài khoản từ SQL Server, chuyển sang dữ liệu mock.");
            return _fallback.GetAccounts();
        }
    }

    public DemoAccount? ValidateLogin(string email, string password)
    {
        if (ShouldUseFallback())
        {
            return ValidateWithFallback(email, password);
        }

        try
        {
            const string sql = """
                               SELECT TOP (1)
                                   a.Id,
                                   a.Username,
                                   a.PasswordHash,
                                   a.FullName,
                                   a.Email,
                                   a.Phone,
                                   a.Role,
                                   a.IsActive,
                                   a.Status,
                                   COALESCE(NULLIF(t.Specialization, N''), CASE a.Role
                                       WHEN N'Admin' THEN N'Ban quản trị'
                                       WHEN N'Staff' THEN N'Giáo vụ'
                                       WHEN N'Teacher' THEN N'Giảng viên'
                                       ELSE N'Hệ thống'
                                   END) AS Department
                               FROM dbo.Accounts a
                               LEFT JOIN dbo.Teachers t ON t.Email = a.Email AND t.IsDeleted = 0
                               WHERE a.IsDeleted = 0
                                 AND (
                                     LOWER(COALESCE(a.Email, N'')) = LOWER(@login)
                                     OR LOWER(a.Username) = LOWER(@login)
                                 )
                               ORDER BY a.Id;
                               """;

            var account = QuerySingle(
                sql,
                [new SqlParameter("@login", email.Trim())],
                MapAccount);
            SqlServerConnectionPolicy.ReportSuccess();

            if (account is null || !string.Equals(account.PasswordHash, password, StringComparison.Ordinal))
            {
                return null;
            }

            return account.Status == ActiveStatus ? account : null;
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or PlatformNotSupportedException)
        {
            MarkDatabaseUnavailable(ex, "Không thể xác thực đăng nhập bằng SQL Server, chuyển sang dữ liệu mock.");
            return ValidateWithFallback(email, password);
        }
    }

    public StudentRegistrationResult RegisterStudent(string fullName, string email, string phone, string password)
    {
        if (ShouldUseFallback())
        {
            return RegisterWithFallback(fullName, email, phone);
        }

        try
        {
            using var connection = OpenConnection();

            const string existsSql = """
                                     SELECT CASE
                                         WHEN EXISTS (
                                             SELECT 1
                                             FROM dbo.Students
                                             WHERE Email = @Email AND IsDeleted = 0
                                         ) OR EXISTS (
                                             SELECT 1
                                             FROM dbo.Accounts
                                             WHERE Email = @Email AND IsDeleted = 0
                                         )
                                         THEN 1
                                         ELSE 0
                                     END;
                                     """;

            using (var existsCommand = new SqlCommand(existsSql, connection))
            {
                existsCommand.Parameters.AddWithValue("@Email", email.Trim());
                var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) == 1;
                if (exists)
                {
                    return StudentRegistrationResult.Fail("Email này đã tồn tại trong cơ sở dữ liệu.");
                }
            }

            string studentCode;
            const string nextCodeSql = """
                                       SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(StudentCode, 2, 10))), 0) + 1
                                       FROM dbo.Students
                                       WHERE StudentCode LIKE N'S%';
                                       """;

            using (var nextCodeCommand = new SqlCommand(nextCodeSql, connection))
            {
                var nextValue = Convert.ToInt32(nextCodeCommand.ExecuteScalar());
                studentCode = $"S{nextValue:000}";
            }

            const string insertSql = """
                                     INSERT INTO dbo.Students
                                     (
                                         StudentCode,
                                         FullName,
                                         Phone,
                                         Email,
                                         Status,
                                         IsDeleted,
                                         CreatedAt
                                     )
                                     VALUES
                                     (
                                         @StudentCode,
                                         @FullName,
                                         @Phone,
                                         @Email,
                                         1,
                                         0,
                                         SYSDATETIME()
                                     );
                                     """;

            using (var insertCommand = new SqlCommand(insertSql, connection))
            {
                insertCommand.Parameters.AddWithValue("@StudentCode", studentCode);
                insertCommand.Parameters.AddWithValue("@FullName", fullName.Trim());
                insertCommand.Parameters.AddWithValue("@Phone", phone.Trim());
                insertCommand.Parameters.AddWithValue("@Email", email.Trim());
                insertCommand.ExecuteNonQuery();
            }

            SqlServerConnectionPolicy.ReportSuccess();
            return StudentRegistrationResult.Success(
                $"Đăng ký thành công. Học viên {studentCode} đã được tạo trong cơ sở dữ liệu.",
                studentCode);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or PlatformNotSupportedException)
        {
            MarkDatabaseUnavailable(ex, "Không thể tạo học viên mới trong SQL Server, chuyển sang chế độ demo.");
            return RegisterWithFallback(fullName, email, phone);
        }
    }

    private DemoAccount? ValidateWithFallback(string email, string password)
    {
        return _fallback
            .GetAccounts()
            .FirstOrDefault(x =>
                (x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) ||
                 x.Username.Equals(email, StringComparison.OrdinalIgnoreCase)) &&
                (x.PasswordHash == password || x.Password == password) &&
                x.Status == ActiveStatus);
    }

    private StudentRegistrationResult RegisterWithFallback(string fullName, string email, string phone)
    {
        return _fallback.RegisterStudent(fullName, email, phone);
    }

    private bool ShouldUseFallback()
    {
        return _databaseUnavailable ||
               SqlServerConnectionPolicy.ShouldBypassDatabase() ||
               string.IsNullOrWhiteSpace(_connectionString);
    }

    private void MarkDatabaseUnavailable(Exception ex, string message)
    {
        _databaseUnavailable = true;
        SqlServerConnectionPolicy.ReportFailure();
        _logger.LogWarning(ex, message);
    }

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private List<T> Query<T>(string sql, IReadOnlyList<SqlParameter> parameters, Func<SqlDataReader, T> map)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        foreach (var parameter in parameters)
        {
            command.Parameters.Add(parameter);
        }

        using var reader = command.ExecuteReader();
        var items = new List<T>();
        while (reader.Read())
        {
            items.Add(map(reader));
        }

        return items;
    }

    private T? QuerySingle<T>(string sql, IReadOnlyList<SqlParameter> parameters, Func<SqlDataReader, T> map)
    {
        return Query(sql, parameters, map).FirstOrDefault();
    }

    private static DemoAccount MapAccount(SqlDataReader reader)
    {
        var storedPassword = GetString(reader, "PasswordHash");

        return new DemoAccount
        {
            Id = GetInt32(reader, "Id"),
            Username = GetString(reader, "Username"),
            FullName = GetString(reader, "FullName"),
            Email = GetString(reader, "Email"),
            Phone = GetString(reader, "Phone"),
            Role = GetString(reader, "Role"),
            Department = GetString(reader, "Department"),
            Status = MapAccountStatus(GetBoolean(reader, "IsActive"), GetByte(reader, "Status")),
            Password = string.IsNullOrWhiteSpace(storedPassword) ? string.Empty : "********",
            PasswordHash = storedPassword
        };
    }

    private static string MapAccountStatus(bool isActive, byte status)
    {
        return isActive && status == 1 ? ActiveStatus : LockedStatus;
    }

    private static string GetString(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name]) ?? string.Empty;
    }

    private static int GetInt32(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? 0 : Convert.ToInt32(reader[name]);
    }

    private static bool GetBoolean(SqlDataReader reader, string name)
    {
        return reader[name] != DBNull.Value && Convert.ToBoolean(reader[name]);
    }

    private static byte GetByte(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? (byte)0 : Convert.ToByte(reader[name]);
    }
}
