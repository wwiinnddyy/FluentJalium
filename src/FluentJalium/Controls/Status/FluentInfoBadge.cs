using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Which of the four things a badge shows. Upstream has no property for this: <c>InfoBadge.idl</c> declares no
/// visual-state contract, and <c>InfoBadge.cpp:59-83</c> picks one of the four <c>DisplayKindStates</c> from
/// <see cref="FluentInfoBadge.Value"/> and the icon, then walks there. A trigger needs a property to read, so the
/// same decision is published here instead of staying inside the control.
/// </summary>
public enum FluentInfoBadgeDisplayKind
{
    /// <summary>A plain dot: no number, no icon.</summary>
    Dot,

    /// <summary>The <see cref="FluentInfoBadge.Value"/> as text.</summary>
    Value,

    /// <summary>An icon element, scaled.</summary>
    Icon,

    /// <summary>An icon element that is a <see cref="FontIcon"/>, which upstream gives a different margin.</summary>
    FontIcon,
}

/// <summary>
/// A small status indicator: WinUI's <c>InfoBadge</c> - a dot, a number, or an icon in a rounded pill.
/// </summary>
/// <remarks>
/// <para>
/// An own type, because the runtime has nothing to retemplate: <c>spike/ControlCensus</c> reports both
/// <c>InfoBadge</c> and <c>Badge</c> absent. The upstream authority for the look is
/// <c>controls/dev/InfoBadge/InfoBadge_themeresources.xaml</c> (blob <c>b09b56572ac8a2159bf9ba2dda7f063ec5460c6a</c>);
/// <c>InfoBadge.xaml</c> is three lines that only base the implicit style on <c>DefaultInfoBadgeStyle</c>.
/// </para>
/// <para>
/// The icon input is where this layer departs from upstream on purpose. Upstream takes an <c>IconSource</c> and the
/// framework turns it into an <see cref="IconElement"/> for the template through <c>TemplateSettings.IconElement</c>.
/// No <c>IconSource</c> type ships here - <c>IconSource</c>, <c>FontIconSource</c>, <c>SymbolIconSource</c>,
/// <c>PathIconSource</c>, <c>BitmapIconSource</c> and <c>ImageIconSource</c> are all absent from 26.10.9 - so there
/// is no object this property could hold. <see cref="Icon"/> therefore takes what the template would have received
/// anyway, which the runtime does export (<see cref="FontIcon"/>, <see cref="SymbolIcon"/>, <see cref="PathIcon"/>).
/// The visual route is unchanged: the runtime has <see cref="Viewbox"/> (<c>Decorator</c>), so the icon is still
/// scaled by a view box the way upstream scales it.
/// </para>
/// <para>
/// The corner radius is computed the way upstream computes it - half the measured height, and a locally-set radius
/// wins (<c>InfoBadge.cpp:91-98</c>) - but the number travels through <see cref="Control.CornerRadiusProperty"/>
/// and a <c>TemplateBinding</c> instead of through the <c>TemplateSettings</c> object upstream writes. The part
/// keeps upstream's name (<c>RootGrid</c>) but is a <see cref="Border"/> here: this runtime's <c>Grid</c> has no
/// <c>CornerRadius</c> member, which the compiler says plainly, and the radius cannot be written onto the control
/// and reach the picture without that binding.
/// </para>
/// <para>
/// The six numeric metric rows upstream publishes stay out of the resource layer, because this reader cannot carry
/// one in any spelling: <c>x:Double</c> is a parse error (adaptation/00 S0-b, read again in S1-o clause 5) and a
/// <c>clr-namespace:System</c> row parses, keeps the right CLR type, and reads back as that type's default - a
/// <c>4</c> in the file, a <c>0</c> in the dictionary, measured in <c>spike/DoubleRowProbe</c> and written up as
/// S1-r. The four numbers the template actually reads are therefore literals in <c>Styles/InfoBadge.jalxaml</c>,
/// and since none of them differs between upstream's Light, Default and HighContrast dictionaries no theme loses a
/// value to the substitution. The two that do differ (<c>InfoBadgeIconHeight</c>, 8 and 9) are dead rows: nothing in
/// upstream reads them, so the difference moves no pixel there either.
/// </para>
/// </remarks>
public class FluentInfoBadge : Control
{
    private const string DefaultStyleResourceKey = "DefaultInfoBadgeStyle";
    private const string ValueTextPartName = "ValueTextBlock";

    /// <summary>The presenter inside the Viewbox upstream calls <c>IconPresenter</c>; the trigger aims the Viewbox.</summary>
    private const string IconContentPartName = "IconContent";

    /// <summary>Upstream's own default: <c>InfoBadge.idl:25</c> declares <c>-1</c>, which means "show no number".</summary>
    private const int NoValue = -1;

    /// <summary>Identifies the <see cref="Value"/> dependency property.</summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(FluentInfoBadge),
        new PropertyMetadata(NoValue, OnDisplayKindConditionChanged));

    /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(FluentInfoBadge),
        new PropertyMetadata(null, OnDisplayKindConditionChanged));

    /// <summary>Identifies the <see cref="DisplayKind"/> dependency property.</summary>
    /// <remarks>
    /// A writable property with a private setter, not a read-only key: this runtime's
    /// <c>DependencyPropertyKey</c> exposes no <c>Property</c> member to hand back, and a trigger needs a real
    /// property to watch. Nothing outside this type can assign it.
    /// </remarks>
    public static readonly DependencyProperty DisplayKindProperty = DependencyProperty.Register(
        nameof(DisplayKind), typeof(FluentInfoBadgeDisplayKind), typeof(FluentInfoBadge),
        new PropertyMetadata(FluentInfoBadgeDisplayKind.Dot));

    private TextBlock? _valueText;
    private ContentPresenter? _iconContent;
    private Style? _appliedDefaultStyle;

    /// <summary>Creates a badge and applies its named style when one is available.</summary>
    public FluentInfoBadge()
    {
        ApplyDefaultStyle();
        Loaded += (_, _) =>
        {
            ApplyDefaultStyle();
            UpdateDisplay();
        };
        SizeChanged += (_, _) => UpdateCornerRadius();
    }

    /// <summary>Gets or sets the number to show. Any value below zero shows no number; below <c>-1</c> throws, as upstream does.</summary>
    public int Value
    {
        get => (int)GetValue(ValueProperty)!;
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Gets or sets the icon to show, or <c>null</c> for a dot or a number.</summary>
    public IconElement? Icon
    {
        get => (IconElement?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Which of the four pictures the badge is currently set up to show. The style's triggers read this.</summary>
    public FluentInfoBadgeDisplayKind DisplayKind
    {
        get => (FluentInfoBadgeDisplayKind)GetValue(DisplayKindProperty)!;
        private set => SetValue(DisplayKindProperty, value);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _valueText = GetTemplateChild(ValueTextPartName) as TextBlock;
        _iconContent = GetTemplateChild(IconContentPartName) as ContentPresenter;
        UpdateDisplay();
    }

    /// <summary>
    /// Upstream's <c>MeasureOverride</c> (<c>InfoBadge.cpp:29-37</c>) raises the width to the height <em>only when
    /// the box is taller than it is wide</em>, so a 4x16 dot becomes a 16x16 circle and a wide value pill stays
    /// wide. Squaring to the longer side in both directions would read the same for a dot and wrongly blow up a
    /// pill, which is the difference this guard keeps.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
        var desired = base.MeasureOverride(constraint);
        return desired.Width < desired.Height ? new Size(desired.Height, desired.Height) : desired;
    }

    private static void OnDisplayKindConditionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentInfoBadge)sender).OnConditionChanged(args);

    private void OnConditionChanged(DependencyPropertyChangedEventArgs args)
    {
        // Upstream walks out of the property change rather than reading a smaller number as "no value"
        // (InfoBadge.cpp:46-49). The callback runs after the property has been written, so the rejected number is
        // handed back before throwing - otherwise the control would go on reporting a value it just refused.
        if (args.Property == ValueProperty && args.NewValue is int rejected && rejected < NoValue)
        {
            SetCurrentValue(ValueProperty, args.OldValue);
            throw new ArgumentOutOfRangeException(nameof(Value), rejected, "Value must be -1 or greater.");
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var kind = ResolveKind();
        DisplayKind = kind;

        if (_valueText is not null)
        {
            _valueText.Text = kind == FluentInfoBadgeDisplayKind.Value ? Value.ToString() : string.Empty;
        }

        if (_iconContent is not null)
        {
            _iconContent.Content = kind is FluentInfoBadgeDisplayKind.Icon or FluentInfoBadgeDisplayKind.FontIcon
                ? Icon
                : null;
        }

        UpdateCornerRadius();
    }

    /// <summary>
    /// The same precedence the C++ walks in <c>OnDisplayKindPropertiesChanged</c>: a number beats an icon, a
    /// <see cref="FontIcon"/> gets its own state, and the dot is what is left.
    /// </summary>
    private FluentInfoBadgeDisplayKind ResolveKind()
    {
        if (Value >= 0)
        {
            return FluentInfoBadgeDisplayKind.Value;
        }

        return Icon switch
        {
            FontIcon => FluentInfoBadgeDisplayKind.FontIcon,
            not null => FluentInfoBadgeDisplayKind.Icon,
            _ => FluentInfoBadgeDisplayKind.Dot,
        };
    }

    private void UpdateCornerRadius()
    {
        // InfoBadge.cpp:91-98: half the measured height, and a locally-set radius wins. The guard reads the local
        // value, so the SetCurrentValue below - which does not create one - never trips it. The number travels to
        // the pill through the template's TemplateBinding, which is what upstream does through TemplateSettings.
        if (ReadLocalValue(CornerRadiusProperty) != DependencyProperty.UnsetValue)
        {
            return;
        }

        var height = ActualHeight;
        if (!double.IsFinite(height) || height <= 0)
        {
            return;
        }

        SetCurrentValue(CornerRadiusProperty, new CornerRadius(height / 2));
    }

    private void ApplyDefaultStyle()
    {
        // The named-style fallback every own type in this layer uses: an implicit style for a derived type is not
        // guaranteed to resolve (adaptation/00 S0-l), and a style an application set - including an explicit null -
        // is never overwritten.
        if ((Style is not null || _appliedDefaultStyle is not null) &&
            !ReferenceEquals(Style, _appliedDefaultStyle))
        {
            return;
        }

        if (_appliedDefaultStyle is null &&
            !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue))
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
}
