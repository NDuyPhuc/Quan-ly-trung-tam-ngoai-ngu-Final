using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class CoursesController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public CoursesController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var courses = DataService.GetCourses();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý khóa học",
            Subtitle = "Quản lý chương trình đào tạo, thời lượng và học phí.",
            Breadcrumbs = Breadcrumbs("Khóa học"),
            PrimaryActionText = "Tạo khóa học",
            PrimaryActionUrl = "/Admin/Courses/Create",
            SearchPlaceholder = "Tìm theo mã, tên khóa học...",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Khóa học" }, new() { Header = "Thời lượng" }, new() { Header = "Học phí" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = courses.Select(course => new TableRowViewModel
                {
                    Id = course.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{course.Name}</strong><div class='text-muted small'>{course.Code}</div>" },
                        new() { Html = course.Duration },
                        new() { Html = $"<strong>{AppUi.Currency(course.TuitionFee)}</strong>" },
                        new() { Html = AppUi.StatusBadge(course.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Courses/Details/{course.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Courses/Edit/{course.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Courses/Delete/{course.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa khóa học này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildCourseForm("Tạo khóa học", "/Admin/Courses/Create", new CourseInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CourseInput input)
    {
        var result = _managementService.SaveCourse(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildCourseForm("Tạo khóa học", "/Admin/Courses/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetCourse(id);
        if (input is null)
        {
            SetToast("Không tìm thấy khóa học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildCourseForm("Cập nhật khóa học", $"/Admin/Courses/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, CourseInput input)
    {
        var result = _managementService.SaveCourse(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildCourseForm("Cập nhật khóa học", $"/Admin/Courses/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteCourse(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var course = DataService.GetCourses().FirstOrDefault(x => x.Id == id);
        if (course is null)
        {
            SetToast("Không tìm thấy khóa học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var classes = DataService.GetClasses().Where(x => x.CourseName == course.Name).ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = course.Name,
            Subtitle = course.ShortDescription,
            Breadcrumbs = Breadcrumbs("Chi tiết khóa học", "Khóa học", "/Admin/Courses"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Học viên", Value = course.StudentCount.ToString(), Description = "Tổng số học viên đang ghi danh", Icon = "bi-people" },
                new SummaryCardViewModel { Title = "Học phí", Value = AppUi.Currency(course.TuitionFee), Description = "Mức học phí hiện tại", Icon = "bi-cash-stack", AccentClass = "warning" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin khóa học",
                    Items =
                    [
                        new() { Label = "Mã khóa học", Value = course.Code },
                        new() { Label = "Trình độ", Value = course.Level },
                        new() { Label = "Thời lượng", Value = course.Duration },
                        new() { Label = "Trạng thái", Value = course.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(course.Status) },
                        new() { Label = "Khai giảng gần nhất", Value = course.NextOpening }
                    ]
                }
            ],
            Timeline = classes.Select(item => new TimelineItemViewModel
            {
                Title = item.Code,
                Meta = item.Schedule,
                Description = $"{item.TeacherName} • {item.Enrolled}/{item.Capacity} học viên",
                AccentClass = item.Status == "Đang hoạt động" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa khóa học", Url = $"/Admin/Courses/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Courses", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildCourseForm(string title, string actionUrl, CourseInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật khóa học trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Khóa học", "/Admin/Courses"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Courses trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Courses",
            SubmitLabel = "Lưu khóa học",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin khóa học",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Mã khóa học", Name = "CourseCode", Value = input.CourseCode, Required = true },
                        new FormFieldViewModel { Label = "Tên khóa học", Name = "CourseName", Value = input.CourseName, Required = true },
                        new FormFieldViewModel { Label = "Thời lượng (giờ)", Name = "DurationHours", Value = input.DurationHours.ToString(), Type = "number", Required = true },
                        new FormFieldViewModel { Label = "Học phí", Name = "TuitionFee", Value = input.TuitionFee.ToString("0"), Type = "number", Required = true },
                        new FormFieldViewModel { Label = "Mô tả", Name = "Description", Value = input.Description, Type = "textarea", ColClass = "col-12" },
                        new FormFieldViewModel
                        {
                            Label = "Trạng thái",
                            Name = "IsActive",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Đang hoạt động", Value = "true", Selected = input.IsActive },
                                new SelectOptionViewModel { Label = "Tạm dừng", Value = "false", Selected = !input.IsActive }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}

public class ClassesController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ClassesController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var classes = DataService.GetClasses();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý lớp học",
            Subtitle = "Quản lý lịch học, sĩ số và giáo viên phụ trách.",
            Breadcrumbs = Breadcrumbs("Lớp học"),
            PrimaryActionText = "Mở lớp mới",
            PrimaryActionUrl = "/Admin/Classes/Create",
            SearchPlaceholder = "Tìm theo mã lớp, khóa học hoặc giáo viên...",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Lớp học" }, new() { Header = "Giáo viên" }, new() { Header = "Sĩ số" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = classes.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.Code}</strong><div class='text-muted small'>{item.CourseName} • {item.Schedule}</div>" },
                        new() { Html = item.TeacherName },
                        new() { Html = $"{item.Enrolled}/{item.Capacity}" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Classes/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Classes/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Classes/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa lớp học này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildClassForm("Mở lớp học", "/Admin/Classes/Create", new ClassInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ClassInput input)
    {
        var result = _managementService.SaveClass(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildClassForm("Mở lớp học", "/Admin/Classes/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetClass(id);
        if (input is null)
        {
            SetToast("Không tìm thấy lớp học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildClassForm("Cập nhật lớp học", $"/Admin/Classes/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ClassInput input)
    {
        var result = _managementService.SaveClass(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildClassForm("Cập nhật lớp học", $"/Admin/Classes/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteClass(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetClasses().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy lớp học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var enrollments = DataService.GetEnrollments().Where(x => x.ClassCode == item.Code).ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = item.Code,
            Subtitle = $"{item.CourseName} • {item.Schedule}",
            Breadcrumbs = Breadcrumbs("Chi tiết lớp học", "Lớp học", "/Admin/Classes"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Sĩ số", Value = $"{item.Enrolled}/{item.Capacity}", Description = "Số học viên hiện tại", Icon = "bi-people-fill" },
                new SummaryCardViewModel { Title = "Trạng thái", Value = item.Status, Description = "Theo dõi vòng đời lớp học", Icon = "bi-activity", AccentClass = "info" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Vận hành lớp học",
                    Items =
                    [
                        new() { Label = "Khóa học", Value = item.CourseName },
                        new() { Label = "Giáo viên", Value = item.TeacherName },
                        new() { Label = "Ngày bắt đầu", Value = item.StartDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Ngày kết thúc", Value = item.EndDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Timeline = enrollments.Select(x => new TimelineItemViewModel
            {
                Title = x.StudentName,
                Meta = x.EnrolledOn.ToString("dd/MM/yyyy"),
                Description = $"{x.Status} • {x.PaymentStatus}",
                AccentClass = x.PaymentStatus == "Đã thanh toán" ? "success" : "warning"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa lớp học", Url = $"/Admin/Classes/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Classes", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildClassForm(string title, string actionUrl, ClassInput input, string? errorMessage = null)
    {
        var courseOptions = DataService.GetCourses()
            .Select(course => new SelectOptionViewModel
            {
                Label = $"{course.Code} - {course.Name}",
                Value = course.Code,
                Selected = string.Equals(course.Code, input.CourseCode, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        var teacherOptions = new List<SelectOptionViewModel>
        {
            new() { Label = "Chưa phân công", Value = "", Selected = string.IsNullOrWhiteSpace(input.TeacherCode) }
        };
        teacherOptions.AddRange(DataService.GetTeachers().Select(teacher => new SelectOptionViewModel
        {
            Label = $"{teacher.Code} - {teacher.FullName}",
            Value = teacher.Code,
            Selected = string.Equals(teacher.Code, input.TeacherCode, StringComparison.OrdinalIgnoreCase)
        }));

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật lớp học trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Lớp học", "/Admin/Classes"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Classes trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Classes",
            SubmitLabel = "Lưu lớp học",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin lớp học",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Mã lớp", Name = "ClassCode", Value = input.ClassCode, Required = true },
                        new FormFieldViewModel { Label = "Tên lớp", Name = "ClassName", Value = input.ClassName, Required = true },
                        new FormFieldViewModel { Label = "Ngày bắt đầu", Name = "StartDate", Value = input.StartDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel { Label = "Ngày kết thúc", Name = "EndDate", Value = input.EndDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel { Label = "Lịch học", Name = "ScheduleText", Value = input.ScheduleText, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Sĩ số tối đa", Name = "Capacity", Value = input.Capacity.ToString(), Type = "number", Required = true },
                        new FormFieldViewModel { Label = "Khóa học", Name = "CourseCode", Type = "select", Required = true, Options = courseOptions },
                        new FormFieldViewModel { Label = "Giáo viên phụ trách", Name = "TeacherCode", Type = "select", Options = teacherOptions },
                        new FormFieldViewModel
                        {
                            Label = "Trạng thái",
                            Name = "IsActive",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Đang hoạt động", Value = "true", Selected = input.IsActive },
                                new SelectOptionViewModel { Label = "Tạm dừng", Value = "false", Selected = !input.IsActive }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
