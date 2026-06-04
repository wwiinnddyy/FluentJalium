using FluentJalium.Gallery.Services;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;

namespace FluentJalium.Gallery.Controls;

internal static class GallerySampleCard
{
    public static FWBorder Create(
        FluentIconRegular icon,
        string title,
        string description,
        UIElement sample,
        UIElement? states = null,
        UIElement? properties = null,
        string? code = null,
        double width = 570)
    {
        var sections = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateHeader(icon, title),
                new FWTextBlock
                {
                    Text = description,
                    FontSize = 12,
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                },
                CreateRegion("Example", sample)
            }
        };

        if (states != null)
        {
            sections.Children.Add(CreateRegion("States", states));
        }

        if (properties != null)
        {
            sections.Children.Add(CreateRegion("Properties", properties));
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            sections.Children.Add(CreateRegion("Code", CreateCodeBlock(code)));
        }

        return new FWBorder
        {
            Width = width,
            Background = GalleryThemeResources.Brush("ControlBackground"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = sections
        };
    }

    private static UIElement CreateHeader(FluentIconRegular icon, string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                FluentIconFactory.Regular(icon, 20, GalleryThemeResources.Brush("TextPrimary")),
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 15,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateRegion(string label, UIElement content)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 6,
            Children =
            {
                new FWTextBlock
                {
                    Text = label,
                    FontSize = 11,
                    Foreground = GalleryThemeResources.Brush("TextSecondary")
                },
                content
            }
        };
    }

    private static UIElement CreateCodeBlock(string code)
    {
        return new Border
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWTextBlock
            {
                Text = code,
                FontFamily = "Cascadia Code",
                FontSize = 12,
                Foreground = GalleryThemeResources.Brush("TextPrimary"),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }
}
