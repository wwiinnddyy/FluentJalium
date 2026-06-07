using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Services;

internal sealed class GalleryCatalogService
{
    public GalleryPage[] CreatePages(Window owner, Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent)
    {
        var localization = new GalleryLocalizationService();
        return GalleryCatalog.Create(localization, CreateContentFactories(owner, applyTheme, applyAccent));
    }

    private static IReadOnlyDictionary<string, Func<UIElement>> CreateContentFactories(
        Window owner,
        Action<FluentThemeVariant> applyTheme,
        Action<Color> applyAccent)
    {
        var factories = new Dictionary<string, Func<UIElement>>(StringComparer.Ordinal)
        {
            [PageId("Overview")] = () => CreatePageStack(new GalleryOverviewPage(applyTheme, applyAccent).CreateContent()),
            [PageId("Theme Architecture")] = () => CreatePageStack(new GalleryThemeArchitecturePage().CreateContent()),
            [PageId("Colors")] = () => CreatePageStack(new GalleryColorsPage().CreateContent()),
            [PageId("Typography")] = () => CreatePageStack(new GalleryTypographyPage().CreateContent()),
            [PageId("Geometry")] = () => CreatePageStack(new GalleryGeometryPage().CreateContent()),
            [PageId("Motion Tokens")] = () => CreatePageStack(new GalleryMotionTokensPage().CreateContent()),
            [PageId("Buttons")] = () => CreatePageStack(new GalleryButtonsPage().CreateContent()),
            [PageId("Switches")] = () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            [PageId("Text Input")] = () => CreatePageStack(new GalleryTextInputPage().CreateContent()),
            [PageId("Selection")] = () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            [PageId("Range")] = () => CreatePageStack(new GalleryRangePage().CreateContent()),
            [PageId("Date and Time")] = () => CreatePageStack(new GalleryDateTimePage().CreateContent()),
            [PageId("Content and Layout")] = () => CreatePageStack(new GalleryContentLayoutPage().CreateContent()),
            [PageId("Visuals")] = () => CreatePageStack(new GalleryVisualsPage().CreateContent()),
            [PageId("Interaction")] = () => CreatePageStack(new GalleryInteractionPage().CreateContent()),
            [PageId("Input and Media")] = () => CreatePageStack(new GalleryInputMediaPage().CreateContent()),
            [PageId("Collections")] = () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            [PageId("Selectors and Properties")] = () => CreatePageStack(new GallerySelectorsPropertiesPage().CreateContent()),
            [PageId("Data Inspectors")] = () => CreatePageStack(new GalleryDataInspectorsPage().CreateContent()),
            [PageId("Navigation")] = () => CreatePageStack(new GalleryNavigationPage().CreateContent()),
            [PageId("Window Backdrops")] = () => CreatePageStack(new GalleryWindowBackdropsPage(owner).CreateContent()),
            [PageId("Materials and Effects")] = () => CreatePageStack(new GalleryMaterialsPage().CreateContent()),
            [PageId("Motion and Transitions")] = () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            [PageId("Menus")] = () => CreatePageStack(new GalleryMenusPage().CreateContent()),
            [PageId("Disclosure")] = () => CreatePageStack(new GalleryDisclosurePage().CreateContent()),
            [PageId("Status")] = () => CreatePageStack(new GalleryStatusPage().CreateContent()),
            [PageId("Design")] = () => CreatePageStack(new GalleryDesignPage().CreateContent()),
            [PageId("Settings")] = () => CreatePageStack(new GallerySettingsPage(applyTheme, applyAccent).CreateContent()),
            [PageId("State Matrix")] = () => CreatePageStack(new GalleryStateMatrixPage().CreateContent())
        };

        return factories;
    }

    private static string PageId(string title) => GalleryCatalog.CreateUniqueId(title);

    private static UIElement CreatePageStack(params UIElement[] sections)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 22
        };

        foreach (var section in sections)
        {
            stack.Children.Add(section);
        }

        return stack;
    }
}
