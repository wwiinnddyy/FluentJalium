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
/// What the base class still owns after that: <c>IsOpen</c> feeding its own measure, and the Click of a
/// <see cref="Button"/> named <c>PART_CloseButton</c>, which raises <see cref="InfoBar.CloseButtonClick"/>
/// and sets <c>IsOpen=false</c>. Nothing else in the template is wired by name, and the base does not
/// collapse a templated bar when <c>IsOpen</c> turns false, so the style does that itself.
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
