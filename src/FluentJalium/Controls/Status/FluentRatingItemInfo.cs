using Jalium.UI;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Base for the two ways a rating item can be drawn. Upstream declares <c>RatingItemInfo</c> as an empty
/// <c>DependencyObject</c> (<c>RatingControl.idl:3-9</c>) and then decides the flavour by asking whether the value
/// is a <c>RatingItemFontInfo</c>; anything that is not is treated as an image. The same rule is kept here, and the
/// fail-fast upstream performs when <see cref="FluentRatingControl.ItemInfo"/> is null is replaced by a guard
/// (audit clause 3).
/// </summary>
public class FluentRatingItemInfo : DependencyObject
{
}

/// <summary>
/// Six glyph strings, one per role, exactly the six <c>RatingControl.idl:17-22</c> names. The runtime default is
/// the empty string for all of them and "absent" is decided by length rather than by null
/// (<c>RatingControl.cpp:478-486</c>), so a role left unset falls through to the chain in
/// <see cref="GlyphFor"/>. There is deliberately no "pointer over unselected" member: upstream has none either,
/// which is why that state asks for the placeholder role's glyph.
/// </summary>
public class FluentRatingItemFontInfo : FluentRatingItemInfo
{
    /// <summary>Identifies the <see cref="Glyph"/> dependency property.</summary>
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
        nameof(Glyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="DisabledGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty DisabledGlyphProperty = DependencyProperty.Register(
        nameof(DisabledGlyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="UnsetGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty UnsetGlyphProperty = DependencyProperty.Register(
        nameof(UnsetGlyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="PlaceholderGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderGlyphProperty = DependencyProperty.Register(
        nameof(PlaceholderGlyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="PointerOverGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty PointerOverGlyphProperty = DependencyProperty.Register(
        nameof(PointerOverGlyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="PointerOverPlaceholderGlyph"/> dependency property.</summary>
    public static readonly DependencyProperty PointerOverPlaceholderGlyphProperty = DependencyProperty.Register(
        nameof(PointerOverPlaceholderGlyph), typeof(string), typeof(FluentRatingItemFontInfo), new PropertyMetadata(string.Empty));

    /// <summary>The star a settled rating draws. The only role with nowhere further to fall back to.</summary>
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty)!;
        set => SetValue(GlyphProperty, value);
    }

    /// <summary>The star a disabled rating draws; falls back to <see cref="Glyph"/>.</summary>
    public string DisabledGlyph
    {
        get => (string)GetValue(DisabledGlyphProperty)!;
        set => SetValue(DisabledGlyphProperty, value);
    }

    /// <summary>The star the background layer draws. Upstream's checked theme puts the hollow star here.</summary>
    public string UnsetGlyph
    {
        get => (string)GetValue(UnsetGlyphProperty)!;
        set => SetValue(UnsetGlyphProperty, value);
    }

    /// <summary>The star drawn for <see cref="FluentRatingControl.PlaceholderValue"/>; falls back to
    /// <see cref="Glyph"/>.</summary>
    public string PlaceholderGlyph
    {
        get => (string)GetValue(PlaceholderGlyphProperty)!;
        set => SetValue(PlaceholderGlyphProperty, value);
    }

    /// <summary>The star drawn while the pointer is over a settled rating; falls back to <see cref="Glyph"/>.</summary>
    public string PointerOverGlyph
    {
        get => (string)GetValue(PointerOverGlyphProperty)!;
        set => SetValue(PointerOverGlyphProperty, value);
    }

    /// <summary>The star drawn while the pointer is over an unsettled rating; falls back to
    /// <see cref="PlaceholderGlyph"/> and on through to <see cref="Glyph"/>.</summary>
    public string PointerOverPlaceholderGlyph
    {
        get => (string)GetValue(PointerOverPlaceholderGlyphProperty)!;
        set => SetValue(PointerOverPlaceholderGlyphProperty, value);
    }

    /// <summary>The glyph for one role, following upstream's fall-through
    /// (<c>RatingControl.cpp:447-486</c>): an empty string counts as "not provided".</summary>
    internal string GlyphFor(FluentRatingControlDisplayState role) => role switch
    {
        FluentRatingControlDisplayState.Disabled => OrElse(DisabledGlyph, FluentRatingControlDisplayState.Set),
        FluentRatingControlDisplayState.PointerOverSet => OrElse(PointerOverGlyph, FluentRatingControlDisplayState.Set),
        FluentRatingControlDisplayState.PointerOverPlaceholder => OrElse(PointerOverPlaceholderGlyph, FluentRatingControlDisplayState.Placeholder),
        FluentRatingControlDisplayState.Placeholder => OrElse(PlaceholderGlyph, FluentRatingControlDisplayState.Set),
        FluentRatingControlDisplayState.Unset => OrElse(UnsetGlyph, FluentRatingControlDisplayState.Set),
        _ => Glyph,
    };

    private string OrElse(string candidate, FluentRatingControlDisplayState fallback) =>
        candidate.Length > 0 ? candidate : GlyphFor(fallback);
}

/// <summary>
/// Six images, one per role, the <c>RatingControl.idl:34-39</c> names. "Absent" is decided by null here where the
/// font flavour decides by string length (<c>RatingControl.cpp:488-520</c>).
/// </summary>
public class FluentRatingItemImageInfo : FluentRatingItemInfo
{
    /// <summary>Identifies the <see cref="Image"/> dependency property.</summary>
    public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
        nameof(Image), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>Identifies the <see cref="DisabledImage"/> dependency property.</summary>
    public static readonly DependencyProperty DisabledImageProperty = DependencyProperty.Register(
        nameof(DisabledImage), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>Identifies the <see cref="UnsetImage"/> dependency property.</summary>
    public static readonly DependencyProperty UnsetImageProperty = DependencyProperty.Register(
        nameof(UnsetImage), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>Identifies the <see cref="PlaceholderImage"/> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderImageProperty = DependencyProperty.Register(
        nameof(PlaceholderImage), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>Identifies the <see cref="PointerOverImage"/> dependency property.</summary>
    public static readonly DependencyProperty PointerOverImageProperty = DependencyProperty.Register(
        nameof(PointerOverImage), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>Identifies the <see cref="PointerOverPlaceholderImage"/> dependency property.</summary>
    public static readonly DependencyProperty PointerOverPlaceholderImageProperty = DependencyProperty.Register(
        nameof(PointerOverPlaceholderImage), typeof(ImageSource), typeof(FluentRatingItemImageInfo));

    /// <summary>The image a settled rating draws.</summary>
    public ImageSource? Image
    {
        get => (ImageSource?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    /// <summary>The image a disabled rating draws; falls back to <see cref="Image"/>.</summary>
    public ImageSource? DisabledImage
    {
        get => (ImageSource?)GetValue(DisabledImageProperty);
        set => SetValue(DisabledImageProperty, value);
    }

    /// <summary>The image the background layer draws; falls back to <see cref="Image"/>.</summary>
    public ImageSource? UnsetImage
    {
        get => (ImageSource?)GetValue(UnsetImageProperty);
        set => SetValue(UnsetImageProperty, value);
    }

    /// <summary>The image drawn for <see cref="FluentRatingControl.PlaceholderValue"/>; falls back to
    /// <see cref="Image"/>.</summary>
    public ImageSource? PlaceholderImage
    {
        get => (ImageSource?)GetValue(PlaceholderImageProperty);
        set => SetValue(PlaceholderImageProperty, value);
    }

    /// <summary>The image drawn while the pointer is over a settled rating; falls back to <see cref="Image"/>.</summary>
    public ImageSource? PointerOverImage
    {
        get => (ImageSource?)GetValue(PointerOverImageProperty);
        set => SetValue(PointerOverImageProperty, value);
    }

    /// <summary>The image drawn while the pointer is over an unsettled rating; falls back to
    /// <see cref="PlaceholderImage"/> and on to <see cref="Image"/>.</summary>
    public ImageSource? PointerOverPlaceholderImage
    {
        get => (ImageSource?)GetValue(PointerOverPlaceholderImageProperty);
        set => SetValue(PointerOverPlaceholderImageProperty, value);
    }

    /// <summary>The image for one role, with the same fall-through the glyph table uses.</summary>
    internal ImageSource? SourceFor(FluentRatingControlDisplayState role) => role switch
    {
        FluentRatingControlDisplayState.Disabled => DisabledImage ?? SourceFor(FluentRatingControlDisplayState.Set),
        FluentRatingControlDisplayState.PointerOverSet => PointerOverImage ?? Image,
        FluentRatingControlDisplayState.PointerOverPlaceholder => PointerOverPlaceholderImage ?? PlaceholderImage ?? Image,
        FluentRatingControlDisplayState.Placeholder => PlaceholderImage ?? Image,
        FluentRatingControlDisplayState.Unset => UnsetImage ?? Image,
        _ => Image,
    };
}
