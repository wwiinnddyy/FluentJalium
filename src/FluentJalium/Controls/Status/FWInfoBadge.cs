using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Interop;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium badge control for lightweight status, value, and icon indicators.
/// </summary>
public class FWInfoBadge : Control, IFluentJaliumControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(FWInfoBadge),
            new PropertyMetadata(-1, OnContentPropertyChanged), IsValidValue);

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(FWInfoBadge),
            new PropertyMetadata(99, OnContentPropertyChanged), IsValidMaxValue);

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(FWInfoBadge),
            new PropertyMetadata(null, OnContentPropertyChanged));

    public static readonly DependencyProperty IconFontFamilyProperty =
        DependencyProperty.Register(nameof(IconFontFamily), typeof(string), typeof(FWInfoBadge),
            new PropertyMetadata(FluentIconFonts.Regular, OnContentPropertyChanged));

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(FWInfoBadgeSeverity), typeof(FWInfoBadge),
            new PropertyMetadata(FWInfoBadgeSeverity.Attention, OnVisualPropertyChanged), IsValidSeverity);

    private static readonly DependencyPropertyKey DisplayKindPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayKind), typeof(FWInfoBadgeDisplayKind), typeof(FWInfoBadge),
            new PropertyMetadata(FWInfoBadgeDisplayKind.Dot));

    public static readonly DependencyProperty DisplayKindProperty = DisplayKindPropertyKey.DependencyProperty;

    public FWInfoBadge()
    {
        Focusable = false;
        IsHitTestVisible = false;
        UpdateDisplayKind();
    }

    /// <summary>
    /// Gets or sets the numeric value. Values below zero switch the badge to icon or dot mode.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public int Value
    {
        get => (int)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the highest value shown before a plus suffix is used.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty)!;
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>
    /// Gets or sets the Fluent icon glyph used when <see cref="Value"/> is less than zero.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    /// <summary>
    /// Gets or sets the font family used to render <see cref="IconGlyph"/>.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string IconFontFamily
    {
        get => (string)(GetValue(IconFontFamilyProperty) ?? FluentIconFonts.Regular);
        set => SetValue(IconFontFamilyProperty, value);
    }

    /// <summary>
    /// Gets or sets the visual severity resource set.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public FWInfoBadgeSeverity Severity
    {
        get => (FWInfoBadgeSeverity)GetValue(SeverityProperty)!;
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>
    /// Gets the display mode chosen from <see cref="Value"/> and <see cref="IconGlyph"/>.
    /// </summary>
    public FWInfoBadgeDisplayKind DisplayKind => (FWInfoBadgeDisplayKind)GetValue(DisplayKindProperty)!;

    /// <summary>
    /// Gets the text currently rendered by the badge in value mode.
    /// </summary>
    public string DisplayValueText => GetDisplayValueText();

    protected override Size MeasureOverride(Size availableSize)
    {
        return CoerceToAvailable(MeasureBadge(), availableSize);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (RenderSize.Width <= 0 || RenderSize.Height <= 0)
            return;

        var rect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
        var background = ResolveBackgroundBrush();
        var foreground = ResolveForegroundBrush();

        if (DisplayKind == FWInfoBadgeDisplayKind.Dot)
        {
            var radius = Math.Min(RenderSize.Width, RenderSize.Height) / 2.0;
            drawingContext.DrawEllipse(background, null, new Point(RenderSize.Width / 2.0, RenderSize.Height / 2.0), radius, radius);
            return;
        }

        drawingContext.DrawRoundedRectangle(background, null, rect, new CornerRadius(ResolveCornerRadius()));

        var text = CreateDisplayText();
        if (string.IsNullOrEmpty(text))
            return;

        var formatted = CreateFormattedText(text, foreground, GetDisplayFontFamily(), GetDisplayFontSize(), GetDisplayFontWeight());
        var x = Math.Max(0, (RenderSize.Width - formatted.Width) / 2.0);
        var y = Math.Max(0, (RenderSize.Height - formatted.Height) / 2.0);
        drawingContext.DrawText(formatted, new Point(x, y));
    }

    private Size MeasureBadge()
    {
        if (DisplayKind == FWInfoBadgeDisplayKind.Dot)
        {
            var dotSize = Math.Max(6.0, Math.Max(MinWidth, MinHeight));
            return new Size(dotSize, dotSize);
        }

        var text = CreateDisplayText();
        var formatted = CreateFormattedText(
            text,
            ResolveForegroundBrush(),
            GetDisplayFontFamily(),
            GetDisplayFontSize(),
            GetDisplayFontWeight());
        var padding = Padding;
        var height = Math.Max(16.0, formatted.Height + padding.Top + padding.Bottom);
        var width = formatted.Width + padding.Left + padding.Right;

        width = Math.Max(height, width);
        width = Math.Max(width, MinWidth);
        height = Math.Max(height, MinHeight);

        return new Size(width, height);
    }

    private Size CoerceToAvailable(Size desired, Size availableSize)
    {
        var width = desired.Width;
        var height = desired.Height;

        if (!double.IsInfinity(availableSize.Width))
            width = Math.Min(width, availableSize.Width);
        if (!double.IsInfinity(availableSize.Height))
            height = Math.Min(height, availableSize.Height);

        return new Size(Math.Max(0, width), Math.Max(0, height));
    }

    private FormattedText CreateFormattedText(string text, Brush foreground, string fontFamily, double fontSize, FontWeight fontWeight)
    {
        var formatted = new FormattedText(text, fontFamily, fontSize)
        {
            Foreground = foreground,
            FontWeight = fontWeight.ToOpenTypeWeight()
        };
        TextMeasurement.MeasureText(formatted);
        return formatted;
    }

    private string CreateDisplayText()
    {
        return DisplayKind switch
        {
            FWInfoBadgeDisplayKind.Value => GetDisplayValueText(),
            FWInfoBadgeDisplayKind.Icon => IconGlyph ?? string.Empty,
            _ => string.Empty
        };
    }

    private string GetDisplayValueText()
    {
        if (Value < 0)
            return string.Empty;

        var maxValue = Math.Max(0, MaxValue);
        return Value > maxValue ? $"{maxValue}+" : Value.ToString();
    }

    private string GetDisplayFontFamily()
    {
        return DisplayKind == FWInfoBadgeDisplayKind.Icon
            ? (string.IsNullOrWhiteSpace(IconFontFamily) ? FluentIconFonts.Regular : IconFontFamily)
            : FontFamily;
    }

    private double GetDisplayFontSize()
    {
        return DisplayKind == FWInfoBadgeDisplayKind.Icon
            ? Math.Max(10.0, FontSize)
            : Math.Max(10.0, FontSize);
    }

    private FontWeight GetDisplayFontWeight()
    {
        return DisplayKind == FWInfoBadgeDisplayKind.Icon ? FontWeights.Normal : FontWeight;
    }

    private double ResolveCornerRadius()
    {
        var radius = CornerRadius.TopLeft;
        if (radius <= 0)
        {
            radius = RenderSize.Height / 2.0;
        }

        return Math.Min(radius, RenderSize.Height / 2.0);
    }

    private Brush ResolveBackgroundBrush()
    {
        return Background
            ?? TryFindResource(GetBackgroundResourceKey(Severity)) as Brush
            ?? TryFindResource("InfoBadgeAttentionBackground") as Brush
            ?? TryFindResource("AccentBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
    }

    private Brush ResolveForegroundBrush()
    {
        return Foreground
            ?? TryFindResource(GetForegroundResourceKey(Severity)) as Brush
            ?? TryFindResource("InfoBadgeForeground") as Brush
            ?? TryFindResource("TextOnAccent") as Brush
            ?? new SolidColorBrush(Colors.White);
    }

    private void UpdateDisplayKind()
    {
        var kind = Value >= 0
            ? FWInfoBadgeDisplayKind.Value
            : string.IsNullOrEmpty(IconGlyph) ? FWInfoBadgeDisplayKind.Dot : FWInfoBadgeDisplayKind.Icon;

        SetValue(DisplayKindPropertyKey.DependencyProperty, kind);
    }

    private static string GetBackgroundResourceKey(FWInfoBadgeSeverity severity)
    {
        return severity switch
        {
            FWInfoBadgeSeverity.Informational => "InfoBadgeInformationalBackground",
            FWInfoBadgeSeverity.Success => "InfoBadgeSuccessBackground",
            FWInfoBadgeSeverity.Caution => "InfoBadgeCautionBackground",
            FWInfoBadgeSeverity.Critical => "InfoBadgeCriticalBackground",
            _ => "InfoBadgeAttentionBackground"
        };
    }

    private static string GetForegroundResourceKey(FWInfoBadgeSeverity severity)
    {
        return severity switch
        {
            FWInfoBadgeSeverity.Informational => "InfoBadgeInformationalForeground",
            FWInfoBadgeSeverity.Success => "InfoBadgeSuccessForeground",
            FWInfoBadgeSeverity.Caution => "InfoBadgeCautionForeground",
            FWInfoBadgeSeverity.Critical => "InfoBadgeCriticalForeground",
            _ => "InfoBadgeAttentionForeground"
        };
    }

    private static bool IsValidValue(object? value)
    {
        return value is int number && number >= -1;
    }

    private static bool IsValidMaxValue(object? value)
    {
        return value is int number && number >= 0;
    }

    private static bool IsValidSeverity(object? value)
    {
        return value is FWInfoBadgeSeverity severity && Enum.IsDefined(severity);
    }

    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWInfoBadge badge)
        {
            badge.UpdateDisplayKind();
            badge.InvalidateMeasure();
            badge.InvalidateVisual();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWInfoBadge badge)
        {
            badge.InvalidateVisual();
        }
    }
}

public enum FWInfoBadgeDisplayKind
{
    Dot,
    Icon,
    Value
}

public enum FWInfoBadgeSeverity
{
    Attention,
    Informational,
    Success,
    Caution,
    Critical
}
