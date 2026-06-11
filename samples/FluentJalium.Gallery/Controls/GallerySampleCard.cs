using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Resources;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWButtonDensity = FluentJalium.Controls.FWButtonDensity;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWExpander = FluentJalium.Controls.FWExpander;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWFluentMaterialRole = FluentJalium.Controls.FWFluentMaterialRole;

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
        double width = double.NaN)
    {
        var mainContent = CreateContentArea(sample, output, options);

        var sections = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Children =
            {
                new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 8,
                    Children =
                    {
                        CreateHeader(icon, title),
                        new FWTextBlock
                        {
                            Text = description,
                            FontSize = 14,
                            Foreground = GalleryThemeResources.Brush("TextSecondary"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                },
                mainContent
            }
        };

        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeExpander = new FWExpander
            {
                Header = new FWStackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children =
                    {
                        FluentIconFactory.Regular(FluentIconRegular.Code24, 16, GalleryThemeResources.Brush("TextPrimary")),
                        new FWTextBlock { Text = Strings.SampleCard_Code, FontSize = 14, Foreground = GalleryThemeResources.Brush("TextPrimary"), VerticalAlignment = VerticalAlignment.Center }
                    }
                },
                Content = CreateCodeBlock(code),
                Margin = new Thickness(0, 8, 0, 0)
            };
            sections.Children.Add(codeExpander);
        }

        return new FWBorder
        {
            Width = width,
            MaxWidth = double.IsNaN(width) ? 1000 : double.NaN,
            HorizontalAlignment = double.IsNaN(width) ? HorizontalAlignment.Stretch : HorizontalAlignment.Left,
            Padding = new Thickness(0, 0, 0, 32),
            Child = sections
        };
    }

    private static UIElement CreateContentArea(UIElement example, UIElement? output, UIElement? options)
    {
        var grid = new Grid();

        var col0 = new ColumnDefinition { Width = GridLength.Star };
        var col1 = new ColumnDefinition { Width = GridLength.Auto };
        var col2 = new ColumnDefinition { Width = GridLength.Auto };
        grid.ColumnDefinitions.Add(col0);
        grid.ColumnDefinitions.Add(col1);
        grid.ColumnDefinitions.Add(col2);

        var row0 = new RowDefinition { Height = GridLength.Auto };
        var row1 = new RowDefinition { Height = GridLength.Auto };
        var row2 = new RowDefinition { Height = GridLength.Auto };
        grid.RowDefinitions.Add(row0);
        grid.RowDefinitions.Add(row1);
        grid.RowDefinitions.Add(row2);

        var exampleContainer = new FWStackPanel
        {
            Margin = new Thickness(24),
            MinHeight = 120,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { example }
        };
        
        Grid.SetColumn(exampleContainer, 0);
        Grid.SetRow(exampleContainer, 0);
        grid.Children.Add(exampleContainer);

        FWBorder? outputContainer = null;
        if (output != null)
        {
            outputContainer = new FWBorder
            {
                Margin = new Thickness(24),
                MinWidth = 140,
                MaxWidth = 200,
                Child = new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 12,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = Strings.SampleCard_Output,
                            FontSize = 14,
                            Foreground = GalleryThemeResources.Brush("TextPrimary"),
                            FontWeight = FontWeights.SemiBold
                        },
                        output
                    }
                }
            };
            Grid.SetColumn(outputContainer, 1);
            Grid.SetRow(outputContainer, 0);
            grid.Children.Add(outputContainer);
        }

        FWBorder? optionsContainer = null;
        if (options != null)
        {
            optionsContainer = new FWBorder
            {
                Margin = new Thickness(24),
                MinWidth = 160,
                MaxWidth = 240,
                Child = new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 12,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = Strings.SampleCard_Options,
                            FontSize = 14,
                            Foreground = GalleryThemeResources.Brush("TextPrimary"),
                            FontWeight = FontWeights.SemiBold
                        },
                        options
                    }
                }
            };
            Grid.SetColumn(optionsContainer, 2);
            Grid.SetRow(optionsContainer, 0);
            grid.Children.Add(optionsContainer);
        }

        void UpdateLayout(double width)
        {
            if (width < 640)
            {
                col0.Width = GridLength.Star;
                col1.Width = new GridLength(0);
                col2.Width = new GridLength(0);

                if (outputContainer != null)
                {
                    Grid.SetColumn(outputContainer, 0);
                    Grid.SetRow(outputContainer, 1);
                    outputContainer.Margin = new Thickness(24, 0, 24, 24);
                    outputContainer.MaxWidth = double.NaN;
                }
                if (optionsContainer != null)
                {
                    Grid.SetColumn(optionsContainer, 0);
                    Grid.SetRow(optionsContainer, 2);
                    optionsContainer.Margin = new Thickness(24, 0, 24, 24);
                    optionsContainer.MaxWidth = double.NaN;
                }
            }
            else
            {
                col0.Width = GridLength.Star;
                col1.Width = GridLength.Auto;
                col2.Width = GridLength.Auto;

                if (outputContainer != null)
                {
                    Grid.SetColumn(outputContainer, 1);
                    Grid.SetRow(outputContainer, 0);
                    outputContainer.Margin = new Thickness(24);
                    outputContainer.MaxWidth = 200;
                }
                if (optionsContainer != null)
                {
                    Grid.SetColumn(optionsContainer, 2);
                    Grid.SetRow(optionsContainer, 0);
                    optionsContainer.Margin = new Thickness(24);
                    optionsContainer.MaxWidth = 240;
                }
            }
        }

        grid.SizeChanged += (s, e) =>
        {
            UpdateLayout(e.NewSize.Width);
        };

        return new FWFluentMaterialSurface
        {
            MaterialRole = FWFluentMaterialRole.Card,
            Child = grid
        };
    }

    private static UIElement CreateHeader(FluentIconRegular icon, string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children =
            {
                FluentIconFactory.Regular(icon, 24, GalleryThemeResources.Brush("TextPrimary")),
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = GalleryThemeResources.Brush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static UIElement CreateCodeBlock(string code)
    {
        var copyBtn = new FWButton
        {
            Density = FWButtonDensity.Compact,
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    FluentIconFactory.Regular(FluentIconRegular.Copy24, 14, GalleryThemeResources.Brush("TextPrimary")),
                    new FWTextBlock { Text = "Copy", FontSize = 12, Foreground = GalleryThemeResources.Brush("TextPrimary"), VerticalAlignment = VerticalAlignment.Center }
                }
            },
            VerticalAlignment = VerticalAlignment.Center
        };
        copyBtn.Click += (_, _) => { Clipboard.SetText(code); GalleryFeedback.Copied("code"); };

        var header = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        Grid.SetColumn(copyBtn, 1);
        header.Children.Add(copyBtn);

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                header,
                new FWBorder
                {
                    Background = GalleryThemeResources.Brush("LayerFillColorDefaultBrush"),
                    BorderBrush = GalleryThemeResources.Brush("ControlBorder"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(16),
                    Child = new FWTextBlock
                    {
                        Text = code,
                        FontFamily = "Cascadia Code",
                        FontSize = 13,
                        Foreground = GalleryThemeResources.Brush("TextPrimary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }
}
