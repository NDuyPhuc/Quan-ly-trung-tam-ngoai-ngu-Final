using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    public DashboardController(IMockDataService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var model = new DashboardPageViewModel
        {
            Title = "Bảng điều khiển quản trị",
            Subtitle = "Tổng quan vận hành trung tâm ngoại ngữ trên giao diện mới, không thay đổi nghiệp vụ hiện có.",
            Breadcrumbs = Breadcrumbs("Tổng quan"),
            RoleName = "Quản trị viên",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng học viên", Value = DataService.GetStudents().Count.ToString(), Description = "Đang theo học và bảo lưu", Icon = "bi-people", AccentClass = "primary", Trend = "+18 tháng này" },
                new SummaryCardViewModel { Title = "Tổng giáo viên", Value = DataService.GetTeachers().Count.ToString(), Description = "Bao gồm IELTS, TOEIC và giao tiếp", Icon = "bi-person-video3", AccentClass = "info", Trend = "1 giáo viên nghỉ phép" },
                new SummaryCardViewModel { Title = "Lớp đang hoạt động", Value = DataService.GetClasses().Count(x => x.Status == "Đang hoạt động").ToString(), Description = "Có lịch học hoặc điểm danh", Icon = "bi-easel2", AccentClass = "success", Trend = "2 lớp sắp khai giảng" },
                new SummaryCardViewModel { Title = "Doanh thu tháng", Value = AppUi.Currency(DataService.GetReceipts().Sum(x => x.Amount)), Description = "Tổng thu theo biên nhận hiện có", Icon = "bi-cash-coin", AccentClass = "warning", Trend = "76% kế hoạch" },
                new SummaryCardViewModel { Title = "Học viên còn nợ", Value = DataService.GetDebts().Count.ToString(), Description = "Cần theo dõi công nợ", Icon = "bi-wallet2", AccentClass = "danger", Trend = "1 khoản quá hạn" }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Thêm học viên", Url = "/Admin/Students/Create", Icon = "bi-person-plus", CssClass = "btn btn-primary" },
                new QuickActionViewModel { Label = "Mở lớp mới", Url = "/Admin/Classes/Create", Icon = "bi-plus-square", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Thu học phí", Url = "/Admin/Receipts/Create", Icon = "bi-receipt", CssClass = "btn btn-outline-dark" }
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
                    Colors = ["#1d4ed8"]
                },
                new ChartCardViewModel
                {
                    ChartId = "adminRevenueChart",
                    Title = "Doanh thu theo tháng",
                    Subtitle = "Biểu đồ tổng thu dựa trên biên nhận hiện có",
                    ChartType = "bar",
                    Labels = ["11", "12", "01", "02", "03", "04"],
                    Values = [48, 55, 63, 71, 68, 82],
                    Colors = ["#0f172a", "#1d4ed8", "#38bdf8", "#f59e0b", "#10b981", "#f97316"]
                }
            ],
            Panels =
            [
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
                },
                new DashboardPanelViewModel
                {
                    Title = "Ghi danh gần đây",
                    Subtitle = "Danh sách ghi danh mới cần kiểm tra",
                    ActionLabel = "Mở ghi danh",
                    ActionUrl = "/Admin/Enrollments",
                    Items = DataService.GetEnrollments().OrderByDescending(x => x.EnrolledOn).Take(4).Select(x => new PanelItemViewModel
                    {
                        Title = x.StudentName,
                        Meta = $"{x.CourseName} • {x.ClassCode}",
                        Value = AppUi.Currency(x.PaidAmount),
                        BadgeText = x.PaymentStatus,
                        BadgeClass = AppUi.StatusBadgeClass(x.PaymentStatus)
                    }).ToList()
                }
            ],
            Timeline =
            [
                new TimelineItemViewModel { Title = "Khóa IELTS Nền Tảng sắp khai giảng", Meta = "20/04/2026", Description = "Cần chốt danh sách, kiểm tra học phí và in thẻ lớp.", AccentClass = "warning" },
                new TimelineItemViewModel { Title = "Có 1 khoản công nợ quá hạn", Meta = "Học viên Đỗ Khánh Linh", Description = "Nên chuyển sang khu giáo vụ để xử lý nhắc phí và cập nhật biên nhận.", AccentClass = "danger" },
                new TimelineItemViewModel { Title = "Báo cáo tháng đã sẵn sàng", Meta = "Dữ liệu tổng hợp", Description = "Khu vực báo cáo đang có biểu đồ doanh thu, ghi danh và top khóa học.", AccentClass = "success" }
            ]
        };

        return DashboardView(model);
    }
}
