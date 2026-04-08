using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class ContactController : Controller
{
    private readonly IContactMessageService _contactMessageService;
    private readonly IPublicSiteContentService _publicSiteContentService;

    public ContactController(
        IContactMessageService contactMessageService,
        IPublicSiteContentService publicSiteContentService)
    {
        _contactMessageService = contactMessageService;
        _publicSiteContentService = publicSiteContentService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = CreateModel();
        ApplyPublicContent();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactViewModel model, string? returnUrl, CancellationToken cancellationToken)
    {
        ApplyPageMeta(model);

        if (!ModelState.IsValid)
        {
            ApplyPublicContent();
            return View(model);
        }

        var result = await _contactMessageService.SendContactAsync(new ContactMessageRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            Phone = model.Phone,
            Topic = model.Topic,
            PreferredProgram = model.PreferredProgram,
            CurrentLevel = model.CurrentLevel,
            PreferredSchedule = model.PreferredSchedule,
            PreferredContactMethod = model.PreferredContactMethod,
            Message = model.Message,
            SourcePage = "Trang liên hệ"
        }, cancellationToken);

        TempData[AppConstants.ToastMessageKey] = result.Message;
        TempData[AppConstants.ToastTypeKey] = result.EmailDelivered ? "success" : "warning";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string email, string? returnUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData[AppConstants.ToastMessageKey] = "Vui lòng nhập email để đăng ký nhận thông tin mới.";
            TempData[AppConstants.ToastTypeKey] = "danger";
        }
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            TempData[AppConstants.ToastMessageKey] = "Địa chỉ email đăng ký chưa hợp lệ.";
            TempData[AppConstants.ToastTypeKey] = "danger";
        }
        else
        {
            var result = await _contactMessageService.SendNewsletterAsync(new NewsletterSubscriptionRequest
            {
                Email = email,
                SourcePage = "Footer newsletter"
            }, cancellationToken);

            TempData[AppConstants.ToastMessageKey] = result.Message;
            TempData[AppConstants.ToastTypeKey] = result.EmailDelivered ? "success" : "warning";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("/#lien-he");
    }

    private ContactViewModel CreateModel()
    {
        var model = new ContactViewModel
        {
            PreferredContactMethod = "Email"
        };

        ApplyPageMeta(model);
        return model;
    }

    private static void ApplyPageMeta(ContactViewModel model)
    {
        model.Title = "Liên hệ";
        model.Subtitle = "Để lại thông tin để trung tâm ghi nhận hồ sơ tư vấn và liên hệ lại đúng nhu cầu.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Liên hệ", IsActive = true }];
    }

    private void ApplyPublicContent()
    {
        var contactSettings = _publicSiteContentService.GetSiteSettings().ContactSection;
        ViewData["ContactSectionTitle"] = contactSettings.SectionTitle;
        ViewData["ContactSectionSubtitle"] = contactSettings.SectionSubtitle;
        ViewData["ContactFormTitle"] = contactSettings.FormTitle;
        ViewData["ContactFormSubtitle"] = contactSettings.FormSubtitle;
        ViewData["SupportEmail"] = contactSettings.SupportEmail;
        ViewData["SupportPhone"] = contactSettings.SupportPhone;
        ViewData["SupportHours"] = contactSettings.SupportHours;
    }
}
