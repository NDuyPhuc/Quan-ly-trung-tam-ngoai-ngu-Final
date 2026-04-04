using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService
{
    public StudentInput? GetStudent(int id)
    {
        return ReadSingle(
            """
            SELECT StudentCode, FullName, DateOfBirth, Gender, Phone, Email, Address, Status
            FROM dbo.Students
            WHERE Id = @Id AND IsDeleted = 0;
            """,
            reader => new StudentInput
            {
                StudentCode = GetString(reader, "StudentCode"),
                FullName = GetString(reader, "FullName"),
                DateOfBirth = GetNullableDateTime(reader, "DateOfBirth"),
                Gender = GetString(reader, "Gender"),
                Phone = GetString(reader, "Phone"),
                Email = GetString(reader, "Email"),
                Address = GetString(reader, "Address"),
                IsActive = GetByte(reader, "Status") == 1
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveStudent(int? id, StudentInput input)
    {
        return ExecuteWrite("save student", (connection, transaction) =>
        {
            var code = Required(input.StudentCode, "Mã học viên là bắt buộc.");
            var fullName = Required(input.FullName, "Tên học viên là bắt buộc.");
            var email = Optional(input.Email);

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Students
                WHERE StudentCode = @Code AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Mã học viên đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(email) && Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Students
                WHERE Email = @Email AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Email", email),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Email học viên đã tồn tại.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Students
                    SET StudentCode = @Code,
                        FullName = @FullName,
                        DateOfBirth = @DateOfBirth,
                        Gender = @Gender,
                        Phone = @Phone,
                        Email = @Email,
                        Address = @Address,
                        Status = @Status,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@DateOfBirth", DbValue(input.DateOfBirth)),
                    new SqlParameter("@Gender", DbValue(Optional(input.Gender))),
                    new SqlParameter("@Phone", DbValue(Optional(input.Phone))),
                    new SqlParameter("@Email", DbValue(email)),
                    new SqlParameter("@Address", DbValue(Optional(input.Address))),
                    new SqlParameter("@Status", input.IsActive ? 1 : 0));

                return ManagementResult.Success("Cập nhật học viên thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Students
                (
                    StudentCode,
                    FullName,
                    DateOfBirth,
                    Gender,
                    Phone,
                    Email,
                    Address,
                    Status,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Code,
                    @FullName,
                    @DateOfBirth,
                    @Gender,
                    @Phone,
                    @Email,
                    @Address,
                    @Status,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@DateOfBirth", DbValue(input.DateOfBirth)),
                new SqlParameter("@Gender", DbValue(Optional(input.Gender))),
                new SqlParameter("@Phone", DbValue(Optional(input.Phone))),
                new SqlParameter("@Email", DbValue(email)),
                new SqlParameter("@Address", DbValue(Optional(input.Address))),
                new SqlParameter("@Status", input.IsActive ? 1 : 0));

            return ManagementResult.Success("Tạo học viên thành công.");
        });
    }

    public ManagementResult DeleteStudent(int id)
    {
        return SoftDelete("delete student", "dbo.Students", id);
    }

    public TeacherInput? GetTeacher(int id)
    {
        return ReadSingle(
            """
            SELECT TeacherCode, FullName, Phone, Email, Specialization, Status
            FROM dbo.Teachers
            WHERE Id = @Id AND IsDeleted = 0;
            """,
            reader => new TeacherInput
            {
                TeacherCode = GetString(reader, "TeacherCode"),
                FullName = GetString(reader, "FullName"),
                Phone = GetString(reader, "Phone"),
                Email = GetString(reader, "Email"),
                Specialization = GetString(reader, "Specialization"),
                IsActive = GetByte(reader, "Status") == 1
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveTeacher(int? id, TeacherInput input)
    {
        return ExecuteWrite("save teacher", (connection, transaction) =>
        {
            var code = Required(input.TeacherCode, "Mã giáo viên là bắt buộc.");
            var fullName = Required(input.FullName, "Tên giáo viên là bắt buộc.");
            var email = Optional(input.Email);

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Teachers
                WHERE TeacherCode = @Code AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Mã giáo viên đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(email) && Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Teachers
                WHERE Email = @Email AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Email", email),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Email giáo viên đã tồn tại.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Teachers
                    SET TeacherCode = @Code,
                        FullName = @FullName,
                        Phone = @Phone,
                        Email = @Email,
                        Specialization = @Specialization,
                        Status = @Status,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@FullName", fullName),
                    new SqlParameter("@Phone", DbValue(Optional(input.Phone))),
                    new SqlParameter("@Email", DbValue(email)),
                    new SqlParameter("@Specialization", DbValue(Optional(input.Specialization))),
                    new SqlParameter("@Status", input.IsActive ? 1 : 0));

                return ManagementResult.Success("Cập nhật giáo viên thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Teachers
                (
                    TeacherCode,
                    FullName,
                    Phone,
                    Email,
                    Specialization,
                    Status,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Code,
                    @FullName,
                    @Phone,
                    @Email,
                    @Specialization,
                    @Status,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Phone", DbValue(Optional(input.Phone))),
                new SqlParameter("@Email", DbValue(email)),
                new SqlParameter("@Specialization", DbValue(Optional(input.Specialization))),
                new SqlParameter("@Status", input.IsActive ? 1 : 0));

            return ManagementResult.Success("Tạo giáo viên thành công.");
        });
    }

    public ManagementResult DeleteTeacher(int id)
    {
        return SoftDelete("delete teacher", "dbo.Teachers", id);
    }
}
