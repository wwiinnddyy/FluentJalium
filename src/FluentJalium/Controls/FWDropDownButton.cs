using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium drop-down button control.
/// </summary>
public class FWDropDownButton : Button, IFluentJaliumControl
{
    public static readonly DependencyProperty FlyoutProperty =
        DependencyProperty.Register(nameof(Flyout), typeof(FlyoutBase), typeof(FWDropDownButton),
            new PropertyMetadata(null, OnFlyoutChanged));

    public static readonly DependencyProperty ShowChevronArrowProperty =
        DependencyProperty.Register(nameof(ShowChevronArrow), typeof(bool), typeof(FWDropDownButton),
            new PropertyMetadata(true));

    private bool _isFlyoutOpen;

    public FWDropDownButton()
    {
        UseTemplateContentManagement();
    }

    /// <summary>
    /// Gets or sets the flyout opened by this button.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FlyoutBase? Flyout
    {
        get => (FlyoutBase?)GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the chevron is visible in the default template.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public bool ShowChevronArrow
    {
        get => (bool)GetValue(ShowChevronArrowProperty)!;
        set => SetValue(ShowChevronArrowProperty, value);
    }

    /// <summary>
    /// Gets whether the flyout is currently open.
    /// </summary>
    public bool IsFlyoutOpen => _isFlyoutOpen;

    public event EventHandler? FlyoutOpened;

    public event EventHandler? FlyoutClosed;

    /// <summary>
    /// Opens the associated flyout.
    /// </summary>
    public void OpenFlyout()
    {
        if (!IsEnabled || Flyout == null || Flyout.IsOpen)
            return;

        Flyout.ShowAt(this);
    }

    /// <summary>
    /// Closes the associated flyout.
    /// </summary>
    public void CloseFlyout()
    {
        Flyout?.Hide();
    }

    protected override void OnClick()
    {
        base.OnClick();
        OpenFlyout();
    }

    private void OnFlyoutChanged(FlyoutBase? oldFlyout, FlyoutBase? newFlyout)
    {
        if (oldFlyout != null)
        {
            if (oldFlyout.IsOpen)
                oldFlyout.Hide();

            oldFlyout.Opened -= OnFlyoutOpened;
            oldFlyout.Closed -= OnFlyoutClosed;
        }

        _isFlyoutOpen = newFlyout?.IsOpen == true;

        if (newFlyout != null)
        {
            newFlyout.Opened += OnFlyoutOpened;
            newFlyout.Closed += OnFlyoutClosed;
        }
    }

    private void OnFlyoutOpened(object? sender, EventArgs e)
    {
        _isFlyoutOpen = true;
        FlyoutOpened?.Invoke(this, EventArgs.Empty);
    }

    private void OnFlyoutClosed(object? sender, EventArgs e)
    {
        _isFlyoutOpen = false;
        FlyoutClosed?.Invoke(this, EventArgs.Empty);
    }

    private static void OnFlyoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWDropDownButton button)
        {
            button.OnFlyoutChanged((FlyoutBase?)e.OldValue, (FlyoutBase?)e.NewValue);
        }
    }
}
