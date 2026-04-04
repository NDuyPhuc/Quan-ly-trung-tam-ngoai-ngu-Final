using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService : ILanguageCenterManagementService
{
    private readonly string? _connectionString;
    private readonly ILogger<SqlLanguageCenterManagementService> _logger;

    public SqlLanguageCenterManagementService(
        IConfiguration configuration,
        ILogger<SqlLanguageCenterManagementService> logger)
    {
        _connectionString = SqlServerConnectionPolicy.PrepareConnectionString(
            configuration.GetConnectionString("LanguageCenterDb"));
        _logger = logger;
    }

    private T? ReadSingle<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(_connectionString) || SqlServerConnectionPolicy.ShouldBypassDatabase())
        {
            return default;
        }

        try
        {
            using var connection = OpenConnection();
            using var command = new SqlCommand(sql, connection);
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return default;
            }

            SqlServerConnectionPolicy.ReportSuccess();
            return map(reader);
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or PlatformNotSupportedException)
        {
            SqlServerConnectionPolicy.ReportFailure();
            _logger.LogWarning(ex, "Could not read management record from SQL Server.");
            return default;
        }
    }

    private ManagementResult ExecuteWrite(string operation, Func<SqlConnection, SqlTransaction, ManagementResult> action)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            return ManagementResult.Fail("Chưa cấu hình kết nối SQL Server.");
        }

        if (SqlServerConnectionPolicy.ShouldBypassDatabase())
        {
            return ManagementResult.Fail("SQL Server đang tạm thời không khả dụng. Vui lòng thử lại sau ít phút.");
        }

        try
        {
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            var result = action(connection, transaction);
            if (result.Succeeded)
            {
                transaction.Commit();
                SqlServerConnectionPolicy.ReportSuccess();
            }
            else
            {
                transaction.Rollback();
            }

            return result;
        }
        catch (Exception ex) when (ex is SqlException or PlatformNotSupportedException)
        {
            SqlServerConnectionPolicy.ReportFailure();
            _logger.LogError(ex, "Could not complete management operation {Operation}.", operation);
            return ManagementResult.Fail("Không thể lưu dữ liệu xuống SQL Server. Vui lòng kiểm tra kết nối và cấu trúc cơ sở dữ liệu.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while executing management operation {Operation}.", operation);
            return ManagementResult.Fail(ex.Message);
        }
    }

    private ManagementResult SoftDelete(string operation, string tableName, int id)
    {
        return ExecuteWrite(operation, (connection, transaction) =>
        {
            ExecuteNonQuery(connection, transaction,
                $"UPDATE {tableName} SET IsDeleted = 1, UpdatedAt = SYSDATETIME() WHERE Id = @Id;",
                new SqlParameter("@Id", id));

            return ManagementResult.Success("Xóa dữ liệu thành công.");
        });
    }

    private ManagementResult DeletePhysical(string operation, string sql, int id)
    {
        return ExecuteWrite(operation, (connection, transaction) =>
        {
            ExecuteNonQuery(connection, transaction, sql, new SqlParameter("@Id", id));
            return ManagementResult.Success("Xóa dữ liệu thành công.");
        });
    }

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static bool Exists(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        var value = command.ExecuteScalar();
        return value is not null && value != DBNull.Value;
    }

    private static int? ResolveId(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        var value = command.ExecuteScalar();
        return value is null || value == DBNull.Value ? null : Convert.ToInt32(value);
    }

    private static decimal? ResolveDecimal(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        var value = command.ExecuteScalar();
        return value is null || value == DBNull.Value ? null : Convert.ToDecimal(value);
    }

    private static void ExecuteNonQuery(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        command.ExecuteNonQuery();
    }

    private static string GenerateCode(SqlConnection connection, SqlTransaction transaction, string tableName, string columnName, string prefix)
    {
        using var command = new SqlCommand(
            $"""
             SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING({columnName}, {prefix.Length + 1}, 20))), 0) + 1
             FROM {tableName}
             WHERE {columnName} LIKE @PrefixPattern;
             """,
            connection,
            transaction);
        command.Parameters.AddWithValue("@PrefixPattern", $"{prefix}%");
        var next = Convert.ToInt32(command.ExecuteScalar());
        return $"{prefix}{next:000}";
    }

    private static object DbValue(object? value)
    {
        return value ?? DBNull.Value;
    }

    private static string Required(string value, string message)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException(message);
        }

        return normalized;
    }

    private static string? Optional(string value)
    {
        var normalized = value.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string GetString(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name]) ?? string.Empty;
    }

    private static int GetInt32(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? 0 : Convert.ToInt32(reader[name]);
    }

    private static decimal GetDecimal(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? 0m : Convert.ToDecimal(reader[name]);
    }

    private static DateTime GetDateTime(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? DateTime.Today : Convert.ToDateTime(reader[name]);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? null : Convert.ToDateTime(reader[name]);
    }

    private static byte GetByte(SqlDataReader reader, string name)
    {
        return reader[name] == DBNull.Value ? (byte)0 : Convert.ToByte(reader[name]);
    }

    private static bool GetBoolean(SqlDataReader reader, string name)
    {
        return reader[name] != DBNull.Value && Convert.ToBoolean(reader[name]);
    }
}
