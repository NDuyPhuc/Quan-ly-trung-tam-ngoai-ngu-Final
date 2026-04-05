using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Staff.Controllers;

public class DashboardController : StaffControllerBase
{
    public DashboardController(ILanguageCenterReadService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        return DashboardView(new DashboardPageViewModel
        {
            Title = "Bảng điều khiển giáo vụ",
            Subtitle = "Theo dõi học viên, ghi danh, học phí và xếp lớp trên cùng một giao diện.",
            Breadcrumbs = Breadcrumbs("Tổng quan"),
            RoleName = "Giáo vụ",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Học viên", Value = DataService.GetStudents().Count.ToString(), Description = "Tổng hồ sơ hiện có", Icon = "bi-people", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Ghi danh", Value = DataService.GetEnrollments().Count.ToString(), Description = "Danh sách ghi danh đang theo dõi", Icon = "bi-journal-check", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Khoản thu", Value = AppUi.Currency(DataService.GetReceipts().Sum(x => x.Amount)), Description = "Tổng biên nhận hiện có", Icon = "bi-cash-stack", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Công nợ", Value = DataService.GetDebts().Count.ToString(), Description = "Học viên còn nợ học phí", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Panels =
            [
                new DashboardPanelViewModel
                {
                    Title = "Lớp sắp khai giảng",
                    Subtitle = "Các lớp cần chốt danh sách sớm",
                    Items = DataService.GetClasses()
                        .Where(x => x.Status == "Sắp khai giảng" || x.Status == "Mở đăng ký")
                        .Select(x => new PanelItemViewModel
                        {
                            Title = x.Code,
                            Meta = $"{x.CourseName} • {x.Schedule}",
                            Value = $"{x.Enrolled}/{x.Capacity} HV",
                            BadgeText = x.Status,
                            BadgeClass = AppUi.StatusBadgeClass(x.Status)
                        }).ToList()
                }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Thêm học viên", Url = "/Staff/Students/Create", Icon = "bi-person-plus" },
                new QuickActionViewModel { Label = "Ghi danh", Url = "/Staff/Enrollments/Create", Icon = "bi-journal-check", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Thu học phí", Url = "/Staff/Receipts/Create", Icon = "bi-receipt", CssClass = "btn btn-outline-dark" }
            ]
        });
    }
}

public class StudentsController : StaffControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public StudentsController(ILanguageCenterReadService dataService, ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var students = DataService.GetStudents();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Học viên",
            Subtitle = "Quản lý hồ sơ cá nhân, tình trạng học tập và công nợ học phí cho học viên.",
            Breadcrumbs = Breadcrumbs("Học viên"),
            PrimaryActionText = "Thêm học viên",
            PrimaryActionUrl = "/Staff/Students/Create",
            SearchPlaceholder = "Tìm theo mã, họ tên, email hoặc số điện thoại...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng học viên", Value = students.Count.ToString(), Description = "Số hồ sơ đang quản lý", Icon = "bi-people", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đang học", Value = students.Count(x => x.Status == "Đang học" || x.Status == "Đã xếp lớp").ToString(), Description = "Học viên đang có lộ trình học", Icon = "bi-person-check", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn công nợ", Value = students.Count(x => x.DebtAmount > 0).ToString(), Description = "Cần theo dõi thanh toán", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Học viên" },
                    new() { Header = "Lớp hiện tại" },
                    new() { Header = "Liên hệ" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "220px" }
                ],
                Rows = students.Select(student => new TableRowViewModel
                {
                    Id = student.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{student.FullName}</strong><div class='text-muted small'>{student.Code} • {student.Email}</div>" },
                        new() { Html = $"{student.CourseName}<div class='text-muted small'>{student.ClassCode}</div>" },
                        new() { Html = $"{student.Phone}<div class='text-muted small'>{(student.DebtAmount > 0 ? AppUi.Currency(student.DebtAmount) : "Đã đủ học phí")}</div>" },
                        new() { Html = AppUi.StatusBadge(student.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Staff/Students/Details/{student.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Staff/Students/Edit/{student.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildStudentForm("Thêm học viên", "/Staff/Students/Create", new StudentInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(StudentInput input)
    {
        var result = _managementService.SaveStudent(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildStudentForm("Thêm học viên", "/Staff/Students/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetStudent(id);
        if (input is null)
        {
            SetToast("Không tìm thấy học viên.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildStudentForm("Cập nhật học viên", $"/Staff/Students/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, StudentInput input)
    {
        var result = _managementService.SaveStudent(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildStudentForm("Cập nhật học viên", $"/Staff/Students/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var student = DataService.GetStudents().FirstOrDefault(x => x.Id == id);
        if (student is null)
        {
            SetToast("Không tìm thấy học viên.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var histories = DataService.GetEnrollments()
            .Where(x => string.Equals(x.StudentName, student.FullName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.EnrolledOn)
            .ToList();

        var debt = DataService.GetDebts()
            .FirstOrDefault(x =>
                string.Equals(x.StudentName, student.FullName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.CourseName, student.CourseName, StringComparison.OrdinalIgnoreCase));

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Học viên {student.FullName}",
            Subtitle = "Theo dõi hồ sơ cá nhân và lịch sử đăng ký học của học viên.",
            Breadcrumbs = Breadcrumbs("Chi tiết học viên", "Học viên", "/Staff/Students"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Trạng thái", Value = student.Status, Description = "Tình trạng học tập hiện tại", Icon = "bi-person-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã đóng", Value = AppUi.Currency(student.PaidAmount), Description = "Tổng học phí đã ghi nhận", Icon = "bi-cash-stack", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn nợ", Value = AppUi.Currency(student.DebtAmount), Description = debt is null ? "Không còn công nợ cần theo dõi" : $"Hạn theo dõi {debt.DueDate:dd/MM/yyyy}", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin cá nhân",
                    Items =
                    [
                        new() { Label = "Mã học viên", Value = student.Code },
                        new() { Label = "Email", Value = student.Email },
                        new() { Label = "Số điện thoại", Value = student.Phone },
                        new() { Label = "Trình độ", Value = student.Level }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Thông tin học tập",
                    Items =
                    [
                        new() { Label = "Khóa học hiện tại", Value = student.CourseName },
                        new() { Label = "Lớp hiện tại", Value = student.ClassCode },
                        new() { Label = "Ngày vào học", Value = student.JoinedOn.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = student.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(student.Status) }
                    ]
                }
            ],
            Timeline = histories.Select(item => new TimelineItemViewModel
            {
                Title = $"{item.CourseName} - {item.ClassCode}",
                Meta = item.EnrolledOn.ToString("dd/MM/yyyy"),
                Description = $"{item.Status} • {item.PaymentStatus} • Đã thu {AppUi.Currency(item.PaidAmount)}",
                AccentClass = item.PaymentStatus == "Đã thanh toán" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa hồ sơ", Url = $"/Staff/Students/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Staff/Students", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildStudentForm(string title, string actionUrl, StudentInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Biểu mẫu nghiệp vụ dành cho giáo vụ.",
            Breadcrumbs = Breadcrumbs(title, "Học viên", "/Staff/Students"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu trực tiếp xuống bảng Students.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Staff/Students",
            SubmitLabel = "Lưu học viên",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin học viên",
                    Fields =
                    [
                        new() { Label = "Mã học viên", Name = "StudentCode", Value = input.StudentCode, Required = true },
                        new() { Label = "Họ và tên", Name = "FullName", Value = input.FullName, Required = true },
                        new() { Label = "Ngày sinh", Name = "DateOfBirth", Value = input.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, Type = "date" },
                        new()
                        {
                            Label = "Giới tính",
                            Name = "Gender",
                            Type = "select",
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Không chọn", Value = "", Selected = string.IsNullOrWhiteSpace(input.Gender) },
                                new SelectOptionViewModel { Label = "Nam", Value = "Nam", Selected = input.Gender == "Nam" },
                                new SelectOptionViewModel { Label = "Nữ", Value = "Nữ", Selected = input.Gender == "Nữ" },
                                new SelectOptionViewModel { Label = "Khác", Value = "Khác", Selected = input.Gender == "Khác" }
                            ]
                        },
                        new() { Label = "Email", Name = "Email", Value = input.Email, Type = "email" },
                        new() { Label = "Số điện thoại", Name = "Phone", Value = input.Phone },
                        new() { Label = "Địa chỉ", Name = "Address", Value = input.Address, Type = "textarea", ColClass = "col-12" },
                        new()
                        {
                            Label = "Trạng thái",
                            Name = "IsActive",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Đang hoạt động", Value = "true", Selected = input.IsActive },
                                new SelectOptionViewModel { Label = "Tạm khóa", Value = "false", Selected = !input.IsActive }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}

public class EnrollmentsController : StaffControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public EnrollmentsController(ILanguageCenterReadService dataService, ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var enrollments = DataService.GetEnrollments();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Ghi danh",
            Subtitle = "Tạo ghi danh, theo dõi trạng thái học và kiểm soát tiến độ thanh toán học phí.",
            Breadcrumbs = Breadcrumbs("Ghi danh"),
            PrimaryActionText = "Tạo ghi danh",
            PrimaryActionUrl = "/Staff/Enrollments/Create",
            SearchPlaceholder = "Tìm theo học viên, mã ghi danh, lớp hoặc khóa học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng ghi danh", Value = enrollments.Count.ToString(), Description = "Bản ghi đang được theo dõi", Icon = "bi-journal-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đang học", Value = enrollments.Count(x => x.Status == "Đang học" || x.Status == "Đã xếp lớp").ToString(), Description = "Học viên còn đang theo lớp", Icon = "bi-person-workspace", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Chưa đủ học phí", Value = enrollments.Count(x => x.PaymentStatus != "Đã thanh toán").ToString(), Description = "Cần tiếp tục thu học phí", Icon = "bi-credit-card", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Học viên" },
                    new() { Header = "Khóa học" },
                    new() { Header = "Lớp học" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "220px" }
                ],
                Rows = enrollments.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.EnrollmentCode}</div>" },
                        new() { Html = $"{item.CourseName}<div class='text-muted small'>{AppUi.Currency(item.TotalFee)}</div>" },
                        new() { Html = $"{item.ClassCode}<div class='text-muted small'>{item.PaymentStatus}</div>" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Staff/Enrollments/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Staff/Enrollments/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildEnrollmentForm("Ghi danh học viên", "/Staff/Enrollments/Create", new EnrollmentInput { EnrollDate = DateTime.Today, Status = "DangHoc" }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EnrollmentInput input)
    {
        var result = _managementService.SaveEnrollment(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildEnrollmentForm("Ghi danh học viên", "/Staff/Enrollments/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetEnrollment(id);
        if (input is null)
        {
            SetToast("Không tìm thấy ghi danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildEnrollmentForm("Cập nhật ghi danh", $"/Staff/Enrollments/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, EnrollmentInput input)
    {
        var result = _managementService.SaveEnrollment(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildEnrollmentForm("Cập nhật ghi danh", $"/Staff/Enrollments/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var enrollment = DataService.GetEnrollments().FirstOrDefault(x => x.Id == id);
        if (enrollment is null)
        {
            SetToast("Không tìm thấy ghi danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var receipts = DataService.GetReceipts()
            .Where(x =>
                string.Equals(x.StudentName, enrollment.StudentName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClassCode, enrollment.ClassCode, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.PaidOn)
            .ToList();

        var remainingAmount = Math.Max(0, enrollment.TotalFee - enrollment.PaidAmount);

        var sections = new List<DetailSectionViewModel>
        {
            new()
            {
                Title = "Thông tin ghi danh",
                Items =
                [
                    new() { Label = "Mã ghi danh", Value = enrollment.EnrollmentCode },
                    new() { Label = "Học viên", Value = enrollment.StudentName },
                    new() { Label = "Khóa học", Value = enrollment.CourseName },
                    new() { Label = "Lớp học", Value = enrollment.ClassCode },
                    new() { Label = "Ngày ghi danh", Value = enrollment.EnrolledOn.ToString("dd/MM/yyyy") },
                    new() { Label = "Trạng thái học", Value = enrollment.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(enrollment.Status) }
                ]
            },
            new()
            {
                Title = "Thanh toán học phí",
                Items =
                [
                    new() { Label = "Tổng học phí", Value = AppUi.Currency(enrollment.TotalFee) },
                    new() { Label = "Đã thu", Value = AppUi.Currency(enrollment.PaidAmount) },
                    new() { Label = "Còn lại", Value = AppUi.Currency(remainingAmount) },
                    new() { Label = "Tình trạng thu", Value = enrollment.PaymentStatus, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(enrollment.PaymentStatus) }
                ]
            }
        };

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Ghi danh {enrollment.EnrollmentCode}",
            Subtitle = "Theo dõi tiến độ học tập và thanh toán của từng học viên theo lớp học.",
            Breadcrumbs = Breadcrumbs("Chi tiết ghi danh", "Ghi danh", "/Staff/Enrollments"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng học phí", Value = AppUi.Currency(enrollment.TotalFee), Description = "Giá trị ghi danh", Icon = "bi-cash-stack", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã thu", Value = AppUi.Currency(enrollment.PaidAmount), Description = "Đã ghi nhận từ biên nhận", Icon = "bi-receipt", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn lại", Value = AppUi.Currency(remainingAmount), Description = "Khoản cần tiếp tục theo dõi", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Sections = sections,
            Timeline = receipts.Select(item => new TimelineItemViewModel
            {
                Title = $"{item.ReceiptCode} - {AppUi.Currency(item.Amount)}",
                Meta = item.PaidOn.ToString("dd/MM/yyyy HH:mm"),
                Description = $"{item.PaymentMethod} • {item.Status}",
                AccentClass = item.Status == "Đã ghi nhận" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa ghi danh", Url = $"/Staff/Enrollments/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Staff/Enrollments", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildEnrollmentForm(string title, string actionUrl, EnrollmentInput input, string? errorMessage = null)
    {
        var studentOptions = DataService.GetStudents().Select(student => new SelectOptionViewModel
        {
            Label = $"{student.Code} - {student.FullName}",
            Value = student.Code,
            Selected = student.Code == input.StudentCode
        }).ToList();

        var classOptions = DataService.GetClasses().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.Code} - {item.CourseName}",
            Value = item.Code,
            Selected = item.Code == input.ClassCode
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Biểu mẫu ghi danh dành cho giáo vụ.",
            Breadcrumbs = Breadcrumbs(title, "Ghi danh", "/Staff/Enrollments"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Enrollments.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Staff/Enrollments",
            SubmitLabel = "Lưu ghi danh",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin ghi danh",
                    Fields =
                    [
                        new() { Label = "Học viên", Name = "StudentCode", Type = "select", Required = true, Options = studentOptions },
                        new() { Label = "Lớp học", Name = "ClassCode", Type = "select", Required = true, Options = classOptions },
                        new() { Label = "Ngày ghi danh", Name = "EnrollDate", Value = input.EnrollDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new()
                        {
                            Label = "Trạng thái",
                            Name = "Status",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Đang học", Value = "DangHoc", Selected = input.Status == "DangHoc" || string.IsNullOrWhiteSpace(input.Status) },
                                new SelectOptionViewModel { Label = "Bảo lưu", Value = "BaoLuu", Selected = input.Status == "BaoLuu" },
                                new SelectOptionViewModel { Label = "Hoàn thành", Value = "HoanThanh", Selected = input.Status == "HoanThanh" },
                                new SelectOptionViewModel { Label = "Hủy", Value = "Huy", Selected = input.Status == "Huy" }
                            ]
                        },
                        new() { Label = "Tổng học phí", Name = "TotalFee", Value = input.TotalFee.ToString("0"), Type = "number" },
                        new() { Label = "Giảm giá", Name = "DiscountAmount", Value = input.DiscountAmount.ToString("0"), Type = "number" },
                        new() { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class ClassesController : StaffControllerBase
{
    public ClassesController(ILanguageCenterReadService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        var classes = DataService.GetClasses();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Xếp lớp",
            Subtitle = "Theo dõi lịch học, sĩ số, giáo viên phụ trách và tình trạng mở lớp hiện tại.",
            Breadcrumbs = Breadcrumbs("Xếp lớp"),
            PrimaryActionText = "Xem lớp đang mở",
            PrimaryActionUrl = "/Admin/Classes",
            SearchPlaceholder = "Tìm theo mã lớp, khóa học hoặc giáo viên...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng lớp", Value = classes.Count.ToString(), Description = "Số lớp đang được quản lý", Icon = "bi-easel", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đang hoạt động", Value = classes.Count(x => x.Status == "Đang hoạt động").ToString(), Description = "Lớp đang vận hành", Icon = "bi-play-circle", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Sắp khai giảng", Value = classes.Count(x => x.Status == "Sắp khai giảng" || x.Status == "Mở đăng ký").ToString(), Description = "Cần chốt danh sách sớm", Icon = "bi-calendar-event", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Lớp học" },
                    new() { Header = "Giáo viên" },
                    new() { Header = "Sĩ số" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "140px" }
                ],
                Rows = classes.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.Code}</strong><div class='text-muted small'>{item.CourseName} • {item.Schedule}</div>" },
                        new() { Html = $"{item.TeacherName}<div class='text-muted small'>{item.Room}</div>" },
                        new() { Html = $"{item.Enrolled}/{item.Capacity}" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Staff/Classes/Details/{item.Id}", Icon = "bi-eye" }
                    ]
                }).ToList()
            }
        });
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetClasses().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy lớp học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var enrollments = DataService.GetEnrollments()
            .Where(x => string.Equals(x.ClassCode, item.Code, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.EnrolledOn)
            .ToList();

        var sessions = DataService.GetSessions()
            .Where(x => string.Equals(x.ClassCode, item.Code, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.SessionDate)
            .ToList();

        var timeline = sessions.Any()
            ? sessions.Select(session => new TimelineItemViewModel
            {
                Title = session.Topic,
                Meta = session.SessionDate.ToString("dd/MM/yyyy"),
                Description = $"{session.TimeSlot} • {session.Room} • {session.Status}",
                AccentClass = session.Status == "Hôm nay" ? "warning" : session.Status == "Hoàn tất" ? "success" : "primary"
            }).ToList()
            : enrollments.Select(enrollment => new TimelineItemViewModel
            {
                Title = enrollment.StudentName,
                Meta = enrollment.EnrolledOn.ToString("dd/MM/yyyy"),
                Description = $"{enrollment.Status} • {enrollment.PaymentStatus}",
                AccentClass = enrollment.PaymentStatus == "Đã thanh toán" ? "success" : "warning"
            }).ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Lớp {item.Code}",
            Subtitle = "Theo dõi lịch dạy, sĩ số và học viên của lớp học.",
            Breadcrumbs = Breadcrumbs("Chi tiết xếp lớp", "Xếp lớp", "/Staff/Classes"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Sĩ số", Value = $"{item.Enrolled}/{item.Capacity}", Description = "Số học viên hiện tại", Icon = "bi-people-fill", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Giáo viên", Value = item.TeacherName, Description = "Giáo viên phụ trách lớp", Icon = "bi-person-video3", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Buổi học", Value = sessions.Count.ToString(), Description = "Buổi học đã được tạo", Icon = "bi-calendar-week", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Vận hành lớp học",
                    Items =
                    [
                        new() { Label = "Khóa học", Value = item.CourseName },
                        new() { Label = "Lịch học", Value = item.Schedule },
                        new() { Label = "Phòng học", Value = item.Room },
                        new() { Label = "Ngày bắt đầu", Value = item.StartDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Ngày kết thúc", Value = item.EndDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Timeline = timeline,
            Actions =
            [
                new QuickActionViewModel { Label = "Mở quản lý lớp", Url = $"/Admin/Classes/Details/{id}", Icon = "bi-box-arrow-up-right" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Staff/Classes", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }
}

public class ReceiptsController : StaffControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ReceiptsController(ILanguageCenterReadService dataService, ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var receipts = DataService.GetReceipts();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Thu học phí",
            Subtitle = "Ghi nhận thanh toán, theo dõi biên nhận và rà soát khoản thu của học viên.",
            Breadcrumbs = Breadcrumbs("Thu học phí"),
            PrimaryActionText = "Tạo biên nhận",
            PrimaryActionUrl = "/Staff/Receipts/Create",
            SearchPlaceholder = "Tìm theo học viên, mã biên nhận hoặc lớp học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng biên nhận", Value = receipts.Count.ToString(), Description = "Lịch sử thanh toán hiện có", Icon = "bi-receipt", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã thu", Value = AppUi.Currency(receipts.Sum(x => x.Amount)), Description = "Tổng số tiền đã ghi nhận", Icon = "bi-cash-stack", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Công nợ còn lại", Value = AppUi.Currency(DataService.GetDebts().Sum(x => x.RemainingAmount)), Description = "Khoản cần tiếp tục theo dõi", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Biên nhận" },
                    new() { Header = "Lớp học" },
                    new() { Header = "Số tiền" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "220px" }
                ],
                Rows = receipts.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.ReceiptCode}</div>" },
                        new() { Html = $"{item.ClassCode}<div class='text-muted small'>{item.PaidOn:dd/MM/yyyy HH:mm}</div>" },
                        new() { Html = $"{AppUi.Currency(item.Amount)}<div class='text-muted small'>{item.PaymentMethod}</div>" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Staff/Receipts/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Staff/Receipts/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildReceiptForm("Thu học phí", "/Staff/Receipts/Create", new ReceiptInput { PaymentDate = DateTime.Now, PaymentMethod = "Cash" }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ReceiptInput input)
    {
        var result = _managementService.SaveReceipt(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildReceiptForm("Thu học phí", "/Staff/Receipts/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetReceipt(id);
        if (input is null)
        {
            SetToast("Không tìm thấy biên nhận.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildReceiptForm("Cập nhật biên nhận", $"/Staff/Receipts/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ReceiptInput input)
    {
        var result = _managementService.SaveReceipt(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildReceiptForm("Cập nhật biên nhận", $"/Staff/Receipts/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var receipt = DataService.GetReceipts().FirstOrDefault(x => x.Id == id);
        if (receipt is null)
        {
            SetToast("Không tìm thấy biên nhận.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var relatedReceipts = DataService.GetReceipts()
            .Where(x =>
                string.Equals(x.StudentName, receipt.StudentName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClassCode, receipt.ClassCode, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.PaidOn)
            .ToList();

        var enrollment = DataService.GetEnrollments()
            .FirstOrDefault(x =>
                string.Equals(x.StudentName, receipt.StudentName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClassCode, receipt.ClassCode, StringComparison.OrdinalIgnoreCase));

        var totalPaid = relatedReceipts.Sum(x => x.Amount);
        var remainingAmount = enrollment is null ? 0 : Math.Max(0, enrollment.TotalFee - totalPaid);

        var sections = new List<DetailSectionViewModel>
        {
            new()
            {
                Title = "Thông tin biên nhận",
                Items =
                [
                    new() { Label = "Mã biên nhận", Value = receipt.ReceiptCode },
                    new() { Label = "Học viên", Value = receipt.StudentName },
                    new() { Label = "Lớp học", Value = receipt.ClassCode },
                    new() { Label = "Ngày thu", Value = receipt.PaidOn.ToString("dd/MM/yyyy HH:mm") },
                    new() { Label = "Phương thức", Value = receipt.PaymentMethod },
                    new() { Label = "Trạng thái", Value = receipt.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(receipt.Status) }
                ]
            }
        };

        if (enrollment is not null)
        {
            sections.Add(new DetailSectionViewModel
            {
                Title = "Đối soát học phí",
                Items =
                [
                    new() { Label = "Mã ghi danh", Value = enrollment.EnrollmentCode },
                    new() { Label = "Tổng học phí", Value = AppUi.Currency(enrollment.TotalFee) },
                    new() { Label = "Đã thu lũy kế", Value = AppUi.Currency(totalPaid) },
                    new() { Label = "Còn lại", Value = AppUi.Currency(remainingAmount) }
                ]
            });
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Biên nhận {receipt.ReceiptCode}",
            Subtitle = "Kiểm tra giao dịch thanh toán và lịch sử biên nhận theo từng ghi danh.",
            Breadcrumbs = Breadcrumbs("Chi tiết biên nhận", "Thu học phí", "/Staff/Receipts"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Số tiền thu", Value = AppUi.Currency(receipt.Amount), Description = "Giá trị của biên nhận này", Icon = "bi-cash", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Đã thu lũy kế", Value = AppUi.Currency(totalPaid), Description = "Tổng biên nhận của cùng ghi danh", Icon = "bi-receipt-cutoff", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Còn lại", Value = AppUi.Currency(remainingAmount), Description = enrollment is null ? "Chưa đối chiếu được với ghi danh" : "Khoản học phí còn cần thu", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Sections = sections,
            Timeline = relatedReceipts.Select(item => new TimelineItemViewModel
            {
                Title = $"{item.ReceiptCode} - {AppUi.Currency(item.Amount)}",
                Meta = item.PaidOn.ToString("dd/MM/yyyy HH:mm"),
                Description = $"{item.PaymentMethod} • {item.Status}",
                AccentClass = item.Id == receipt.Id ? "primary" : "success"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa biên nhận", Url = $"/Staff/Receipts/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Staff/Receipts", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildReceiptForm(string title, string actionUrl, ReceiptInput input, string? errorMessage = null)
    {
        var enrollmentOptions = DataService.GetEnrollments().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.EnrollmentCode} - {item.StudentName} - {item.ClassCode}",
            Value = item.Id.ToString(),
            Selected = item.Id == input.EnrollmentId
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Biểu mẫu học phí dành cho giáo vụ.",
            Breadcrumbs = Breadcrumbs(title, "Thu học phí", "/Staff/Receipts"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Receipts.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Staff/Receipts",
            SubmitLabel = "Lưu biên nhận",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin biên nhận",
                    Fields =
                    [
                        new() { Label = "Ghi danh", Name = "EnrollmentId", Type = "select", Required = true, Options = enrollmentOptions },
                        new() { Label = "Ngày thu", Name = "PaymentDate", Value = input.PaymentDate.ToString("yyyy-MM-ddTHH:mm"), Type = "datetime-local", Required = true },
                        new() { Label = "Số tiền", Name = "Amount", Value = input.Amount.ToString("0"), Type = "number", Required = true },
                        new()
                        {
                            Label = "Phương thức",
                            Name = "PaymentMethod",
                            Type = "select",
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Tiền mặt", Value = "Cash", Selected = input.PaymentMethod == "Cash" },
                                new SelectOptionViewModel { Label = "Chuyển khoản", Value = "Transfer", Selected = input.PaymentMethod == "Transfer" },
                                new SelectOptionViewModel { Label = "Thẻ", Value = "Card", Selected = input.PaymentMethod == "Card" }
                            ]
                        },
                        new() { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}
