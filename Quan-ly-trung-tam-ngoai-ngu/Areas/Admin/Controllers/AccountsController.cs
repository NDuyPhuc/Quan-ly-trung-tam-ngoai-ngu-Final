using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class AccountsController : AdminControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public AccountsController(
        IMockDataService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
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
            SearchPlaceholder = "Tìm theo email, vai trò, phòng ban...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tài khoản kích hoạt", Value = accounts.Count(x => x.Status == "Đang hoạt động").ToString(), Description = "Có thể đăng nhập vào hệ thống", Icon = "bi-person-check", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Tài khoản tạm khóa", Value = accounts.Count(x => x.Status != "Đang hoạt động").ToString(), Description = "Đang bị vô hiệu hóa", Icon = "bi-person-lock", AccentClass = "warning" }
            ],
            Filters =
            [
                new FilterGroupViewModel { Label = "Vai trò", InputId = "role", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Quản trị viên", Value = "Admin" }, new() { Label = "Giáo vụ", Value = "Staff" }, new() { Label = "Giáo viên", Value = "Teacher" }] }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Tài khoản" }, new() { Header = "Vai trò" }, new() { Header = "Phòng ban" }, new() { Header = "Trạng thái" }, new() { Header = "Thao tác", Width = "260px" }],
                Rows = accounts.Select(account => new TableRowViewModel
                {
                    Id = account.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{account.FullName}</strong><div class='text-muted small'>{account.Username} • {account.Email}</div>" },
                        new() { Html = AppUi.RoleLabel(account.Role) },
                        new() { Html = account.Department },
                        new() { Html = AppUi.StatusBadge(account.Status) },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/Accounts/Details/{account.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/Accounts/Edit/{account.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/Accounts/Delete/{account.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa tài khoản này?" }
                    ]
                }).ToList()
            }
        };

        return ManagementListView(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildForm("Tạo tài khoản", "/Admin/Accounts/Create", new AccountInput()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(AccountInput input)
    {
        var result = _managementService.SaveAccount(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildForm("Tạo tài khoản", "/Admin/Accounts/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetAccount(id);
        if (input is null)
        {
            SetToast("Không tìm thấy tài khoản cần chỉnh sửa.", "danger");
            return RedirectToAction(nameof(Index));
        }

        input.Password = string.Empty;
        return ManagementFormView(BuildForm("Cập nhật tài khoản", $"/Admin/Accounts/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, AccountInput input)
    {
        var result = _managementService.SaveAccount(id, input);
        if (!result.Succeeded)
        {
            input.Password = string.Empty;
            return ManagementFormView(BuildForm("Cập nhật tài khoản", $"/Admin/Accounts/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _managementService.DeleteAccount(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetAccounts().FirstOrDefault(x => x.Id == id);
        if (item is null)
        {
            SetToast("Không tìm thấy tài khoản.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var roleLabel = AppUi.RoleLabel(item.Role);

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = $"Chi tiết tài khoản {item.FullName}",
            Subtitle = "Thông tin đăng nhập, phân quyền và trạng thái tài khoản.",
            Breadcrumbs = Breadcrumbs("Chi tiết tài khoản", "Tài khoản", "/Admin/Accounts"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Vai trò", Value = roleLabel, Description = "Quyền truy cập của tài khoản", Icon = "bi-person-badge", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Trạng thái", Value = item.Status, Description = "Khả năng đăng nhập hiện tại", Icon = "bi-shield-check", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin đăng nhập",
                    Items =
                    [
                        new() { Label = "Username", Value = item.Username },
                        new() { Label = "Email", Value = item.Email },
                        new() { Label = "Vai trò", Value = roleLabel, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass("Đang hoạt động") }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Thông tin liên hệ",
                    Items =
                    [
                        new() { Label = "Họ tên", Value = item.FullName },
                        new() { Label = "Số điện thoại", Value = item.Phone },
                        new() { Label = "Phòng ban", Value = item.Department },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Sửa tài khoản", Url = $"/Admin/Accounts/Edit/{id}", Icon = "bi-pencil-square", CssClass = "btn btn-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/Accounts", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildForm(string title, string actionUrl, AccountInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo hoặc cập nhật tài khoản đăng nhập cho các vai trò trong hệ thống.",
            Breadcrumbs = Breadcrumbs(title, "Tài khoản", "/Admin/Accounts"),
            FormTitle = title,
            FormDescription = "Thông tin ở đây sẽ được lưu trực tiếp xuống bảng Accounts trong SQL Server.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/Accounts",
            SubmitLabel = "Lưu tài khoản",
            Notice = "Nếu để trống mật khẩu ở màn hình cập nhật, hệ thống sẽ giữ nguyên mật khẩu cũ.",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin tài khoản",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Username", Name = "Username", Value = input.Username, Required = true },
                        new FormFieldViewModel { Label = "Họ và tên", Name = "FullName", Value = input.FullName, Required = true },
                        new FormFieldViewModel { Label = "Email", Name = "Email", Value = input.Email, Type = "email" },
                        new FormFieldViewModel { Label = "Số điện thoại", Name = "Phone", Value = input.Phone },
                        new FormFieldViewModel { Label = "Mật khẩu", Name = "Password", Value = input.Password, Type = "password", Hint = "Bắt buộc khi tạo mới, có thể để trống khi cập nhật." },
                        new FormFieldViewModel
                        {
                            Label = "Vai trò",
                            Name = "Role",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Quản trị viên", Value = AppConstants.Roles.Admin, Selected = input.Role == AppConstants.Roles.Admin },
                                new SelectOptionViewModel { Label = "Giáo vụ", Value = AppConstants.Roles.Staff, Selected = input.Role == AppConstants.Roles.Staff },
                                new SelectOptionViewModel { Label = "Giáo viên", Value = AppConstants.Roles.Teacher, Selected = input.Role == AppConstants.Roles.Teacher }
                            ]
                        },
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
