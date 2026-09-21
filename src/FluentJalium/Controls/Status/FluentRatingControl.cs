using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Which of upstream's visual states the row is in, published as a property so a template trigger can carry the
/// foreground swap. Upstream never exposes this: <c>RatingControl.cpp:283-370</c> walks one of six
/// <c>CommonStates</c> by hand and this runtime has no visual-state manager, so the same decision becomes a value a
/// trigger can read - the stand-in <see cref="FluentInfoBadgeDisplayKind"/> uses for four
/// <c>DisplayKindStates</c>.
/// </summary>
public enum FluentRatingControlDisplayState
{
    /// <summary>Nothing is set and no placeholder is showing: the foreground layer is cropped out entirely.</summary>
    Unset,

    /// <summary>A value is set and the pointer is not over the row.</summary>
    Set,

    /// <summary>No value, but a placeholder average is showing.</summary>
    Placeholder,

    /// <summary>A value is set and the pointer is over the row. Upstream gives this state the same brush as
    /// <see cref="Set"/> and the same glyph.</summary>
    PointerOverSet,

    /// <summary>No value, no placeholder, pointer over the row.</summary>
    PointerOverPlaceholder,

    /// <summary>No value but a placeholder exists and the pointer is over the row. Upstream names the state one
    /// thing and asks for the other's glyph, and that asymmetry is kept (<c>RatingControl.cpp:303-308</c>).</summary>
    PointerOverUnselected,

    /// <summary>The row is disabled. Upstream applies this last and over the top of the hover branch, so a hovered
    /// disabled row keeps the hover crop and loses the hover colour.</summary>
    Disabled,
}

/// <summary>
/// A star rating: a hollow background row, a clipped foreground row on top of it, and an optional caption -
/// WinUI's <c>RatingControl</c> shape.
/// </summary>
/// <remarks>
/// <para>
/// Own type because 26.10.9 exports no <c>RatingControl</c> and none of the three <c>RatingItem*</c> types
/// (<c>spike/RatingProbe</c>, asked of the assembly that really holds <see cref="Button"/>:
/// <c>Jalium.UI.Managed</c>, 2 958 public types).
/// </para>
/// <para>
/// The half star is the one piece that cannot be done upstream's way, and the replacement is measured rather than
/// guessed: <c>UIElement.Clip</c> is a real property that accepts a real <see cref="RectangleGeometry"/> and then
/// does nothing at all - a 60x60 block of one colour clipped to 30x60 still read 3 600 pixels of that colour, from
/// code as well as from markup, where the markup form read back an <c>Empty</c> rect on top of it.
/// <c>ClipToBounds</c> on a host does cut, and proportionally: the same block behind a 30-wide host read 1 800,
/// behind a 45-wide one 2 700, and a markup-written 20-wide host over 32 DIP of ink read exactly 320. So every
/// foreground item is a clip host instead of a clipped element.
/// </para>
/// <para>
/// Upstream's item pitch contradicts itself - the template's <c>-8</c> margins cancel the spacing for layout while
/// the width and pointer maths keep using <c>size + spacing</c> - and the sanctioned adaptation resolves it by
/// measuring the star run and handing the panel the difference for spacing. That method is taken here, which makes
/// the laid-out pitch and the pointer bands one number; the <c>-8</c> margins are not copied because the
/// double-size rendering they compensate for is not a thing this runtime does. The whole ledger, including what is
/// deliberately dropped, is in <c>docs/astra/audits/rating-control.md</c>.
/// </para>
/// </remarks>
public class FluentRatingControl : Control
{
    /// <summary>The size upstream renders a star at: <c>RatingControlFontSizeForRendering</c>, 32. That row is an
    /// <c>x:Double</c>, which this reader cannot publish, so the number lives here and the template writes the same
    /// literal.</summary>
    internal const double RenderedItemSize = 32;

    /// <summary>The size the width and pointer maths count in: upstream's <c>ActualRatingFontSize</c>, half the
    /// rendering size (<c>RatingControl.cpp:51-55</c>) - and the rendering size is not 32. Upstream overwrites
    /// <c>m_scaledFontSizeForRendering</c> with the measured <c>DesiredSize.Width</c> of one star run at 32
    /// (<c>RatingControl.cpp:196</c>), so what the maths use is half of whatever the font actually gives: 17 with the
    /// Segoe star, which measures 34 wide here (spike/RatingProbe), not the 16 the configured pair implies.</summary>
    internal double ActualItemSize => _itemAdvance / 2;

    /// <summary><c>RatingControlItemSpacing</c>, 8 - another row this reader cannot publish.</summary>
    internal const double ItemSpacing = 8;

    /// <summary>The floor of upstream's item magnifier: <c>ApplyScaleExpressionAnimation</c>
    /// (<c>RatingControl.cpp:372-392</c>) drives each item's composition scale off
    /// <c>max(pointerScalar - 0.0005 * pointerScalar * (starCenter - focal)², 0.5)</c>, and with the focal point
    /// parked at <see cref="NoFocalPoint"/> the quadratic is astronomically negative, so every item rests at 0.5.
    /// That resting scale is the "default scale down" upstream's template comments name: the star is rendered at 32
    /// and shown at half, which is why the model counts <see cref="ActualItemSize"/> and not 32. This runtime has no
    /// composition visual to hand an expression animation to, so the same expression is evaluated per state
    /// change.</summary>
    internal const double RestItemScale = 0.5;

    /// <summary><c>c_mouseOverScale</c> / <c>c_touchOverScale</c> (<c>RatingControl.cpp:19-20</c>): the ceiling the
    /// item under the pointer rises to, and how far, for the two pointer kinds.</summary>
    internal const double MouseItemScale = 0.8;

    /// <summary>The touch ceiling (the same two lines): a finger magnifies the star all the way to its drawn size.</summary>
    internal const double TouchItemScale = 1.0;

    /// <summary><c>c_noPointerOverMagicNumber</c>: where upstream parks the focal point when nothing is over the row.</summary>
    internal const double NoFocalPoint = -99999;

    /// <summary><c>c_captionSpacing</c>: the gap upstream wants between the last star and the caption text.</summary>
    internal const double CaptionSpacing = 12;

    /// <summary>The sentinel upstream calls "no value set".</summary>
    internal const double UnsetValue = -1;

    private const string DefaultStyleResourceKey = "DefaultRatingControlStyle";
    private const string BackgroundPartName = "RatingBackgroundStackPanel";
    private const string ForegroundPartName = "RatingForegroundStackPanel";
    private const string ForegroundPresenterPartName = "ForegroundContentPresenter";
    private const string CaptionPartName = "Caption";

    private Style? _appliedDefaultStyle;
    private StackPanel? _backgroundPanel;
    private StackPanel? _foregroundPanel;
    private ContentControl? _foregroundPresenter;
    private TextBlock? _captionText;
    private double _itemAdvance = RenderedItemSize;
    private double _firstItemOffset;
    private double _pointerPercentage;
    private double _focalX = NoFocalPoint;
    private bool _pointerIsTouch;
    private bool _isPointerOver;
    private bool _isPointerDown;
    private bool _hasMouseCapture;
    private TouchDevice? _touch;
    private bool _restamping;

    /// <summary>Creates a rating control and applies its named style when one is available.</summary>
    public FluentRatingControl()
    {
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();

        PreviewMouseLeftButtonDown += OnPointerPressed;
        PreviewMouseLeftButtonUp += OnPointerReleased;
        LostMouseCapture += OnCaptureLost;

        PreviewTouchDown += OnTouchPressed;
        PreviewTouchMove += OnTouchMoved;
        PreviewTouchUp += OnTouchReleased;

        PreviewKeyDown += OnKeyDownHandler;
    }

    /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(FluentRatingControl), new PropertyMetadata(UnsetValue, OnValueChanged));

    /// <summary>Identifies the <see cref="PlaceholderValue"/> dependency property.</summary>
    public static readonly DependencyProperty PlaceholderValueProperty = DependencyProperty.Register(
        nameof(PlaceholderValue), typeof(double), typeof(FluentRatingControl), new PropertyMetadata(UnsetValue, OnPlaceholderValueChanged));

    /// <summary>Identifies the <see cref="MaxRating"/> dependency property.</summary>
    public static readonly DependencyProperty MaxRatingProperty = DependencyProperty.Register(
        nameof(MaxRating), typeof(int), typeof(FluentRatingControl), new PropertyMetadata(5, OnMaxRatingChanged));

    /// <summary>Identifies the <see cref="Caption"/> dependency property.</summary>
    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        nameof(Caption), typeof(string), typeof(FluentRatingControl), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="ItemInfo"/> dependency property.</summary>
    public static readonly DependencyProperty ItemInfoProperty = DependencyProperty.Register(
        nameof(ItemInfo), typeof(FluentRatingItemInfo), typeof(FluentRatingControl), new PropertyMetadata(null, OnItemInfoChanged));

    /// <summary>Identifies the <see cref="IsClearEnabled"/> dependency property.</summary>
    public static readonly DependencyProperty IsClearEnabledProperty = DependencyProperty.Register(
        nameof(IsClearEnabled), typeof(bool), typeof(FluentRatingControl), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="IsReadOnly"/> dependency property.</summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(FluentRatingControl), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="InitialSetValue"/> dependency property.</summary>
    public static readonly DependencyProperty InitialSetValueProperty = DependencyProperty.Register(
        nameof(InitialSetValue), typeof(double), typeof(FluentRatingControl), new PropertyMetadata(1.0));

    /// <summary>Identifies the <see cref="ActiveRatingState"/> dependency property.</summary>
    public static readonly DependencyPropertyKey ActiveRatingStatePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ActiveRatingState), typeof(FluentRatingControlDisplayState), typeof(FluentRatingControl),
        new PropertyMetadata(FluentRatingControlDisplayState.Unset));

    /// <summary>Identifies the <see cref="ActiveRatingState"/> dependency property.</summary>
    public static readonly DependencyProperty ActiveRatingStateProperty = ActiveRatingStatePropertyKey.DependencyProperty;

    /// <summary>The star drawn when <see cref="Value"/> is set; -1 means nothing is rated.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, value);
    }

    /// <summary>An average to show until the user rates, in the same unit as <see cref="Value"/>; -1 turns it off.
    /// Fractions below 1 are kept here, unlike on <see cref="Value"/>.</summary>
    public double PlaceholderValue
    {
        get => (double)GetValue(PlaceholderValueProperty)!;
        set => SetValue(PlaceholderValueProperty, value);
    }

    /// <summary>How many stars the row holds. Clamped to at least 1, and a change pulls <see cref="Value"/> and
    /// <see cref="PlaceholderValue"/> down with it.</summary>
    public int MaxRating
    {
        get => (int)GetValue(MaxRatingProperty)!;
        set => SetValue(MaxRatingProperty, value);
    }

    /// <summary>Text drawn after the stars; upstream keeps it inside the control's own width.</summary>
    public string Caption
    {
        get => (string)GetValue(CaptionProperty)!;
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>The glyphs or images the row draws. Null leaves the row empty instead of failing fast the way
    /// upstream does (<c>RatingControl.cpp:431</c>); see audit clause 3.</summary>
    public FluentRatingItemInfo? ItemInfo
    {
        get => (FluentRatingItemInfo?)GetValue(ItemInfoProperty);
        set => SetValue(ItemInfoProperty, value);
    }

    /// <summary>Whether clicking the current star, or dragging off the left edge, clears the rating.</summary>
    public bool IsClearEnabled
    {
        get => (bool)GetValue(IsClearEnabledProperty)!;
        set => SetValue(IsClearEnabledProperty, value);
    }

    /// <summary>Whether pointer and keyboard input are refused. Reading still shows the value.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty)!;
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>The value a step from "unset" lands on - upstream's <c>InitialSetValue</c>.</summary>
    public double InitialSetValue
    {
        get => (double)GetValue(InitialSetValueProperty)!;
        set => SetValue(InitialSetValueProperty, value);
    }

    /// <summary>The state the template's triggers read. Written by this control, never by an app.</summary>
    public FluentRatingControlDisplayState ActiveRatingState =>
        (FluentRatingControlDisplayState)GetValue(ActiveRatingStateProperty)!;

    /// <summary>Raised whenever a commit runs, including one that leaves the value alone: upstream raises it inside
    /// <c>SetRatingTo</c> without a delta guard (<c>RatingControl.cpp:616</c>).</summary>
    public event EventHandler? ValueChanged;

    /// <summary>Gets the foreground item at <paramref name="index"/> - the clip host - or <c>null</c> before the
    /// template has run. This is how a test or an app reads the half-star crop without walking the visual tree.</summary>
    public FrameworkElement? ForegroundItem(int index) => ItemAt(_foregroundPanel, index);

    /// <summary>Gets the background item at <paramref name="index"/>, or <c>null</c> before the template has run.</summary>
    public FrameworkElement? BackgroundItem(int index) => ItemAt(_backgroundPanel, index);

    /// <summary>Upstream's <c>m_firstItemOffset</c> (<c>cpp:866</c>): where the first cell really sits inside the
    /// panel, read after layout. The move path subtracts it and the release path does not (<c>cpp:846</c>,
    /// <c>cpp:925</c>) - an asymmetry this layout happens to make invisible, because the cells start at the origin.</summary>
    internal double FirstItemOffset => _firstItemOffset;

    /// <summary>The width the pointer maths divide by: <c>MaxRating * size + (MaxRating - 1) * spacing</c> -
    /// upstream's <c>CalculateActualRatingWidth</c> kept number-for-number.</summary>
    internal double ActualRatingWidth => (MaxRating * ActualItemSize) + ((MaxRating - 1) * ItemSpacing);

    /// <summary>How far the ink sits inside the run's layout box. A 32 star run measures 34 wide and prints
    /// <see cref="ActualItemSize"/> of it, so the run is pulled left by half the difference and each cell's host is
    /// sized to the ink. Two things follow: the host's width is a linear fraction of the visible star (a quarter crop
    /// cuts a quarter of the ink, which a 34-wide box would not do until the crop passed the ink's left edge), and the
    /// cell pitch is the ink box plus the published spacing - <c>17 + 8 = 25</c> - which is exactly the number the
    /// width model divides by, so no compensation is needed at all.
    /// Upstream does need one, because its cells are the full run box: it writes
    /// <c>Spacing = ItemSpacing - (advance - actual)</c> (<c>RatingControl.cpp:249-268</c>), a negative number here,
    /// and a negative <c>StackPanel.Spacing</c> keeps the value you set while arranging it as zero
    /// (spike/RatingProbe: <c>Spacing=-10</c> read back -10 with three 16-wide cells still a pitch of 16 apart,
    /// desired 48 not 28). Sizing the cell to the ink instead of copying that line is what keeps this row honest
    /// without reaching for a mechanism the runtime drops.</summary>
    internal double ItemInset => (_itemAdvance - ActualItemSize) / 2;

    /// <summary>The clip width the item at <paramref name="index"/> gets for a shown rating of
    /// <paramref name="rating"/>: the whole star, the fraction of it, or nothing
    /// (<c>RatingControl.cpp:336-366</c>). Upstream sizes that rect off <c>RenderingRatingFontSize()</c>, the measured
    /// advance it stored at <c>cpp:196</c>, because its item is the full run box; ours is the ink box, so the same
    /// fractions are taken of <see cref="ActualItemSize"/> - see <see cref="ItemInset"/>.</summary>
    internal double ClipWidthFor(int index, double rating)
    {
        if (index + 1 > rating)
        {
            return index < rating ? ActualItemSize * (rating - Math.Floor(rating)) : 0;
        }

        return ActualItemSize;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _backgroundPanel = GetTemplateChild(BackgroundPartName) as StackPanel;
        _foregroundPanel = GetTemplateChild(ForegroundPartName) as StackPanel;
        _foregroundPresenter = GetTemplateChild(ForegroundPresenterPartName) as ContentControl;
        _captionText = GetTemplateChild(CaptionPartName) as TextBlock;

        if (_backgroundPanel is { } panel)
        {
            // Input lives on the background layer, as it does upstream: the foreground layer and its host are not
            // hit-test visible, so the stars never eat the pointer from the row that measures them.
            panel.MouseEnter += OnBackgroundMouseEnter;
            panel.MouseLeave += OnBackgroundMouseLeave;
            panel.MouseMove += OnBackgroundMouseMove;
        }

        StampOutRatingItems();
    }

    /// <inheritdoc />
    protected override void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        base.OnIsEnabledChanged(oldValue, newValue);
        if (!newValue)
        {
            CancelPointer();
        }

        UpdateRatingItemsAppearance();
    }

    /// <summary>The glyph for one role, or the empty string when the row is drawn from images or has no item
    /// info at all.</summary>
    internal string GetGlyph(FluentRatingControlDisplayState role) =>
        ItemInfo is FluentRatingItemFontInfo font ? font.GlyphFor(role) : string.Empty;

    private static FrameworkElement? ItemAt(StackPanel? panel, int index) =>
        panel is not null && index >= 0 && index < panel.Children.Count ? panel.Children[index] as FrameworkElement : null;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FluentRatingControl)d;
        var value = (double)e.NewValue!;
        var coerced = control.CoerceValue(value);
        if (coerced != value)
        {
            control.SetValue(ValueProperty, coerced);
            return;
        }

        control.UpdateRatingItemsAppearance();
    }

    private static void OnPlaceholderValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FluentRatingControl)d;
        var value = (double)e.NewValue!;
        var coerced = control.CoercePlaceholderValue(value);
        if (coerced != value)
        {
            control.SetValue(PlaceholderValueProperty, coerced);
            return;
        }

        control.UpdateRatingItemsAppearance();
    }

    private static void OnMaxRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (FluentRatingControl)d;
        var rating = (int)e.NewValue!;
        var coerced = Math.Max(1, rating);
        if (coerced != rating)
        {
            // Commit the corrected maximum before anything reads it, which is what keeps a transient invalid
            // MaxRating out of layout (RatingControl.cpp:630-649, a regression a pinned upstream test guards).
            control.SetValue(MaxRatingProperty, coerced);
            return;
        }

        if (control.Value > coerced)
        {
            control.Value = coerced;
        }

        if (control.PlaceholderValue > coerced)
        {
            control.PlaceholderValue = coerced;
        }

        control.StampOutRatingItems();
    }

    private static void OnItemInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((FluentRatingControl)d).StampOutRatingItems();

    private double CoerceValue(double value)
    {
        // Negative is the sentinel, anything at or below one star is one star, and the top is the maximum:
        // upstream's CoerceValueBetweenMinAndMax (RatingControl.cpp:122-138) - it never throws.
        if (value < 0)
        {
            return UnsetValue;
        }

        if (value <= 1)
        {
            return 1;
        }

        return value > MaxRating ? MaxRating : value;
    }

    private double CoercePlaceholderValue(double value)
    {
        // MaxRating can be mid-coercion when this runs, so clamp against a maximum of at least 1. Fractions below
        // one survive: a placeholder is a display average, not a rating (RatingControl.cpp:140-160).
        var effectiveMax = Math.Max(1, MaxRating);
        if (value < 0)
        {
            return UnsetValue;
        }

        return value > effectiveMax ? effectiveMax : value;
    }

    private void ApplyDefaultStyle()
    {
        // Same constructor fallback the other own types use: a host that attaches the Astra dictionaries after
        // building its window has nothing to find yet, and an app-assigned style - including an explicit null - is
        // never replaced.
        if ((Style is not null || _appliedDefaultStyle is not null) && !ReferenceEquals(Style, _appliedDefaultStyle))
        {
            return;
        }

        if (_appliedDefaultStyle is null && !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
        {
            return;
        }

        if (TryFindResource(DefaultStyleResourceKey) is not Style style)
        {
            return;
        }

        _appliedDefaultStyle = style;
        SetCurrentValue(StyleProperty, style);
    }

    private void StampOutRatingItems()
    {
        if (_restamping || _backgroundPanel is null || _foregroundPanel is null)
        {
            // A property callback fired before the template exists; upstream returns for the same reason.
            return;
        }

        _restamping = true;
        try
        {
            _itemAdvance = MeasureItemAdvance();
            _backgroundPanel.Children.Clear();
            _foregroundPanel.Children.Clear();

            for (var index = 0; index < MaxRating; index++)
            {
                _backgroundPanel.Children.Add(BuildItem(FluentRatingControlDisplayState.Unset, clipped: false));
                _foregroundPanel.Children.Add(BuildItem(FluentRatingControlDisplayState.Set, clipped: true));
            }

            // The published gap is the only horizontal number on the panels: each cell is as wide as its own ink, so
            // advance + spacing is already the pitch the model divides by (see ItemInset).
            _backgroundPanel.Spacing = ItemSpacing;
            _foregroundPanel.Spacing = ItemSpacing;
            RememberFirstItemOffset();

            if (_captionText is { } caption)
            {
                var margin = caption.Margin;

                // Upstream subtracts the same defaultItemSpacing it took off the panels, because its last cell
                // overhangs the row by that much. Ours is sized to its own ink, so the panel ends where the last star
                // ends and the caption only needs the gap upstream asks for (cpp:253-259, c_captionSpacing).
                margin.Left = CaptionSpacing;
                caption.Margin = margin;
            }
        }
        finally
        {
            _restamping = false;
        }

        UpdateRatingItemsAppearance();
    }

    /// <summary>Upstream's <c>m_scaledFontSizeForRendering</c> (<c>RatingControl.cpp:190-203</c>): the width one star
    /// run really takes at the rendering size, measured on a bare run of the control's own font - not on a template
    /// item, which carries the inset margin this number is later used to compute. The image path takes the configured
    /// size, exactly as <c>cpp:202</c> does, because an image has no glyph to measure. The configured size is also the
    /// fallback, because a run that has not been through layout yet has no desired size to read and the control does
    /// not own a measure pass.</summary>
    private double MeasureItemAdvance()
    {
        if (ItemInfo is FluentRatingItemImageInfo)
        {
            return RenderedItemSize;
        }

        var probe = new TextBlock
        {
            FontFamily = FontFamily,
            FontSize = RenderedItemSize,
            Text = GetGlyph(FluentRatingControlDisplayState.Set),
        };
        probe.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var advance = probe.DesiredSize.Width;
        return double.IsFinite(advance) && advance > 0 ? advance : RenderedItemSize;
    }

    /// <summary>One cell: the run inside a host the width of its ink. The host is what the panel lays out and what
    /// gets cropped; the run inside is drawn at half and pulled left so the ink starts on the host's left edge.</summary>
    private FrameworkElement BuildItem(FluentRatingControlDisplayState role, bool clipped)
    {
        var item = CreateItemContent(role, useStateBrush: clipped);

        // Upstream clips the item itself; Clip does nothing on this runtime (see the type remarks), so the crop is a
        // host around it. Both layers get one, which is also what makes the two rows register cell for cell: the same
        // box, the same inset, the same published spacing on the panel.
        var host = new Grid
        {
            ClipToBounds = clipped,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = ActualItemSize,
        };
        host.Children.Add(item);
        return host;
    }

    private FrameworkElement CreateItemContent(FluentRatingControlDisplayState role, bool useStateBrush)
    {
        var isImage = ItemInfo is FluentRatingItemImageInfo;
        var templateKey = (isImage, role) switch
        {
            (true, FluentRatingControlDisplayState.Unset) => "BackgroundImageDefaultTemplate",
            (true, _) => "ForegroundImageDefaultTemplate",
            (false, FluentRatingControlDisplayState.Unset) => "BackgroundGlyphDefaultTemplate",
            (false, _) => "ForegroundGlyphDefaultTemplate",
        };

        // Upstream looks these four up with no null check and casts the result (RatingControl.cpp:395-410), so a
        // missing or mismatched template is a crash or silence. Guarded here: a host without the resource
        // dictionary still gets a row.
        var content = TryFindResource(templateKey) is DataTemplate template ? template.LoadContent() : null;
        content ??= isImage ? new Image() : new TextBlock();

        CustomizeItem(content, role, useStateBrush);
        return content;
    }

    private void CustomizeItem(FrameworkElement content, FluentRatingControlDisplayState role, bool useStateBrush)
    {
        // The double-size rendering and its scale-down, both of which upstream states in the comments on the two
        // glyph rows (RatingControl_themeresources.xaml:38-46: "32 = 2 * [default fontsize]", "-8, -8 are to
        // compensate for the default scale down"). Here the scale is the resting value of upstream's magnifier and
        // the inset is half the leftover, which puts the ink flush with the crop host's left edge. The transform is
        // created once and rewritten in place by Magnify.
        content.RenderTransformOrigin = new Point(0.5, 0.5);
        content.RenderTransform ??= new ScaleTransform(RestItemScale, RestItemScale);
        content.Margin = new Thickness(-ItemInset, 0, 0, 0);

        if (content is TextBlock text)
        {
            text.FontFamily = FontFamily;
            text.FontSize = RenderedItemSize;
            text.Text = GetGlyph(role);
            if (useStateBrush && StateBrush is { } brush)
            {
                // Upstream lets the foreground run inherit the state brush off its ContentPresenter. Nothing
                // inherits a Foreground here (adaptation/00 S1-r clause 4), and a run loaded from a DataTemplate is
                // outside the template's name scope anyway, so the brush is handed over explicitly.
                text.Foreground = brush;
            }

            return;
        }

        if (content is Image image && ItemInfo is FluentRatingItemImageInfo pictures)
        {
            image.Source = pictures.SourceFor(role);
            image.Width = RenderedItemSize;
            image.Height = RenderedItemSize;
        }
    }

    /// <summary>The brush the six state triggers write: the part upstream names
    /// <c>ForegroundContentPresenter</c>, or this control's own foreground before a template has been applied. The
    /// part is a <see cref="ContentControl">ContentControl</see> rather than upstream's ContentPresenter because
    /// 26.10.9's <c>ContentPresenter</c> declares no <c>Foreground</c> at all - writing one onto it is the dead
    /// setter class #31 and #33 already catalogued.</summary>
    private Brush? StateBrush => _foregroundPresenter?.Foreground ?? Foreground;

    /// <summary>The one exit that decides what the row looks like, in upstream's order: hover first, then value,
    /// then placeholder, then the disabled pass over the top (<c>RatingControl.cpp:283-370</c>).</summary>
    private void UpdateRatingItemsAppearance()
    {
        if (_foregroundPanel is null)
        {
            return;
        }

        var value = Value;
        var placeholder = PlaceholderValue;
        var shown = 0d;
        var state = FluentRatingControlDisplayState.Unset;

        if (_isPointerOver && !IsReadOnly)
        {
            shown = Math.Ceiling(_pointerPercentage * MaxRating);

            if (value == UnsetValue)
            {
                // Upstream's locked API (RatingControl.cpp:303-308): the state is named PointerOverUnselected but
                // the glyph role asked for is still PointerOverPlaceholder.
                state = placeholder == UnsetValue
                    ? FluentRatingControlDisplayState.PointerOverPlaceholder
                    : FluentRatingControlDisplayState.PointerOverUnselected;
            }
            else
            {
                state = FluentRatingControlDisplayState.PointerOverSet;
            }
        }
        else if (value > UnsetValue)
        {
            shown = value;
            state = FluentRatingControlDisplayState.Set;
        }
        else if (placeholder > UnsetValue)
        {
            shown = placeholder;
            state = FluentRatingControlDisplayState.Placeholder;
        }

        if (!IsEnabled)
        {
            state = FluentRatingControlDisplayState.Disabled;
        }

        // "Unset" has no glyph of its own upstream, so the foreground layer keeps asking for the settled one while
        // it is cropped to nothing; PointerOverUnselected is the state whose name has no member behind it.
        var role = state switch
        {
            FluentRatingControlDisplayState.PointerOverUnselected => FluentRatingControlDisplayState.PointerOverPlaceholder,
            FluentRatingControlDisplayState.Unset => FluentRatingControlDisplayState.Set,
            _ => state,
        };

        // The state is published before the items are dressed, because the brush each foreground run wears is read
        // back off the part the state's trigger writes: the new state has to be on the tree before it can be read
        // off it.
        SetValue(ActiveRatingStatePropertyKey, state);

        var index = 0;
        var backgroundPanel = _backgroundPanel;
        foreach (var child in _foregroundPanel.Children)
        {
            var width = ClipWidthFor(index, shown);
            if (child is Grid host)
            {
                host.Width = width;
                if (host.Children.Count > 0 && host.Children[0] is FrameworkElement inner)
                {
                    CustomizeItem(inner, role, useStateBrush: true);
                }
            }
            else if (child is FrameworkElement element)
            {
                CustomizeItem(element, role, useStateBrush: true);
            }

            // The magnifier runs on both layers, or the star and the outline behind it would not grow together.
            Magnify(child, index);
            if (backgroundPanel is not null && index < backgroundPanel.Children.Count)
            {
                Magnify(backgroundPanel.Children[index], index);
            }

            index++;
        }
    }

    /// <summary>Upstream's <c>ApplyScaleExpressionAnimation</c> (<c>RatingControl.cpp:372-392</c>) in code: the item
    /// under the pointer lifts toward its pointer-kind ceiling and the rest fall off quadratically to the 0.5 floor.
    /// The focal point is the raw pointer X the move handler saw, or the magic number when nothing is over the row.</summary>
    private void Magnify(object? cell, int index)
    {
        if (cell is not Grid host || host.Children.Count == 0 ||
            host.Children[0].RenderTransform is not ScaleTransform scale)
        {
            return;
        }

        var scalar = _pointerIsTouch ? TouchItemScale : MouseItemScale;
        var focal = _isPointerOver && !IsReadOnly ? _focalX : NoFocalPoint;
        var delta = StarCentre(index) - focal;
        var value = Math.Max(scalar - (0.0005 * scalar * delta * delta), RestItemScale);
        scale.ScaleX = value;
        scale.ScaleY = value;
    }

    /// <summary>Upstream's <c>GetActualRatingItemCenter</c> (<c>RatingControl.cpp:961</c>): the centre of the item at
    /// <paramref name="index"/>, in the panel's own coordinates.</summary>
    private double StarCentre(int index) => (index * (ActualItemSize + ItemSpacing)) + (ActualItemSize / 2);

    private void OnBackgroundMouseEnter(object sender, MouseEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

        _isPointerOver = true;
        RememberFirstItemOffset();
        UpdateRatingItemsAppearance();
    }

    private void OnBackgroundMouseLeave(object sender, MouseEventArgs e)
    {
        // While the button is held the pointer is captured, and leaving the row is exactly the drag-off-the-left
        // gesture that has to stay live, so the hover state waits for the release.
        if (_isPointerDown)
        {
            return;
        }

        _isPointerOver = false;
        UpdateRatingItemsAppearance();
    }

    private void OnBackgroundMouseMove(object sender, MouseEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

        _isPointerOver = true;
        PreviewAt(e.GetPosition(_backgroundPanel).X, subtractFirstItemOffset: true);
    }

    private void OnPointerPressed(object sender, MouseButtonEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }

        _isPointerDown = true;
        _isPointerOver = true;
        RememberFirstItemOffset();

        // Capture on the way down so a drag off the left edge can still clear the value - the reason upstream
        // captures.
        if (_backgroundPanel?.CaptureMouse() == true)
        {
            _hasMouseCapture = true;
        }

        Focus();
        PreviewAt(e.GetPosition(_backgroundPanel).X, subtractFirstItemOffset: false);
        e.Handled = true;
    }

    private void OnPointerReleased(object sender, MouseButtonEventArgs e)
    {
        if (!IsReadOnly)
        {
            // Upstream divides the release position by the whole width without the first-item offset it uses while
            // moving; that asymmetry is what makes a drag off the left edge land on "clear".
            SetRatingTo(StarsAt(e.GetPosition(_backgroundPanel).X, subtractFirstItemOffset: false), fromPointer: true);
        }

        _isPointerDown = false;
        if (_hasMouseCapture)
        {
            _backgroundPanel?.ReleaseMouseCapture();
            _hasMouseCapture = false;
        }

        UpdateRatingItemsAppearance();
        e.Handled = true;
    }

    private void OnTouchPressed(object sender, TouchEventArgs e)
    {
        if (IsReadOnly || !CaptureTouch(e.TouchDevice))
        {
            return;
        }

        _touch = e.TouchDevice;
        _isPointerDown = true;
        _isPointerOver = true;
        RememberFirstItemOffset();
        Focus();
        PreviewAt(e.GetTouchPoint(_backgroundPanel).Position.X, subtractFirstItemOffset: false, touch: true);
        e.Handled = true;
    }

    private void OnTouchMoved(object sender, TouchEventArgs e)
    {
        if (IsReadOnly || _touch?.Id != e.TouchDevice.Id)
        {
            return;
        }

        _isPointerOver = true;
        PreviewAt(e.GetTouchPoint(_backgroundPanel).Position.X, subtractFirstItemOffset: true, touch: true);
        e.Handled = true;
    }

    private void OnTouchReleased(object sender, TouchEventArgs e)
    {
        if (_touch?.Id != e.TouchDevice.Id)
        {
            return;
        }

        if (!IsReadOnly)
        {
            // The same asymmetry as the mouse release: no first-item offset here, which is what lets a finger drag
            // off the left edge reach the clear branch.
            SetRatingTo(StarsAt(e.GetTouchPoint(_backgroundPanel).Position.X, subtractFirstItemOffset: false), fromPointer: true);
        }

        ReleaseTouchCapture(e.TouchDevice);
        _touch = null;
        _isPointerDown = false;
        _isPointerOver = false;
        UpdateRatingItemsAppearance();
        e.Handled = true;
    }

    private void OnCaptureLost(object sender, MouseEventArgs e)
    {
        if (_isPointerDown)
        {
            CancelPointer();
        }
    }

    private void CancelPointer()
    {
        _isPointerDown = false;
        _isPointerOver = false;
        _hasMouseCapture = false;
        UpdateRatingItemsAppearance();
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
        if (TryHandleKey(e.Key))
        {
            e.Handled = true;
        }
    }

    /// <summary>The key table, split out of the routed handler so the arithmetic can be driven without a synthetic
    /// keyboard event - this suite has no synthesised-pointer or key path yet (#13), so the wiring itself stays
    /// unproven while the model it reaches is tested. Returns whether the key belongs to this control.</summary>
    internal bool TryHandleKey(Key key)
    {
        if (IsReadOnly)
        {
            return false;
        }

        var reverser = FlowDirection == FlowDirection.RightToLeft ? -1.0 : 1.0;

        // Up and down mean right and left, and never reverse: upstream's own comment calls them keyboard-only
        // (RatingControl.cpp:1000-1010).
        switch (key)
        {
            case Key.Left:
                ChangeRatingBy(-1.0 * reverser);
                return true;
            case Key.Right:
                ChangeRatingBy(1.0 * reverser);
                return true;
            case Key.Up:
                ChangeRatingBy(1.0);
                return true;
            case Key.Down:
                ChangeRatingBy(-1.0);
                return true;
            case Key.Home:
                SetRatingTo(0, fromPointer: false);
                return true;
            case Key.End:
                SetRatingTo(MaxRating, fromPointer: false);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The hover preview the move handlers compute, split out for the same reason as
    /// <see cref="TryHandleKey"/>: <paramref name="x"/> is a position inside the background layer.</summary>
    internal void PreviewPointerAt(double x, bool touch = false)
    {
        if (IsReadOnly)
        {
            return;
        }

        _isPointerOver = true;
        RememberFirstItemOffset();
        PreviewAt(x, subtractFirstItemOffset: true, touch);
    }

    /// <summary>The pointer leaving the row: upstream's <c>PointerExitedImpl</c> parks the focal point back at the
    /// magic number, which is what drops every item onto the resting scale. Split out for the same reason as
    /// <see cref="PreviewPointerAt"/>.</summary>
    internal void LeavePointer()
    {
        _isPointerOver = false;
        _focalX = NoFocalPoint;
        UpdateRatingItemsAppearance();
    }

    /// <summary>The commit the release handlers perform, split out for the same reason as
    /// <see cref="TryHandleKey"/>.</summary>
    internal void CommitPointerAt(double x)
    {
        if (IsReadOnly)
        {
            return;
        }

        SetRatingTo(StarsAt(x, subtractFirstItemOffset: false), fromPointer: true);
        _isPointerDown = false;
        UpdateRatingItemsAppearance();
    }

    private void RememberFirstItemOffset()
    {
        if (_backgroundPanel is { Children.Count: > 0 } panel && panel.Children[0] is UIElement first)
        {
            _firstItemOffset = first.TranslatePoint(new Point(0, 0), panel).X;
        }
    }

    private double StarsAt(double x, bool subtractFirstItemOffset)
    {
        var width = ActualRatingWidth;
        if (width <= 0)
        {
            return 0;
        }

        var numerator = subtractFirstItemOffset ? x - _firstItemOffset : x;
        return Math.Ceiling((numerator / width) * MaxRating);
    }

    private void PreviewAt(double x, bool subtractFirstItemOffset, bool touch = false)
    {
        var width = ActualRatingWidth;
        var numerator = subtractFirstItemOffset ? x - _firstItemOffset : x;
        _pointerPercentage = width > 0 ? Math.Clamp(numerator / width, 0, 1) : 0;

        // The magnifier's focal point is the raw pointer, offset or not: upstream writes the same X it is given
        // (cpp:OnPointerMovedOverBackgroundStackPanel), and the two kinds ceiling differ (0.8 mouse, 1.0 touch).
        _focalX = x;
        _pointerIsTouch = touch;
        UpdateRatingItemsAppearance();
    }

    private void ChangeRatingBy(double change)
    {
        if (change == 0)
        {
            return;
        }

        var current = Value;
        var target = current != UnsetValue ? Math.Truncate(current) + change : InitialSetValue;
        SetRatingTo(target, fromPointer: false);
    }

    private void SetRatingTo(double newRating, bool fromPointer)
    {
        var previous = Value;
        var target = Math.Clamp(newRating, 0, MaxRating);

        if (previous <= UnsetValue && target == 0)
        {
            // Already unset and asked to unset: upstream skips the whole block, the event included.
            return;
        }

        if (!IsClearEnabled && target <= 0)
        {
            Value = 1;
        }
        else if (target == previous && IsClearEnabled && (target != MaxRating || fromPointer))
        {
            // Re-activating the star that already holds the value clears it - from the pointer always, from the
            // keyboard only while not maxed out (RatingControl.cpp:599-604).
            Value = UnsetValue;
        }
        else if (target > 0)
        {
            Value = target;
        }
        else
        {
            Value = UnsetValue;
        }

        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
