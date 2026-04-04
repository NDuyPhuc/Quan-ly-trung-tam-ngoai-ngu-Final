using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class StudentsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public StudentsController(
        IMockDataService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var students = DataService.GetStudents();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý học viên",
            Subtitle = "Quản lý hồ sơ học viên, trạng thái học tập và công nợ học phí.",
            Breadcrumbs = Breadcrumbs("Học viên"),
            PrimaryActionText = "Thêm học viên",
            PrimaryActionUrl = "/Admin/Students/Create",
            SearchPlaceholder = "Tìm theo mã, họ tên hoặc email...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Đang học", Value = students.Count(x => x.Status == "Đang học").ToString(), Description = "Học viên đang theo học", Icon = "bi-person-lines-fill", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Còn nợ học phí", Value = students.Count(x => x.DebtAmount > 0).ToString(), Description = "Cần theo dõi và nhắc thu", Icon = "bi-credit-card", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Lớp gần nhất" }, new() { Header = "Trạng thái" }, new() { Header = "Công nợ" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = students.Select(student => new TableRowViewModel
                {
                    Id = student.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{student.FullName}</strong><div class='text-muted small'>{student.Code} • {student.Email}</div>" },
                        new() { Html = $"{student.CourseName}<div class='text-muted small'>{student.ClassCode}</div>" },
                        new() { Html = AppUi.StatusBadge(student.Status) },
                        new() { Html = student.DebtAmount > 0 ? $"<span class='text-danger fw-semibold'>{AppUi.Currency(student.DebtAmount)}</span>" : "<span class='text-success fw-semibold'>Đã đủ</span>" },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Students/Details/{student.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Students/Edit/{student.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Students/Delete/{student.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa học viên này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildStudentForm("Thêm học viên", "/Admin/Students/Create", new StudentInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(StudentInput input)
    {
        var result = _managementService.SaveStudent(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildStudentForm("Thêm học viên", "/Admin/Students/Create", input, result.Message));
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

        return ManagementFormView(BuildStudentForm("Cập nhật học viên", $"/Admin/Students/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, StudentInput input)
    {
        var result = _managementService.SaveStudent(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildStudentForm("Cập nhật học viên", $"/Admin/Students/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteStudent(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
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

        var history = DataService.GetEnrollments()
            .Where(x => string.Equals(x.StudentName, student.FullName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.EnrolledOn)
            .Take(5)
            .ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Học viên {student.FullName}",
            Subtitle = "Chi tiết hồ sơ học viên và lịch sử ghi danh.",
            Breadcrumbs = Breadcrumbs("Chi tiết học viên", "Học viên", "/Admin/Students"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Trạng thái", Value = student.Status, Description = "Tình trạng học tập hiện tại", Icon = "bi-person-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã đóng", Value = AppUi.Currency(student.PaidAmount), Description = "Tổng học phí đã thu", Icon = "bi-cash", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn nợ", Value = AppUi.Currency(student.DebtAmount), Description = "Khoản học phí còn phải thu", Icon = "bi-wallet2", AccentClass = "danger" }
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
                        new() { Label = "Khóa học gần nhất", Value = student.CourseName },
                        new() { Label = "Lớp học gần nhất", Value = student.ClassCode },
                        new() { Label = "Ngày vào học", Value = student.JoinedOn.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = student.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(student.Status) }
                    ]
                }
            ],
            Timeline = history.Select(item => new TimelineItemViewModel
            {
                Title = item.CourseName,
                Meta = item.EnrolledOn.ToString("dd/MM/yyyy"),
                Description = $"{item.ClassCode} • {item.Status} • {AppUi.Currency(item.PaidAmount)}",
                AccentClass = item.PaymentStatus == "Đã thanh toán" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa hồ sơ", Url = $"/Admin/Students/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Students", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildStudentForm(string title, string actionUrl, StudentInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật hồ sơ học viên trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Học viên", "/Admin/Students"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Students trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Students",
            SubmitLabel = "Lưu học viên",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin học viên",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Mã học viên", Name = "StudentCode", Value = input.StudentCode, Required = true },
                        new FormFieldViewModel { Label = "Họ và tên", Name = "FullName", Value = input.FullName, Required = true },
                        new FormFieldViewModel { Label = "Ngày sinh", Name = "DateOfBirth", Value = input.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty, Type = "date" },
                        new FormFieldViewModel
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
                        new FormFieldViewModel { Label = "Email", Name = "Email", Value = input.Email, Type = "email" },
                        new FormFieldViewModel { Label = "Số điện thoại", Name = "Phone", Value = input.Phone },
                        new FormFieldViewModel { Label = "Địa chỉ", Name = "Address", Value = input.Address, ColClass = "col-12", Type = "textarea" },
                        new FormFieldViewModel
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

public class TeachersController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public TeachersController(
        IMockDataService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var teachers = DataService.GetTeachers();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý giáo viên",
            Subtitle = "Quản lý hồ sơ giáo viên và lớp đang phụ trách.",
            Breadcrumbs = Breadcrumbs("Giáo viên"),
            PrimaryActionText = "Thêm giáo viên",
            PrimaryActionUrl = "/Admin/Teachers/Create",
            SearchPlaceholder = "Tìm theo mã, họ tên hoặc email...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Đang giảng dạy", Value = teachers.Count(x => x.Status == "Đang giảng dạy").ToString(), Description = "Giáo viên đang phụ trách lớp", Icon = "bi-person-video3", AccentClass = "success" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Giáo viên" }, new() { Header = "Chuyên môn" }, new() { Header = "Lớp phụ trách" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = teachers.Select(teacher => new TableRowViewModel
                {
                    Id = teacher.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{teacher.FullName}</strong><div class='text-muted small'>{teacher.Code} • {teacher.Email}</div>" },
                        new() { Html = teacher.Specialty },
                        new() { Html = $"{teacher.AssignedClassCount} lớp" },
                        new() { Html = AppUi.StatusBadge(teacher.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Teachers/Details/{teacher.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Teachers/Edit/{teacher.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Teachers/Delete/{teacher.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa giáo viên này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildTeacherForm("Thêm giáo viên", "/Admin/Teachers/Create", new TeacherInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(TeacherInput input)
    {
        var result = _managementService.SaveTeacher(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildTeacherForm("Thêm giáo viên", "/Admin/Teachers/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetTeacher(id);
        if (input is null)
        {
            SetToast("Không tìm thấy giáo viên.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildTeacherForm("Cập nhật giáo viên", $"/Admin/Teachers/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, TeacherInput input)
    {
        var result = _managementService.SaveTeacher(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildTeacherForm("Cập nhật giáo viên", $"/Admin/Teachers/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteTeacher(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var teacher = DataService.GetTeachers().FirstOrDefault(x => x.Id == id);
        if (teacher is null)
        {
            SetToast("Không tìm thấy giáo viên.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var classes = DataService.GetClasses()
            .Where(x => string.Equals(x.TeacherName, teacher.FullName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Giáo viên {teacher.FullName}",
            Subtitle = "Chi tiết hồ sơ giáo viên và các lớp đang phụ trách.",
            Breadcrumbs = Breadcrumbs("Chi tiết giáo viên", "Giáo viên", "/Admin/Teachers"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Lớp phụ trách", Value = classes.Count.ToString(), Description = "Số lớp đang được phân công", Icon = "bi-easel", AccentClass = "primary" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin chung",
                    Items =
                    [
                        new() { Label = "Mã giáo viên", Value = teacher.Code },
                        new() { Label = "Email", Value = teacher.Email },
                        new() { Label = "Số điện thoại", Value = teacher.Phone },
                        new() { Label = "Chuyên môn", Value = teacher.Specialty },
                        new() { Label = "Trạng thái", Value = teacher.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(teacher.Status) }
                    ]
                }
            ],
            Timeline = classes.Select(item => new TimelineItemViewModel
            {
                Title = item.Code,
                Meta = item.Schedule,
                Description = $"{item.CourseName} • {item.Enrolled}/{item.Capacity} học viên",
                AccentClass = item.Status == "Đang hoạt động" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa hồ sơ", Url = $"/Admin/Teachers/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Teachers", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildTeacherForm(string title, string actionUrl, TeacherInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật hồ sơ giáo viên trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Giáo viên", "/Admin/Teachers"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Teachers trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Teachers",
            SubmitLabel = "Lưu giáo viên",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin giáo viên",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Mã giáo viên", Name = "TeacherCode", Value = input.TeacherCode, Required = true },
                        new FormFieldViewModel { Label = "Họ và tên", Name = "FullName", Value = input.FullName, Required = true },
                        new FormFieldViewModel { Label = "Email", Name = "Email", Value = input.Email, Type = "email" },
                        new FormFieldViewModel { Label = "Số điện thoại", Name = "Phone", Value = input.Phone },
                        new FormFieldViewModel { Label = "Chuyên môn", Name = "Specialization", Value = input.Specialization, ColClass = "col-12" },
                        new FormFieldViewModel
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
