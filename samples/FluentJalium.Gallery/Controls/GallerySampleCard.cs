using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Resources;
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
        UIElement? output = null,
        UIElement? options = null,
        string? code = null,
        double width = 600)
    {
        var mainContent = CreateThreeColumnLayout(sample, output, options);

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
                mainContent
            }
        };

        if (!string.IsNullOrWhiteSpace(code))
        {
            sections.Children.Add(CreateRegion(Strings.SampleCard_Code, CreateCodeBlock(code)));
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

    private static UIElement CreateThreeColumnLayout(UIElement example, UIElement? output, UIElement? options)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var exampleBorder = new FWBorder
        {
            Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
            BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            MinHeight = 120,
            Child = example
        };
        Grid.SetColumn(exampleBorder, 0);
        grid.Children.Add(exampleBorder);

        if (output != null)
        {
            var outputBorder = new FWBorder
            {
                Background = GalleryThemeResources.Brush("ControlBackground"),
                BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(8, 0, 0, 0),
                MinWidth = 140,
                MaxWidth = 180,
                Child = new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 6,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = Strings.SampleCard_Output,
                            FontSize = 12,
                            Foreground = GalleryThemeResources.Brush("TextPrimary"),
                            FontWeight = FontWeights.SemiBold
                        },
                        output
                    }
                }
            };
            Grid.SetColumn(outputBorder, 1);
            grid.Children.Add(outputBorder);
        }

        if (options != null)
        {
            var optionsBorder = new FWBorder
            {
                Background = GalleryThemeResources.Brush("CardBackgroundFillColorDefaultBrush"),
                BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(8, 0, 0, 0),
                MinWidth = 160,
                MaxWidth = 220,
                Child = new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 6,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = Strings.SampleCard_Options,
                            FontSize = 12,
                            Foreground = GalleryThemeResources.Brush("TextPrimary"),
                            FontWeight = FontWeights.SemiBold
                        },
                        options
                    }
                }
            };
            Grid.SetColumn(optionsBorder, 2);
            grid.Children.Add(optionsBorder);
        }

        return grid;
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
                    Foreground = GalleryThemeResources.Brush("TextSecondary"),
                    FontWeight = FontWeights.SemiBold
                },
                content
            }
        };
    }

    private static UIElement CreateCodeBlock(string code)
    {
        return new FWBorder
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
