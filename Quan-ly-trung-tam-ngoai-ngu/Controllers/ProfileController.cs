using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

[DemoAuthorize]
public class ProfileController : Controller
{
    private readonly ILanguageCenterReadService _dataService;

    public ProfileController(ILanguageCenterReadService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var email = HttpContext.Session.GetString(AppConstants.SessionDemoUserEmail);
        var account = _dataService.GetAccounts().FirstOrDefault(x => x.Email == email) ?? _dataService.GetAccounts().First();
        var roleLabel = AppUi.RoleLabel(account.Role);

        var model = new ProfileViewModel
        {
            Title = "Hồ sơ cá nhân",
            Subtitle = "Thông tin hồ sơ và hoạt động gần đây trong hệ thống.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Hồ sơ cá nhân", IsActive = true }],
            FullName = account.FullName,
            Email = account.Email,
            Phone = account.Phone,
            CurrentRole = roleLabel,
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Vai trò", Value = roleLabel, Description = "Điều hướng bảng điều khiển theo quyền hiện tại", Icon = "bi-person-badge", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Trạng thái tài khoản", Value = account.Status, Description = "Hiển thị quyền truy cập và trạng thái sử dụng", Icon = "bi-shield-check", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Phòng ban", Value = account.Department, Description = "Thông tin nội bộ gắn với hồ sơ đang đăng nhập", Icon = "bi-diagram-3", AccentClass = "info" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin cơ bản",
                    Items =
                    [
                        new DetailItemViewModel { Label = "Họ và tên", Value = account.FullName },
                        new DetailItemViewModel { Label = "Email", Value = account.Email },
                        new DetailItemViewModel { Label = "Số điện thoại", Value = account.Phone },
                        new DetailItemViewModel { Label = "Vai trò", Value = roleLabel, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass("Đang hoạt động") }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Ghi chú triển khai",
                    Description = "Các trường dưới đây tóm tắt nhanh thông tin tài khoản và cấu hình hiện tại.",
                    Items =
                    [
                        new DetailItemViewModel { Label = "Nguồn dữ liệu", Value = "Phiên đăng nhập hiện tại" },
                        new DetailItemViewModel { Label = "Tình trạng", Value = "Sẵn sàng sử dụng", IsBadge = true, BadgeClass = "bg-success-subtle text-success-emphasis" },
                        new DetailItemViewModel { Label = "Bước tiếp theo", Value = "Đồng bộ thêm dữ liệu hồ sơ khi hệ thống mở rộng." }
                    ]
                }
            ],
            RecentActivities =
            [
                new TimelineItemViewModel { Title = "Đăng nhập hệ thống", Meta = "Hôm nay", Description = $"Phiên hiện tại đã ghi nhận người dùng {roleLabel.ToLowerInvariant()} truy cập vào hệ thống.", AccentClass = "primary" },
                new TimelineItemViewModel { Title = "Kiểm tra hồ sơ", Meta = "Hôm nay", Description = "Trang hồ sơ đã được làm mới để thống nhất với toàn bộ giao diện bên trong hệ thống.", AccentClass = "info" },
                new TimelineItemViewModel { Title = "Sẵn sàng mở rộng dữ liệu", Meta = "Bước tiếp theo", Description = "Khi hệ thống phát triển thêm, hồ sơ người dùng có thể được đồng bộ với nhiều phân hệ hơn.", AccentClass = "warning" }
            ]
        };

        return View(model);
    }
}
