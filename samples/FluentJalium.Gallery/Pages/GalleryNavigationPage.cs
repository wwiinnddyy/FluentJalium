using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Navigation shell: WinUI structure with the FW surfaces that already exist.
/// Full WinUI sample migration (breadcrumb, pips, selector bar, tab view, title bar)
/// lands in Phase 6; until then this page renders the shared preview shell plus the
/// covered FW surface index.
/// </summary>
internal sealed class GalleryNavigationPage
{
    public UIElement CreateContent(GalleryPageInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Children =
            {
                GalleryPlaceholderPage.Create(info),
                CreateSurfaceIndex(info)
            }
        };
    }

    private static UIElement CreateSurfaceIndex(GalleryPageInfo info)
    {
        var pills = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var control in info.RelatedControls)
        {
            pills.Children.Add(new FWBorder
            {
                Background = GalleryThemeResources.Brush("ControlBackground"),
                BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999),
                Padding = new Thickness(10, 5, 10, 5),
                Child = new FWTextBlock
                {
                    Text = control,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        return pills;
    }
}
