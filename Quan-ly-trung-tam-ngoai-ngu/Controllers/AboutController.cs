using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        var model = new AboutPageViewModel
        {
            Title = "Giới thiệu trung tâm",
            Subtitle = "NorthStar English định hướng đào tạo chuẩn đầu ra, quản trị minh bạch và trải nghiệm học tập rõ ràng.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Giới thiệu", IsActive = true }],
            Achievements =
            [
                new SummaryCardViewModel { Title = "10+ năm", Value = "2015 - 2026", Description = "Kinh nghiệm đào tạo ngoại ngữ cho sinh viên và người đi làm", Icon = "bi-building", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "95%", Value = "Tỷ lệ hài lòng", Description = "Theo khảo sát nội bộ sau khóa học", Icon = "bi-hand-thumbs-up", AccentClass = "success" },
                new SummaryCardViewModel { Title = "1200+", Value = "Học viên/năm", Description = "Từ các chương trình TOEIC, IELTS và giao tiếp", Icon = "bi-mortarboard", AccentClass = "info" }
            ],
            CoreValues =
            [
                "Lấy trải nghiệm học viên làm trọng tâm",
                "Quản trị lớp học minh bạch và kịp thời",
                "Tối ưu quy trình tuyển sinh, ghi danh và học phí",
                "Sẵn sàng mở rộng dữ liệu và vận hành ở các giai đoạn tiếp theo"
            ],
            Advantages =
            [
                "Lộ trình học rõ ràng theo đầu ra",
                "Báo cáo tiến độ cho giáo vụ và giáo viên",
                "Quản lý học phí, công nợ và biên nhận trên cùng hệ thống",
                "Dễ mở rộng thêm phân hệ khảo thí và CRM sau này"
            ]
        };

        return View(model);
    }
}
