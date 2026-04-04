using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService
{
    public AttendanceInput? GetAttendance(int id)
    {
        return ReadSingle(
            """
            SELECT EnrollmentId, ClassSessionId, AttendanceStatus, Note
            FROM dbo.Attendances
            WHERE Id = @Id;
            """,
            reader => new AttendanceInput
            {
                EnrollmentId = GetInt32(reader, "EnrollmentId"),
                ClassSessionId = GetInt32(reader, "ClassSessionId"),
                AttendanceStatus = GetString(reader, "AttendanceStatus"),
                Note = GetString(reader, "Note")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveAttendance(int? id, AttendanceInput input)
    {
        return ExecuteWrite("save attendance", (connection, transaction) =>
        {
            if (input.EnrollmentId <= 0 || input.ClassSessionId <= 0)
            {
                return ManagementResult.Fail("Ghi danh và buổi học là bắt buộc.");
            }

            if (!AttendanceBelongsToSessionClass(connection, transaction, input.EnrollmentId, input.ClassSessionId))
            {
                return ManagementResult.Fail("Học viên không thuộc lớp của buổi học đã chọn.");
            }

            var attendanceStatus = Required(input.AttendanceStatus, "Trạng thái điểm danh là bắt buộc.");

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Attendances
                    SET EnrollmentId = @EnrollmentId,
                        ClassSessionId = @ClassSessionId,
                        AttendanceStatus = @AttendanceStatus,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@EnrollmentId", input.EnrollmentId),
                    new SqlParameter("@ClassSessionId", input.ClassSessionId),
                    new SqlParameter("@AttendanceStatus", attendanceStatus),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật điểm danh thành công.");
            }

            var existingId = ResolveId(connection, transaction,
                """
                SELECT Id
                FROM dbo.Attendances
                WHERE EnrollmentId = @EnrollmentId AND ClassSessionId = @ClassSessionId;
                """,
                new SqlParameter("@EnrollmentId", input.EnrollmentId),
                new SqlParameter("@ClassSessionId", input.ClassSessionId));

            if (existingId.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Attendances
                    SET AttendanceStatus = @AttendanceStatus,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", existingId.Value),
                    new SqlParameter("@AttendanceStatus", attendanceStatus),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật điểm danh thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Attendances
                (
                    EnrollmentId,
                    ClassSessionId,
                    AttendanceStatus,
                    Note,
                    CreatedAt
                )
                VALUES
                (
                    @EnrollmentId,
                    @ClassSessionId,
                    @AttendanceStatus,
                    @Note,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@EnrollmentId", input.EnrollmentId),
                new SqlParameter("@ClassSessionId", input.ClassSessionId),
                new SqlParameter("@AttendanceStatus", attendanceStatus),
                new SqlParameter("@Note", DbValue(Optional(input.Note))));

            return ManagementResult.Success("Tạo điểm danh thành công.");
        });
    }

    public ManagementResult DeleteAttendance(int id)
    {
        return DeletePhysical("delete attendance", "DELETE FROM dbo.Attendances WHERE Id = @Id;", id);
    }

    public ExamResultInput? GetExamResult(int id)
    {
        return ReadSingle(
            """
            SELECT er.EnrollmentId, ex.ExamName, ex.ExamType, ex.ExamDate, ex.MaxScore, er.Score, er.ResultStatus, er.Note
            FROM dbo.ExamResults er
            INNER JOIN dbo.Exams ex ON ex.Id = er.ExamId
            WHERE er.Id = @Id;
            """,
            reader => new ExamResultInput
            {
                EnrollmentId = GetInt32(reader, "EnrollmentId"),
                ExamName = GetString(reader, "ExamName"),
                ExamType = GetString(reader, "ExamType"),
                ExamDate = GetDateTime(reader, "ExamDate"),
                MaxScore = GetDecimal(reader, "MaxScore"),
                Score = GetDecimal(reader, "Score"),
                ResultStatus = GetString(reader, "ResultStatus"),
                Note = GetString(reader, "Note")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveExamResult(int? id, ExamResultInput input)
    {
        return ExecuteWrite("save exam result", (connection, transaction) =>
        {
            if (input.EnrollmentId <= 0)
            {
                return ManagementResult.Fail("Ghi danh là bắt buộc.");
            }

            if (input.MaxScore <= 0)
            {
                return ManagementResult.Fail("Điểm tối đa phải lớn hơn 0.");
            }

            if (input.Score < 0)
            {
                return ManagementResult.Fail("Điểm số phải lớn hơn hoặc bằng 0.");
            }

            var examName = Required(input.ExamName, "Tên bài kiểm tra là bắt buộc.");
            var examType = Required(input.ExamType, "Loại bài kiểm tra là bắt buộc.");
            var classId = ResolveId(connection, transaction,
                """
                SELECT ClassId
                FROM dbo.Enrollments
                WHERE Id = @Id AND IsDeleted = 0;
                """,
                new SqlParameter("@Id", input.EnrollmentId));

            if (!classId.HasValue)
            {
                return ManagementResult.Fail("Không tìm thấy ghi danh đã chọn.");
            }

            var examId = ResolveId(connection, transaction,
                """
                SELECT Id
                FROM dbo.Exams
                WHERE ClassId = @ClassId
                  AND ExamName = @ExamName
                  AND ExamType = @ExamType
                  AND ExamDate = @ExamDate;
                """,
                new SqlParameter("@ClassId", classId.Value),
                new SqlParameter("@ExamName", examName),
                new SqlParameter("@ExamType", examType),
                new SqlParameter("@ExamDate", input.ExamDate.Date));

            if (!examId.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    INSERT INTO dbo.Exams
                    (
                        ClassId,
                        ExamName,
                        ExamType,
                        ExamDate,
                        MaxScore,
                        CreatedAt
                    )
                    VALUES
                    (
                        @ClassId,
                        @ExamName,
                        @ExamType,
                        @ExamDate,
                        @MaxScore,
                        SYSDATETIME()
                    );
                    """,
                    new SqlParameter("@ClassId", classId.Value),
                    new SqlParameter("@ExamName", examName),
                    new SqlParameter("@ExamType", examType),
                    new SqlParameter("@ExamDate", input.ExamDate.Date),
                    new SqlParameter("@MaxScore", input.MaxScore));

                examId = ResolveId(connection, transaction,
                    """
                    SELECT TOP (1) Id
                    FROM dbo.Exams
                    WHERE ClassId = @ClassId
                      AND ExamName = @ExamName
                      AND ExamType = @ExamType
                      AND ExamDate = @ExamDate
                    ORDER BY Id DESC;
                    """,
                    new SqlParameter("@ClassId", classId.Value),
                    new SqlParameter("@ExamName", examName),
                    new SqlParameter("@ExamType", examType),
                    new SqlParameter("@ExamDate", input.ExamDate.Date));
            }
            else
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Exams
                    SET ExamName = @ExamName,
                        ExamType = @ExamType,
                        ExamDate = @ExamDate,
                        MaxScore = @MaxScore
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", examId.Value),
                    new SqlParameter("@ExamName", examName),
                    new SqlParameter("@ExamType", examType),
                    new SqlParameter("@ExamDate", input.ExamDate.Date),
                    new SqlParameter("@MaxScore", input.MaxScore));
            }

            var resultStatus = !string.IsNullOrWhiteSpace(input.ResultStatus)
                ? input.ResultStatus.Trim()
                : input.Score >= input.MaxScore * 0.5m ? "Pass" : "Fail";

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.ExamResults
                    SET ExamId = @ExamId,
                        EnrollmentId = @EnrollmentId,
                        Score = @Score,
                        ResultStatus = @ResultStatus,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@ExamId", examId!.Value),
                    new SqlParameter("@EnrollmentId", input.EnrollmentId),
                    new SqlParameter("@Score", input.Score),
                    new SqlParameter("@ResultStatus", DbValue(Optional(resultStatus))),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật điểm số thành công.");
            }

            var existingResultId = ResolveId(connection, transaction,
                """
                SELECT Id
                FROM dbo.ExamResults
                WHERE ExamId = @ExamId AND EnrollmentId = @EnrollmentId;
                """,
                new SqlParameter("@ExamId", examId!.Value),
                new SqlParameter("@EnrollmentId", input.EnrollmentId));

            if (existingResultId.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.ExamResults
                    SET Score = @Score,
                        ResultStatus = @ResultStatus,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", existingResultId.Value),
                    new SqlParameter("@Score", input.Score),
                    new SqlParameter("@ResultStatus", DbValue(Optional(resultStatus))),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật điểm số thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.ExamResults
                (
                    ExamId,
                    EnrollmentId,
                    Score,
                    ResultStatus,
                    Note,
                    CreatedAt
                )
                VALUES
                (
                    @ExamId,
                    @EnrollmentId,
                    @Score,
                    @ResultStatus,
                    @Note,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@ExamId", examId!.Value),
                new SqlParameter("@EnrollmentId", input.EnrollmentId),
                new SqlParameter("@Score", input.Score),
                new SqlParameter("@ResultStatus", DbValue(Optional(resultStatus))),
                new SqlParameter("@Note", DbValue(Optional(input.Note))));

            return ManagementResult.Success("Tạo điểm số thành công.");
        });
    }

    public ManagementResult DeleteExamResult(int id)
    {
        return ExecuteWrite("delete exam result", (connection, transaction) =>
        {
            var examId = ResolveId(connection, transaction,
                "SELECT ExamId FROM dbo.ExamResults WHERE Id = @Id;",
                new SqlParameter("@Id", id));

            ExecuteNonQuery(connection, transaction,
                "DELETE FROM dbo.ExamResults WHERE Id = @Id;",
                new SqlParameter("@Id", id));

            if (examId.HasValue && !Exists(connection, transaction,
                "SELECT 1 FROM dbo.ExamResults WHERE ExamId = @ExamId;",
                new SqlParameter("@ExamId", examId.Value)))
            {
                ExecuteNonQuery(connection, transaction,
                    "DELETE FROM dbo.Exams WHERE Id = @Id;",
                    new SqlParameter("@Id", examId.Value));
            }

            return ManagementResult.Success("Xóa điểm số thành công.");
        });
    }

    private static bool AttendanceBelongsToSessionClass(SqlConnection connection, SqlTransaction transaction, int enrollmentId, int classSessionId)
    {
        var match = ResolveId(connection, transaction,
            """
            SELECT 1
            FROM dbo.Enrollments e
            INNER JOIN dbo.ClassSessions cs ON cs.ClassId = e.ClassId
            WHERE e.Id = @EnrollmentId AND cs.Id = @ClassSessionId AND e.IsDeleted = 0;
            """,
            new SqlParameter("@EnrollmentId", enrollmentId),
            new SqlParameter("@ClassSessionId", classSessionId));

        return match.HasValue;
    }
}
