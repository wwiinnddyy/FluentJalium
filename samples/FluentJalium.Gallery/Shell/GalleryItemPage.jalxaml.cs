using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Gallery.Shell;

/// <summary>
/// WinUI Gallery's item-page layout: page header, description, sample content, and metadata.
/// </summary>
public sealed partial class GalleryItemPage : Page
{
    public GalleryItemPage()
    {
        InitializeComponent();

        InfoButton.Click += (_, _) => InfoExpander.IsExpanded = !InfoExpander.IsExpanded;
        ThemeButton.Click += (_, _) => CycleTheme();
    }

    public void ApplyNavigationParameter(object? parameter)
    {
        if (parameter is not GalleryPage page)
        {
            return;
        }

        TitlePresenter.Text = page.Title;
        DescriptionPresenter.Text = page.Description;
        SampleHost.Content = page.CreateContent();

        NamespacePresenter.Text = string.IsNullOrEmpty(page.ApiNamespace)
            ? "No API namespace recorded."
            : $"Namespace: {page.ApiNamespace}";
        InheritancePresenter.Text = page.BaseClasses.Count > 0
            ? $"Inheritance: {string.Join(" › ", page.BaseClasses)}"
            : "No base-class chain recorded.";
        InfoExpander.Visibility = Visibility.Collapsed;

        RelatedPanel.Children.Clear();
        foreach (var related in page.RelatedControls)
        {
            var chip = new TextBlock
            {
                Text = related,
                FontSize = 12
            };
            chip.SetResourceReference(TextBlock.ForegroundProperty, "TextFillColorSecondaryBrush");
            RelatedPanel.Children.Add(chip);
        }

        RelatedExpander.Visibility = page.RelatedControls.Count > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        RelatedExpander.IsExpanded = false;
    }

    private static void CycleTheme()
    {
        FluentThemeManager.ApplyTheme(FluentThemeManager.CurrentTheme switch
        {
            FluentThemeVariant.Dark => FluentThemeVariant.Light,
            FluentThemeVariant.Light => FluentThemeVariant.HighContrast,
            _ => FluentThemeVariant.Dark
        });
    }
}
