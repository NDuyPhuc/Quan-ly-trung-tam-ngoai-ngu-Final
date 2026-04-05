using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class CoursesController : Controller
{
    private readonly ILanguageCenterReadService _dataService;

    public CoursesController(ILanguageCenterReadService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var model = new CourseCatalogPageViewModel
        {
            Title = "Danh sách khóa học",
            Subtitle = "Danh mục khóa học theo mục tiêu đầu ra, học phí rõ ràng và lịch học linh hoạt.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Khóa học", IsActive = true }],
            Filters =
            [
                new FilterGroupViewModel { Label = "Trình độ", InputId = "level", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Sơ cấp", Value = "beginner" }, new() { Label = "Trung cấp", Value = "intermediate" }, new() { Label = "Nâng cao", Value = "advanced" }] },
                new FilterGroupViewModel { Label = "Học phí", InputId = "tuition", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Dưới 6 triệu", Value = "under-6" }, new() { Label = "6 - 10 triệu", Value = "6-10" }, new() { Label = "Trên 10 triệu", Value = "10plus" }] },
                new FilterGroupViewModel { Label = "Trạng thái", InputId = "status", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Đang tuyển sinh", Value = "active" }, new() { Label = "Khai giảng sớm", Value = "soon" }, new() { Label = "Sắp mở lớp", Value = "coming" }] }
            ],
            Courses = _dataService.GetCourses().Select(AppUi.ToCourseCard).ToList()
        };

        return View(model);
    }

    public IActionResult Details(string id)
    {
        var course = _dataService.GetCourses().FirstOrDefault(item => item.Slug == id);
        if (course is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new CourseDetailPageViewModel
        {
            Title = course.Name,
            Subtitle = course.ShortDescription,
            Breadcrumbs = AppUi.Breadcrumbs(("Khóa học", Url.Action(nameof(Index), "Courses"), false), (course.Name, null, true)),
            Course = AppUi.ToCourseCard(course),
            Description = course.Description,
            TargetOutput = course.TargetOutput,
            Objectives = course.Objectives,
            Highlights = course.Highlights,
            RelatedClasses = _dataService.GetClasses().Where(item => item.CourseName == course.Name).Select(AppUi.ToClassCard).ToList()
        };

        return View(model);
    }
}
