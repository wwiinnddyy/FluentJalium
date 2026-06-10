using Jalium.UI;

namespace FluentJalium.Gallery.Services;

internal static class GalleryLayoutTokens
{
    public const double PageHeaderFontSize = 36;
    public const string PageHeaderFontFamily = "Segoe UI Variable Display";
    public const double SectionTitleFontSize = 20;
    public const double SectionSpacing = 36;
    public static Thickness ContentMargin => new(36, 40, 36, 40);
    public const double CardMaxWidth = 1000;
    public const double CardCornerRadius = 8;
    public const double SectionTitleIconSize = 22;
    public const double SectionTitleSpacing = 10;
}
