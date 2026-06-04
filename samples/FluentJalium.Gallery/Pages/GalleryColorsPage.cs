using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWGrid = FluentJalium.Controls.FWGrid;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryColorsPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Colors");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateColorExampleCard(
            FluentIconRegular.Color24,
            "Accent states",
            "FluentJalium keeps WinUI-style accent state aliases for default, hover, pressed, and disabled control states.",
            CreateAccentStateSample()));
        examples.Children.Add(CreateColorExampleCard(
            FluentIconRegular.TextColor24,
            "Text and fills",
            "Text, control fill, layer fill, and solid background tokens mirror Fluent roles across light, dark, and high contrast.",
            CreateRoleSwatchSample()));
        examples.Children.Add(CreateColorExampleCard(
            FluentIconRegular.PaintBrushSparkle24,
            "Semantic surfaces",
            "Selection, hyperlink, progress, and status colors are exposed as theme resources for FW controls.",
            CreateSemanticSwatchSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateAccentStateSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateSwatchTile("Default", "AccentBrush", "AccentFillColorDefaultBrush"),
                CreateSwatchTile("Hover", "AccentBrushHover", "AccentFillColorSecondaryBrush"),
                CreateSwatchTile("Pressed", "AccentBrushPressed", "AccentFillColorTertiaryBrush"),
                CreateSwatchTile("Disabled", "AccentBrushDisabled", "AccentFillColorDisabledBrush")
            }
        };
    }

    private static UIElement CreateRoleSwatchSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateSwatchTile("Text primary", "TextPrimary", "TextFillColorPrimaryBrush"),
                CreateSwatchTile("Text secondary", "TextSecondary", "TextFillColorSecondaryBrush"),
                CreateSwatchTile("Control fill", "ControlBackground", "ControlFillColorDefaultBrush"),
                CreateSwatchTile("Layer fill", "LayerFillColorDefaultBrush", "LayerFillColorDefaultBrush"),
                CreateSwatchTile("Surface", "SurfaceBackground", "SolidBackgroundFillColorSecondaryBrush"),
                CreateSwatchTile("Window", "WindowBackground", "SolidBackgroundFillColorBaseBrush")
            }
        };
    }

    private static UIElement CreateSemanticSwatchSample()
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateSwatchTile("Selection", "SelectionBackground", "SelectionBackgroundWeak"),
                CreateSwatchTile("Hyperlink", "HyperlinkForeground", "HyperlinkForegroundHover"),
                CreateSwatchTile("Progress", "ProgressBarForeground", "ProgressRingForeground"),
                CreateSwatchTile("Critical", "SystemFillColorCriticalBrush", "InfoBarErrorBrush")
            }
        };
    }

    private static FWBorder CreateSwatchTile(string title, string primaryKey, string secondaryKey)
    {
        return new FWBorder
        {
            Width = 170,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 8,
                Children =
                {
                    new FWGrid
                    {
                        Height = 50,
                        Children =
                        {
                            new FWBorder
                            {
                                Background = ThemeBrush(primaryKey),
                                BorderBrush = ThemeBrush("ControlBorder"),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(6)
                            },
                            new FWBorder
                            {
                                Width = 48,
                                Height = 28,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Background = ThemeBrush(secondaryKey),
                                BorderBrush = ThemeBrush("ControlBorder"),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(4),
                                Margin = new Thickness(0, 0, 6, 6)
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = title,
                        FontSize = 13,
                        Foreground = ThemeBrush("TextPrimary")
                    },
                    new FWTextBlock
                    {
                        Text = primaryKey,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWTextBlock
                    {
                        Text = secondaryKey,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateColorExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = 570,
            Background = ThemeBrush("ControlBackground"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    new FWStackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children =
                        {
                            CreateIcon(icon, 20, ThemeBrush("TextPrimary")),
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 15,
                                Foreground = ThemeBrush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    content
                }
            }
        };
    }

    private static FWStackPanel CreateSection(string title)
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
                    Spacing = 10,
                    Children =
                    {
                        CreateIcon(FluentIconRegular.Color24, 24, ThemeBrush("TextPrimary")),
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = 22,
                            Foreground = ThemeBrush("TextPrimary"),
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }
            }
        };
    }

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size, Brush? foreground = null)
    {
        return FluentIconFactory.Regular(icon, size, foreground ?? ThemeBrush("TextPrimary"));
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
