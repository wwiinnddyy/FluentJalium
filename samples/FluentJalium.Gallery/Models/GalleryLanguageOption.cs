namespace FluentJalium.Gallery.Models;

internal sealed record GalleryLanguageOption(
    string CultureName,
    string DisplayName,
    string NativeName)
{
    public string Label => string.Equals(DisplayName, NativeName, StringComparison.Ordinal)
        ? DisplayName
        : $"{DisplayName} / {NativeName}";

    public override string ToString() => Label;
}
