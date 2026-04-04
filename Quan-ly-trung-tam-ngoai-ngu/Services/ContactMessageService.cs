using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services;

public sealed class ContactMessageService : IContactMessageService
{
    private readonly SmtpMailOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ContactMessageService> _logger;

    public ContactMessageService(
        IOptions<SmtpMailOptions> options,
        IWebHostEnvironment environment,
        ILogger<ContactMessageService> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ContactDispatchResult> SendContactAsync(ContactMessageRequest request, CancellationToken cancellationToken = default)
    {
        var subject = $"[NorthStar English] Yêu cầu tư vấn: {request.Topic}";
        var body = BuildContactBody(request);

        return await SendOrArchiveAsync(
            payload: request,
            archivePrefix: "contact",
            subject: subject,
            body: body,
            replyToEmail: request.Email,
            replyToName: request.FullName,
            successMessage: "Trung tâm đã ghi nhận yêu cầu tư vấn của bạn.",
            fallbackMessage: "Trung tâm đã ghi nhận thông tin của bạn. Email sẽ được đồng bộ ngay khi cấu hình SMTP hoàn tất.",
            cancellationToken: cancellationToken);
    }

    public async Task<ContactDispatchResult> SendNewsletterAsync(NewsletterSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var subject = "[NorthStar English] Đăng ký nhận thông tin mới";
        var body = $"""
Nguồn gửi: {request.SourcePage}
Email đăng ký: {request.Email}
Thời gian: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
""";

        return await SendOrArchiveAsync(
            payload: request,
            archivePrefix: "newsletter",
            subject: subject,
            body: body,
            replyToEmail: request.Email,
            replyToName: request.Email,
            successMessage: "Đã ghi nhận email đăng ký nhận thông tin mới.",
            fallbackMessage: "Đã ghi nhận email của bạn. Hệ thống sẽ đồng bộ thông tin ngay khi cấu hình SMTP hoàn tất.",
            cancellationToken: cancellationToken);
    }

    private async Task<ContactDispatchResult> SendOrArchiveAsync(
        object payload,
        string archivePrefix,
        string subject,
        string body,
        string replyToEmail,
        string replyToName,
        string successMessage,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        if (CanSendEmail())
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_options.SenderEmail, _options.SenderName),
                    Subject = subject,
                    Body = body,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    IsBodyHtml = false
                };

                message.To.Add(_options.RecipientEmail);

                if (!string.IsNullOrWhiteSpace(replyToEmail))
                {
                    message.ReplyToList.Add(new MailAddress(replyToEmail, replyToName));
                }

                using var client = new SmtpClient(_options.Host, _options.Port)
                {
                    EnableSsl = _options.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_options.Username, _options.Password)
                };

                cancellationToken.ThrowIfCancellationRequested();
                await client.SendMailAsync(message, cancellationToken);
                return ContactDispatchResult.Success(successMessage, emailDelivered: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không gửi được email tư vấn/newsletter. Chuyển sang lưu dữ liệu nội bộ.");
            }
        }

        await ArchivePayloadAsync(archivePrefix, payload, cancellationToken);
        return ContactDispatchResult.Success(fallbackMessage, emailDelivered: false);
    }

    private bool CanSendEmail()
    {
        return !string.IsNullOrWhiteSpace(_options.Host)
            && _options.Port > 0
            && !string.IsNullOrWhiteSpace(_options.SenderEmail)
            && !string.IsNullOrWhiteSpace(_options.Username)
            && !string.IsNullOrWhiteSpace(_options.Password)
            && !string.IsNullOrWhiteSpace(_options.RecipientEmail);
    }

    private async Task ArchivePayloadAsync(string archivePrefix, object payload, CancellationToken cancellationToken)
    {
        var archiveDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "contact-archive");
        Directory.CreateDirectory(archiveDirectory);

        var fileName = $"{archivePrefix}-{DateTime.Now:yyyyMMdd-HHmmssfff}.json";
        var filePath = Path.Combine(archiveDirectory, fileName);

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, payload, payload.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        }, cancellationToken);
    }

    private static string BuildContactBody(ContactMessageRequest request)
    {
        return $"""
Họ tên: {request.FullName}
Email: {request.Email}
Số điện thoại: {request.Phone}
Nhu cầu: {request.Topic}
Khóa học quan tâm: {request.PreferredProgram}
Trình độ hiện tại: {request.CurrentLevel}
Khung giờ mong muốn: {request.PreferredSchedule}
Cách liên hệ mong muốn: {request.PreferredContactMethod}
Nguồn gửi: {request.SourcePage}

Nội dung:
{request.Message}

Thời gian gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
""";
    }
}
