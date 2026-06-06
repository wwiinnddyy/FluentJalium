using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Interop;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium rating control size presets.
/// </summary>
public enum FWRatingControlSize
{
    Small,
    Medium,
    Large
}

/// <summary>
/// FluentJalium rating control inspired by WinUI RatingControl.
/// </summary>
public class FWRatingControl : Control, IFluentJaliumControl
{
    public const double UnsetValue = -1.0;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(FWRatingControl),
            new PropertyMetadata(UnsetValue, OnValuePropertyChanged), IsValidRatingValue);

    public static readonly DependencyProperty MaxRatingProperty =
        DependencyProperty.Register(nameof(MaxRating), typeof(int), typeof(FWRatingControl),
            new PropertyMetadata(5, OnMaxRatingPropertyChanged), IsValidMaxRating);

    public static readonly DependencyProperty PlaceholderValueProperty =
        DependencyProperty.Register(nameof(PlaceholderValue), typeof(double), typeof(FWRatingControl),
            new PropertyMetadata(UnsetValue, OnVisualPropertyChanged), IsValidRatingValue);

    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(string), typeof(FWRatingControl),
            new PropertyMetadata(string.Empty, OnLayoutPropertyChanged));

    public static readonly DependencyProperty IsClearEnabledProperty =
        DependencyProperty.Register(nameof(IsClearEnabled), typeof(bool), typeof(FWRatingControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(FWRatingControl),
            new PropertyMetadata(false, OnVisualPropertyChanged));

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(FWRatingControl),
            new PropertyMetadata(8.0, OnLayoutPropertyChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty RatingItemFontSizeProperty =
        DependencyProperty.Register(nameof(RatingItemFontSize), typeof(double), typeof(FWRatingControl),
            new PropertyMetadata(20.0, OnLayoutPropertyChanged), IsValidPositiveDouble);

    public static readonly DependencyProperty RatingSizeProperty =
        DependencyProperty.Register(nameof(RatingSize), typeof(FWRatingControlSize), typeof(FWRatingControl),
            new PropertyMetadata(FWRatingControlSize.Medium, OnRatingSizePropertyChanged));

    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(FWRatingControl),
            new PropertyMetadata(FluentIconRegular.Star48.GetString(), OnLayoutPropertyChanged));

    public static readonly DependencyProperty UnsetGlyphProperty =
        DependencyProperty.Register(nameof(UnsetGlyph), typeof(string), typeof(FWRatingControl),
            new PropertyMetadata(FluentIconRegular.Star48.GetString(), OnLayoutPropertyChanged));

    public static readonly DependencyProperty GlyphFontFamilyProperty =
        DependencyProperty.Register(nameof(GlyphFontFamily), typeof(string), typeof(FWRatingControl),
            new PropertyMetadata(FluentIconFonts.Regular, OnLayoutPropertyChanged));

    private bool _isPointerOver;
    private double _pointerPreviewValue = UnsetValue;
    private bool _isApplyingValueCoercion;

    public FWRatingControl()
    {
        Focusable = true;
        ApplyRatingSize(this, RatingSize);
        AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDownHandler));
        AddHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMoveHandler));
        AddHandler(MouseLeaveEvent, new MouseEventHandler(OnMouseLeaveHandler));
        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDownHandler));
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double Value
    {
        get => (double)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public int MaxRating
    {
        get => (int)GetValue(MaxRatingProperty)!;
        set => SetValue(MaxRatingProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public double PlaceholderValue
    {
        get => (double)GetValue(PlaceholderValueProperty)!;
        set => SetValue(PlaceholderValueProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string Caption
    {
        get => (string)(GetValue(CaptionProperty) ?? string.Empty);
        set => SetValue(CaptionProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsClearEnabled
    {
        get => (bool)GetValue(IsClearEnabledProperty)!;
        set => SetValue(IsClearEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty)!;
        set => SetValue(IsReadOnlyProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty)!;
        set => SetValue(ItemSpacingProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public double RatingItemFontSize
    {
        get => (double)GetValue(RatingItemFontSizeProperty)!;
        set => SetValue(RatingItemFontSizeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWRatingControlSize RatingSize
    {
        get => (FWRatingControlSize)GetValue(RatingSizeProperty)!;
        set => SetValue(RatingSizeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string Glyph
    {
        get => (string)(GetValue(GlyphProperty) ?? FluentIconRegular.Star48.GetString());
        set => SetValue(GlyphProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string UnsetGlyph
    {
        get => (string)(GetValue(UnsetGlyphProperty) ?? FluentIconRegular.Star48.GetString());
        set => SetValue(UnsetGlyphProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public string GlyphFontFamily
    {
        get => (string)(GetValue(GlyphFontFamilyProperty) ?? FluentIconFonts.Regular);
        set => SetValue(GlyphFontFamilyProperty, value);
    }

    public event EventHandler<FWRatingControlValueChangedEventArgs>? ValueChanged;

    public void Clear()
    {
        SetRatingTo(UnsetValue, originatedFromMouse: false);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var metrics = MeasureGlyphs();
        var width = metrics.RatingWidth;
        var height = metrics.ItemHeight;

        if (!string.IsNullOrEmpty(Caption))
        {
            var captionText = CreateText(Caption, FontFamily, Math.Max(10.0, FontSize), ResolveCaptionForegroundBrush(), FontWeights.Normal);
            width += 12.0 + captionText.Width;
            height = Math.Max(height, captionText.Height);
        }

        return new Size(
            double.IsInfinity(availableSize.Width) ? width : Math.Min(width, availableSize.Width),
            double.IsInfinity(availableSize.Height) ? height : Math.Min(height, availableSize.Height));
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        var metrics = MeasureGlyphs();
        var itemTop = Math.Max(0, (RenderSize.Height - metrics.ItemHeight) / 2.0);
        var activeValue = GetActiveDisplayValue();
        var activeBrush = ResolveActiveBrush();
        var unsetBrush = ResolveUnsetBrush();
        var itemFontFamily = string.IsNullOrWhiteSpace(GlyphFontFamily) ? FluentIconFonts.Regular : GlyphFontFamily;

        for (var index = 0; index < MaxRating; index++)
        {
            var x = index * (metrics.ItemWidth + ItemSpacing);
            var filled = index + 1 <= activeValue;
            var partial = !filled && index < activeValue;
            var brush = filled || partial ? activeBrush : unsetBrush;
            var glyph = filled || partial ? Glyph : UnsetGlyph;
            var formatted = CreateText(glyph, itemFontFamily, RatingItemFontSize, brush, FontWeights.Normal);
            drawingContext.DrawText(formatted, new Point(x + ((metrics.ItemWidth - formatted.Width) / 2.0), itemTop));
        }

        if (!string.IsNullOrEmpty(Caption))
        {
            var caption = CreateText(Caption, FontFamily, Math.Max(10.0, FontSize), ResolveCaptionForegroundBrush(), FontWeights.Normal);
            var captionX = metrics.RatingWidth + 12.0;
            var captionY = Math.Max(0, (RenderSize.Height - caption.Height) / 2.0);
            drawingContext.DrawText(caption, new Point(captionX, captionY));
        }
    }

    private RatingMetrics MeasureGlyphs()
    {
        var itemFontFamily = string.IsNullOrWhiteSpace(GlyphFontFamily) ? FluentIconFonts.Regular : GlyphFontFamily;
        var set = CreateText(Glyph, itemFontFamily, RatingItemFontSize, ResolveActiveBrush(), FontWeights.Normal);
        var unset = CreateText(UnsetGlyph, itemFontFamily, RatingItemFontSize, ResolveUnsetBrush(), FontWeights.Normal);
        var itemWidth = Math.Max(RatingItemFontSize, Math.Max(set.Width, unset.Width));
        var itemHeight = Math.Max(RatingItemFontSize, Math.Max(set.Height, unset.Height));
        var ratingWidth = (MaxRating * itemWidth) + (Math.Max(0, MaxRating - 1) * ItemSpacing);
        return new RatingMetrics(itemWidth, itemHeight, ratingWidth);
    }

    private FormattedText CreateText(string text, string fontFamily, double fontSize, Brush foreground, FontWeight weight)
    {
        var formatted = new FormattedText(text, string.IsNullOrWhiteSpace(fontFamily) ? FrameworkElement.DefaultFontFamilyName : fontFamily, fontSize)
        {
            Foreground = foreground,
            FontWeight = weight.ToOpenTypeWeight()
        };
        TextMeasurement.MeasureText(formatted);
        return formatted;
    }

    private double GetActiveDisplayValue()
    {
        if (_isPointerOver && !IsReadOnly && _pointerPreviewValue >= 0)
            return _pointerPreviewValue;

        if (Value >= 0)
            return Value;

        return PlaceholderValue >= 0 ? CoerceRatingValue(PlaceholderValue) : 0.0;
    }

    private double RatingFromPosition(Point position)
    {
        var metrics = MeasureGlyphs();
        if (metrics.RatingWidth <= 0)
            return UnsetValue;

        var clampedX = Math.Clamp(position.X, 0, metrics.RatingWidth);
        var itemExtent = metrics.ItemWidth + ItemSpacing;
        var raw = Math.Ceiling(clampedX / itemExtent);

        if (clampedX > 0 && raw < 1)
            raw = 1;

        return Math.Clamp(raw, 1, MaxRating);
    }

    private void SetRatingTo(double newRating, bool originatedFromMouse)
    {
        if (!IsEnabled || IsReadOnly)
            return;

        var oldValue = Value;
        var coerced = CoerceRatingValue(newRating);

        if (!IsClearEnabled && coerced < 1)
            coerced = 1;
        else if (IsClearEnabled && originatedFromMouse && oldValue >= 0 && Math.Abs(coerced - oldValue) < double.Epsilon)
            coerced = UnsetValue;

        Value = coerced;
    }

    private double CoerceRatingValue(double value)
    {
        if (value < 0 || double.IsNaN(value))
            return UnsetValue;

        if (value <= 1.0)
            return 1.0;

        return Math.Min(value, MaxRating);
    }

    private void EnsureCoercedValues()
    {
        if (_isApplyingValueCoercion)
            return;

        try
        {
            _isApplyingValueCoercion = true;

            var coercedValue = CoerceRatingValue(Value);
            if (Math.Abs(coercedValue - Value) > double.Epsilon)
                Value = coercedValue;

            var coercedPlaceholder = CoerceRatingValue(PlaceholderValue);
            if (Math.Abs(coercedPlaceholder - PlaceholderValue) > double.Epsilon)
                PlaceholderValue = coercedPlaceholder;
        }
        finally
        {
            _isApplyingValueCoercion = false;
        }
    }

    private Brush ResolveActiveBrush()
    {
        if (!IsEnabled)
        {
            return TryFindResource("RatingControlDisabledSelectedForeground") as Brush
                ?? TryFindResource("TextDisabled") as Brush
                ?? new SolidColorBrush(Colors.Gray);
        }

        if (_isPointerOver && !IsReadOnly)
        {
            return TryFindResource("RatingControlPointerOverSelectedForeground") as Brush
                ?? TryFindResource("RatingControlSelectedForeground") as Brush
                ?? TryFindResource("AccentBrush") as Brush
                ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
        }

        if (Value < 0 && PlaceholderValue >= 0)
        {
            return TryFindResource("RatingControlPlaceholderForeground") as Brush
                ?? TryFindResource("TextPrimary") as Brush
                ?? new SolidColorBrush(Colors.White);
        }

        return TryFindResource("RatingControlSelectedForeground") as Brush
            ?? TryFindResource("AccentBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD4));
    }

    private Brush ResolveUnsetBrush()
    {
        if (!IsEnabled)
        {
            return TryFindResource("TextDisabled") as Brush
                ?? new SolidColorBrush(Colors.Gray);
        }

        if (_isPointerOver && !IsReadOnly)
        {
            return TryFindResource("RatingControlPointerOverUnselectedForeground") as Brush
                ?? TryFindResource("RatingControlUnselectedForeground") as Brush
                ?? TryFindResource("TextSecondary") as Brush
                ?? new SolidColorBrush(Colors.Gray);
        }

        return TryFindResource("RatingControlUnselectedForeground") as Brush
            ?? TryFindResource("TextSecondary") as Brush
            ?? new SolidColorBrush(Colors.Gray);
    }

    private Brush ResolveCaptionForegroundBrush()
    {
        return Foreground
            ?? TryFindResource("RatingControlCaptionForeground") as Brush
            ?? TryFindResource("TextSecondary") as Brush
            ?? new SolidColorBrush(Colors.Gray);
    }

    private void OnMouseDownHandler(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || IsReadOnly || !IsEnabled)
            return;

        Focus();
        SetRatingTo(RatingFromPosition(e.GetPosition(this)), originatedFromMouse: true);
        e.Handled = true;
    }

    private void OnMouseMoveHandler(object sender, MouseEventArgs e)
    {
        if (IsReadOnly || !IsEnabled)
            return;

        _isPointerOver = true;
        _pointerPreviewValue = RatingFromPosition(e.GetPosition(this));
        InvalidateVisual();
    }

    private void OnMouseLeaveHandler(object sender, MouseEventArgs e)
    {
        _isPointerOver = false;
        _pointerPreviewValue = UnsetValue;
        InvalidateVisual();
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (IsReadOnly || !IsEnabled)
            return;

        switch (e.Key)
        {
            case Key.Left:
            case Key.Down:
                SetRatingTo((Value < 0 ? 1 : Value) - 1, originatedFromMouse: false);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Up:
                SetRatingTo((Value < 0 ? 0 : Value) + 1, originatedFromMouse: false);
                e.Handled = true;
                break;
            case Key.Home:
                SetRatingTo(IsClearEnabled ? UnsetValue : 1, originatedFromMouse: false);
                e.Handled = true;
                break;
            case Key.End:
                SetRatingTo(MaxRating, originatedFromMouse: false);
                e.Handled = true;
                break;
            case Key.Delete:
            case Key.Back:
                if (IsClearEnabled)
                {
                    SetRatingTo(UnsetValue, originatedFromMouse: false);
                    e.Handled = true;
                }
                break;
        }
    }

    private static bool IsValidRatingValue(object? value)
    {
        return value is double number && !double.IsPositiveInfinity(number) && !double.IsNegativeInfinity(number);
    }

    private static bool IsValidMaxRating(object? value)
    {
        return value is int number && number >= 1 && number <= 10;
    }

    private static bool IsValidNonNegativeDouble(object? value)
    {
        return value is double number && number >= 0 && !double.IsInfinity(number) && !double.IsNaN(number);
    }

    private static bool IsValidPositiveDouble(object? value)
    {
        return value is double number && number > 0 && !double.IsInfinity(number) && !double.IsNaN(number);
    }

    internal static (double RatingItemFontSize, double ItemSpacing, double MinHeight) GetSizeMetrics(FWRatingControlSize size)
    {
        return size switch
        {
            FWRatingControlSize.Small => (16.0, 6.0, 20.0),
            FWRatingControlSize.Large => (24.0, 10.0, 30.0),
            _ => (20.0, 8.0, 24.0)
        };
    }

    private static void ApplyRatingSize(FWRatingControl rating, FWRatingControlSize size)
    {
        var (ratingItemFontSize, itemSpacing, minHeight) = GetSizeMetrics(size);
        rating.RatingItemFontSize = ratingItemFontSize;
        rating.ItemSpacing = itemSpacing;
        rating.MinHeight = minHeight;
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRatingControl rating)
        {
            rating.EnsureCoercedValues();
            rating.InvalidateMeasure();
            rating.InvalidateVisual();

            if (!rating._isApplyingValueCoercion)
            {
                rating.ValueChanged?.Invoke(
                    rating,
                    new FWRatingControlValueChangedEventArgs((double)e.OldValue!, rating.Value));
            }
        }
    }

    private static void OnMaxRatingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRatingControl rating)
        {
            rating.EnsureCoercedValues();
            rating.InvalidateMeasure();
            rating.InvalidateVisual();
        }
    }

    private static void OnRatingSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRatingControl rating && e.NewValue is FWRatingControlSize size)
        {
            ApplyRatingSize(rating, size);
        }
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRatingControl rating)
        {
            rating.InvalidateMeasure();
            rating.InvalidateVisual();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWRatingControl rating)
        {
            rating.EnsureCoercedValues();
            rating.InvalidateVisual();
        }
    }

    private readonly record struct RatingMetrics(double ItemWidth, double ItemHeight, double RatingWidth);
}

public sealed class FWRatingControlValueChangedEventArgs : EventArgs
{
    public FWRatingControlValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public double OldValue { get; }

    public double NewValue { get; }
}
