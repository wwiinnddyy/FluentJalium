using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Pages;
using FluentJalium.Gallery.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Themes;

namespace FluentJalium.Tests;

/// <summary>
/// Contract tests for the replacement Gallery. These intentionally validate the public surface
/// rather than the deleted Gallery's implementation details: pages must be discoverable, each
/// page must materialize, and the shared Jalxaml components must construct successfully.
/// </summary>
[Collection("Application")]
public sealed class GalleryJalxamlSmokeTests
{
    [Fact]
    public void GalleryCatalog_ShouldExposeEveryPrimaryFamily()
    {
        Assert.True(GalleryPages.All.Count >= 19);
        Assert.Contains(GalleryPages.All, page => page.Key == "controls");
        Assert.Contains(GalleryPages.All, page => page.Key == "charts");
        Assert.Contains(GalleryPages.All, page => page.Key == "menus");
        Assert.Contains(GalleryPages.All, page => page.Key == "visuals");
        Assert.Contains(GalleryPages.All, page => page.Key == "motion");
    }

    [Fact]
    public void JalxamlGalleryComponents_ShouldConstructWithFluentThemeResources()
    {
        ResetApplicationState();
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);
        app.Resources.MergedDictionaries.Add(new GalleryStyles());

        Assert.NotNull(new GalleryHero());
        Assert.NotNull(new GalleryPageFrame("Smoke", "Description"));
        Assert.NotNull(new GalleryExampleCard("Example", "Description", new FWButton { Content = "Ready" }));
        Assert.NotNull(new GalleryLinkCard(GalleryPages.All[0], _ => { }));
        Assert.NotNull(new GalleryControlCatalogCard(typeof(FWButton), "Buttons"));
    }

    [Fact]
    public void EveryGalleryPage_ShouldMaterializeAContentTree()
    {
        ResetApplicationState();
        var app = new Application();
        ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);
        app.Resources.MergedDictionaries.Add(new GalleryStyles());

        foreach (var page in GalleryPages.All.Append(GalleryPages.About))
        {
            var content = page.CreateContent(_ => { });
            Assert.NotNull(content);
        }
    }

    private static void ResetApplicationState()
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        typeof(Application).GetField("_current", flags)?.SetValue(null, null);
        typeof(ThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
        typeof(FluentThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
    }
}
