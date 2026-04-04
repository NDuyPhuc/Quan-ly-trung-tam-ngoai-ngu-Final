using System.Globalization;
using System.Text;
using System.Text.Json;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

namespace Quan_ly_trung_tam_ngoai_ngu.Services;

public sealed class PublicSiteContentService : IPublicSiteContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly object _syncRoot = new();
    private readonly string _newsFilePath;
    private readonly string _settingsFilePath;
    private readonly ILogger<PublicSiteContentService> _logger;
    private List<NewsArticle> _articles;
    private PublicSiteSettings _settings;

    public PublicSiteContentService(IWebHostEnvironment environment, ILogger<PublicSiteContentService> logger)
    {
        _logger = logger;

        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);

        _newsFilePath = Path.Combine(dataDirectory, "public-news.json");
        _settingsFilePath = Path.Combine(dataDirectory, "public-site-settings.json");
        _articles = LoadArticles();
        _settings = LoadSettings();
    }

    public PublicSiteSettings GetSiteSettings()
    {
        lock (_syncRoot)
        {
            return CloneSettings(_settings);
        }
    }

    public ManagementResult SaveHomePageContent(PublicHomePageInput input)
    {
        if (string.IsNullOrWhiteSpace(input.HeroTitle))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề chính cho trang chủ.");
        }

        if (string.IsNullOrWhiteSpace(input.HeroSubtitle))
        {
            return ManagementResult.Fail("Vui lòng nhập phần mô tả cho trang chủ.");
        }

        lock (_syncRoot)
        {
            _settings.HomePage.HeroTitle = input.HeroTitle.Trim();
            _settings.HomePage.HeroSubtitle = input.HeroSubtitle.Trim();
            SaveSettings();
        }

        return ManagementResult.Success("Đã cập nhật nội dung phần Trang chủ.");
    }

    public ManagementResult SaveAboutPageContent(PublicAboutPageInput input)
    {
        if (string.IsNullOrWhiteSpace(input.SectionTitle))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề phần Giới thiệu.");
        }

        if (string.IsNullOrWhiteSpace(input.SectionSubtitle))
        {
            return ManagementResult.Fail("Vui lòng nhập phần mô tả phần Giới thiệu.");
        }

        if (string.IsNullOrWhiteSpace(input.HighlightTitle))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề khối nổi bật.");
        }

        if (string.IsNullOrWhiteSpace(input.HighlightBody))
        {
            return ManagementResult.Fail("Vui lòng nhập nội dung khối nổi bật.");
        }

        lock (_syncRoot)
        {
            _settings.AboutSection.SectionTitle = input.SectionTitle.Trim();
            _settings.AboutSection.SectionSubtitle = input.SectionSubtitle.Trim();
            _settings.AboutSection.HighlightTitle = input.HighlightTitle.Trim();
            _settings.AboutSection.HighlightBody = input.HighlightBody.Trim();
            SaveSettings();
        }

        return ManagementResult.Success("Đã cập nhật nội dung phần Giới thiệu.");
    }

    public ManagementResult SaveContactPageContent(PublicContactPageInput input)
    {
        if (string.IsNullOrWhiteSpace(input.SectionTitle))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề phần Liên hệ.");
        }

        if (string.IsNullOrWhiteSpace(input.SectionSubtitle))
        {
            return ManagementResult.Fail("Vui lòng nhập mô tả phần Liên hệ.");
        }

        if (string.IsNullOrWhiteSpace(input.FormTitle))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề khối biểu mẫu.");
        }

        if (string.IsNullOrWhiteSpace(input.FormSubtitle))
        {
            return ManagementResult.Fail("Vui lòng nhập mô tả khối biểu mẫu.");
        }

        if (string.IsNullOrWhiteSpace(input.SupportEmail))
        {
            return ManagementResult.Fail("Vui lòng nhập email hỗ trợ.");
        }

        if (string.IsNullOrWhiteSpace(input.SupportPhone))
        {
            return ManagementResult.Fail("Vui lòng nhập số điện thoại hỗ trợ.");
        }

        if (string.IsNullOrWhiteSpace(input.SupportHours))
        {
            return ManagementResult.Fail("Vui lòng nhập khung giờ hỗ trợ.");
        }

        lock (_syncRoot)
        {
            _settings.ContactSection.SectionTitle = input.SectionTitle.Trim();
            _settings.ContactSection.SectionSubtitle = input.SectionSubtitle.Trim();
            _settings.ContactSection.FormTitle = input.FormTitle.Trim();
            _settings.ContactSection.FormSubtitle = input.FormSubtitle.Trim();
            _settings.ContactSection.SupportEmail = input.SupportEmail.Trim();
            _settings.ContactSection.SupportPhone = input.SupportPhone.Trim();
            _settings.ContactSection.SupportHours = input.SupportHours.Trim();
            SaveSettings();
        }

        return ManagementResult.Success("Đã cập nhật nội dung phần Liên hệ.");
    }

    public IReadOnlyList<NewsArticle> GetNewsArticles()
    {
        lock (_syncRoot)
        {
            return _articles
                .OrderByDescending(article => article.IsFeatured)
                .ThenByDescending(article => article.PublishedOn)
                .Select(CloneArticle)
                .ToList();
        }
    }

    public NewsArticle? GetNewsArticle(int id)
    {
        lock (_syncRoot)
        {
            var article = _articles.FirstOrDefault(item => item.Id == id);
            return article is null ? null : CloneArticle(article);
        }
    }

    public ManagementResult SaveNewsArticle(int? id, NewsArticleInput input)
    {
        var title = input.Title.Trim();
        var category = input.Category.Trim();
        var summary = input.Summary.Trim();
        var content = input.Content.Trim();
        var author = input.Author.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return ManagementResult.Fail("Vui lòng nhập tiêu đề tin tức.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            return ManagementResult.Fail("Vui lòng nhập chuyên mục.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            return ManagementResult.Fail("Vui lòng nhập phần tóm tắt.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ManagementResult.Fail("Vui lòng nhập nội dung bài viết.");
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            return ManagementResult.Fail("Vui lòng nhập tên người đăng.");
        }

        lock (_syncRoot)
        {
            var article = id.HasValue ? _articles.FirstOrDefault(item => item.Id == id.Value) : null;
            if (id.HasValue && article is null)
            {
                return ManagementResult.Fail("Không tìm thấy tin tức cần cập nhật.");
            }

            var slug = EnsureUniqueSlug(BuildSlug(input.Slug, title), id);
            var publishedOn = input.PublishedOn == default ? DateTime.Today : input.PublishedOn.Date;

            if (article is null)
            {
                article = new NewsArticle
                {
                    Id = _articles.Count == 0 ? 1 : _articles.Max(item => item.Id) + 1
                };
                _articles.Add(article);
            }

            article.Slug = slug;
            article.Title = title;
            article.Category = category;
            article.Summary = summary;
            article.Content = content;
            article.Author = author;
            article.PublishedOn = publishedOn;
            article.IsFeatured = input.IsFeatured;

            SaveArticles();

            return ManagementResult.Success(id.HasValue
                ? "Đã cập nhật tin tức công khai."
                : "Đã thêm tin tức công khai mới.");
        }
    }

    public ManagementResult DeleteNewsArticle(int id)
    {
        lock (_syncRoot)
        {
            var article = _articles.FirstOrDefault(item => item.Id == id);
            if (article is null)
            {
                return ManagementResult.Fail("Không tìm thấy tin tức để xóa.");
            }

            _articles.Remove(article);
            SaveArticles();

            return ManagementResult.Success("Đã xóa tin tức công khai.");
        }
    }

    private List<NewsArticle> LoadArticles()
    {
        if (!File.Exists(_newsFilePath))
        {
            var defaults = GetDefaultArticles();
            PersistArticles(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_newsFilePath);
            var articles = JsonSerializer.Deserialize<List<NewsArticle>>(json, JsonOptions);

            if (articles is { Count: > 0 })
            {
                return articles;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể tải dữ liệu tin tức công khai từ tệp JSON, sẽ khởi tạo lại dữ liệu mặc định.");
        }

        var seededArticles = GetDefaultArticles();
        PersistArticles(seededArticles);
        return seededArticles;
    }

    private PublicSiteSettings LoadSettings()
    {
        if (!File.Exists(_settingsFilePath))
        {
            var defaults = GetDefaultSettings();
            PersistSettings(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<PublicSiteSettings>(json, JsonOptions);

            if (settings is not null)
            {
                return EnsureSettingsDefaults(settings);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể tải cấu hình nội dung công khai, sẽ khởi tạo lại dữ liệu mặc định.");
        }

        var seededSettings = GetDefaultSettings();
        PersistSettings(seededSettings);
        return seededSettings;
    }

    private void SaveArticles()
    {
        PersistArticles(_articles);
    }

    private void SaveSettings()
    {
        PersistSettings(_settings);
    }

    private void PersistArticles(List<NewsArticle> articles)
    {
        var json = JsonSerializer.Serialize(articles.OrderBy(item => item.Id).ToList(), JsonOptions);
        File.WriteAllText(_newsFilePath, json, Encoding.UTF8);
    }

    private void PersistSettings(PublicSiteSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsFilePath, json, Encoding.UTF8);
    }

    private string BuildSlug(string requestedSlug, string title)
    {
        var source = string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug;
        source = RemoveDiacritics(source).ToLowerInvariant();

        var builder = new StringBuilder();
        var previousDash = false;

        foreach (var character in source)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousDash = false;
            }
            else if (!previousDash)
            {
                builder.Append('-');
                previousDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"tin-tuc-{DateTime.UtcNow:yyyyMMddHHmmss}" : slug;
    }

    private string EnsureUniqueSlug(string baseSlug, int? currentId)
    {
        var slug = baseSlug;
        var suffix = 2;

        while (_articles.Any(item =>
                   string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase) &&
                   (!currentId.HasValue || item.Id != currentId.Value)))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }

    private static NewsArticle CloneArticle(NewsArticle article)
    {
        return new NewsArticle
        {
            Id = article.Id,
            Slug = article.Slug,
            Title = article.Title,
            Category = article.Category,
            Summary = article.Summary,
            Content = article.Content,
            Author = article.Author,
            PublishedOn = article.PublishedOn,
            IsFeatured = article.IsFeatured
        };
    }

    private static PublicSiteSettings CloneSettings(PublicSiteSettings settings)
    {
        return new PublicSiteSettings
        {
            HomePage = new PublicHomeSectionContent
            {
                HeroTitle = settings.HomePage.HeroTitle,
                HeroSubtitle = settings.HomePage.HeroSubtitle
            },
            AboutSection = new PublicAboutSectionContent
            {
                SectionTitle = settings.AboutSection.SectionTitle,
                SectionSubtitle = settings.AboutSection.SectionSubtitle,
                HighlightTitle = settings.AboutSection.HighlightTitle,
                HighlightBody = settings.AboutSection.HighlightBody
            },
            ContactSection = new PublicContactSectionContent
            {
                SectionTitle = settings.ContactSection.SectionTitle,
                SectionSubtitle = settings.ContactSection.SectionSubtitle,
                FormTitle = settings.ContactSection.FormTitle,
                FormSubtitle = settings.ContactSection.FormSubtitle,
                SupportEmail = settings.ContactSection.SupportEmail,
                SupportPhone = settings.ContactSection.SupportPhone,
                SupportHours = settings.ContactSection.SupportHours
            }
        };
    }

    private static PublicSiteSettings EnsureSettingsDefaults(PublicSiteSettings settings)
    {
        settings.HomePage ??= new PublicHomeSectionContent();
        settings.AboutSection ??= new PublicAboutSectionContent();
        settings.ContactSection ??= new PublicContactSectionContent();

        var defaults = GetDefaultSettings();

        settings.HomePage.HeroTitle = string.IsNullOrWhiteSpace(settings.HomePage.HeroTitle)
            ? defaults.HomePage.HeroTitle
            : settings.HomePage.HeroTitle;
        settings.HomePage.HeroSubtitle = string.IsNullOrWhiteSpace(settings.HomePage.HeroSubtitle)
            ? defaults.HomePage.HeroSubtitle
            : settings.HomePage.HeroSubtitle;

        settings.AboutSection.SectionTitle = string.IsNullOrWhiteSpace(settings.AboutSection.SectionTitle)
            ? defaults.AboutSection.SectionTitle
            : settings.AboutSection.SectionTitle;
        settings.AboutSection.SectionSubtitle = string.IsNullOrWhiteSpace(settings.AboutSection.SectionSubtitle)
            ? defaults.AboutSection.SectionSubtitle
            : settings.AboutSection.SectionSubtitle;
        settings.AboutSection.HighlightTitle = string.IsNullOrWhiteSpace(settings.AboutSection.HighlightTitle)
            ? defaults.AboutSection.HighlightTitle
            : settings.AboutSection.HighlightTitle;
        settings.AboutSection.HighlightBody = string.IsNullOrWhiteSpace(settings.AboutSection.HighlightBody)
            ? defaults.AboutSection.HighlightBody
            : settings.AboutSection.HighlightBody;

        settings.ContactSection.SectionTitle = string.IsNullOrWhiteSpace(settings.ContactSection.SectionTitle)
            ? defaults.ContactSection.SectionTitle
            : settings.ContactSection.SectionTitle;
        settings.ContactSection.SectionSubtitle = string.IsNullOrWhiteSpace(settings.ContactSection.SectionSubtitle)
            ? defaults.ContactSection.SectionSubtitle
            : settings.ContactSection.SectionSubtitle;
        settings.ContactSection.FormTitle = string.IsNullOrWhiteSpace(settings.ContactSection.FormTitle)
            ? defaults.ContactSection.FormTitle
            : settings.ContactSection.FormTitle;
        settings.ContactSection.FormSubtitle = string.IsNullOrWhiteSpace(settings.ContactSection.FormSubtitle)
            ? defaults.ContactSection.FormSubtitle
            : settings.ContactSection.FormSubtitle;
        settings.ContactSection.SupportEmail = string.IsNullOrWhiteSpace(settings.ContactSection.SupportEmail)
            ? defaults.ContactSection.SupportEmail
            : settings.ContactSection.SupportEmail;
        settings.ContactSection.SupportPhone = string.IsNullOrWhiteSpace(settings.ContactSection.SupportPhone)
            ? defaults.ContactSection.SupportPhone
            : settings.ContactSection.SupportPhone;
        settings.ContactSection.SupportHours = string.IsNullOrWhiteSpace(settings.ContactSection.SupportHours)
            ? defaults.ContactSection.SupportHours
            : settings.ContactSection.SupportHours;

        return settings;
    }

    private static List<NewsArticle> GetDefaultArticles()
    {
        return
        [
            new NewsArticle
            {
                Id = 1,
                Slug = "lich-khai-giang-thang-4",
                Title = "Lịch khai giảng các khóa tháng 4/2026",
                Category = "Thông báo",
                Summary = "Cập nhật các lớp TOEIC, IELTS và Giao tiếp bắt đầu trong tháng 4 cùng ưu đãi giữ chỗ sớm.",
                Content = "Trung tâm NorthStar English chính thức mở đăng ký cho các lớp khai giảng tháng 4. Học viên có thể lựa chọn các khung giờ tối 2-4-6, 3-5-7 hoặc cuối tuần tùy theo lịch học và làm việc. Chương trình ưu đãi áp dụng cho học viên đăng ký trước ngày khai giảng 7 ngày.",
                Author = "Phòng giáo vụ",
                PublishedOn = new DateTime(2026, 4, 1),
                IsFeatured = true
            },
            new NewsArticle
            {
                Id = 2,
                Slug = "workshop-chien-luoc-toeic",
                Title = "Buổi chia sẻ miễn phí: Chiến lược đạt TOEIC 700+",
                Category = "Sự kiện",
                Summary = "Buổi chia sẻ cách học và quản lý thời gian khi làm bài TOEIC dành cho sinh viên năm cuối.",
                Content = "Buổi chia sẻ tập trung vào các chiến lược cải thiện nhanh điểm Đọc hiểu và Nghe hiểu, kết hợp phần giải đáp trực tiếp với giáo viên TOEIC tại trung tâm. Người tham dự sẽ được tặng bộ tài liệu ôn tập nhanh và phiếu học thử.",
                Author = "Ban học thuật",
                PublishedOn = new DateTime(2026, 3, 28),
                IsFeatured = true
            },
            new NewsArticle
            {
                Id = 3,
                Slug = "kinh-nghiem-hoc-ielts-cho-nguoi-moi",
                Title = "Kinh nghiệm học IELTS cho người mới bắt đầu",
                Category = "Chia sẻ học tập",
                Summary = "5 lưu ý quan trọng khi bắt đầu lộ trình IELTS từ nền tảng cơ bản.",
                Content = "Với người mới bắt đầu IELTS, điều quan trọng là xác định mục tiêu đầu ra, đánh giá đúng trình độ và chọn lộ trình phù hợp. Bên cạnh đó, cần dành thời gian xây dựng phát âm, từ vựng nền và thói quen tiếp xúc với tiếng Anh mỗi ngày.",
                Author = "Nguyễn Mai Anh",
                PublishedOn = new DateTime(2026, 3, 25),
                IsFeatured = false
            }
        ];
    }

    private static PublicSiteSettings GetDefaultSettings()
    {
        return new PublicSiteSettings
        {
            HomePage = new PublicHomeSectionContent
            {
                HeroTitle = "Nền tảng quản lý trung tâm ngoại ngữ hiện đại và trực quan.",
                HeroSubtitle = "Hệ thống hỗ trợ tuyển sinh, quản lý lớp học, theo dõi giảng dạy, cập nhật học phí và tư vấn học viên trong cùng một không gian rõ ràng, sáng và dễ dùng."
            },
            AboutSection = new PublicAboutSectionContent
            {
                SectionTitle = "Giá trị cốt lõi và cách vận hành",
                SectionSubtitle = "NorthStar English tập trung vào sự rõ ràng trong lộ trình học, giao diện dễ dùng và khả năng theo dõi tiến độ xuyên suốt cho học viên.",
                HighlightTitle = "Lợi thế khi học tại NorthStar",
                HighlightBody = "Thiết kế mới giữ nguyên nghiệp vụ nhưng trình bày hiện đại hơn để người dùng nhìn là hiểu ngay các khu vực cần thao tác."
            },
            ContactSection = new PublicContactSectionContent
            {
                SectionTitle = "Liên hệ và đăng ký tư vấn",
                SectionSubtitle = "Đây là khu vực liên hệ thật sự dùng được: người dùng có thể để lại nhu cầu, khung giờ học và cách liên hệ mong muốn để trung tâm phản hồi.",
                FormTitle = "Gửi thông tin ngay tại trang chủ",
                FormSubtitle = "Điền form bên dưới để trung tâm nhận nhu cầu cụ thể và phản hồi đúng khóa học bạn đang quan tâm.",
                SupportEmail = "nguyenthanhdanhctk42@gmail.com",
                SupportPhone = "0901 001 001",
                SupportHours = "08:00 - 20:30 mỗi ngày"
            }
        };
    }
}
