using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class AccountController : Controller
{
    private readonly IAccountAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
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
        model.Subtitle = "Đăng nhập bằng tài khoản đang lưu trong cơ sở dữ liệu để truy cập đúng khu vực quản trị.";
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
        return View(new RegisterViewModel
        {
            Title = "Đăng ký",
            Subtitle = "Biểu mẫu đăng ký dành cho học viên mới.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng ký", IsActive = true }]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Title = "Đăng ký";
        model.Subtitle = "Biểu mẫu đăng ký dành cho học viên mới.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng ký", IsActive = true }];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterStudentAsync(model.FullName, model.Email, model.Phone, model.Password);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData[AppConstants.ToastMessageKey] = result.Message;
        TempData[AppConstants.ToastTypeKey] = "success";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel
        {
            Title = "Quên mật khẩu",
            Subtitle = "Hướng dẫn lấy lại mật khẩu cho tài khoản học viên.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Quên mật khẩu", IsActive = true }]
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        model.Title = "Quên mật khẩu";
        model.Subtitle = "Hướng dẫn lấy lại mật khẩu cho tài khoản học viên.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Quên mật khẩu", IsActive = true }];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData[AppConstants.ToastMessageKey] = "Yêu cầu khôi phục đã được ghi nhận. Trung tâm sẽ liên hệ lại qua email đã đăng ký.";
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
            Subtitle = "Đăng nhập bằng tài khoản đang lưu trong cơ sở dữ liệu để truy cập đúng khu vực quản trị.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Đăng nhập", IsActive = true }]
        };
    }
}
