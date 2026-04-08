namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class ManagementResult
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ManagementResult Success(string message)
    {
        return new ManagementResult
        {
            Succeeded = true,
            Message = message
        };
    }

    public static ManagementResult Fail(string message)
    {
        return new ManagementResult
        {
            Succeeded = false,
            Message = message
        };
    }
}

public sealed class AccountInput
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class StudentInput
{
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class TeacherInput
{
    public string TeacherCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class CourseInput
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public decimal TuitionFee { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ClassInput
{
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today;
    public string ScheduleText { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class EnrollmentInput
{
    public string StudentCode { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public DateTime EnrollDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = "DangHoc";
    public decimal TotalFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class ReceiptInput
{
    public int EnrollmentId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string Note { get; set; } = string.Empty;
}

public sealed class SessionInput
{
    public string ClassCode { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; } = DateTime.Today;
    public string Topic { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public sealed class AttendanceInput
{
    public string SelectedClassCode { get; set; } = string.Empty;
    public int EnrollmentId { get; set; }
    public int ClassSessionId { get; set; }
    public string AttendanceStatus { get; set; } = "Present";
    public string Note { get; set; } = string.Empty;
}

public sealed class ExamInput
{
    public string ClassCode { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; } = DateTime.Today;
    public decimal MaxScore { get; set; } = 10;
}

public sealed class ExamResultInput
{
    public int EnrollmentId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; } = DateTime.Today;
    public decimal MaxScore { get; set; } = 10;
    public decimal Score { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public sealed class NewsArticleInput
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; } = DateTime.Today;
    public bool IsFeatured { get; set; }
}

public sealed class PublicHomePageInput
{
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
}

public sealed class PublicAboutPageInput
{
    public string SectionTitle { get; set; } = string.Empty;
    public string SectionSubtitle { get; set; } = string.Empty;
    public string HighlightTitle { get; set; } = string.Empty;
    public string HighlightBody { get; set; } = string.Empty;
}

public sealed class PublicContactPageInput
{
    public string SectionTitle { get; set; } = string.Empty;
    public string SectionSubtitle { get; set; } = string.Empty;
    public string FormTitle { get; set; } = string.Empty;
    public string FormSubtitle { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string SupportHours { get; set; } = string.Empty;
}
