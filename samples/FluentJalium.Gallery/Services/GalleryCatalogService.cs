using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Services;

internal sealed class GalleryCatalogService
{
    public GalleryPage[] CreatePages(Window owner, Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent, Action<string> navigate)
    {
        var localization = new GalleryLocalizationService();
        var pageInfos = GalleryCatalog.CreatePageInfos(localization);
        return GalleryCatalog.Create(localization, CreateContentFactories(owner, applyTheme, applyAccent, navigate, pageInfos));
    }

    public string[] CreateRegisteredPageIds(Window owner, Action<FluentThemeVariant> applyTheme, Action<Color> applyAccent, Action<string> navigate)
    {
        var localization = new GalleryLocalizationService();
        var pageInfos = GalleryCatalog.CreatePageInfos(localization);
        return CreateContentFactories(owner, applyTheme, applyAccent, navigate, pageInfos)
            .Keys
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, Func<UIElement>> CreateContentFactories(
        Window owner,
        Action<FluentThemeVariant> applyTheme,
        Action<Color> applyAccent,
        Action<string> navigate,
        GalleryPageInfo[] pageInfos)
    {
        var factories = new Dictionary<string, Func<UIElement>>(StringComparer.Ordinal)
        {
            [PageId("Overview")] = () => CreatePageStack(new GalleryOverviewPage(pageInfos, navigate).CreateContent()),
            [PageId("Resources")] = CreatePlaceholderContent(pageInfos, "resources"),
            [PageId("Styles")] = CreatePlaceholderContent(pageInfos, "styles"),
            [PageId("Binding")] = CreatePlaceholderContent(pageInfos, "binding"),
            [PageId("Templates")] = CreatePlaceholderContent(pageInfos, "templates"),
            [PageId("Custom User Controls")] = CreatePlaceholderContent(pageInfos, "customusercontrols"),
            [PageId("XAML Conditions")] = CreatePlaceholderContent(pageInfos, "xamlconditions"),
            [PageId("Scratch Pad")] = CreatePlaceholderContent(pageInfos, "scratchpad"),
            [PageId("Screen Reader Support")] = CreatePlaceholderContent(pageInfos, "screenreadersupport"),
            [PageId("Keyboard Support")] = CreatePlaceholderContent(pageInfos, "keyboardsupport"),
            [PageId("Color Contrast")] = CreatePlaceholderContent(pageInfos, "colorcontrast"),
            [PageId("All Controls")] = () => CreatePageStack(new GalleryCatalogFilterPage(GalleryCatalogFilter.AllControls, pageInfos, navigate).CreateContent()),
            [PageId("New Controls")] = () => CreatePageStack(new GalleryCatalogFilterPage(GalleryCatalogFilter.New, pageInfos, navigate).CreateContent()),
            [PageId("Updated Controls")] = () => CreatePageStack(new GalleryCatalogFilterPage(GalleryCatalogFilter.Updated, pageInfos, navigate).CreateContent()),
            [PageId("Preview Controls")] = () => CreatePageStack(new GalleryCatalogFilterPage(GalleryCatalogFilter.Preview, pageInfos, navigate).CreateContent()),
            [PageId("Diagnostic Controls")] = () => CreatePageStack(new GalleryCatalogFilterPage(GalleryCatalogFilter.Diagnostic, pageInfos, navigate).CreateContent()),
            [PageId("Theme Architecture")] = () => CreatePageStack(new GalleryThemeArchitecturePage().CreateContent()),
            [PageId("Colors")] = () => CreatePageStack(new GalleryColorsPage().CreateContent()),
            [PageId("Typography")] = () => CreatePageStack(new GalleryTypographyPage().CreateContent()),
            [PageId("Geometry")] = () => CreatePageStack(new GalleryGeometryPage().CreateContent()),
            [PageId("Motion Tokens")] = () => CreatePageStack(new GalleryMotionTokensPage().CreateContent()),
            [PageId("Buttons")] = () => CreatePageStack(new GalleryButtonsPage().CreateContent()),
            [PageId("Switches")] = () => CreatePageStack(new GallerySwitchesPage().CreateContent()),
            [PageId("Text Input")] = () => CreatePageStack(new GalleryTextInputPage().CreateContent()),
            [PageId("Selection")] = () => CreatePageStack(new GallerySelectionPage().CreateContent()),
            [PageId("Forms")] = () => CreatePageStack(new GalleryFormsPage().CreateContent()),
            [PageId("Range")] = () => CreatePageStack(new GalleryRangePage().CreateContent()),
            [PageId("Date and Time")] = () => CreatePageStack(new GalleryDateTimePage().CreateContent()),
            [PageId("Content and Layout")] = () => CreatePageStack(new GalleryContentLayoutPage().CreateContent()),
            [PageId("Visuals")] = () => CreatePageStack(new GalleryVisualsPage().CreateContent()),
            [PageId("Interaction")] = () => CreatePageStack(new GalleryInteractionPage().CreateContent()),
            [PageId("Input and Media")] = () => CreatePageStack(new GalleryInputMediaPage().CreateContent()),
            [PageId("Collections")] = () => CreatePageStack(new GalleryCollectionsPage().CreateContent()),
            [PageId("Advanced Collections")] = () => CreatePageStack(new AdvancedCollectionsPage().CreateContent()),
            [PageId("Selectors and Properties")] = () => CreatePageStack(new GallerySelectorsPropertiesPage().CreateContent()),
            [PageId("Data Inspectors")] = () => CreatePageStack(new GalleryDataInspectorsPage().CreateContent()),
            [PageId("Charts")] = () => CreatePageStack(new GalleryChartsPage().CreateContent()),
            [PageId("Navigation")] = () => CreatePageStack(new GalleryNavigationPage().CreateContent(FindPageInfo(pageInfos, "navigation"))),
            [PageId("Window Backdrops")] = () => CreatePageStack(new GalleryWindowBackdropsPage(owner).CreateContent()),
            [PageId("Materials and Effects")] = () => CreatePageStack(new GalleryMaterialsPage().CreateContent()),
            [PageId("Material Primitives")] = () => CreatePageStack(new MaterialsPage().CreateContent()),
            [PageId("Motion and Transitions")] = () => CreatePageStack(new GalleryMotionPage().CreateContent()),
            [PageId("Animated Controls")] = () => CreatePageStack(new MotionControlsPage().CreateContent()),
            [PageId("Menus")] = () => CreatePageStack(new GalleryMenusPage().CreateContent()),
            [PageId("Disclosure")] = () => CreatePageStack(new GalleryDisclosurePage().CreateContent()),
            [PageId("Advanced Interaction")] = () => CreatePageStack(new InteractionControlsPage().CreateContent()),
            [PageId("Status")] = () => CreatePageStack(new GalleryStatusPage().CreateContent()),
            [PageId("Design")] = () => CreatePageStack(new GalleryDesignPage().CreateContent()),
            [PageId("Settings")] = () => CreatePageStack(new GallerySettingsPage(applyTheme, applyAccent).CreateContent()),
            [PageId("Control Gap Matrix")] = () => CreatePageStack(new GalleryControlGapPage().CreateContent()),
            [PageId("Visual QA Coverage")] = () => CreatePageStack(new GalleryVisualQaCoveragePage().CreateContent()),
            [PageId("State Matrix")] = () => CreatePageStack(new GalleryStateMatrixPage().CreateContent())
        };

        return factories;
    }

    private static string PageId(string title) => GalleryCatalog.CreateUniqueId(title);

    private static Func<UIElement> CreatePlaceholderContent(GalleryPageInfo[] pageInfos, string uniqueId)
    {
        var info = FindPageInfo(pageInfos, uniqueId);
        return () => CreatePageStack(GalleryPlaceholderPage.Create(info));
    }

    private static GalleryPageInfo FindPageInfo(GalleryPageInfo[] pageInfos, string uniqueId)
    {
        return pageInfos.First(page => string.Equals(page.UniqueId, uniqueId, StringComparison.Ordinal));
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
