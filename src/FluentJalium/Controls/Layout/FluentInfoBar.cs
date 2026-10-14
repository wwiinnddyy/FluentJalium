using Jalium.UI;
using Jalium.UI.Controls;

namespace FluentJalium.Controls;

/// <summary>
/// An info bar that can be styled: WinUI's severity, closable and open semantics, with its appearance
/// written in JALXAML instead of being painted by the control.
/// </summary>
/// <remarks>
/// <para>
/// This type exists for one measured reason: 26.10.9's <see cref="InfoBar"/> never opts into template
/// content management, so assigning it a <c>Template</c> - through a style or as a local value - builds no
/// tree at all, and the control instead draws its own background, icon, text and 32-pixel close-button hit
/// rectangle from literal geometry. It only steps aside from that painting when it finds a template part
/// named <c>RootBorder</c>, which it cannot do without a tree. The opt-in call is protected on
/// <c>ContentControl</c>, so switching it on from a constructor is the whole adaptation; the probe records
/// the before-and-after tree (spike/ExpanderInfoBarProbe, docs/astra/adaptation/00 S0-m).
/// </para>
/// <para>
/// Two more overrides, each for a second measured gap in the same base class:
/// <list type="bullet">
/// <item><description>the part name. <c>InfoBar.OnApplyTemplate</c> looks up <c>RootBorder</c> and
/// <c>PART_CloseButton</c> by name (IL string literals, spike/InfoBarGhostProbe pass 2), and its
/// <c>OnRender</c> only steps aside once the first is found. Upstream's template calls its root
/// <c>ContentRoot</c>, so the shipping template uses the runtime's name instead: with the upstream name the
/// base drew a second, offset copy of the icon, the title, the message and its own close mark on top of our
/// template - the ghost in spike/VisualQA/out/surfaces.png. Renaming that one part made a styled bar and a
/// bar whose <c>OnRender</c> is suppressed capture byte-identically (pass 3: 7 distinct colours, 936 ink
/// pixels, both).</description></item>
/// <item><description>the measure. <c>InfoBar.MeasureOverride</c> returns the height of the base class's own
/// literal layout, not the template's: a bar whose message wraps to two lines measured 107.1 tall inside and
/// 69.8 outside, so the second line painted below the surface (spike/TextWrapProbe pass 1 case B). Taking the
/// larger of the two restores what upstream gets for free from a template-driven measure.</description></item>
/// </list>
/// What the base class still owns after all this: the Click of a <see cref="Button"/> named
/// <c>PART_CloseButton</c>, which raises <see cref="InfoBar.CloseButtonClick"/> and sets <c>IsOpen=false</c>,
/// and the collapse of a closed bar, which the style does itself because the base does not do it for a
/// templated control.
/// </para>
/// </remarks>
public class FluentInfoBar : InfoBar
{
    private const string DefaultStyleResourceKey = "DefaultFluentInfoBarStyle";

    private Style? _appliedDefaultStyle;

    /// <summary>Creates an info bar, switches on template content management and applies its named style.</summary>
    public FluentInfoBar()
    {
        UseTemplateContentManagement();
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();
    }

    /// <summary>Measures the base class's own geometry and the built template, and returns the larger of the two.</summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var self = base.MeasureOverride(availableSize);
        if (VisualChildrenCount == 0 || GetVisualChild(0) is not FrameworkElement root || root.Visibility == Visibility.Collapsed)
        {
            return self;
        }

        // The base class measures its own literal layout, not the tree it now hosts, so the template's
        // DesiredSize is whatever an earlier pass left behind. Measuring the root under this pass's own
        // constraint is what a template-driven control does for free, and it is the only way the wrapped
        // second line is inside the height before it is painted below it.
        root.Measure(availableSize);
        return new Size(Math.Max(self.Width, root.DesiredSize.Width), Math.Max(self.Height, root.DesiredSize.Height));
    }

    private void ApplyDefaultStyle()
    {
        // Same constructor fallback as FluentDropDownButton: an implicit derived-control style is not
        // guaranteed to resolve, and an application-assigned style - including an explicit null - is never
        // replaced.
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
