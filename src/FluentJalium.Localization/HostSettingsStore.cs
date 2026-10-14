using System.Text.Json;

namespace FluentJalium.Localization;

/// <summary>
/// Reads and writes <see cref="HostSettings"/> as JSON next to the executable.
/// </summary>
/// <remarks>
/// <para>
/// Every operation is best effort and silent about failure. The file lives beside the binary, which on a
/// machine-installed app is a directory the process may not be allowed to write, and a launcher that refuses to
/// start because its preferences could not be saved is worse than one that forgets a preference. A missing,
/// unreadable, truncated or hand-mangled file therefore all resolve to the shipped defaults.
/// </para>
/// <para>
/// Unknown enum spellings fall back per field rather than throwing the document away, so adding a renderer in a
/// later build does not cost an existing user the rest of their settings.
/// </para>
/// </remarks>
public static class HostSettingsStore
{
    /// <summary>The file name written beside the executable.</summary>
    public const string FileName = "fluentjalium.settings.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>The default location: <c>launcher.settings.json</c>-style name beside the running binary.</summary>
    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>Loads settings, or returns the defaults when the file is absent or unusable.</summary>
    public static HostSettings Load(string? path = null)
    {
        var resolved = path ?? DefaultPath;
        try
        {
            if (!File.Exists(resolved))
            {
                return new HostSettings();
            }

            var document = JsonSerializer.Deserialize<HostSettingsDocument>(
                File.ReadAllText(resolved), SerializerOptions);
            return FromDocument(document);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                              or JsonException or NotSupportedException)
        {
            return new HostSettings();
        }
    }

    /// <summary>
    /// Writes settings, reporting success rather than throwing. <c>false</c> means the preferences were not
    /// persisted and the caller may surface that, but never at the cost of failing the launch.
    /// </summary>
    public static bool TrySave(HostSettings settings, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var resolved = path ?? DefaultPath;
        try
        {
            File.WriteAllText(resolved, JsonSerializer.Serialize(ToDocument(settings), SerializerOptions));
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                                              or JsonException or NotSupportedException
                                              or ArgumentException)
        {
            return false;
        }
    }

    internal static HostSettings FromDocument(HostSettingsDocument? document)
    {
        var settings = new HostSettings();
        if (document is null)
        {
            return settings;
        }

        settings.Backend = Parse(document.Backend, settings.Backend);
        settings.Engine = Parse(document.Engine, settings.Engine);
        settings.Language = Parse(document.Language, settings.Language);
        settings.PathAntiAliasing = Parse(document.PathAntiAliasing, settings.PathAntiAliasing);
        settings.Theme = Parse(document.Theme, settings.Theme);
        settings.ReduceMotion = document.ReduceMotion ?? settings.ReduceMotion;
        return settings;
    }

    internal static HostSettingsDocument ToDocument(HostSettings settings) => new()
    {
        Backend = settings.Backend.ToString(),
        Engine = settings.Engine.ToString(),
        Language = settings.Language.ToString(),
        PathAntiAliasing = settings.PathAntiAliasing.ToString(),
        Theme = settings.Theme.ToString(),
        ReduceMotion = settings.ReduceMotion,
    };

    private static TEnum Parse<TEnum>(string? text, TEnum fallback) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(text, ignoreCase: true, out var parsed) ? parsed : fallback;
}
