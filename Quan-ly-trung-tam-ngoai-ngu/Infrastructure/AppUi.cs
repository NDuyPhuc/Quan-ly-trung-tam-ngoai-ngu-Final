using System.Globalization;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Infrastructure;

public static class AppUi
{
    private static readonly CultureInfo ViCulture = new("vi-VN");

    public static string Currency(decimal amount)
    {
        return string.Format(ViCulture, "{0:N0} đ", amount);
    }

    public static string RoleLabel(string value)
    {
        return value switch
        {
            AppConstants.Roles.Admin => "Quản trị viên",
            AppConstants.Roles.Staff => "Giáo vụ",
            AppConstants.Roles.Teacher => "Giáo viên",
            _ => value
        };
    }

    public static string StatusBadge(string value)
    {
        return $"<span class=\"badge {StatusBadgeClass(value)}\">{value}</span>";
    }

    public static string StatusBadgeClass(string value)
    {
        return value switch
        {
            "Đang hoạt động" or "Đang học" or "Đã thanh toán" or "Đạt" or "Có mặt" or "Hoàn tất" or "Đã ghi nhận" or "Đã xếp lớp" or "Đang giảng dạy" or "Hoàn thành"
                => "bg-success-subtle text-success-emphasis",
            "Sắp khai giảng" or "Sắp mở lớp" or "Sắp đến hạn" or "Đóng một phần" or "Đóng cọc" or "Muộn" or "Khai giảng sớm"
                => "bg-warning-subtle text-warning-emphasis",
            "Quá hạn" or "Còn nợ" or "Chờ xác nhận" or "Bảo lưu" or "Cần cải thiện" or "Vắng" or "Đã hủy" or "Tạm khóa" or "Tạm dừng"
                => "bg-danger-subtle text-danger-emphasis",
            "Đang tuyển sinh" or "Mở đăng ký" or "Hôm nay" or "Sắp diễn ra"
                => "bg-info-subtle text-info-emphasis",
            _ => "bg-secondary-subtle text-secondary-emphasis"
        };
    }

    public static string CourseImageUrl(string slug)
    {
        return slug switch
        {
            "toeic-650-plus" => "/images/placeholders/course-toeic.svg",
            "ielts-foundation" => "/images/placeholders/course-ielts-foundation.svg",
            "giao-tiep-ung-dung" => "/images/placeholders/course-communication.svg",
            "ielts-intensive-65" => "/images/placeholders/course-ielts-intensive.svg",
            _ => "/images/placeholders/course-generic.svg"
        };
    }

    public static string NewsImageUrl(string slug)
    {
        return slug switch
        {
            "lich-khai-giang-thang-4" => "/images/placeholders/news-announcement.svg",
            "workshop-chien-luoc-toeic" => "/images/placeholders/news-workshop.svg",
            "kinh-nghiem-hoc-ielts-cho-nguoi-moi" => "/images/placeholders/news-study.svg",
            _ => "/images/placeholders/news-generic.svg"
        };
    }

    public static string PublicImageUrl(string key)
    {
        return key switch
        {
            "hero" => "/images/placeholders/home-hero.svg",
            "about" => "/images/placeholders/about-hero.svg",
            "contact" => "/images/placeholders/contact-hero.svg",
            "login" => "/images/placeholders/auth-login.svg",
            "register" => "/images/placeholders/auth-register.svg",
            "forgot" => "/images/placeholders/auth-forgot.svg",
            _ => "/images/placeholders/home-hero.svg"
        };
    }

    public static string CourseTag(string slug)
    {
        return slug switch
        {
            "toeic-650-plus" => "PHỔ BIẾN",
            "ielts-foundation" => "NỀN TẢNG",
            "giao-tiep-ung-dung" => "ĐANG MỞ",
            "ielts-intensive-65" => "CHUYÊN SÂU",
            _ => "KHÓA HỌC"
        };
    }

    public static string TeacherInitials(string fullName)
    {
        return Initials(fullName, 2);
    }

    public static string ToneClass(string value)
    {
        var tones = new[]
        {
            "tone-blue",
            "tone-gold",
            "tone-indigo",
            "tone-teal"
        };

        var index = value.Sum(character => character) % tones.Length;
        return tones[index];
    }

    public static string Initials(string value, int maxLetters)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "NS";
        }

        var parts = value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .TakeLast(Math.Max(1, maxLetters))
            .Select(part => char.ToUpperInvariant(part[0]));

        return string.Concat(parts);
    }

    public static CourseCardViewModel ToCourseCard(Course course)
    {
        return new CourseCardViewModel
        {
            Id = course.Id,
            Slug = course.Slug,
            Name = course.Name,
            Level = course.Level,
            Duration = course.Duration,
            ScheduleSummary = course.ScheduleSummary,
            TuitionFee = course.TuitionFee,
            Status = course.Status,
            ShortDescription = course.ShortDescription,
            NextOpening = course.NextOpening
        };
    }

    public static ClassCardViewModel ToClassCard(CourseClass item)
    {
        return new ClassCardViewModel
        {
            Id = item.Id,
            Code = item.Code,
            CourseName = item.CourseName,
            TeacherName = item.TeacherName,
            Schedule = item.Schedule,
            Room = item.Room,
            Status = item.Status,
            StartDate = item.StartDate,
            SeatsLeft = Math.Max(0, item.Capacity - item.Enrolled)
        };
    }

    public static NewsCardViewModel ToNewsCard(NewsArticle article)
    {
        return new NewsCardViewModel
        {
            Id = article.Id,
            Slug = article.Slug,
            Title = article.Title,
            Category = article.Category,
            Summary = article.Summary,
            Author = article.Author,
            PublishedOn = article.PublishedOn
        };
    }

    public static List<BreadcrumbItemViewModel> Breadcrumbs(params (string Label, string? Url, bool IsActive)[] items)
    {
        return items.Select(item => new BreadcrumbItemViewModel
        {
            Label = item.Label,
            Url = item.Url,
            IsActive = item.IsActive
        }).ToList();
    }
}
