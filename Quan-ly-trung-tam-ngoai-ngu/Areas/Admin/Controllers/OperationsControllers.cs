using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class EnrollmentsController : AdminControllerBase
{
    public EnrollmentsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(new ManagementListPageViewModel
    {
        Title = "Ghi danh",
        Subtitle = "Theo dõi đăng ký, xếp lớp và tình trạng thanh toán.",
        Breadcrumbs = Breadcrumbs("Ghi danh"),
        PrimaryActionText = "Tạo ghi danh",
        PrimaryActionUrl = "/Admin/Enrollments/Create",
        Table = new TableViewModel
        {
            Columns = [new() { Header = "Ghi danh" }, new() { Header = "Khóa/Lớp" }, new() { Header = "Trạng thái" }, new() { Header = "Thanh toán" }, new() { Header = "Thao tác", Width = "220px" }],
            Rows = DataService.GetEnrollments().Select(x => new TableRowViewModel
            {
                Id = x.Id.ToString(),
                Cells =
                [
                    new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.EnrollmentCode}</div>" },
                    new() { Html = $"{x.CourseName}<div class='text-muted small'>{x.ClassCode}</div>" },
                    new() { Html = AppUi.StatusBadge(x.Status) },
                    new() { Html = AppUi.StatusBadge(x.PaymentStatus) },
                    new() { Html = "" }
                ],
                Actions =
                [
                    new() { Label = "Chi tiết", Url = $"/Admin/Enrollments/Details/{x.Id}", Icon = "bi-eye" },
                    new() { Label = "Sửa", Url = $"/Admin/Enrollments/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                ]
            }).ToList()
        }
    });

    public IActionResult Create() => ManagementFormView(AdminPageHelpers.SimpleForm("Tạo ghi danh", Breadcrumbs("Tạo ghi danh", "Ghi danh", "/Admin/Enrollments"), "/Admin/Enrollments",
    [
        new() { Label = "Học viên", Name = "StudentName", Required = true },
        new() { Label = "Khóa học", Name = "CourseName", Required = true },
        new() { Label = "Lớp học", Name = "ClassCode", Required = true },
        new() { Label = "Đã đóng", Name = "PaidAmount", Type = "number", Required = true }
    ]));

    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => ManagementDetailsView(AdminPageHelpers.SimpleDetails("Chi tiết ghi danh", Breadcrumbs("Chi tiết ghi danh", "Ghi danh", "/Admin/Enrollments"), "/Admin/Enrollments"));
}

public class ReceiptsController : AdminControllerBase
{
    public ReceiptsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(new ManagementListPageViewModel
    {
        Title = "Học phí",
        Subtitle = "Theo dõi biên nhận và công nợ học viên.",
        Breadcrumbs = Breadcrumbs("Học phí"),
        PrimaryActionText = "Thu học phí",
        PrimaryActionUrl = "/Admin/Receipts/Create",
        ExportLabel = "In biên nhận",
        SummaryCards =
        [
            new SummaryCardViewModel { Title = "Đã thu", Value = AppUi.Currency(DataService.GetReceipts().Sum(x => x.Amount)), Description = "Tổng tiền đã ghi nhận", Icon = "bi-receipt-cutoff", AccentClass = "success" },
            new SummaryCardViewModel { Title = "Công nợ", Value = AppUi.Currency(DataService.GetDebts().Sum(x => x.RemainingAmount)), Description = "Tổng số tiền cần thu thêm", Icon = "bi-wallet2", AccentClass = "danger" }
        ],
        Table = new TableViewModel
        {
            Columns = [new() { Header = "Biên nhận" }, new() { Header = "Lớp" }, new() { Header = "Phương thức" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
            Rows = DataService.GetReceipts().Select(x => new TableRowViewModel
            {
                Id = x.Id.ToString(),
                Cells =
                [
                    new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.ReceiptCode} • {AppUi.Currency(x.Amount)}</div>" },
                    new() { Html = x.ClassCode },
                    new() { Html = x.PaymentMethod },
                    new() { Html = AppUi.StatusBadge(x.Status) },
                    new() { Html = "" }
                ],
                Actions =
                [
                    new() { Label = "Chi tiết", Url = $"/Admin/Receipts/Details/{x.Id}", Icon = "bi-eye" },
                    new() { Label = "Sửa", Url = $"/Admin/Receipts/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                    new() { Label = "In", Url = "#", Icon = "bi-printer", CssClass = "btn btn-sm btn-outline-dark", RequiresConfirm = true, ConfirmMessage = "Bạn muốn tiếp tục in biên nhận học phí này?" }
                ]
            }).ToList()
        }
    });

    public IActionResult Create() => ManagementFormView(AdminPageHelpers.SimpleForm("Thu học phí", Breadcrumbs("Thu học phí", "Học phí", "/Admin/Receipts"), "/Admin/Receipts",
    [
        new() { Label = "Học viên", Name = "StudentName", Required = true },
        new() { Label = "Lớp học", Name = "ClassCode", Required = true },
        new() { Label = "Số tiền", Name = "Amount", Type = "number", Required = true },
        new() { Label = "Phương thức", Name = "PaymentMethod", Required = true }
    ]));

    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => ManagementDetailsView(AdminPageHelpers.SimpleDetails("Chi tiết biên nhận", Breadcrumbs("Chi tiết biên nhận", "Học phí", "/Admin/Receipts"), "/Admin/Receipts"));
}

public class SessionsController : AdminControllerBase
{
    public SessionsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(AdminPageHelpers.SimpleList("Buổi học", "/Admin/Sessions",
        DataService.GetSessions().Select(x => new TableRowViewModel
        {
            Id = x.Id.ToString(),
            Cells =
            [
                new() { Html = $"<strong>{x.ClassCode}</strong><div class='text-muted small'>{x.SessionDate:dd/MM/yyyy}</div>" },
                new() { Html = x.Topic },
                new() { Html = x.TimeSlot },
                new() { Html = AppUi.StatusBadge(x.Status) },
                new() { Html = "" }
            ],
            Actions = [new() { Label = "Chi tiết", Url = $"/Admin/Sessions/Details/{x.Id}", Icon = "bi-eye" }]
        }).ToList()));

    public IActionResult Create() => ManagementFormView(AdminPageHelpers.SimpleForm("Thêm buổi học", Breadcrumbs("Thêm buổi học", "Buổi học", "/Admin/Sessions"), "/Admin/Sessions",
    [
        new() { Label = "Lớp học", Name = "ClassCode", Required = true },
        new() { Label = "Ngày học", Name = "SessionDate", Type = "date", Required = true },
        new() { Label = "Khung giờ", Name = "TimeSlot", Required = true },
        new() { Label = "Chủ đề", Name = "Topic", ColClass = "col-12", Required = true }
    ]));

    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => ManagementDetailsView(AdminPageHelpers.SimpleDetails("Chi tiết buổi học", Breadcrumbs("Chi tiết buổi học", "Buổi học", "/Admin/Sessions"), "/Admin/Sessions"));
}

public class AttendanceController : AdminControllerBase
{
    public AttendanceController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(AdminPageHelpers.SimpleList("Điểm danh", "/Admin/Attendance",
        DataService.GetAttendanceRecords().Select(x => new TableRowViewModel
        {
            Id = x.Id.ToString(),
            Cells =
            [
                new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.ClassCode}</div>" },
                new() { Html = x.SessionTopic },
                new() { Html = x.AttendanceDate.ToString("dd/MM/yyyy") },
                new() { Html = AppUi.StatusBadge(x.Status) },
                new() { Html = "" }
            ],
            Actions = [new() { Label = "Chi tiết", Url = $"/Admin/Attendance/Details/{x.Id}", Icon = "bi-eye" }]
        }).ToList()));

    public IActionResult Create() => ManagementFormView(AdminPageHelpers.SimpleForm("Điểm danh theo buổi", Breadcrumbs("Điểm danh", "Điểm danh", "/Admin/Attendance"), "/Admin/Attendance",
    [
        new() { Label = "Lớp học", Name = "ClassCode", Required = true },
        new() { Label = "Buổi học", Name = "SessionTopic", ColClass = "col-12", Required = true },
        new() { Label = "Học viên", Name = "StudentName", Required = true },
        new()
        {
            Label = "Trạng thái",
            Name = "Status",
            Type = "select",
            Options =
            [
                new() { Label = "Có mặt", Value = "Có mặt", Selected = true },
                new() { Label = "Vắng", Value = "Vắng" },
                new() { Label = "Muộn", Value = "Muộn" }
            ]
        }
    ]));

    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => ManagementDetailsView(AdminPageHelpers.SimpleDetails("Chi tiết điểm danh", Breadcrumbs("Chi tiết điểm danh", "Điểm danh", "/Admin/Attendance"), "/Admin/Attendance"));
}

public class ExamsController : AdminControllerBase
{
    public ExamsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(AdminPageHelpers.SimpleList("Điểm số", "/Admin/Exams",
        DataService.GetExamResults().Select(x => new TableRowViewModel
        {
            Id = x.Id.ToString(),
            Cells =
            [
                new() { Html = $"<strong>{x.StudentName}</strong><div class='text-muted small'>{x.ClassCode}</div>" },
                new() { Html = x.ExamType },
                new() { Html = x.Score.ToString("0.0") },
                new() { Html = AppUi.StatusBadge(x.Result) },
                new() { Html = "" }
            ],
            Actions = [new() { Label = "Chi tiết", Url = $"/Admin/Exams/Details/{x.Id}", Icon = "bi-eye" }]
        }).ToList()));

    public IActionResult Create() => ManagementFormView(AdminPageHelpers.SimpleForm("Nhập điểm", Breadcrumbs("Nhập điểm", "Điểm số", "/Admin/Exams"), "/Admin/Exams",
    [
        new() { Label = "Lớp học", Name = "ClassCode", Required = true },
        new() { Label = "Học viên", Name = "StudentName", Required = true },
        new() { Label = "Loại bài kiểm tra", Name = "ExamType", Required = true },
        new() { Label = "Điểm số", Name = "Score", Type = "number", Required = true }
    ]));

    public IActionResult Edit(int id) => Create();
    public IActionResult Details(int id) => ManagementDetailsView(AdminPageHelpers.SimpleDetails("Chi tiết điểm số", Breadcrumbs("Chi tiết điểm số", "Điểm số", "/Admin/Exams"), "/Admin/Exams"));
}

public class ReportsController : AdminControllerBase
{
    public ReportsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        return DashboardView(new DashboardPageViewModel
        {
            Title = "Báo cáo - thống kê",
            Subtitle = "Theo dõi nhanh doanh thu, công nợ và quy mô lớp học.",
            Breadcrumbs = Breadcrumbs("Báo cáo"),
            RoleName = "Admin",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Doanh thu kỳ này", Value = AppUi.Currency(DataService.GetReceipts().Sum(x => x.Amount)), Description = "Tổng thu đã ghi nhận", Icon = "bi-bar-chart-line", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Công nợ tồn", Value = AppUi.Currency(DataService.GetDebts().Sum(x => x.RemainingAmount)), Description = "Cần giáo vụ theo dõi", Icon = "bi-wallet2", AccentClass = "danger" },
                new SummaryCardViewModel { Title = "Khóa học nổi bật", Value = DataService.GetCourses().OrderByDescending(x => x.StudentCount).First().Name, Description = "Khóa có sĩ số cao nhất", Icon = "bi-trophy", AccentClass = "success" }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Xuất PDF", Url = "#", Icon = "bi-file-earmark-pdf", CssClass = "btn btn-outline-danger" },
                new QuickActionViewModel { Label = "Xuất Excel", Url = "#", Icon = "bi-file-earmark-spreadsheet", CssClass = "btn btn-outline-success" }
            ],
            Charts =
            [
                new ChartCardViewModel { ChartId = "reportRevenue", Title = "Doanh thu theo tháng", Subtitle = "Tổng hợp từ biên nhận hiện có", ChartType = "bar", Labels = ["01", "02", "03", "04"], Values = [63, 71, 68, 82], Colors = ["#1d4ed8", "#0ea5e9", "#10b981", "#f59e0b"] },
                new ChartCardViewModel { ChartId = "reportCourse", Title = "Khóa học có sĩ số cao", Subtitle = "So sánh quy mô lớp và mức quan tâm", ChartType = "doughnut", Labels = DataService.GetCourses().Select(x => x.Name).ToList(), Values = DataService.GetCourses().Select(x => (decimal)x.StudentCount).ToList(), Colors = ["#0f172a", "#1d4ed8", "#38bdf8", "#f97316"] }
            ],
            Panels =
            [
                new DashboardPanelViewModel
                {
                    Title = "Học viên còn nợ học phí",
                    Subtitle = "Danh sách ưu tiên cần liên hệ và xử lý",
                    Items = DataService.GetDebts().Select(x => new PanelItemViewModel
                    {
                        Title = x.StudentName,
                        Meta = $"{x.CourseName} • Hạn {x.DueDate:dd/MM/yyyy}",
                        Value = AppUi.Currency(x.RemainingAmount),
                        BadgeText = x.Status,
                        BadgeClass = AppUi.StatusBadgeClass(x.Status)
                    }).ToList()
                }
            ]
        });
    }
}

internal static class AdminPageHelpers
{
    internal static ManagementListPageViewModel SimpleList(string title, string baseUrl, List<TableRowViewModel> rows) => new()
    {
        Title = title,
        Subtitle = $"Quản lý {title.ToLowerInvariant()} trên cùng giao diện điều hành trung tâm.",
        Breadcrumbs = AdminControllerBase.Breadcrumbs(title),
        PrimaryActionText = $"Thêm {title.ToLowerInvariant()}",
        PrimaryActionUrl = $"{baseUrl}/Create",
        Table = new TableViewModel { Columns = [new() { Header = title }, new() { Header = "Thông tin" }, new() { Header = "Mốc thời gian" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }], Rows = rows }
    };

    internal static ManagementFormPageViewModel SimpleForm(string title, List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, List<FormFieldViewModel> fields) => new()
    {
        Title = title,
        Subtitle = "Biểu mẫu nghiệp vụ dành cho quản trị viên.",
        Breadcrumbs = breadcrumbs,
        FormTitle = title,
        FormDescription = "Điền thông tin để thao tác trên từng phân hệ của trung tâm.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng kiểm tra kỹ dữ liệu trước khi xác nhận.",
        Sections = [new FormSectionViewModel { Title = "Thông tin", Fields = fields }]
    };

    internal static ManagementDetailsPageViewModel SimpleDetails(string title, List<BreadcrumbItemViewModel> breadcrumbs, string backUrl) => new()
    {
        Title = title,
        Subtitle = "Theo dõi nhanh thông tin và hành động liên quan trong khu quản trị.",
        Breadcrumbs = breadcrumbs,
        Sections =
        [
            new DetailSectionViewModel
            {
                Title = "Ghi chú quản trị",
                Items =
                [
                    new() { Label = "Tình trạng", Value = "Sẵn sàng theo dõi", IsBadge = true, BadgeClass = "bg-success-subtle text-success-emphasis" },
                    new() { Label = "Lưu ý", Value = "Chi tiết được hiển thị theo dữ liệu hiện có của hệ thống." }
                ]
            }
        ],
        Actions = [new QuickActionViewModel { Label = "Quay lại", Url = backUrl, Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }]
    };
}
