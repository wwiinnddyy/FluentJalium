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
        return GalleryCatalog.Create(CreateContentFactories(owner, applyTheme, applyAccent));
    }

    private static GalleryPageContentFactories CreateContentFactories(
        Window owner,
        Action<FluentThemeVariant> applyTheme,
        Action<Color> applyAccent)
    {
        return new GalleryPageContentFactories(
            Overview: () => CreatePageStack(new GalleryOverviewPage(applyTheme, applyAccent).CreateContent()),
            ThemeArchitecture: () => CreatePageStack(new GalleryThemeArchitecturePage().CreateContent()),
            Colors: () => CreatePageStack(new GalleryColorsPage().CreateContent()),
            Typography: () => CreatePageStack(new GalleryTypographyPage().CreateContent()),
            Geometry: () => CreatePageStack(new GalleryGeometryPage().CreateContent()),
            MotionTokens: () => CreatePageStack(new GalleryMotionTokensPage().CreateContent()),
            Buttons: () => CreatePageStack(new GalleryButtonsPage().CreateContent()),
            Switches: () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            TextInput: () => CreatePageStack(new GalleryTextInputPage().CreateContent()),
            Selection: () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            Range: () => CreatePageStack(new GalleryRangePage().CreateContent()),
            DateAndTime: () => CreatePageStack(new GalleryDateTimePage().CreateContent()),
            ContentAndLayout: () => CreatePageStack(new GalleryContentLayoutPage().CreateContent()),
            Visuals: () => CreatePageStack(new GalleryVisualsPage().CreateContent()),
            Interaction: () => CreatePageStack(new GalleryInteractionPage().CreateContent()),
            InputAndMedia: () => CreatePageStack(new GalleryInputMediaPage().CreateContent()),
            Collections: () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            SelectorsAndProperties: () => CreatePageStack(new GallerySelectorsPropertiesPage().CreateContent()),
            DataInspectors: () => CreatePageStack(new GalleryDataInspectorsPage().CreateContent()),
            Navigation: () => CreatePageStack(new GalleryNavigationPage().CreateContent()),
            WindowBackdrops: () => CreatePageStack(new GalleryWindowBackdropsPage(owner).CreateContent()),
            MaterialsAndEffects: () => CreatePageStack(new GalleryMaterialsPage().CreateContent()),
            MotionAndTransitions: () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            Menus: () => CreatePageStack(new GalleryMenusPage().CreateContent()),
            Disclosure: () => CreatePageStack(new GalleryDisclosurePage().CreateContent()),
            Status: () => CreatePageStack(new GalleryStatusPage().CreateContent()),
            StateMatrix: () => CreatePageStack(new GalleryStateMatrixPage().CreateContent()));
    }

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
