using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class EnrollmentsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public EnrollmentsController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Ghi danh",
            Subtitle = "Theo dõi đăng ký học, xếp lớp và trạng thái thanh toán.",
            Breadcrumbs = Breadcrumbs("Ghi danh"),
            PrimaryActionText = "Tạo ghi danh",
            PrimaryActionUrl = "/Admin/Enrollments/Create",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Ghi danh" }, new() { Header = "Khóa/Lớp" }, new() { Header = "Trạng thái" }, new() { Header = "Thanh toán" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = DataService.GetEnrollments().Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.EnrollmentCode}</div>" },
                        new() { Html = $"{item.CourseName}<div class='text-muted small'>{item.ClassCode}</div>" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = AppUi.StatusBadge(item.PaymentStatus) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Enrollments/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Enrollments/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Enrollments/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa ghi danh này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildEnrollmentForm("Tạo ghi danh", "/Admin/Enrollments/Create", new EnrollmentInput { EnrollDate = DateTime.Today, Status = "DangHoc" }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EnrollmentInput input)
    {
        var result = _managementService.SaveEnrollment(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildEnrollmentForm("Tạo ghi danh", "/Admin/Enrollments/Create", input, result.Message));
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

        return ManagementFormView(BuildEnrollmentForm("Cập nhật ghi danh", $"/Admin/Enrollments/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, EnrollmentInput input)
    {
        var result = _managementService.SaveEnrollment(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildEnrollmentForm("Cập nhật ghi danh", $"/Admin/Enrollments/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteEnrollment(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetEnrollments().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy ghi danh.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Ghi danh {item.EnrollmentCode}",
            Subtitle = "Chi tiết hồ sơ ghi danh và thanh toán.",
            Breadcrumbs = Breadcrumbs("Chi tiết ghi danh", "Ghi danh", "/Admin/Enrollments"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Trạng thái", Value = item.Status, Description = "Tình trạng học tập", Icon = "bi-journal-check", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã đóng", Value = AppUi.Currency(item.PaidAmount), Description = "Học phí đã thu", Icon = "bi-cash", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin ghi danh",
                    Items =
                    [
                        new() { Label = "Học viên", Value = item.StudentName },
                        new() { Label = "Khóa học", Value = item.CourseName },
                        new() { Label = "Lớp học", Value = item.ClassCode },
                        new() { Label = "Ngày ghi danh", Value = item.EnrolledOn.ToString("dd/MM/yyyy") },
                        new() { Label = "Thanh toán", Value = item.PaymentStatus, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.PaymentStatus) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa ghi danh", Url = $"/Admin/Enrollments/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Enrollments", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
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
            Subtitle = "Tạo hoặc cập nhật ghi danh trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Ghi danh", "/Admin/Enrollments"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Enrollments trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Enrollments",
            SubmitLabel = "Lưu ghi danh",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin ghi danh",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Học viên", Name = "StudentCode", Type = "select", Required = true, Options = studentOptions },
                        new FormFieldViewModel { Label = "Lớp học", Name = "ClassCode", Type = "select", Required = true, Options = classOptions },
                        new FormFieldViewModel { Label = "Ngày ghi danh", Name = "EnrollDate", Value = input.EnrollDate.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel { Label = "Tổng học phí", Name = "TotalFee", Value = input.TotalFee.ToString("0"), Type = "number" },
                        new FormFieldViewModel { Label = "Giảm giá", Name = "DiscountAmount", Value = input.DiscountAmount.ToString("0"), Type = "number" },
                        new FormFieldViewModel
                        {
                            Label = "Trạng thái",
                            Name = "Status",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Đang học", Value = "DangHoc", Selected = input.Status == "DangHoc" },
                                new SelectOptionViewModel { Label = "Bảo lưu", Value = "BaoLuu", Selected = input.Status == "BaoLuu" },
                                new SelectOptionViewModel { Label = "Hoàn thành", Value = "HoanThanh", Selected = input.Status == "HoanThanh" },
                                new SelectOptionViewModel { Label = "Hủy", Value = "Huy", Selected = input.Status == "Huy" }
                            ]
                        },
                        new FormFieldViewModel { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class ReceiptsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ReceiptsController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var receipts = DataService.GetReceipts();
        var debts = DataService.GetDebts();
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Học phí",
            Subtitle = "Theo dõi biên nhận và công nợ học phí.",
            Breadcrumbs = Breadcrumbs("Học phí"),
            PrimaryActionText = "Thu học phí",
            PrimaryActionUrl = "/Admin/Receipts/Create",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Đã thu", Value = AppUi.Currency(receipts.Sum(x => x.Amount)), Description = "Tổng số tiền đã ghi nhận", Icon = "bi-receipt-cutoff", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Công nợ", Value = AppUi.Currency(debts.Sum(x => x.RemainingAmount)), Description = "Tổng số tiền cần thu thêm", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Biên nhận" }, new() { Header = "Lớp" }, new() { Header = "Phương thức" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = receipts.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>{item.ReceiptCode} • {AppUi.Currency(item.Amount)}</div>" },
                        new() { Html = item.ClassCode },
                        new() { Html = item.PaymentMethod },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Receipts/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Receipts/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Receipts/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa biên nhận này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildReceiptForm("Thu học phí", "/Admin/Receipts/Create", new ReceiptInput { PaymentDate = DateTime.Now, PaymentMethod = "Cash" }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ReceiptInput input)
    {
        var result = _managementService.SaveReceipt(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildReceiptForm("Thu học phí", "/Admin/Receipts/Create", input, result.Message));
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

        return ManagementFormView(BuildReceiptForm("Cập nhật biên nhận", $"/Admin/Receipts/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ReceiptInput input)
    {
        var result = _managementService.SaveReceipt(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildReceiptForm("Cập nhật biên nhận", $"/Admin/Receipts/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteReceipt(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetReceipts().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy biên nhận.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Biên nhận {item.ReceiptCode}",
            Subtitle = "Chi tiết biên nhận học phí.",
            Breadcrumbs = Breadcrumbs("Chi tiết biên nhận", "Học phí", "/Admin/Receipts"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Số tiền", Value = AppUi.Currency(item.Amount), Description = "Khoản thu đã ghi nhận", Icon = "bi-cash-coin", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin biên nhận",
                    Items =
                    [
                        new() { Label = "Học viên", Value = item.StudentName },
                        new() { Label = "Lớp học", Value = item.ClassCode },
                        new() { Label = "Ngày thu", Value = item.PaidOn.ToString("dd/MM/yyyy HH:mm") },
                        new() { Label = "Phương thức", Value = item.PaymentMethod },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa biên nhận", Url = $"/Admin/Receipts/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Receipts", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
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
            Subtitle = "Tạo hoặc cập nhật biên nhận học phí trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Học phí", "/Admin/Receipts"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Receipts trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Receipts",
            SubmitLabel = "Lưu biên nhận",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin biên nhận",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Ghi danh", Name = "EnrollmentId", Type = "select", Required = true, Options = enrollmentOptions },
                        new FormFieldViewModel { Label = "Ngày thu", Name = "PaymentDate", Value = input.PaymentDate.ToString("yyyy-MM-ddTHH:mm"), Type = "datetime-local", Required = true },
                        new FormFieldViewModel { Label = "Số tiền", Name = "Amount", Value = input.Amount.ToString("0"), Type = "number", Required = true },
                        new FormFieldViewModel
                        {
                            Label = "Phương thức",
                            Name = "PaymentMethod",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Tiền mặt", Value = "Cash", Selected = input.PaymentMethod == "Cash" },
                                new SelectOptionViewModel { Label = "Chuyển khoản", Value = "Transfer", Selected = input.PaymentMethod == "Transfer" },
                                new SelectOptionViewModel { Label = "Thẻ", Value = "Card", Selected = input.PaymentMethod == "Card" }
                            ]
                        },
                        new FormFieldViewModel { Label = "Ghi chú", Name = "Note", Value = input.Note, Type = "textarea", ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}

public class SessionsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public SessionsController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Buổi học",
            Subtitle = "Quản lý lịch học chi tiết theo từng lớp.",
            Breadcrumbs = Breadcrumbs("Buổi học"),
            PrimaryActionText = "Thêm buổi học",
            PrimaryActionUrl = "/Admin/Sessions/Create",
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Lớp" }, new() { Header = "Chủ đề" }, new() { Header = "Khung giờ" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = DataService.GetSessions().Select(item => new TableRowViewModel
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
                        new() { Label = "Chi tiết", Url = $"/Admin/Sessions/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Sessions/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Sessions/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa buổi học này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildSessionForm("Thêm buổi học", "/Admin/Sessions/Create", new SessionInput { SessionDate = DateTime.Today }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(SessionInput input)
    {
        var result = _managementService.SaveSession(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildSessionForm("Thêm buổi học", "/Admin/Sessions/Create", input, result.Message));
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

        return ManagementFormView(BuildSessionForm("Cập nhật buổi học", $"/Admin/Sessions/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, SessionInput input)
    {
        var result = _managementService.SaveSession(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildSessionForm("Cập nhật buổi học", $"/Admin/Sessions/Edit/{id}", input, result.Message));
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
        var item = DataService.GetSessions().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy buổi học.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Buổi học {item.ClassCode}",
            Subtitle = item.Topic,
            Breadcrumbs = Breadcrumbs("Chi tiết buổi học", "Buổi học", "/Admin/Sessions"),
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
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa buổi học", Url = $"/Admin/Sessions/Edit/{id}", Icon = "bi-pencil-square" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Sessions", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildSessionForm(string title, string actionUrl, SessionInput input, string? errorMessage = null)
    {
        var classOptions = DataService.GetClasses().Select(item => new SelectOptionViewModel
        {
            Label = $"{item.Code} - {item.CourseName}",
            Value = item.Code,
            Selected = item.Code == input.ClassCode
        }).ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật buổi học trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Buổi học", "/Admin/Sessions"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng ClassSessions trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Sessions",
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
