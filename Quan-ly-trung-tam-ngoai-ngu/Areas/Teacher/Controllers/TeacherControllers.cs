using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Teacher.Controllers;

public class DashboardController : TeacherControllerBase
{
    public DashboardController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        return DashboardView(new DashboardPageViewModel
        {
            Title = "Bảng điều khiển giáo viên",
            Subtitle = "Theo dõi lớp đang dạy, buổi học hôm nay, điểm danh và nhập điểm trên cùng một bố cục trực quan hơn.",
            Breadcrumbs = Breadcrumbs("Tổng quan"),
            RoleName = "Giáo viên",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Lớp đang dạy", Value = DataService.GetClasses().Count.ToString(), Description = "Theo phân công hiện tại", Icon = "bi-easel", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Buổi học hôm nay", Value = DataService.GetSessions().Count(x => x.Status == "Hôm nay").ToString(), Description = "Cần chuẩn bị điểm danh", Icon = "bi-calendar-day", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Học viên cần điểm danh", Value = DataService.GetAttendanceRecords().Count.ToString(), Description = "Danh sách thao tác nhanh", Icon = "bi-list-check", AccentClass = "warning" },
                new SummaryCardViewModel { Title = "Bài kiểm tra cần nhập điểm", Value = DataService.GetExamResults().Count.ToString(), Description = "Kết quả chờ cập nhật", Icon = "bi-journal-richtext", AccentClass = "success" }
            ],
            Charts = [new ChartCardViewModel { ChartId = "teacherProgress", Title = "Tiến độ lớp theo tuần", Subtitle = "Biểu đồ tiến độ học tập của lớp", ChartType = "line", Labels = ["Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4"], Values = [25, 48, 66, 82], Colors = ["#1d4ed8"] }],
            Panels = [new DashboardPanelViewModel { Title = "Lịch dạy gần nhất", Subtitle = "Các buổi học cần chuẩn bị", Items = DataService.GetSessions().Select(x => new PanelItemViewModel { Title = x.ClassCode, Meta = $"{x.SessionDate:dd/MM/yyyy} • {x.TimeSlot}", Value = x.Topic, BadgeText = x.Status, BadgeClass = AppUi.StatusBadgeClass(x.Status) }).ToList() }],
            QuickActions = [new QuickActionViewModel { Label = "Điểm danh theo buổi", Url = "/Teacher/Attendance/Create", Icon = "bi-list-check" }, new QuickActionViewModel { Label = "Nhập điểm", Url = "/Teacher/Exams/Create", Icon = "bi-pencil-square", CssClass = "btn btn-outline-primary" }]
        });
    }
}

public class ClassesController : TeacherControllerBase
{
    public ClassesController(IMockDataService dataService) : base(dataService) { }
    public IActionResult Index() => TeacherPageHelpers.List(this, "Lớp được phân công", "/Teacher/Classes", DataService.GetClasses().Select(x => new TableRowViewModel { Id = x.Id.ToString(), Cells = [new() { Html = $"<strong>{x.Code}</strong><div class='text-muted small'>{x.CourseName}</div>" }, new() { Html = x.Schedule }, new() { Html = $"{x.Enrolled}/{x.Capacity} HV" }, new() { Html = AppUi.StatusBadge(x.Status) }, new() { Html = "" }], Actions = [new() { Label = "Chi tiết", Url = $"/Teacher/Classes/Details/{x.Id}", Icon = "bi-eye" }] }).ToList());
    public IActionResult Details(int id) => TeacherPageHelpers.Details(this, "Chi tiết lớp được phân công", Breadcrumbs("Chi tiết lớp", "Lớp được phân công", "/Teacher/Classes"), "/Teacher/Classes");
}

public class AttendanceController : TeacherControllerBase
{
    public AttendanceController(IMockDataService dataService) : base(dataService) { }
    public IActionResult Index() => TeacherPageHelpers.List(this, "Điểm danh", "/Teacher/Attendance", DataService.GetAttendanceRecords().Select(x => new TableRowViewModel { Id = x.Id.ToString(), Cells = [new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.ClassCode}</div>" }, new() { Html = x.SessionTopic }, new() { Html = x.AttendanceDate.ToString("dd/MM/yyyy") }, new() { Html = AppUi.StatusBadge(x.Status) }, new() { Html = "" }], Actions = [new() { Label = "Chi tiết", Url = $"/Teacher/Attendance/Details/{x.Id}", Icon = "bi-eye" }, new() { Label = "Sửa", Url = $"/Teacher/Attendance/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }] }).ToList());
    public IActionResult Create() => TeacherPageHelpers.Form(this, "Điểm danh theo buổi", Breadcrumbs("Điểm danh", "Điểm danh", "/Teacher/Attendance"), "/Teacher/Attendance", [new() { Label = "Lớp học", Name = "ClassCode", Required = true }, new() { Label = "Buổi học", Name = "SessionTopic", ColClass = "col-12", Required = true }, new() { Label = "Học viên", Name = "StudentName", Required = true }, new() { Label = "Trạng thái", Name = "Status", Type = "select", Options = [new() { Label = "Có mặt", Value = "Có mặt", Selected = true }, new() { Label = "Vắng", Value = "Vắng" }, new() { Label = "Muộn", Value = "Muộn" }] }, new() { Label = "Ghi chú", Name = "Note", ColClass = "col-12" }]);
    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => TeacherPageHelpers.Details(this, "Chi tiết điểm danh", Breadcrumbs("Chi tiết điểm danh", "Điểm danh", "/Teacher/Attendance"), "/Teacher/Attendance");
}

public class ExamsController : TeacherControllerBase
{
    public ExamsController(IMockDataService dataService) : base(dataService) { }
    public IActionResult Index() => TeacherPageHelpers.List(this, "Nhập điểm", "/Teacher/Exams", DataService.GetExamResults().Select(x => new TableRowViewModel { Id = x.Id.ToString(), Cells = [new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.ClassCode}</div>" }, new() { Html = x.ExamType }, new() { Html = x.Score.ToString("0.0") }, new() { Html = AppUi.StatusBadge(x.Result) }, new() { Html = "" }], Actions = [new() { Label = "Chi tiết", Url = $"/Teacher/Exams/Details/{x.Id}", Icon = "bi-eye" }, new() { Label = "Sửa", Url = $"/Teacher/Exams/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }] }).ToList());
    public IActionResult Create() => TeacherPageHelpers.Form(this, "Nhập điểm giữa kỳ / cuối kỳ", Breadcrumbs("Nhập điểm", "Nhập điểm", "/Teacher/Exams"), "/Teacher/Exams", [new() { Label = "Lớp học", Name = "ClassCode", Required = true }, new() { Label = "Học viên", Name = "StudentName", Required = true }, new() { Label = "Loại bài kiểm tra", Name = "ExamType", Required = true }, new() { Label = "Điểm số", Name = "Score", Type = "number", Required = true }, new() { Label = "Kết quả", Name = "Result", Type = "select", Options = [new() { Label = "Đạt", Value = "Đạt", Selected = true }, new() { Label = "Cần cải thiện", Value = "Cần cải thiện" }] }]);
    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => TeacherPageHelpers.Details(this, "Chi tiết điểm số", Breadcrumbs("Chi tiết điểm số", "Nhập điểm", "/Teacher/Exams"), "/Teacher/Exams");
}

public class ScheduleController : TeacherControllerBase
{
    public ScheduleController(IMockDataService dataService) : base(dataService) { }
    public IActionResult Index() => TeacherPageHelpers.List(this, "Lịch dạy", "/Teacher/Schedule", DataService.GetSessions().Select(x => new TableRowViewModel { Id = x.Id.ToString(), Cells = [new() { Html = $"<strong>{x.ClassCode}</strong><div class='text-muted small'>{x.Topic}</div>" }, new() { Html = x.SessionDate.ToString("dd/MM/yyyy") }, new() { Html = x.TimeSlot }, new() { Html = AppUi.StatusBadge(x.Status) }, new() { Html = "" }], Actions = [new() { Label = "Chi tiết", Url = $"/Teacher/Schedule/Details/{x.Id}", Icon = "bi-eye" }] }).ToList());
    public IActionResult Details(int id) => TeacherPageHelpers.Details(this, "Chi tiết lịch dạy", Breadcrumbs("Chi tiết lịch dạy", "Lịch dạy", "/Teacher/Schedule"), "/Teacher/Schedule");
}

internal static class TeacherPageHelpers
{
    internal static IActionResult List(TeacherControllerBase controller, string title, string baseUrl, List<TableRowViewModel> rows) => controller.ManagementListView(new ManagementListPageViewModel
    {
        Title = title,
        Subtitle = $"Phân hệ {title.ToLowerInvariant()} dành cho giáo viên.",
        Breadcrumbs = TeacherControllerBase.Breadcrumbs(title),
        PrimaryActionText = $"Mở {title.ToLowerInvariant()}",
        PrimaryActionUrl = $"{baseUrl}/Create",
        Table = new TableViewModel { Columns = [new() { Header = title }, new() { Header = "Thông tin 1" }, new() { Header = "Thông tin 2" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }], Rows = rows }
    });

    internal static IActionResult Form(TeacherControllerBase controller, string title, List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, List<FormFieldViewModel> fields) => controller.ManagementFormView(new ManagementFormPageViewModel
    {
        Title = title,
        Subtitle = "Biểu mẫu nghiệp vụ dành cho giáo viên.",
        Breadcrumbs = breadcrumbs,
        FormTitle = title,
        FormDescription = "Dữ liệu hiện phục vụ trình bày giao diện và sẽ được nối với hệ thống thật ở bước sau.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng kiểm tra kỹ thông tin trước khi xác nhận.",
        Sections = [new FormSectionViewModel { Title = "Thông tin", Fields = fields }]
    });

    internal static IActionResult Details(TeacherControllerBase controller, string title, List<BreadcrumbItemViewModel> breadcrumbs, string backUrl) => controller.ManagementDetailsView(new ManagementDetailsPageViewModel
    {
        Title = title,
        Subtitle = "Trang chi tiết dùng để minh họa luồng thao tác của giáo viên.",
        Breadcrumbs = breadcrumbs,
        Sections = [new DetailSectionViewModel { Title = "Ghi chú", Items = [new() { Label = "Tình trạng", Value = "Sẵn sàng sử dụng", IsBadge = true, BadgeClass = "bg-success-subtle text-success-emphasis" }, new() { Label = "Bước tiếp theo", Value = "Bổ sung thêm dữ liệu giảng dạy khi hệ thống mở rộng." }] }],
        Actions = [new QuickActionViewModel { Label = "Quay lại", Url = backUrl, Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }]
    });
}
