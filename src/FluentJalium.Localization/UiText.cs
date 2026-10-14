using System.Globalization;

namespace FluentJalium.Localization;

/// <summary>
/// The runtime text catalog. English is the key form: <see cref="T(string)"/> returns the key itself unless a
/// translation is registered, so an untranslated string degrades to readable English instead of a blank or a
/// resource key, and the coverage test can name exactly what is still missing.
/// </summary>
/// <remarks>
/// <para>
/// A host registers its strings once at startup - <c>LauncherText.Register()</c> and <c>GalleryText.Register()</c>
/// - and then reads them through <see cref="T(string)"/>. Registration order does not matter, and re-registering the same
/// key with a different translation is rejected rather than silently overwritten, because that shape of mistake
/// (a copy-pasted row) is invisible at runtime and shows up as one wrong page.
/// </para>
/// <para>
/// This is deliberately not a <c>.resx</c> / <c>ResourceManager</c> pipeline. Neither Jalium.UI nor FluentJalium
/// ships one, the strings are authored in C# rather than in XML, and the catalog needs to be walkable by a test to
/// prove both languages carry the same keys.
/// </para>
/// </remarks>
public static class UiText
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, string> ChineseByEnglish = new(StringComparer.Ordinal);
    private static readonly SortedSet<string> Keys = new(StringComparer.Ordinal);

    /// <summary>The language <see cref="T(string)"/> currently answers in.</summary>
    public static UiLanguage Language { get; private set; } = UiLanguage.English;

    /// <summary>Raised after <see cref="Language"/> changes, so hosts can re-apply text to live elements.</summary>
    public static event Action? LanguageChanged;

    /// <summary>
    /// Registers one translation. <paramref name="english"/> is the key and must stay byte-identical to the
    /// English the host shows, so a key that drifts out of the UI fails the coverage test.
    /// </summary>
    public static void Add(string english, string simplifiedChinese)
    {
        ArgumentException.ThrowIfNullOrEmpty(english);
        ArgumentException.ThrowIfNullOrEmpty(simplifiedChinese);

        lock (Sync)
        {
            Keys.Add(english);
            if (ChineseByEnglish.TryGetValue(english, out var existing))
            {
                if (!string.Equals(existing, simplifiedChinese, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"\"{english}\" is already registered with a different translation. " +
                        "Two hosts claiming the same key with different text is a copy-paste mistake, not a fallback.");
                }

                return;
            }

            ChineseByEnglish[english] = simplifiedChinese;
        }
    }

    /// <summary>Switches the catalog and raises <see cref="LanguageChanged"/> when the value actually changed.</summary>
    public static void SetLanguage(UiLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        LanguageChanged?.Invoke();
    }

    /// <summary>Returns the string for <paramref name="english"/> in the current language.</summary>
    public static string T(string english)
    {
        if (string.IsNullOrEmpty(english))
        {
            return english;
        }

        if (Language != UiLanguage.ZhHans)
        {
            return english;
        }

        lock (Sync)
        {
            return ChineseByEnglish.TryGetValue(english, out var chinese) ? chinese : english;
        }
    }

    /// <summary>
    /// <see cref="T(string)"/> plus <see cref="string.Format(IFormatProvider,string,object[])"/> against
    /// <see cref="CultureInfo.CurrentCulture"/>, for the interpolated report lines a host builds at runtime.
    /// </summary>
    public static string T(string english, params object?[] arguments) =>
        Format(T(english), arguments);

    /// <summary>Formats an already-translated template. The public entry point for a string the host composed itself.</summary>
    public static string Format(string template, params object?[] arguments) =>
        arguments.Length == 0 ? template : string.Format(CultureInfo.CurrentCulture, template, arguments);

    /// <summary>Every registered key, ordinal sorted. The coverage test walks this against both languages.</summary>
    public static IReadOnlyList<string> RegisteredKeys
    {
        get
        {
            lock (Sync)
            {
                return [.. Keys];
            }
        }
    }

    /// <summary>Keys with no 简体中文 translation, so they render as English in Chinese mode.</summary>
    public static IReadOnlyList<string> MissingChinese
    {
        get
        {
            lock (Sync)
            {
                return [.. Keys.Where(key => !ChineseByEnglish.ContainsKey(key))];
            }
        }
    }

    /// <summary>True when <paramref name="english"/> has a registered 简体中文 translation.</summary>
    public static bool HasChinese(string english)
    {
        lock (Sync)
        {
            return ChineseByEnglish.ContainsKey(english);
        }
    }

    /// <summary>Clears every registration. Tests only: the catalog is process-wide static state.</summary>
    internal static void ResetForTests()
    {
        lock (Sync)
        {
            ChineseByEnglish.Clear();
            Keys.Clear();
            Language = UiLanguage.English;
            LanguageChanged = null;
        }
    }
}
