using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class StudentsController : AdminControllerBase
{
    public StudentsController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        var students = DataService.GetStudents();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý học viên",
            Subtitle = "Theo dõi hồ sơ, trạng thái học tập và công nợ học phí.",
            Breadcrumbs = Breadcrumbs("Học viên"),
            PrimaryActionText = "Thêm học viên",
            PrimaryActionUrl = "/Admin/Students/Create",
            SearchPlaceholder = "Tìm theo mã học viên, họ tên hoặc lớp học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Đang học", Value = students.Count(x => x.Status == "Đang học").ToString(), Description = "Học viên đang theo học", Icon = "bi-person-lines-fill", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Còn nợ học phí", Value = students.Count(x => x.DebtAmount > 0).ToString(), Description = "Cần theo dõi và nhắc thu", Icon = "bi-credit-card", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Lớp" }, new() { Header = "Trạng thái" }, new() { Header = "Học phí còn nợ" }, new() { Header = "Thao tác", Width = "220px" }],
                Rows = students.Select(x => new TableRowViewModel
                {
                    Id = x.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{x.FullName}</strong><div class='text-muted small'>{x.Code} • {x.Phone}</div>" },
                        new() { Html = $"{x.CourseName}<div class='text-muted small'>{x.ClassCode}</div>" },
                        new() { Html = AppUi.StatusBadge(x.Status) },
                        new() { Html = x.DebtAmount > 0 ? $"<span class='text-danger fw-semibold'>{AppUi.Currency(x.DebtAmount)}</span>" : "<span class='text-success fw-semibold'>Đã đủ</span>" },
                        new() { Html = "" }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Students/Details/{x.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Students/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = "#", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger", RequiresConfirm = true, ConfirmMessage = "Bạn muốn tiếp tục thao tác xóa hồ sơ học viên này?" }
                    ]
                }).ToList()
            }
        });
    }

    public IActionResult Create() => ManagementFormView(BuildStudentForm(Breadcrumbs("Thêm học viên", "Học viên", "/Admin/Students"), "/Admin/Students"));
    public IActionResult Edit(int id) => ManagementFormView(BuildStudentForm(Breadcrumbs("Cập nhật học viên", "Học viên", "/Admin/Students"), "/Admin/Students", DataService.GetStudents().First(x => x.Id == id)));

    public IActionResult Details(int id)
    {
        var x = DataService.GetStudents().First(s => s.Id == id);
        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Học viên {x.FullName}",
            Subtitle = "Chi tiết hồ sơ, lớp học và tình trạng học phí.",
            Breadcrumbs = Breadcrumbs("Chi tiết học viên", "Học viên", "/Admin/Students"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Trạng thái", Value = x.Status, Description = "Tình trạng học tập hiện tại", Icon = "bi-person-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã đóng", Value = AppUi.Currency(x.PaidAmount), Description = "Tổng học phí đã thu", Icon = "bi-cash", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn nợ", Value = AppUi.Currency(x.DebtAmount), Description = "Khoản học phí cần theo dõi", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin cá nhân",
                    Items =
                    [
                        new() { Label = "Mã học viên", Value = x.Code },
                        new() { Label = "Email", Value = x.Email },
                        new() { Label = "Số điện thoại", Value = x.Phone },
                        new() { Label = "Trình độ", Value = x.Level }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Thông tin học tập",
                    Items =
                    [
                        new() { Label = "Khóa học", Value = x.CourseName },
                        new() { Label = "Lớp học", Value = x.ClassCode },
                        new() { Label = "Ngày vào học", Value = x.JoinedOn.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = x.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(x.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa hồ sơ", Url = $"/Admin/Students/Edit/{x.Id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Students", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildStudentForm(List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, Student? x = null) => new()
    {
        Title = x is null ? "Thêm học viên" : "Cập nhật học viên",
        Subtitle = "Quản lý hồ sơ học viên trên giao diện điều hành thống nhất.",
        Breadcrumbs = breadcrumbs,
        FormTitle = x is null ? "Thêm học viên mới" : $"Cập nhật {x.FullName}",
        FormDescription = "Cập nhật thông tin cá nhân, khóa học và học phí của học viên.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng kiểm tra kỹ thông tin trước khi xác nhận.",
        Sections =
        [
            new FormSectionViewModel
            {
                Title = "Thông tin học viên",
                Fields =
                [
                    new() { Label = "Mã học viên", Name = "Code", Value = x?.Code ?? "HV00X", Required = true },
                    new() { Label = "Họ và tên", Name = "FullName", Value = x?.FullName ?? "", Required = true },
                    new() { Label = "Email", Name = "Email", Value = x?.Email ?? "", Required = true, Type = "email" },
                    new() { Label = "Số điện thoại", Name = "Phone", Value = x?.Phone ?? "", Required = true },
                    new() { Label = "Khóa học", Name = "CourseName", Value = x?.CourseName ?? "", Required = true },
                    new() { Label = "Lớp học", Name = "ClassCode", Value = x?.ClassCode ?? "", Required = true },
                    new() { Label = "Trình độ", Name = "Level", Value = x?.Level ?? "", Required = true },
                    new()
                    {
                        Label = "Trạng thái",
                        Name = "Status",
                        Type = "select",
                        Options =
                        [
                            new() { Label = "Đang học", Value = "Đang học", Selected = x?.Status == "Đang học" || x is null },
                            new() { Label = "Bảo lưu", Value = "Bảo lưu", Selected = x?.Status == "Bảo lưu" },
                            new() { Label = "Hoàn thành", Value = "Hoàn thành", Selected = x?.Status == "Hoàn thành" }
                        ]
                    }
                ]
            }
        ]
    };
}

public class TeachersController : AdminControllerBase
{
    public TeachersController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index()
    {
        var items = DataService.GetTeachers();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Quản lý giáo viên",
            Subtitle = "Theo dõi hồ sơ giảng viên, chuyên môn và lớp phụ trách.",
            Breadcrumbs = Breadcrumbs("Giáo viên"),
            PrimaryActionText = "Thêm giáo viên",
            PrimaryActionUrl = "/Admin/Teachers/Create",
            SearchPlaceholder = "Tìm theo tên, mã giáo viên hoặc chuyên môn...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Đang giảng dạy", Value = items.Count(x => x.Status == "Đang giảng dạy").ToString(), Description = "Giảng viên đang phụ trách lớp", Icon = "bi-person-video3", AccentClass = "success" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Giáo viên" }, new() { Header = "Chuyên môn" }, new() { Header = "Lớp phụ trách" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
                Rows = items.Select(x => new TableRowViewModel
                {
                    Id = x.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{x.FullName}</strong><div class='text-muted small'>{x.Code} • {x.Email}</div>" },
                        new() { Html = $"{x.Specialty}<div class='text-muted small'>{x.Qualification}</div>" },
                        new() { Html = $"{x.AssignedClassCount} lớp" },
                        new() { Html = AppUi.StatusBadge(x.Status) },
                        new() { Html = "" }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Teachers/Details/{x.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Teachers/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                    ]
                }).ToList()
            }
        });
    }

    public IActionResult Create() => ManagementFormView(BuildTeacherForm(Breadcrumbs("Thêm giáo viên", "Giáo viên", "/Admin/Teachers"), "/Admin/Teachers"));
    public IActionResult Edit(int id) => ManagementFormView(BuildTeacherForm(Breadcrumbs("Cập nhật giáo viên", "Giáo viên", "/Admin/Teachers"), "/Admin/Teachers", DataService.GetTeachers().First(x => x.Id == id)));

    public IActionResult Details(int id)
    {
        var x = DataService.GetTeachers().First(t => t.Id == id);
        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Giáo viên {x.FullName}",
            Subtitle = "Hồ sơ giảng viên và tình trạng phân công lớp.",
            Breadcrumbs = Breadcrumbs("Chi tiết giáo viên", "Giáo viên", "/Admin/Teachers"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Lớp phụ trách", Value = x.AssignedClassCount.ToString(), Description = "Số lớp đang được phân công", Icon = "bi-easel", AccentClass = "primary" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin chung",
                    Items =
                    [
                        new() { Label = "Mã giáo viên", Value = x.Code },
                        new() { Label = "Chuyên môn", Value = x.Specialty },
                        new() { Label = "Bằng cấp", Value = x.Qualification },
                        new() { Label = "Trạng thái", Value = x.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(x.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Cập nhật hồ sơ", Url = $"/Admin/Teachers/Edit/{x.Id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Teachers", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildTeacherForm(List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, Models.Teacher? x = null) => new()
    {
        Title = x is null ? "Thêm giáo viên" : "Cập nhật giáo viên",
        Subtitle = "Quản lý hồ sơ giảng viên trong khu điều hành trung tâm.",
        Breadcrumbs = breadcrumbs,
        FormTitle = x is null ? "Thêm giáo viên mới" : $"Cập nhật {x.FullName}",
        FormDescription = "Cập nhật thông tin liên hệ, chuyên môn và năng lực giảng dạy.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng rà soát kỹ thông tin giảng viên trước khi xác nhận.",
        Sections =
        [
            new FormSectionViewModel
            {
                Title = "Thông tin giảng viên",
                Fields =
                [
                    new() { Label = "Mã giáo viên", Name = "Code", Value = x?.Code ?? "GV00X", Required = true },
                    new() { Label = "Họ và tên", Name = "FullName", Value = x?.FullName ?? "", Required = true },
                    new() { Label = "Email", Name = "Email", Value = x?.Email ?? "", Required = true, Type = "email" },
                    new() { Label = "Số điện thoại", Name = "Phone", Value = x?.Phone ?? "", Required = true },
                    new() { Label = "Chuyên môn", Name = "Specialty", Value = x?.Specialty ?? "", Required = true },
                    new() { Label = "Bằng cấp", Name = "Qualification", Value = x?.Qualification ?? "", Required = true }
                ]
            }
        ]
    };
}

public class CoursesController : AdminControllerBase
{
    public CoursesController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(new ManagementListPageViewModel
    {
        Title = "Quản lý khóa học",
        Subtitle = "Quản lý chương trình, học phí và mục tiêu đầu ra.",
        Breadcrumbs = Breadcrumbs("Khóa học"),
        PrimaryActionText = "Tạo khóa học",
        PrimaryActionUrl = "/Admin/Courses/Create",
        SearchPlaceholder = "Tìm theo tên khóa học hoặc trình độ...",
        Table = new TableViewModel
        {
            Columns = [new() { Header = "Khóa học" }, new() { Header = "Lịch học" }, new() { Header = "Học phí" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
            Rows = DataService.GetCourses().Select(x => new TableRowViewModel
            {
                Id = x.Id.ToString(),
                Cells =
                [
                    new() { Html = $"<strong>{x.Name}</strong><div class='text-muted small'>{x.Level} • {x.Duration}</div>" },
                    new() { Html = $"{x.ScheduleSummary}<div class='text-muted small'>Khai giảng {x.NextOpening}</div>" },
                    new() { Html = $"<strong>{AppUi.Currency(x.TuitionFee)}</strong>" },
                    new() { Html = AppUi.StatusBadge(x.Status) },
                    new() { Html = "" }
                ],
                Actions =
                [
                    new() { Label = "Chi tiết", Url = $"/Admin/Courses/Details/{x.Id}", Icon = "bi-eye" },
                    new() { Label = "Sửa", Url = $"/Admin/Courses/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                ]
            }).ToList()
        }
    });

    public IActionResult Create() => ManagementFormView(BuildCourseForm(Breadcrumbs("Thêm khóa học", "Khóa học", "/Admin/Courses"), "/Admin/Courses"));
    public IActionResult Edit(int id) => ManagementFormView(BuildCourseForm(Breadcrumbs("Cập nhật khóa học", "Khóa học", "/Admin/Courses"), "/Admin/Courses", DataService.GetCourses().First(x => x.Id == id)));

    public IActionResult Details(int id)
    {
        var x = DataService.GetCourses().First(c => c.Id == id);
        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = x.Name,
            Subtitle = x.ShortDescription,
            Breadcrumbs = Breadcrumbs("Chi tiết khóa học", "Khóa học", "/Admin/Courses"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Học viên", Value = x.StudentCount.ToString(), Description = "Số lượng học viên hiện có", Icon = "bi-people" },
                new SummaryCardViewModel { Title = "Học phí", Value = AppUi.Currency(x.TuitionFee), Description = "Mức học phí tiêu chuẩn", Icon = "bi-cash-stack", AccentClass = "warning" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Chi tiết khóa học",
                    Items =
                    [
                        new() { Label = "Trình độ", Value = x.Level },
                        new() { Label = "Thời lượng", Value = x.Duration },
                        new() { Label = "Lịch học", Value = x.ScheduleSummary },
                        new() { Label = "Khai giảng", Value = x.NextOpening },
                        new() { Label = "Trạng thái", Value = x.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(x.Status) },
                        new() { Label = "Đầu ra", Value = x.TargetOutput }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa khóa học", Url = $"/Admin/Courses/Edit/{x.Id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Courses", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildCourseForm(List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, Course? x = null) => new()
    {
        Title = x is null ? "Thêm khóa học" : "Cập nhật khóa học",
        Subtitle = "Thiết lập chương trình đào tạo và thông tin vận hành.",
        Breadcrumbs = breadcrumbs,
        FormTitle = x is null ? "Tạo khóa học" : $"Cập nhật {x.Name}",
        FormDescription = "Thiết lập mô tả, học phí, lịch học và mục tiêu đầu ra cho khóa học.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng kiểm tra kỹ thông tin khóa học trước khi xác nhận.",
        Sections =
        [
            new FormSectionViewModel
            {
                Title = "Thông tin khóa học",
                Fields =
                [
                    new() { Label = "Tên khóa học", Name = "Name", Value = x?.Name ?? "", Required = true },
                    new() { Label = "Trình độ", Name = "Level", Value = x?.Level ?? "", Required = true },
                    new() { Label = "Thời lượng", Name = "Duration", Value = x?.Duration ?? "", Required = true },
                    new() { Label = "Học phí", Name = "TuitionFee", Value = x?.TuitionFee.ToString() ?? "", Type = "number", Required = true },
                    new() { Label = "Lịch học", Name = "ScheduleSummary", Value = x?.ScheduleSummary ?? "", Required = true },
                    new() { Label = "Khai giảng", Name = "NextOpening", Value = x?.NextOpening ?? "", Required = true },
                    new() { Label = "Mô tả ngắn", Name = "ShortDescription", Value = x?.ShortDescription ?? "", ColClass = "col-12", Required = true },
                    new() { Label = "Mục tiêu đầu ra", Name = "TargetOutput", Value = x?.TargetOutput ?? "", ColClass = "col-12", Required = true }
                ]
            }
        ]
    };
}

public class ClassesController : AdminControllerBase
{
    public ClassesController(IMockDataService dataService) : base(dataService) { }

    public IActionResult Index() => ManagementListView(new ManagementListPageViewModel
    {
        Title = "Quản lý lớp học",
        Subtitle = "Quản lý lịch học, sĩ số và giáo viên phụ trách.",
        Breadcrumbs = Breadcrumbs("Lớp học"),
        PrimaryActionText = "Mở lớp mới",
        PrimaryActionUrl = "/Admin/Classes/Create",
        SearchPlaceholder = "Tìm theo mã lớp, khóa học hoặc giáo viên...",
        Table = new TableViewModel
        {
            Columns = [new() { Header = "Lớp học" }, new() { Header = "Giáo viên" }, new() { Header = "Sĩ số" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
            Rows = DataService.GetClasses().Select(x => new TableRowViewModel
            {
                Id = x.Id.ToString(),
                Cells =
                [
                    new() { Html = $"<strong>{x.Code}</strong><div class='text-muted small'>{x.CourseName} • {x.Schedule}</div>" },
                    new() { Html = $"{x.TeacherName}<div class='text-muted small'>{x.Room}</div>" },
                    new() { Html = $"{x.Enrolled}/{x.Capacity}" },
                    new() { Html = AppUi.StatusBadge(x.Status) },
                    new() { Html = "" }
                ],
                Actions =
                [
                    new() { Label = "Chi tiết", Url = $"/Admin/Classes/Details/{x.Id}", Icon = "bi-eye" },
                    new() { Label = "Sửa", Url = $"/Admin/Classes/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" }
                ]
            }).ToList()
        }
    });

    public IActionResult Create() => ManagementFormView(BuildClassForm(Breadcrumbs("Thêm lớp học", "Lớp học", "/Admin/Classes"), "/Admin/Classes"));
    public IActionResult Edit(int id) => ManagementFormView(BuildClassForm(Breadcrumbs("Cập nhật lớp học", "Lớp học", "/Admin/Classes"), "/Admin/Classes", DataService.GetClasses().First(x => x.Id == id)));

    public IActionResult Details(int id)
    {
        var x = DataService.GetClasses().First(c => c.Id == id);
        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = x.Code,
            Subtitle = $"{x.CourseName} • {x.Schedule}",
            Breadcrumbs = Breadcrumbs("Chi tiết lớp học", "Lớp học", "/Admin/Classes"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Sĩ số", Value = $"{x.Enrolled}/{x.Capacity}", Description = "Số học viên đã xếp lớp", Icon = "bi-people-fill" },
                new SummaryCardViewModel { Title = "Trạng thái", Value = x.Status, Description = "Theo dõi vòng đời lớp học", Icon = "bi-activity", AccentClass = "info" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Vận hành lớp học",
                    Items =
                    [
                        new() { Label = "Khóa học", Value = x.CourseName },
                        new() { Label = "Giáo viên", Value = x.TeacherName },
                        new() { Label = "Phòng học", Value = x.Room },
                        new() { Label = "Ngày bắt đầu", Value = x.StartDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Ngày kết thúc", Value = x.EndDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = x.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(x.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa lớp học", Url = $"/Admin/Classes/Edit/{x.Id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Classes", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildClassForm(List<BreadcrumbItemViewModel> breadcrumbs, string cancelUrl, CourseClass? x = null) => new()
    {
        Title = x is null ? "Thêm lớp học" : "Cập nhật lớp học",
        Subtitle = "Thiết lập phân lớp và lịch vận hành cho từng khóa học.",
        Breadcrumbs = breadcrumbs,
        FormTitle = x is null ? "Mở lớp học mới" : $"Cập nhật {x.Code}",
        FormDescription = "Chuẩn bị thông tin xếp lớp, giáo viên phụ trách và lịch học.",
        CancelUrl = cancelUrl,
        Notice = "Vui lòng kiểm tra kỹ sĩ số và lịch học trước khi xác nhận.",
        Sections =
        [
            new FormSectionViewModel
            {
                Title = "Thông tin lớp",
                Fields =
                [
                    new() { Label = "Mã lớp", Name = "Code", Value = x?.Code ?? "", Required = true },
                    new() { Label = "Khóa học", Name = "CourseName", Value = x?.CourseName ?? "", Required = true },
                    new() { Label = "Giáo viên", Name = "TeacherName", Value = x?.TeacherName ?? "", Required = true },
                    new() { Label = "Phòng học", Name = "Room", Value = x?.Room ?? "", Required = true },
                    new() { Label = "Lịch học", Name = "Schedule", Value = x?.Schedule ?? "", Required = true },
                    new() { Label = "Sĩ số tối đa", Name = "Capacity", Value = x?.Capacity.ToString() ?? "", Type = "number", Required = true }
                ]
            }
        ]
    };
}
