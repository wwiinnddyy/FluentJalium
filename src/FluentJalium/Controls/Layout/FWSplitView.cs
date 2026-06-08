using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace FluentJalium.Controls;

/// <summary>
/// Describes how a <see cref="FWSplitView"/> pane is layered against content.
/// </summary>
public enum FWSplitViewDisplayMode
{
    Overlay,
    Inline,
    CompactOverlay,
    CompactInline
}

/// <summary>
/// Describes which side hosts a <see cref="FWSplitView"/> pane.
/// </summary>
public enum FWSplitViewPanePlacement
{
    Left,
    Right
}

/// <summary>
/// FluentJalium SplitView control for shell panes and master-detail layouts.
/// </summary>
public class FWSplitView : ContentControl, IFluentJaliumControl
{
    public static readonly DependencyProperty PaneProperty =
        DependencyProperty.Register(nameof(Pane), typeof(object), typeof(FWSplitView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty PaneTemplateProperty =
        DependencyProperty.Register(nameof(PaneTemplate), typeof(DataTemplate), typeof(FWSplitView),
            new PropertyMetadata(null, OnLayoutStateChanged));

    public static readonly DependencyProperty IsPaneOpenProperty =
        DependencyProperty.Register(nameof(IsPaneOpen), typeof(bool), typeof(FWSplitView),
            new PropertyMetadata(true, OnIsPaneOpenChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(FWSplitViewDisplayMode), typeof(FWSplitView),
            new PropertyMetadata(FWSplitViewDisplayMode.Inline, OnLayoutStateChanged), IsValidDisplayMode);

    public static readonly DependencyProperty PanePlacementProperty =
        DependencyProperty.Register(nameof(PanePlacement), typeof(FWSplitViewPanePlacement), typeof(FWSplitView),
            new PropertyMetadata(FWSplitViewPanePlacement.Left, OnLayoutStateChanged), IsValidPanePlacement);

    public static readonly DependencyProperty OpenPaneLengthProperty =
        DependencyProperty.Register(nameof(OpenPaneLength), typeof(double), typeof(FWSplitView),
            new PropertyMetadata(320.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    public static readonly DependencyProperty CompactPaneLengthProperty =
        DependencyProperty.Register(nameof(CompactPaneLength), typeof(double), typeof(FWSplitView),
            new PropertyMetadata(48.0, OnLayoutStateChanged), IsValidNonNegativeDouble);

    private static readonly DependencyPropertyKey ActualPaneLengthPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ActualPaneLength), typeof(double), typeof(FWSplitView),
            new PropertyMetadata(320.0));

    public static readonly DependencyProperty ActualPaneLengthProperty = ActualPaneLengthPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IsLightDismissEnabledProperty =
        DependencyProperty.Register(nameof(IsLightDismissEnabled), typeof(bool), typeof(FWSplitView),
            new PropertyMetadata(true));

    public static readonly DependencyProperty PaneBackgroundProperty =
        DependencyProperty.Register(nameof(PaneBackground), typeof(Brush), typeof(FWSplitView),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ContentBackgroundProperty =
        DependencyProperty.Register(nameof(ContentBackground), typeof(Brush), typeof(FWSplitView),
            new PropertyMetadata(null));

    public FWSplitView()
    {
        UseTemplateContentManagement();
        UpdateActualPaneLength();
    }

    public event EventHandler? PaneOpened;

    public event EventHandler? PaneClosed;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Pane
    {
        get => GetValue(PaneProperty);
        set => SetValue(PaneProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? PaneTemplate
    {
        get => (DataTemplate?)GetValue(PaneTemplateProperty);
        set => SetValue(PaneTemplateProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty)!;
        set => SetValue(IsPaneOpenProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSplitViewDisplayMode DisplayMode
    {
        get => (FWSplitViewDisplayMode)GetValue(DisplayModeProperty)!;
        set => SetValue(DisplayModeProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWSplitViewPanePlacement PanePlacement
    {
        get => (FWSplitViewPanePlacement)GetValue(PanePlacementProperty)!;
        set => SetValue(PanePlacementProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty)!;
        set => SetValue(OpenPaneLengthProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty)!;
        set => SetValue(CompactPaneLengthProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public bool IsLightDismissEnabled
    {
        get => (bool)GetValue(IsLightDismissEnabledProperty)!;
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Brush? PaneBackground
    {
        get => (Brush?)GetValue(PaneBackgroundProperty);
        set => SetValue(PaneBackgroundProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Appearance)]
    public Brush? ContentBackground
    {
        get => (Brush?)GetValue(ContentBackgroundProperty);
        set => SetValue(ContentBackgroundProperty, value);
    }

    public void OpenPane()
    {
        IsPaneOpen = true;
    }

    public void ClosePane()
    {
        IsPaneOpen = false;
    }

    public void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public bool RequestLightDismiss()
    {
        if (!IsOverlayMode || !IsLightDismissEnabled || !IsPaneOpen)
        {
            return false;
        }

        ClosePane();
        return true;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public double ActualPaneLength => (double)GetValue(ActualPaneLengthProperty)!;

    public bool IsOverlayMode => DisplayMode is FWSplitViewDisplayMode.Overlay or FWSplitViewDisplayMode.CompactOverlay;

    private static void OnLayoutStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWSplitView splitView)
        {
            splitView.UpdateActualPaneLength();
            splitView.InvalidateMeasure();
            splitView.InvalidateVisual();
        }
    }

    private static void OnIsPaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        OnLayoutStateChanged(d, e);

        if (d is not FWSplitView splitView ||
            e.OldValue is not bool oldValue ||
            e.NewValue is not bool newValue ||
            oldValue == newValue)
        {
            return;
        }

        if (newValue)
        {
            splitView.PaneOpened?.Invoke(splitView, EventArgs.Empty);
        }
        else
        {
            splitView.PaneClosed?.Invoke(splitView, EventArgs.Empty);
        }
    }

    private void UpdateActualPaneLength()
    {
        var paneLength = IsPaneOpen
            ? OpenPaneLength
            : DisplayMode is FWSplitViewDisplayMode.CompactInline or FWSplitViewDisplayMode.CompactOverlay
                ? CompactPaneLength
                : 0.0;

        SetValue(ActualPaneLengthPropertyKey.DependencyProperty, paneLength);
    }

    private static bool IsValidDisplayMode(object? value)
    {
        return value is FWSplitViewDisplayMode mode && Enum.IsDefined(mode);
    }

    private static bool IsValidPanePlacement(object? value)
    {
        return value is FWSplitViewPanePlacement placement && Enum.IsDefined(placement);
    }

    private static bool IsValidNonNegativeDouble(object? value)
    {
        return value is double number && double.IsFinite(number) && number >= 0;
    }
}
