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
        Assert.Contains(controls, control => control.Name == "FWSettingsCard" && control.Page.UniqueId == "contentandlayout");
        Assert.Contains(controls, control => control.Name == "FWSettingsExpander" && control.Page.UniqueId == "disclosure");
        Assert.Contains(controls, control => control.Name == "FWTaskDialog" && control.Page.UniqueId == "disclosure");
        Assert.Contains(controls, control => control.Name == "FWSnackbar" && control.Page.UniqueId == "status");
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
    }
}
