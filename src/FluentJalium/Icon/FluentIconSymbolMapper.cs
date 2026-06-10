using Jalium.UI.Controls;

namespace FluentJalium.Icon;

internal static class FluentIconSymbolMapper
{
    public static string GetGlyph(FluentIconRegular icon) => ConvertCodePoint((int)icon);

    public static string GetGlyph(FluentIconFilled icon) => ConvertCodePoint((int)icon);

    private static string ConvertCodePoint(int codePoint)
        => codePoint <= 0 ? string.Empty : char.ConvertFromUtf32(codePoint);
}
