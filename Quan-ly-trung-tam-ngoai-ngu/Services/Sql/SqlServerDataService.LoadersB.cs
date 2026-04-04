using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public partial class SqlServerDataService
{
    private IReadOnlyList<CourseClass> LoadClasses()
    {
        const string sql = """
                           SELECT
                               cl.Id,
                               cl.ClassCode,
                               cl.StartDate,
                               cl.EndDate,
                               cl.ScheduleText,
                               cl.Capacity,
                               cl.Status,
                               c.CourseName,
                               ISNULL(t.FullName, N'Chưa phân công') AS TeacherName,
                               ISNULL(enrollmentStats.Enrolled, 0) AS Enrolled
                           FROM dbo.Classes cl
                           INNER JOIN dbo.Courses c ON c.Id = cl.CourseId AND c.IsDeleted = 0
                           LEFT JOIN dbo.Teachers t ON t.Id = cl.TeacherId AND t.IsDeleted = 0
                           OUTER APPLY
                           (
                               SELECT COUNT(*) AS Enrolled
                               FROM dbo.Enrollments e
                               WHERE e.ClassId = cl.Id
                                 AND e.IsDeleted = 0
                                 AND e.Status <> N'Huy'
                           ) enrollmentStats
                           WHERE cl.IsDeleted = 0
                           ORDER BY cl.StartDate DESC, cl.Id DESC;
                           """;

        return Query(sql, reader =>
        {
            var startDate = GetDateTime(reader, "StartDate");
            var endDate = GetDateTime(reader, "EndDate");
            var capacity = GetInt32(reader, "Capacity");
            var enrolled = GetInt32(reader, "Enrolled");

            return new CourseClass
            {
                Id = GetInt32(reader, "Id"),
                Code = GetString(reader, "ClassCode"),
                CourseName = GetString(reader, "CourseName"),
                TeacherName = GetString(reader, "TeacherName"),
                Schedule = GetString(reader, "ScheduleText"),
                Room = "Đang cập nhật",
                Status = MapClassStatus(GetByte(reader, "Status"), startDate, endDate, enrolled, capacity),
                Capacity = capacity,
                Enrolled = enrolled,
                StartDate = startDate,
                EndDate = endDate
            };
        });
    }

    private IReadOnlyList<Enrollment> LoadEnrollments()
    {
        const string sql = """
                           SELECT
                               e.Id,
                               e.EnrollDate,
                               e.Status,
                               e.FinalFee,
                               ISNULL(payment.PaidAmount, 0) AS PaidAmount,
                               s.FullName AS StudentName,
                               c.CourseName,
                               cl.ClassCode,
                               cl.StartDate AS ClassStartDate
                           FROM dbo.Enrollments e
                           INNER JOIN dbo.Students s ON s.Id = e.StudentId AND s.IsDeleted = 0
                           INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId AND cl.IsDeleted = 0
                           INNER JOIN dbo.Courses c ON c.Id = cl.CourseId AND c.IsDeleted = 0
                           OUTER APPLY
                           (
                               SELECT SUM(r.Amount) AS PaidAmount
                               FROM dbo.Receipts r
                               WHERE r.EnrollmentId = e.Id
                           ) payment
                           WHERE e.IsDeleted = 0
                           ORDER BY e.EnrollDate DESC, e.Id DESC;
                           """;

        return Query(sql, reader =>
        {
            var enrollDate = GetDateTime(reader, "EnrollDate");
            var totalFee = GetDecimal(reader, "FinalFee");
            var paidAmount = GetDecimal(reader, "PaidAmount");

            return new Enrollment
            {
                Id = GetInt32(reader, "Id"),
                EnrollmentCode = $"GD{enrollDate:yyMMdd}{GetInt32(reader, "Id"):000}",
                StudentName = GetString(reader, "StudentName"),
                CourseName = GetString(reader, "CourseName"),
                ClassCode = GetString(reader, "ClassCode"),
                EnrolledOn = enrollDate,
                Status = MapEnrollmentStatus(GetString(reader, "Status"), GetNullableDateTime(reader, "ClassStartDate")),
                PaymentStatus = MapPaymentStatus(totalFee, paidAmount),
                TotalFee = totalFee,
                PaidAmount = paidAmount
            };
        });
    }

    private IReadOnlyList<Receipt> LoadReceipts()
    {
        const string sql = """
                           SELECT
                               r.Id,
                               r.ReceiptCode,
                               r.PaymentDate,
                               r.Amount,
                               r.PaymentMethod,
                               s.FullName AS StudentName,
                               cl.ClassCode
                           FROM dbo.Receipts r
                           INNER JOIN dbo.Enrollments e ON e.Id = r.EnrollmentId AND e.IsDeleted = 0
                           INNER JOIN dbo.Students s ON s.Id = e.StudentId AND s.IsDeleted = 0
                           INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId AND cl.IsDeleted = 0
                           ORDER BY r.PaymentDate DESC, r.Id DESC;
                           """;

        return Query(sql, reader => new Receipt
        {
            Id = GetInt32(reader, "Id"),
            ReceiptCode = GetString(reader, "ReceiptCode"),
            StudentName = GetString(reader, "StudentName"),
            ClassCode = GetString(reader, "ClassCode"),
            PaidOn = GetDateTime(reader, "PaymentDate"),
            Amount = GetDecimal(reader, "Amount"),
            PaymentMethod = MapPaymentMethod(GetString(reader, "PaymentMethod")),
            Status = "Đã ghi nhận"
        });
    }

    private IReadOnlyList<TuitionDebt> LoadDebts()
    {
        const string sql = """
                           SELECT
                               e.Id,
                               e.EnrollDate,
                               e.FinalFee,
                               ISNULL(payment.PaidAmount, 0) AS PaidAmount,
                               s.FullName AS StudentName,
                               c.CourseName,
                               cl.StartDate AS ClassStartDate
                           FROM dbo.Enrollments e
                           INNER JOIN dbo.Students s ON s.Id = e.StudentId AND s.IsDeleted = 0
                           INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId AND cl.IsDeleted = 0
                           INNER JOIN dbo.Courses c ON c.Id = cl.CourseId AND c.IsDeleted = 0
                           OUTER APPLY
                           (
                               SELECT SUM(r.Amount) AS PaidAmount
                               FROM dbo.Receipts r
                               WHERE r.EnrollmentId = e.Id
                           ) payment
                           WHERE e.IsDeleted = 0
                             AND e.FinalFee > ISNULL(payment.PaidAmount, 0)
                           ORDER BY e.EnrollDate DESC, e.Id DESC;
                           """;

        return Query(sql, reader =>
        {
            var totalFee = GetDecimal(reader, "FinalFee");
            var paidAmount = GetDecimal(reader, "PaidAmount");
            var dueDate = CalculateDebtDueDate(
                GetDateTime(reader, "EnrollDate"),
                GetNullableDateTime(reader, "ClassStartDate"));
            var remainingAmount = Math.Max(0, totalFee - paidAmount);

            return new TuitionDebt
            {
                Id = GetInt32(reader, "Id"),
                StudentName = GetString(reader, "StudentName"),
                CourseName = GetString(reader, "CourseName"),
                TotalFee = totalFee,
                PaidAmount = paidAmount,
                RemainingAmount = remainingAmount,
                DueDate = dueDate,
                Status = MapDebtStatus(dueDate)
            };
        });
    }

    private IReadOnlyList<ClassSession> LoadSessions()
    {
        const string sql = """
                           SELECT
                               cs.Id,
                               cl.ClassCode,
                               cs.Topic,
                               cs.SessionDate,
                               cl.ScheduleText
                           FROM dbo.ClassSessions cs
                           INNER JOIN dbo.Classes cl ON cl.Id = cs.ClassId AND cl.IsDeleted = 0
                           ORDER BY cs.SessionDate DESC, cs.Id DESC;
                           """;

        return Query(sql, reader =>
        {
            var sessionDate = GetDateTime(reader, "SessionDate");

            return new ClassSession
            {
                Id = GetInt32(reader, "Id"),
                ClassCode = GetString(reader, "ClassCode"),
                Topic = GetString(reader, "Topic"),
                SessionDate = sessionDate,
                TimeSlot = GetString(reader, "ScheduleText"),
                Room = "Đang cập nhật",
                Status = MapSessionStatus(sessionDate)
            };
        });
    }

    private IReadOnlyList<AttendanceRecord> LoadAttendanceRecords()
    {
        const string sql = """
                           SELECT
                               a.Id,
                               cl.ClassCode,
                               ISNULL(cs.Topic, N'Buổi học') AS SessionTopic,
                               s.FullName AS StudentName,
                               cs.SessionDate,
                               a.AttendanceStatus,
                               a.Note
                           FROM dbo.Attendances a
                           INNER JOIN dbo.Enrollments e ON e.Id = a.EnrollmentId AND e.IsDeleted = 0
                           INNER JOIN dbo.Students s ON s.Id = e.StudentId AND s.IsDeleted = 0
                           INNER JOIN dbo.ClassSessions cs ON cs.Id = a.ClassSessionId
                           INNER JOIN dbo.Classes cl ON cl.Id = cs.ClassId AND cl.IsDeleted = 0
                           ORDER BY cs.SessionDate DESC, a.Id DESC;
                           """;

        return Query(sql, reader => new AttendanceRecord
        {
            Id = GetInt32(reader, "Id"),
            ClassCode = GetString(reader, "ClassCode"),
            SessionTopic = GetString(reader, "SessionTopic"),
            StudentName = GetString(reader, "StudentName"),
            AttendanceDate = GetDateTime(reader, "SessionDate"),
            Status = MapAttendanceStatus(GetString(reader, "AttendanceStatus")),
            Note = GetString(reader, "Note")
        });
    }

    private IReadOnlyList<ExamResult> LoadExamResults()
    {
        const string sql = """
                           SELECT
                               er.Id,
                               cl.ClassCode,
                               s.FullName AS StudentName,
                               ex.ExamType,
                               ex.ExamName,
                               er.Score,
                               AVG(CAST(er.Score AS DECIMAL(10,2))) OVER (PARTITION BY er.ExamId) AS AverageScore,
                               er.ResultStatus
                           FROM dbo.ExamResults er
                           INNER JOIN dbo.Exams ex ON ex.Id = er.ExamId
                           INNER JOIN dbo.Enrollments e ON e.Id = er.EnrollmentId AND e.IsDeleted = 0
                           INNER JOIN dbo.Students s ON s.Id = e.StudentId AND s.IsDeleted = 0
                           INNER JOIN dbo.Classes cl ON cl.Id = ex.ClassId AND cl.IsDeleted = 0
                           ORDER BY ex.ExamDate DESC, er.Id DESC;
                           """;

        return Query(sql, reader =>
        {
            var score = GetDecimal(reader, "Score");
            var averageScore = GetDecimal(reader, "AverageScore");
            var resultStatus = GetString(reader, "ResultStatus");

            return new ExamResult
            {
                Id = GetInt32(reader, "Id"),
                ClassCode = GetString(reader, "ClassCode"),
                StudentName = GetString(reader, "StudentName"),
                ExamType = BuildExamLabel(GetString(reader, "ExamType"), GetString(reader, "ExamName")),
                Score = score,
                AverageScore = averageScore,
                Result = MapExamResult(resultStatus, score, averageScore)
            };
        });
    }
}
