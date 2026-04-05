using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class NewsController : AdminControllerBase
{
    private readonly IPublicSiteContentService _publicSiteContentService;

    public NewsController(ILanguageCenterReadService dataService, IPublicSiteContentService publicSiteContentService)
        : base(dataService)
    {
        _publicSiteContentService = publicSiteContentService;
    }

    public IActionResult Index()
    {
        var articles = _publicSiteContentService.GetNewsArticles()
            .OrderByDescending(item => item.IsFeatured)
            .ThenByDescending(item => item.PublishedOn)
            .ToList();

        return ManagementListView(new ManagementListPageViewModel
        {
            Title = "Tin tức công khai",
            Subtitle = "Quản lý các bài viết sẽ hiển thị ở trang chủ và trang tin tức trước khi người dùng đăng nhập.",
            Breadcrumbs = Breadcrumbs("Tin tức công khai"),
            PrimaryActionText = "Thêm tin tức",
            PrimaryActionUrl = "/Admin/News/Create",
            SearchPlaceholder = "Tìm theo tiêu đề, chuyên mục, tác giả hoặc slug...",
            ToolbarNote = "Tin tức được lưu tại hệ thống quản trị nội dung công khai và hiển thị ngay ra trang chủ sau khi lưu.",
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Tổng bài viết", Value = articles.Count.ToString(), Description = "Số bài viết đang công khai", Icon = "bi-newspaper", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Tin nổi bật", Value = articles.Count(item => item.IsFeatured).ToString(), Description = "Ưu tiên hiển thị ở trang chủ", Icon = "bi-stars", AccentClass = "success" },
                new SummaryCardViewModel { Title = "Mới nhất", Value = articles.FirstOrDefault()?.PublishedOn.ToString("dd/MM/yyyy") ?? "Chưa có", Description = "Ngày đăng bài gần nhất", Icon = "bi-calendar-event", AccentClass = "info" }
            ],
            Table = new TableViewModel
            {
                Columns =
                [
                    new() { Header = "Bài viết" },
                    new() { Header = "Chuyên mục" },
                    new() { Header = "Ngày đăng" },
                    new() { Header = "Trạng thái" },
                    new() { Header = "Thao tác", Width = "280px" }
                ],
                Rows = articles.Select(article => new TableRowViewModel
                {
                    Id = article.Id.ToString(),
                    Cells =
                    [
                        new() { Html = $"<strong>{article.Title}</strong><div class='text-muted small'>{article.Slug} • {article.Author}</div>" },
                        new() { Html = article.Category },
                        new() { Html = article.PublishedOn.ToString("dd/MM/yyyy") },
                        new() { Html = article.IsFeatured ? "<span class='badge bg-info-subtle text-info-emphasis'>Nổi bật</span>" : "<span class='badge bg-secondary-subtle text-secondary-emphasis'>Thường</span>" },
                        new() { Html = string.Empty }
                    ],
                    Actions =
                    [
                        new() { Label = "Chi tiết", Url = $"/Admin/News/Details/{article.Id}", Icon = "bi-eye" },
                        new() { Label = "Sửa", Url = $"/Admin/News/Edit/{article.Id}", Icon = "bi-pencil-square", CssClass = "btn btn-sm btn-outline-secondary" },
                        new() { Label = "Xóa", Url = $"/Admin/News/Delete/{article.Id}", Icon = "bi-trash", CssClass = "btn btn-sm btn-outline-danger", RequiresConfirm = true, ConfirmMessage = "Bạn muốn xóa bài viết này khỏi trang công khai?" }
                    ]
                }).ToList()
            }
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return ManagementFormView(BuildNewsForm("Thêm tin tức", "/Admin/News/Create", new NewsArticleInput
        {
            PublishedOn = DateTime.Today,
            Author = "Phòng truyền thông"
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NewsArticleInput input)
    {
        var result = _publicSiteContentService.SaveNewsArticle(null, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildNewsForm("Thêm tin tức", "/Admin/News/Create", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var article = _publicSiteContentService.GetNewsArticle(id);
        if (article is null)
        {
            SetToast("Không tìm thấy bài viết.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementFormView(BuildNewsForm("Cập nhật tin tức", $"/Admin/News/Edit/{id}", new NewsArticleInput
        {
            Slug = article.Slug,
            Title = article.Title,
            Category = article.Category,
            Summary = article.Summary,
            Content = article.Content,
            Author = article.Author,
            PublishedOn = article.PublishedOn,
            IsFeatured = article.IsFeatured
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, NewsArticleInput input)
    {
        var result = _publicSiteContentService.SaveNewsArticle(id, input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildNewsForm("Cập nhật tin tức", $"/Admin/News/Edit/{id}", input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var result = _publicSiteContentService.DeleteNewsArticle(id);
        SetToast(result.Message, result.Succeeded ? "success" : "danger");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var article = _publicSiteContentService.GetNewsArticle(id);
        if (article is null)
        {
            SetToast("Không tìm thấy bài viết.", "danger");
            return RedirectToAction(nameof(Index));
        }

        return ManagementDetailsView(new ManagementDetailsPageViewModel
        {
            Title = article.Title,
            Subtitle = "Xem trước nội dung bài viết đang hiển thị ở khu vực công khai.",
            Breadcrumbs = Breadcrumbs("Chi tiết tin tức", "Tin tức công khai", "/Admin/News"),
            SummaryCards =
            [
                new SummaryCardViewModel { Title = "Chuyên mục", Value = article.Category, Description = "Nhóm nội dung công khai", Icon = "bi-bookmark-star", AccentClass = "primary" },
                new SummaryCardViewModel { Title = "Ngày đăng", Value = article.PublishedOn.ToString("dd/MM/yyyy"), Description = "Thời điểm công bố bài viết", Icon = "bi-calendar-event", AccentClass = "info" },
                new SummaryCardViewModel { Title = "Hiển thị", Value = article.IsFeatured ? "Nổi bật" : "Thường", Description = article.IsFeatured ? "Ưu tiên hiển thị ở trang chủ" : "Hiển thị theo danh sách tin tức", Icon = "bi-stars", AccentClass = article.IsFeatured ? "success" : "warning" }
            ],
            Sections =
            [
                new DetailSectionViewModel
                {
                    Title = "Thông tin xuất bản",
                    Items =
                    [
                        new() { Label = "Slug", Value = article.Slug },
                        new() { Label = "Tác giả", Value = article.Author },
                        new() { Label = "Chuyên mục", Value = article.Category },
                        new() { Label = "Tóm tắt", Value = article.Summary }
                    ]
                },
                new DetailSectionViewModel
                {
                    Title = "Nội dung bài viết",
                    Items =
                    [
                        new() { Label = "Nội dung", Value = article.Content }
                    ]
                }
            ],
            Actions =
            [
                new QuickActionViewModel { Label = "Xem ngoài trang công khai", Url = $"/News/Details/{article.Slug}", Icon = "bi-box-arrow-up-right" },
                new QuickActionViewModel { Label = "Sửa bài viết", Url = $"/Admin/News/Edit/{id}", Icon = "bi-pencil-square", CssClass = "btn btn-outline-primary" },
                new QuickActionViewModel { Label = "Quay lại", Url = "/Admin/News", Icon = "bi-arrow-left", CssClass = "btn btn-outline-secondary" }
            ]
        });
    }

    private static ManagementFormPageViewModel BuildNewsForm(string title, string actionUrl, NewsArticleInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = title,
            Subtitle = "Bài viết lưu tại đây sẽ hiển thị cho người dùng ở trang chủ và trang tin tức trước khi đăng nhập.",
            Breadcrumbs = Breadcrumbs(title, "Tin tức công khai", "/Admin/News"),
            FormTitle = title,
            FormDescription = "Quản trị nội dung tin tức công khai cho website trung tâm.",
            FormActionUrl = actionUrl,
            CancelUrl = "/Admin/News",
            SubmitLabel = "Lưu bài viết",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin bài viết",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Tiêu đề", Name = "Title", Value = input.Title, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Slug", Name = "Slug", Value = input.Slug, Hint = "Có thể để trống để hệ thống tự tạo từ tiêu đề." },
                        new FormFieldViewModel { Label = "Chuyên mục", Name = "Category", Value = input.Category, Required = true },
                        new FormFieldViewModel { Label = "Tác giả", Name = "Author", Value = input.Author, Required = true },
                        new FormFieldViewModel { Label = "Ngày đăng", Name = "PublishedOn", Value = input.PublishedOn.ToString("yyyy-MM-dd"), Type = "date", Required = true },
                        new FormFieldViewModel
                        {
                            Label = "Hiển thị nổi bật",
                            Name = "IsFeatured",
                            Type = "select",
                            Options =
                            [
                                new SelectOptionViewModel { Label = "Bài viết thường", Value = "false", Selected = !input.IsFeatured },
                                new SelectOptionViewModel { Label = "Tin nổi bật", Value = "true", Selected = input.IsFeatured }
                            ]
                        },
                        new FormFieldViewModel { Label = "Tóm tắt", Name = "Summary", Value = input.Summary, Type = "textarea", ColClass = "col-12", Required = true },
                        new FormFieldViewModel { Label = "Nội dung", Name = "Content", Value = input.Content, Type = "textarea", ColClass = "col-12", Required = true }
                    ]
                }
            ]
        };
    }
}
