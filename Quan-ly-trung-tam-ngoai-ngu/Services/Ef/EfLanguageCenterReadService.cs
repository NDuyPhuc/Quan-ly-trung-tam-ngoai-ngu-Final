using Microsoft.EntityFrameworkCore;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Ef;

public sealed class EfLanguageCenterReadService : ILanguageCenterReadService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublicSiteContentService _publicSiteContentService;
    private readonly ILogger<EfLanguageCenterReadService> _logger;

    public EfLanguageCenterReadService(
        ApplicationDbContext dbContext,
        IPublicSiteContentService publicSiteContentService,
        ILogger<EfLanguageCenterReadService> logger)
    {
        _dbContext = dbContext;
        _publicSiteContentService = publicSiteContentService;
        _logger = logger;
    }

    public IReadOnlyList<DemoAccount> GetAccounts()
    {
        try
        {
            var teacherMap = _dbContext.Teachers
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Email != null)
                .ToDictionary(x => x.Email!, x => x.Specialization ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            return _dbContext.Accounts
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList()
                .Select(account =>
                {
                    teacherMap.TryGetValue(account.Email ?? string.Empty, out var specialization);

                    return new DemoAccount
                    {
                        Id = account.Id,
                        Username = account.Username,
                        FullName = account.FullName,
                        Email = account.Email ?? string.Empty,
                        Phone = account.Phone ?? string.Empty,
                        Role = account.Role,
                        Department = !string.IsNullOrWhiteSpace(specialization)
                            ? specialization
                            : account.Role == "Admin" ? "Ban quản trị" : account.Role == "Staff" ? "Giáo vụ" : "Giảng viên",
                        Status = EfServiceMapper.MapAccountStatus(account.IsActive, account.Status),
                        Password = string.IsNullOrWhiteSpace(account.PasswordHash) ? string.Empty : "********",
                        PasswordHash = account.PasswordHash
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load accounts.");
            return [];
        }
    }

    public IReadOnlyList<Student> GetStudents()
    {
        try
        {
            var students = _dbContext.Students
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList();

            var enrollmentMap = _dbContext.Enrollments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Course)
                .Include(x => x.Receipts)
                .ToList()
                .GroupBy(x => x.StudentId)
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderByDescending(item => item.EnrollDate).ThenByDescending(item => item.Id).First());

            return students.Select(student =>
            {
                enrollmentMap.TryGetValue(student.Id, out var enrollment);

                var totalFee = enrollment?.FinalFee ?? 0m;
                var paidAmount = enrollment?.Receipts.Sum(x => x.Amount) ?? 0m;
                var debtAmount = Math.Max(0m, totalFee - paidAmount);
                var isConsultationLead = EfServiceMapper.IsConsultationLead(student.StudentCode);
                var courseName = enrollment?.Class.Course.CourseName
                    ?? (isConsultationLead ? "Chưa xác định" : string.Empty);
                var classCode = enrollment?.Class.ClassCode
                    ?? (isConsultationLead ? "Chưa xếp lớp" : string.Empty);
                var level = enrollment is not null
                    ? EfServiceMapper.InferCourseLevel(enrollment.Class.Course.CourseName)
                    : isConsultationLead ? "Chờ tư vấn đầu vào" : "Tổng hợp";

                return new Student
                {
                    Id = student.Id,
                    Code = student.StudentCode,
                    FullName = student.FullName,
                    Email = student.Email ?? string.Empty,
                    Phone = student.Phone ?? string.Empty,
                    Address = student.Address ?? string.Empty,
                    Level = level,
                    Status = EfServiceMapper.MapStudentStatus(student.StudentCode, enrollment?.Status, enrollment?.Class.StartDate, enrollment?.Class.EndDate, student.Status),
                    CourseName = courseName,
                    ClassCode = classCode,
                    JoinedOn = enrollment?.EnrollDate ?? student.CreatedAt,
                    TuitionFee = totalFee,
                    PaidAmount = paidAmount,
                    DebtAmount = debtAmount
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load students.");
            return [];
        }
    }

    public IReadOnlyList<Teacher> GetTeachers()
    {
        try
        {
            var classCountByTeacher = _dbContext.Classes
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.TeacherId.HasValue)
                .GroupBy(x => x.TeacherId!.Value)
                .ToDictionary(group => group.Key, group => group.Count());

            return _dbContext.Teachers
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList()
                .Select(teacher =>
                {
                    classCountByTeacher.TryGetValue(teacher.Id, out var assignedCount);

                    return new Teacher
                    {
                        Id = teacher.Id,
                        Code = teacher.TeacherCode,
                        FullName = teacher.FullName,
                        Email = teacher.Email ?? string.Empty,
                        Phone = teacher.Phone ?? string.Empty,
                        Specialty = teacher.Specialization ?? string.Empty,
                        Qualification = string.IsNullOrWhiteSpace(teacher.Specialization) ? "Đang cập nhật" : $"Chuyên môn {teacher.Specialization}",
                        Status = teacher.Status == 1 ? "Đang giảng dạy" : "Tạm khóa",
                        AssignedClassCount = assignedCount
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load teachers.");
            return [];
        }
    }

    public IReadOnlyList<Course> GetCourses()
    {
        try
        {
            var courses = _dbContext.Courses
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList();

            var classes = _dbContext.Classes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.StartDate)
                .ToList();

            var enrollments = _dbContext.Enrollments
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.Status != "Huy")
                .ToList();

            return courses.Select(course =>
            {
                var courseClasses = classes.Where(x => x.CourseId == course.Id).ToList();
                var nextClass = courseClasses
                    .OrderBy(x => x.StartDate >= DateTime.Today ? 0 : 1)
                    .ThenBy(x => x.StartDate)
                    .ThenBy(x => x.Id)
                    .FirstOrDefault();

                var courseClassIds = courseClasses.Select(x => x.Id).ToHashSet();
                var studentCount = enrollments.Count(x => courseClassIds.Contains(x.ClassId));

                return new Course
                {
                    Id = course.Id,
                    Code = course.CourseCode,
                    Slug = EfServiceMapper.CreateSlug($"{course.CourseCode} {course.CourseName}"),
                    Name = course.CourseName,
                    Level = EfServiceMapper.InferCourseLevel(course.CourseName),
                    Duration = course.DurationHours > 0 ? $"{course.DurationHours} giờ" : "Đang cập nhật",
                    ScheduleSummary = string.IsNullOrWhiteSpace(nextClass?.ScheduleText) ? "Lịch sẽ cập nhật theo lớp mở" : nextClass.ScheduleText!,
                    TuitionFee = course.TuitionFee,
                    Status = EfServiceMapper.MapCourseStatus(course.Status, nextClass?.StartDate),
                    ShortDescription = EfServiceMapper.BuildShortDescription(course.Description, course.CourseName),
                    Description = string.IsNullOrWhiteSpace(course.Description)
                        ? $"Chương trình {course.CourseName} đang được cập nhật mô tả chi tiết trong cơ sở dữ liệu."
                        : course.Description,
                    TargetOutput = EfServiceMapper.BuildCourseTarget(course.CourseName, course.DurationHours),
                    NextOpening = nextClass?.StartDate.ToString("dd/MM/yyyy") ?? "Đang cập nhật",
                    StudentCount = studentCount,
                    Objectives = EfServiceMapper.BuildCourseObjectives(course.CourseName),
                    Highlights = EfServiceMapper.BuildCourseHighlights(course.CourseName)
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load courses.");
            return [];
        }
    }

    public IReadOnlyList<CourseClass> GetClasses()
    {
        try
        {
            return _dbContext.Classes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Course)
                .Include(x => x.Teacher)
                .Include(x => x.Enrollments)
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(courseClass =>
                {
                    var enrolledCount = courseClass.Enrollments.Count(x => !x.IsDeleted && x.Status != "Huy");

                    return new CourseClass
                    {
                        Id = courseClass.Id,
                        Code = courseClass.ClassCode,
                        CourseName = courseClass.Course.CourseName,
                        TeacherName = courseClass.Teacher?.FullName ?? "Chưa phân công",
                        Schedule = courseClass.ScheduleText ?? string.Empty,
                        Room = "Đang cập nhật",
                        Status = EfServiceMapper.MapClassStatus(courseClass.Status, courseClass.StartDate, courseClass.EndDate, enrolledCount, courseClass.Capacity),
                        Capacity = courseClass.Capacity,
                        Enrolled = enrolledCount,
                        StartDate = courseClass.StartDate,
                        EndDate = courseClass.EndDate
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load classes.");
            return [];
        }
    }

    public IReadOnlyList<Enrollment> GetEnrollments()
    {
        try
        {
            return _dbContext.Enrollments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Student)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Course)
                .Include(x => x.Receipts)
                .OrderByDescending(x => x.EnrollDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(enrollment =>
                {
                    var totalFee = enrollment.FinalFee ?? Math.Max(0m, enrollment.TotalFee - enrollment.DiscountAmount);
                    var paidAmount = enrollment.Receipts.Sum(x => x.Amount);

                    return new Enrollment
                    {
                        Id = enrollment.Id,
                        EnrollmentCode = $"GD{enrollment.EnrollDate:yyMMdd}{enrollment.Id:000}",
                        StudentName = enrollment.Student.FullName,
                        CourseName = enrollment.Class.Course.CourseName,
                        ClassCode = enrollment.Class.ClassCode,
                        EnrolledOn = enrollment.EnrollDate,
                        Status = EfServiceMapper.MapEnrollmentStatus(enrollment.Status, enrollment.Class.StartDate),
                        PaymentStatus = EfServiceMapper.MapPaymentStatus(totalFee, paidAmount),
                        TotalFee = totalFee,
                        PaidAmount = paidAmount
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load enrollments.");
            return [];
        }
    }

    public IReadOnlyList<Receipt> GetReceipts()
    {
        try
        {
            return _dbContext.Receipts
                .AsNoTracking()
                .Include(x => x.Enrollment)
                    .ThenInclude(x => x.Student)
                .Include(x => x.Enrollment)
                    .ThenInclude(x => x.Class)
                .OrderByDescending(x => x.PaymentDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(receipt => new Receipt
                {
                    Id = receipt.Id,
                    ReceiptCode = receipt.ReceiptCode,
                    StudentName = receipt.Enrollment.Student.FullName,
                    ClassCode = receipt.Enrollment.Class.ClassCode,
                    PaidOn = receipt.PaymentDate,
                    Amount = receipt.Amount,
                    PaymentMethod = EfServiceMapper.MapPaymentMethod(receipt.PaymentMethod),
                    Status = "Đã ghi nhận"
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load receipts.");
            return [];
        }
    }

    public IReadOnlyList<TuitionDebt> GetDebts()
    {
        try
        {
            return _dbContext.Enrollments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Student)
                .Include(x => x.Class)
                    .ThenInclude(x => x.Course)
                .Include(x => x.Receipts)
                .OrderByDescending(x => x.EnrollDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(enrollment =>
                {
                    var totalFee = enrollment.FinalFee ?? Math.Max(0m, enrollment.TotalFee - enrollment.DiscountAmount);
                    var paidAmount = enrollment.Receipts.Sum(x => x.Amount);
                    var remainingAmount = Math.Max(0m, totalFee - paidAmount);
                    var dueDate = EfServiceMapper.CalculateDebtDueDate(enrollment.EnrollDate, enrollment.Class.StartDate);

                    return new TuitionDebt
                    {
                        Id = enrollment.Id,
                        StudentName = enrollment.Student.FullName,
                        CourseName = enrollment.Class.Course.CourseName,
                        TotalFee = totalFee,
                        PaidAmount = paidAmount,
                        RemainingAmount = remainingAmount,
                        DueDate = dueDate,
                        Status = EfServiceMapper.MapDebtStatus(dueDate)
                    };
                })
                .Where(x => x.RemainingAmount > 0)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load debts.");
            return [];
        }
    }

    public IReadOnlyList<ClassSession> GetSessions()
    {
        try
        {
            return _dbContext.ClassSessions
                .AsNoTracking()
                .Include(x => x.Class)
                .OrderByDescending(x => x.SessionDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(session => new ClassSession
                {
                    Id = session.Id,
                    ClassCode = session.Class.ClassCode,
                    Topic = string.IsNullOrWhiteSpace(session.Topic) ? "Buổi học" : session.Topic,
                    SessionDate = session.SessionDate,
                    TimeSlot = session.Class.ScheduleText ?? string.Empty,
                    Room = "Đang cập nhật",
                    Status = EfServiceMapper.MapSessionStatus(session.SessionDate)
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load sessions.");
            return [];
        }
    }

    public IReadOnlyList<AttendanceRecord> GetAttendanceRecords()
    {
        try
        {
            return _dbContext.Attendances
                .AsNoTracking()
                .Include(x => x.Enrollment)
                    .ThenInclude(x => x.Student)
                .Include(x => x.ClassSession)
                    .ThenInclude(x => x.Class)
                .OrderByDescending(x => x.ClassSession.SessionDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(attendance => new AttendanceRecord
                {
                    Id = attendance.Id,
                    ClassCode = attendance.ClassSession.Class.ClassCode,
                    SessionTopic = string.IsNullOrWhiteSpace(attendance.ClassSession.Topic) ? "Buổi học" : attendance.ClassSession.Topic,
                    StudentName = attendance.Enrollment.Student.FullName,
                    AttendanceDate = attendance.ClassSession.SessionDate,
                    Status = EfServiceMapper.MapAttendanceStatus(attendance.AttendanceStatus),
                    Note = attendance.Note ?? string.Empty
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load attendance records.");
            return [];
        }
    }

    public IReadOnlyList<Exam> GetExams()
    {
        try
        {
            return _dbContext.Exams
                .AsNoTracking()
                .Include(x => x.Class)
                    .ThenInclude(x => x.Course)
                .Include(x => x.Results)
                .OrderByDescending(x => x.ExamDate)
                .ThenByDescending(x => x.Id)
                .ToList()
                .Select(exam =>
                {
                    var resultCount = exam.Results.Count;
                    var averageScore = resultCount == 0 ? 0m : exam.Results.Average(x => x.Score);

                    return new Exam
                    {
                        Id = exam.Id,
                        ClassCode = exam.Class.ClassCode,
                        CourseName = exam.Class.Course.CourseName,
                        ExamName = exam.ExamName,
                        ExamType = EfServiceMapper.BuildExamLabel(exam.ExamType, string.Empty),
                        ExamDate = exam.ExamDate,
                        MaxScore = exam.MaxScore,
                        ResultCount = resultCount,
                        AverageScore = averageScore,
                        Status = exam.ExamDate.Date > DateTime.Today
                            ? "Sắp diễn ra"
                            : resultCount > 0 ? "Đã có kết quả" : "Chưa nhập điểm"
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load exams.");
            return [];
        }
    }

    public IReadOnlyList<ExamResult> GetExamResults()
    {
        try
        {
            var examResults = _dbContext.ExamResults
                .AsNoTracking()
                .Include(x => x.Exam)
                    .ThenInclude(x => x.Class)
                .Include(x => x.Enrollment)
                    .ThenInclude(x => x.Student)
                .OrderByDescending(x => x.Exam.ExamDate)
                .ThenByDescending(x => x.Id)
                .ToList();

            var averageScoreByExam = examResults
                .GroupBy(x => x.ExamId)
                .ToDictionary(group => group.Key, group => group.Average(item => item.Score));

            return examResults
                .Select(result =>
                {
                    averageScoreByExam.TryGetValue(result.ExamId, out var averageScore);
                    if (averageScore == 0)
                    {
                        averageScore = result.Score;
                    }

                    return new ExamResult
                    {
                        Id = result.Id,
                        ClassCode = result.Exam.Class.ClassCode,
                        StudentName = result.Enrollment.Student.FullName,
                        ExamType = EfServiceMapper.BuildExamLabel(result.Exam.ExamType, result.Exam.ExamName),
                        Score = result.Score,
                        AverageScore = averageScore,
                        Result = EfServiceMapper.MapExamResult(result.ResultStatus, result.Score, averageScore)
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load exam results.");
            return [];
        }
    }

    public IReadOnlyList<NewsArticle> GetNewsArticles() => _publicSiteContentService.GetNewsArticles();
}
