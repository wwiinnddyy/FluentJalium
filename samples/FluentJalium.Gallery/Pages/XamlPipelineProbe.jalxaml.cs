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
            ReportResolvedTheme();
        };
        ProbeDarkButton.Click += (_, _) =>
        {
            FluentThemeManager.ApplyTheme(FluentThemeVariant.Dark);
            ReportResolvedTheme();
        };
    }

    private void ReportResolvedTheme()
    {
        var brush = Application.Current?.Resources["TextFillColorPrimaryBrush"] as SolidColorBrush;
        ProbeStatus.Text = $"{FluentThemeManager.CurrentTheme} resolved={brush?.Color.ToString() ?? "<null>"}";
    }
}
