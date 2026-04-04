using Microsoft.Data.SqlClient;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.Services.Mocks;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Sql;

public partial class SqlServerDataService : IMockDataService
{
    private readonly string? _connectionString;
    private readonly MockDataService _fallback;
    private readonly ILogger<SqlServerDataService> _logger;
    private bool _databaseUnavailable;

    private IReadOnlyList<DemoAccount>? _accounts;
    private IReadOnlyList<Student>? _students;
    private IReadOnlyList<Teacher>? _teachers;
    private IReadOnlyList<Course>? _courses;
    private IReadOnlyList<CourseClass>? _classes;
    private IReadOnlyList<Enrollment>? _enrollments;
    private IReadOnlyList<Receipt>? _receipts;
    private IReadOnlyList<TuitionDebt>? _debts;
    private IReadOnlyList<ClassSession>? _sessions;
    private IReadOnlyList<AttendanceRecord>? _attendanceRecords;
    private IReadOnlyList<ExamResult>? _examResults;

    public SqlServerDataService(
        IConfiguration configuration,
        MockDataService fallback,
        ILogger<SqlServerDataService> logger)
    {
        _connectionString = SqlServerConnectionPolicy.PrepareConnectionString(
            configuration.GetConnectionString("LanguageCenterDb"));
        _fallback = fallback;
        _logger = logger;
    }

    public IReadOnlyList<DemoAccount> GetAccounts() => _accounts ??= TryLoad(LoadAccounts, _fallback.GetAccounts, nameof(GetAccounts));
    public IReadOnlyList<Student> GetStudents() => _students ??= TryLoad(LoadStudents, _fallback.GetStudents, nameof(GetStudents));
    public IReadOnlyList<Teacher> GetTeachers() => _teachers ??= TryLoad(LoadTeachers, _fallback.GetTeachers, nameof(GetTeachers));
    public IReadOnlyList<Course> GetCourses() => _courses ??= TryLoad(LoadCourses, _fallback.GetCourses, nameof(GetCourses));
    public IReadOnlyList<CourseClass> GetClasses() => _classes ??= TryLoad(LoadClasses, _fallback.GetClasses, nameof(GetClasses));
    public IReadOnlyList<Enrollment> GetEnrollments() => _enrollments ??= TryLoad(LoadEnrollments, _fallback.GetEnrollments, nameof(GetEnrollments));
    public IReadOnlyList<Receipt> GetReceipts() => _receipts ??= TryLoad(LoadReceipts, _fallback.GetReceipts, nameof(GetReceipts));
    public IReadOnlyList<TuitionDebt> GetDebts() => _debts ??= TryLoad(LoadDebts, _fallback.GetDebts, nameof(GetDebts));
    public IReadOnlyList<ClassSession> GetSessions() => _sessions ??= TryLoad(LoadSessions, _fallback.GetSessions, nameof(GetSessions));
    public IReadOnlyList<AttendanceRecord> GetAttendanceRecords() => _attendanceRecords ??= TryLoad(LoadAttendanceRecords, _fallback.GetAttendanceRecords, nameof(GetAttendanceRecords));
    public IReadOnlyList<ExamResult> GetExamResults() => _examResults ??= TryLoad(LoadExamResults, _fallback.GetExamResults, nameof(GetExamResults));
    public IReadOnlyList<NewsArticle> GetNewsArticles() => _fallback.GetNewsArticles();

    private IReadOnlyList<T> TryLoad<T>(Func<IReadOnlyList<T>> databaseLoader, Func<IReadOnlyList<T>> fallbackLoader, string operationName)
    {
        if (_databaseUnavailable || SqlServerConnectionPolicy.ShouldBypassDatabase() || string.IsNullOrWhiteSpace(_connectionString))
        {
            return fallbackLoader();
        }

        try
        {
            var result = databaseLoader();
            SqlServerConnectionPolicy.ReportSuccess();
            return result;
        }
        catch (Exception ex) when (ex is SqlException or InvalidOperationException or PlatformNotSupportedException)
        {
            _databaseUnavailable = true;
            SqlServerConnectionPolicy.ReportFailure();
            _logger.LogWarning(ex, "Khong the tai {OperationName} tu SQL Server, chuyen sang du lieu mock.", operationName);
            return fallbackLoader();
        }
    }

    private List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        using var reader = command.ExecuteReader();
        var items = new List<T>();
        while (reader.Read())
        {
            items.Add(map(reader));
        }

        return items;
    }

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
