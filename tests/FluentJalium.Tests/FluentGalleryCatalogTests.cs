using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;

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
        Assert.Contains(controls, control => control.Name == "FWSnackbarPresenterDiagnostics" && control.Page.UniqueId == "status");
        Assert.Contains(controls, control => control.Name == "FWSplitView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWSplitViewDiagnostics" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWTwoPaneView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWTwoPaneViewDiagnostics" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWParallaxView" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWParallaxViewDiagnostics" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWSettingsCardDiagnostics" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWRadioButtons" && control.Page.UniqueId == "selection");
        Assert.Contains(controls, control => control.Name == "FWComboBoxItem" && control.Page.UniqueId == "selection");
        Assert.Contains(controls, control => control.Name == "FWScrollViewer" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWScrollBar" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWSwipeControl" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWGridSplitter" && control.Page.UniqueId == "interaction");
        Assert.Contains(controls, control => control.Name == "FWWebView" && control.Page.UniqueId == "inputandmedia");
        Assert.Contains(controls, control => control.Name == "FWWebViewDiagnostics" && control.Page.UniqueId == "inputandmedia");
        Assert.Contains(controls, control => control.Name == "FWRefreshContainerDiagnostics" && control.Page.UniqueId == "advancedinteraction");
        Assert.Contains(controls, control => control.Name == "FWScrollerViewportDiagnostics" && control.Page.UniqueId == "advancedinteraction");
        Assert.Contains(controls, control => control.Name == "FWAnnotatedScrollBarDiagnostics" && control.Page.UniqueId == "advancedinteraction");
        Assert.Contains(controls, control => control.Name == "FWItemsRepeaterDiagnostics" && control.Page.UniqueId == "advancedcollections");
        Assert.Contains(controls, control => control.Name == "FWFluentWindowSurfaceDiagnostics" && control.Page.UniqueId == "windowbackdrops");
    }

    [Fact]
    public void GalleryControlInfo_ShouldExposeImplementedFamilySubControls()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var controls = GalleryControlInfo.CreateFromPages(pages);

        AssertCatalogControl(controls, "FWAppBarSeparator", "buttons");
        AssertCatalogControl(controls, "FWToolBarTray", "buttons");

        AssertCatalogControl(controls, "FWTextBlock", "contentandlayout");
        AssertCatalogControl(controls, "FWAccessText", "contentandlayout");
        AssertCatalogControl(controls, "FWCanvas", "contentandlayout");
        AssertCatalogControl(controls, "FWBorder", "contentandlayout");
        AssertCatalogControl(controls, "FWContentPresenter", "contentandlayout");
        AssertCatalogControl(controls, "FWWrapPanel", "contentandlayout");
        AssertCatalogControl(controls, "FWRelativePanel", "contentandlayout");

        AssertCatalogControl(controls, "FWLine", "visuals");
        AssertCatalogControl(controls, "FWPolyline", "visuals");
        AssertCatalogControl(controls, "FWPolygon", "visuals");

        AssertCatalogControl(controls, "FWListBoxItem", "collections");
        AssertCatalogControl(controls, "FWListViewItem", "collections");
        AssertCatalogControl(controls, "FWTreeViewItem", "collections");

        AssertCatalogControl(controls, "FWTabControl", "navigation");
        AssertCatalogControl(controls, "FWTabItem", "navigation");

        AssertCatalogControl(controls, "FWMenuBarItem", "menus");
        AssertCatalogControl(controls, "FWMenuItem", "menus");
        AssertCatalogControl(controls, "FWToggleMenuFlyoutItem", "menus");
        AssertCatalogControl(controls, "FWMenuFlyoutSeparator", "menus");

        AssertCatalogControl(controls, "FWTwoPaneViewDiagnostics", "contentandlayout");
        AssertCatalogControl(controls, "FWParallaxViewDiagnostics", "contentandlayout");
        AssertCatalogControl(controls, "FWFluentWindowSurfaceDiagnostics", "windowbackdrops");
        AssertCatalogControl(controls, "FWSnackbarPresenterDiagnostics", "status");
        AssertCatalogControl(controls, "FWSnackbarHostDiagnostics", "status");
        AssertCatalogControl(controls, "FWToastNotificationItem", "status");
        AssertCatalogControl(controls, "FWStatusBarItem", "status");
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

        var itemsRepeaterDiagnostics = Assert.Single(controls, control => control.Name == "FWItemsRepeaterDiagnostics");
        Assert.True(itemsRepeaterDiagnostics.IsNew);
        Assert.Equal(GalleryPageStatus.Preview, itemsRepeaterDiagnostics.Status);
        Assert.Equal("advancedcollections.itemsrepeater", itemsRepeaterDiagnostics.SampleCodeKey);
        Assert.Contains("Panel", itemsRepeaterDiagnostics.BaseClasses);
        Assert.Equal("/Collections/FWItemsRepeater/FWItemsRepeaterDiagnostics", itemsRepeaterDiagnostics.SourcePath);

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

        var splitViewDiagnostics = Assert.Single(controls, control => control.Name == "FWSplitViewDiagnostics");
        Assert.True(splitViewDiagnostics.IsUpdated);
        Assert.Equal("layout.splitview.settingscard", splitViewDiagnostics.SampleCodeKey);
        Assert.Contains("ContentControl", splitViewDiagnostics.BaseClasses);
        Assert.Equal("/Layout/FWSplitView/FWSplitViewDiagnostics", splitViewDiagnostics.SourcePath);

        var twoPaneDiagnostics = Assert.Single(controls, control => control.Name == "FWTwoPaneViewDiagnostics");
        Assert.True(twoPaneDiagnostics.IsUpdated);
        Assert.Equal("layout.splitview.settingscard", twoPaneDiagnostics.SampleCodeKey);
        Assert.Contains("ContentControl", twoPaneDiagnostics.BaseClasses);
        Assert.Equal("/Layout/FWSplitView/FWTwoPaneViewDiagnostics", twoPaneDiagnostics.SourcePath);

        var parallaxDiagnostics = Assert.Single(controls, control => control.Name == "FWParallaxViewDiagnostics");
        Assert.True(parallaxDiagnostics.IsUpdated);
        Assert.Equal("layout.splitview.settingscard", parallaxDiagnostics.SampleCodeKey);
        Assert.Contains("ContentControl", parallaxDiagnostics.BaseClasses);
        Assert.Equal("/Layout/FWSplitView/FWParallaxViewDiagnostics", parallaxDiagnostics.SourcePath);

        var windowSurfaceDiagnostics = Assert.Single(controls, control => control.Name == "FWFluentWindowSurfaceDiagnostics");
        Assert.True(windowSurfaceDiagnostics.IsUpdated);
        Assert.Equal("materials.windowbackdrop", windowSurfaceDiagnostics.SampleCodeKey);
        Assert.Contains("Border", windowSurfaceDiagnostics.BaseClasses);
        Assert.Equal("/Materials/FWFluentWindowSurface/FWFluentWindowSurfaceDiagnostics", windowSurfaceDiagnostics.SourcePath);

        var snackbarPresenterDiagnostics = Assert.Single(controls, control => control.Name == "FWSnackbarPresenterDiagnostics");
        Assert.True(snackbarPresenterDiagnostics.IsUpdated);
        Assert.Equal("status.snackbar", snackbarPresenterDiagnostics.SampleCodeKey);
        Assert.Contains("ContentControl", snackbarPresenterDiagnostics.BaseClasses);
        Assert.Equal("/Status/FWSnackbar/FWSnackbarPresenterDiagnostics", snackbarPresenterDiagnostics.SourcePath);

        var scrollViewer = Assert.Single(controls, control => control.Name == "FWScrollViewer");
        Assert.True(scrollViewer.IsUpdated);
        Assert.Equal("interaction.scrollviewer.swipe.splitter", scrollViewer.SampleCodeKey);
        Assert.Equal("FluentJalium.Controls", scrollViewer.ApiNamespace);
        Assert.Contains("ScrollViewer", scrollViewer.BaseClasses);
        Assert.Equal("/Interaction/FWScrollViewer", scrollViewer.SourcePath);

        var swipeControl = Assert.Single(controls, control => control.Name == "FWSwipeControl");
        Assert.Equal("/Interaction/FWScrollViewer/FWSwipeControl", swipeControl.SourcePath);

        var webView = Assert.Single(controls, control => control.Name == "FWWebView");
        Assert.True(webView.IsUpdated);
        Assert.Equal("inputmedia.color.ink.media", webView.SampleCodeKey);
        Assert.Equal("FluentJalium.Controls", webView.ApiNamespace);
        Assert.Contains("WebView", webView.BaseClasses);
        Assert.Equal("/InputMedia/FWColorPicker/FWWebView", webView.SourcePath);

        var webViewDiagnostics = Assert.Single(controls, control => control.Name == "FWWebViewDiagnostics");
        Assert.True(webViewDiagnostics.IsUpdated);
        Assert.Equal("inputmedia.color.ink.media", webViewDiagnostics.SampleCodeKey);
        Assert.Contains("WebView", webViewDiagnostics.BaseClasses);
        Assert.Equal("/InputMedia/FWColorPicker/FWWebViewDiagnostics", webViewDiagnostics.SourcePath);

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
    public void GalleryTextInputPage_ShouldExposeAutoSuggestReasonMetadataAndSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "textinput");

        Assert.Equal("textinput.autosuggestbox", page.SampleCodeKey);
        Assert.Contains("AutoSuggestTextChanged", page.Tags);
        Assert.Contains("TextChangeReason", page.Tags);
        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWAutoSuggestBox", sampleCode);
        Assert.Contains("AutoSuggestTextChanged", sampleCode);
        Assert.Contains("args.Reason", sampleCode);
        Assert.Contains("QuerySubmitted", sampleCode);
        Assert.Contains("ChosenSuggestion", sampleCode);
        Assert.Contains("FWAutoSuggestBoxTextChangeReason.UserInput", sampleCode);
        Assert.Contains("Formatting recipe", sampleCode);
        Assert.Contains("FormatPhoneRecipe", sampleCode);
        Assert.Contains("FormatLicenseKeyRecipe", sampleCode);
        Assert.Contains("CreateFormattingRecipeSnapshot", sampleCode);
        Assert.Contains("FormatFormattingRecipeQa", sampleCode);
        Assert.Contains("IsRecipeOnly", sampleCode);
        Assert.DoesNotContain("new FWMaskedTextBox", sampleCode);
    }

    [Fact]
    public void GalleryInputMediaPage_ShouldExposeWebViewMetadataAndSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "inputandmedia");

        Assert.Equal("inputmedia.color.ink.media", page.SampleCodeKey);
        Assert.Equal("FluentJalium.Controls", page.ApiNamespace);
        Assert.Contains("FWWebView", page.RelatedControls);
        Assert.Contains("FWWebViewDiagnostics", page.RelatedControls);
        Assert.Contains("WebView", page.BaseClasses!);
        Assert.True(page.MatchesSearch("webview2 browser diagnostics"));

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWColorPicker", sampleCode);
        Assert.Contains("new FWInkCanvas", sampleCode);
        Assert.Contains("new FWMediaElement", sampleCode);
        Assert.Contains("new FWWebView", sampleCode);
        Assert.Contains("NavigateToString", sampleCode);
        Assert.Contains("GalleryInputMediaPage.CreateWebViewSampleHtml", sampleCode);
        Assert.Contains("FWWebViewDiagnostics", sampleCode);
        Assert.Contains("FormatWebViewDiagnostics", sampleCode);

        var html = GalleryInputMediaPage.CreateWebViewSampleHtml("FluentJalium WebView");
        Assert.Contains("<title>FluentJalium WebView</title>", html);
        Assert.Contains("Inline HTML keeps the Gallery sample deterministic", html);

        var diagnostics = new FluentJalium.Controls.FWWebViewDiagnostics(
            new Uri("https://example.com/"),
            "Example",
            CanGoBack: false,
            CanGoForward: true,
            IsInitialized: false,
            IsNavigating: false,
            ZoomFactor: 1.25,
            DefaultBackgroundColor: Jalium.UI.Media.Colors.White,
            InitializationError: null);
        var formatted = GalleryInputMediaPage.FormatWebViewDiagnostics("Sample", diagnostics);

        Assert.Contains("Sample: source https://example.com/", formatted);
        Assert.Contains("title Example", formatted);
        Assert.Contains("initialized off", formatted);
        Assert.Contains("back/forward off/on", formatted);
        Assert.Contains("zoom 1.25", formatted);
        Assert.Contains("error none", formatted);
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
        Assert.Contains("TrySelectIndex", sampleCode);
        Assert.Contains("SelectedText", sampleCode);
        Assert.Contains("SelectedHeader", sampleCode);
        Assert.Contains("ItemCount", sampleCode);
        Assert.Contains("new FWAutoSuggestBox", sampleCode);
        Assert.Contains("new FWBreadcrumbBar", sampleCode);
        Assert.Contains("new FWNavigationService", sampleCode);
        Assert.Contains("new FWPipsPager", sampleCode);
        Assert.Contains("new FWSelectorBar", sampleCode);
        Assert.Contains("new FWTabView", sampleCode);
        Assert.Contains("TryMoveTab", sampleCode);
        Assert.Contains("CanReorderTabs", sampleCode);
        Assert.Contains("App shell", sampleCode);
        Assert.Contains("CreateNavigationShellQaSnapshot", sampleCode);
        Assert.Contains("FormatNavigationShellQa", sampleCode);
        Assert.Contains("App shell QA", sampleCode);
        Assert.Contains("HasPageTypeProvider", sampleCode);
        Assert.Contains("IsAppShellReady", sampleCode);
        Assert.Contains("HasRouteProviderCoverage", sampleCode);
        Assert.Contains("HasFooterSettingsCoverage", sampleCode);
        Assert.Contains("HasSearchRouteCoverage", sampleCode);
        Assert.Contains("HasDocumentWorkspaceCoverage", sampleCode);
        Assert.Contains("HasPageNavigationCoverage", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeFlyoutPresenterQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "menus");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWFlyout", sampleCode);
        Assert.Contains("CreateFlyoutQaSnapshot", sampleCode);
        Assert.Contains("FormatFlyoutQa", sampleCode);
        Assert.Contains("Flyout QA", sampleCode);
        Assert.Contains("new FWMenuBar", sampleCode);
        Assert.Contains("new FWMenuFlyout", sampleCode);
        Assert.Contains("new FWCommandBarFlyout", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeDateTimeCompatibilityQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "dateandtime");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWCalendarDatePicker", sampleCode);
        Assert.Contains("new FWCalendarView", sampleCode);
        Assert.Contains("CreateCalendarDatePickerQaSnapshot", sampleCode);
        Assert.Contains("CreateCalendarViewQaSnapshot", sampleCode);
        Assert.Contains("FormatCalendarDatePickerQa", sampleCode);
        Assert.Contains("FormatCalendarViewQa", sampleCode);
        Assert.Contains("CalendarDatePicker QA", sampleCode);
        Assert.Contains("CalendarView QA", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeAdvancedCollectionNavigationRecipes()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "advancedcollections");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWItemsRepeater", sampleCode);
        Assert.Contains("AttachViewport", sampleCode);
        Assert.Contains("FWItemsRepeaterDiagnostics", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("CreateItemsRepeaterVisualQaSnapshot", sampleCode);
        Assert.Contains("FormatItemsRepeaterVisualQa", sampleCode);
        Assert.Contains("ItemsViewSelection", sampleCode);
        Assert.Contains("FlipViewPaging", sampleCode);
        Assert.Contains("SemanticZoomGrouping", sampleCode);
        Assert.Contains("new FWPipsPager", sampleCode);
        Assert.Contains("ApplyCollectionRecipeCommand", sampleCode);
        Assert.Contains("CreateCollectionRecipeDiagnosticsText", sampleCode);
        Assert.Contains("SelectedIndex", sampleCode);
        Assert.Contains("InvokedIndex", sampleCode);
        Assert.Contains("CreateCollectionNavigationEvaluations", sampleCode);
        Assert.Contains("CreateCollectionNavigationEvaluationSummary", sampleCode);
        Assert.Contains("CreateCollectionNavigationEvidenceSummary", sampleCode);
        Assert.Contains("FormatCollectionNavigationEvaluation", sampleCode);
        Assert.Contains("FormatCollectionNavigationEvidence", sampleCode);
        Assert.Contains("Recipe evidence", sampleCode);
        Assert.Contains("Missing public API evidence", sampleCode);
        Assert.Contains("MissingPublicApiEvidence", sampleCode);
        Assert.Contains("FWItemsView", sampleCode);
        Assert.Contains("FWFlipView", sampleCode);
        Assert.Contains("FWSemanticZoom", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeChartVisualQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "charts");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWLineChart", sampleCode);
        Assert.Contains("new FWBarChart", sampleCode);
        Assert.Contains("new FWPieChart", sampleCode);
        Assert.Contains("new FWChartLegend", sampleCode);
        Assert.Contains("new FWChartTooltip", sampleCode);
        Assert.Contains("CreateLineSeries", sampleCode);
        Assert.Contains("CreateChartVisualQaSnapshot", sampleCode);
        Assert.Contains("FormatChartVisualQa", sampleCode);
        Assert.Contains("Chart visual QA", sampleCode);
        Assert.Contains("CreateLegendTooltipQaSnapshot", sampleCode);
        Assert.Contains("FormatLegendTooltipVisualQa", sampleCode);
        Assert.Contains("Legend tooltip QA", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeVisualsDeepSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "visuals");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWPersonPicture", sampleCode);
        Assert.Contains("new FWMarkdown", sampleCode);
        Assert.Contains("LinkClicked", sampleCode);
        Assert.Contains("args.Handled = true", sampleCode);
        Assert.Contains("new FWQRCode", sampleCode);
        Assert.Contains("QRCodeErrorCorrectionLevel.Q", sampleCode);
        Assert.Contains("QRModuleShape.RoundedSquare", sampleCode);
        Assert.Contains("QREyeShape.Rounded", sampleCode);
        Assert.Contains("QuietZoneModules = 3", sampleCode);
        Assert.Contains("new FWRectangle", sampleCode);
        Assert.Contains("new FWEllipse", sampleCode);
        Assert.Contains("new FWLine", sampleCode);
        Assert.Contains("new FWPolyline", sampleCode);
        Assert.Contains("new FWPolygon", sampleCode);
        Assert.Contains("new FWPath", sampleCode);
        Assert.Contains("ShapePointCollection.Parse", sampleCode);
        Assert.Contains("CreateShapeControlsQaSnapshot", sampleCode);
        Assert.Contains("FormatShapeControlsVisualQa", sampleCode);
        Assert.Contains("Shape controls QA", sampleCode);
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
        Assert.Contains("FWSplitViewDiagnostics", sampleCode);
        Assert.Contains("FormatSplitViewDiagnostics", sampleCode);
        Assert.Contains("new FWSettingsCard", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("IsInvokable", sampleCode);
        Assert.Contains("IsInteractionPressed", sampleCode);
        Assert.Contains("GetAutomationDiagnostics", sampleCode);
        Assert.Contains("Settings visual QA", sampleCode);
        Assert.Contains("CreateSettingsVisualQaSnapshot", sampleCode);
        Assert.Contains("FormatSettingsVisualQa", sampleCode);
        Assert.Contains("IsSettingsVisualQaReady", sampleCode);
        Assert.Contains("HasAdaptiveLayoutEvidence", sampleCode);
        Assert.Contains("HasPrimaryCommandEvidence", sampleCode);
        Assert.Contains("HasHoverStateEvidence", sampleCode);
        Assert.Contains("HasDisabledRowEvidence", sampleCode);
        Assert.Contains("HasAutomationEvidence", sampleCode);
        Assert.Contains("ClickMode.Hover", sampleCode);
        Assert.Contains("disabledCard", sampleCode);
        Assert.Contains("IsEnabled = false", sampleCode);
        Assert.Contains("new FWTwoPaneView", sampleCode);
        Assert.Contains("FWTwoPaneViewDiagnostics", sampleCode);
        Assert.Contains("FWParallaxViewDiagnostics", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldKeepSettingsExpanderItemHostTemplateSampleRegistered()
    {
        Assert.True(GallerySampleCodeRegistry.TryGetRegisteredSampleCode("disclosure.settings.teachingtip", out var sampleCode));
        Assert.Contains("new FWSettingsExpander", sampleCode);
        Assert.Contains("ItemsSource = new[]", sampleCode);
        Assert.Contains("new SettingsRow", sampleCode);
        Assert.Contains("PreviewMaterialCommand", sampleCode);
        Assert.Contains("new FWTeachingTip", sampleCode);
        Assert.Contains("Target = target", sampleCode);
        Assert.Contains("HeroContent = new FWBorder", sampleCode);
        Assert.Contains("TeachingTipPlacementMode.Bottom", sampleCode);
        Assert.Contains("TeachingTipTailVisibility.Visible", sampleCode);
        Assert.Contains("CreateTeachingTipVisualQaSnapshot", sampleCode);
        Assert.Contains("FormatTeachingTipVisualQa", sampleCode);
        Assert.Contains("TeachingTip visual QA", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeTaskDialogRealWindowQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "disclosure");

        Assert.Equal("disclosure.taskdialog", page.SampleCodeKey);
        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));

        Assert.Contains("new FWTaskDialogHost", sampleCode);
        Assert.Contains("FocusRestoreTarget", sampleCode);
        Assert.Contains("Panel.SetZIndex", sampleCode);
        Assert.Contains("TaskDialog real-window QA", sampleCode);
        Assert.Contains("TaskDialog root-window smoke", sampleCode);
        Assert.Contains("CreateTaskDialogRealWindowQaSnapshot", sampleCode);
        Assert.Contains("FormatTaskDialogRealWindowQa", sampleCode);
        Assert.Contains("RestoreTiming", sampleCode);
        Assert.Contains("ClipGuard", sampleCode);
        Assert.Contains("RootWindowSmoke", sampleCode);
        Assert.Contains("restore timing", sampleCode);
        Assert.Contains("clip guard", sampleCode);
        Assert.Contains("new KeyEventArgs", sampleCode);
        Assert.Contains("Key.Tab", sampleCode);
        Assert.Contains("ModifierKeys.Shift", sampleCode);
        Assert.Contains("RequestLightDismiss", sampleCode);
        Assert.Contains("LastKeyboardRequest", sampleCode);
        Assert.Contains("GetAutomationDiagnostics", sampleCode);
        Assert.Contains("PrimaryButton.AutomationId", sampleCode);
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
    public void GallerySampleCodeRegistry_ShouldExposeDataInspectorsWorkbenchQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "datainspectors");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWDiffViewer", sampleCode);
        Assert.Contains("new FWHexEditor", sampleCode);
        Assert.Contains("new FWJsonTreeViewer", sampleCode);
        Assert.Contains("new FWFluentMaterialSurface", sampleCode);
        Assert.Contains("FWFluentMaterialKind.LiquidGlass", sampleCode);
        Assert.Contains("CreateDataInspectorWorkbenchSnapshot", sampleCode);
        Assert.Contains("FormatDataInspectorWorkbenchQa", sampleCode);
        Assert.Contains("Data Inspectors workbench QA", sampleCode);
        Assert.Contains("workbenchSnapshot.IsReady", sampleCode);
        Assert.Contains("ShowMinimap = true", sampleCode);
        Assert.Contains("IsReadOnly = true", sampleCode);
        Assert.Contains("ShowDataInterpretation = true", sampleCode);
        Assert.Contains("ExpandDepth = 2", sampleCode);
        Assert.Contains("MaxRenderDepth = 8", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeSelectorsPropertiesQaSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "selectorsandproperties");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWTreeSelector", sampleCode);
        Assert.Contains("new FWTreeSelectorItem", sampleCode);
        Assert.Contains("new FWPropertyGrid", sampleCode);
        Assert.Contains("TreeSelectorCheckCascadeMode.Cascade", sampleCode);
        Assert.Contains("new FWFluentMaterialSurface", sampleCode);
        Assert.Contains("FWFluentMaterialKind.LiquidGlass", sampleCode);
        Assert.Contains("CreateSelectorsPropertiesQaSnapshot", sampleCode);
        Assert.Contains("FormatSelectorsPropertiesQa", sampleCode);
        Assert.Contains("Selectors and properties QA", sampleCode);
        Assert.Contains("propertySnapshot.IsReady", sampleCode);
        Assert.Contains("ShowDescription = true", sampleCode);
        Assert.Contains("ShowToolBar = false", sampleCode);
        Assert.Contains("Density = FWPropertyGridDensity.Compact", sampleCode);
        Assert.Contains("IsReadOnly = true", sampleCode);
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
        Assert.Contains("new FWProgressBar", sampleCode);
        Assert.Contains("new FWButton", sampleCode);
        Assert.Contains("new FWNumberBox", sampleCode);
        Assert.Contains("ValidationIssue", sampleCode);
        Assert.Contains("validation summary", sampleCode, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isDirty", sampleCode);
        Assert.Contains("ResetDataFormDraft", sampleCode);
        Assert.Contains("SaveDataFormDraftAsync", sampleCode);
        Assert.Contains("CreateDataFormValidationIssues", sampleCode);
        Assert.Contains("FormatDataFormValidationSummary", sampleCode);
        Assert.Contains("RunSubmitAsync", sampleCode);
        Assert.Contains("Disable reviewer fields", sampleCode);
        Assert.Contains("Focus QA", sampleCode);
        Assert.Contains("Forms visual QA", sampleCode);
        Assert.Contains("GetDiagnostics", sampleCode);
        Assert.Contains("forms.submit", sampleCode);
        Assert.DoesNotContain("new FWForm", sampleCode);
    }

    [Fact]
    public void GalleryFormsPage_ShouldCreateDataFormValidationSnapshots()
    {
        var invalidIssues = GalleryFormsPage.CreateDataFormValidationIssues(
            title: "",
            hours: 0,
            owner: "",
            requiresReview: true);

        Assert.Contains(invalidIssues, issue => issue.Field == "Title" && issue.Severity == InfoBarSeverity.Error);
        Assert.Contains(invalidIssues, issue => issue.Field == "Hours" && issue.Severity == InfoBarSeverity.Error);
        Assert.Contains(invalidIssues, issue => issue.Field == "Owner" && issue.Severity == InfoBarSeverity.Error);

        var invalidSnapshot = GalleryFormsPage.CreateDataFormRecipeSnapshot(
            "Validate",
            invalidIssues,
            isDirty: true,
            isSaving: false);

        Assert.True(invalidSnapshot.IsDirty);
        Assert.False(invalidSnapshot.IsSaving);
        Assert.Equal(3, invalidSnapshot.IssueCount);
        Assert.Equal(InfoBarSeverity.Error, invalidSnapshot.Severity);
        Assert.Contains("Draft is dirty", invalidSnapshot.Summary);
        Assert.Contains("Title is required", invalidSnapshot.Summary);

        var warningIssues = GalleryFormsPage.CreateDataFormValidationIssues(
            title: "Long QA run",
            hours: 13,
            owner: "Gallery Operations",
            requiresReview: true);

        var warningSnapshot = GalleryFormsPage.CreateDataFormRecipeSnapshot(
            "Validate",
            warningIssues,
            isDirty: true,
            isSaving: false);

        Assert.Single(warningIssues);
        Assert.Equal(InfoBarSeverity.Warning, warningSnapshot.Severity);
        Assert.Contains("split before save", warningSnapshot.Summary);

        var validIssues = GalleryFormsPage.CreateDataFormValidationIssues(
            title: "Release checklist",
            hours: 6,
            owner: "",
            requiresReview: false);

        var savingSnapshot = GalleryFormsPage.CreateDataFormRecipeSnapshot(
            "SaveDataFormDraftAsync",
            validIssues,
            isDirty: true,
            isSaving: true);

        Assert.Empty(validIssues);
        Assert.Equal(InfoBarSeverity.Informational, savingSnapshot.Severity);
        Assert.Contains("ready to save", savingSnapshot.Summary);
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
        Assert.Contains("FWFluentWindowSurfaceDiagnostics", sampleCode);
        Assert.Contains("GetWindowSurfaceDiagnostics", sampleCode);
        Assert.Contains("GalleryWindowSurfaceDiagnostics.Create", sampleCode);
        Assert.Contains("FormatWindowSurfaceDiagnostics", sampleCode);
        Assert.Contains("GalleryWindowSurfaceEnvironment.Create", sampleCode);
        Assert.Contains("FluentThemeVariant.HighContrast", sampleCode);
        Assert.Contains("ResolveWindowSurfaceActualBackdrop", sampleCode);
        Assert.Contains("High contrast fallback", sampleCode);
        Assert.Contains("Inactive window material", sampleCode);
        Assert.Contains("Unsupported host fallback", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeDerivedSurfaceFamilySample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "materialsand effects".Replace(" ", string.Empty));

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWLayerSurface", sampleCode);
        Assert.Contains("new FWMicaSurface", sampleCode);
        Assert.Contains("new FWMicaAltSurface", sampleCode);
        Assert.Contains("new FWAcrylicSurface", sampleCode);
        Assert.Contains("new FWFrostedGlassSurface", sampleCode);
        Assert.Contains("new FWCardSurface", sampleCode);
        Assert.Contains("new FWFlyoutSurface", sampleCode);
        Assert.Contains("new FWFocusGlassSurface", sampleCode);
        Assert.Contains("new FWFluentWindowSurface", sampleCode);
        Assert.Contains("FWFluentMaterialRecipe.Create", sampleCode);
        Assert.Contains("FWFluentMaterialKind.Acrylic", sampleCode);
        Assert.Contains("FWFluentMaterialKind.LiquidGlass", sampleCode);
        Assert.Contains("UseMaterialRecipe", sampleCode);
        Assert.Contains("Derived surface recipes", sampleCode);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeBackdropPrimitiveFallbackSample()
    {
        var page = Assert.Single(
            GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()),
            page => page.UniqueId == "materialprimitives");

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("new FWBackdrop", sampleCode);
        Assert.Contains("FWBackdropType.Mica", sampleCode);
        Assert.Contains("FWBackdropType.Acrylic", sampleCode);
        Assert.Contains("FWBackdropType.None", sampleCode);
        Assert.Contains("FallbackColor", sampleCode);
        Assert.Contains("AlwaysUseFallback = true", sampleCode);
        Assert.Contains("Backdrop primitive QA", sampleCode);
        Assert.Contains("Forced fallback QA", sampleCode);
        Assert.Contains("Solid fallback QA", sampleCode);
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
        Assert.Contains("pendingRefreshDeferral", sampleCode);
        Assert.Contains("FormatRefreshContainerDiagnostics", sampleCode);
        Assert.Contains("Snap requested", sampleCode);
        Assert.Contains("FormatScrollerDiagnostics", sampleCode);
        Assert.Contains("FormatAnnotatedScrollBarDetail", sampleCode);
        Assert.Contains("Error marker", sampleCode);
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
    public void GalleryVisualQaCoverageCatalog_ShouldMapFamiliesToCatalogPagesAndRegisteredSamples()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var pageIds = pages.Select(page => page.UniqueId).ToHashSet(StringComparer.Ordinal);
        var families = GalleryVisualQaCoverageCatalog.CreateFamilies();

        Assert.True(families.Count >= 10);

        foreach (var family in families)
        {
            Assert.True(pageIds.Contains(family.PageId), $"{family.FamilyId} references missing page '{family.PageId}'.");
            Assert.True(
                GallerySampleCodeRegistry.ContainsRegisteredSampleCodeKey(family.SampleCodeKey),
                $"{family.FamilyId} references unregistered sample key '{family.SampleCodeKey}'.");
            Assert.All(family.Controls, control => Assert.StartsWith("FW", control, StringComparison.Ordinal));
            Assert.True(family.CoveredStates.Length >= 6, $"{family.FamilyId} has too few covered states.");
            Assert.NotEmpty(family.Evidence);
            Assert.True(family.HasReadinessEvidence, $"{family.FamilyId} must declare readiness evidence.");
            Assert.False(string.IsNullOrWhiteSpace(family.Readiness));
            Assert.False(string.IsNullOrWhiteSpace(family.EvidenceLevel));
            Assert.False(string.IsNullOrWhiteSpace(family.NextAction));
            Assert.Contains(family.Title, family.Summary);
        }
    }

    [Fact]
    public void GalleryVisualQaCoverageCatalog_ShouldCoverCoreFluentStateTokens()
    {
        var families = GalleryVisualQaCoverageCatalog.CreateFamilies();
        var snapshot = GalleryVisualQaCoverageCatalog.CreateSnapshot();
        var summary = GalleryVisualQaCoverageCatalog.FormatSnapshot(snapshot);

        Assert.Equal(families.Count, snapshot.FamilyCount);
        Assert.True(snapshot.ControlCount >= 50);
        Assert.True(snapshot.StateCount >= 15);
        Assert.True(snapshot.DiagnosticFamilyCount >= 8);
        Assert.Equal(families.Count, snapshot.ReadinessFamilyCount);
        Assert.True(snapshot.NeedsRenderedQaCount >= 10);
        Assert.Equal(0, snapshot.RenderedQaFamilyCount);
        Assert.True(snapshot.CoversPage("windowbackdrops"));
        Assert.True(snapshot.CoversPage("advancedcollections"));
        Assert.True(snapshot.HasSample("materials.windowbackdrop"));
        Assert.True(snapshot.HasSample("advancedinteraction.scroller"));

        Assert.Contains(families, family => family.Covers("normal"));
        Assert.Contains(families, family => family.Covers("hover"));
        Assert.Contains(families, family => family.Covers("pressed"));
        Assert.Contains(families, family => family.Covers("selected"));
        Assert.Contains(families, family => family.Covers("disabled"));
        Assert.Contains(families, family => family.Covers("focus"));
        Assert.Contains(families, family => family.Covers("density"));
        Assert.Contains(families, family => family.Covers("light"));
        Assert.Contains(families, family => family.Covers("dark"));
        Assert.Contains(families, family => family.Covers("high contrast"));
        Assert.Contains(families, family => family.Covers("diagnostics"));
        Assert.Contains(families, family => family is { FamilyId: "charts", Readiness: "Rendered QA needed", EvidenceLevel: "SampleCode", RequiresRenderedQa: true });
        Assert.Contains(families, family => family is { FamilyId: "visuals", Readiness: "Rendered QA needed", EvidenceLevel: "SampleCode", RequiresRenderedQa: true });
        Assert.Contains(families, family => family is { FamilyId: "disclosure", EvidenceLevel: "RealWindowSmoke", RequiresRenderedQa: true });
        Assert.Contains(families, family => family is { FamilyId: "materials", EvidenceLevel: "RuntimeDiagnostics", RequiresRenderedQa: true });
        Assert.Contains("Visual QA coverage", summary);
        Assert.Contains("readiness-scored", summary);
        Assert.Contains("need rendered QA", summary);
    }

    [Fact]
    public void GalleryVisualQaCoveragePage_ShouldExposeDiagnosticFooterMetadataAndSampleCode()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var page = Assert.Single(pages, page => page.UniqueId == "visualqacoverage");

        Assert.Equal("Visual QA Coverage", page.Title);
        Assert.Equal(GalleryNavigationGroup.Diagnostics, page.Group);
        Assert.True(page.IsFooter);
        Assert.True(page.IsUpdated);
        Assert.Equal(GalleryPageStatus.Diagnostic, page.Status);
        Assert.Equal("/GalleryDiagnostics/VisualQaCoverage", page.SourcePath);
        Assert.Equal("FluentJalium.Gallery.Models", page.ApiNamespace);
        Assert.Equal("diagnostics.visualqa.coverage", page.SampleCodeKey);
        Assert.Contains("GalleryVisualQaCoverageCatalog", page.RelatedControls);
        Assert.Contains("GalleryVisualQaCoverageSnapshot", page.BaseClasses!);
        Assert.True(page.MatchesSearch("visual qa evidence"));

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("GalleryVisualQaCoverageCatalog.CreateFamilies", sampleCode);
        Assert.Contains("GalleryVisualQaCoveragePage.CreateSnapshot", sampleCode);
        Assert.Contains("FormatFamilyCoverage", sampleCode);
        Assert.Contains("HasReadinessEvidence", sampleCode);
        Assert.Contains("RequiresRenderedQa", sampleCode);
        Assert.Contains("ContainsRegisteredSampleCodeKey", sampleCode);
        Assert.DoesNotContain("Generated from", sampleCode);
    }

    [Fact]
    public void GalleryControlGapCatalog_ShouldTrackPublicRecipeAndEvaluateCandidates()
    {
        var entries = GalleryControlGapCatalog.CreateEntries();
        var snapshot = GalleryControlGapCatalog.CreateSnapshot();
        var summary = GalleryControlGapCatalog.FormatSnapshot(snapshot);

        Assert.True(entries.Count >= 18);
        Assert.True(snapshot.PublicControlCount >= 10);
        Assert.True(snapshot.RecipeOnlyCount >= 3);
        Assert.True(snapshot.EvaluateCount >= 3);
        Assert.True(snapshot.RenderedQaRequiredCount >= 4);
        Assert.True(snapshot.P0Count >= 7);
        Assert.True(snapshot.P1Count >= 5);
        Assert.True(snapshot.P2Count >= 1);
        Assert.True(snapshot.CoversArea("advancedcollections"));
        Assert.True(snapshot.CoversArea("inputandmedia"));
        Assert.True(snapshot.CoversReference("WinUI / WinUI Gallery"));
        Assert.True(snapshot.CoversReference("WPF UI"));
        Assert.True(snapshot.CoversReference("FluentAvalonia / Community Toolkit"));
        Assert.True(snapshot.HasSample("advancedcollections.itemsrepeater"));
        Assert.True(snapshot.HasSample("disclosure.taskdialog"));

        Assert.Contains(entries, entry => entry is
        {
            CandidateControl: "FWFlyout / FWFlyoutPresenter",
            Stage: GalleryControlGapStage.PublicFwControl,
            Priority: "P0"
        });
        Assert.Contains(entries, entry => entry is
        {
            CandidateControl: "FWAutoSuggestBox",
            Stage: GalleryControlGapStage.PublicFwControl,
            SampleCodeKey: "textinput.autosuggestbox"
        });
        Assert.Contains(entries, entry => entry is
        {
            CandidateControl: "FWSettingsCard / FWSettingsExpander",
            Stage: GalleryControlGapStage.RenderedQaRequired
        });
        Assert.Contains(entries, entry => entry.CandidateControl == "FWItemsView" && entry.IsRecipeOnly);
        Assert.Contains(entries, entry => entry.CandidateControl == "FWFlipView" && entry.IsRecipeOnly);
        Assert.Contains(entries, entry => entry.CandidateControl == "FWSemanticZoom" && entry.IsRecipeOnly);
        Assert.Contains(entries, entry => entry.CandidateControl == "FWMaskedTextBox / FWForm" && entry.IsEvaluateOnly);
        Assert.Contains(entries, entry => entry.CandidateControl == "FWContactCard" && entry.IsEvaluateOnly);
        Assert.Contains(entries, entry => entry.CandidateControl == "FWInkToolbar" && entry.IsEvaluateOnly);
        Assert.All(entries, entry => Assert.True(entry.HasDecisionEvidence));
        Assert.All(entries.Where(entry => entry.RequiresPublicApi), entry => Assert.NotEmpty(entry.RequiredBeforePublicApi));
        Assert.Contains("Control gap matrix", summary);
        Assert.Contains("public FW controls", summary);
        Assert.Contains("recipe-only candidates", summary);
        Assert.Contains("rendered-QA gates", summary);
    }

    [Fact]
    public void GalleryControlGapPage_ShouldExposeDiagnosticFooterMetadataAndSampleCode()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var page = Assert.Single(pages, page => page.UniqueId == "controlgapmatrix");

        Assert.Equal("Control Gap Matrix", page.Title);
        Assert.Equal(GalleryNavigationGroup.Diagnostics, page.Group);
        Assert.True(page.IsFooter);
        Assert.True(page.IsUpdated);
        Assert.Equal(GalleryPageStatus.Diagnostic, page.Status);
        Assert.Equal("/GalleryDiagnostics/ControlGapMatrix", page.SourcePath);
        Assert.Equal("FluentJalium.Gallery.Models", page.ApiNamespace);
        Assert.Equal("diagnostics.controlgap.matrix", page.SampleCodeKey);
        Assert.Contains("GalleryControlGapCatalog", page.RelatedControls);
        Assert.Contains("GalleryControlGapSnapshot", page.BaseClasses!);
        Assert.True(page.MatchesSearch("control gap evaluate"));
        Assert.True(page.MatchesSearch("FluentAvalonia Community Toolkit"));

        Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
        Assert.Contains("GalleryControlGapCatalog.CreateEntries", sampleCode);
        Assert.Contains("GalleryControlGapPage.CreateSnapshot", sampleCode);
        Assert.Contains("FormatGapEntry", sampleCode);
        Assert.Contains("HasDecisionEvidence", sampleCode);
        Assert.Contains("RequiredBeforePublicApi", sampleCode);
        Assert.Contains("WinUI / WinUI Gallery", sampleCode);
        Assert.DoesNotContain("Generated from", sampleCode);
    }

    [Fact]
    public void GalleryControlGapPage_ShouldCreateSnapshotContentAndFormatEntries()
    {
        var page = new GalleryControlGapPage();
        var snapshot = GalleryControlGapPage.CreateSnapshot();
        var entry = Assert.Single(
            GalleryControlGapCatalog.CreateEntries(),
            entry => entry.CandidateControl == "FWInkToolbar");
        var text = GalleryControlGapPage.FormatGapEntry(entry);
        var content = page.CreateContent();

        Assert.True(snapshot.CoversArea("inputandmedia"));
        Assert.True(snapshot.HasSample("inputmedia.color.ink.media"));
        Assert.Contains("FWInkToolbar", text);
        Assert.Contains("Evaluate", text);
        Assert.Contains("FWToolBar plus FWInkCanvas recipe", text);
        Assert.Contains("Dedicated toolbar command model", text);
        Assert.IsAssignableFrom<UIElement>(content);
    }

    [Fact]
    public void GalleryVisualQaCoveragePage_ShouldCreateSnapshotContentAndFormatFamilies()
    {
        var page = new GalleryVisualQaCoveragePage();
        var snapshot = GalleryVisualQaCoveragePage.CreateSnapshot();
        var family = Assert.Single(
            GalleryVisualQaCoverageCatalog.CreateFamilies(),
            family => family.FamilyId == "materials");
        var text = GalleryVisualQaCoveragePage.FormatFamilyCoverage(family);
        var content = page.CreateContent();

        Assert.True(snapshot.CoversPage("windowbackdrops"));
        Assert.True(snapshot.HasSample("materials.windowbackdrop"));
        Assert.Contains("Materials and window backdrops", text);
        Assert.Contains("Runtime diagnostic-ready", text);
        Assert.Contains("RuntimeDiagnostics", text);
        Assert.Contains("next action", text);
        Assert.Contains("high contrast", text);
        Assert.Contains("fallback brushes", text);
        Assert.Contains("high contrast", text);
        Assert.Contains("materials.windowbackdrop", text);
        Assert.IsAssignableFrom<UIElement>(content);
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
    public void GallerySampleCodeRegistry_ShouldExplicitlyRegisterEveryCatalogSampleKey()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService())
            .Where(page => !page.IsFooter && !string.IsNullOrWhiteSpace(page.SampleCodeKey))
            .ToArray();

        Assert.NotEmpty(pages);

        foreach (var page in pages)
        {
            Assert.True(
                GallerySampleCodeRegistry.ContainsRegisteredSampleCodeKey(page.SampleCodeKey),
                $"{page.UniqueId} references unregistered sample key '{page.SampleCodeKey}'.");

            Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
            Assert.False(string.IsNullOrWhiteSpace(sampleCode));
            Assert.DoesNotContain("Generated from", sampleCode);
        }
    }

    [Fact]
    public void GalleryCatalogFilterSnapshots_ShouldMatchCatalogMetadata()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var controls = GalleryControlInfo.CreateFromPages(pages);
        var all = GalleryCatalogFilterSnapshot.Create(GalleryCatalogFilter.AllControls, controls);
        var newest = GalleryCatalogFilterSnapshot.Create(GalleryCatalogFilter.New, controls);
        var updated = GalleryCatalogFilterSnapshot.Create(GalleryCatalogFilter.Updated, controls);
        var preview = GalleryCatalogFilterSnapshot.Create(GalleryCatalogFilter.Preview, controls);
        var diagnostic = GalleryCatalogFilterSnapshot.Create(GalleryCatalogFilter.Diagnostic, controls);

        Assert.Equal(controls.Length, all.ControlCount);
        Assert.Equal(controls.Count(control => control.IsNew), all.NewCount);
        Assert.Equal(controls.Count(control => control.IsUpdated), all.UpdatedCount);
        Assert.Equal(controls.Count(control => control.Status == GalleryPageStatus.Preview), all.PreviewCount);
        Assert.Equal(controls.Count(IsDiagnosticCatalogControl), all.DiagnosticCount);
        Assert.Equal(controls.Select(control => control.Page.UniqueId).Distinct(StringComparer.Ordinal).Count(), all.PageCount);
        Assert.Equal(controls.Count(control => !string.IsNullOrWhiteSpace(control.SourcePath)), all.WithSourcePathCount);
        Assert.Equal(controls.Count(control => !string.IsNullOrWhiteSpace(control.SampleCodeKey)), all.WithSampleCodeKeyCount);
        Assert.Equal(controls.Count(control => !string.IsNullOrWhiteSpace(control.ApiNamespace)), all.WithApiNamespaceCount);
        Assert.Equal(
            controls
                .GroupBy(control => control.Group, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new GalleryCatalogFilterGroupSnapshot(
                    group.Key,
                    group.Count(),
                    group.Select(control => control.Page.UniqueId).Distinct(StringComparer.Ordinal).Count()))
                .ToArray(),
            all.GroupCounts);
        Assert.True(all.HasCompleteNavigationMetadata);
        Assert.True(all.ContainsPage("navigation"));
        Assert.True(all.ContainsSampleCodeKey("navigation.breadcrumb.pips.selector.tabview.titlebar"));

        Assert.NotEmpty(newest.Matches);
        Assert.All(newest.Matches, control => Assert.True(control.IsNew));
        Assert.True(newest.ContainsControl("FWItemsRepeater"));

        Assert.NotEmpty(updated.Matches);
        Assert.All(updated.Matches, control => Assert.True(control.IsUpdated));
        Assert.True(updated.ContainsControl("FWTaskDialog"));
        Assert.True(updated.ContainsControl("FWSnackbar"));

        Assert.NotEmpty(preview.Matches);
        Assert.All(preview.Matches, control => Assert.Equal(GalleryPageStatus.Preview, control.Status));
        Assert.True(preview.ContainsControl("FWItemsRepeater"));
        Assert.True(preview.ContainsControl("FWAnimatedIcon"));
        Assert.True(preview.ContainsPage("advancedcollections"));
        Assert.True(preview.ContainsSampleCodeKey("advancedcollections.itemsrepeater"));
        Assert.True(preview.HasCompleteNavigationMetadata);

        Assert.NotEmpty(diagnostic.Matches);
        Assert.All(diagnostic.Matches, control => Assert.True(IsDiagnosticCatalogControl(control)));
        Assert.True(diagnostic.ContainsControl("FWRefreshContainerDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWAnnotatedScrollBarDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWItemsRepeaterDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWTwoPaneViewDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWParallaxViewDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWFluentWindowSurfaceDiagnostics"));
        Assert.True(diagnostic.ContainsControl("FWSnackbarPresenterDiagnostics"));
    }

    [Fact]
    public void GalleryCatalogFilterPage_ShouldExposeSnapshotUsedByFilterEntryPages()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var filterPage = new GalleryCatalogFilterPage(GalleryCatalogFilter.Preview, pages);

        var snapshot = filterPage.CreateSnapshot();
        var content = filterPage.CreateContent();

        Assert.True(snapshot.ControlCount > 0);
        Assert.Equal(snapshot.ControlCount, snapshot.PreviewCount);
        Assert.True(snapshot.PageCount > 0);
        Assert.True(snapshot.ContainsControl("FWItemsRepeater"));
        Assert.True(snapshot.ContainsPage("advancedcollections"));
        Assert.True(snapshot.ContainsSampleCodeKey("advancedcollections.itemsrepeater"));
        Assert.NotEmpty(snapshot.GroupCounts);
        Assert.True(snapshot.HasCompleteNavigationMetadata);
        Assert.IsAssignableFrom<UIElement>(content);
    }

    [Fact]
    public void GallerySampleCodeRegistry_ShouldExposeCatalogFilterSnapshotSamples()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService())
            .Where(page => page.Group == GalleryNavigationGroup.Catalog)
            .ToArray();

        Assert.Equal(5, pages.Length);

        foreach (var page in pages)
        {
            Assert.True(GallerySampleCodeRegistry.TryGetSampleCode(page, out var sampleCode));
            Assert.Contains("new GalleryCatalogFilterPage", sampleCode);
            Assert.Contains("CreateSnapshot()", sampleCode);
            Assert.Contains("snapshot.ControlCount", sampleCode);
            Assert.Contains("snapshot.PageCount", sampleCode);
            Assert.Contains("snapshot.GroupCounts", sampleCode);
            Assert.Contains("snapshot.WithSourcePathCount", sampleCode);
            Assert.Contains("snapshot.WithSampleCodeKeyCount", sampleCode);
            Assert.Contains("snapshot.WithApiNamespaceCount", sampleCode);
            Assert.DoesNotContain("Generated from", sampleCode);
        }
    }

    [Fact]
    public void GalleryCatalogService_ShouldRegisterFactoryForEveryCatalogPage()
    {
        var expectedPageIds = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService())
            .Select(page => page.UniqueId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var service = new GalleryCatalogService();
        var owner = new Window();

        var registeredPageIds = service.CreateRegisteredPageIds(owner, _ => { }, _ => { });

        Assert.Equal(expectedPageIds, registeredPageIds);
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
        Assert.Contains("TaskDialog root-window smoke", document);
        Assert.Contains("real-window QA snapshot", document);
        Assert.Contains("ItemsRepeater visual QA", document);
        Assert.Contains("Settings visual QA", document);
        Assert.Contains("Navigation app-shell recipes", document);
        Assert.Contains("Forms pattern Gallery page", document);
        Assert.Contains("Visuals and chart sample depth", document);
        Assert.Contains("derived surface recipes", document);
        Assert.Contains("FWBackdrop", document);
        Assert.Contains("CreateLegendTooltipQaSnapshot", document);
        Assert.Contains("FormatShapeControlsVisualQa", document);
        Assert.Contains("GalleryCatalogFilterSnapshot", document);
        Assert.Contains("validation summaries", document);
        Assert.Contains("Rendered Gallery QA pass", document);
        Assert.Contains("phone/license-key formatting", document);
        Assert.Contains("FormatPhoneRecipe", document);
        Assert.Contains("FWMaskedTextBox", document);
        Assert.Contains("missing automation, gesture/animation, or two-view contract evidence", document);
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

    private static void AssertCatalogControl(GalleryControlInfo[] controls, string name, string pageId)
    {
        var control = Assert.Single(controls, control => control.Name == name);

        Assert.Equal(pageId, control.Page.UniqueId);
        Assert.True(control.IsUpdated);
        Assert.Equal("FluentJalium.Controls", control.ApiNamespace);
        Assert.False(string.IsNullOrWhiteSpace(control.SourcePath));
        Assert.False(string.IsNullOrWhiteSpace(control.SampleCodeKey));
    }

    private static bool IsDiagnosticCatalogControl(GalleryControlInfo control)
    {
        return control.Status == GalleryPageStatus.Diagnostic
            || control.Name.Contains("Diagnostics", StringComparison.Ordinal);
    }
}
