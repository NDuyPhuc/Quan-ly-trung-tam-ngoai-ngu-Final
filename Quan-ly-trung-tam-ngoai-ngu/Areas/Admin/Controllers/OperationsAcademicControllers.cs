using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class AttendanceController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public AttendanceController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Điểm danh",
            Subtitle = "Theo dõi chuyên cần theo từng buổi học.",
            Breadcrumbs = Breadcrumbs("Điểm danh"),
            PrimaryActionText = "Điểm danh theo buổi",
            PrimaryActionUrl = "/Admin/Attendance/Create",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Buổi học" }, new() { Header = "Ngày" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = DataService.GetAttendanceRecords().Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.ClassCode}</div>" },
                        new() { Html = item.SessionTopic },
                        new() { Html = item.AttendanceDate.ToString("dd/MM/yyyy") },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Attendance/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Attendance/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Attendance/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa dòng điểm danh này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildAttendanceForm("Điểm danh theo buổi", "/Admin/Attendance/Create", new AttendanceInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AttendanceInput input)
    {
        var result = _managementService.SaveAttendance(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildAttendanceForm("Điểm danh theo buổi", "/Admin/Attendance/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetAttendance(id);
        if (input is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildAttendanceForm("Cập nhật điểm danh", $"/Admin/Attendance/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, AttendanceInput input)
    {
        var result = _managementService.SaveAttendance(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildAttendanceForm("Cập nhật điểm danh", $"/Admin/Attendance/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteAttendance(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetAttendanceRecords().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Điểm danh {item.StudentName}",
            Subtitle = item.SessionTopic,
            Breadcrumbs = Breadcrumbs("Chi tiết điểm danh", "Điểm danh", "/Admin/Attendance"),
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin điểm danh",
                    Items =
                    [
                        new() { Label = "Học viên", Value = item.StudentName },
                        new() { Label = "Lớp học", Value = item.ClassCode },
                        new() { Label = "Ngày học", Value = item.AttendanceDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) },
                        new() { Label = "Ghi chú", Value = item.Note }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa điểm danh", Url = $"/Admin/Attendance/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Attendance", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildAttendanceForm(string title, string actionUrl, AttendanceInput input, string? errorMessage = null)
    {
        var enrollmentOptions = DataService.GetEnrollments().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.StudentName} - {item.ClassCode}",
            Value = item.Id.ToString(),
            Selected = item.Id == input.EnrollmentId
        }).ToList();

        var sessionOptions = DataService.GetSessions().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.ClassCode} - {item.SessionDate:dd/MM/yyyy} - {item.Topic}",
            Value = item.Id.ToString(),
            Selected = item.Id == input.ClassSessionId
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật dữ liệu điểm danh trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Điểm danh", "/Admin/Attendance"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Attendances trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Attendance",
            SubmitLabel = "Lưu điểm danh",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin điểm danh",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Ghi danh", Name = "EnrollmentId", Type = "select", Required = true, Options = enrollmentOptions },
                        new FormFieldViewModel { Label = "Buổi học", Name = "ClassSessionId", Type = "select", Required = true, Options = sessionOptions },
                        new FormFieldViewModel
                        {
                            Label = "Trạng thái",
                            Name = "AttendanceStatus",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Có mặt", Value = "Present", Selected = input.AttendanceStatus == "Present" || string.IsNullOrWhiteSpace(input.AttendanceStatus) },
                                new SelectOptionViewModel { Label = "Vắng", Value = "Absent", Selected = input.AttendanceStatus == "Absent" },
                                new SelectOptionViewModel { Label = "Muộn", Value = "Late", Selected = input.AttendanceStatus == "Late" }
                            ]
                        },
                        new FormFieldViewModel { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class ExamResultsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ExamResultsController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Điểm số",
            Subtitle = "Quản lý đầu điểm và kết quả học tập.",
            Breadcrumbs = Breadcrumbs("Điểm số"),
            PrimaryActionText = "Nhập điểm",
            PrimaryActionUrl = "/Admin/ExamResults/Create",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Bài kiểm tra" }, new() { Header = "Điểm" }, new() { Header = "Kết quả" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = DataService.GetExamResults().Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.ClassCode}</div>" },
                        new() { Html = item.ExamType },
                        new() { Html = item.Score.ToString("0.0") },
                        new() { Html = AppUi.StatusBadge(item.Result) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/ExamResults/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/ExamResults/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/ExamResults/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa đầu điểm này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildExamForm("Nhập điểm", "/Admin/ExamResults/Create", new ExamResultInput { ExamDate = DateTime.Today, MaxScore = 10 }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ExamResultInput input)
    {
        var result = _managementService.SaveExamResult(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Nhập điểm", "/Admin/ExamResults/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetExamResult(id);
        if (input is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildExamForm("Cập nhật điểm", $"/Admin/ExamResults/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ExamResultInput input)
    {
        var result = _managementService.SaveExamResult(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Cập nhật điểm", $"/Admin/ExamResults/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteExamResult(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetExamResults().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Điểm số {item.StudentName}",
            Subtitle = item.ExamType,
            Breadcrumbs = Breadcrumbs("Chi tiết điểm số", "Điểm số", "/Admin/ExamResults"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Điểm số", Value = item.Score.ToString("0.0"), Description = "Kết quả của học viên", Icon = "bi-award", AccentClass = item.Result == "Đạt" ? "success" : "warning" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin bài kiểm tra",
                    Items =
                    [
                        new() { Label = "Học viên", Value = item.StudentName },
                        new() { Label = "Lớp học", Value = item.ClassCode },
                        new() { Label = "Bài kiểm tra", Value = item.ExamType },
                        new() { Label = "Điểm trung bình", Value = item.AverageScore.ToString("0.0") },
                        new() { Label = "Kết quả", Value = item.Result, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Result) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa điểm", Url = $"/Admin/ExamResults/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/ExamResults", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildExamForm(string title, string actionUrl, ExamResultInput input, string? errorMessage = null)
    {
        var enrollmentOptions = DataService.GetEnrollments().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.StudentName} - {item.ClassCode}",
            Value = item.Id.ToString(),
            Selected = item.Id == input.EnrollmentId
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật dữ liệu điểm số trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Điểm số", "/Admin/ExamResults"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Exams và ExamResults trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/ExamResults",
            SubmitLabel = "Lưu điểm số",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin bài kiểm tra",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Ghi danh", Name = "EnrollmentId", Type = "select", Required = true, Options = enrollmentOptions },
                        new FormFieldViewModel { Label = "Tên bài kiểm tra", Name = "ExamName", Value = input.ExamName, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Ngày kiểm tra", Name = "ExamDate", Value = input.ExamDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel { Label = "Điểm tối đa", Name = "MaxScore", Value = input.MaxScore.ToString("0.##"), Type = "number", Required = true },
                        new FormFieldViewModel { Label = "Điểm số", Name = "Score", Value = input.Score.ToString("0.##"), Type = "number", Required = true },
                        new FormFieldViewModel
                        {
                            Label = "Loại bài kiểm tra",
                            Name = "ExamType",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Giữa kỳ", Value = "Midterm", Selected = input.ExamType == "Midterm" },
                                new SelectOptionViewModel { Label = "Cuối kỳ", Value = "Final", Selected = input.ExamType == "Final" },
                                new SelectOptionViewModel { Label = "Nói", Value = "Speaking", Selected = input.ExamType == "Speaking" },
                                new SelectOptionViewModel { Label = "Kiểm tra", Value = "Test", Selected = input.ExamType == "Test" || string.IsNullOrWhiteSpace(input.ExamType) }
                            ]
                        },
                        new FormFieldViewModel
                        {
                            Label = "Kết quả",
                            Name = "ResultStatus",
                            Type = "select",
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Tự động tính", Value = "", Selected = string.IsNullOrWhiteSpace(input.ResultStatus) },
                                new SelectOptionViewModel { Label = "Đạt", Value = "Pass", Selected = input.ResultStatus == "Pass" },
                                new SelectOptionViewModel { Label = "Chưa đạt", Value = "Fail", Selected = input.ResultStatus == "Fail" }
                            ]
                        },
                        new FormFieldViewModel { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class ReportsController : AdminControllerBase
{
    public ReportsController(ILanguageCenterReadService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var receipts = DataService.GetReceipts();
        var debts = DataService.GetDebts();
        var courses = DataService.GetCourses().OrderByDescending(x => x.StudentCount).ToList();
        var monthlyRevenue = receipts
            .GroupBy(x => x.PaidOn.ToString("MM/yyyy"))
            .OrderBy(x => x.Min(item => item.PaidOn))
            .TakeLast(6)
            .ToList();

        return DashboardView(new DashboardPageViewModel
        {
            Title = "Báo cáo - thống kê",
            Subtitle = "Theo dõi nhanh doanh thu, công nợ và quy mô lớp học.",
            Breadcrumbs = Breadcrumbs("Báo cáo"),
            RoleName = "Admin",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Doanh thu", Value = AppUi.Currency(receipts.Sum(x => x.Amount)), Description = "Tổng thu đã ghi nhận", Icon = "bi-bar-chart-line", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Công nợ tồn", Value = AppUi.Currency(debts.Sum(x => x.RemainingAmount)), Description = "Cần giáo vụ theo dõi", Icon = "bi-wallet2", AccentClass = "danger" },
                new SummaryCardViewModel { Title = "Khóa học nổi bật", Value = courses.FirstOrDefault()?.Name ?? "Chưa có", Description = "Khóa học có sĩ số cao nhất", Icon = "bi-trophy", AccentClass = "success" }
            ],
            Charts =
            [
                new ChartCardViewModel
                {
                    ChartId = "reportRevenue",
                    Title = "Doanh thu theo tháng",
                    Subtitle = "Tổng hợp từ biên nhận hiện có",
                    ChartType = "bar",
                    Labels = monthlyRevenue.Select(x => x.Key).ToList(),
                    Values = monthlyRevenue.Select(x => x.Sum(item => item.Amount)).ToList(),
                    Colors = ["#1d4ed8", "#0ea5e9", "#10b981", "#f59e0b", "#f97316", "#8b5cf6"]
                },
                new ChartCardViewModel
                {
                    ChartId = "reportCourse",
                    Title = "Khóa học có sĩ số cao",
                    Subtitle = "So sánh quy mô ghi danh theo khóa học",
                    ChartType = "doughnut",
                    Labels = courses.Select(x => x.Name).ToList(),
                    Values = courses.Select(x => (decimal)x.StudentCount).ToList(),
                    Colors = ["#0f172a", "#1d4ed8", "#38bdf8", "#f97316", "#10b981", "#ef4444"]
                }
            ],
            Panels =
            [
                new DashboardPanelViewModel
                {
                    Title = "Học viên còn nợ học phí",
                    Subtitle = "Danh sách ưu tiên cần liên hệ và xử lý",
                    Items = debts.Select(item => new PanelItemViewModel
                    {
                        Title = item.StudentName,
                        Meta = $"{item.CourseName} • Hạn {item.DueDate:dd/MM/yyyy}",
                        Value = AppUi.Currency(item.RemainingAmount),
                        BadgeText = item.Status,
                        BadgeClass = AppUi.StatusBadgeClass(item.Status)
                    }).ToList()
                }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Xem học phí", Url = "/Admin/Receipts", Icon = "bi-receipt" },
                new QuickActionViewModel { Label = "Xem ghi danh", Url = "/Admin/Enrollments", Icon = "bi-journal-check", CssClass = "btn btn-outline-primary" }
            ]
        });
    }
}
