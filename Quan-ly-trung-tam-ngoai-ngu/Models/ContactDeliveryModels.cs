namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class ContactMessageRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string PreferredProgram { get; set; } = string.Empty;
    public string CurrentLevel { get; set; } = string.Empty;
    public string PreferredSchedule { get; set; } = string.Empty;
    public string PreferredContactMethod { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SourcePage { get; set; } = string.Empty;
}

public sealed class NewsletterSubscriptionRequest
{
    public string Email { get; set; } = string.Empty;
    public string SourcePage { get; set; } = string.Empty;
}

public sealed class ContactDispatchResult
{
    public bool Succeeded { get; init; }
    public bool EmailDelivered { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ContactDispatchResult Success(string message, bool emailDelivered)
    {
        return new ContactDispatchResult
        {
            Succeeded = true,
            EmailDelivered = emailDelivered,
            Message = message
        };
    }

    public static ContactDispatchResult Fail(string message)
    {
        return new ContactDispatchResult
        {
            Succeeded = false,
            EmailDelivered = false,
            Message = message
        };
    }
}

public sealed class SmtpMailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
}
