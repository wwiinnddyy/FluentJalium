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
        Assert.Contains(controls, control => control.Name == "FWRadioButtons" && control.Page.UniqueId == "selection");
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
}
