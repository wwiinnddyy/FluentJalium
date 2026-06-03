using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;
using JaliumThemeManager = Jalium.UI.Controls.Themes.ThemeManager;

namespace FluentJalium.Tests;

[Collection("Application")]
public sealed class FluentOverviewThemeTests
{
    [Fact]
    [RequiresUnreferencedCode("Exercises runtime theme dictionary loading.")]
    public void FWOverviewThemeControls_ShouldApplyThemeAccentTypographyAndMaterialPreview()
    {
        ResetApplicationState();
        var app = new Application();

        try
        {
            FluentThemeManager.Apply(app);
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);

            var accent = Color.FromRgb(0xC2, 0x39, 0xB3);
            FluentThemeManager.ApplyAccent(accent);

            var swatch = new FWBorder
            {
                Width = 16,
                Height = 16,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(accent),
                BorderBrush = Assert.IsType<SolidColorBrush>(app.Resources["ControlBorder"]),
                BorderThickness = new Thickness(1)
            };
            var themeButton = CreateOverviewButton("Light");
            var accentButton = CreateOverviewButton("Rose");
            var typography = new FWStackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                Children =
                {
                    new FWTextBlock
                    {
                        Text = "Display",
                        FontFamily = FluentThemeManager.CurrentDisplayFontFamily,
                        FontSize = 22
                    },
                    new FWTextBlock
                    {
                        Text = "Body",
                        FontFamily = FluentThemeManager.CurrentBodyFontFamily,
                        FontSize = 14
                    },
                    new FWTextBlock
                    {
                        Text = "Mono",
                        FontFamily = FluentThemeManager.CurrentMonoFontFamily,
                        FontSize = 13
                    }
                }
            };
            var preview = new FWFluentMaterialSurface
            {
                MaterialKind = FWFluentMaterialKind.LiquidGlass,
                TintOpacity = 0.2,
                BlurRadius = 14,
                RefractionAmount = 70,
                ChromaticAberration = 0.42,
                FusionRadius = 24,
                Shape = BorderShape.SuperEllipse,
                SuperEllipseN = 4,
                Child = new FWStackPanel
                {
                    Orientation = Orientation.Vertical,
                    Spacing = 12,
                    Children =
                    {
                        themeButton,
                        accentButton,
                        new FWToggleSwitch
                        {
                            Header = "Backdrop aware",
                            IsOn = true
                        },
                        typography
                    }
                }
            };

            Assert.Equal(FluentThemeVariant.Light, FluentThemeManager.CurrentTheme);
            Assert.Equal("Light", ResourceDictionary.CurrentThemeKey);
            Assert.Equal(accent, FluentThemeManager.CurrentAccentColor);
            Assert.Equal(accent, GetBrushColor(app.Resources["AccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["FluentAccentBrush"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ToggleSwitchOnBackground"]));
            Assert.Equal(accent, GetBrushColor(app.Resources["ProgressBarForeground"]));
            Assert.Equal("Segoe UI Variable Display", FluentThemeManager.CurrentDisplayFontFamily);
            Assert.Equal("Segoe UI Variable Text", FluentThemeManager.CurrentBodyFontFamily);
            Assert.Equal("Cascadia Code", FluentThemeManager.CurrentMonoFontFamily);
            Assert.Equal(16, swatch.Width);
            Assert.Equal(8, swatch.CornerRadius.TopLeft);
            Assert.Equal(accent, GetBrushColor(swatch.Background!));
            Assert.Equal("Light", themeButton.Content);
            Assert.Equal("Rose", accentButton.Content);
            Assert.Equal(3, typography.Children.Count);
            Assert.Equal(FWFluentMaterialKind.LiquidGlass, preview.MaterialKind);
            Assert.True(preview.LiquidGlass);
            Assert.Equal(BorderShape.SuperEllipse, preview.Shape);

            var previewContent = Assert.IsType<FWStackPanel>(preview.Child);
            Assert.Equal(4, previewContent.Children.Count);
            Assert.IsType<FWToggleSwitch>(previewContent.Children[2]);
            Assert.Same(typography, previewContent.Children[3]);
        }
        finally
        {
            ResetApplicationState();
        }
    }

    private static FWButton CreateOverviewButton(string text)
    {
        return new FWButton
        {
            Content = text
        };
    }

    private static Color GetBrushColor(object value)
    {
        return Assert.IsType<SolidColorBrush>(value).Color;
    }

    private static void ResetApplicationState()
    {
        var currentField = typeof(Application).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static);
        currentField?.SetValue(null, null);

        var jaliumReset = typeof(JaliumThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        jaliumReset?.Invoke(null, null);

        var fluentReset = typeof(FluentThemeManager).GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Static);
        fluentReset?.Invoke(null, null);
    }
}
