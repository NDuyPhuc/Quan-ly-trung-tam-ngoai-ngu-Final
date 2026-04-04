using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Controllers;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Teacher.Controllers;

[Area("Teacher")]
[DemoAuthorize(AppConstants.Roles.Teacher)]
public abstract class TeacherControllerBase : ModuleControllerBase
{
    protected TeacherControllerBase(IMockDataService dataService)
        : base(dataService)
    {
    }

    protected string CurrentTeacherName =>
        HttpContext.Session.GetString(AppConstants.SessionDemoUserDisplayName) ?? "Giáo viên";

    protected List<CourseClass> GetTeacherClasses()
    {
        var classes = DataService.GetClasses().ToList();
        if (classes.Count == 0)
        {
            return classes;
        }

        var displayName = HttpContext.Session.GetString(AppConstants.SessionDemoUserDisplayName);
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return classes;
        }

        var matchedClasses = classes
            .Where(item => string.Equals(item.TeacherName, displayName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matchedClasses.Count > 0 ? matchedClasses : classes;
    }

    protected HashSet<string> GetTeacherClassCodes()
    {
        return GetTeacherClasses()
            .Select(item => item.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public static List<BreadcrumbItemViewModel> Breadcrumbs(string current, string? previousLabel = null, string? previousUrl = null)
    {
        var items = new List<BreadcrumbItemViewModel>
        {
            new() { Label = "Giáo viên", Url = "/Teacher", IsActive = false }
        };

        if (!string.IsNullOrWhiteSpace(previousLabel))
        {
            items.Add(new BreadcrumbItemViewModel { Label = previousLabel, Url = previousUrl, IsActive = false });
        }

        items.Add(new BreadcrumbItemViewModel { Label = current, IsActive = true });
        return items;
    }
}
