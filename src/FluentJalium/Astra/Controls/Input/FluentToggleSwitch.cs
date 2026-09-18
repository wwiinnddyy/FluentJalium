using Jalium.UI;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using FluentJalium.Themes;

namespace FluentJalium.Controls;

/// <summary>
/// A two-state switch with WinUI track geometry and Jalium's toggle, click,
/// command, keyboard and automation contracts.
/// </summary>
/// <remarks>
/// Adapted from LanStartWrite.Inkcanvas/FluentToggleSwitch.cs. Jalium's native
/// ToggleSwitch uses a different fixed track geometry; the visual template is
/// therefore backed by ToggleButton and only pointer dragging is implemented here.
/// </remarks>
public sealed class FluentToggleSwitch : ToggleButton
{
    private const string DefaultStyleResourceKey = "FluentToggleSwitchStyle";
    private const double ThumbTravel = 20;
    private const double DragThreshold = 3;

    /// <summary>Identifies the template's thumb translation.</summary>
    public static readonly DependencyProperty ThumbOffsetProperty = DependencyProperty.Register(
        nameof(ThumbOffset), typeof(Thickness), typeof(FluentToggleSwitch),
        new PropertyMetadata(new Thickness(0)));

    /// <summary>Identifies whether a captured pointer is pressing the switch.</summary>
    public static readonly DependencyProperty IsPointerPressedProperty = DependencyProperty.Register(
        nameof(IsPointerPressed), typeof(bool), typeof(FluentToggleSwitch),
        new PropertyMetadata(false));

    private TouchDevice? _touch;
    private bool _mouse;
    private double _startX;
    private double _startOffset;
    private bool _dragged;
    private bool? _gestureValue;
    private Style? _appliedDefaultStyle;
    private bool _listeningToTheme;

    /// <summary>Gets the thumb offset used by FluentToggleSwitchStyle.</summary>
    public Thickness ThumbOffset
    {
        get => (Thickness)GetValue(ThumbOffsetProperty)!;
        private set => SetValue(ThumbOffsetProperty, value);
    }

    /// <summary>Gets whether a mouse or touch gesture is currently captured.</summary>
    public bool IsPointerPressed
    {
        get => (bool)GetValue(IsPointerPressedProperty)!;
        private set => SetValue(IsPointerPressedProperty, value);
    }

    /// <summary>Creates a switch and applies its named style when available.</summary>
    public FluentToggleSwitch()
    {
        IsThreeState = false;
        ApplyDefaultStyle();

        PreviewMouseLeftButtonDown += OnMouseDown;
        PreviewMouseMove += OnMouseMove;
        PreviewMouseLeftButtonUp += OnMouseUp;
        LostMouseCapture += (_, _) => { if (_mouse) Cancel(); };

        PreviewTouchDown += OnTouchDown;
        PreviewTouchMove += OnTouchMove;
        PreviewTouchUp += OnTouchUp;
        LostTouchCapture += (_, e) => { if (_touch?.Id == e.TouchDevice.Id) Cancel(); };
        PreviewPointerCancel += (_, e) =>
        {
            if (e is PointerEventArgs pointer && _touch?.Id == pointer.Pointer.PointerId)
                Cancel();
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && IsPointerPressed)
            {
                Cancel();
                e.Handled = true;
            }
        };

        Loaded += (_, _) =>
        {
            if (!_listeningToTheme)
            {
                FluentThemeManager.Changed += OnThemeChanged;
                _listeningToTheme = true;
            }
            OnThemeChanged();
        };
        Unloaded += (_, _) =>
        {
            Cancel();
            if (_listeningToTheme)
            {
                FluentThemeManager.Changed -= OnThemeChanged;
                _listeningToTheme = false;
            }
        };
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Snap();
    }

    /// <inheritdoc />
    protected override void OnChecked(RoutedEventArgs e)
    {
        Snap();
        base.OnChecked(e);
    }

    /// <inheritdoc />
    protected override void OnUnchecked(RoutedEventArgs e)
    {
        Snap();
        base.OnUnchecked(e);
    }

    /// <inheritdoc />
    protected override void OnIndeterminate(RoutedEventArgs e)
    {
        Snap();
        base.OnIndeterminate(e);
    }

    /// <inheritdoc />
    protected override void OnToggle()
    {
        // Pointer commits go through OnClick, exactly like keyboard/automation
        // activation. Supplying the drag result prevents a second toggle. Use
        // SetCurrentValue on every path so an IsChecked binding remains attached.
        SetCurrentValue(IsCheckedProperty, _gestureValue ?? IsChecked != true);
    }

    /// <inheritdoc />
    protected override void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        base.OnIsEnabledChanged(oldValue, newValue);
        if (!newValue) Cancel();
    }

    private void OnThemeChanged() => ApplyDefaultStyle();

    private void ApplyDefaultStyle()
    {
        // Constructor fallback for runtimes which do not resolve an implicit
        // derived-control style. Never replace an application-assigned style,
        // including an explicit null. The application can also replace Template.
        if ((Style is not null || _appliedDefaultStyle is not null) &&
            !ReferenceEquals(Style, _appliedDefaultStyle)) return;
        if (_appliedDefaultStyle is null &&
            !ReferenceEquals(ReadLocalValue(StyleProperty), DependencyProperty.UnsetValue)) return;
        if (TryFindResource(DefaultStyleResourceKey) is not Style style) return;

        _appliedDefaultStyle = style;
        SetCurrentValue(StyleProperty, style);
    }

    private void Begin(double x)
    {
        _startX = x;
        _startOffset = IsChecked == true ? ThumbTravel : 0;
        _dragged = false;
        IsPointerPressed = true;
        Focus();
    }

    private void Move(double x)
    {
        var delta = x - _startX;
        _dragged |= Math.Abs(delta) >= DragThreshold;
        if (_dragged)
            ThumbOffset = new Thickness(Math.Clamp(_startOffset + delta, 0, ThumbTravel), 0, 0, 0);
    }

    private void End(Point point)
    {
        var commit = IsEnabled && (_dragged ||
            (point.X >= 0 && point.X <= ActualWidth && point.Y >= 0 && point.Y <= ActualHeight));
        var next = _dragged ? ThumbOffset.Left >= ThumbTravel / 2 : IsChecked != true;
        Release();
        try
        {
            if (commit && IsEnabled)
            {
                _gestureValue = next;
                OnClick();
            }
        }
        finally
        {
            _gestureValue = null;
            Snap();
        }
    }

    private void Snap() => ThumbOffset = new Thickness(IsChecked == true ? ThumbTravel : 0, 0, 0, 0);

    private void Cancel()
    {
        Release();
        Snap();
    }

    private void Release()
    {
        var touch = _touch;
        var mouse = _mouse;
        _touch = null;
        _mouse = false;
        IsPointerPressed = false;
        if (touch is not null) ReleaseTouchCapture(touch);
        if (mouse && IsMouseCaptured) ReleaseMouseCapture();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled) return;
        // Prevent a second pointer from reaching ToggleButton's base handlers.
        e.Handled = true;
        if (_touch is not null || _mouse || !CaptureMouse()) return;
        _mouse = true;
        Begin(e.GetPosition(this).X);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouse) return;
        Move(e.GetPosition(this).X);
        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_mouse) return;
        End(e.GetPosition(this));
        e.Handled = true;
    }

    private void OnTouchDown(object sender, TouchEventArgs e)
    {
        if (!IsEnabled) return;
        e.Handled = true;
        if (_touch is not null || _mouse || !CaptureTouch(e.TouchDevice)) return;
        _touch = e.TouchDevice;
        Begin(e.GetTouchPoint(this).Position.X);
    }

    private void OnTouchMove(object sender, TouchEventArgs e)
    {
        if (_touch?.Id != e.TouchDevice.Id) return;
        Move(e.GetTouchPoint(this).Position.X);
        e.Handled = true;
    }

    private void OnTouchUp(object sender, TouchEventArgs e)
    {
        if (_touch?.Id != e.TouchDevice.Id) return;
        End(e.GetTouchPoint(this).Position);
        e.Handled = true;
    }
}
