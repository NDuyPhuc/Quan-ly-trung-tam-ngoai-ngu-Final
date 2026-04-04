namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class StudentRegistrationResult
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? StudentCode { get; init; }

    public static StudentRegistrationResult Success(string message, string? studentCode = null)
    {
        return new StudentRegistrationResult
        {
            Succeeded = true,
            Message = message,
            StudentCode = studentCode
        };
    }

    public static StudentRegistrationResult Fail(string message)
    {
        return new StudentRegistrationResult
        {
            Succeeded = false,
            Message = message
        };
    }
}
