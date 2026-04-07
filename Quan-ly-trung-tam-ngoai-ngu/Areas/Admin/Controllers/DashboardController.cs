using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly IPublicSiteContentService _publicSiteContentService;

    public DashboardController(ILanguageCenterReadService dataService, IPublicSiteContentService publicSiteContentService)
        : base(dataService)
    {
        _publicSiteContentService = publicSiteContentService;
    }

    public IActionResult Index()
    {
        var publicArticles = _publicSiteContentService.GetNewsArticles();

        var model = new DashboardPageViewModel
        {
            Title = "Bảng điều khiển quản trị",
            Subtitle = "Theo dõi vận hành trung tâm và quản trị luôn cả nội dung công khai xuất hiện ở trang chủ trước khi người dùng đăng nhập.",
            Breadcrumbs = Breadcrumbs("Tổng quan"),
            RoleName = "Quản trị viên",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng học viên", Value = DataService.GetStudents().Count.ToString(), Description = "Đang theo học và bảo lưu", Icon = "bi-people", AccentClass = "primary", Trend = "+18 tháng này" },
                new SummaryCardViewModel { Title = "Lớp hoạt động", Value = DataService.GetClasses().Count(x => x.Status == "Đang hoạt động").ToString(), Description = "Lớp đang vận hành trong hệ thống", Icon = "bi-easel2", AccentClass = "success", Trend = "2 lớp sắp khai giảng" },
                new SummaryCardViewModel { Title = "Tin công khai", Value = publicArticles.Count.ToString(), Description = "Bài viết đang hiển thị ngoài trang public", Icon = "bi-newspaper", AccentClass = "info", Trend = $"{publicArticles.Count(item => item.IsFeatured)} bài nổi bật" },
                new SummaryCardViewModel { Title = "Doanh thu tháng", Value = AppUi.Currency(DataService.GetReceipts().Sum(x => x.Amount)), Description = "Tổng thu theo biên nhận hiện có", Icon = "bi-cash-coin", AccentClass = "warning", Trend = "76% kế hoạch" }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Tạo khóa học", Url = "/Admin/Courses/Create", Icon = "bi-journal-plus", CssClass = "btn btn-primary" },
                new QuickActionViewModel { Label = "Mở lớp mới", Url = "/Admin/Classes/Create", Icon = "bi-easel2", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Thêm giáo viên", Url = "/Admin/Teachers/Create", Icon = "bi-person-plus", CssClass = "btn btn-outline-secondary" },
                new QuickActionViewModel { Label = "Tạo bài kiểm tra", Url = "/Admin/Exams/Create", Icon = "bi-clipboard-plus", CssClass = "btn btn-outline-success" },
                new QuickActionViewModel { Label = "Quản lý tin tức", Url = "/Admin/News", Icon = "bi-newspaper", CssClass = "btn btn-outline-info" },
                new QuickActionViewModel { Label = "Xem trang chủ", Url = "/", Icon = "bi-house-door", CssClass = "btn btn-outline-dark" }
            ],
            Charts =
            [
                new ChartCardViewModel
                {
                    ChartId = "adminEnrollmentChart",
                    Title = "Học viên đăng ký theo tháng",
                    Subtitle = "Số lượng ghi danh trong 6 tháng gần nhất",
                    ChartType = "line",
                    Labels = ["11", "12", "01", "02", "03", "04"],
                    Values = [28, 35, 41, 46, 52, 61],
                    Colors = ["#dbeafe"]
                },
                new ChartCardViewModel
                {
                    ChartId = "adminRevenueChart",
                    Title = "Doanh thu theo tháng",
                    Subtitle = "Biểu đồ tổng thu dựa trên biên nhận hiện có",
                    ChartType = "bar",
                    Labels = ["11", "12", "01", "02", "03", "04"],
                    Values = [48, 55, 63, 71, 68, 82],
                    Colors = ["#dbeafe", "#bfdbfe", "#93c5fd", "#60a5fa", "#3b82f6", "#2563eb"]
                }
            ],
            Panels =
            [
                new DashboardPanelViewModel
                {
                    Title = "Tin công khai mới nhất",
                    Subtitle = "Những nội dung người dùng sẽ nhìn thấy ở trang chủ và trang tin tức",
                    ActionLabel = "Mở quản lý tin tức",
                    ActionUrl = "/Admin/News",
                    Items = publicArticles.Take(4).Select(article => new PanelItemViewModel
                    {
                        Title = article.Title,
                        Meta = $"{article.Category} • {article.Author}",
                        Value = article.PublishedOn.ToString("dd/MM/yyyy"),
                        BadgeText = article.IsFeatured ? "Nổi bật" : "Thường",
                        BadgeClass = article.IsFeatured ? "bg-info-subtle text-info-emphasis" : "bg-secondary-subtle text-secondary-emphasis"
                    }).ToList()
                },
                new DashboardPanelViewModel
                {
                    Title = "Lớp đang hoạt động",
                    Subtitle = "Các lớp ưu tiên theo dõi trong tuần này",
                    ActionLabel = "Xem tất cả lớp",
                    ActionUrl = "/Admin/Classes",
                    Items = DataService.GetClasses().Where(x => x.Status == "Đang hoạt động" || x.Status == "Sắp khai giảng").Take(4).Select(x => new PanelItemViewModel
                    {
                        Title = x.Code,
                        Meta = $"{x.CourseName} • {x.Schedule}",
                        Value = $"{x.Enrolled}/{x.Capacity} HV",
                        BadgeText = x.Status,
                        BadgeClass = AppUi.StatusBadgeClass(x.Status)
                    }).ToList()
                }
            ],
            Timeline =
            [
                new TimelineItemViewModel { Title = "Tin tức công khai đã có khu quản trị riêng", Meta = "Admin / Tin tức công khai", Description = "Bạn có thể thêm, sửa, xóa bài viết và cập nhật ra trang chủ ngay từ khu quản trị.", AccentClass = "success" },
                new TimelineItemViewModel { Title = "Khóa IELTS Nền tảng sắp khai giảng", Meta = "20/04/2026", Description = "Cần chốt danh sách, kiểm tra học phí và in thẻ lớp.", AccentClass = "warning" },
                new TimelineItemViewModel { Title = "Có 1 khoản công nợ quá hạn", Meta = "Học viên Đỗ Khánh Linh", Description = "Nên chuyển sang khu giáo vụ để xử lý nhắc phí và cập nhật biên nhận.", AccentClass = "danger" }
            ]
        };

        return DashboardView(model);
    }
}
