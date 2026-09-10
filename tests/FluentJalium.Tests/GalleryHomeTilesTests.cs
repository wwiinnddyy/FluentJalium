using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Pages;
using FluentJalium.Gallery.Services;
using Jalium.UI;
using Jalium.UI.Controls;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// WinUI Gallery's home page opens with a hero, a Recent/Favorites switcher, visited and
/// added-or-updated rails, and a favorites grid with its empty state.
/// </summary>
[Collection("Application")]
public sealed class GalleryHomeTilesTests
{
    [Fact]
    public void OverviewPage_ShouldOfferQuickLinkTilesUnderTheHero()
    {
        ResetApplicationState();
        var app = new Application();
        Jalium.UI.Controls.Themes.ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
        var navigated = new List<string>();
        var content = new GalleryOverviewPage(pages, navigated.Add).CreateContent();

        var tiles = Walk(content).OfType<GalleryTile>().ToArray();

        Assert.Equal(6, tiles.Length);
        Assert.All(tiles, tile => Assert.False(string.IsNullOrWhiteSpace(tile.Title)));
        Assert.All(tiles, tile => Assert.False(string.IsNullOrWhiteSpace(tile.Caption)));
        Assert.All(tiles, tile => Assert.Equal(232d, tile.Width));

        tiles[0].Invoke();
        Assert.Single(navigated);
    }

    [Fact]
    public void OverviewPage_ShouldSwitchBetweenRecentAndFavorites()
    {
        ResetApplicationState();
        var app = new Application();
        Jalium.UI.Controls.Themes.ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        GalleryFavoritesService.Instance.SetFavorite("overview", false);
        try
        {
            var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());
            var content = new GalleryOverviewPage(pages, _ => { }).CreateContent();

            var selector = Walk(content).OfType<FWSelectorBar>().Single();
            Assert.Equal(2, selector.Items.Count);
            Assert.Equal(0, selector.SelectedIndex);

            var localization = new GalleryLocalizationService();
            var texts = Walk(content).OfType<TextBlock>().Select(text => text.Text).ToArray();
            Assert.Contains(localization.Text("home.recentlyAddedOrUpdated"), texts);
            Assert.Contains(localization.Text("home.noFavoritesTitle"), texts);
        }
        finally
        {
            GalleryFavoritesService.Instance.SetFavorite("overview", false);
        }
    }

    [Fact]
    public void OverviewPage_ShouldRenderFavoritedSamplesAsCards()
    {
        ResetApplicationState();
        var app = new Application();
        Jalium.UI.Controls.Themes.ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        var localization = new GalleryLocalizationService();
        var pages = GalleryCatalog.CreatePageInfos(localization);
        GalleryFavoritesService.Instance.SetFavorite("overview", true);
        try
        {
            var content = new GalleryOverviewPage(pages, _ => { }).CreateContent();

            var titles = Walk(content).OfType<GalleryControlCard>().Select(card => card.Title).ToArray();
            Assert.Contains(localization.PageTitle("overview"), titles);
        }
        finally
        {
            GalleryFavoritesService.Instance.SetFavorite("overview", false);
        }
    }

    [Fact]
    public void HomeSnapshot_ShouldJoinVisitsAndFavoritesAgainstCatalog()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        var snapshot = GalleryOverviewPage.CreateSnapshot(
            pages,
            ["buttons", "unknown-page", "overview"],
            ["switches", "missing-page"]);

        Assert.Equal(["buttons", "overview"], snapshot.Visited.Select(page => page.UniqueId));
        Assert.Equal(["switches"], snapshot.Favorites.Select(page => page.UniqueId));
        Assert.NotEmpty(snapshot.AddedOrUpdated);
        Assert.All(snapshot.AddedOrUpdated, page => Assert.False(page.IsFooter));
        Assert.All(snapshot.AddedOrUpdated, page => Assert.True(page.IsNew || page.IsUpdated));

        var empty = GalleryOverviewPage.CreateSnapshot(pages, [], []);
        Assert.Empty(empty.Visited);
        Assert.Empty(empty.Favorites);
    }

    [Fact]
    public void FavoritesService_ShouldPersistToggledFavorites()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fj-favorites-{Guid.NewGuid():N}");
        try
        {
            var service = new GalleryFavoritesService(path);

            Assert.False(service.IsFavorite("buttons"));
            Assert.True(service.ToggleFavorite("buttons"));
            Assert.True(service.IsFavorite("buttons"));
            Assert.Equal(["buttons"], service.GetFavorites());

            var reloaded = new GalleryFavoritesService(path);
            Assert.True(reloaded.IsFavorite("buttons"));

            Assert.False(service.ToggleFavorite("buttons"));
            Assert.False(service.IsFavorite("buttons"));
            Assert.Empty(new GalleryFavoritesService(path).GetFavorites());
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void ResetApplicationState()
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
        typeof(Application).GetField("_current", flags)?.SetValue(null, null);
        typeof(Jalium.UI.Controls.Themes.ThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
        typeof(FluentThemeManager).GetMethod("Reset", flags)?.Invoke(null, null);
    }

    private static IEnumerable<DependencyObject> Walk(DependencyObject node)
    {
        yield return node;

        switch (node)
        {
            case Panel panel:
                foreach (UIElement child in panel.Children)
                {
                    foreach (var descendant in Walk(child))
                    {
                        yield return descendant;
                    }
                }
                break;

            case ContentControl contentControl when contentControl.Content is DependencyObject content:
                foreach (var descendant in Walk(content))
                {
                    yield return descendant;
                }
                break;

            case Decorator decorator when decorator.Child is DependencyObject child:
                foreach (var descendant in Walk(child))
                {
                    yield return descendant;
                }
                break;
        }
    }
}
