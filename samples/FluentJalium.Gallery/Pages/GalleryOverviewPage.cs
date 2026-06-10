using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using FluentJalium.Gallery.Resources;
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
        var panel = CreateSection(Strings.Overview_Title);
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.DarkTheme24,
            Strings.Overview_ThemeVariants,
            Strings.Overview_ThemeVariants_Desc,
            CreateThemeVariantSample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.Color24,
            Strings.Overview_AccentPalette,
            Strings.Overview_AccentPalette_Desc,
            CreateAccentPaletteSample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.TextFont24,
            Strings.Overview_TypographyTokens,
            Strings.Overview_TypographyTokens_Desc,
            CreateTypographySample()));
        examples.Children.Add(CreateOverviewCard(
            FluentIconRegular.LayerDiagonalSparkle24,
            Strings.Overview_MaterialPreview,
            Strings.Overview_MaterialPreview_Desc,
            CreateMaterialThemePreview()));

        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateThemeVariantSample()
    {
        string currentThemeLabel = FluentThemeManager.CurrentTheme switch
        {
            FluentThemeVariant.Light => Strings.Settings_ThemeLight,
            FluentThemeVariant.Dark => Strings.Settings_ThemeDark,
            _ => Strings.Settings_ThemeHighContrast
        };
        var output = CreateOverviewOutput(string.Format(Strings.Overview_Theme, currentThemeLabel));

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateOverviewButtonRow(
                    CreateOverviewActionButton(FluentIconRegular.WeatherSunny24, Strings.Settings_ThemeLight, () =>
                    {
                        _applyTheme(FluentThemeVariant.Light);
                        output.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeLight);
                    }),
                    CreateOverviewActionButton(FluentIconRegular.DarkTheme24, Strings.Settings_ThemeDark, () =>
                    {
                        _applyTheme(FluentThemeVariant.Dark);
                        output.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeDark);
                    }),
                    CreateOverviewActionButton(FluentIconRegular.Accessibility24, Strings.Settings_ThemeHighContrast, () =>
                    {
                        _applyTheme(FluentThemeVariant.HighContrast);
                        output.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeHighContrast);
                    })),
                CreateOverviewStatus(output)
            }
        };
    }

    private UIElement CreateAccentPaletteSample()
    {
        var output = CreateOverviewOutput(string.Format(Strings.Overview_Accent, $"{FluentThemeManager.CurrentAccentColor.R:X2}{FluentThemeManager.CurrentAccentColor.G:X2}{FluentThemeManager.CurrentAccentColor.B:X2}"));

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
                CreateTypographyPreview(Strings.Overview_Typography_Display, FluentThemeManager.CurrentDisplayFontFamily, 22, Strings.Overview_Typography_DisplaySample),
                CreateTypographyPreview(Strings.Overview_Typography_Body, FluentThemeManager.CurrentBodyFontFamily, 14, Strings.Overview_Typography_BodySample),
                CreateTypographyPreview(Strings.Overview_Typography_Mono, FluentThemeManager.CurrentMonoFontFamily, 13, "FWButton | FWTextBox | FWNavigationView")
            }
        };
    }

    private UIElement CreateMaterialThemePreview()
    {
        var output = CreateOverviewOutput(Strings.Overview_Material_Active);

        return new FWFluentMaterialSurface
        {
            Width = double.NaN,
            MaxWidth = 800,
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
                        Text = Strings.Overview_ResourceLayer_Desc,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 8,
                        VerticalSpacing = 8,
                        Children =
                        {
                            CreateOverviewActionButton(FluentIconRegular.Color24, Strings.Color_Blue, () =>
                            {
                                _applyAccent(Color.FromRgb(0x00, 0x78, 0xD4));
                                output.Text = string.Format(Strings.Overview_MaterialAccent, Strings.Color_Blue);
                            }),
                            CreateOverviewActionButton(FluentIconRegular.ColorFill24, Strings.Color_Rose, () =>
                            {
                                _applyAccent(Color.FromRgb(0xC2, 0x39, 0xB3));
                                output.Text = string.Format(Strings.Overview_MaterialAccent, Strings.Color_Rose);
                            }),
                            new FWToggleSwitch
                            {
                                Header = Strings.Overview_BackdropAware,
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
                CreateAccentSwatch(Strings.Color_Blue, Color.FromRgb(0x00, 0x78, 0xD4), output),
                CreateAccentSwatch(Strings.Color_Rose, Color.FromRgb(0xC2, 0x39, 0xB3), output),
                CreateAccentSwatch(Strings.Color_Orange, Color.FromRgb(0xD8, 0x3B, 0x01), output),
                CreateAccentSwatch(Strings.Color_Green, Color.FromRgb(0x10, 0x7C, 0x10), output)
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
            output.Text = string.Format(Strings.Overview_Accent, $"{color.R:X2}{color.G:X2}{color.B:X2}");
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
                    Text = Strings.Overview_ResourceLayer,
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
            Width = double.NaN,
            MaxWidth = 800,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = ThemeBrush("CardBackgroundFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(24),
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
            Padding = new Thickness(16),
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
                            FontSize = 24,
                            FontWeight = FontWeights.SemiBold,
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
