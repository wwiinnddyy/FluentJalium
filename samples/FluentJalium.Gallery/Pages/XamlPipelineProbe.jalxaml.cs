using FluentJalium.Controls;
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

        BuildExampleCard();
    }

    private void BuildExampleCard()
    {
        var clicks = 0;
        var output = new TextBlock { Text = "Ready" };
        var sample = new FWButton { Content = "Click me" };
        sample.Click += (_, _) => output.Text = $"Clicked {++clicks} time(s)";

        var options = new StackPanel { Spacing = 8, MinWidth = 180 };
        options.Children.Add(new TextBlock { Text = "Actions" });
        var reset = new FWButton { Content = "Reset" };
        reset.Click += (_, _) =>
        {
            clicks = 0;
            output.Text = "Ready";
        };
        options.Children.Add(reset);

        ProbeRoot.Children.Add(new FluentJalium.Gallery.Controls.ControlExample
        {
            HeaderText = "Counter",
            Example = sample,
            Output = output,
            Options = options,
            XamlCode = """
                <Button Content="Click me" Click="OnSampleClick" />
                """,
            CSharpCode = """
                private void OnSampleClick(object sender, RoutedEventArgs e)
                {
                    Output.Text = $"Clicked {++_clicks} time(s)";
                }
                """
        });
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
