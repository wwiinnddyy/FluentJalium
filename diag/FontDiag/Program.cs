// Runtime Font Loading Diagnostic
// Tests the actual FluentIconFontLoader extraction and registration flow

using System.Reflection;
using System.Runtime.InteropServices;

Console.WriteLine("=== Runtime Font Loading Diagnostic ===\n");

// 1. Load FluentJalium assembly
var assemblyPath = "d:/github/Jalium/FluentJalium/src/FluentJalium/bin/Debug/net10.0/FluentJalium.dll";

Console.WriteLine($"Assembly path: {assemblyPath}");
Console.WriteLine($"Exists: {File.Exists(assemblyPath)}");

if (!File.Exists(assemblyPath))
{
    Console.WriteLine("ERROR: FluentJalium.dll not found. Build the project first.");
    return;
}

var assembly = Assembly.LoadFrom(assemblyPath);
Console.WriteLine($"Loaded: {assembly.FullName}\n");

// 2. List embedded resources
Console.WriteLine("=== Embedded Resources (*.ttf) ===");
var resourceNames = assembly.GetManifestResourceNames();
var ttfResources = resourceNames.Where(n => n.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)).ToArray();
foreach (var name in ttfResources)
{
    using var stream = assembly.GetManifestResourceStream(name);
    Console.WriteLine($"  {name} ({stream?.Length:N0} bytes)");
}
if (ttfResources.Length == 0)
{
    Console.WriteLine("  WARNING: No TTF embedded resources found!");
    Console.WriteLine($"  All resources ({resourceNames.Length}):");
    foreach (var n in resourceNames.Take(20))
        Console.WriteLine($"    {n}");
}
Console.WriteLine();

// 3. Extract resources manually (same logic as FluentIconFontLoader)
var fontDir = Path.Combine(Path.GetTempPath(), "FluentJalium", "Fonts");
Directory.CreateDirectory(fontDir);
Console.WriteLine($"=== Extracting fonts to {fontDir} ===");

string? regularPath = null, filledPath = null;
foreach (var name in ttfResources)
{
    var parts = name.Split('.');
    var fileName = parts.Length >= 2 ? $"{parts[^2]}.{parts[^1]}" : name;
    var outputPath = Path.Combine(fontDir, fileName);

    using var stream = assembly.GetManifestResourceStream(name);
    if (stream != null)
    {
        if (File.Exists(outputPath) && new FileInfo(outputPath).Length == stream.Length)
        {
            Console.WriteLine($"  {fileName}: already exists ({stream.Length:N0} bytes) - skip");
        }
        else
        {
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
            Console.WriteLine($"  {fileName}: extracted ({fs.Length:N0} bytes)");
        }
    }
    else
    {
        Console.WriteLine($"  {fileName}: FAILED to open resource stream!");
    }

    if (name.Contains("Regular")) regularPath = outputPath;
    if (name.Contains("Filled")) filledPath = outputPath;
}
Console.WriteLine();

// 4. Register fonts with AddFontResourceExW
Console.WriteLine("=== Registering fonts with Windows GDI ===");
if (regularPath != null && File.Exists(regularPath))
{
    int result = NativeMethods.AddFontResourceExW(regularPath, 0x10, nint.Zero);
    Console.WriteLine($"  Regular: AddFontResourceExW returned {result} (0 = failure, >0 = fonts added)");
}
if (filledPath != null && File.Exists(filledPath))
{
    int result = NativeMethods.AddFontResourceExW(filledPath, 0x10, nint.Zero);
    Console.WriteLine($"  Filled: AddFontResourceExW returned {result} (0 = failure, >0 = fonts added)");
}
Console.WriteLine();

// 5. Verify with GDI EnumFontFamilies
Console.WriteLine("=== Verifying font availability via GDI ===");
var hdc = NativeMethods.GetDC(nint.Zero);
if (hdc != nint.Zero)
{
    var families = new List<string>();
    NativeMethods.EnumFontFamExProc callback = (logFont, textMetric, fontType, lParam) =>
    {
        if (logFont != nint.Zero)
        {
            var lf = Marshal.PtrToStructure<NativeMethods.LOGFONT>(logFont);
            families.Add(lf.lfFaceName);
        }
        return 1;
    };

    NativeMethods.EnumFontFamiliesExW(hdc, nint.Zero, callback, nint.Zero, 0);

    var matchingFonts = families.Where(f =>
        f.Contains("Fluent", StringComparison.OrdinalIgnoreCase) ||
        f.Contains("SystemIcon", StringComparison.OrdinalIgnoreCase)).ToArray();
    Console.WriteLine($"  Total fonts enumerated: {families.Count}");
    Console.WriteLine($"  Fluent-related fonts found: {matchingFonts.Length}");
    foreach (var f in matchingFonts)
        Console.WriteLine($"    \"{f}\"");

    bool foundRegular = families.Any(f => f == "FluentSystemIcons-Regular");
    bool foundFilled = families.Any(f => f == "FluentSystemIcons-Filled");
    Console.WriteLine($"\n  FluentSystemIcons-Regular: {(foundRegular ? "FOUND" : "NOT FOUND")}");
    Console.WriteLine($"  FluentSystemIcons-Filled: {(foundFilled ? "FOUND" : "NOT FOUND")}");

    NativeMethods.ReleaseDC(nint.Zero, hdc);
}
Console.WriteLine();

// 6. Test through FluentIconFonts API (reflection since it may trigger lazy load)
Console.WriteLine("=== Testing FluentIconFonts API via reflection ===");
try
{
    var fluentIconFontsType = assembly.GetType("FluentJalium.Icon.FluentIconFonts");
    if (fluentIconFontsType != null)
    {
        var regularProp = fluentIconFontsType.GetProperty("Regular", BindingFlags.Public | BindingFlags.Static);
        var filledProp = fluentIconFontsType.GetProperty("Filled", BindingFlags.Public | BindingFlags.Static);

        var regularValue = regularProp?.GetValue(null);
        var filledValue = filledProp?.GetValue(null);

        Console.WriteLine($"  FluentIconFonts.Regular = \"{regularValue}\"");
        Console.WriteLine($"  FluentIconFonts.Filled = \"{filledValue}\"");
    }
    else
    {
        Console.WriteLine("  WARNING: FluentIconFonts type not found");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"  ERROR: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

Console.WriteLine("\n=== Done ===");

static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string lfFaceName;
    }

    public delegate int EnumFontFamExProc(IntPtr lpLogfont, IntPtr lpTextMetric, uint dwFontType, IntPtr lParam);

    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceExW", CharSet = CharSet.Unicode)]
    public static extern int AddFontResourceExW(string lpFileName, uint fl, nint pdv);

    [DllImport("user32.dll")]
    public static extern nint GetDC(nint hwnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(nint hwnd, nint hdc);

    [DllImport("gdi32.dll", EntryPoint = "EnumFontFamiliesExW", CharSet = CharSet.Unicode)]
    public static extern int EnumFontFamiliesExW(IntPtr hdc, IntPtr lpLogfont, EnumFontFamExProc lpEnumFontFamExProc, IntPtr lParam, uint dwFlags);
}
