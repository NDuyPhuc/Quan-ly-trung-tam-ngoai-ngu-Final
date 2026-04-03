using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class AccountsController : AdminControllerBase
{
    public AccountsController(IMockDataService dataService) : base(dataService)
    {
    }

    public IActionResult Index()
    {
        var accounts = DataService.GetAccounts();
        var model = new ManagementListPageViewModel
        {
            Title = "Quản lý tài khoản",
            Subtitle = "Quản trị tài khoản và phân quyền hiển thị cho quản trị viên, giáo vụ và giáo viên.",
            Breadcrumbs = Breadcrumbs("Tài khoản"),
            PrimaryActionText = "Tạo tài khoản",
            PrimaryActionUrl = "/Admin/Accounts/Create",
            ToolbarNote = "Dữ liệu đang lấy từ dịch vụ mô phỏng và chưa ghi xuống cơ sở dữ liệu thật.",
            SearchPlaceholder = "Tìm theo email, vai trò, phòng ban...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tài khoản kích hoạt", Value = accounts.Count(x => x.Status == "Đang hoạt động").ToString(), Description = "Có thể đăng nhập ở chế độ mô phỏng", Icon = "bi-person-check", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Tài khoản tạm khóa", Value = accounts.Count(x => x.Status != "Đang hoạt động").ToString(), Description = "Hiển thị badge và thao tác phù hợp", Icon = "bi-person-lock", AccentClass = "warning" }
            ],
            Filters =
            [
                new FilterGroupViewModel { Label = "Vai trò", InputId = "role", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Quản trị viên", Value = "Admin" }, new() { Label = "Giáo vụ", Value = "Staff" }, new() { Label = "Giáo viên", Value = "Teacher" }] },
                new FilterGroupViewModel { Label = "Trạng thái", InputId = "status", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Đang hoạt động", Value = "active" }, new() { Label = "Tạm khóa", Value = "locked" }] }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Tài khoản" }, new() { Header = "Vai trò" }, new() { Header = "Phòng ban" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "220px" }],
                Rows = accounts.Select(x => new TableRowViewModel
                {
                    Id = x.Id.ToString(),
                    Cells =
                    [
                        new TableCellViewModel { Html = $"<strong>{x.FullName}</strong><div class='text-muted small'>{x.Email}</div>" },
                        new TableCellViewModel { Html = AppUi.RoleLabel(x.Role) },
                        new TableCellViewModel { Html = x.Department },
                        new TableCellViewModel { Html = AppUi.StatusBadge(x.Status) },
                        new TableCellViewModel { Html = "" }
                    ],
                    Actions =
                    [
                        new TableRowActionViewModel { Label = "Chi tiết", Url = $"/Admin/Accounts/Details/{x.Id}", Icon = "bi-eye" },
                        new TableRowActionViewModel { Label = "Sửa", Url = $"/Admin/Accounts/Edit/{x.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new TableRowActionViewModel { Label = "Khóa", Url = "#", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger", RequiresConfirm = true, ConfirmMessage = "Bạn muốn khóa tài khoản mô phỏng này?" }
                    ]
                }).ToList()
            }
        };

        return ManagementListView(model);
    }

    public IActionResult Create()
    {
        return ManagementFormView(BuildForm("Tạo tài khoản", "/Admin/Accounts"));
    }

    public IActionResult Edit(int id)
    {
        return ManagementFormView(BuildForm("Cập nhật tài khoản", "/Admin/Accounts", DataService.GetAccounts().First(x => x.Id == id)));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetAccounts().First(x => x.Id == id);
        var roleLabel = AppUi.RoleLabel(item.Role);

        var model = new ManagementDetailsPageViewModel
        {
            Title = $"Chi tiết tài khoản {item.FullName}",
            Subtitle = "Thông tin phân quyền và cấu hình đăng nhập hiển thị trên giao diện mô phỏng.",
            Breadcrumbs = Breadcrumbs("Chi tiết tài khoản", "Tài khoản", "/Admin/Accounts"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Vai trò", Value = roleLabel, Description = "Điều hướng bảng điều khiển theo phiên đăng nhập", Icon = "bi-person-badge", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Trạng thái", Value = item.Status, Description = "Hiển thị nhãn trạng thái tài khoản", Icon = "bi-shield-check", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel { Title = "Thông tin đăng nhập", Items = [new() { Label = "Email", Value = item.Email }, new() { Label = "Mật khẩu mẫu", Value = item.Password }, new() { Label = "Vai trò", Value = roleLabel, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass("Đang hoạt động") }] },
                new DetailSectionViewModel { Title = "Thông tin nội bộ", Items = [new() { Label = "Họ tên", Value = item.FullName }, new() { Label = "Số điện thoại", Value = item.Phone }, new() { Label = "Phòng ban", Value = item.Department }, new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }] }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa tài khoản", Url = $"/Admin/Accounts/Edit/{id}", Icon = "bi-pencil-square", CssClass = "btn btn-primary" },
                new QuickActionViewModel { Label = "Quay lại danh sách", Url = "/Admin/Accounts", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ],
            Timeline =
            [
                new TimelineItemViewModel { Title = "Đăng nhập mô phỏng", Meta = "Dữ liệu mẫu", Description = "Tài khoản có thể được dùng để chuyển hướng đến đúng bảng điều khiển theo vai trò.", AccentClass = "primary" },
                new TimelineItemViewModel { Title = "Kết nối cơ sở dữ liệu", Meta = "Bước sau", Description = "Khi có backend hoàn chỉnh, dữ liệu tài khoản sẽ được lưu và phân quyền thật.", AccentClass = "warning" }
            ]
        };

        return ManagementDetailsView(model);
    }

    private static ManagementFormPageViewModel BuildForm(string title, string cancelUrl, DemoAccount? account = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Biểu mẫu tài khoản dùng để trình bày cấu trúc và validation giao diện.",
            Breadcrumbs = Breadcrumbs(title, "Tài khoản", "/Admin/Accounts"),
            FormTitle = title,
            FormDescription = "Dùng để minh họa bố cục biểu mẫu, kiểm tra nhập liệu và cấu trúc dữ liệu tài khoản.",
            CancelUrl = cancelUrl,
            Notice = "Biểu mẫu đang ở chế độ mô phỏng và sẽ kết nối cơ sở dữ liệu ở bước sau.",
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin tài khoản",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Họ và tên", Name = "FullName", Value = account?.FullName ?? "", Required = true },
                        new FormFieldViewModel { Label = "Email", Name = "Email", Value = account?.Email ?? "", Required = true, Type = "email" },
                        new FormFieldViewModel { Label = "Số điện thoại", Name = "Phone", Value = account?.Phone ?? "", Required = true },
                        new FormFieldViewModel { Label = "Mật khẩu", Name = "Password", Value = account?.Password ?? "123456", Required = true, Type = "password" },
                        new FormFieldViewModel { Label = "Vai trò", Name = "Role", Type = "select", Required = true, Options = [new() { Label = "Quản trị viên", Value = "Admin", Selected = account?.Role == "Admin" }, new() { Label = "Giáo vụ", Value = "Staff", Selected = account?.Role == "Staff" }, new() { Label = "Giáo viên", Value = "Teacher", Selected = account?.Role == "Teacher" }] },
                        new FormFieldViewModel { Label = "Trạng thái", Name = "Status", Type = "select", Required = true, Options = [new() { Label = "Đang hoạt động", Value = "Đang hoạt động", Selected = account?.Status != "Tạm khóa" }, new() { Label = "Tạm khóa", Value = "Tạm khóa", Selected = account?.Status == "Tạm khóa" }] }
                    ]
                }
            ]
        };
    }
}
