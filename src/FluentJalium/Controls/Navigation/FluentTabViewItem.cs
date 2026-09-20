using Jalium.UI;
using Jalium.UI.Automation;
using Jalium.UI.Controls;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// One tab: its label and icon in the strip, its content in the body while it is selected.
/// </summary>
/// <remarks>
/// <para>
/// The type is own, not the runtime's <see cref="TabItem"/>, on measured grounds (spike/TabViewStyle,
/// docs/astra/adaptation/00 S1-h): a <c>TabItem</c> subclass can reach the framework's
/// <c>UseTemplateContentManagement</c> - the opt-in is protected on <c>ContentControl</c> and the tab item is one -
/// yet the tree it builds is still empty, the assigned template's parts never appear, and the item paints itself
/// through its own <c>OnRender</c>. With no part there is nothing to hang a header, a separator, a close button
/// or the selected tab's out-curved corners on, so the geometry WinUI draws cannot be expressed on that base at
/// all. <see cref="HeaderedContentControl"/> keeps upstream's own split of <c>Header</c> for the strip and
/// <c>Content</c> for the body, and its chain reaches the same opt-in the host uses - which is what makes a
/// JALXAML template build at all (adaptation/00 S0-m).
/// </para>
/// <para>
/// The item never presents its own <c>Content</c>: the template has no presenter for it, so exactly one parent -
/// the view's body presenter - ever holds that element. That is the same division upstream's template makes
/// (TabView.xaml:303-599 draws the header row only; TabView.xaml:46 shows the body).
/// </para>
/// </remarks>
public class FluentTabViewItem : HeaderedContentControl
{
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected), typeof(bool), typeof(FluentTabViewItem),
        new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(FluentTabViewItem), new PropertyMetadata(true));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(object), typeof(FluentTabViewItem),
        new PropertyMetadata(null, OnIconChanged));

    // Upstream keeps the icon column collapsed through the IconStates group's NoIcon visual state
    // (TabView.xaml:475-480); a trigger needs a value it can compare, and an object DP cannot be matched against
    // null by a property trigger, so the item publishes the boolean the template actually keys on.
    private static readonly DependencyPropertyKey HasIconPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(HasIcon), typeof(bool), typeof(FluentTabViewItem), new PropertyMetadata(false));

    public static readonly DependencyProperty HasIconProperty = HasIconPropertyKey.DependencyProperty;

    // The two states that shorten the 1 DIP line under the strip next to the selected tab
    // (TabView.xaml:511-524): the view knows the neighbours, the item only reports them to its own template.
    private static readonly DependencyPropertyKey IsLeftOfSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsLeftOfSelected), typeof(bool), typeof(FluentTabViewItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsLeftOfSelectedProperty = IsLeftOfSelectedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsRightOfSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsRightOfSelected), typeof(bool), typeof(FluentTabViewItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsRightOfSelectedProperty = IsRightOfSelectedPropertyKey.DependencyProperty;

    // Upstream filters the control's CornerRadius down to its top two before handing it to the tab's Border
    // (TabView.xaml:505, TopCornerRadiusFilterConverter); a Border here needs the same four numbers and has no
    // converter to ask, so the item publishes the filtered value.
    private static readonly DependencyPropertyKey TopCornerRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(TopCornerRadius), typeof(CornerRadius), typeof(FluentTabViewItem), new PropertyMetadata(new CornerRadius()));

    public static readonly DependencyProperty TopCornerRadiusProperty = TopCornerRadiusPropertyKey.DependencyProperty;

    private Button? _closeButton;
    private bool _pressedHere;
    private string? _automaticName;

    /// <summary>Creates a tab and switches on the template content management its base class leaves off.</summary>
    public FluentTabViewItem()
    {
        UseTemplateContentManagement();
        DefaultStyleKey = typeof(FluentTabViewItem);
        MouseLeftButtonDown += OnMousePressed;
        MouseLeftButtonUp += OnMouseReleased;
        PreviewKeyDown += OnItemPreviewKeyDown;
    }

    /// <summary>Whether this tab is the one whose content fills the body.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty)!;
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Whether the tab shows its own close button. Default is true, as upstream's is.</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty)!;
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>A small visual for the 16-DIP icon column, or null to collapse it.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool HasIcon => (bool)GetValue(HasIconProperty)!;

    /// <summary>True for the tab immediately left of the selected one; it draws a 2-DIP gap in the strip's line.</summary>
    public bool IsLeftOfSelected => (bool)GetValue(IsLeftOfSelectedProperty)!;

    /// <summary>True for the tab immediately right of the selected one.</summary>
    public bool IsRightOfSelected => (bool)GetValue(IsRightOfSelectedProperty)!;

    /// <summary>The item's <c>CornerRadius</c> with its bottom two corners zeroed, which is what the tab Border draws.</summary>
    public CornerRadius TopCornerRadius => (CornerRadius)GetValue(TopCornerRadiusProperty)!;

    /// <summary>Recomputes <see cref="TopCornerRadius"/> so an app or a theme that changes the radius is followed.</summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        var radius = CornerRadius;
        var top = new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0);
        if (top != TopCornerRadius)
        {
            SetValue(TopCornerRadiusPropertyKey, top);
        }

        return base.MeasureOverride(availableSize);
    }

    internal FluentTabView? Owner { get; set; }

    /// <summary>Applied to every pointer press and activation key on the tab body.</summary>
    internal void SelectFromPointer()
    {
        if (IsEnabled)
        {
            Owner?.SelectItem(this);
        }
    }

    internal void SetNeighboursOfSelected(bool left, bool right)
    {
        SetValue(IsLeftOfSelectedPropertyKey, left);
        SetValue(IsRightOfSelectedPropertyKey, right);
    }

    /// <summary>
    /// Resolves the template's parts. A missing close button is allowed - a tab whose <see cref="IsClosable"/> is
    /// false never shows one - so the wiring is per lookup, not asserted.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_closeButton != null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        _closeButton = GetTemplateChild("CloseButton") as Button;
        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;
        }
    }

    protected override void OnHeaderChanged(object? oldHeader, object? newHeader)
    {
        base.OnHeaderChanged(oldHeader, newHeader);
        var current = AutomationProperties.GetName(this);
        if (string.IsNullOrEmpty(current) || current == _automaticName)
        {
            _automaticName = newHeader as string ?? string.Empty;
            AutomationProperties.SetName(this, _automaticName);
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs args) => Owner?.OnCloseRequested(this);

    private void OnMousePressed(object? sender, MouseButtonEventArgs args) => _pressedHere = true;

    private void OnMouseReleased(object? sender, MouseButtonEventArgs args)
    {
        // Only a release that started on this tab counts: a press on one tab and an up on another is the gesture
        // upstream keeps for CanReorderTabs, and selecting there would fire on a drag.
        var started = _pressedHere;
        _pressedHere = false;
        if (started && IsMouseOver)
        {
            SelectFromPointer();
            args.Handled = true;
        }
    }

    private void OnItemPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (args.KeyboardModifiers != ModifierKeys.None &&
            args.Key is not (Key.Enter or Key.Space))
        {
            return;
        }

        switch (args.Key)
        {
            // Upstream sets SingleSelectionFollowsFocus=False on the strip (TabView.xaml:56), so the arrow keys
            // move focus and these two keys are what actually changes the selection.
            case Key.Enter or Key.Space:
                SelectFromPointer();
                args.Handled = true;
                break;
            case Key.Left or Key.Right:
                Owner?.MoveFocus(this, args.Key == Key.Left ? -1 : 1);
                args.Handled = true;
                break;
            case Key.Home:
                Owner?.MoveFocus(this, 0);
                args.Handled = true;
                break;
            case Key.End:
                Owner?.MoveFocus(this, -2);
                args.Handled = true;
                break;
            case Key.Delete when IsClosable:
                Owner?.OnCloseRequested(this);
                args.Handled = true;
                break;
        }
    }

    private static void OnIsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var item = (FluentTabViewItem)sender;
        if ((bool)args.NewValue! && item.Owner != null)
        {
            item.Owner.OnItemSelectedByItself(item);
        }
    }

    private static void OnIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var item = (FluentTabViewItem)sender;
        item.RemoveLogicalChild(args.OldValue);
        item.AddLogicalChild(args.NewValue);
        item.SetValue(HasIconPropertyKey, args.NewValue != null);
    }
}
