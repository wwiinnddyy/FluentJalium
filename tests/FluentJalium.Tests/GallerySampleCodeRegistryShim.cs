using FluentJalium.Gallery.Models;
using FluentJalium.Gallery.Services;

namespace FluentJalium.Tests;

/// <summary>
/// Test shim so the catalog tests keep compiling against the old sample-code API while the
/// hand-written registry is being replaced by real page sources.
/// </summary>
/// <remarks>
/// Every member resolves through <see cref="GallerySampleSourceRegistry"/>, so an assertion here
/// now proves the page's own file exists and is embedded, not that a snippet was authored.
/// </remarks>
internal static class GallerySampleCodeRegistry
{
    public static bool TryGetSampleCode(GalleryPageInfo page, out string sampleCode) =>
        GallerySampleSourceRegistry.TryGetSource(page, out sampleCode!);

    public static bool TryGetRegisteredSampleCode(string sampleCodeKey, out string sampleCode)
    {
        sampleCode = string.Empty;

        return ContainsRegisteredSampleCodeKey(sampleCodeKey)
            && TryGetSampleCode(PageForSampleCodeKey(sampleCodeKey), out sampleCode!);
    }

    private static GalleryPageInfo PageForSampleCodeKey(string sampleCodeKey) =>
        GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()).First(page =>
            string.Equals(page.SampleCodeKey?.Trim(), sampleCodeKey.Trim(), StringComparison.Ordinal));

    public static bool ContainsRegisteredSampleCodeKey(string? sampleCodeKey)
    {
        if (string.IsNullOrWhiteSpace(sampleCodeKey))
        {
            return false;
        }

        var normalized = sampleCodeKey.Trim();

        return GalleryCatalog.CreatePageInfos(new GalleryLocalizationService()).Any(page =>
            string.Equals(page.SampleCodeKey?.Trim(), normalized, StringComparison.Ordinal) &&
            GallerySampleSourceRegistry.TryGetSource(page, out _));
    }
}
