using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public sealed partial class SqlLanguageCenterManagementService
{
    public EnrollmentInput? GetEnrollment(int id)
    {
        return ReadSingle(
            """
            SELECT
                s.StudentCode,
                cl.ClassCode,
                e.EnrollDate,
                e.Status,
                e.TotalFee,
                e.DiscountAmount,
                e.Note
            FROM dbo.Enrollments e
            INNER JOIN dbo.Students s ON s.Id = e.StudentId
            INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId
            WHERE e.Id = @Id AND e.IsDeleted = 0;
            """,
            reader => new EnrollmentInput
            {
                StudentCode = GetString(reader, "StudentCode"),
                ClassCode = GetString(reader, "ClassCode"),
                EnrollDate = GetDateTime(reader, "EnrollDate"),
                Status = GetString(reader, "Status"),
                TotalFee = GetDecimal(reader, "TotalFee"),
                DiscountAmount = GetDecimal(reader, "DiscountAmount"),
                Note = GetString(reader, "Note")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveEnrollment(int? id, EnrollmentInput input)
    {
        return ExecuteWrite("save enrollment", (connection, transaction) =>
        {
            var studentCode = Required(input.StudentCode, "Học viên là bắt buộc.");
            var classCode = Required(input.ClassCode, "Lớp học là bắt buộc.");
            var studentId = ResolveId(connection, transaction,
                "SELECT Id FROM dbo.Students WHERE StudentCode = @Code AND IsDeleted = 0;",
                new SqlParameter("@Code", studentCode));
            var classId = ResolveId(connection, transaction,
                "SELECT Id FROM dbo.Classes WHERE ClassCode = @Code AND IsDeleted = 0;",
                new SqlParameter("@Code", classCode));

            if (!studentId.HasValue || !classId.HasValue)
            {
                return ManagementResult.Fail("Không tìm thấy học viên hoặc lớp học đã chọn.");
            }

            var totalFee = input.TotalFee > 0
                ? input.TotalFee
                : ResolveDecimal(connection, transaction,
                    """
                    SELECT c.TuitionFee
                    FROM dbo.Classes cl
                    INNER JOIN dbo.Courses c ON c.Id = cl.CourseId
                    WHERE cl.Id = @ClassId;
                    """,
                    new SqlParameter("@ClassId", classId.Value)) ?? 0m;

            if (totalFee < 0)
            {
                return ManagementResult.Fail("Tổng học phí phải lớn hơn hoặc bằng 0.");
            }

            if (input.DiscountAmount < 0 || input.DiscountAmount > totalFee)
            {
                return ManagementResult.Fail("Số tiền giảm giá không hợp lệ.");
            }

            if (Exists(connection, transaction,
                """
                SELECT 1
                FROM dbo.Enrollments
                WHERE StudentId = @StudentId AND ClassId = @ClassId AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);
                """,
                new SqlParameter("@StudentId", studentId.Value),
                new SqlParameter("@ClassId", classId.Value),
                new SqlParameter("@Id", (object?)id ?? DBNull.Value)))
            {
                return ManagementResult.Fail("Học viên này đã được ghi danh vào lớp đã chọn.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Enrollments
                    SET StudentId = @StudentId,
                        ClassId = @ClassId,
                        EnrollDate = @EnrollDate,
                        Status = @Status,
                        TotalFee = @TotalFee,
                        DiscountAmount = @DiscountAmount,
                        Note = @Note,
                        UpdatedAt = SYSDATETIME()
                    WHERE Id = @Id AND IsDeleted = 0;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@StudentId", studentId.Value),
                    new SqlParameter("@ClassId", classId.Value),
                    new SqlParameter("@EnrollDate", input.EnrollDate.Date),
                    new SqlParameter("@Status", Required(input.Status, "Trạng thái là bắt buộc.")),
                    new SqlParameter("@TotalFee", totalFee),
                    new SqlParameter("@DiscountAmount", input.DiscountAmount),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật ghi danh thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Enrollments
                (
                    StudentId,
                    ClassId,
                    EnrollDate,
                    Status,
                    TotalFee,
                    DiscountAmount,
                    Note,
                    IsDeleted,
                    CreatedAt
                )
                VALUES
                (
                    @StudentId,
                    @ClassId,
                    @EnrollDate,
                    @Status,
                    @TotalFee,
                    @DiscountAmount,
                    @Note,
                    0,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@StudentId", studentId.Value),
                new SqlParameter("@ClassId", classId.Value),
                new SqlParameter("@EnrollDate", input.EnrollDate.Date),
                new SqlParameter("@Status", Required(input.Status, "Trạng thái là bắt buộc.")),
                new SqlParameter("@TotalFee", totalFee),
                new SqlParameter("@DiscountAmount", input.DiscountAmount),
                new SqlParameter("@Note", DbValue(Optional(input.Note))));

            return ManagementResult.Success("Tạo ghi danh thành công.");
        });
    }

    public ManagementResult DeleteEnrollment(int id)
    {
        return SoftDelete("delete enrollment", "dbo.Enrollments", id);
    }

    public ReceiptInput? GetReceipt(int id)
    {
        return ReadSingle(
            """
            SELECT EnrollmentId, PaymentDate, Amount, PaymentMethod, Note
            FROM dbo.Receipts
            WHERE Id = @Id;
            """,
            reader => new ReceiptInput
            {
                EnrollmentId = GetInt32(reader, "EnrollmentId"),
                PaymentDate = GetDateTime(reader, "PaymentDate"),
                Amount = GetDecimal(reader, "Amount"),
                PaymentMethod = GetString(reader, "PaymentMethod"),
                Note = GetString(reader, "Note")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveReceipt(int? id, ReceiptInput input)
    {
        return ExecuteWrite("save receipt", (connection, transaction) =>
        {
            if (input.EnrollmentId <= 0)
            {
                return ManagementResult.Fail("Ghi danh là bắt buộc.");
            }

            if (!Exists(connection, transaction,
                "SELECT 1 FROM dbo.Enrollments WHERE Id = @Id AND IsDeleted = 0;",
                new SqlParameter("@Id", input.EnrollmentId)))
            {
                return ManagementResult.Fail("Không tìm thấy ghi danh đã chọn.");
            }

            if (input.Amount <= 0)
            {
                return ManagementResult.Fail("Số tiền phải lớn hơn 0.");
            }

            var paymentMethod = Required(input.PaymentMethod, "Phương thức thanh toán là bắt buộc.");

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.Receipts
                    SET EnrollmentId = @EnrollmentId,
                        PaymentDate = @PaymentDate,
                        Amount = @Amount,
                        PaymentMethod = @PaymentMethod,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@EnrollmentId", input.EnrollmentId),
                    new SqlParameter("@PaymentDate", input.PaymentDate),
                    new SqlParameter("@Amount", input.Amount),
                    new SqlParameter("@PaymentMethod", paymentMethod),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật biên nhận thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.Receipts
                (
                    ReceiptCode,
                    EnrollmentId,
                    PaymentDate,
                    Amount,
                    PaymentMethod,
                    Note,
                    CreatedAt
                )
                VALUES
                (
                    @ReceiptCode,
                    @EnrollmentId,
                    @PaymentDate,
                    @Amount,
                    @PaymentMethod,
                    @Note,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@ReceiptCode", GenerateCode(connection, transaction, "dbo.Receipts", "ReceiptCode", "R")),
                new SqlParameter("@EnrollmentId", input.EnrollmentId),
                new SqlParameter("@PaymentDate", input.PaymentDate),
                new SqlParameter("@Amount", input.Amount),
                new SqlParameter("@PaymentMethod", paymentMethod),
                new SqlParameter("@Note", DbValue(Optional(input.Note))));

            return ManagementResult.Success("Tạo biên nhận thành công.");
        });
    }

    public ManagementResult DeleteReceipt(int id)
    {
        return DeletePhysical("delete receipt", "DELETE FROM dbo.Receipts WHERE Id = @Id;", id);
    }

    public SessionInput? GetSession(int id)
    {
        return ReadSingle(
            """
            SELECT cl.ClassCode, cs.SessionDate, cs.Topic, cs.Note
            FROM dbo.ClassSessions cs
            INNER JOIN dbo.Classes cl ON cl.Id = cs.ClassId
            WHERE cs.Id = @Id;
            """,
            reader => new SessionInput
            {
                ClassCode = GetString(reader, "ClassCode"),
                SessionDate = GetDateTime(reader, "SessionDate"),
                Topic = GetString(reader, "Topic"),
                Note = GetString(reader, "Note")
            },
            new SqlParameter("@Id", id));
    }

    public ManagementResult SaveSession(int? id, SessionInput input)
    {
        return ExecuteWrite("save session", (connection, transaction) =>
        {
            var classCode = Required(input.ClassCode, "Lớp học là bắt buộc.");
            var topic = Required(input.Topic, "Chủ đề buổi học là bắt buộc.");
            var classId = ResolveId(connection, transaction,
                "SELECT Id FROM dbo.Classes WHERE ClassCode = @Code AND IsDeleted = 0;",
                new SqlParameter("@Code", classCode));

            if (!classId.HasValue)
            {
                return ManagementResult.Fail("Không tìm thấy lớp học đã chọn.");
            }

            if (id.HasValue)
            {
                ExecuteNonQuery(connection, transaction,
                    """
                    UPDATE dbo.ClassSessions
                    SET ClassId = @ClassId,
                        SessionDate = @SessionDate,
                        Topic = @Topic,
                        Note = @Note
                    WHERE Id = @Id;
                    """,
                    new SqlParameter("@Id", id.Value),
                    new SqlParameter("@ClassId", classId.Value),
                    new SqlParameter("@SessionDate", input.SessionDate.Date),
                    new SqlParameter("@Topic", topic),
                    new SqlParameter("@Note", DbValue(Optional(input.Note))));

                return ManagementResult.Success("Cập nhật buổi học thành công.");
            }

            ExecuteNonQuery(connection, transaction,
                """
                INSERT INTO dbo.ClassSessions
                (
                    ClassId,
                    SessionDate,
                    Topic,
                    Note,
                    CreatedAt
                )
                VALUES
                (
                    @ClassId,
                    @SessionDate,
                    @Topic,
                    @Note,
                    SYSDATETIME()
                );
                """,
                new SqlParameter("@ClassId", classId.Value),
                new SqlParameter("@SessionDate", input.SessionDate.Date),
                new SqlParameter("@Topic", topic),
                new SqlParameter("@Note", DbValue(Optional(input.Note))));

            return ManagementResult.Success("Tạo buổi học thành công.");
        });
    }

    public ManagementResult DeleteSession(int id)
    {
        return DeletePhysical("delete session", "DELETE FROM dbo.ClassSessions WHERE Id = @Id;", id);
    }
}
