using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;

namespace FluentJalium.Tests;

public sealed class FluentGalleryCatalogTests
{
    [Fact]
    public void GalleryControlInfo_ShouldExpandCatalogPagesIntoControlIndex()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var controls = GalleryControlInfo.CreateFromPages(pages);

        Assert.Contains(controls, control => control.Name == "FWAutoSuggestBox" && control.Page.UniqueId == "textinput");
        Assert.Contains(controls, control => control.Name == "FWCalendarDatePicker" && control.Page.UniqueId == "dateandtime");
        Assert.Contains(controls, control => control.Name == "FWCalendarView" && control.Page.UniqueId == "dateandtime");
        Assert.Contains(controls, control => control.Name == "FWGridView" && control.Page.UniqueId == "collections");
        Assert.Contains(controls, control => control.Name == "FWSelectorBar" && control.Page.UniqueId == "navigation");
        Assert.Contains(controls, control => control.Name == "FWTabView" && control.Page.UniqueId == "navigation");
        Assert.Contains(controls, control => control.Name == "FWFlyout" && control.Page.UniqueId == "menus");
        Assert.Contains(controls, control => control.Name == "FWFlyoutPresenter" && control.Page.UniqueId == "menus");
        Assert.Contains(controls, control => control.Name == "FWSettingsCard" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWSettingsExpander" && control.Page.UniqueId == "disclosure");
        Assert.Contains(controls, control => control.Name == "FWTaskDialog" && control.Page.UniqueId == "disclosure");
        Assert.Contains(controls, control => control.Name == "FWSnackbar" && control.Page.UniqueId == "status");
        Assert.Contains(controls, control => control.Name == "FWSplitView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWTwoPaneView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWParallaxView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWSettingsCardDiagnostics" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWRadioButtons" && control.Page.UniqueId == "selection");
        Assert.Contains(controls, control => control.Name == "FWScrollViewer" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWSwipeControl" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWGridSplitter" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWRefreshContainerDiagnostics" && control.Page.UniqueId == "advancedinteraction");
        Assert.Contains(controls, control => control.Name == "FWScrollerViewportDiagnostics" && control.Page.UniqueId == "advancedinteraction");
        Assert.Contains(controls, control => control.Name == "FWAnnotatedScrollBarDiagnostics" && control.Page.UniqueId == "advancedinteraction");
    }

    [Fact]
    public void GalleryControlInfo_ShouldPreserveControlMetadataForFilters()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var controls = GalleryControlInfo.CreateFromPages(pages);

        var tabView = Assert.Single(controls, control => control.Name == "FWTabView");
        Assert.True(tabView.IsUpdated);
        Assert.Equal(GalleryPageStatus.Stable, tabView.Status);
        Assert.Equal("FluentJalium.Controls", tabView.ApiNamespace);
        Assert.Equal("navigation.breadcrumb.pips.selector.tabview.titlebar", tabView.SampleCodeKey);
        Assert.Contains("Selector", tabView.BaseClasses);
        Assert.Equal("/Navigation/FWNavigationService/FWTabView", tabView.SourcePath);

        var itemsRepeater = Assert.Single(controls, control => control.Name == "FWItemsRepeater");
        Assert.True(itemsRepeater.IsNew);
        Assert.Equal(GalleryPageStatus.Preview, itemsRepeater.Status);

        var selectorBarDiagnostics = Assert.Single(controls, control => control.Name == "FWSelectorBarDiagnostics");
        Assert.True(selectorBarDiagnostics.IsUpdated);
        Assert.Equal("navigation.breadcrumb.pips.selector.tabview.titlebar", selectorBarDiagnostics.SampleCodeKey);
        Assert.Contains("Selector", selectorBarDiagnostics.BaseClasses);
        Assert.Equal("/Navigation/FWNavigationService/FWSelectorBarDiagnostics", selectorBarDiagnostics.SourcePath);

        var tabViewDiagnostics = Assert.Single(controls, control => control.Name == "FWTabViewDiagnostics");
        Assert.True(tabViewDiagnostics.IsUpdated);
        Assert.Equal("navigation.breadcrumb.pips.selector.tabview.titlebar", tabViewDiagnostics.SampleCodeKey);
        Assert.Contains("TabItem", tabViewDiagnostics.BaseClasses);
        Assert.Equal("/Navigation/FWNavigationService/FWTabViewDiagnostics", tabViewDiagnostics.SourcePath);

        var flyout = Assert.Single(controls, control => control.Name == "FWFlyout");
        Assert.True(flyout.IsUpdated);
        Assert.Equal("menus.flyout.commandbar", flyout.SampleCodeKey);
        Assert.Contains("FlyoutBase", flyout.BaseClasses);
        Assert.Equal("/Menus/FWFlyout", flyout.SourcePath);

        var splitView = Assert.Single(controls, control => control.Name == "FWSplitView");
        Assert.True(splitView.IsUpdated);
        Assert.Equal("layout.splitview.settingscard", splitView.SampleCodeKey);
        Assert.Contains("ContentControl", splitView.BaseClasses);
        Assert.Equal("/Layout/FWSplitView", splitView.SourcePath);

        var scrollViewer = Assert.Single(controls, control => control.Name == "FWScrollViewer");
        Assert.True(scrollViewer.IsUpdated);
        Assert.Equal("interaction.scrollviewer.swipe.splitter", scrollViewer.SampleCodeKey);
        Assert.Equal("FluentJalium.Controls", scrollViewer.ApiNamespace);
        Assert.Contains("ScrollViewer", scrollViewer.BaseClasses);
        Assert.Equal("/Interaction/FWScrollViewer", scrollViewer.SourcePath);

        var swipeControl = Assert.Single(controls, control => control.Name == "FWSwipeControl");
        Assert.Equal("/Interaction/FWScrollViewer/FWSwipeControl", swipeControl.SourcePath);

        var refreshDiagnostics = Assert.Single(controls, control => control.Name == "FWRefreshContainerDiagnostics");
        Assert.True(refreshDiagnostics.IsUpdated);
        Assert.Equal(GalleryPageStatus.Preview, refreshDiagnostics.Status);
        Assert.Equal("advancedinteraction.scroller", refreshDiagnostics.SampleCodeKey);
        Assert.Contains("ContentControl", refreshDiagnostics.BaseClasses);
        Assert.Equal("/Interaction/FWScroller/FWRefreshContainerDiagnostics", refreshDiagnostics.SourcePath);

        var annotatedDiagnostics = Assert.Single(controls, control => control.Name == "FWAnnotatedScrollBarDiagnostics");
        Assert.True(annotatedDiagnostics.IsUpdated);
        Assert.Equal(GalleryPageStatus.Preview, annotatedDiagnostics.Status);
        Assert.Equal("advancedinteraction.scroller", annotatedDiagnostics.SampleCodeKey);
        Assert.Contains("ScrollBar", annotatedDiagnostics.BaseClasses);
        Assert.Equal("/Interaction/FWScroller/FWAnnotatedScrollBarDiagnostics", annotatedDiagnostics.SourcePath);
    }

    [Fact]
    public void GalleryCatalog_ShouldExposeFormsPatternMetadata()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        var forms = Assert.Single(pages, page => page.UniqueId == "forms");
        Assert.Equal("Forms", forms.Title);
        Assert.Equal(GalleryNavigationGroup.Input, forms.Group);
        Assert.True(forms.IsNew);
        Assert.Equal("/Patterns/Forms", forms.SourcePath);
        Assert.Equal("FluentJalium.Controls", forms.ApiNamespace);
        Assert.Equal("patterns.forms", forms.SampleCodeKey);
        Assert.Contains("FWLabel", forms.RelatedControls);
        Assert.Contains("FWTextBox", forms.RelatedControls);
        Assert.Contains("FWAutoSuggestBox", forms.RelatedControls);
        Assert.Contains("FWRadioButtons", forms.RelatedControls);
        Assert.Contains("FWInfoBar", forms.RelatedControls);
        Assert.Contains("FWSettingsCard", forms.RelatedControls);
        Assert.Contains("FWToggleSwitch", forms.RelatedControls);
        Assert.Contains("FWButton", forms.RelatedControls);
        Assert.Contains("Selector", forms.BaseClasses!);
        Assert.True(forms.MatchesSearch("forms validation"));
        Assert.True(forms.MatchesSearch("settingscard submit"));
    }

    [Fact]
    public void GalleryControlInfo_ShouldPreferControlFamilyPagesOverPatternPages()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var controls = GalleryControlInfo.CreateFromPages(pages);

        Assert.Equal("visuals", Assert.Single(controls, control => control.Name == "FWLabel").Page.UniqueId);
        Assert.Equal("textinput", Assert.Single(controls, control => control.Name == "FWTextBox").Page.UniqueId);
        Assert.Equal("textinput", Assert.Single(controls, control => control.Name == "FWAutoSuggestBox").Page.UniqueId);
        Assert.Equal("selection", Assert.Single(controls, control => control.Name == "FWRadioButtons").Page.UniqueId);
        Assert.Equal("status", Assert.Single(controls, control => control.Name == "FWInfoBar").Page.UniqueId);
        Assert.Equal("contentandlayout", Assert.Single(controls, control => control.Name == "FWSettingsCard").Page.UniqueId);
        Assert.Equal("switches", Assert.Single(controls, control => control.Name == "FWToggleSwitch").Page.UniqueId);
        Assert.Equal("buttons", Assert.Single(controls, control => control.Name == "FWButton").Page.UniqueId);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeNavigationSelectorDiagnosticsSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "navigation");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWSelectorBar", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("SelectedText", sampleCode);
        Assert.Contains("SelectedHeader", sampleCode);
        Assert.Contains("ItemCount", sampleCode);
        Assert.Contains("new FWAutoSuggestBox", sampleCode);
        Assert.Contains("new FWBreadcrumbBar", sampleCode);
        Assert.Contains("new FWNavigationService", sampleCode);
        Assert.Contains("new FWPipsPager", sampleCode);
        Assert.Contains("new FWSelectorBar", sampleCode);
        Assert.Contains("new FWTabView", sampleCode);
        Assert.Contains("App shell", sampleCode);
        Assert.Contains("HasPageTypeProvider", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeSplitViewAndSettingsCardLayoutSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "contentandlayout");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWSplitView", sampleCode);
        Assert.Contains("FWSplitViewDisplayMode.CompactInline", sampleCode);
        Assert.Contains("ActualPaneLength", sampleCode);
        Assert.Contains("new FWSettingsCard", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("IsInvokable", sampleCode);
        Assert.Contains("IsInteractionPressed", sampleCode);
        Assert.Contains("GetAutomationDiagnostics", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeSettingsExpanderItemHostTemplateSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "disclosure");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWSettingsExpander", sampleCode);
        Assert.Contains("ItemsSource = new[]", sampleCode);
        Assert.Contains("new SettingsRow", sampleCode);
        Assert.Contains("PreviewMaterialCommand", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeScrollViewerSwipeAndSplitterSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "interaction");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWScrollViewer", sampleCode);
        Assert.Contains("IsScrollBarAutoHideEnabled = false", sampleCode);
        Assert.Contains("ScrollToVerticalOffset", sampleCode);
        Assert.Contains("new FWSwipeControl", sampleCode);
        Assert.Contains("new SwipeItems", sampleCode);
        Assert.Contains("new FWGridSplitter", sampleCode);
        Assert.Contains("KeyboardIncrement = 12", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeFormsPatternSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "forms");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWLabel", sampleCode);
        Assert.Contains("new FWTextBox", sampleCode);
        Assert.Contains("new FWAutoSuggestBox", sampleCode);
        Assert.Contains("new FWRadioButtons", sampleCode);
        Assert.Contains("new FWInfoBar", sampleCode);
        Assert.Contains("new FWToggleSwitch", sampleCode);
        Assert.Contains("new FWSettingsCard", sampleCode);
        Assert.Contains("new FWButton", sampleCode);
        Assert.Contains("ValidateAndSubmit", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("forms.submit", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeWindowBackdropDiagnosticsSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "windowbackdrops");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWFluentWindowSurface", sampleCode);
        Assert.Contains("FWFluentWindowMaterialProfile.MicaShell", sampleCode);
        Assert.Contains("FWFluentWindowMaterialProfileRecipe.Create", sampleCode);
        Assert.Contains("profileRecipe.SystemBackdrop", sampleCode);
        Assert.Contains("ApplyWindowMaterialProfile", sampleCode);
        Assert.Contains("FWFluentWindowMaterialProfile.FocusGlassShell", sampleCode);
        Assert.Contains("ApplyWindowBackdrop", sampleCode);
        Assert.Contains("window.SystemBackdrop", sampleCode);
        Assert.Contains("isMatched", sampleCode);
        Assert.Contains("Window backdrop QA", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeAdvancedInteractionScrollerDiagnosticsSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "advancedinteraction");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWRefreshContainer", sampleCode);
        Assert.Contains("RefreshRequested", sampleCode);
        Assert.Contains("RequestRefresh", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("PullProgress", sampleCode);
        Assert.Contains("new FWScroller", sampleCode);
        Assert.Contains("AttachScrollViewer", sampleCode);
        Assert.Contains("GetViewportDiagnostics", sampleCode);
        Assert.Contains("ViewportWidth", sampleCode);
        Assert.Contains("VerticalOffset", sampleCode);
        Assert.Contains("new FWAnnotatedScrollBar", sampleCode);
        Assert.Contains("DetailLabelRequested", sampleCode);
        Assert.Contains("FWAnnotatedScrollBarDiagnostics", sampleCode);
        Assert.Contains("RegisteredLabelCount", sampleCode);
    }

    [Fact]
    public void GalleryDesignPages_ShouldExposeMetadataAndSampleKeys()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        var themeArchitecture = Assert.Single(pages, page => page.UniqueId == "themearchitecture");
        Assert.True(themeArchitecture.IsUpdated);
        Assert.Equal("/Design/ThemeArchitecture", themeArchitecture.SourcePath);
        Assert.Equal("FluentJalium.Controls.Themes", themeArchitecture.ApiNamespace);
        Assert.Equal("design.themearchitecture", themeArchitecture.SampleCodeKey);
        Assert.Contains("ResourceDictionary", themeArchitecture.BaseClasses!);

        var colors = Assert.Single(pages, page => page.UniqueId == "colors");
        Assert.Equal("/Design/Colors", colors.SourcePath);
        Assert.Equal("design.colors", colors.SampleCodeKey);
        Assert.Contains("FluentColors", colors.RelatedControls);

        var typography = Assert.Single(pages, page => page.UniqueId == "typography");
        Assert.Equal("/Design/Typography", typography.SourcePath);
        Assert.Equal("design.typography", typography.SampleCodeKey);
        Assert.Contains("FontFamily", typography.BaseClasses!);

        var geometry = Assert.Single(pages, page => page.UniqueId == "geometry");
        Assert.Equal("/Design/Geometry", geometry.SourcePath);
        Assert.Equal("design.geometry", geometry.SampleCodeKey);
        Assert.Contains("CornerRadius", geometry.BaseClasses!);

        var motion = Assert.Single(pages, page => page.UniqueId == "motiontokens");
        Assert.Equal("/Design/MotionTokens", motion.SourcePath);
        Assert.Equal("design.motiontokens", motion.SampleCodeKey);
        Assert.Contains("TransitionMode", motion.RelatedControls);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeDesignTokenSamples()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        AssertDesignSample(pages, "themearchitecture", "FluentThemeManager.Apply", "FluentControlsResourceName");
        AssertDesignSample(pages, "colors", "AccentFillColorDefaultBrush", "SelectionBackgroundWeak");
        AssertDesignSample(pages, "typography", "CurrentDisplayFontFamily", "ControlContentThemeFontSize");
        AssertDesignSample(pages, "geometry", "CardCornerRadius", "FluentOverlayBorderThickness");
        AssertDesignSample(pages, "motiontokens", "FWConnectedAnimationService", "FluentMotionDurationNormal");
    }

    [Fact]
    public void FluentControlBacklogMatrix_ShouldKeepReferenceAndExecutionCoverage()
    {
        var document = File.ReadAllText(FindRepositoryFile("docs", "FLUENT_CONTROL_BACKLOG_MATRIX.md"));

        Assert.Contains("WinUI / WinUI Gallery", document);
        Assert.Contains("WPF UI", document);
        Assert.Contains("UI.WPF.Modern", document);
        Assert.Contains("FluentAvalonia", document);
        Assert.Contains("Community Toolkit", document);
        Assert.Contains("FW*", document);
        Assert.Contains("P0", document);
        Assert.Contains("P1", document);
        Assert.Contains("Evaluate", document);
        Assert.Contains("TaskDialog real-window QA", document);
        Assert.Contains("ItemsRepeater visual QA", document);
        Assert.Contains("Settings visual QA", document);
        Assert.Contains("Navigation app-shell recipes", document);
        Assert.Contains("Forms pattern Gallery page", document);
        Assert.Contains("FWSemanticZoom", document);
        Assert.Contains("FWFlipView", document);
        Assert.Contains("IFluentJaliumControl", document);
        Assert.Contains("Gallery changes must include catalog metadata", document);
    }

    private static string FindRepositoryFile(params string[] pathSegments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathSegments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file '{Path.Combine(pathSegments)}'.", Path.Combine(pathSegments));
    }

    private static void AssertDesignSample(GalleryPageInfo[] pages, string uniqueId, string firstExpected, string secondExpected)
    {
        var page = Assert.Single(pages, page => page.UniqueId == uniqueId);

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains(firstExpected, sampleCode);
        Assert.Contains(secondExpected, sampleCode);
    }
}
