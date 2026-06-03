using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWFluentMaterialKind = FluentJalium.Controls.FWFluentMaterialKind;
using FWFluentMaterialSurface = FluentJalium.Controls.FWFluentMaterialSurface;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GalleryOverviewPage
{
    private readonly Action<FluentThemeVariant> _applyTheme;
    private readonly Action<Color> _applyAccent;

    public GalleryOverviewPage(Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent)
    {
        _applyTheme = applyTheme;
        _applyAccent = applyAccent;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Overview");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.DarkTheme24,
            "Theme variants",
            "Switch the active FluentJalium resource dictionary across light, dark, and high contrast.",
            CreateThemeVariantSample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.Color24,
            "Accent palette",
            "Apply WinUI-inspired accent colors and validate dependent brushes across controls.",
            CreateAccentPaletteSample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.TextFont24,
            "Typography tokens",
            "Display, body, mono, and control font resources mirror Fluent typography roles.",
            CreateTypographySample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            "Layered material preview",
            "Overview controls sit on LiquidGlass while preserving readable text, borders, and accent states.",
            CreateMaterialThemePreview()));

        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateThemeVariantSample()
    {
        var output = CreateOverviewOutput($"Theme: {FluentThemeManager.CurrentTheme}.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateOverviewButtonRow(
                    CreateOverviewActionButton(FluentIconRegular.WeatherSunny24, "Light", () =>
                    {
                        _applyTheme(FluentThemeVariant.Light);
                        output.Text = "Theme: Light.";
                    }),
                    CreateOverviewActionButton(FluentIconRegular.DarkTheme24, "Dark", () =>
                    {
                        _applyTheme(FluentThemeVariant.Dark);
                        output.Text = "Theme: Dark.";
                    }),
                    CreateOverviewActionButton(FluentIconRegular.Accessibility24, "High Contrast", () =>
                    {
                        _applyTheme(FluentThemeVariant.HighContrast);
                        output.Text = "Theme: HighContrast.";
                    })),
                CreateOverviewStatus(output)
            }
        };
    }

    private UIElement CreateAccentPaletteSample()
    {
        var output = CreateOverviewOutput($"Accent: #{FluentThemeManager.CurrentAccentColor.R:X2}{FluentThemeManager.CurrentAccentColor.G:X2}{FluentThemeManager.CurrentAccentColor.B:X2}.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateAccentSwatchRow(output),
                CreateOverviewStatus(output)
            }
        };
    }

    private static UIElement CreateTypographySample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateTypographyPreview("Display", FluentThemeManager.CurrentDisplayFontFamily, 22, "FluentJalium Gallery"),
                CreateTypographyPreview("Body", FluentThemeManager.CurrentBodyFontFamily, 14, "Readable control documentation and state labels."),
                CreateTypographyPreview("Mono", FluentThemeManager.CurrentMonoFontFamily, 13, "FWButton | FWTextBox | FWNavigationView")
            }
        };
    }

    private UIElement CreateMaterialThemePreview()
    {
        var output = CreateOverviewOutput("Material preview: LiquidGlass layer active.");

        return new FWFluentMaterialSurface
        {
            Width = 520,
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(180, 20, 84, 145),
            TintOpacity = 0.2,
            BlurRadius = 14,
            RefractionAmount = 70,
            ChromaticAberration = 0.42,
            FusionRadius = 24,
            Background = new SolidColorBrush(Color.FromArgb(66, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Padding = new Thickness(16),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    CreateMaterialHeader(),
                    new FWTextBlock
                    {
                        Text = "Layer fill, accent, typography, and shell brushes are refreshed together.",
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 8,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreateOverviewActionButton(FluentIconRegular.Color24, "Blue", () =>
                            {
                                _applyAccent(Color.FromRgb(0x00, 0x78, 0xD4));
                                output.Text = "Material accent: Blue.";
                            }),
                            CreateOverviewActionButton(FluentIconRegular.ColorFill24, "Rose", () =>
                            {
                                _applyAccent(Color.FromRgb(0xC2, 0x39, 0xB3));
                                output.Text = "Material accent: Rose.";
                            }),
                            new FWToggleSwitch
                            {
                                Header = "Backdrop aware",
                                IsOn = true
                            }
                        }
                    },
                    CreateOverviewStatus(output)
                }
            }
        };
    }

    private FWWrapPanel CreateAccentSwatchRow(TextBlock output)
    {
        return new FWWrapPanel
        {
            HorizontalSpacing = 10,
            VerticalSpacing = 10,
            Children =
            {
                CreateAccentSwatch("Blue", Color.FromRgb(0x00, 0x78, 0xD4), output),
                CreateAccentSwatch("Rose", Color.FromRgb(0xC2, 0x39, 0xB3), output),
                CreateAccentSwatch("Orange", Color.FromRgb(0xD8, 0x3B, 0x01), output),
                CreateAccentSwatch("Green", Color.FromRgb(0x10, 0x7C, 0x10), output)
            }
        };
    }

    private FWButton CreateAccentSwatch(string label, Color color, TextBlock output)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new FWBorder
                    {
                        Width = 16,
                        Height = 16,
                        CornerRadius = new CornerRadius(8),
                        Background = new SolidColorBrush(color),
                        BorderBrush = ThemeBrush("ControlBorder"),
                        BorderThickness = new Thickness(1)
                    },
                    new FWTextBlock
                    {
                        Text = label,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        button.Click += (_, _) =>
        {
            _applyAccent(color);
            output.Text = $"Accent: #{color.R:X2}{color.G:X2}{color.B:X2}.";
        };
        return button;
    }

    private static FWBorder CreateTypographyPreview(string label, string family, double fontSize, string sample)
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
                Spacing = 4,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = $"{label}: {family}",
                        FontSize = 12,
                        Foreground = ThemeBrush("TextSecondary")
                    },
                    new FWTextBlock
                    {
                        Text = sample,
                        FontFamily = family,
                        FontSize = fontSize,
                        Foreground = ThemeBrush("TextPrimary")
                    }
                }
            }
        };
    }

    private static FWStackPanel CreateMaterialHeader()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                CreateIcon(FluentIconRegular.LayerDiagonalSparkle24, 18, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = "FluentJalium resource layer",
                    FontSize = 15,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateOverviewCard(FluentIconRegular icon, string title, string description, UIElement content)
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

    private static FWWrapPanel CreateOverviewButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel
        {
            HorizontalSpacing = 8,
            VerticalSpacing = 8
        };

        foreach (var button in buttons)
        {
            row.Children.Add(button);
        }

        return row;
    }

    private static FWButton CreateOverviewActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 6,
                Children =
                {
                    CreateIcon(icon, 16, ThemeBrush("TextPrimary")),
                    new FWTextBlock
                    {
                        Text = text,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static TextBlock CreateOverviewOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateOverviewStatus(TextBlock status)
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
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 18, ThemeBrush("TextSecondary")),
                    status
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
                        CreateIcon(FluentIconRegular.Home24, 24, ThemeBrush("TextPrimary")),
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

    private static FluentIcon CreateIcon(FluentIconRegular icon, double size = FluentIcon.DefaultSize, Brush? foreground = null)
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
