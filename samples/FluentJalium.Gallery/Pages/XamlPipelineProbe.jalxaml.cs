using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Pages;

/// <summary>
/// Proves the JalXAML pipeline end to end: generated <c>x:Name</c> fields, semantic typography
/// styles, and <c>{ThemeResource}</c> repainting without rebuilding the visual tree.
/// </summary>
public sealed partial class XamlPipelineProbe : Page
{
    private int _clicks;

    public XamlPipelineProbe()
    {
        InitializeComponent();

        ProbeButton.Click += (_, _) => ProbeStatus.Text = $"Clicks: {++_clicks}";
        ProbeLightButton.Click += (_, _) =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Light);
            ReportLiveTheme();
        };
        ProbeDarkButton.Click += (_, _) =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            ReportLiveTheme();
        };

        Loaded += (_, _) => ReportLiveTheme();
    }

    private void ReportLiveTheme()
    {
        var before = ToHex(ProbeTitle.Foreground);

        FluentThemeManager.ApplyTheme(
            FluentThemeManager.CurrentTheme == FluentThemeVariant.Dark
                ? FluentThemeVariant.Light
                : FluentThemeVariant.Dark);

        var after = ToHex(ProbeTitle.Foreground);
        ProbeStatus.Text = $"live {before} -> {after} ({FluentThemeManager.CurrentTheme})";
    }

    private static string ToHex(Brush? brush) =>
        brush is SolidColorBrush solid
            ? $"#{solid.Color.A:X2}{solid.Color.R:X2}{solid.Color.G:X2}{solid.Color.B:X2}"
            : "<null>";
}
