using FluentJalium.Gallery.Models;

namespace FluentJalium.Gallery.Services;

internal sealed class GalleryRecentSamplesService
{
    private static readonly GalleryRecentSamplesService _instance = new();
    public static GalleryRecentSamplesService Instance => _instance;

    private readonly List<GalleryPage> _recent = [];
    private const int MaxRecent = 20;

    public void RecordVisit(GalleryPage page)
    {
        _recent.RemoveAll(p => p.UniqueId == page.UniqueId);
        _recent.Insert(0, page);
        if (_recent.Count > MaxRecent)
        {
            _recent.RemoveRange(MaxRecent, _recent.Count - MaxRecent);
        }
    }

    public IReadOnlyList<GalleryPage> GetRecent(int count = 8)
    {
        return _recent.Take(count).ToArray();
    }
}
