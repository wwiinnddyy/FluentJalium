using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Controls;
using FluentJalium.Gallery.Pages;
using Jalium.UI;
using Jalium.UI.Controls;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// WinUI Gallery's home page opens with a strip of quick-link tiles under the hero.
/// </summary>
[Collection("Application")]
public sealed class GalleryHomeTilesTests
{
    [Fact]
    public void OverviewPage_ShouldOfferQuickLinkTilesUnderTheHero()
    {
        var app = new Application();
        FluentThemeManager.Apply(app);

        var navigated = new List<string>();
        var content = new GalleryOverviewPage(
            _ => { },
            _ => { },
            navigated.Add).CreateContent();

        var tiles = Walk(content).OfType<GalleryTile>().ToArray();

        Assert.Equal(6, tiles.Length);
        Assert.All(tiles, tile => Assert.False(string.IsNullOrWhiteSpace(tile.Title)));
        Assert.All(tiles, tile => Assert.Equal(232d, tile.Width));

        tiles[0].Invoke();
        Assert.Single(navigated);
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
