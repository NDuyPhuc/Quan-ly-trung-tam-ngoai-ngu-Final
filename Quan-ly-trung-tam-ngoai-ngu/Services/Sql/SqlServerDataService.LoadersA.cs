using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public partial class SqlServerDataService
{
    private IReadOnlyList<DemoAccount> LoadAccounts()
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

        return Query(sql, reader =>
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
        });
    }

    private IReadOnlyList<Student> LoadStudents()
    {
        const string sql = """
                           SELECT
                               s.Id,
                               s.StudentCode,
                               s.FullName,
                               s.Email,
                               s.Phone,
                               s.Status AS StudentStatus,
                               s.CreatedAt,
                               latest.EnrollDate,
                               latest.EnrollmentStatus,
                               latest.FinalFee,
                               latest.PaidAmount,
                               latest.CourseName,
                               latest.ClassCode,
                               latest.ClassStartDate,
                               latest.ClassEndDate
                           FROM dbo.Students s
                           OUTER APPLY
                           (
                               SELECT TOP (1)
                                   e.EnrollDate,
                                   e.Status AS EnrollmentStatus,
                                   e.FinalFee,
                                   ISNULL(payment.PaidAmount, 0) AS PaidAmount,
                                   c.CourseName,
                                   cl.ClassCode,
                                   cl.StartDate AS ClassStartDate,
                                   cl.EndDate AS ClassEndDate
                               FROM dbo.Enrollments e
                               INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId AND cl.IsDeleted = 0
                               INNER JOIN dbo.Courses c ON c.Id = cl.CourseId AND c.IsDeleted = 0
                               OUTER APPLY
                               (
                                   SELECT SUM(r.Amount) AS PaidAmount
                                   FROM dbo.Receipts r
                                   WHERE r.EnrollmentId = e.Id
                               ) payment
                               WHERE e.StudentId = s.Id AND e.IsDeleted = 0
                               ORDER BY e.EnrollDate DESC, e.Id DESC
                           ) latest
                           WHERE s.IsDeleted = 0
                           ORDER BY s.Id;
                           """;

        return Query(sql, reader =>
        {
            var courseName = GetString(reader, "CourseName");
            var finalFee = GetDecimal(reader, "FinalFee");
            var paidAmount = GetDecimal(reader, "PaidAmount");
            var debtAmount = Math.Max(0, finalFee - paidAmount);
            var enrollDate = GetNullableDateTime(reader, "EnrollDate");
            var createdAt = GetNullableDateTime(reader, "CreatedAt");

            return new Student
            {
                Id = GetInt32(reader, "Id"),
                Code = GetString(reader, "StudentCode"),
                FullName = GetString(reader, "FullName"),
                Email = GetString(reader, "Email"),
                Phone = GetString(reader, "Phone"),
                Level = InferCourseLevel(courseName),
                Status = MapStudentStatus(
                    GetString(reader, "EnrollmentStatus"),
                    GetNullableDateTime(reader, "ClassStartDate"),
                    GetNullableDateTime(reader, "ClassEndDate"),
                    GetByte(reader, "StudentStatus")),
                CourseName = courseName,
                ClassCode = GetString(reader, "ClassCode"),
                JoinedOn = enrollDate ?? createdAt ?? DateTime.Today,
                TuitionFee = finalFee,
                PaidAmount = paidAmount,
                DebtAmount = debtAmount
            };
        });
    }

    private IReadOnlyList<Teacher> LoadTeachers()
    {
        const string sql = """
                           SELECT
                               t.Id,
                               t.TeacherCode,
                               t.FullName,
                               t.Email,
                               t.Phone,
                               t.Specialization,
                               t.Status,
                               ISNULL(classStats.AssignedClassCount, 0) AS AssignedClassCount
                           FROM dbo.Teachers t
                           OUTER APPLY
                           (
                               SELECT COUNT(*) AS AssignedClassCount
                               FROM dbo.Classes c
                               WHERE c.TeacherId = t.Id AND c.IsDeleted = 0
                           ) classStats
                           WHERE t.IsDeleted = 0
                           ORDER BY t.Id;
                           """;

        return Query(sql, reader =>
        {
            var specialization = GetString(reader, "Specialization");

            return new Teacher
            {
                Id = GetInt32(reader, "Id"),
                Code = GetString(reader, "TeacherCode"),
                FullName = GetString(reader, "FullName"),
                Email = GetString(reader, "Email"),
                Phone = GetString(reader, "Phone"),
                Specialty = specialization,
                Qualification = string.IsNullOrWhiteSpace(specialization) ? "Đang cập nhật" : $"Chuyên môn {specialization}",
                Status = GetByte(reader, "Status") == 1 ? "Đang giảng dạy" : "Tạm khóa",
                AssignedClassCount = GetInt32(reader, "AssignedClassCount")
            };
        });
    }

    private IReadOnlyList<Course> LoadCourses()
    {
        const string sql = """
                           SELECT
                               c.Id,
                               c.CourseCode,
                               c.CourseName,
                               c.Description,
                               c.DurationHours,
                               c.TuitionFee,
                               c.Status,
                               ISNULL(studentStats.StudentCount, 0) AS StudentCount,
                               nextClass.StartDate AS NextStartDate,
                               nextClass.ScheduleText
                           FROM dbo.Courses c
                           OUTER APPLY
                           (
                               SELECT COUNT(*) AS StudentCount
                               FROM dbo.Enrollments e
                               INNER JOIN dbo.Classes cl ON cl.Id = e.ClassId AND cl.IsDeleted = 0
                               WHERE cl.CourseId = c.Id
                                 AND e.IsDeleted = 0
                                 AND e.Status <> N'Huy'
                           ) studentStats
                           OUTER APPLY
                           (
                               SELECT TOP (1)
                                   cl.StartDate,
                                   cl.ScheduleText
                               FROM dbo.Classes cl
                               WHERE cl.CourseId = c.Id AND cl.IsDeleted = 0
                               ORDER BY
                                   CASE WHEN cl.StartDate >= CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END,
                                   cl.StartDate,
                                   cl.Id
                           ) nextClass
                           WHERE c.IsDeleted = 0
                           ORDER BY c.Id;
                           """;

        return Query(sql, reader =>
        {
            var courseName = GetString(reader, "CourseName");
            var description = GetString(reader, "Description");
            var nextStartDate = GetNullableDateTime(reader, "NextStartDate");
            var durationHours = GetInt32(reader, "DurationHours");
            var scheduleSummary = GetString(reader, "ScheduleText");

            return new Course
            {
                Id = GetInt32(reader, "Id"),
                Code = GetString(reader, "CourseCode"),
                Slug = CreateSlug($"{GetString(reader, "CourseCode")} {courseName}"),
                Name = courseName,
                Level = InferCourseLevel(courseName),
                Duration = durationHours > 0 ? $"{durationHours} giờ" : "Đang cập nhật",
                ScheduleSummary = string.IsNullOrWhiteSpace(scheduleSummary) ? "Lịch sẽ cập nhật theo lớp mở" : scheduleSummary,
                TuitionFee = GetDecimal(reader, "TuitionFee"),
                Status = MapCourseStatus(GetByte(reader, "Status"), nextStartDate),
                ShortDescription = BuildShortDescription(description, courseName),
                Description = string.IsNullOrWhiteSpace(description) ? $"Chương trình {courseName} đang được cập nhật mô tả chi tiết trong cơ sở dữ liệu." : description,
                TargetOutput = BuildCourseTarget(courseName, durationHours),
                NextOpening = nextStartDate?.ToString("dd/MM/yyyy") ?? "Đang cập nhật",
                StudentCount = GetInt32(reader, "StudentCount"),
                Objectives = BuildCourseObjectives(courseName),
                Highlights = BuildCourseHighlights(courseName)
            };
        });
    }
}
