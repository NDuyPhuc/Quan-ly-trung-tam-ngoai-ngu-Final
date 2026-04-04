namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public class DemoAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class Student
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public DateTime JoinedOn { get; set; }
    public decimal TuitionFee { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DebtAmount { get; set; }
}

public class Teacher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AssignedClassCount { get; set; }
}

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string ScheduleSummary { get; set; } = string.Empty;
    public decimal TuitionFee { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetOutput { get; set; } = string.Empty;
    public string NextOpening { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public List<string> Objectives { get; set; } = [];
    public List<string> Highlights { get; set; } = [];
}

public class CourseClass
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Enrolled { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class Enrollment
{
    public int Id { get; set; }
    public string EnrollmentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public DateTime EnrolledOn { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal TotalFee { get; set; }
    public decimal PaidAmount { get; set; }
}

public class Receipt
{
    public int Id { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ClassCode { get; set; } = string.Empty;
    public DateTime PaidOn { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TuitionDebt
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal TotalFee { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ClassSession
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AttendanceRecord
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string SessionTopic { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public class ExamResult
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal AverageScore { get; set; }
    public string Result { get; set; } = string.Empty;
}

public class NewsArticle
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedOn { get; set; }
    public bool IsFeatured { get; set; }
}
