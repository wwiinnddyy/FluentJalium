using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Controls;

internal static class GalleryPageSection
{
    public static FWStackPanel Create(string title, FluentIconRegular icon)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = GalleryLayoutTokens.SectionTitleSpacing,
                    Children =
                    {
                        FluentIconFactory.Regular(icon, GalleryLayoutTokens.SectionTitleIconSize, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = GalleryLayoutTokens.SectionTitleFontSize,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static Brush ThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Transparent);
    }
}
