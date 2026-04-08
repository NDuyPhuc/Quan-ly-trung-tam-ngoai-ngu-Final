using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Teacher.Controllers;

public class DashboardController : TeacherControllerBase
{
    public DashboardController(ILanguageCenterReadService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var classes = GetTeacherClasses();
        var classCodes = classes.Select(item => item.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sessions = DataService.GetSessions()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderBy(item => item.SessionDate)
            .ToList();
        var attendance = DataService.GetAttendanceRecords()
            .Where(item => classCodes.Contains(item.ClassCode))
            .ToList();
        var examResults = DataService.GetExamResults()
            .Where(item => classCodes.Contains(item.ClassCode))
            .ToList();

        var attendanceSummary = new Dictionary<string, decimal>
        {
            ["Có mặt"] = attendance.Count(item => item.Status == "Có mặt"),
            ["Muộn"] = attendance.Count(item => item.Status == "Muộn"),
            ["Vắng"] = attendance.Count(item => item.Status == "Vắng")
        };

        return DashboardView(new DashboardPageViewModel
        {
            Title = "Bảng điều khiển giáo viên",
            Subtitle = $"Theo dõi lớp học, lịch dạy, điểm danh và điểm số của {CurrentTeacherName}.",
            Breadcrumbs = Breadcrumbs("Tổng quan"),
            RoleName = CurrentTeacherName,
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Lớp phụ trách", Value = classes.Count.ToString(), Description = "Danh sách lớp đang theo dõi", Icon = "bi-easel", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Buổi học hôm nay", Value = sessions.Count(item => item.SessionDate.Date == DateTime.Today).ToString(), Description = "Cần chuẩn bị điểm danh", Icon = "bi-calendar-day", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Lượt điểm danh", Value = attendance.Count.ToString(), Description = "Đã ghi nhận trong hệ thống", Icon = "bi-list-check", AccentClass = "warning" },
                new SummaryCardViewModel { Title = "Đầu điểm", Value = examResults.Count.ToString(), Description = "Kết quả học tập đã nhập", Icon = "bi-journal-richtext", AccentClass = "success" }
            ],
            Charts =
            [
                new ChartCardViewModel
                {
                    ChartId = "teacherAttendance",
                    Title = "Tình hình điểm danh",
                    Subtitle = "Tổng hợp trạng thái chuyên cần của các lớp phụ trách",
                    ChartType = "doughnut",
                    Labels = attendanceSummary.Keys.ToList(),
                    Values = attendanceSummary.Values.ToList(),
                    Colors = ["#10b981", "#f59e0b", "#ef4444"]
                }
            ],
            Panels =
            [
                new DashboardPanelViewModel
                {
                    Title = "Lịch dạy gần nhất",
                    Subtitle = "Các buổi học sắp tới theo lịch đã tạo",
                    Items = sessions
                        .Take(5)
                        .Select(item => new PanelItemViewModel
                        {
                            Title = item.ClassCode,
                            Meta = $"{item.SessionDate:dd/MM/yyyy} • {item.TimeSlot}",
                            Value = item.Topic,
                            BadgeText = item.Status,
                            BadgeClass = AppUi.StatusBadgeClass(item.Status)
                        }).ToList()
                }
            ],
            QuickActions =
            [
                new QuickActionViewModel { Label = "Tạo buổi học", Url = "/Teacher/Schedule/Create", Icon = "bi-calendar-plus" },
                new QuickActionViewModel { Label = "Tạo bài kiểm tra", Url = "/Teacher/Exams/Create", Icon = "bi-journal-plus", CssClass = "btn btn-outline-success" },
                new QuickActionViewModel { Label = "Điểm danh", Url = "/Teacher/Attendance/Create", Icon = "bi-list-check", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Nhập điểm", Url = "/Teacher/ExamResults/Create", Icon = "bi-pencil-square", CssClass = "btn btn-outline-dark" }
            ]
        });
    }
}

public class ClassesController : TeacherControllerBase
{
    public ClassesController(ILanguageCenterReadService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var classes = GetTeacherClasses();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Lớp được phân công",
            Subtitle = "Theo dõi các lớp mà giáo viên đang phụ trách giảng dạy.",
            Breadcrumbs = Breadcrumbs("Lớp được phân công"),
            PrimaryActionText = "Mở lịch dạy",
            PrimaryActionUrl = "/Teacher/Schedule",
            SearchPlaceholder = "Tìm theo mã lớp, khóa học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng lớp", Value = classes.Count.ToString(), Description = "Số lớp đang hiển thị", Icon = "bi-easel2", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đang hoạt động", Value = classes.Count(item => item.Status == "Đang hoạt động").ToString(), Description = "Lớp đang diễn ra", Icon = "bi-activity", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Sắp khai giảng", Value = classes.Count(item => item.Status == "Sắp khai giảng").ToString(), Description = "Lớp chuẩn bị vào buổi đầu", Icon = "bi-alarm", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Lớp học" }, new() { Header = "Lịch học" }, new() { Header = "Sĩ số" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
                Rows = classes.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.Code}</strong><div class='text-muted small'>{item.CourseName}</div>" },
                        new() { Html = item.Schedule },
                        new() { Html = $"{item.Enrolled}/{item.Capacity} HV" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Teacher/Classes/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Lịch dạy", Url = "/Teacher/Schedule", Icon = "bi-calendar-week", CssClass = "btn btn-sm btn-outline-secondary" }
                    ]
                }).ToList()
            }
        });
    }

    public IActionResult Details(int id)
    {
        var item = GetTeacherClasses().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy lớp học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var sessions = DataService.GetSessions()
            .Where(session => session.ClassCode == item.Code)
            .OrderBy(session => session.SessionDate)
            .ToList();
        var enrollments = DataService.GetEnrollments()
            .Where(enrollment => enrollment.ClassCode == item.Code)
            .OrderByDescending(enrollment => enrollment.EnrolledOn)
            .ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = item.Code,
            Subtitle = $"{item.CourseName} • {item.Schedule}",
            Breadcrumbs = Breadcrumbs("Chi tiết lớp", "Lớp được phân công", "/Teacher/Classes"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Sĩ số", Value = $"{item.Enrolled}/{item.Capacity}", Description = "Học viên đang ghi danh", Icon = "bi-people-fill", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Buổi học", Value = sessions.Count.ToString(), Description = "Đã lên lịch trong hệ thống", Icon = "bi-calendar-check", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Trạng thái", Value = item.Status, Description = "Vòng đời lớp học", Icon = "bi-flag", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin lớp học",
                    Items =
                    [
                        new() { Label = "Khóa học", Value = item.CourseName },
                        new() { Label = "Giáo viên", Value = item.TeacherName },
                        new() { Label = "Ngày bắt đầu", Value = item.StartDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Ngày kết thúc", Value = item.EndDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Tổng quan học viên",
                    Items =
                    [
                        new() { Label = "Học viên ghi danh", Value = enrollments.Count.ToString() },
                        new() { Label = "Đang học", Value = enrollments.Count(x => x.Status == "Đang học").ToString() },
                        new() { Label = "Đã thanh toán", Value = enrollments.Count(x => x.PaymentStatus == "Đã thanh toán").ToString() }
                    ]
                }
            ],
            Timeline = sessions.Select(session => new TimelineItemViewModel
            {
                Title = session.Topic,
                Meta = $"{session.SessionDate:dd/MM/yyyy} • {session.TimeSlot}",
                Description = session.Status,
                AccentClass = session.Status == "Đã diễn ra" ? "secondary" : "info"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Tạo buổi học", Url = "/Teacher/Schedule/Create", Icon = "bi-calendar-plus" },
                new QuickActionViewModel { Label = "Tạo bài kiểm tra", Url = "/Teacher/Exams/Create", Icon = "bi-journal-plus", CssClass = "btn btn-outline-success" },
                new QuickActionViewModel { Label = "Điểm danh", Url = "/Teacher/Attendance/Create", Icon = "bi-list-check", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Teacher/Classes", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }
}

public class ScheduleController : TeacherControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ScheduleController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var classCodes = GetTeacherClassCodes();
        var sessions = DataService.GetSessions()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderByDescending(item => item.SessionDate)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Lịch dạy",
            Subtitle = "Tạo và quản lý các buổi học gắn với lớp được phân công.",
            Breadcrumbs = Breadcrumbs("Lịch dạy"),
            PrimaryActionText = "Tạo buổi học",
            PrimaryActionUrl = "/Teacher/Schedule/Create",
            SearchPlaceholder = "Tìm theo lớp, chủ đề hoặc ngày học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng buổi", Value = sessions.Count.ToString(), Description = "Đã lên lịch trong hệ thống", Icon = "bi-calendar3", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Hôm nay", Value = sessions.Count(item => item.SessionDate.Date == DateTime.Today).ToString(), Description = "Buổi học cần chuẩn bị", Icon = "bi-calendar-day", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Sắp diễn ra", Value = sessions.Count(item => item.Status == "Sắp diễn ra").ToString(), Description = "Buổi học sắp tới", Icon = "bi-alarm", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Lớp" }, new() { Header = "Chủ đề" }, new() { Header = "Khung giờ" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = sessions.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.ClassCode}</strong><div class='text-muted small'>{item.SessionDate:dd/MM/yyyy}</div>" },
                        new() { Html = item.Topic },
                        new() { Html = item.TimeSlot },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Teacher/Schedule/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Điểm danh", Url = $"/Teacher/Attendance/Create?sessionId={item.Id}", Icon = "bi-list-check", CssClass = "btn btn-sm btn-outline-primary" },
                        new() { Label = "Sửa", Url = $"/Teacher/Schedule/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Teacher/Schedule/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa buổi học này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildSessionForm("Tạo buổi học", "/Teacher/Schedule/Create", new SessionInput { SessionDate = DateTime.Today }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SessionInput input)
    {
        var result = _managementService.SaveSession(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildSessionForm("Tạo buổi học", "/Teacher/Schedule/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetSession(id);
        if (input is null)
        {
            SetToast("Không tìm thấy buổi học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildSessionForm("Cập nhật buổi học", $"/Teacher/Schedule/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, SessionInput input)
    {
        var result = _managementService.SaveSession(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildSessionForm("Cập nhật buổi học", $"/Teacher/Schedule/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteSession(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var classCodes = GetTeacherClassCodes();
        var item = DataService.GetSessions()
            .Where(session => classCodes.Contains(session.ClassCode))
            .FirstOrDefault(session => session.Id == id);

        if (item is null)
        {
            SetToast("Không tìm thấy buổi học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var attendance = DataService.GetAttendanceRecords()
            .Where(record =>
                record.ClassCode == item.ClassCode &&
                record.AttendanceDate.Date == item.SessionDate.Date &&
                string.Equals(record.SessionTopic, item.Topic, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Buổi học {item.ClassCode}",
            Subtitle = item.Topic,
            Breadcrumbs = Breadcrumbs("Chi tiết lịch dạy", "Lịch dạy", "/Teacher/Schedule"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Điểm danh", Value = attendance.Count.ToString(), Description = "Lượt điểm danh đã có", Icon = "bi-list-check", AccentClass = "primary" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin buổi học",
                    Items =
                    [
                        new() { Label = "Lớp học", Value = item.ClassCode },
                        new() { Label = "Ngày học", Value = item.SessionDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Khung giờ", Value = item.TimeSlot },
                        new() { Label = "Phòng học", Value = item.Room },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Timeline = attendance.Select(record => new TimelineItemViewModel
            {
                Title = record.StudentName,
                Meta = record.Status,
                Description = string.IsNullOrWhiteSpace(record.Note) ? "Không có ghi chú." : record.Note,
                AccentClass = record.Status == "Có mặt" ? "success" : record.Status == "Muộn" ? "warning" : "danger"
            }).ToList(),
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa buổi học", Url = $"/Teacher/Schedule/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Điểm danh", Url = $"/Teacher/Attendance/Create?sessionId={id}", Icon = "bi-list-check", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Teacher/Schedule", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildSessionForm(string title, string actionUrl, SessionInput input, string? errorMessage = null)
    {
        var classOptions = GetTeacherClasses().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.Code} - {item.CourseName}",
            Value = item.Code,
            Selected = item.Code == input.ClassCode
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật buổi học cho lớp đang phụ trách.",
            Breadcrumbs = Breadcrumbs(title, "Lịch dạy", "/Teacher/Schedule"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng ClassSessions trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Teacher/Schedule",
            SubmitLabel = "Lưu buổi học",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin buổi học",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Lớp học", Name = "ClassCode", Type = "select", Required = true, Options = classOptions },
                        new FormFieldViewModel { Label = "Ngày học", Name = "SessionDate", Value = input.SessionDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel { Label = "Chủ đề", Name = "Topic", Value = input.Topic, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class AttendanceController : TeacherControllerBase
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
        var classCodes = GetTeacherClassCodes();
        var attendance = DataService.GetAttendanceRecords()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderByDescending(item => item.AttendanceDate)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Điểm danh",
            Subtitle = "Ghi nhận chuyên cần theo từng buổi học của lớp phụ trách.",
            Breadcrumbs = Breadcrumbs("Điểm danh"),
            PrimaryActionText = "Điểm danh theo buổi",
            PrimaryActionUrl = "/Teacher/Attendance/Create",
            SearchPlaceholder = "Tìm theo học viên, lớp hoặc chủ đề buổi học...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng lượt", Value = attendance.Count.ToString(), Description = "Dòng điểm danh đang có", Icon = "bi-list-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Có mặt", Value = attendance.Count(item => item.Status == "Có mặt").ToString(), Description = "Học viên tham gia đầy đủ", Icon = "bi-check2-circle", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Vắng / Muộn", Value = attendance.Count(item => item.Status != "Có mặt").ToString(), Description = "Cần theo dõi thêm", Icon = "bi-exclamation-circle", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Buổi học" }, new() { Header = "Ngày" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = attendance.Select(item => new TableRowViewModel
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
                        new() { Label = "Chi tiết", Url = $"/Teacher/Attendance/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Teacher/Attendance/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Teacher/Attendance/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa dòng điểm danh này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create(int? sessionId, int? enrollmentId, string? selectedClassCode)
    {
        return ManagementFormView(BuildAttendanceForm("Điểm danh theo buổi", "/Teacher/Attendance/Create", new AttendanceInput
        {
            SelectedClassCode = selectedClassCode ?? string.Empty,
            ClassSessionId = sessionId ?? 0,
            EnrollmentId = enrollmentId ?? 0
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AttendanceInput input)
    {
        var result = _managementService.SaveAttendance(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildAttendanceForm("Điểm danh theo buổi", "/Teacher/Attendance/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id, string? selectedClassCode)
    {
        var input = _managementService.GetAttendance(id);
        if (input is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(selectedClassCode))
        {
            input.SelectedClassCode = selectedClassCode;
        }

        return ManagementFormView(BuildAttendanceForm("Cập nhật điểm danh", $"/Teacher/Attendance/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, AttendanceInput input)
    {
        var result = _managementService.SaveAttendance(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildAttendanceForm("Cập nhật điểm danh", $"/Teacher/Attendance/Edit/{id}", input, result.Message));
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
        var classCodes = GetTeacherClassCodes();
        var item = DataService.GetAttendanceRecords()
            .Where(record => classCodes.Contains(record.ClassCode))
            .FirstOrDefault(record => record.Id == id);

        if (item is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Điểm danh {item.StudentName}",
            Subtitle = item.SessionTopic,
            Breadcrumbs = Breadcrumbs("Chi tiết điểm danh", "Điểm danh", "/Teacher/Attendance"),
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
                        new() { Label = "Ghi chú", Value = string.IsNullOrWhiteSpace(item.Note) ? "Không có ghi chú." : item.Note }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa điểm danh", Url = $"/Teacher/Attendance/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Teacher/Attendance", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildAttendanceForm(string title, string actionUrl, AttendanceInput input, string? errorMessage = null)
    {
        var classCodes = GetTeacherClassCodes();
        var enrollments = DataService.GetEnrollments()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderBy(item => item.ClassCode)
            .ThenBy(item => item.StudentName)
            .ToList();

        var sessions = DataService.GetSessions()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderByDescending(item => item.SessionDate)
            .ThenBy(item => item.ClassCode)
            .ToList();

        var selectedEnrollmentClassCode = enrollments
            .FirstOrDefault(item => item.Id == input.EnrollmentId)?
            .ClassCode;

        var selectedSessionClassCode = sessions
            .FirstOrDefault(item => item.Id == input.ClassSessionId)?
            .ClassCode;

        var targetClassCode = string.IsNullOrWhiteSpace(input.SelectedClassCode)
            ? selectedEnrollmentClassCode ?? selectedSessionClassCode
            : input.SelectedClassCode;

        input.SelectedClassCode = targetClassCode ?? string.Empty;

        var classOptions = GetTeacherClasses()
            .OrderBy(item => item.Code)
            .Select(item => new SelectOptionViewModel
            {
                Label = $"{item.Code} - {item.CourseName}",
                Value = item.Code,
                Selected = item.Code == input.SelectedClassCode
            })
            .ToList();

        classOptions.Insert(0, new SelectOptionViewModel
        {
            Label = "Chọn lớp học để lọc",
            Value = string.Empty,
            Selected = string.IsNullOrWhiteSpace(input.SelectedClassCode)
        });

        var filteredEnrollments = string.IsNullOrWhiteSpace(targetClassCode)
            ? enrollments
            : enrollments.Where(item => item.ClassCode == targetClassCode).ToList();

        var filteredSessions = string.IsNullOrWhiteSpace(targetClassCode)
            ? sessions
            : sessions.Where(item => item.ClassCode == targetClassCode).ToList();

        var enrollmentOptions = filteredEnrollments
            .Select(item => new SelectOptionViewModel
            {
                Label = $"{item.StudentName} - {item.ClassCode}",
                Value = item.Id.ToString(),
                Selected = item.Id == input.EnrollmentId
            }).ToList();

        var sessionOptions = filteredSessions
            .Select(item => new SelectOptionViewModel
            {
                Label = $"{item.ClassCode} - {item.SessionDate:dd/MM/yyyy} - {item.Topic}",
                Value = item.Id.ToString(),
                Selected = item.Id == input.ClassSessionId
            }).ToList();

        var notice = !string.IsNullOrWhiteSpace(targetClassCode)
            ? $"Đang lọc theo lớp {targetClassCode}. Danh sách ghi danh và buổi học bên dưới chỉ hiển thị dữ liệu thuộc lớp này."
            : "Hãy chọn lớp học trước để hệ thống tự lọc danh sách học viên ghi danh và các buổi học tương ứng.";

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật dữ liệu điểm danh của giáo viên.",
            Breadcrumbs = Breadcrumbs(title, "Điểm danh", "/Teacher/Attendance"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Attendances trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Teacher/Attendance",
            SubmitLabel = "Lưu điểm danh",
            ErrorMessage = errorMessage,
            Notice = notice,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin điểm danh",
                    Fields =
                    [
                        new FormFieldViewModel
                        {
                            Label = "Lớp học",
                            Name = "SelectedClassCode",
                            Type = "select",
                            ColClass = "col-12",
                            Options = classOptions,
                            Hint = "Khi đổi lớp, biểu mẫu sẽ tự tải lại để lọc học viên và buổi học theo lớp đã chọn.",
                            AttributesHtml = $"data-auto-submit-url=\"{actionUrl}\" data-auto-submit-param=\"SelectedClassCode\""
                        },
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

public class ExamResultsController : TeacherControllerBase
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
        var classCodes = GetTeacherClassCodes();
        var examResults = DataService.GetExamResults()
            .Where(item => classCodes.Contains(item.ClassCode))
            .OrderByDescending(item => item.Id)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Nhập điểm",
            Subtitle = "Quản lý đầu điểm cho các lớp mà giáo viên đang phụ trách.",
            Breadcrumbs = Breadcrumbs("Nhập điểm"),
            PrimaryActionText = "Nhập điểm",
            PrimaryActionUrl = "/Teacher/ExamResults/Create",
            SearchPlaceholder = "Tìm theo học viên, lớp hoặc loại bài kiểm tra...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Bài đã nhập", Value = examResults.Count.ToString(), Description = "Số kết quả đang có", Icon = "bi-journal-richtext", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Điểm TB", Value = examResults.Count == 0 ? "0.0" : examResults.Average(item => item.Score).ToString("0.0"), Description = "Trung bình điểm hiện tại", Icon = "bi-graph-up", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Đạt", Value = examResults.Count(item => item.Result == "Đạt").ToString(), Description = "Học viên đạt yêu cầu", Icon = "bi-award", AccentClass = "success" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Học viên" }, new() { Header = "Bài kiểm tra" }, new() { Header = "Điểm" }, new() { Header = "Kết quả" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = examResults.Select(item => new TableRowViewModel
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
                        new() { Label = "Chi tiết", Url = $"/Teacher/ExamResults/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Teacher/ExamResults/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Teacher/ExamResults/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa đầu điểm này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildExamForm("Nhập điểm", "/Teacher/ExamResults/Create", new ExamResultInput { ExamDate = DateTime.Today, MaxScore = 10 }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ExamResultInput input)
    {
        var result = _managementService.SaveExamResult(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Nhập điểm", "/Teacher/ExamResults/Create", input, result.Message));
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

        return ManagementFormView(BuildExamForm("Cập nhật điểm", $"/Teacher/ExamResults/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ExamResultInput input)
    {
        var result = _managementService.SaveExamResult(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Cập nhật điểm", $"/Teacher/ExamResults/Edit/{id}", input, result.Message));
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
        var classCodes = GetTeacherClassCodes();
        var item = DataService.GetExamResults()
            .Where(result => classCodes.Contains(result.ClassCode))
            .FirstOrDefault(result => result.Id == id);

        if (item is null)
        {
            SetToast("Không tìm thấy dữ liệu điểm.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Điểm số {item.StudentName}",
            Subtitle = item.ExamType,
            Breadcrumbs = Breadcrumbs("Chi tiết điểm số", "Nhập điểm", "/Teacher/ExamResults"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Điểm số", Value = item.Score.ToString("0.0"), Description = "Kết quả học viên đạt được", Icon = "bi-award", AccentClass = item.Result == "Đạt" ? "success" : "warning" }
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
                new QuickActionViewModel { Label = "Sửa điểm", Url = $"/Teacher/ExamResults/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Teacher/ExamResults", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildExamForm(string title, string actionUrl, ExamResultInput input, string? errorMessage = null)
    {
        var classCodes = GetTeacherClassCodes();
        var enrollmentOptions = DataService.GetEnrollments()
            .Where(item => classCodes.Contains(item.ClassCode))
            .Select(item => new SelectOptionViewModel
            {
                Label = $"{item.StudentName} - {item.ClassCode}",
                Value = item.Id.ToString(),
                Selected = item.Id == input.EnrollmentId
            }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật dữ liệu điểm số của giáo viên.",
            Breadcrumbs = Breadcrumbs(title, "Nhập điểm", "/Teacher/ExamResults"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Exams và ExamResults trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Teacher/ExamResults",
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
