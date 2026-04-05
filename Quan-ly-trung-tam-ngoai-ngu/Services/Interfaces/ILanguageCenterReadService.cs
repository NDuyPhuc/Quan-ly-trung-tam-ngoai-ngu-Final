using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface ILanguageCenterReadService
{
    IReadOnlyList<DemoAccount> GetAccounts();
    IReadOnlyList<Student> GetStudents();
    IReadOnlyList<Teacher> GetTeachers();
    IReadOnlyList<Course> GetCourses();
    IReadOnlyList<CourseClass> GetClasses();
    IReadOnlyList<Enrollment> GetEnrollments();
    IReadOnlyList<Receipt> GetReceipts();
    IReadOnlyList<TuitionDebt> GetDebts();
    IReadOnlyList<ClassSession> GetSessions();
    IReadOnlyList<AttendanceRecord> GetAttendanceRecords();
    IReadOnlyList<Exam> GetExams();
    IReadOnlyList<ExamResult> GetExamResults();
    IReadOnlyList<NewsArticle> GetNewsArticles();
}
