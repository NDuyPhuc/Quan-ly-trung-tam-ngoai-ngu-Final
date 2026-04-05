using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class HomeController : ModuleControllerBase
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPublicSiteContentService _publicSiteContentService;

    public HomeController(
        ILogger<HomeController> logger,
        ILanguageCenterReadService dataService,
        IPublicSiteContentService publicSiteContentService)
        : base(dataService)
    {
        _logger = logger;
        _publicSiteContentService = publicSiteContentService;
    }

    public IActionResult Index()
    {
        var openClasses = DataService.GetClasses()
            .Where(item => !string.Equals(item.Status, "Đã đủ chỗ", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var siteSettings = _publicSiteContentService.GetSiteSettings();

        var model = new HomePageViewModel
        {
            Title = "Trang chủ",
            Subtitle = "Hệ thống quản lý trung tâm ngoại ngữ NorthStar English",
            HeroTitle = siteSettings.HomePage.HeroTitle,
            HeroSubtitle = siteSettings.HomePage.HeroSubtitle,
            AboutTitle = siteSettings.AboutSection.SectionTitle,
            AboutSubtitle = siteSettings.AboutSection.SectionSubtitle,
            AboutHighlightTitle = siteSettings.AboutSection.HighlightTitle,
            AboutHighlightBody = siteSettings.AboutSection.HighlightBody,
            ContactSectionTitle = siteSettings.ContactSection.SectionTitle,
            ContactSectionSubtitle = siteSettings.ContactSection.SectionSubtitle,
            ContactFormTitle = siteSettings.ContactSection.FormTitle,
            ContactFormSubtitle = siteSettings.ContactSection.FormSubtitle,
            SupportEmail = siteSettings.ContactSection.SupportEmail,
            SupportPhone = siteSettings.ContactSection.SupportPhone,
            SupportHours = siteSettings.ContactSection.SupportHours,
            HighlightStats =
            [
                new SummaryCardViewModel { Title = "Học viên đang theo học", Value = DataService.GetStudents().Count.ToString(), Description = "Theo dõi theo lớp và trạng thái hiện tại", Icon = "bi-people", AccentClass = "primary", Trend = "+12% trong tháng này" },
                new SummaryCardViewModel { Title = "Lớp đang mở", Value = openClasses.Count.ToString(), Description = "Có thể điều phối ngay từ bảng điều khiển", Icon = "bi-easel2", AccentClass = "info", Trend = "4 lớp sắp khai giảng" },
                new SummaryCardViewModel { Title = "Giáo viên phụ trách", Value = DataService.GetTeachers().Count.ToString(), Description = "Phân công và theo dõi lịch dạy", Icon = "bi-person-workspace", AccentClass = "success", Trend = "3 giảng viên đang hoạt động" },
                new SummaryCardViewModel { Title = "Tổng học phí dự kiến", Value = AppUi.Currency(DataService.GetEnrollments().Sum(item => item.TotalFee)), Description = "Doanh thu dự kiến cho kỳ tuyển sinh hiện tại", Icon = "bi-cash-stack", AccentClass = "warning", Trend = "Nhu cầu cao ở nhóm IELTS" }
            ],
            FeaturedCourses = DataService.GetCourses().Take(3).Select(AppUi.ToCourseCard).ToList(),
            OpenClasses = openClasses.Take(3).Select(AppUi.ToClassCard).ToList(),
            LatestNews = _publicSiteContentService.GetNewsArticles()
                .OrderByDescending(item => item.IsFeatured)
                .ThenByDescending(item => item.PublishedOn)
                .Take(3)
                .Select(AppUi.ToNewsCard)
                .ToList(),
            ContactForm = new ContactViewModel
            {
                Topic = string.Empty,
                PreferredContactMethod = "Email"
            }
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
