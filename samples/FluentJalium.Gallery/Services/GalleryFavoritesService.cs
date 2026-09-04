namespace FluentJalium.Gallery.Services;

/// <summary>
/// Persisted favorite samples, mirroring WinUI Gallery's Favorites rail on Home.
/// Favorite state is keyed by locale-independent page UniqueId and survives restarts.
/// </summary>
internal sealed class GalleryFavoritesService
{
    private static readonly GalleryFavoritesService _instance = new();
    public static GalleryFavoritesService Instance => _instance;

    private readonly List<string> _favorites = [];
    private readonly string _favoritesPath;

    public GalleryFavoritesService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FluentJalium",
            "Gallery",
            "favorites"))
    {
    }

    internal GalleryFavoritesService(string favoritesPath)
    {
        _favoritesPath = favoritesPath;
        Load();
    }

    public event EventHandler? FavoritesChanged;

    public bool IsFavorite(string uniqueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);
        return _favorites.Contains(uniqueId, StringComparer.Ordinal);
    }

    public IReadOnlyList<string> GetFavorites()
    {
        return _favorites.ToArray();
    }

    public bool ToggleFavorite(string uniqueId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);

        var isFavorite = !IsFavorite(uniqueId);
        SetFavorite(uniqueId, isFavorite);
        return isFavorite;
    }

    public void SetFavorite(string uniqueId, bool isFavorite)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uniqueId);

        var changed = isFavorite
            ? AddFavorite(uniqueId)
            : _favorites.RemoveAll(id => string.Equals(id, uniqueId, StringComparison.Ordinal)) > 0;

        if (changed)
        {
            Save();
            FavoritesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool AddFavorite(string uniqueId)
    {
        if (IsFavorite(uniqueId))
        {
            return false;
        }

        _favorites.Insert(0, uniqueId);
        return true;
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_favoritesPath))
            {
                return;
            }

            foreach (var line in File.ReadAllLines(_favoritesPath))
            {
                var uniqueId = line.Trim();
                if (!string.IsNullOrWhiteSpace(uniqueId) && !IsFavorite(uniqueId))
                {
                    _favorites.Add(uniqueId);
                }
            }
        }
        catch
        {
            _favorites.Clear();
        }
    }

    private void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_favoritesPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(_favoritesPath, _favorites);
        }
        catch
        {
            // Favorites are best-effort: a read-only profile must not break navigation.
        }
    }
}
