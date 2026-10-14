using System.Globalization;

namespace FluentJalium.Localization;

/// <summary>Parsing helpers for <see cref="UiLanguage"/>, for command lines and the system culture.</summary>
public static class UiLanguageExtensions
{
    /// <summary>
    /// Accepts the spellings a user or a script is likely to pass: <c>zh</c>, <c>zh-CN</c>, <c>zh-Hans</c>,
    /// <c>zh-Hans-CN</c>, <c>en</c>, <c>en-US</c>. Anything else fails rather than guessing.
    /// </summary>
    public static bool TryParse(string? text, out UiLanguage language)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            language = UiLanguage.English;
            return false;
        }

        if (trimmed.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            language = UiLanguage.ZhHans;
            return true;
        }

        if (trimmed.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            language = UiLanguage.English;
            return true;
        }

        language = UiLanguage.English;
        return false;
    }

    /// <summary>Chinese for a Chinese UI culture, English for everything else.</summary>
    public static UiLanguage FromSystemCulture() =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase)
            ? UiLanguage.ZhHans
            : UiLanguage.English;
}
