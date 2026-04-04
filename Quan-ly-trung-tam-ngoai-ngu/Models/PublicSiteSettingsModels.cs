namespace Quan_ly_trung_tam_ngoai_ngu.Models;

public sealed class PublicSiteSettings
{
    public PublicHomeSectionContent HomePage { get; set; } = new();
    public PublicAboutSectionContent AboutSection { get; set; } = new();
    public PublicContactSectionContent ContactSection { get; set; } = new();
}

public sealed class PublicHomeSectionContent
{
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
}

public sealed class PublicAboutSectionContent
{
    public string SectionTitle { get; set; } = string.Empty;
    public string SectionSubtitle { get; set; } = string.Empty;
    public string HighlightTitle { get; set; } = string.Empty;
    public string HighlightBody { get; set; } = string.Empty;
}

public sealed class PublicContactSectionContent
{
    public string SectionTitle { get; set; } = string.Empty;
    public string SectionSubtitle { get; set; } = string.Empty;
    public string FormTitle { get; set; } = string.Empty;
    public string FormSubtitle { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string SupportHours { get; set; } = string.Empty;
}
