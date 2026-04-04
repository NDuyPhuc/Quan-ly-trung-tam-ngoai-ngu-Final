using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService
{
    public AccountInput? GetAccount(int id)
    {
        return ReadSingle(
            """
            SELECT Username, PasswordHash, FullName, Email, Phone, Role, IsActive
            FROM dbo.Accounts
            WHERE Id = @Id AND IsDeleted = 0;
            """,
            reader => new AccountInput
            {
                Username = GetString(reader, "Username"),
                Password = GetString(reader, "PasswordHash"),
                FullName = GetString(reader, "FullName"),
                Email = GetString(reader, "Email"),
                Phone = GetString(reader, "Phone"),
                Role = GetString(reader, "Role"),
                IsActive = GetBoolean(reader, "IsActive")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveAccount(int? id, AccountInput input)
    {
        return ExecuteWrite("save account", (connection, transaction) =>
        {
            var username = Required(input.Username, "Tên đăng nhập là bắt buộc.");
            var fullName = Required(input.FullName, "Họ và tên là bắt buộc.");
            var role = Required(input.Role, "Vai trò là bắt buộc.");
            var password = id.HasValue ? input.Password.Trim() : Required(input.Password, "Mật khẩu là bắt buộc.");
            var email = Optional(input.Email);
            var phone = Optional(input.Phone);

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Accounts
                WHERE Username = @Username AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Username", username),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Tên đăng nhập đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(email) && Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Accounts
                WHERE Email = @Email AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Email", email),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Email đã tồn tại.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Accounts
                    SET Username = @Username,
                        PasswordHash = CASE WHEN @Password = N'' THEN PasswordHash ELSE @Password END,
                        FullName = @FullName,
                        Email = @Email,
                        Phone = @Phone,
                        Role = @Role,
                        IsActive = @IsActive,
                        Status = @Status,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Email", DbValue(email)),
                    new SqlParameter("@Phone", DbValue(phone)),
                    new SqlParameter("@Role", role),
                    new SqlParameter("@IsActive", input.IsActive),
                    new SqlParameter("@Status", input.IsActive ? 1 : 0));

                return ManagementResult.Success("Cập nhật tài khoản thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Accounts
                (
                    Username,
                    PasswordHash,
                    FullName,
                    Email,
                    Phone,
                    Role,
                    IsActive,
                    Status,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Username,
                    @Password,
                    @FullName,
                    @Email,
                    @Phone,
                    @Role,
                    @IsActive,
                    @Status,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", DbValue(email)),
                new SqlParameter("@Phone", DbValue(phone)),
                new SqlParameter("@Role", role),
                new SqlParameter("@IsActive", input.IsActive),
                new SqlParameter("@Status", input.IsActive ? 1 : 0));

            return ManagementResult.Success("Tạo tài khoản thành công.");
        });
    }

    public ManagementResult DeleteAccount(int id)
    {
        return SoftDelete("delete account", "dbo.Accounts", id);
    }
}
