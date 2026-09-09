using FluentJalium.Controls;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Jalxaml-defined example card modeled after WinUI Gallery's ControlExample: explanatory text,
/// live content, and an optional source expander in one reusable surface.
/// </summary>
public sealed partial class GalleryExampleCard : UserControl
{
    public GalleryExampleCard()
    {
        InitializeComponent();
    }

    public GalleryExampleCard(string title, string description, UIElement sample, string? code = null)
        : this()
    {
        TitlePresenter.Text = title;
        DescriptionPresenter.Text = description;
        SampleHost.Content = sample;
        CodePresenter.Text = code ?? CreateCodeHint(sample);
        // Keep the source affordance present for every sample. A generated hint is preferable to
        // a hidden section: it makes the Jalxaml-first authoring model discoverable even for
        // examples whose live content is assembled from a small code-behind interaction.
        CodeExpander.Visibility = Visibility.Visible;
    }

    private static string CreateCodeHint(UIElement sample) =>
        $"// {sample.GetType().Name} sample\n// Configure this control in Jalxaml or code-behind.";
}
