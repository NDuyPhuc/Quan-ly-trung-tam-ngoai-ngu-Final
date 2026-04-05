using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Controllers;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Staff.Controllers;

[Area("Staff")]
[DemoAuthorize(AppConstants.Roles.Staff)]
public abstract class StaffControllerBase : ModuleControllerBase
{
    protected StaffControllerBase(ILanguageCenterReadService dataService)
        : base(dataService)
    {
    }

    public static List<BreadcrumbItemViewModel> Breadcrumbs(string current, string? previousLabel = null, string? previousUrl = null)
    {
        var items = new List<BreadcrumbItemViewModel>
        {
            new() { Label = "Giáo vụ", Url = "/Staff", IsActive = false }
        };

        if (!string.IsNullOrWhiteSpace(previousLabel))
        {
            items.Add(new BreadcrumbItemViewModel { Label = previousLabel, Url = previousUrl, IsActive = false });
        }

        items.Add(new BreadcrumbItemViewModel { Label = current, IsActive = true });
        return items;
    }
}
