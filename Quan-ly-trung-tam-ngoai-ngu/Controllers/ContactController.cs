using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class ContactController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(CreateModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ContactViewModel model)
    {
        model.Title = "Liên hệ";
        model.Subtitle = "Để lại thông tin để trung tâm tư vấn lộ trình phù hợp.";
        model.Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Liên hệ", IsActive = true }];

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData[AppConstants.ToastMessageKey] = "Trung tâm đã ghi nhận yêu cầu tư vấn của bạn và sẽ phản hồi sớm.";
        TempData[AppConstants.ToastTypeKey] = "success";
        return RedirectToAction(nameof(Index));
    }

    private static ContactViewModel CreateModel()
    {
        return new ContactViewModel
        {
            Title = "Liên hệ",
            Subtitle = "Để lại thông tin để trung tâm tư vấn lộ trình phù hợp.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Liên hệ", IsActive = true }]
        };
    }
}
