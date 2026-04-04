using Quan_ly_trung_tam_ngoai_ngu.Models;

namespace Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;

public interface IPublicSiteContentService
{
    PublicSiteSettings GetSiteSettings();
    ManagementResult SaveHomePageContent(PublicHomePageInput input);
    ManagementResult SaveAboutPageContent(PublicAboutPageInput input);
    ManagementResult SaveContactPageContent(PublicContactPageInput input);
    IReadOnlyList<NewsArticle> GetNewsArticles();
    NewsArticle? GetNewsArticle(int id);
    ManagementResult SaveNewsArticle(int? id, NewsArticleInput input);
    ManagementResult DeleteNewsArticle(int id);
}
