using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryNavigationOverviewPage : Page
{
    public GalleryNavigationOverviewPage()
    {
        Content = GalleryNavigationPageContent.Create(
            "Overview",
            "Frame content can preserve navigation state and theme resources.");
    }
}

internal sealed class GalleryNavigationDetailsPage : Page
{
    public GalleryNavigationDetailsPage()
    {
        Content = GalleryNavigationPageContent.Create(
            "Details",
            "Back and forward navigation update the same Fluent frame surface.");
    }
}

internal static class GalleryNavigationPageContent
{
    public static UIElement Create(string title, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 18,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new FWTextBlock
                {
                    Text = text,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
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
