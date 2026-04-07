using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Staff.Controllers;

public class DebtsController : StaffControllerBase
{
    public DebtsController(ILanguageCenterReadService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var debts = DataService.GetDebts()
            .OrderByDescending(x => x.RemainingAmount)
            .ThenBy(x => x.DueDate)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Công nợ học phí",
            Subtitle = "Theo dõi các khoản học phí còn lại để giáo vụ chủ động nhắc phí và tạo biên nhận tiếp theo.",
            Breadcrumbs = Breadcrumbs("Công nợ"),
            PrimaryActionText = "Thu học phí",
            PrimaryActionUrl = "/Staff/Receipts/Create",
            SearchPlaceholder = "Tìm theo học viên, khóa học hoặc trạng thái công nợ...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Số hồ sơ công nợ", Value = debts.Count.ToString(), Description = "Học viên còn phải thanh toán", Icon = "bi-wallet2", AccentClass = "danger" },
                new SummaryCardViewModel { Title = "Tổng còn lại", Value = AppUi.Currency(debts.Sum(x => x.RemainingAmount)), Description = "Khoản học phí cần tiếp tục theo dõi", Icon = "bi-cash-coin", AccentClass = "warning" },
                new SummaryCardViewModel { Title = "Quá hạn", Value = debts.Count(x => x.Status == "Quá hạn").ToString(), Description = "Cần ưu tiên xử lý trước", Icon = "bi-exclamation-triangle", AccentClass = "danger" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Học viên" },
                    new() { Header = "Khóa học" },
                    new() { Header = "Còn lại" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "240px" }
                ],
                Rows = debts.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.StudentName}</strong><div class='text-muted small'>Hạn {item.DueDate:dd/MM/yyyy}</div>" },
                        new() { Html = item.CourseName },
                        new() { Html = $"<strong class='text-danger'>{AppUi.Currency(item.RemainingAmount)}</strong><div class='text-muted small'>Đã thu {AppUi.Currency(item.PaidAmount)}</div>" },
                        new() { Html = AppUi.StatusBadge(item.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Staff/Debts/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Thu học phí", Url = "/Staff/Receipts/Create", Icon = "bi-receipt", CssClass = "btn btn-sm btn-outline-primary" }
                    ]
                }).ToList()
            }
        });
    }

    public IActionResult Details(int id)
    {
        var debt = DataService.GetDebts().FirstOrDefault(x => x.Id == id);
        if (debt is null)
        {
            SetToast("Không tìm thấy công nợ học phí.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var relatedReceipts = DataService.GetReceipts()
            .Where(x => string.Equals(x.StudentName, debt.StudentName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.PaidOn)
            .ToList();

        var enrollment = DataService.GetEnrollments()
            .FirstOrDefault(x =>
                string.Equals(x.StudentName, debt.StudentName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.CourseName, debt.CourseName, StringComparison.OrdinalIgnoreCase));
        var student = DataService.GetStudents()
            .FirstOrDefault(x => string.Equals(x.FullName, debt.StudentName, StringComparison.OrdinalIgnoreCase));

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Công nợ {debt.StudentName}",
            Subtitle = "Đối chiếu học phí, lịch sử biên nhận và hạn theo dõi của từng học viên.",
            Breadcrumbs = Breadcrumbs("Chi tiết công nợ", "Công nợ", "/Staff/Debts"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng học phí", Value = AppUi.Currency(debt.TotalFee), Description = debt.CourseName, Icon = "bi-journal-text", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã thu", Value = AppUi.Currency(debt.PaidAmount), Description = "Tổng biên nhận đã ghi nhận", Icon = "bi-cash-stack", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Còn lại", Value = AppUi.Currency(debt.RemainingAmount), Description = $"Hạn xử lý {debt.DueDate:dd/MM/yyyy}", Icon = "bi-wallet2", AccentClass = "danger" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin công nợ",
                    Items =
                    [
                        new() { Label = "Học viên", Value = debt.StudentName },
                        new() { Label = "Khóa học", Value = debt.CourseName },
                        new() { Label = "Hạn theo dõi", Value = debt.DueDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Trạng thái", Value = debt.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(debt.Status) }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Đối chiếu ghi danh",
                    Items =
                    [
                        new() { Label = "Mã ghi danh", Value = enrollment?.EnrollmentCode ?? "Chưa tìm thấy" },
                        new() { Label = "Lớp học", Value = enrollment?.ClassCode ?? "Chưa đối chiếu" },
                        new() { Label = "Trạng thái học", Value = enrollment?.Status ?? "Chưa đối chiếu" },
                        new() { Label = "Thanh toán", Value = enrollment?.PaymentStatus ?? "Chưa đối chiếu" }
                    ]
                }
            ],
            Timeline = relatedReceipts.Any()
                ? relatedReceipts.Select(item => new TimelineItemViewModel
                {
                    Title = $"{item.ReceiptCode} - {AppUi.Currency(item.Amount)}",
                    Meta = item.PaidOn.ToString("dd/MM/yyyy HH:mm"),
                    Description = $"{item.PaymentMethod} • {item.Status}",
                    AccentClass = "success"
                }).ToList()
                : [new TimelineItemViewModel
                {
                    Title = "Chưa có biên nhận",
                    Meta = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Description = "Học viên này chưa phát sinh lịch sử thu học phí.",
                    AccentClass = "warning"
                }],
            Actions =
            [
                new QuickActionViewModel { Label = "Thu học phí", Url = "/Staff/Receipts/Create", Icon = "bi-receipt" },
                new QuickActionViewModel { Label = "Xem học viên", Url = student is null ? "/Staff/Students" : $"/Staff/Students/Details/{student.Id}", Icon = "bi-person-badge", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Staff/Debts", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }
}
