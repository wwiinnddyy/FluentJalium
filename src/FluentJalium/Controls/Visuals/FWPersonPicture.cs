using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium PersonPicture control for displaying user avatars.
/// </summary>
public class FWPersonPicture : Control, IFluentJaliumControl
{
    private const double DefaultAvatarSize = 48.0;
    private const double MinimumAvatarSize = 16.0;
    private const double MaximumBadgeNumber = 99.0;

    public static readonly DependencyProperty ProfilePictureProperty =
        DependencyProperty.Register(nameof(ProfilePicture), typeof(ImageSource), typeof(FWPersonPicture),
            new PropertyMetadata(null, OnDisplayPropertyChanged));

    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(nameof(Initials), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(nameof(DisplayName), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

    public static readonly DependencyProperty IsGroupProperty =
        DependencyProperty.Register(nameof(IsGroup), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false, OnDisplayPropertyChanged));

    public static readonly DependencyProperty BadgeNumberProperty =
        DependencyProperty.Register(nameof(BadgeNumber), typeof(int), typeof(FWPersonPicture),
            new PropertyMetadata(0, OnBadgePropertyChanged), IsValidBadgeNumber);

    public static readonly DependencyProperty BadgeGlyphProperty =
        DependencyProperty.Register(nameof(BadgeGlyph), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty, OnBadgePropertyChanged));

    public static readonly DependencyProperty BadgeImageSourceProperty =
        DependencyProperty.Register(nameof(BadgeImageSource), typeof(ImageSource), typeof(FWPersonPicture),
            new PropertyMetadata(null, OnBadgePropertyChanged));

    public static readonly DependencyProperty PreferSmallImageProperty =
        DependencyProperty.Register(nameof(PreferSmallImage), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false, OnVisualPropertyChanged));

    public static readonly DependencyProperty BadgeBackgroundProperty =
        DependencyProperty.Register(nameof(BadgeBackground), typeof(Brush), typeof(FWPersonPicture),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty BadgeForegroundProperty =
        DependencyProperty.Register(nameof(BadgeForeground), typeof(Brush), typeof(FWPersonPicture),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty BadgeBorderBrushProperty =
        DependencyProperty.Register(nameof(BadgeBorderBrush), typeof(Brush), typeof(FWPersonPicture),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty BadgeBorderThicknessProperty =
        DependencyProperty.Register(nameof(BadgeBorderThickness), typeof(Thickness), typeof(FWPersonPicture),
            new PropertyMetadata(new Thickness(0), OnVisualPropertyChanged));

    private static readonly DependencyPropertyKey DisplayInitialsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayInitials), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DisplayInitialsProperty = DisplayInitialsPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey DisplayKindPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayKind), typeof(FWPersonPictureDisplayKind), typeof(FWPersonPicture),
            new PropertyMetadata(FWPersonPictureDisplayKind.Initials));

    public static readonly DependencyProperty DisplayKindProperty = DisplayKindPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey HasProfilePicturePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasProfilePicture), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasProfilePictureProperty = HasProfilePicturePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey HasBadgePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasBadge), typeof(bool), typeof(FWPersonPicture),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasBadgeProperty = HasBadgePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BadgeKindPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeKind), typeof(FWPersonPictureBadgeKind), typeof(FWPersonPicture),
            new PropertyMetadata(FWPersonPictureBadgeKind.None));

    public static readonly DependencyProperty BadgeKindProperty = BadgeKindPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BadgeDisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(BadgeDisplayText), typeof(string), typeof(FWPersonPicture),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeDisplayTextProperty = BadgeDisplayTextPropertyKey.DependencyProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="FWPersonPicture"/> class.
    /// </summary>
    public FWPersonPicture()
    {
        Width = DefaultAvatarSize;
        Height = DefaultAvatarSize;
        Focusable = false;
        UpdateDisplayState();
    }

    /// <summary>
    /// Gets or sets the profile picture image source.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ImageSource? ProfilePicture
    {
        get => (ImageSource?)GetValue(ProfilePictureProperty);
        set => SetValue(ProfilePictureProperty, value);
    }

    /// <summary>
    /// Gets or sets the initials to display when no profile picture is available.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string Initials
    {
        get => (string)GetValue(InitialsProperty)!;
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>
    /// Gets or sets the display name used to generate initials.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty)!;
        set => SetValue(DisplayNameProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this represents a group rather than an individual.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool IsGroup
    {
        get => (bool)GetValue(IsGroupProperty)!;
        set => SetValue(IsGroupProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge number to display (0 to hide badge).
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int BadgeNumber
    {
        get => (int)GetValue(BadgeNumberProperty)!;
        set => SetValue(BadgeNumberProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge glyph to display.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string BadgeGlyph
    {
        get => (string)GetValue(BadgeGlyphProperty)!;
        set => SetValue(BadgeGlyphProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge image source.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ImageSource? BadgeImageSource
    {
        get => (ImageSource?)GetValue(BadgeImageSourceProperty);
        set => SetValue(BadgeImageSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to prefer smaller image sizes for better performance.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public bool PreferSmallImage
    {
        get => (bool)GetValue(PreferSmallImageProperty)!;
        set => SetValue(PreferSmallImageProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used behind the badge.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Brush? BadgeBackground
    {
        get => (Brush?)GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for badge text or glyph content.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Brush? BadgeForeground
    {
        get => (Brush?)GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge outline brush.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Brush? BadgeBorderBrush
    {
        get => (Brush?)GetValue(BadgeBorderBrushProperty);
        set => SetValue(BadgeBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the badge outline thickness.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Thickness BadgeBorderThickness
    {
        get => (Thickness)GetValue(BadgeBorderThicknessProperty)!;
        set => SetValue(BadgeBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets the initials currently used for fallback avatar rendering.
    /// </summary>
    public string DisplayInitials => (string)GetValue(DisplayInitialsProperty)!;

    /// <summary>
    /// Gets the resolved avatar display kind.
    /// </summary>
    public FWPersonPictureDisplayKind DisplayKind => (FWPersonPictureDisplayKind)GetValue(DisplayKindProperty)!;

    /// <summary>
    /// Gets a value indicating whether the avatar has an image source.
    /// </summary>
    public bool HasProfilePicture => (bool)GetValue(HasProfilePictureProperty)!;

    /// <summary>
    /// Gets a value indicating whether the badge is visible.
    /// </summary>
    public bool HasBadge => (bool)GetValue(HasBadgeProperty)!;

    /// <summary>
    /// Gets the resolved badge display kind.
    /// </summary>
    public FWPersonPictureBadgeKind BadgeKind => (FWPersonPictureBadgeKind)GetValue(BadgeKindProperty)!;

    /// <summary>
    /// Gets the text currently displayed inside the badge.
    /// </summary>
    public string BadgeDisplayText => (string)GetValue(BadgeDisplayTextProperty)!;

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = Math.Max(MinWidth, ResolveDesiredLength(Width));
        var height = Math.Max(MinHeight, ResolveDesiredLength(Height));

        if (!double.IsInfinity(availableSize.Width))
            width = Math.Min(width, availableSize.Width);
        if (!double.IsInfinity(availableSize.Height))
            height = Math.Min(height, availableSize.Height);

        return new Size(Math.Max(0, width), Math.Max(0, height));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var avatarSize = Math.Min(RenderSize.Width, RenderSize.Height);
        var avatarRect = new Rect(
            (RenderSize.Width - avatarSize) / 2.0,
            (RenderSize.Height - avatarSize) / 2.0,
            avatarSize,
            avatarSize);
        var center = new Point(avatarRect.X + avatarRect.Width / 2.0, avatarRect.Y + avatarRect.Height / 2.0);
        var radius = avatarSize / 2.0;

        drawingContext.DrawEllipse(ResolveAvatarBackground(), null, center, radius, radius);

        if (ProfilePicture != null)
        {
            drawingContext.DrawImage(ProfilePicture, avatarRect);
        }
        else
        {
            DrawFallbackText(drawingContext, avatarRect);
        }

        var borderBrush = ResolveAvatarBorderBrush();
        var borderThickness = Math.Max(0, BorderThickness.Left);
        if (borderBrush != null && borderThickness > 0)
        {
            var borderRadius = Math.Max(0, radius - borderThickness / 2.0);
            drawingContext.DrawEllipse(null, new Pen(borderBrush, borderThickness), center, borderRadius, borderRadius);
        }

        if (HasBadge)
        {
            DrawBadge(drawingContext, avatarRect);
        }
    }

    private void DrawFallbackText(DrawingContext drawingContext, Rect avatarRect)
    {
        var text = ResolveFallbackText();
        if (string.IsNullOrEmpty(text))
            return;

        var fontSize = Math.Max(9.0, Math.Min(avatarRect.Width, avatarRect.Height) * (DisplayKind == FWPersonPictureDisplayKind.Group ? 0.32 : 0.38));
        var formatted = CreateFormattedText(text, ResolveAvatarForeground(), FontFamily, fontSize, FontWeight);
        var x = avatarRect.X + Math.Max(0, (avatarRect.Width - formatted.Width) / 2.0);
        var y = avatarRect.Y + Math.Max(0, (avatarRect.Height - formatted.Height) / 2.0);
        drawingContext.DrawText(formatted, new Point(x, y));
    }

    private void DrawBadge(DrawingContext drawingContext, Rect avatarRect)
    {
        var badgeSize = Math.Clamp(Math.Min(avatarRect.Width, avatarRect.Height) * 0.34, 12.0, 24.0);
        var badgeText = BadgeDisplayText;
        FormattedText? formatted = null;

        if (BadgeKind != FWPersonPictureBadgeKind.Image)
        {
            formatted = CreateFormattedText(
                badgeText,
                ResolveBadgeForeground(),
                FontFamily,
                Math.Max(8.0, badgeSize * 0.58),
                FontWeights.SemiBold);
        }

        var badgeWidth = formatted == null ? badgeSize : Math.Max(badgeSize, formatted.Width + 8.0);
        var badgeRect = new Rect(
            avatarRect.X + avatarRect.Width - badgeWidth,
            avatarRect.Y + avatarRect.Height - badgeSize,
            badgeWidth,
            badgeSize);
        var borderThickness = Math.Max(0, BadgeBorderThickness.Left);
        var borderBrush = borderThickness > 0 ? ResolveBadgeBorderBrush() : null;

        if (badgeWidth <= badgeSize + 0.5)
        {
            var center = new Point(badgeRect.X + badgeRect.Width / 2.0, badgeRect.Y + badgeRect.Height / 2.0);
            drawingContext.DrawEllipse(ResolveBadgeBackground(), borderBrush == null ? null : new Pen(borderBrush, borderThickness), center, badgeSize / 2.0, badgeSize / 2.0);
        }
        else
        {
            drawingContext.DrawRoundedRectangle(ResolveBadgeBackground(), borderBrush == null ? null : new Pen(borderBrush, borderThickness), badgeRect, new CornerRadius(badgeSize / 2.0));
        }

        if (BadgeKind == FWPersonPictureBadgeKind.Image && BadgeImageSource != null)
        {
            var inset = Math.Max(2.0, badgeSize * 0.18);
            drawingContext.DrawImage(BadgeImageSource, new Rect(badgeRect.X + inset, badgeRect.Y + inset, Math.Max(0, badgeRect.Width - inset * 2.0), Math.Max(0, badgeRect.Height - inset * 2.0)));
            return;
        }

        if (formatted != null && !string.IsNullOrEmpty(badgeText))
        {
            var textX = badgeRect.X + Math.Max(0, (badgeRect.Width - formatted.Width) / 2.0);
            var textY = badgeRect.Y + Math.Max(0, (badgeRect.Height - formatted.Height) / 2.0);
            drawingContext.DrawText(formatted, new Point(textX, textY));
        }
    }

    private FormattedText CreateFormattedText(string text, Brush foreground, string fontFamily, double fontSize, FontWeight fontWeight)
    {
        var formatted = new FormattedText(text, string.IsNullOrWhiteSpace(fontFamily) ? FrameworkElement.DefaultFontFamilyName : fontFamily, fontSize)
        {
            Foreground = foreground,
            FontWeight = fontWeight.ToOpenTypeWeight()
        };
        TextMeasurement.MeasureText(formatted);
        return formatted;
    }

    private string ResolveFallbackText()
    {
        if (!string.IsNullOrEmpty(DisplayInitials))
            return DisplayInitials;

        return DisplayKind == FWPersonPictureDisplayKind.Group ? "++" : "?";
    }

    private Brush ResolveAvatarBackground()
    {
        return Background
            ?? TryFindResource("ControlBackground") as Brush
            ?? TryFindResource("AccentBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
    }

    private Brush ResolveAvatarForeground()
    {
        return Foreground
            ?? TryFindResource("TextPrimary") as Brush
            ?? TryFindResource("TextOnAccent") as Brush
            ?? new SolidColorBrush(Colors.White);
    }

    private Brush? ResolveAvatarBorderBrush()
    {
        return BorderBrush ?? TryFindResource("ControlBorder") as Brush;
    }

    private Brush ResolveBadgeBackground()
    {
        return BadgeBackground
            ?? TryFindResource("AccentBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
    }

    private Brush ResolveBadgeForeground()
    {
        return BadgeForeground
            ?? TryFindResource("TextOnAccent") as Brush
            ?? new SolidColorBrush(Colors.White);
    }

    private Brush ResolveBadgeBorderBrush()
    {
        return BadgeBorderBrush
            ?? TryFindResource("SurfaceBackground") as Brush
            ?? new SolidColorBrush(Colors.White);
    }

    private void UpdateDisplayState()
    {
        var hasImage = ProfilePicture != null;
        var displayInitials = NormalizeInitials(Initials);
        if (string.IsNullOrEmpty(displayInitials) && !IsGroup)
            displayInitials = GenerateInitials(DisplayName);

        var displayKind = hasImage
            ? FWPersonPictureDisplayKind.Image
            : IsGroup ? FWPersonPictureDisplayKind.Group : FWPersonPictureDisplayKind.Initials;
        var badgeKind = ResolveBadgeKind();
        var badgeText = badgeKind switch
        {
            FWPersonPictureBadgeKind.Number => GetBadgeNumberText(),
            FWPersonPictureBadgeKind.Glyph => BadgeGlyph ?? string.Empty,
            _ => string.Empty
        };

        SetValue(HasProfilePicturePropertyKey.DependencyProperty, hasImage);
        SetValue(DisplayInitialsPropertyKey.DependencyProperty, displayInitials);
        SetValue(DisplayKindPropertyKey.DependencyProperty, displayKind);
        SetValue(BadgeKindPropertyKey.DependencyProperty, badgeKind);
        SetValue(HasBadgePropertyKey.DependencyProperty, badgeKind != FWPersonPictureBadgeKind.None);
        SetValue(BadgeDisplayTextPropertyKey.DependencyProperty, badgeText);
    }

    private FWPersonPictureBadgeKind ResolveBadgeKind()
    {
        if (BadgeImageSource != null)
            return FWPersonPictureBadgeKind.Image;

        if (BadgeNumber > 0)
            return FWPersonPictureBadgeKind.Number;

        return string.IsNullOrEmpty(BadgeGlyph) ? FWPersonPictureBadgeKind.None : FWPersonPictureBadgeKind.Glyph;
    }

    private string GetBadgeNumberText()
    {
        return BadgeNumber > MaximumBadgeNumber ? "99+" : BadgeNumber.ToString();
    }

    private static string GenerateInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return string.Empty;

        var parts = displayName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return string.Empty;

        if (parts.Length == 1)
        {
            return parts[0].Length >= 2
                ? parts[0].Substring(0, 2).ToUpperInvariant()
                : parts[0].ToUpperInvariant();
        }

        return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpperInvariant();
    }

    private static string NormalizeInitials(string? initials)
    {
        if (string.IsNullOrWhiteSpace(initials))
            return string.Empty;

        var compact = new string(initials.Where(character => !char.IsWhiteSpace(character)).Take(2).ToArray());
        return compact.ToUpperInvariant();
    }

    private static double ResolveDesiredLength(double length)
    {
        return double.IsNaN(length) || length <= 0 ? DefaultAvatarSize : Math.Max(MinimumAvatarSize, length);
    }

    private static bool IsValidBadgeNumber(object? value)
    {
        return value is int number && number >= 0;
    }

    private static void OnDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPersonPicture personPicture)
        {
            personPicture.UpdateDisplayState();
            personPicture.InvalidateMeasure();
            personPicture.InvalidateVisual();
        }
    }

    private static void OnBadgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPersonPicture personPicture)
        {
            personPicture.UpdateDisplayState();
            personPicture.InvalidateVisual();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWPersonPicture personPicture)
        {
            personPicture.InvalidateVisual();
        }
    }
}

public enum FWPersonPictureDisplayKind
{
    Initials,
    Group,
    Image
}

public enum FWPersonPictureBadgeKind
{
    None,
    Number,
    Glyph,
    Image
}
