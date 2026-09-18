using FluentJalium.Motion;
using FluentJalium.Themes;
using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// A Jalium-native, flat left navigation pane with application-owned page content.
/// Arrow keys move focus; Button activation changes selection.
/// </summary>
public sealed class FluentNavigationView : ContentControl
{
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(FluentNavigationItem), typeof(FluentNavigationView),
        new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty IsPaneOpenProperty = DependencyProperty.Register(
        nameof(IsPaneOpen), typeof(bool), typeof(FluentNavigationView),
        new PropertyMetadata(true, OnPanePropertyChanged));

    public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register(
        nameof(OpenPaneLength), typeof(double), typeof(FluentNavigationView),
        new PropertyMetadata(240d, OnPanePropertyChanged), IsPositiveLength);

    public static readonly DependencyProperty CompactPaneLengthProperty = DependencyProperty.Register(
        nameof(CompactPaneLength), typeof(double), typeof(FluentNavigationView),
        new PropertyMetadata(48d, OnPanePropertyChanged), IsPositiveLength);

    public static readonly DependencyProperty CompactModeThresholdWidthProperty = DependencyProperty.Register(
        nameof(CompactModeThresholdWidth), typeof(double), typeof(FluentNavigationView),
        new FrameworkPropertyMetadata(800d, FrameworkPropertyMetadataOptions.AffectsMeasure), IsPositiveLength);

    private Border? _paneRoot;
    private StackPanel? _menuPanel;
    private StackPanel? _footerPanel;
    private ScrollViewer? _menuScroll;
    private Canvas? _indicatorLayer;
    private Border? _indicatorBorder;
    private Button? _paneToggle;
    private NavigationIndicatorAnimator? _indicator;
    private bool _syncSelection;
    private bool _listening;
    private bool _animateNextIndicator;
    private bool? _lastNarrow;

    public FluentNavigationView()
    {
        MenuItems = new FluentNavigationItemCollection(this);
        FooterMenuItems = new FluentNavigationItemCollection(this);
        UseTemplateContentManagement();
        DefaultStyleKey = typeof(FluentNavigationView);
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    public FluentNavigationItemCollection MenuItems { get; }
    public FluentNavigationItemCollection FooterMenuItems { get; }

    /// <summary>The selected destination, or null. Non-null values must belong to this view.</summary>
    public FluentNavigationItem? SelectedItem
    {
        get => (FluentNavigationItem?)GetValue(SelectedItemProperty);
        set
        {
            if (value != null && value.Owner != this)
                throw new ArgumentException("SelectedItem must belong to this navigation view.", nameof(value));
            SetValue(SelectedItemProperty, value);
        }
    }

    public bool IsPaneOpen
    {
        get => (bool)GetValue(IsPaneOpenProperty)!;
        set => SetValue(IsPaneOpenProperty, value);
    }

    public bool IsCompact => !IsPaneOpen;

    public double OpenPaneLength
    {
        get => (double)GetValue(OpenPaneLengthProperty)!;
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public double CompactPaneLength
    {
        get => (double)GetValue(CompactPaneLengthProperty)!;
        set => SetValue(CompactPaneLengthProperty, value);
    }

    /// <summary>Width below which the pane initially collapses. Crossing it resets a manual toggle.</summary>
    public double CompactModeThresholdWidth
    {
        get => (double)GetValue(CompactModeThresholdWidthProperty)!;
        set => SetValue(CompactModeThresholdWidthProperty, value);
    }

    public event EventHandler<FluentNavigationSelectionChangedEventArgs>? SelectionChanged;

    public override void OnApplyTemplate()
    {
        ReleaseTemplateParts();
        base.OnApplyTemplate();
        _paneRoot = GetTemplateChild("PART_PaneRoot") as Border;
        _menuPanel = GetTemplateChild("PART_MenuItems") as StackPanel;
        _footerPanel = GetTemplateChild("PART_FooterMenuItems") as StackPanel;
        _menuScroll = GetTemplateChild("PART_MenuScrollViewer") as ScrollViewer;
        _indicatorLayer = GetTemplateChild("PART_IndicatorLayer") as Canvas;
        _indicatorBorder = GetTemplateChild("PART_SelectionIndicator") as Border;
        _paneToggle = GetTemplateChild("PART_PaneToggle") as Button;
        if (_indicatorBorder != null) _indicator = new NavigationIndicatorAnimator(_indicatorBorder);
        if (_paneToggle != null) _paneToggle.Click += OnPaneToggle;
        RefreshItems();
        UpdatePane();
        if (_listening) OnThemeChanged();
    }

    protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
    {
        ReleaseTemplateParts();
        base.OnTemplateChanged(oldTemplate, newTemplate);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        AdaptPane(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    internal void OnItemsChanged(FluentNavigationItem? previous, FluentNavigationItem? added)
    {
        if (previous != null) DetachItem(previous);
        if (added != null)
        {
            added.Owner = this;
            added.Click += OnItemInvoked;
            added.PreviewKeyDown += OnItemPreviewKeyDown;
            added.SetCompact(IsCompact);
            if (added.ToolTip == null && added.Content is string label) added.ToolTip = label;
        }
        RefreshItems();
        if (added?.IsSelected == true) SetCurrentValue(SelectedItemProperty, added);
    }

    internal void DetachItem(FluentNavigationItem item)
    {
        item.Click -= OnItemInvoked;
        item.PreviewKeyDown -= OnItemPreviewKeyDown;
        _menuPanel?.Children.Remove(item);
        _footerPanel?.Children.Remove(item);
        item.Owner = null;
        item.SetCompact(false);
        item.SetCurrentValue(FluentNavigationItem.IsSelectedProperty, false);
        if (ReferenceEquals(SelectedItem, item)) SetCurrentValue(SelectedItemProperty, null);
    }

    internal void RefreshItems()
    {
        SynchronizePanel(_menuPanel, MenuItems);
        SynchronizePanel(_footerPanel, FooterMenuItems);
        InvalidateMeasure();
        UpdateSelectionIndicator();
    }

    internal void OnItemSelectionChanged(FluentNavigationItem item, bool selected)
    {
        if (_syncSelection) return;
        if (selected) SetCurrentValue(SelectedItemProperty, item);
        else if (ReferenceEquals(SelectedItem, item)) SetCurrentValue(SelectedItemProperty, null);
    }

    private static void SynchronizePanel(StackPanel? panel, FluentNavigationItemCollection items)
    {
        if (panel == null) return;
        for (var index = panel.Children.Count - 1; index >= 0; index--)
            if (panel.Children[index] is not FluentNavigationItem item || !items.Contains(item))
                panel.Children.RemoveAt(index);
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            if (index < panel.Children.Count && ReferenceEquals(panel.Children[index], item)) continue;
            panel.Children.Remove(item);
            panel.Children.Insert(index, item);
        }
    }

    private IEnumerable<FluentNavigationItem> AllItems() => MenuItems.Concat(FooterMenuItems);

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var view = (FluentNavigationView)sender;
        if (view._syncSelection) return;
        var selected = (FluentNavigationItem?)args.NewValue;
        view._syncSelection = true;
        try
        {
            // A binding can set the DP without entering the CLR setter. Restore the previous
            // value before reporting an invalid destination so selection cannot become orphaned.
            if (selected != null && selected.Owner != view)
            {
                view.SetCurrentValue(SelectedItemProperty, args.OldValue);
                throw new ArgumentException("SelectedItem must belong to this navigation view.");
            }
            foreach (var item in view.AllItems())
                item.SetCurrentValue(FluentNavigationItem.IsSelectedProperty, ReferenceEquals(item, selected));
        }
        finally { view._syncSelection = false; }

        view._animateNextIndicator = view._listening;
        selected?.BringIntoView();
        view.UpdateSelectionIndicator();
        view.SelectionChanged?.Invoke(view, new FluentNavigationSelectionChangedEventArgs(
            (FluentNavigationItem?)args.OldValue, selected));
    }

    private void OnItemInvoked(object sender, RoutedEventArgs args)
    {
        if (sender is FluentNavigationItem { IsEnabled: true } item && item.Owner == this)
            SetCurrentValue(SelectedItemProperty, item);
    }

    private void OnItemPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (args.KeyboardModifiers != ModifierKeys.None) return;
        var items = AllItems().Where(item => item.IsEnabled && item.IsVisible && item.Focusable).ToArray();
        var current = Array.FindIndex(items, item => ReferenceEquals(item, sender));
        if (current < 0 || items.Length == 0) return;
        var target = args.Key switch
        {
            Key.Down => (current + 1) % items.Length,
            Key.Up => (current + items.Length - 1) % items.Length,
            Key.Home => 0,
            Key.End => items.Length - 1,
            _ => -1,
        };
        if (target < 0) return;
        items[target].BringIntoView();
        items[target].Focus();
        args.Handled = true;
    }

    private void OnPaneToggle(object sender, RoutedEventArgs args) => SetCurrentValue(IsPaneOpenProperty, !IsPaneOpen);

    private static void OnPanePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentNavigationView)sender).UpdatePane();

    private void OnSizeChanged(object sender, SizeChangedEventArgs args) => AdaptPane(args.NewSize.Width);

    private void AdaptPane(double width)
    {
        if (!double.IsFinite(width) || width <= 0) return;
        var narrow = width < CompactModeThresholdWidth;
        if (_lastNarrow == narrow) return;
        var applyAutomaticState = _lastNarrow.HasValue || !HasLocalValue(IsPaneOpenProperty);
        _lastNarrow = narrow;
        if (applyAutomaticState) SetCurrentValue(IsPaneOpenProperty, !narrow);
    }

    private void UpdatePane()
    {
        if (_paneRoot != null) _paneRoot.Width = IsPaneOpen ? OpenPaneLength : CompactPaneLength;
        foreach (var item in AllItems()) item.SetCompact(IsCompact);
        if (_paneToggle != null)
        {
            var label = IsPaneOpen ? "Collapse navigation" : "Expand navigation";
            _paneToggle.ToolTip = label;
            AutomationProperties.SetName(_paneToggle, label);
        }
        InvalidateMeasure();
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        if (_listening) return;
        _listening = true;
        LayoutUpdated += OnLayoutUpdated;
        FluentThemeManager.Changed += OnThemeChanged;
        OnThemeChanged();
        UpdateSelectionIndicator();
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (!_listening) return;
        _listening = false;
        LayoutUpdated -= OnLayoutUpdated;
        FluentThemeManager.Changed -= OnThemeChanged;
        _animateNextIndicator = false;
        _indicator?.Hide();
    }

    private void OnLayoutUpdated(object? sender, EventArgs args) => UpdateSelectionIndicator();

    private void OnThemeChanged()
    {
        if (_indicatorBorder != null)
            _indicatorBorder.Background = FluentThemeManager.GetBrush("AccentFillColorDefaultBrush");
        if (!FluentThemeManager.AnimationsEnabled) _indicator?.Complete();
        FluentThemeManager.ApplyMotionPolicy(this);
    }

    private void UpdateSelectionIndicator()
    {
        if (_indicator == null || _indicatorLayer == null) return;
        var item = SelectedItem;
        if (item == null || !item.IsVisible)
        {
            _animateNextIndicator = false;
            _indicator.Hide();
            return;
        }
        if (item.ActualHeight <= 0 || _indicatorLayer.ActualHeight <= 0) return;
        var transform = item.TransformToVisual(_indicatorLayer);
        if (transform == null) return;
        var position = transform.Transform(new Point(0, (item.ActualHeight - NavigationIndicatorAnimator.RestingHeight) / 2));

        // The shared layer spans the footer, so it cannot inherit the menu scroller's clip.
        // Suppress the bar while its selected menu row lies outside that viewport.
        if (MenuItems.Contains(item) && _menuScroll != null)
        {
            var scrollTransform = item.TransformToVisual(_menuScroll);
            if (scrollTransform == null) return;
            var center = scrollTransform.Transform(new Point(0, item.ActualHeight / 2));
            if (center.Y < 0 || center.Y > _menuScroll.ActualHeight)
            {
                _indicator.Hide();
                return;
            }
        }

        _indicator.MoveTo(position.X, position.Y,
            _animateNextIndicator && _listening && FluentThemeManager.AnimationsEnabled);
        _animateNextIndicator = false;
    }

    private void ReleaseTemplateParts()
    {
        if (_paneToggle != null) _paneToggle.Click -= OnPaneToggle;
        _indicator?.Dispose();
        _indicator = null;
        // Items are retained by the public collections, but must leave the discarded tree.
        _menuPanel?.Children.Clear();
        _footerPanel?.Children.Clear();
        _paneRoot = null;
        _menuPanel = null;
        _footerPanel = null;
        _menuScroll = null;
        _indicatorLayer = null;
        _indicatorBorder = null;
        _paneToggle = null;
    }

    private static bool IsPositiveLength(object? value) => value is double length && double.IsFinite(length) && length > 0;
}
