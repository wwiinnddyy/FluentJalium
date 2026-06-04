using System.Globalization;
using FluentJalium.Controls.Themes;
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

internal sealed class GalleryTypographyPage
{
    public UIElement CreateContent()
    {
        var panel = CreateSection("Typography");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateTypographyExampleCard(
            FluentIconRegular.TextFont24,
            "Font families",
            "FluentJalium exposes display, body, mono, and icon family resources through the theme manager and generic dictionary.",
            CreateFontFamilySample()));
        examples.Children.Add(CreateTypographyExampleCard(
            FluentIconRegular.TextFontSize24,
            "Type ramp",
            "Caption, body, subtitle, title, and control font size tokens keep Gallery pages and FW controls visually aligned.",
            CreateTypeRampSample()));
        examples.Children.Add(CreateTypographyExampleCard(
            FluentIconRegular.TextFontInfo24,
            "Control text roles",
            "Text roles combine family, size, foreground, and spacing so dense controls remain readable on Fluent materials.",
            CreateControlTextRoleSample()));

        panel.Children.Add(examples);
        return panel;
    }

    private static UIElement CreateFontFamilySample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateFamilyPreview("Display", "DisplayFontFamily", FluentThemeManager.CurrentDisplayFontFamily, 22, "FluentJalium Gallery"),
                CreateFamilyPreview("Body", "BodyFontFamily", FluentThemeManager.CurrentBodyFontFamily, 14, "Readable controls, descriptions, and state labels."),
                CreateFamilyPreview("Mono", "MonoFontFamily", FluentThemeManager.CurrentMonoFontFamily, 13, "FWButton | FWTextBox | FWNavigationView"),
                CreateFamilyPreview("Icon", "FluentIconFontFamily", FormatResourceValue("FluentIconFontFamily"), 18, "Segoe Fluent Icons")
            }
        };
    }

    private static UIElement CreateTypeRampSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Children =
            {
                CreateTypeRampRow("Caption", "FluentCaptionFontSize", "Metadata and compact labels"),
                CreateTypeRampRow("Body", "FluentBodyFontSize", "Default content and control text"),
                CreateTypeRampRow("Control", "ControlContentThemeFontSize", "FW control content"),
                CreateTypeRampRow("Subtitle", "FluentSubtitleFontSize", "Section labels and sample headers"),
                CreateTypeRampRow("Title", "FluentTitleFontSize", "Page headings and major affordances")
            }
        };
    }

    private static UIElement CreateControlTextRoleSample()
    {
        return new FWBorder
        {
            Width = 490,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateControlRoleRow(FluentIconRegular.TextFont24, "Primary", "TextPrimary", "ControlContentThemeFontSize", "Command labels and selected rows"),
                    CreateControlRoleRow(FluentIconRegular.TextBulletListSquare24, "Secondary", "TextSecondary", "FluentCaptionFontSize", "Descriptions, hints, and supporting metadata"),
                    CreateControlRoleRow(FluentIconRegular.TextColor24, "On accent", "TextOnAccent", "ControlContentThemeFontSize", "Primary buttons and selected accents")
                }
            }
        };
    }

    private static FWBorder CreateFamilyPreview(string label, string resourceKey, string family, double fontSize, string sample)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = $"{label}: {resourceKey}",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    },
                    new FWTextBlock
                    {
                        Text = sample,
                        FontFamily = family,
                        FontSize = fontSize,
                        Foreground = ThemeBrush("TextPrimary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWTextBlock
                    {
                        Text = family,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static FWBorder CreateTypeRampRow(string role, string tokenKey, string sample)
    {
        var fontSize = ResourceDouble(tokenKey, 14);

        return new FWBorder
        {
            Width = 490,
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWGrid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(105) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(58) }
                },
                Children =
                {
                    CreateTypeLabel(role, tokenKey),
                    CreateTypeSample(sample, fontSize),
                    CreateTypeSize(fontSize)
                }
            }
        };
    }

    private static FWStackPanel CreateTypeLabel(string role, string tokenKey)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                new FWTextBlock
                {
                    Text = role,
                    FontSize = 13,
                    Foreground = ThemeBrush("TextPrimary")
                },
                new FWTextBlock
                {
                    Text = tokenKey,
                    FontSize = 10,
                    Foreground = ThemeBrush("TextSecondary"),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static FWTextBlock CreateTypeSample(string sample, double fontSize)
    {
        var text = new FWTextBlock
        {
            Text = sample,
            FontFamily = FluentThemeManager.CurrentBodyFontFamily,
            FontSize = fontSize,
            Foreground = ThemeBrush("TextPrimary"),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(text, 1);
        return text;
    }

    private static FWTextBlock CreateTypeSize(double fontSize)
    {
        var text = new FWTextBlock
        {
            Text = $"{fontSize:0.##} px",
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(text, 2);
        return text;
    }

    private static FWStackPanel CreateControlRoleRow(FluentIconRegular icon, string title, string brushKey, string sizeKey, string description)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateIcon(icon, 18, ThemeBrush("TextSecondary")),
                new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 3,
                    Children =
                    {
                        new FWTextBlock
                        {
                            Text = title,
                            FontSize = ResourceDouble(sizeKey, 14),
                            Foreground = ThemeBrush(brushKey)
                        },
                        new FWTextBlock
                        {
                            Text = $"{brushKey} / {sizeKey}",
                            FontSize = 11,
                            Foreground = ThemeBrush("TextSecondary")
                        },
                        new FWTextBlock
                        {
                            Text = description,
                            FontSize = 12,
                            Foreground = ThemeBrush("TextSecondary"),
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            }
        };
    }

    private static FWBorder CreateTypographyExampleCard(FluentIconRegular icon, string title, string description, UIElement content)
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
                        CreateIcon(FluentIconRegular.TextFont24, 24, ThemeBrush("TextPrimary")),
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

    private static string FormatResourceValue(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return "-";
        }

        return value switch
        {
            string text => text,
            double number => number.ToString("0.##", CultureInfo.InvariantCulture),
            int number => number.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "-"
        };
    }

    private static double ResourceDouble(string key, double fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) != true)
        {
            return fallback;
        }

        return value switch
        {
            double number => number,
            int number => number,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) => number,
            _ => fallback
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
