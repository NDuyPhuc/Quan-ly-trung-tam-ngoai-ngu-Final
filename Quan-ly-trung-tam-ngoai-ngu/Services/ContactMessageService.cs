using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services;

public sealed class ContactMessageService : IContactMessageService
{
    private const string ConsultationLeadPrefix = "TV";

    private readonly ApplicationDbContext _dbContext;
    private readonly SmtpMailOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ContactMessageService> _logger;

    public ContactMessageService(
        ApplicationDbContext dbContext,
        IOptions<SmtpMailOptions> options,
        IWebHostEnvironment environment,
        ILogger<ContactMessageService> logger)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ContactDispatchResult> SendContactAsync(ContactMessageRequest request, CancellationToken cancellationToken = default)
    {
        var leadRecord = await UpsertConsultationLeadAsync(request, cancellationToken);
        var subject = $"[NorthStar English] Yêu cầu tư vấn: {request.Topic}";
        var body = BuildContactBody(request);
        var successMessage = leadRecord.WasCreated
            ? $"Trung tâm đã ghi nhận hồ sơ tư vấn {leadRecord.StudentCode} của bạn."
            : $"Trung tâm đã cập nhật hồ sơ tư vấn {leadRecord.StudentCode} của bạn.";
        var fallbackMessage = leadRecord.WasCreated
            ? $"Trung tâm đã ghi nhận hồ sơ tư vấn {leadRecord.StudentCode}. Email sẽ được đồng bộ khi cấu hình SMTP hoàn tất."
            : $"Trung tâm đã cập nhật hồ sơ tư vấn {leadRecord.StudentCode}. Email sẽ được đồng bộ khi cấu hình SMTP hoàn tất.";

        return await SendOrArchiveAsync(
            payload: request,
            archivePrefix: "contact",
            subject: subject,
            body: body,
            replyToEmail: request.Email,
            replyToName: request.FullName,
            successMessage: successMessage,
            fallbackMessage: fallbackMessage,
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

    private async Task<ConsultationLeadRecord> UpsertConsultationLeadAsync(ContactMessageRequest request, CancellationToken cancellationToken)
    {
        var normalizedFullName = request.FullName.Trim();
        var normalizedEmail = NormalizeNullable(request.Email);
        var normalizedPhone = NormalizeNullable(request.Phone);
        var note = BuildConsultationNote(request);

        StudentEntity? student = null;
        if (!string.IsNullOrWhiteSpace(normalizedEmail))
        {
            student = await _dbContext.Students
                .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
        }

        if (student is null && !string.IsNullOrWhiteSpace(normalizedPhone))
        {
            student = await _dbContext.Students
                .FirstOrDefaultAsync(x => x.Phone == normalizedPhone, cancellationToken);
        }

        if (student is not null)
        {
            student.IsDeleted = false;
            student.UpdatedAt = DateTime.Now;

            if (IsConsultationLead(student))
            {
                student.FullName = normalizedFullName;
                student.Email = normalizedEmail;
                student.Phone = normalizedPhone;
                student.Address = note;
                student.Status = 1;
            }
            else
            {
                student.Phone ??= normalizedPhone;
                student.Email ??= normalizedEmail;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return new ConsultationLeadRecord(student.StudentCode, WasCreated: false);
        }

        var nextStudentCode = await GenerateNextConsultationLeadCodeAsync(cancellationToken);
        _dbContext.Students.Add(new StudentEntity
        {
            StudentCode = nextStudentCode,
            FullName = normalizedFullName,
            Email = normalizedEmail,
            Phone = normalizedPhone,
            Address = note,
            Status = 1,
            IsDeleted = false,
            CreatedAt = DateTime.Now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ConsultationLeadRecord(nextStudentCode, WasCreated: true);
    }

    private async Task<string> GenerateNextConsultationLeadCodeAsync(CancellationToken cancellationToken)
    {
        var codes = await _dbContext.Students
            .AsNoTracking()
            .Where(x => x.StudentCode.StartsWith(ConsultationLeadPrefix))
            .Select(x => x.StudentCode)
            .ToListAsync(cancellationToken);

        var nextNumber = codes
            .Select(code => int.TryParse(code[ConsultationLeadPrefix.Length..], out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        return $"{ConsultationLeadPrefix}{nextNumber:0000}";
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

    private static bool IsConsultationLead(StudentEntity student)
    {
        return student.StudentCode.StartsWith(ConsultationLeadPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeNullable(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string BuildConsultationNote(ContactMessageRequest request)
    {
        var parts = new List<string>
        {
            $"Lead tư vấn từ {request.SourcePage}"
        };

        if (!string.IsNullOrWhiteSpace(request.Topic))
        {
            parts.Add($"Nhu cầu: {request.Topic}");
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredProgram))
        {
            parts.Add($"Khóa học: {request.PreferredProgram}");
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredSchedule))
        {
            parts.Add($"Lịch mong muốn: {request.PreferredSchedule}");
        }

        return string.Join(" | ", parts);
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

    private sealed record ConsultationLeadRecord(string StudentCode, bool WasCreated);
}
