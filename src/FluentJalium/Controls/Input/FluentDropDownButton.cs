using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// A button whose only action is to open a flyout: WinUI's DropDownButton shape, on Jalium's native command,
/// click, keyboard and automation semantics.
/// </summary>
/// <remarks>
/// <para>
/// This is an own type because the runtime has no such control to retemplate: 26.10.9 exports
/// <see cref="SplitButton"/> but no DropDownButton or ToggleSplitButton (spike/SplitButtonProbe scans the
/// 3129 exported names). Plain <see cref="Button"/> has no flyout of its own and <see cref="SplitButton"/>
/// always paints a primary half, so the gap is the control, not the paint.
/// </para>
/// <para>
/// <see cref="IsExpanded"/> is the reason the type exists as far as visuals go: <c>FlyoutBase.IsOpen</c> is a
/// get-only CLR property rather than a dependency property, so no template cell or binding on a native control
/// can read whether a flyout is open (probe section J). Upstream's own drop-down has only Normal, PointerOver,
/// Pressed and Disabled states - <see cref="IsExpanded"/> drives no visual there either - so this property is
/// a state and a test lever, and nothing is claimed about how it looks.
/// </para>
/// </remarks>
public class FluentDropDownButton : Button
{
    private const string DefaultStyleResourceKey = "DefaultDropDownButtonStyle";

    private Style? _appliedDefaultStyle;
    private FlyoutBase? _hookedFlyout;
    private bool _syncing;

    /// <summary>Identifies the <see cref="Flyout"/> dependency property.</summary>
    public static readonly DependencyProperty FlyoutProperty = DependencyProperty.Register(
        nameof(Flyout), typeof(FlyoutBase), typeof(FluentDropDownButton),
        new PropertyMetadata(null, OnFlyoutChanged));

    /// <summary>Identifies the <see cref="IsExpanded"/> dependency property.</summary>
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(FluentDropDownButton),
        new PropertyMetadata(false, OnIsExpandedChanged));

    /// <summary>Creates a drop-down button and applies its named style when one is available.</summary>
    public FluentDropDownButton()
    {
        // The framework raises Click on the pointer, keyboard and automation-activation paths, so handling it
        // here keeps all three open the flyout instead of only the one a pointer gesture takes.
        Click += (_, _) => SetCurrentValue(IsExpandedProperty, !IsExpanded);
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();
    }

    /// <summary>Gets or sets the flyout shown when the button is activated.</summary>
    public FlyoutBase? Flyout
    {
        get => (FlyoutBase?)GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>Gets or sets whether the flyout is open. Setting it opens or closes that flyout.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty)!;
        set => SetValue(IsExpandedProperty, value);
    }

    private static void OnFlyoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        => ((FluentDropDownButton)sender).HookFlyout((FlyoutBase?)args.OldValue, (FlyoutBase?)args.NewValue);

    private static void OnIsExpandedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        => ((FluentDropDownButton)sender).ApplyExpanded((bool)args.NewValue!);

    private void HookFlyout(FlyoutBase? previous, FlyoutBase? current)
    {
        if (previous is not null)
        {
            previous.Opened -= OnFlyoutOpened;
            previous.Closed -= OnFlyoutClosed;
            if (previous.IsOpen)
            {
                previous.Hide();
            }
        }

        _hookedFlyout = current;
        if (current is not null)
        {
            current.Opened += OnFlyoutOpened;
            current.Closed += OnFlyoutClosed;
        }

        SetCurrentValue(IsExpandedProperty, false);
    }

    private void ApplyExpanded(bool expanded)
    {
        if (_syncing || !IsEnabled)
        {
            return;
        }

        var flyout = _hookedFlyout;
        if (flyout is null)
        {
            // Nothing to show: an app that sets IsExpanded before the flyout is a caller mistake, not a
            // reason to throw in an activation path. Stay closed rather than claim an open state.
            SetCurrentValue(IsExpandedProperty, false);
            return;
        }

        if (expanded && !flyout.IsOpen)
        {
            _syncing = true;
            try
            {
                flyout.ShowAt(this);
            }
            finally
            {
                _syncing = false;
            }
        }
        else if (!expanded && flyout.IsOpen)
        {
            _syncing = true;
            try
            {
                flyout.Hide();
            }
            finally
            {
                _syncing = false;
            }
        }
    }

    private void OnFlyoutOpened(object? sender, object args)
    {
        _syncing = true;
        SetCurrentValue(IsExpandedProperty, true);
        _syncing = false;
    }

    private void OnFlyoutClosed(object? sender, object args)
    {
        _syncing = true;
        SetCurrentValue(IsExpandedProperty, false);
        _syncing = false;
    }

    private void ApplyDefaultStyle()
    {
        // Same constructor fallback as FluentToggleSwitch: an implicit derived-control style is not
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
