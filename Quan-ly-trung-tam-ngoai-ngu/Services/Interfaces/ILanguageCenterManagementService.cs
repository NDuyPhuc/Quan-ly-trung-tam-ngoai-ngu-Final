using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface ILanguageCenterManagementService
{
    AccountInput? GetAccount(int id);
    ManagementResult SaveAccount(int? id, AccountInput input);
    ManagementResult DeleteAccount(int id);

    StudentInput? GetStudent(int id);
    ManagementResult SaveStudent(int? id, StudentInput input);
    ManagementResult DeleteStudent(int id);

    TeacherInput? GetTeacher(int id);
    ManagementResult SaveTeacher(int? id, TeacherInput input);
    ManagementResult DeleteTeacher(int id);

    CourseInput? GetCourse(int id);
    ManagementResult SaveCourse(int? id, CourseInput input);
    ManagementResult DeleteCourse(int id);

    ClassInput? GetClass(int id);
    ManagementResult SaveClass(int? id, ClassInput input);
    ManagementResult DeleteClass(int id);

    EnrollmentInput? GetEnrollment(int id);
    ManagementResult SaveEnrollment(int? id, EnrollmentInput input);
    ManagementResult DeleteEnrollment(int id);

    ReceiptInput? GetReceipt(int id);
    ManagementResult SaveReceipt(int? id, ReceiptInput input);
    ManagementResult DeleteReceipt(int id);

    SessionInput? GetSession(int id);
    ManagementResult SaveSession(int? id, SessionInput input);
    ManagementResult DeleteSession(int id);

    AttendanceInput? GetAttendance(int id);
    ManagementResult SaveAttendance(int? id, AttendanceInput input);
    ManagementResult DeleteAttendance(int id);

    ExamInput? GetExam(int id);
    ManagementResult SaveExam(int? id, ExamInput input);
    ManagementResult DeleteExam(int id);

    ExamResultInput? GetExamResult(int id);
    ManagementResult SaveExamResult(int? id, ExamResultInput input);
    ManagementResult DeleteExamResult(int id);
}
