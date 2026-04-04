using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Public;

namespace Quan_ly_trung_tam_ngoai_ngu.Controllers;

public class NewsController : Controller
{
    private readonly IPublicSiteContentService _publicSiteContentService;

    public NewsController(IPublicSiteContentService publicSiteContentService)
    {
        _publicSiteContentService = publicSiteContentService;
    }

    public IActionResult Index()
    {
        var model = new NewsListPageViewModel
        {
            Title = "Tin tức và bài viết",
            Subtitle = "Cập nhật lịch khai giảng, workshop và chia sẻ học tập mới nhất từ trung tâm.",
            Breadcrumbs = [new BreadcrumbItemViewModel { Label = "Tin tức", IsActive = true }],
            Articles = _publicSiteContentService.GetNewsArticles()
                .OrderByDescending(item => item.IsFeatured)
                .ThenByDescending(item => item.PublishedOn)
                .Select(AppUi.ToNewsCard)
                .ToList()
        };

        return View(model);
    }

    public IActionResult Details(string id)
    {
        var article = _publicSiteContentService.GetNewsArticles()
            .FirstOrDefault(item => string.Equals(item.Slug, id, StringComparison.OrdinalIgnoreCase));

        if (article is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new NewsDetailPageViewModel
        {
            Title = article.Title,
            Subtitle = article.Summary,
            Breadcrumbs = AppUi.Breadcrumbs(("Tin tức", Url.Action(nameof(Index), "News"), false), (article.Title, null, true)),
            Article = AppUi.ToNewsCard(article),
            Content = article.Content,
            RelatedArticles = _publicSiteContentService.GetNewsArticles()
                .Where(item => item.Id != article.Id)
                .OrderByDescending(item => item.IsFeatured)
                .ThenByDescending(item => item.PublishedOn)
                .Take(2)
                .Select(AppUi.ToNewsCard)
                .ToList()
        };

        return View(model);
    }
}
