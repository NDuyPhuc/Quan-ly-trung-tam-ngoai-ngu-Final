using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Dashboard;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public abstract class ModuleControllerBase : Controller
{
    protected readonly ILanguageCenterReadService DataService;

    protected ModuleControllerBase(ILanguageCenterReadService dataService)
    {
        DataService = dataService;
    }

    public IActionResult DashboardView(DashboardPageViewModel model)
    {
        return View("~/Views/Shared/Modules/Dashboard.cshtml", model);
    }

    public IActionResult ManagementListView(ManagementListPageViewModel model)
    {
        return View("~/Views/Shared/Modules/ManagementList.cshtml", model);
    }

    public IActionResult ManagementFormView(ManagementFormPageViewModel model)
    {
        return View("~/Views/Shared/Modules/ManagementForm.cshtml", model);
    }

    public IActionResult ManagementDetailsView(ManagementDetailsPageViewModel model)
    {
        return View("~/Views/Shared/Modules/ManagementDetails.cshtml", model);
    }

    protected void SetToast(string message, string type = "success")
    {
        TempData[AppConstants.ToastMessageKey] = message;
        TempData[AppConstants.ToastTypeKey] = type;
    }
}
