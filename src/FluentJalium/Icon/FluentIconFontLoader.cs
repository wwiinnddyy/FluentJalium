using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace FluentJalium.Icon;

/// <summary>
/// Extracts embedded Fluent System Icons TTF resources and installs them
/// into the Windows user font directory so that Jalium.UI's DirectWrite-based
/// text rendering engine can resolve the font family names at render time.
/// </summary>
internal static partial class FluentIconFontLoader
{
    private const string RegularResourceName = "FluentJalium.Fonts.FluentSystemIcons-Regular.ttf";
    private const string FilledResourceName = "FluentJalium.Fonts.FluentSystemIcons-Filled.ttf";

    private const string RegularFamilyName = "FluentSystemIcons-Regular";
    private const string FilledFamilyName = "FluentSystemIcons-Filled";

    private const string RegularFileName = "FluentSystemIcons-Regular.ttf";
    private const string FilledFileName = "FluentSystemIcons-Filled.ttf";

    private const string FontsRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts";

    private static readonly Lazy<LoadResult> s_loadResult = new(Load, isThreadSafe: true);

    /// <summary>
    /// Gets the font family name for the regular icon set.
    /// Triggers font extraction and registration on first access.
    /// </summary>
    public static string Regular => s_loadResult.Value.RegularFamily;

    /// <summary>
    /// Gets the font family name for the filled icon set.
    /// Triggers font extraction and registration on first access.
    /// </summary>
    public static string Filled => s_loadResult.Value.FilledFamily;

    /// <summary>
    /// Ensures fonts are extracted and registered. Call once at application startup
    /// before creating any icon controls.
    /// </summary>
    public static void EnsureLoaded()
    {
        _ = s_loadResult.Value;
    }

    private static LoadResult Load()
    {
        var assembly = typeof(FluentIconFontLoader).Assembly;

        if (OperatingSystem.IsWindows())
        {
            // Install to Windows user fonts directory (%LOCALAPPDATA%\Microsoft\Windows\Fonts\)
            // so that DirectWrite's system font collection can discover them.
            var userFontsDir = GetUserFontsDirectory();
            Directory.CreateDirectory(userFontsDir);

            var regularPath = ExtractResource(assembly, RegularResourceName, userFontsDir, RegularFileName);
            var filledPath = ExtractResource(assembly, FilledResourceName, userFontsDir, FilledFileName);

            var installed = false;
            installed |= InstallFontInRegistry(RegularFamilyName, RegularFileName, regularPath);
            installed |= InstallFontInRegistry(FilledFamilyName, FilledFileName, filledPath);

            // Also register with GDI (without FR_PRIVATE) for legacy GDI consumers.
            if (regularPath != null) AddFontResourceW(regularPath);
            if (filledPath != null) AddFontResourceW(filledPath);

            // Notify all top-level windows that the font list has changed.
            if (installed)
            {
                SendMessageTimeoutW(
                    HWND_BROADCAST, WM_FONTCHANGE,
                    nuint.Zero, nint.Zero,
                    SMTO_ABORTIFHUNG, 1000, out _);
            }
        }
        else
        {
            // On non-Windows, extract to a temp directory for Fontconfig discovery.
            var fontDir = Path.Combine(Path.GetTempPath(), "FluentJalium", "Fonts");
            Directory.CreateDirectory(fontDir);
            ExtractResource(assembly, RegularResourceName, fontDir, RegularFileName);
            ExtractResource(assembly, FilledResourceName, fontDir, FilledFileName);
        }

        return new LoadResult(RegularFamilyName, FilledFamilyName);
    }

    private static string GetUserFontsDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "Microsoft", "Windows", "Fonts");
    }

    private static string? ExtractResource(Assembly assembly, string resourceName, string outputDir, string fileName)
    {
        var outputPath = Path.Combine(outputDir, fileName);

        // Skip extraction if the file already exists with the expected size.
        if (File.Exists(outputPath))
        {
            using var existingStream = assembly.GetManifestResourceStream(resourceName);
            if (existingStream != null && new FileInfo(outputPath).Length == existingStream.Length)
            {
                return outputPath;
            }
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return null;
        }

        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        stream.CopyTo(fileStream);

        return outputPath;
    }

    /// <summary>
    /// Registers a font in HKCU registry so that DirectWrite's system font collection includes it.
    /// Returns true if a new registry entry was written.
    /// </summary>
    private static bool InstallFontInRegistry(string familyName, string fileName, string? fontPath)
    {
        if (fontPath == null) return false;

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(FontsRegistryKey, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(FontsRegistryKey);

            // Registry value name format: "FontName (TrueType)"
            var valueName = $"{familyName} (TrueType)";
            var existing = key.GetValue(valueName);

            if (existing is string existingPath && existingPath == fileName)
            {
                return false; // Already registered
            }

            key.SetValue(valueName, fileName, RegistryValueKind.String);
            return true;
        }
        catch
        {
            // Registry access may fail in restricted environments; continue gracefully.
            return false;
        }
    }

    [LibraryImport("gdi32.dll", EntryPoint = "AddFontResourceW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int AddFontResourceW(string lpFileName);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint SendMessageTimeoutW(
        nint hWnd, uint msg, nuint wParam, nint lParam,
        uint fuFlags, uint uTimeout, out nuint lpdwResult);

    private const nint HWND_BROADCAST = 0xFFFF;
    private const uint WM_FONTCHANGE = 0x001D;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

    private sealed record LoadResult(string RegularFamily, string FilledFamily);
}
