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
        Assert.True(GallerySampleSourceRegistry.TryGetSource(page, out var sampleCode));
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

        Assert.True(GallerySampleSourceRegistry.TryGetSource(page, out var sampleCode));
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

        Assert.True(GallerySampleSourceRegistry.TryGetSource(page, out var sampleCode));
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

        Assert.True(GallerySampleSourceRegistry.TryGetSource(page, out var sampleCode));
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
    public void GalleryCatalogService_ShouldRegisterFactoryForEveryCatalogPage()
    {
        var expectedPageIds = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService())
            .Select(page => page.UniqueId)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var service = new GalleryCatalogService();
        var owner = new Window();

        var registeredPageIds = service.CreateRegisteredPageIds(owner, _ => { }, _ => { }, _ => { });

        Assert.Equal(expectedPageIds, registeredPageIds);
    }

    [Fact]
    public void GalleryCatalog_ShouldExposeWinUiFundamentalsAndAccessibilityShells()
    {
        var localization = new GalleryLocalizationService();
        var pages = GalleryCatalog.CreatePageInfos(localization);

        Assert.Equal(GalleryNavigationGroup.Catalog, GalleryNavigationGroup.FirstControlsGroup);
        var order = GalleryNavigationGroup.Order;
        Assert.True(Array.IndexOf(order, GalleryNavigationGroup.Fundamentals) >= 0);
        Assert.True(Array.IndexOf(order, GalleryNavigationGroup.Accessibility) >= 0);
        Assert.True(Array.IndexOf(order, GalleryNavigationGroup.Fundamentals) < Array.IndexOf(order, GalleryNavigationGroup.Catalog));
        Assert.True(Array.IndexOf(order, GalleryNavigationGroup.Accessibility) < Array.IndexOf(order, GalleryNavigationGroup.Catalog));

        Assert.Equal(
            localization.IsChinese ? "控件" : "Controls",
            localization.Text("shell.controlsHeader"));

        var shells = new (string UniqueId, string GroupId)[]
        {
            ("resources", GalleryNavigationGroup.Fundamentals),
            ("styles", GalleryNavigationGroup.Fundamentals),
            ("binding", GalleryNavigationGroup.Fundamentals),
            ("templates", GalleryNavigationGroup.Fundamentals),
            ("customusercontrols", GalleryNavigationGroup.Fundamentals),
            ("xamlconditions", GalleryNavigationGroup.Fundamentals),
            ("scratchpad", GalleryNavigationGroup.Fundamentals),
            ("screenreadersupport", GalleryNavigationGroup.Accessibility),
            ("keyboardsupport", GalleryNavigationGroup.Accessibility),
            ("colorcontrast", GalleryNavigationGroup.Accessibility)
        };

        foreach (var shell in shells)
        {
            var page = Assert.Single(pages, page => page.UniqueId == shell.UniqueId);
            Assert.Equal(localization.PageTitle(shell.UniqueId), page.Title);
            Assert.Equal(localization.GroupName(shell.GroupId), page.Group);
            Assert.Equal(GalleryPageStatus.Preview, page.Status);
            Assert.False(page.IsFooter);
            Assert.False(string.IsNullOrWhiteSpace(page.Description));
            Assert.True(page.MatchesSearch(page.Title));
        }

        var navigation = Assert.Single(pages, page => page.UniqueId == "navigation");
        Assert.Equal(localization.PageTitle("navigation"), navigation.Title);
        Assert.Equal(localization.GroupName(GalleryNavigationGroup.AppStructure), navigation.Group);
        Assert.True(navigation.IsUpdated);
        Assert.Equal("/Navigation/FWNavigationService", navigation.SourcePath);
        Assert.Equal("navigation.breadcrumb.pips.selector.tabview.titlebar", navigation.SampleCodeKey);
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

        Assert.True(GallerySampleSourceRegistry.TryGetSource(page, out var sampleCode));
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
