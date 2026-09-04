namespace FluentJalium.Gallery.Services;

/// <summary>
/// In-app navigation requests from pages that the frame instantiates without callbacks
/// (search results, section grids). The shell subscribes and routes by UniqueId,
/// mirroring WinUI Gallery's frame navigation from item clicks.
/// </summary>
internal static class GalleryNavigationBroker
{
    public static event Action<string>? NavigateRequested;

    public static void RequestNavigate(string uniqueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);
        NavigateRequested?.Invoke(uniqueId);
    }
}
