using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using FWBorder = FluentJalium.Controls.FWBorder;
using FWButton = FluentJalium.Controls.FWButton;
using FWStackPanel = FluentJalium.Controls.FWStackPanel;
using FWTextBlock = FluentJalium.Controls.FWTextBlock;
using FWToggleSwitch = FluentJalium.Controls.FWToggleSwitch;
using FWWrapPanel = FluentJalium.Controls.FWWrapPanel;

namespace FluentJalium.Gallery.Pages;

internal sealed class GallerySettingsPage
{
    private readonly Action<FluentThemeVariant> _applyTheme;
    private readonly Action<Color> _applyAccent;

    public GallerySettingsPage(Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent)
    {
        _applyTheme = applyTheme;
        _applyAccent = applyAccent;
    }

    public UIElement CreateContent()
    {
        var panel = CreateSection("Gallery Settings");
        var examples = new FWWrapPanel
        {
            HorizontalSpacing = 16,
            VerticalSpacing = 16
        };

        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.DarkTheme24,
            "Theme mode",
            "Apply the active FluentJalium theme dictionary from a footer settings surface.",
            CreateThemeModeSample(),
            "<FWNavigationView.FooterMenuItems>\n    <FWNavigationViewItem Content=\"Settings\" Icon=\"Settings\" />\n</FWNavigationView.FooterMenuItems>"));
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.Color24,
            "Accent color",
            "Validate accent-dependent brushes without leaving the current Gallery shell.",
            CreateAccentSample(),
            "FluentThemeManager.ApplyAccent(application, Color.FromRgb(0x00, 0x78, 0xD4));"));
        examples.Children.Add(CreateSettingsCard(
            FluentIconRegular.DataUsage24,
            "Gallery diagnostics",
            "Keep design, metadata, and state checks discoverable from footer navigation.",
            CreateDiagnosticsSample(),
            "GalleryCatalog.CreatePageInfos()\n    .Where(page => page.IsFooter || page.Status != GalleryPageStatus.Stable);"));

        panel.Children.Add(examples);
        return panel;
    }

    private UIElement CreateThemeModeSample()
    {
        var output = CreateOutput($"Theme: {FluentThemeManager.CurrentTheme}.");

        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateButtonRow(
                    CreateActionButton(FluentIconRegular.WeatherSunny24, "Light", () =>
                    {
                        _applyTheme(FluentThemeVariant.Light);
                        output.Text = "Theme: Light.";
                    }),
                    CreateActionButton(FluentIconRegular.DarkTheme24, "Dark", () =>
                    {
                        _applyTheme(FluentThemeVariant.Dark);
                        output.Text = "Theme: Dark.";
                    }),
                    CreateActionButton(FluentIconRegular.Accessibility24, "High Contrast", () =>
                    {
                        _applyTheme(FluentThemeVariant.HighContrast);
                        output.Text = "Theme: HighContrast.";
                    })),
                CreateStatus(output)
            }
        };
    }

    private UIElement CreateAccentSample()
    {
        var output = CreateOutput($"Accent: #{FluentThemeManager.CurrentAccentColor.R:X2}{FluentThemeManager.CurrentAccentColor.G:X2}{FluentThemeManager.CurrentAccentColor.B:X2}.");

        return new FWStackPanel
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
                        CreateAccentButton("Blue", Color.FromRgb(0x00, 0x78, 0xD4), output),
                        CreateAccentButton("Rose", Color.FromRgb(0xC2, 0x39, 0xB3), output),
                        CreateAccentButton("Orange", Color.FromRgb(0xD8, 0x3B, 0x01), output),
                        CreateAccentButton("Green", Color.FromRgb(0x10, 0x7C, 0x10), output)
                    }
                },
                CreateStatus(output)
            }
        };
    }

    private static UIElement CreateDiagnosticsSample()
    {
        return new FWStackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                CreateDiagnosticRow(FluentIconRegular.Navigation24, "Shell", "NavigationView + Frame routes through GalleryHostPage."),
                CreateDiagnosticRow(FluentIconRegular.DatabaseSearch24, "Catalog", "Metadata entries resolve through UniqueId page factories."),
                CreateDiagnosticRow(FluentIconRegular.DataUsage24, "State Matrix", "Footer diagnostics stay available beside Settings."),
                new FWToggleSwitch
                {
                    Header = "Show metadata chips",
                    IsOn = true
                }
            }
        };
    }

    private FWButton CreateAccentButton(string label, Color color, TextBlock output)
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

    private static FWBorder CreateDiagnosticRow(FluentIconRegular icon, string title, string description)
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
                Spacing = 10,
                Children =
                {
                    CreateIcon(icon, 18, ThemeBrush("TextPrimary")),
                    new FWStackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 3,
                        Children =
                        {
                            new FWTextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                Foreground = ThemeBrush("TextPrimary")
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
            }
        };
    }

    private static FWBorder CreateSettingsCard(FluentIconRegular icon, string title, string description, UIElement content, string code)
    {
        return GallerySampleCard.Create(icon, title, description, content, code: code);
    }

    private static FWWrapPanel CreateButtonRow(params FWButton[] buttons)
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

    private static FWButton CreateActionButton(FluentIconRegular icon, string text, Action action)
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

    private static FWBorder CreateStatus(TextBlock status)
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
                        CreateIcon(FluentIconRegular.Settings24, 24, ThemeBrush("TextPrimary")),
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
