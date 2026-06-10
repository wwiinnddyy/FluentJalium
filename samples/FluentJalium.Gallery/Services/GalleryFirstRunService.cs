namespace FluentJalium.Gallery.Services;

internal sealed class GalleryFirstRunService
{
    private static readonly GalleryFirstRunService _instance = new();
    public static GalleryFirstRunService Instance => _instance;

    private bool _firstRunCompleted;

    public GalleryFirstRunService()
    {
        var flagPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FluentJalium",
            "Gallery",
            "firstRunCompleted");
        _firstRunCompleted = File.Exists(flagPath);
    }

    public bool IsFirstRun => !_firstRunCompleted;

    public void MarkCompleted()
    {
        if (_firstRunCompleted) return;
        _firstRunCompleted = true;

        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FluentJalium",
            "Gallery");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "firstRunCompleted"), "1");
    }
}
