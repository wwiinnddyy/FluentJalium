using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using FluentJalium.Gallery.Resources;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWButtonDensity = FluentJalium.Controls.FWButtonDensity;
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
        var root = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 36
        };

        root.Children.Add(CreateHeroBanner());
        root.Children.Add(CreateThemeAndAccentSection());
        root.Children.Add(CreateTypographySection());
        root.Children.Add(CreateMaterialSection());

        return root;
    }

    private UIElement CreateHeroBanner()
    {
        var heroContent = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(40, 48, 40, 48)
        };

        heroContent.Children.Add(new FWTextBlock
        {
            Text = Strings.Overview_Title,
            FontSize = 40,
            FontFamily = "Segoe UI Variable Display",
            FontWeight = FontWeights.SemiBold,
            Foreground = ThemeBrush("TextPrimary")
        });

        heroContent.Children.Add(new FWTextBlock
        {
            Text = "Explore the Fluent Design System for Jalium.UI — a comprehensive toolkit for building modern, accessible, and beautiful applications.",
            FontSize = 16,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 600
        });

        var buttonRow = new FWWrapPanel
        {
            HorizontalSpacing = 12,
            VerticalSpacing = 8,
            Margin = new Thickness(0, 8, 0, 0)
        };

        var browseBtn = new FWButton
        {
            Content = CreateButtonContent(FluentIconRegular.ControlButton24, "Browse Controls"),
            MinWidth = 160
        };
        buttonRow.Children.Add(browseBtn);

        var designBtn = new FWButton
        {
            Content = CreateButtonContent(FluentIconRegular.DesignIdeas24, "Design System"),
            MinWidth = 160
        };
        buttonRow.Children.Add(designBtn);

        heroContent.Children.Add(buttonRow);

        return new FWFluentMaterialSurface
        {
            MaterialKind = FWFluentMaterialKind.LiquidGlass,
            TintColor = Color.FromArgb(140, 0, 120, 212),
            TintOpacity = 0.14,
            BlurRadius = 18,
            RefractionAmount = 40,
            ChromaticAberration = 0.25,
            FusionRadius = 20,
            Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
            BorderBrush = ThemeBrush("ControlElevationBorderBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Shape = BorderShape.SuperEllipse,
            SuperEllipseN = 4,
            Child = heroContent
        };
    }

    private UIElement CreateThemeAndAccentSection()
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };

        panel.Children.Add(CreateSectionTitle(FluentIconRegular.DarkTheme24, Strings.Overview_ThemeVariants));

        var cards = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        var themeOutput = CreateOutput(string.Format(Strings.Overview_Theme, CurrentThemeLabel()));
        cards.Children.Add(CreateCard(
            FluentIconRegular.DarkTheme24,
            Strings.Overview_ThemeVariants,
            Strings.Overview_ThemeVariants_Desc,
            new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    CreateButtonRow(
                        CreateActionButton(FluentIconRegular.WeatherSunny24, Strings.Settings_ThemeLight, () =>
                        {
                            _applyTheme(FluentThemeVariant.Light);
                            themeOutput.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeLight);
                        }),
                        CreateActionButton(FluentIconRegular.DarkTheme24, Strings.Settings_ThemeDark, () =>
                        {
                            _applyTheme(FluentThemeVariant.Dark);
                            themeOutput.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeDark);
                        }),
                        CreateActionButton(FluentIconRegular.Accessibility24, Strings.Settings_ThemeHighContrast, () =>
                        {
                            _applyTheme(FluentThemeVariant.HighContrast);
                            themeOutput.Text = string.Format(Strings.Overview_Theme, Strings.Settings_ThemeHighContrast);
                        })),
                    CreateStatusBar(themeOutput)
                }
            }));

        var accentOutput = CreateOutput(string.Format(Strings.Overview_Accent, CurrentAccentHex()));
        cards.Children.Add(CreateCard(
            FluentIconRegular.Color24,
            Strings.Overview_AccentPalette,
            Strings.Overview_AccentPalette_Desc,
            new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 10,
                Children =
                {
                    new FWWrapPanel
                    {
                        HorizontalSpacing = 10,
                        VerticalSpacing = 10,
                        Children =
                        {
                            CreateAccentSwatch(Strings.Color_Blue, Color.FromRgb(0x00, 0x78, 0xD4), accentOutput),
                            CreateAccentSwatch(Strings.Color_Rose, Color.FromRgb(0xC2, 0x39, 0xB3), accentOutput),
                            CreateAccentSwatch(Strings.Color_Orange, Color.FromRgb(0xD8, 0x3B, 0x01), accentOutput),
                            CreateAccentSwatch(Strings.Color_Green, Color.FromRgb(0x10, 0x7C, 0x10), accentOutput)
                        }
                    },
                    CreateStatusBar(accentOutput)
                }
            }));

        panel.Children.Add(cards);
        return panel;
    }

    private UIElement CreateTypographySection()
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };

        panel.Children.Add(CreateSectionTitle(FluentIconRegular.TextFont24, Strings.Overview_TypographyTokens));

        var typeRamp = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8
        };

        typeRamp.Children.Add(CreateTypeRampRow("Display", FluentThemeManager.CurrentDisplayFontFamily, 40, "Segoe UI Variable Display"));
        typeRamp.Children.Add(CreateTypeRampRow("Title", FluentThemeManager.CurrentDisplayFontFamily, 28, "FluentTitleFontSize"));
        typeRamp.Children.Add(CreateTypeRampRow("Subtitle", FluentThemeManager.CurrentBodyFontFamily, 20, "FluentSubtitleFontSize"));
        typeRamp.Children.Add(CreateTypeRampRow("Body", FluentThemeManager.CurrentBodyFontFamily, 14, "FluentBodyFontSize"));
        typeRamp.Children.Add(CreateTypeRampRow("Caption", FluentThemeManager.CurrentBodyFontFamily, 12, "FluentCaptionFontSize"));
        typeRamp.Children.Add(CreateTypeRampRow("Mono", FluentThemeManager.CurrentMonoFontFamily, 13, "Cascadia Code"));

        panel.Children.Add(typeRamp);
        return panel;
    }

    private UIElement CreateMaterialSection()
    {
        var panel = new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };

        panel.Children.Add(CreateSectionTitle(FluentIconRegular.LayerDiagonalSparkle24, Strings.Overview_MaterialPreview));

        var output = CreateOutput(Strings.Overview_Material_Active);

        panel.Children.Add(new FWFluentMaterialSurface
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
            Padding = new Thickness(24),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 12,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = Strings.Overview_ResourceLayer,
                        FontSize = 18,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = ThemeBrush("TextPrimary")
                    },
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
                            CreateActionButton(FluentIconRegular.Color24, Strings.Color_Blue, () =>
                            {
                                _applyAccent(Color.FromRgb(0x00, 0x78, 0xD4));
                                output.Text = string.Format(Strings.Overview_MaterialAccent, Strings.Color_Blue);
                            }),
                            CreateActionButton(FluentIconRegular.ColorFill24, Strings.Color_Rose, () =>
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
                    CreateStatusBar(output)
                }
            }
        });

        return panel;
    }

    private static UIElement CreateSectionTitle(FluentIconRegular icon, string title)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                CreateIcon(icon, 22, ThemeBrush("TextPrimary")),
                new FWTextBlock
                {
                    Text = title,
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = ThemeBrush("TextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static FWBorder CreateCard(FluentIconRegular icon, string title, string description, UIElement content)
    {
        return new FWBorder
        {
            Width = double.NaN,
            MaxWidth = 480,
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
                                FontSize = 16,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = ThemeBrush("TextPrimary"),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    },
                    new FWTextBlock
                    {
                        Text = description,
                        FontSize = 13,
                        Foreground = ThemeBrush("TextSecondary"),
                        TextWrapping = TextWrapping.Wrap
                    },
                    content
                }
            }
        };
    }

    private static FWBorder CreateTypeRampRow(string label, string family, double fontSize, string token)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(16, 12, 16, 12),
            Child = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(100) },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    CreateGridChild(0, new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 2,
                        Children =
                        {
                            new FWTextBlock { Text = label, FontSize = 12, FontWeight = FontWeights.SemiBold, Foreground = ThemeBrush("TextPrimary") },
                            new FWTextBlock { Text = $"{fontSize}px", FontSize = 11, Foreground = ThemeBrush("TextSecondary") }
                        }
                    }),
                    CreateGridChild(1, new FWTextBlock
                    {
                        Text = "The quick brown fox jumps over the lazy dog",
                        FontFamily = family,
                        FontSize = fontSize,
                        Foreground = ThemeBrush("TextPrimary"),
                        VerticalAlignment = VerticalAlignment.Center
                    }),
                    CreateGridChild(2, new FWTextBlock
                    {
                        Text = token,
                        FontSize = 11,
                        Foreground = ThemeBrush("TextSecondary"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(12, 0, 0, 0)
                    })
                }
            }
        };
    }

    private static UIElement CreateGridChild(int column, UIElement child)
    {
        Grid.SetColumn(child, column);
        return child;
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

    private static FWWrapPanel CreateButtonRow(params FWButton[] buttons)
    {
        var row = new FWWrapPanel { HorizontalSpacing = 8, VerticalSpacing = 8 };
        foreach (var button in buttons) row.Children.Add(button);
        return row;
    }

    private static FWButton CreateActionButton(FluentIconRegular icon, string text, Action action)
    {
        var button = new FWButton
        {
            Content = CreateButtonContent(icon, text)
        };
        button.Click += (_, _) => action();
        return button;
    }

    private static FWStackPanel CreateButtonContent(FluentIconRegular icon, string text)
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children =
            {
                CreateIcon(icon, 16, ThemeBrush("TextPrimary")),
                new FWTextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center }
            }
        };
    }

    private static TextBlock CreateOutput(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = ThemeBrush("TextSecondary"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static FWBorder CreateStatusBar(TextBlock status)
    {
        return new FWBorder
        {
            Background = ThemeBrush("LayerFillColorDefaultBrush"),
            BorderBrush = ThemeBrush("ControlBorder"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Child = new FWStackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    CreateIcon(FluentIconRegular.InfoSparkle24, 16, ThemeBrush("TextSecondary")),
                    status
                }
            }
        };
    }

    private string CurrentThemeLabel()
    {
        return FluentThemeManager.CurrentTheme switch
        {
            FluentThemeVariant.Light => Strings.Settings_ThemeLight,
            FluentThemeVariant.Dark => Strings.Settings_ThemeDark,
            _ => Strings.Settings_ThemeHighContrast
        };
    }

    private static string CurrentAccentHex()
    {
        var c = FluentThemeManager.CurrentAccentColor;
        return $"{c.R:X2}{c.G:X2}{c.B:X2}";
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
