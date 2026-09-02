using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;
using Xunit;

namespace FluentJalium.Tests;

/// <summary>
/// The source panel has to show a page's real file, not hand-written text that has drifted away
/// from it.
/// </summary>
public sealed class GallerySampleSourceRegistryTests
{
    private static GalleryPageInfo[] Infos => GalleryCatalog.CreatePageInfos(new GalleryLocalizationService());

    [Theory]
    [InlineData("buttons", "GalleryButtonsPage")]
    [InlineData("textinput", "GalleryTextInputPage")]
    [InlineData("selection", "GallerySelectionPage")]
    [InlineData("switches", "GallerySwitchesPage")]
    public void TryGetSource_ShouldReturnThePagesOwnFile(string uniqueId, string className)
    {
        var info = Infos.Single(candidate => candidate.UniqueId == uniqueId);

        Assert.True(GallerySampleSourceRegistry.TryGetSource(info, out var source), $"no source for {uniqueId}");
        Assert.Contains(className, source, StringComparison.Ordinal);
        Assert.True(source.Length > 500, $"{uniqueId} source looks truncated at {source.Length} chars");
    }

    [Fact]
    public void TryGetSource_ShouldResolveMostCatalogPages()
    {
        var resolved = Infos
            .Where(info => !info.IsFooter)
            .Count(info => GallerySampleSourceRegistry.TryGetSource(info, out _));

        Assert.True(resolved >= 15, $"only {resolved} of {Infos.Length} pages resolve to their own source file");
    }

    [Fact]
    public void TryGetSource_ShouldNotInventCodeForAnUnknownPage()
    {
        var unknown = Infos.First() with
        {
            UniqueId = "no-such-page",
            SourcePath = "/NoSuchFolder/X"
        };

        Assert.False(GallerySampleSourceRegistry.TryGetSource(unknown, out _));
    }
}
