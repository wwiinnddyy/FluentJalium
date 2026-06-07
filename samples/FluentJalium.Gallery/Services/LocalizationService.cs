using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FluentJalium.Gallery.Services;

/// <summary>
/// Service that manages application localization and language switching.
/// Supports English, Chinese (Simplified), and Chinese (Traditional).
/// </summary>
public sealed class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    private CultureInfo _currentCulture;

    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private LocalizationService()
    {
        _currentCulture = CultureInfo.CurrentUICulture;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the current culture. Setting this will trigger UI updates.
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            if (_currentCulture.Name == value.Name) return;

            _currentCulture = value;
            CultureInfo.CurrentUICulture = value;
            CultureInfo.CurrentCulture = value;

            OnPropertyChanged();
            OnPropertyChanged("Item[]"); // Trigger all string updates
        }
    }

    /// <summary>
    /// Gets a localized string by key. Used for binding in code.
    /// </summary>
    public string this[string key]
    {
        get
        {
            try
            {
                return Resources.Strings.ResourceManager.GetString(key, _currentCulture) ?? key;
            }
            catch
            {
                return key;
            }
        }
    }

    /// <summary>
    /// Gets all supported languages.
    /// </summary>
    public IReadOnlyList<LanguageInfo> SupportedLanguages { get; } = new[]
    {
        new LanguageInfo("en-US", "English (United States)", "🇺🇸"),
        new LanguageInfo("zh-CN", "简体中文 (Simplified Chinese)", "🇨🇳"),
        new LanguageInfo("zh-TW", "繁體中文 (Traditional Chinese)", "🇹🇼")
    };

    /// <summary>
    /// Changes the current language by culture name.
    /// </summary>
    public void ChangeLanguage(string cultureName)
    {
        try
        {
            var culture = new CultureInfo(cultureName);
            CurrentCulture = culture;
        }
        catch (CultureNotFoundException)
        {
            // Fallback to English if culture not found
            CurrentCulture = new CultureInfo("en-US");
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Represents information about a supported language.
/// </summary>
public sealed class LanguageInfo
{
    public LanguageInfo(string cultureName, string displayName, string flag)
    {
        CultureName = cultureName;
        DisplayName = displayName;
        Flag = flag;
    }

    public string CultureName { get; }
    public string DisplayName { get; }
    public string Flag { get; }

    public override string ToString() => $"{Flag} {DisplayName}";
}
