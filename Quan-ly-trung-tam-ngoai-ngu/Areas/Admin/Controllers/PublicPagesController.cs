using Microsoft.AspNetCore.Mvc;
using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.ViewModels.Common;

namespace Quan_ly_trung_tam_ngoai_ngu.Areas.Admin.Controllers;

public class PublicPagesController : AdminControllerBase
{
    private readonly IPublicSiteContentService _publicSiteContentService;

    public PublicPagesController(ILanguageCenterReadService dataService, IPublicSiteContentService publicSiteContentService)
        : base(dataService)
    {
        _publicSiteContentService = publicSiteContentService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(HomePage));
    }

    [HttpGet]
    public IActionResult HomePage()
    {
        var settings = _publicSiteContentService.GetSiteSettings();
        return ManagementFormView(BuildHomePageForm(new PublicHomePageInput
        {
            HeroTitle = settings.HomePage.HeroTitle,
            HeroSubtitle = settings.HomePage.HeroSubtitle
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult HomePage(PublicHomePageInput input)
    {
        var result = _publicSiteContentService.SaveHomePageContent(input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildHomePageForm(input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(HomePage));
    }

    [HttpGet]
    public IActionResult AboutPage()
    {
        var settings = _publicSiteContentService.GetSiteSettings();
        return ManagementFormView(BuildAboutPageForm(new PublicAboutPageInput
        {
            SectionTitle = settings.AboutSection.SectionTitle,
            SectionSubtitle = settings.AboutSection.SectionSubtitle,
            HighlightTitle = settings.AboutSection.HighlightTitle,
            HighlightBody = settings.AboutSection.HighlightBody
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AboutPage(PublicAboutPageInput input)
    {
        var result = _publicSiteContentService.SaveAboutPageContent(input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildAboutPageForm(input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(AboutPage));
    }

    [HttpGet]
    public IActionResult ContactPage()
    {
        var settings = _publicSiteContentService.GetSiteSettings();
        return ManagementFormView(BuildContactPageForm(new PublicContactPageInput
        {
            SectionTitle = settings.ContactSection.SectionTitle,
            SectionSubtitle = settings.ContactSection.SectionSubtitle,
            FormTitle = settings.ContactSection.FormTitle,
            FormSubtitle = settings.ContactSection.FormSubtitle,
            SupportEmail = settings.ContactSection.SupportEmail,
            SupportPhone = settings.ContactSection.SupportPhone,
            SupportHours = settings.ContactSection.SupportHours
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContactPage(PublicContactPageInput input)
    {
        var result = _publicSiteContentService.SaveContactPageContent(input);
        if (!result.Succeeded)
        {
            return ManagementFormView(BuildContactPageForm(input, result.Message));
        }

        SetToast(result.Message);
        return RedirectToAction(nameof(ContactPage));
    }

    private static ManagementFormPageViewModel BuildHomePageForm(PublicHomePageInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = "Trang chủ công khai",
            Subtitle = "Cập nhật phần mở đầu mà người dùng nhìn thấy đầu tiên trước khi đăng nhập.",
            Breadcrumbs = Breadcrumbs("Trang chủ công khai"),
            FormTitle = "Nội dung phần Trang chủ",
            FormDescription = "Các thay đổi tại đây sẽ hiển thị ngay trên khối hero của trang chủ public.",
            FormActionUrl = "/Admin/PublicPages/HomePage",
            CancelUrl = "/Admin",
            SubmitLabel = "Lưu trang chủ",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin hero",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Tiêu đề chính", Name = "HeroTitle", Value = input.HeroTitle, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Mô tả ngắn", Name = "HeroSubtitle", Value = input.HeroSubtitle, Type = "textarea", Required = true, ColClass = "col-12" }
                    ]
                }
            ]
        };
    }

    private static ManagementFormPageViewModel BuildAboutPageForm(PublicAboutPageInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = "Giới thiệu công khai",
            Subtitle = "Cập nhật phần giới thiệu và khối lợi thế xuất hiện trên trang chủ.",
            Breadcrumbs = Breadcrumbs("Giới thiệu công khai"),
            FormTitle = "Nội dung phần Giới thiệu",
            FormDescription = "Phần này dùng để chỉnh tiêu đề, mô tả và khối nổi bật trong mục Giới thiệu.",
            FormActionUrl = "/Admin/PublicPages/AboutPage",
            CancelUrl = "/Admin",
            SubmitLabel = "Lưu giới thiệu",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin hiển thị",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Tiêu đề phần", Name = "SectionTitle", Value = input.SectionTitle, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Mô tả phần", Name = "SectionSubtitle", Value = input.SectionSubtitle, Type = "textarea", Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Tiêu đề khối nổi bật", Name = "HighlightTitle", Value = input.HighlightTitle, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Nội dung khối nổi bật", Name = "HighlightBody", Value = input.HighlightBody, Type = "textarea", Required = true, ColClass = "col-12" }
                    ]
                }
            ]
        };
    }

    private static ManagementFormPageViewModel BuildContactPageForm(PublicContactPageInput input, string? errorMessage = null)
    {
        return new ManagementFormPageViewModel
        {
            Title = "Liên hệ công khai",
            Subtitle = "Cập nhật phần liên hệ, thông tin hỗ trợ và khối biểu mẫu trên trang chủ.",
            Breadcrumbs = Breadcrumbs("Liên hệ công khai"),
            FormTitle = "Nội dung phần Liên hệ",
            FormDescription = "Tại đây bạn có thể chỉnh thông tin hỗ trợ mà người dùng nhìn thấy trước khi đăng nhập.",
            FormActionUrl = "/Admin/PublicPages/ContactPage",
            CancelUrl = "/Admin",
            SubmitLabel = "Lưu liên hệ",
            ErrorMessage = errorMessage,
            Sections =
            [
                new FormSectionViewModel
                {
                    Title = "Thông tin chung",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Tiêu đề phần", Name = "SectionTitle", Value = input.SectionTitle, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Mô tả phần", Name = "SectionSubtitle", Value = input.SectionSubtitle, Type = "textarea", Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Tiêu đề biểu mẫu", Name = "FormTitle", Value = input.FormTitle, Required = true, ColClass = "col-12" },
                        new FormFieldViewModel { Label = "Mô tả biểu mẫu", Name = "FormSubtitle", Value = input.FormSubtitle, Type = "textarea", Required = true, ColClass = "col-12" }
                    ]
                },
                new FormSectionViewModel
                {
                    Title = "Thông tin hỗ trợ",
                    Fields =
                    [
                        new FormFieldViewModel { Label = "Email hỗ trợ", Name = "SupportEmail", Value = input.SupportEmail, Required = true },
                        new FormFieldViewModel { Label = "Số điện thoại", Name = "SupportPhone", Value = input.SupportPhone, Required = true },
                        new FormFieldViewModel { Label = "Khung giờ hỗ trợ", Name = "SupportHours", Value = input.SupportHours, Required = true, ColClass = "col-12" }
                    ]
                }
            ]
        };
    }
}
