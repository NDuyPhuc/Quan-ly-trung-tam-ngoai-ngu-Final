using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Teacher.Controllers;

public class ExamsController : TeacherControllerBase
{
    private readonly ILanguageCenterManagementService _managementService;

    public ExamsController(
        ILanguageCenterReadService dataService,
        ILanguageCenterManagementService managementService) : base(dataService)
    {
        _managementService = managementService;
    }

    public IActionResult Index()
    {
        var classCodes = GetTeacherClassCodes();
        var exams = DataService.GetExams()
            .Where(x => classCodes.Contains(x.ClassCode))
            .OrderByDescending(x => x.ExamDate)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Bài kiểm tra",
            Subtitle = "Giáo viên tạo lịch kiểm tra trước khi nhập điểm cho học viên trong lớp phụ trách.",
            Breadcrumbs = Breadcrumbs("Bài kiểm tra"),
            PrimaryActionText = "Tạo bài kiểm tra",
            PrimaryActionUrl = "/Teacher/Exams/Create",
            SearchPlaceholder = "Tìm theo lớp, tên bài kiểm tra hoặc loại kiểm tra...",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng bài kiểm tra", Value = exams.Count.ToString(), Description = "Theo lớp đang phụ trách", Icon = "bi-journal-text", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Đã nhập điểm", Value = exams.Count(x => x.ResultCount > 0).ToString(), Description = "Đã phát sinh kết quả", Icon = "bi-clipboard2-check", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Chờ nhập điểm", Value = exams.Count(x => x.ResultCount == 0).ToString(), Description = "Sẵn sàng thao tác tiếp", Icon = "bi-hourglass-split", AccentClass = "warning" }
            ],
            Table = new TableViewModel
            {
                Columns = [new() { Header = "Bài kiểm tra" }, new() { Header = "Lớp học" }, new() { Header = "Ngày thi" }, new() { Header = "Kết quả" }, new() { Header = "Thao tác", Width = "280px" }],
                Rows = exams.Select(item => new TableRowViewModel
                {
                    Id = item.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{item.ExamName}</strong><div class='text-muted small'>{item.ExamType}</div>" },
                        new() { Html = $"<strong>{item.ClassCode}</strong><div class='text-muted small'>{item.CourseName}</div>" },
                        new() { Html = $"{item.ExamDate:dd/MM/yyyy}<div class='text-muted small'>Tối đa {item.MaxScore:0.##}</div>" },
                        new() { Html = AppUi.StatusBadge(item.Status) + $"<div class='text-muted small mt-1'>{item.ResultCount} kết quả</div>" },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Teacher/Exams/Details/{item.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Teacher/Exams/Edit/{item.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Nhập điểm", Url = "/Teacher/ExamResults/Create", Icon = "bi-clipboard2-data", CssClass = "btn btn-sm btn-outline-primary" },
                        new() { Label = "Xóa", Url = $"/Teacher/Exams/Delete/{item.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger confirm-action", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa bài kiểm tra này?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildExamForm("Tạo bài kiểm tra", "/Teacher/Exams/Create", new ExamInput { ExamDate = DateTime.Today, MaxScore = 10 }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(ExamInput input)
    {
        var result = _managementService.SaveExam(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Tạo bài kiểm tra", "/Teacher/Exams/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var input = _managementService.GetExam(id);
        if (input is null || !GetTeacherClassCodes().Contains(input.ClassCode))
        {
            SetToast("Không tìm thấy bài kiểm tra.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildExamForm("Cập nhật bài kiểm tra", $"/Teacher/Exams/Edit/{id}", input));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, ExamInput input)
    {
        if (!GetTeacherClassCodes().Contains(input.ClassCode))
        {
            return ManagementFormView(BuildExamForm("Cập nhật bài kiểm tra", $"/Teacher/Exams/Edit/{id}", input, "Bạn chỉ có thể quản lý bài kiểm tra của lớp được phân công."));
        }

        var result = _managementService.SaveExam(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildExamForm("Cập nhật bài kiểm tra", $"/Teacher/Exams/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var input = _managementService.GetExam(id);
        if (input is null || !GetTeacherClassCodes().Contains(input.ClassCode))
        {
            SetToast("Không tìm thấy bài kiểm tra.", "danger");
            return RedirectToAction(nameof(Index));
        }

        var result = _managementService.DeleteExam(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var item = DataService.GetExams()
            .Where(x => GetTeacherClassCodes().Contains(x.ClassCode))
            .FirstOrDefault(x => x.Id == id);

        if (item is null)
        {
            SetToast("Không tìm thấy bài kiểm tra.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = item.ExamName,
            Subtitle = $"{item.ClassCode} • {item.ExamType}",
            Breadcrumbs = Breadcrumbs("Chi tiết bài kiểm tra", "Bài kiểm tra", "/Teacher/Exams"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Lớp học", Value = item.ClassCode, Description = item.CourseName, Icon = "bi-easel2", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Số kết quả", Value = item.ResultCount.ToString(), Description = "Dòng điểm đã nhập", Icon = "bi-clipboard2-check", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Điểm trung bình", Value = item.ResultCount == 0 ? "Chưa có" : item.AverageScore.ToString("0.0"), Description = "Tính từ kết quả hiện có", Icon = "bi-graph-up", AccentClass = "success" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin bài kiểm tra",
                    Items =
                    [
                        new() { Label = "Tên bài kiểm tra", Value = item.ExamName },
                        new() { Label = "Loại bài kiểm tra", Value = item.ExamType, IsBadge = true, BadgeClass = "bg-primary-subtle text-primary-emphasis" },
                        new() { Label = "Ngày thi", Value = item.ExamDate.ToString("dd/MM/yyyy") },
                        new() { Label = "Điểm tối đa", Value = item.MaxScore.ToString("0.##") },
                        new() { Label = "Trạng thái", Value = item.Status, IsBadge = true, BadgeClass = AppUi.StatusBadgeClass(item.Status) }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Nhập điểm", Url = "/Teacher/ExamResults/Create", Icon = "bi-clipboard2-data" },
                new QuickActionViewModel { Label = "Sửa bài kiểm tra", Url = $"/Teacher/Exams/Edit/{id}", Icon = "bi-pencil-square", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Teacher/Exams", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private ManagementFormPageViewModel BuildExamForm(string title, string actionUrl, ExamInput input, string? errorMessage = null)
    {
        var classOptions = GetTeacherClasses()
            .OrderBy(x => x.Code)
            .Select(item => new SelectOptionViewModel
            {
                Label = $"{item.Code} - {item.CourseName}",
                Value = item.Code,
                Selected = item.Code == input.ClassCode
            })
            .ToList();

        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Tạo riêng bài kiểm tra để việc nhập điểm và chụp màn hình báo cáo rõ ràng hơn.",
            Breadcrumbs = Breadcrumbs(title, "Bài kiểm tra", "/Teacher/Exams"),
            FormTitle = title,
            FormDescription = "Thông tin sẽ được lưu vào bảng Exams. Kết quả điểm số sẽ được nhập ở module ExamResults.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Teacher/Exams",
            SubmitLabel = "Lưu bài kiểm tra",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin bài kiểm tra",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Lớp học", Name = "ClassCode", Type = "select", Required = true, Options = classOptions },
                        new FormFieldViewModel { Label = "Tên bài kiểm tra", Name = "ExamName", Value = input.ExamName, Required = true, ColClass = "col-md-6" },
                        new FormFieldViewModel { Label = "Ngày thi", Name = "ExamDate", Value = input.ExamDate.ToString("yyyy-MM-dd"), Type = "date", Required = true, ColClass = "col-md-3" },
                        new FormFieldViewModel { Label = "Điểm tối đa", Name = "MaxScore", Value = input.MaxScore.ToString("0.##"), Type = "number", Required = true, ColClass = "col-md-3" },
                        new FormFieldViewModel
                        {
                            Label = "Loại bài kiểm tra",
                            Name = "ExamType",
                            Type = "select",
                            Required = true,
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Giữa kỳ", Value = "Midterm", Selected = input.ExamType == "Midterm" },
                                new SelectOptionViewModel { Label = "Cuối kỳ", Value = "Final", Selected = input.ExamType == "Final" },
                                new SelectOptionViewModel { Label = "Nói", Value = "Speaking", Selected = input.ExamType == "Speaking" },
                                new SelectOptionViewModel { Label = "Kiểm tra", Value = "Test", Selected = input.ExamType == "Test" || string.IsNullOrWhiteSpace(input.ExamType) }
                            ]
                        }
                    ]
                }
            ]
        };
    }
}
