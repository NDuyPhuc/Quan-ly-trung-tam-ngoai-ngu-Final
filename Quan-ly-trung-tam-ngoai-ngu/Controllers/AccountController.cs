using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class AccountController : Controller
{
    private readonly IAccountAuthService _authService;
    private readonly IContactMessageService _contactMessageService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IAccountAuthService authService,
        IContactMessageService contactMessageService,
        ILogger<AccountController> logger)
    {
        _authService = authService;
        _contactMessageService = contactMessageService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(CreateLoginModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.Title = "Đăng nhập";
        model.Subtitle = "Đăng nhập bằng tài khoản nội bộ đang lưu trong cơ sở dữ liệu để truy cập đúng khu vực quản trị.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng nhập", IsActive = true }];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var account = await _authService.ValidateLoginAsync(model.Email, model.Password);
        if (account is null)
        {
            _logger.LogWarning("Login failed for {Login}.", model.Email);
            model.ErrorMessage = "Thông tin đăng nhập không hợp lệ hoặc tài khoản đã bị khóa.";
            return View(model);
        }

        HttpContext.Session.SetString(AppConstants.SessionDemoUserEmail, account.Email);
        HttpContext.Session.SetString(AppConstants.SessionDemoUserRole, account.Role);
        HttpContext.Session.SetString(AppConstants.SessionDemoUserDisplayName, account.FullName);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.FullName),
            new(ClaimTypes.Email, account.Email),
            new(ClaimTypes.Role, account.Role),
            new("username", account.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        TempData[AppConstants.ToastMessageKey] = $"Đăng nhập thành công với vai trò {AppUi.RoleLabel(account.Role)}.";
        TempData[AppConstants.ToastTypeKey] = "success";

        return account.Role switch
        {
            AppConstants.Roles.Admin => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
            AppConstants.Roles.Staff => RedirectToAction("Index", "Dashboard", new { area = "Staff" }),
            AppConstants.Roles.Teacher => RedirectToAction("Index", "Dashboard", new { area = "Teacher" }),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [HttpGet]
    public IActionResult Register()
    {
        return RedirectToAction(nameof(Consultation));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Register(ConsultationRequestViewModel model, CancellationToken cancellationToken)
    {
        return Consultation(model, cancellationToken);
    }

    [HttpGet]
    public IActionResult Consultation()
    {
        return View("Register", CreateConsultationModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Consultation(ConsultationRequestViewModel model, CancellationToken cancellationToken)
    {
        ApplyConsultationMeta(model);

        if (!ModelState.IsValid)
        {
            return View("Register", model);
        }

        var result = await _contactMessageService.SendContactAsync(new ContactMessageRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            Topic = "Đăng ký tư vấn",
            PreferredProgram = model.PreferredProgram,
            CurrentLevel = string.Empty,
            PreferredSchedule = model.PreferredSchedule,
            PreferredContactMethod = model.PreferredContactMethod,
            Message = model.Message,
            SourcePage = "Trang đăng ký tư vấn"
        }, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View("Register", model);
        }

        TempData[AppConstants.ToastMessageKey] = result.Message;
        TempData[AppConstants.ToastTypeKey] = result.EmailDelivered ? "success" : "warning";
        return RedirectToAction(nameof(Consultation));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel
        {
            Title = "Quên mật khẩu",
            Subtitle = "Hướng dẫn lấy lại mật khẩu cho tài khoản nội bộ có quyền truy cập khu quản trị.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Quên mật khẩu", IsActive = true }]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        model.Title = "Quên mật khẩu";
        model.Subtitle = "Hướng dẫn lấy lại mật khẩu cho tài khoản nội bộ có quyền truy cập khu quản trị.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Quên mật khẩu", IsActive = true }];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData[AppConstants.ToastMessageKey] = "Yêu cầu khôi phục đã được ghi nhận. Quản trị hệ thống sẽ liên hệ lại qua email nội bộ đã đăng ký.";
        TempData[AppConstants.ToastTypeKey] = "success";
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        TempData[AppConstants.ToastMessageKey] = "Đã đăng xuất khỏi phiên làm việc hiện tại.";
        TempData[AppConstants.ToastTypeKey] = "info";
        return RedirectToAction(nameof(Login));
    }

    private static LoginViewModel CreateLoginModel()
    {
        return new LoginViewModel
        {
            Title = "Đăng nhập",
            Subtitle = "Đăng nhập bằng tài khoản nội bộ đang lưu trong cơ sở dữ liệu để truy cập đúng khu vực quản trị.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng nhập", IsActive = true }]
        };
    }

    private static ConsultationRequestViewModel CreateConsultationModel()
    {
        var model = new ConsultationRequestViewModel
        {
            PreferredContactMethod = "Phone"
        };

        ApplyConsultationMeta(model);
        return model;
    }

    private static void ApplyConsultationMeta(ConsultationRequestViewModel model)
    {
        model.Title = "Đăng ký tư vấn";
        model.Subtitle = "Để lại thông tin để trung tâm ghi nhận hồ sơ tư vấn và liên hệ lại. Đây không phải là tạo tài khoản đăng nhập vào web quản trị.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng ký tư vấn", IsActive = true }];
    }
}
