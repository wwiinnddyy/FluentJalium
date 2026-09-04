using FluentJalium.Controls;
using FluentJalium.Controls.Themes;
using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using FluentJalium.Gallery.Shell;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// Phase 3: item PageHeader parity (docs/source menus, copy-link, favorite),
/// search results, section grids, and sample deep links.
/// </summary>
[Collection("Application")]
public sealed class GalleryItemPageTests
{
    [Fact]
    public void DeepLink_ShouldRoundTripSampleUniqueId()
    {
        const string uniqueId = "dateandtime";

        Assert.Equal("fluentjalium://sample?uniqueId=dateandtime", GalleryDeepLink.FormatSampleLink(uniqueId));
        Assert.True(GalleryDeepLink.TryParseSampleLink("fluentjalium://sample?uniqueId=dateandtime", out var parsed));
        Assert.Equal(uniqueId, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://example.com/sample?uniqueId=buttons")]
    [InlineData("fluentjalium://other?uniqueId=buttons")]
    [InlineData("fluentjalium://sample")]
    [InlineData("fluentjalium://sample?name=buttons")]
    public void DeepLink_ShouldRejectNonSampleLinks(string? link)
    {
        Assert.False(GalleryDeepLink.TryParseSampleLink(link, out _));
    }

    [Fact]
    public void NavigationBroker_ShouldDeliverRequestedUniqueId()
    {
        var received = new List<string>();
        void Handler(string uniqueId) => received.Add(uniqueId);

        GalleryNavigationBroker.NavigateRequested += Handler;
        try
        {
            GalleryNavigationBroker.RequestNavigate("buttons");
        }
        finally
        {
            GalleryNavigationBroker.NavigateRequested -= Handler;
        }

        Assert.Equal(["buttons"], received);
    }

    [Fact]
    public void SearchResultsSnapshot_ShouldMatchCatalogSearch()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        var matches = GallerySearchResultsPage.CreateSnapshot(pages, "button");
        Assert.Contains(matches, page => page.UniqueId == "buttons");
        Assert.DoesNotContain(matches, page => page.IsFooter);

        Assert.Empty(GallerySearchResultsPage.CreateSnapshot(pages, "   "));
        Assert.Empty(GallerySearchResultsPage.CreateSnapshot(pages, "zzz-no-such-sample"));
    }

    [Fact]
    public void SectionSnapshot_ShouldListGroupPages()
    {
        var pages = GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

        var fundamentals = GallerySectionPage.CreateSnapshot(pages, GalleryNavigationGroup.Fundamentals);
        Assert.NotEmpty(fundamentals);
        Assert.All(fundamentals, page => Assert.Equal(GalleryNavigationGroup.Fundamentals, page.GroupId));
        Assert.Contains(fundamentals, page => page.UniqueId == "resources");

        Assert.Empty(GallerySectionPage.CreateSnapshot(pages, ""));
        Assert.Empty(GallerySectionPage.CreateSnapshot(pages, "no-such-group"));
    }

    [Fact]
    public void ItemView_ShouldExposeHeaderActionsAndFavoriteToggle()
    {
        ResetApplicationState();
        var app = new Application();
        Jalium.UI.Controls.Themes.ThemeManager.Initialize(app);
        FluentThemeManager.Apply(app);

        var localization = new GalleryLocalizationService();
        var info = GalleryCatalog.CreatePageInfos(localization).First(page => page.UniqueId == "buttons");
        var page = new GalleryPage(info, () => new TextBlock());

        GalleryFavoritesService.Instance.SetFavorite("buttons", false);
        try
        {
            var view = new GalleryItemView();
            view.ApplyNavigationParameter(page);

            var texts = Walk(view).OfType<TextBlock>().Select(text => text.Text).ToArray();
            Assert.Contains(localization.PageTitle("buttons"), texts);
            Assert.Contains(localization.PageDescription("buttons"), texts);

            var toggle = Walk(view).OfType<FWToggleButton>().Single();
            Assert.False(toggle.IsChecked is true);

            toggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.True(GalleryFavoritesService.Instance.IsFavorite("buttons"));
            Assert.True(toggle.IsChecked is true);

            toggle.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.False(GalleryFavoritesService.Instance.IsFavorite("buttons"));
        }
        finally
        {
            GalleryFavoritesService.Instance.SetFavorite("buttons", false);
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
