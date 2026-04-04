using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService
{
    public CourseInput? GetCourse(int id)
    {
        return ReadSingle(
            """
            SELECT CourseCode, CourseName, Description, DurationHours, TuitionFee, Status
            FROM dbo.Courses
            WHERE Id = @Id AND IsDeleted = 0;
            """,
            reader => new CourseInput
            {
                CourseCode = GetString(reader, "CourseCode"),
                CourseName = GetString(reader, "CourseName"),
                Description = GetString(reader, "Description"),
                DurationHours = GetInt32(reader, "DurationHours"),
                TuitionFee = GetDecimal(reader, "TuitionFee"),
                IsActive = GetByte(reader, "Status") == 1
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveCourse(int? id, CourseInput input)
    {
        return ExecuteWrite("save course", (connection, transaction) =>
        {
            var code = Required(input.CourseCode, "Mã khóa học là bắt buộc.");
            var name = Required(input.CourseName, "Tên khóa học là bắt buộc.");
            if (input.DurationHours <= 0)
            {
                return ManagementResult.Fail("Thời lượng khóa học phải lớn hơn 0.");
            }

            if (input.TuitionFee < 0)
            {
                return ManagementResult.Fail("Học phí phải lớn hơn hoặc bằng 0.");
            }

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Courses
                WHERE CourseCode = @Code AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Mã khóa học đã tồn tại.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Courses
                    SET CourseCode = @Code,
                        CourseName = @Name,
                        Description = @Description,
                        DurationHours = @DurationHours,
                        TuitionFee = @TuitionFee,
                        Status = @Status,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@Name", name),
                    new SqlParameter("@Description", DbValue(Optional(input.Description))),
                    new SqlParameter("@DurationHours", input.DurationHours),
                    new SqlParameter("@TuitionFee", input.TuitionFee),
                    new SqlParameter("@Status", input.IsActive ? 1 : 0));

                return ManagementResult.Success("Cập nhật khóa học thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Courses
                (
                    CourseCode,
                    CourseName,
                    Description,
                    DurationHours,
                    TuitionFee,
                    Status,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Code,
                    @Name,
                    @Description,
                    @DurationHours,
                    @TuitionFee,
                    @Status,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@Name", name),
                new SqlParameter("@Description", DbValue(Optional(input.Description))),
                new SqlParameter("@DurationHours", input.DurationHours),
                new SqlParameter("@TuitionFee", input.TuitionFee),
                new SqlParameter("@Status", input.IsActive ? 1 : 0));

            return ManagementResult.Success("Tạo khóa học thành công.");
        });
    }

    public ManagementResult DeleteCourse(int id)
    {
        return SoftDelete("delete course", "dbo.Courses", id);
    }

    public ClassInput? GetClass(int id)
    {
        return ReadSingle(
            """
            SELECT
                cl.ClassCode,
                cl.ClassName,
                c.CourseCode,
                t.TeacherCode,
                cl.StartDate,
                cl.EndDate,
                cl.ScheduleText,
                cl.Capacity,
                cl.Status
            FROM dbo.Classes cl
            INNER JOIN dbo.Courses c ON c.Id = cl.CourseId
            LEFT JOIN dbo.Teachers t ON t.Id = cl.TeacherId
            WHERE cl.Id = @Id AND cl.IsDeleted = 0;
            """,
            reader => new ClassInput
            {
                ClassCode = GetString(reader, "ClassCode"),
                ClassName = GetString(reader, "ClassName"),
                CourseCode = GetString(reader, "CourseCode"),
                TeacherCode = GetString(reader, "TeacherCode"),
                StartDate = GetDateTime(reader, "StartDate"),
                EndDate = GetDateTime(reader, "EndDate"),
                ScheduleText = GetString(reader, "ScheduleText"),
                Capacity = GetInt32(reader, "Capacity"),
                IsActive = GetByte(reader, "Status") == 1
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveClass(int? id, ClassInput input)
    {
        return ExecuteWrite("save class", (connection, transaction) =>
        {
            var code = Required(input.ClassCode, "Mã lớp là bắt buộc.");
            var className = Required(input.ClassName, "Tên lớp là bắt buộc.");
            var courseCode = Required(input.CourseCode, "Khóa học là bắt buộc.");

            if (input.Capacity <= 0)
            {
                return ManagementResult.Fail("Sĩ số tối đa phải lớn hơn 0.");
            }

            if (input.EndDate.Date < input.StartDate.Date)
            {
                return ManagementResult.Fail("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
            }

            var courseId = ResolveId(connection, transaction,
                "SELECT Id FROM dbo.Courses WHERE CourseCode = @Code AND IsDeleted = 0;",
                new SqlParameter("@Code", courseCode));

            if (!courseId.HasValue)
            {
                return ManagementResult.Fail("Không tìm thấy khóa học đã chọn.");
            }

            int? teacherId = null;
            if (!string.IsNullOrWhiteSpace(input.TeacherCode))
            {
                teacherId = ResolveId(connection, transaction,
                    "SELECT Id FROM dbo.Teachers WHERE TeacherCode = @Code AND IsDeleted = 0;",
                    new SqlParameter("@Code", input.TeacherCode.Trim()));

                if (!teacherId.HasValue)
                {
                    return ManagementResult.Fail("Không tìm thấy giáo viên đã chọn.");
                }
            }

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Classes
                WHERE ClassCode = @Code AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Mã lớp đã tồn tại.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Classes
                    SET ClassCode = @Code,
                        ClassName = @ClassName,
                        CourseId = @CourseId,
                        TeacherId = @TeacherId,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        ScheduleText = @ScheduleText,
                        Capacity = @Capacity,
                        Status = @Status,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@Code", code),
                    new SqlParameter("@ClassName", className),
                    new SqlParameter("@CourseId", courseId.Value),
                    new SqlParameter("@TeacherId", DbValue(teacherId)),
                    new SqlParameter("@StartDate", input.StartDate.Date),
                    new SqlParameter("@EndDate", input.EndDate.Date),
                    new SqlParameter("@ScheduleText", DbValue(Optional(input.ScheduleText))),
                    new SqlParameter("@Capacity", input.Capacity),
                    new SqlParameter("@Status", input.IsActive ? 1 : 0));

                return ManagementResult.Success("Cập nhật lớp học thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Classes
                (
                    ClassCode,
                    ClassName,
                    CourseId,
                    TeacherId,
                    StartDate,
                    EndDate,
                    ScheduleText,
                    Capacity,
                    Status,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @Code,
                    @ClassName,
                    @CourseId,
                    @TeacherId,
                    @StartDate,
                    @EndDate,
                    @ScheduleText,
                    @Capacity,
                    @Status,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@Code", code),
                new SqlParameter("@ClassName", className),
                new SqlParameter("@CourseId", courseId.Value),
                new SqlParameter("@TeacherId", DbValue(teacherId)),
                new SqlParameter("@StartDate", input.StartDate.Date),
                new SqlParameter("@EndDate", input.EndDate.Date),
                new SqlParameter("@ScheduleText", DbValue(Optional(input.ScheduleText))),
                new SqlParameter("@Capacity", input.Capacity),
                new SqlParameter("@Status", input.IsActive ? 1 : 0));

            return ManagementResult.Success("Tạo lớp học thành công.");
        });
    }

    public ManagementResult DeleteClass(int id)
    {
        return SoftDelete("delete class", "dbo.Classes", id);
    }
}
