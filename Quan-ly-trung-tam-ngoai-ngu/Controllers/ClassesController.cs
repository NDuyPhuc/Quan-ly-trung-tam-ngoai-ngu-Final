using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class ClassesController : Controller
{
    private readonly IMockDataService _dataService;

    public ClassesController(IMockDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index()
    {
        var model = new ClassCatalogPageViewModel
        {
            Title = "Lớp học đang mở",
            Subtitle = "Theo dõi nhanh lịch học, giáo viên phụ trách, phòng học và số chỗ còn trống.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Lớp học", IsActive = true }],
            Filters =
            [
                new FilterGroupViewModel { Label = "Ca học", InputId = "schedule", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Tối 2-4-6", Value = "246" }, new() { Label = "Tối 3-5-7", Value = "357" }, new() { Label = "Cuối tuần", Value = "weekend" }] },
                new FilterGroupViewModel { Label = "Giáo viên", InputId = "teacher", Options = [new() { Label = "Tất cả", Value = "" }, .. _dataService.GetTeachers().Select(item => new SelectOptionViewModel { Label = item.FullName, Value = item.Id.ToString() })] },
                new FilterGroupViewModel { Label = "Trạng thái", InputId = "status", Options = [new() { Label = "Tất cả", Value = "" }, new() { Label = "Đang hoạt động", Value = "running" }, new() { Label = "Sắp khai giảng", Value = "upcoming" }, new() { Label = "Mở đăng ký", Value = "open" }] }
            ],
            Classes = _dataService.GetClasses().Select(AppUi.ToClassCard).ToList()
        };

        return View(model);
    }
}
