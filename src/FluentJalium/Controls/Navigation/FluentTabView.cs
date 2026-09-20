using System.Windows.Input;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;

namespace FluentJalium.Controls;

/// <summary>
/// A row of tabs over one body: the WinUI <c>TabView</c> surface, with the strip's selection, close and add
/// semantics driven from the view and its appearance written in JALXAML.
/// </summary>
/// <remarks>
/// <para>
/// The host is own, not the runtime's <see cref="TabControl"/>, on measured grounds rather than preference.
/// spike/TabViewStyle walked the framework's own type chains and tried both template routes:
/// <list type="bullet">
/// <item><description><c>TabControl</c> is a <c>Selector</c>-style <c>ItemsControl</c> - <c>TabControl &lt; Selector &lt;
/// ItemsControl &lt; Control</c> - so <c>ContentControl</c>'s protected <c>UseTemplateContentManagement</c>, the one
/// switch that makes a JALXAML template build here (adaptation/00 S0-m), is not in its chain at all. No subclass
/// can reach it.</description></item>
/// <item><description>Both routes that do build a tree for <c>ListBox</c> in the same window - an implicit style's
/// <c>Template</c> setter (the route every shipping Astra list style uses) and a local value - leave the tab pair's
/// realized tree unchanged: the assigned template's parts are absent, the strip is still the framework's own
/// <c>StackPanel</c> of <c>TabItem</c>s, and the tint put on the template's root paints zero pixels (spike/TabViewStyle
/// sections C-G, docs/astra/adaptation/00 S1-h).</description></item>
/// <item><description>Worse than not building: asking a <c>TabControl</c> for a template collapses its own strip's
/// measure to zero wide, so the tabs stop taking space while still being drawn (same section D, 420 DIP host,
/// strip reported 0x0).</description></item>
/// </list>
/// A <c>ContentControl</c> subclass on the same route does build (section I: named parts present, template fill
/// painted), which is what <see cref="FluentNavigationView"/> already ships on. So this type takes that base and
/// the geometry WinUI draws, and the item type takes the same base for the same reason.
/// </para>
/// <para>
/// What this host does not carry: tab drag-out and reorder (upstream's <c>CanDragTabs</c>, <c>CanReorderTabs</c>,
/// <c>AllowDropTabs</c> and their eleven drag VisualStates), the overflow scroll RepeatButtons and the
/// <c>TabWidthMode</c> states. Those are named in docs/astra/audits/tab-view.md §7 rather than half-implemented.
/// </para>
/// </remarks>
public class FluentTabView : ContentControl
{
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(FluentTabView),
        new PropertyMetadata(-1, OnSelectedIndexChanged));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(FluentTabViewItem), typeof(FluentTabView),
        new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty IsAddButtonVisibleProperty = DependencyProperty.Register(
        nameof(IsAddButtonVisible), typeof(bool), typeof(FluentTabView), new PropertyMetadata(false));

    public static readonly DependencyProperty AddTabButtonCommandProperty = DependencyProperty.Register(
        nameof(AddTabButtonCommand), typeof(ICommand), typeof(FluentTabView), new PropertyMetadata(null));

    public static readonly DependencyProperty AddTabButtonCommandParameterProperty = DependencyProperty.Register(
        nameof(AddTabButtonCommandParameter), typeof(object), typeof(FluentTabView), new PropertyMetadata(null));

    public static readonly DependencyProperty TabStripHeaderProperty = DependencyProperty.Register(
        nameof(TabStripHeader), typeof(object), typeof(FluentTabView),
        new PropertyMetadata(null, OnStripContentChanged));

    public static readonly DependencyProperty TabStripFooterProperty = DependencyProperty.Register(
        nameof(TabStripFooter), typeof(object), typeof(FluentTabView),
        new PropertyMetadata(null, OnStripContentChanged));

    private static readonly DependencyPropertyKey SelectedContentPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SelectedContent), typeof(object), typeof(FluentTabView), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedContentProperty = SelectedContentPropertyKey.DependencyProperty;

    private StackPanel? _strip;
    private Button? _addButton;
    private bool _syncSelection;

    /// <summary>Creates a tab view, opts into template content management and applies the style's strip.</summary>
    public FluentTabView()
    {
        TabItems = new FluentTabViewItemCollection(this);
        UseTemplateContentManagement();
        DefaultStyleKey = typeof(FluentTabView);
        PreviewKeyDown += OnViewPreviewKeyDown;
    }

    /// <summary>The tabs, in strip order.</summary>
    public FluentTabViewItemCollection TabItems { get; }

    /// <summary>
    /// The selected tab's index, or -1. Nothing selects itself: an app sets this, as upstream requires.
    /// An index past the end names no tab and simply leaves the view with none selected, because markup sets
    /// attributes before it appends children - <c>SelectedIndex="0"</c> on an empty <c>TabItems</c> is the normal
    /// order, and refusing it there is a crash at window start. Only a value below -1 is refused.
    /// </summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty)!;
        set
        {
            if (value < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "SelectedIndex cannot be below -1.");
            }

            SetValue(SelectedIndexProperty, value);
        }
    }

    public FluentTabViewItem? SelectedItem
    {
        get => (FluentTabViewItem?)GetValue(SelectedItemProperty);
        set
        {
            if (value != null && value.Owner != this)
            {
                throw new ArgumentException("SelectedItem must belong to this tab view.", nameof(value));
            }

            SetCurrentValue(SelectedIndexProperty, value == null ? -1 : TabItems.IndexOf(value));
        }
    }

    /// <summary>Whether the + button at the end of the strip shows. Default false, as upstream's.</summary>
    public bool IsAddButtonVisible
    {
        get => (bool)GetValue(IsAddButtonVisibleProperty)!;
        set => SetValue(IsAddButtonVisibleProperty, value);
    }

    /// <summary>Invoked when the + button is clicked, after <see cref="AddTabButtonClick"/> and before the event's
    /// default of doing nothing.</summary>
    public ICommand? AddTabButtonCommand
    {
        get => (ICommand?)GetValue(AddTabButtonCommandProperty);
        set => SetValue(AddTabButtonCommandProperty, value);
    }

    public object? AddTabButtonCommandParameter
    {
        get => GetValue(AddTabButtonCommandParameterProperty);
        set => SetValue(AddTabButtonCommandParameterProperty, value);
    }

    /// <summary>Anything to sit left of the strip; upstream binds this to a ContentPresenter of its own.</summary>
    public object? TabStripHeader
    {
        get => GetValue(TabStripHeaderProperty);
        set => SetValue(TabStripHeaderProperty, value);
    }

    /// <summary>Anything to sit right of the strip, in the column that takes the remaining width.</summary>
    public object? TabStripFooter
    {
        get => GetValue(TabStripFooterProperty);
        set => SetValue(TabStripFooterProperty, value);
    }

    /// <summary>The selected tab's content, which the body presenter shows. Null selects an empty body.</summary>
    public object? SelectedContent => GetValue(SelectedContentProperty);

    /// <summary>Raised when the selection moves.</summary>
    public event EventHandler<FluentTabViewSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the + button is clicked. Nothing is added on its own; upstream does not either.</summary>
    public event EventHandler? AddTabButtonClick;

    /// <summary>Raised when a tab's close button or Delete key fires. The tab stays until the app removes it.</summary>
    public event EventHandler<FluentTabViewTabCloseRequestedEventArgs>? TabCloseRequested;

    /// <summary>
    /// Resolves the parts the code drives: the strip the tabs live in and the + button whose click this view
    /// reports. A template without a + button is allowed - the button's column is simply empty.
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (_addButton != null)
        {
            _addButton.Click -= OnAddButtonClick;
        }

        _strip = GetTemplateChild("TabListView") as StackPanel;
        _addButton = GetTemplateChild("AddButton") as Button;
        if (_addButton != null)
        {
            _addButton.Click += OnAddButtonClick;
        }

        RefreshStrip();
    }

    internal void OnItemsChanged(FluentTabViewItem item)
    {
        item.Owner = this;
        if (item.IsSelected && !ReferenceEquals(SelectedItem, item))
        {
            SetCurrentValue(SelectedIndexProperty, TabItems.IndexOf(item));
        }
        else if (SelectedItem == null && SelectedIndex >= 0 && SelectedIndex < TabItems.Count)
        {
            // Settle an index that arrived before this tab did.
            SetCurrentValue(SelectedItemProperty, TabItems[SelectedIndex]);
        }

        RefreshStrip();
    }

    internal void DetachItem(FluentTabViewItem item)
    {
        item.Owner = null;
        item.SetCurrentValue(FluentTabViewItem.IsSelectedProperty, false);
        item.SetNeighboursOfSelected(false, false);
        _strip?.Children.Remove(item);
        if (ReferenceEquals(SelectedItem, item))
        {
            SetCurrentValue(SelectedIndexProperty, -1);
        }
    }

    /// <summary>Puts the tabs in the strip in collection order and drops any that left it.</summary>
    internal void RefreshStrip()
    {
        if (_strip == null)
        {
            return;
        }

        for (var index = _strip.Children.Count - 1; index >= 0; index--)
        {
            if (_strip.Children[index] is not FluentTabViewItem item || !TabItems.Contains(item))
            {
                _strip.Children.RemoveAt(index);
            }
        }

        for (var index = 0; index < TabItems.Count; index++)
        {
            var item = TabItems[index];
            if (index < _strip.Children.Count && ReferenceEquals(_strip.Children[index], item))
            {
                continue;
            }

            _strip.Children.Remove(item);
            _strip.Children.Insert(index, item);
        }

        InvalidateMeasure();
    }

    internal void SelectItem(FluentTabViewItem item)
    {
        var index = TabItems.IndexOf(item);
        if (index >= 0)
        {
            SetCurrentValue(SelectedIndexProperty, index);
        }
    }

    internal void OnItemSelectedByItself(FluentTabViewItem item)
    {
        if (_syncSelection)
        {
            return;
        }

        SelectItem(item);
    }

    internal void OnCloseRequested(FluentTabViewItem item) =>
        TabCloseRequested?.Invoke(this, new FluentTabViewTabCloseRequestedEventArgs(item));

    /// <summary>
    /// Moves keyboard focus inside the strip without touching the selection: the tabs are a list whose
    /// SingleSelectionFollowsFocus is false upstream, so arrows browse and Enter, Space or a click selects.
    /// </summary>
    /// <param name="current">The tab that has focus.</param>
    /// <param name="delta">-1 and 1 step, 0 goes to the first tab and -2 to the last.</param>
    internal void MoveFocus(FluentTabViewItem current, int delta)
    {
        var enabled = TabItems.Where(item => item.IsEnabled && item.IsVisible).ToArray();
        if (enabled.Length == 0)
        {
            return;
        }

        var from = Array.IndexOf(enabled, current);
        var target = delta switch
        {
            0 => 0,
            -2 => enabled.Length - 1,
            _ when from < 0 => 0,
            _ => (from + delta + enabled.Length) % enabled.Length,
        };
        enabled[target].Focus();
    }

    private static void OnSelectedIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var view = (FluentTabView)sender;
        if (view._syncSelection)
        {
            return;
        }

        var index = (int)args.NewValue!;
        // An index may name a tab that has not arrived yet - see SelectedIndex. The value stays, and
        // OnItemsChanged settles it as soon as the tab it names exists.
        view.SetCurrentValue(SelectedItemProperty,
            index >= 0 && index < view.TabItems.Count ? view.TabItems[index] : null);
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var view = (FluentTabView)sender;
        if (view._syncSelection)
        {
            return;
        }

        var selected = (FluentTabViewItem?)args.NewValue;
        if (selected != null && selected.Owner != view)
        {
            view.SetCurrentValue(SelectedItemProperty, args.OldValue);
            throw new ArgumentException("SelectedItem must belong to this tab view.", nameof(view.SelectedItem));
        }

        view._syncSelection = true;
        try
        {
            for (var index = 0; index < view.TabItems.Count; index++)
            {
                var item = view.TabItems[index];
                var isCurrent = ReferenceEquals(item, selected);
                item.SetCurrentValue(FluentTabViewItem.IsSelectedProperty, isCurrent);
                item.SetNeighboursOfSelected(
                    selected != null && index == TabIndexOffset(view, selected, -1),
                    selected != null && index == TabIndexOffset(view, selected, 1));
            }

            view.SetValue(SelectedContentPropertyKey, selected?.Content);
        }
        finally
        {
            view._syncSelection = false;
        }

        selected?.BringIntoView();
        view.SelectionChanged?.Invoke(view,
            new FluentTabViewSelectionChangedEventArgs((FluentTabViewItem?)args.OldValue, selected));
    }

    private static int TabIndexOffset(FluentTabView view, FluentTabViewItem selected, int offset)
    {
        var index = view.TabItems.IndexOf(selected) + offset;
        return index < 0 || index >= view.TabItems.Count ? -1 : index;
    }

    private static void OnStripContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentTabView)sender).InvalidateMeasure();

    private void OnAddButtonClick(object sender, RoutedEventArgs args)
    {
        // The + column is hidden by a trigger, and a collapsed element takes no pointer input - but an automation
        // peer can still be handed the button, so the visibility gate is checked on this side too.
        if (!IsAddButtonVisible)
        {
            return;
        }

        AddTabButtonClick?.Invoke(this, EventArgs.Empty);
        if (AddTabButtonCommand is { } command && command.CanExecute(AddTabButtonCommandParameter))
        {
            command.Execute(AddTabButtonCommandParameter);
        }
    }

    private void OnViewPreviewKeyDown(object sender, KeyEventArgs args)
    {
        // WinUI 3 ships these two as TabView's own keyboard accelerators, so they change the selection from
        // anywhere inside the control rather than only while a tab holds focus.
        if (args.Key != Key.Tab || (args.KeyboardModifiers & ModifierKeys.Control) == 0)
        {
            return;
        }

        MoveSelection((args.KeyboardModifiers & ModifierKeys.Shift) != 0 ? -1 : 1);
        args.Handled = true;
    }

    /// <summary>
    /// Moves the selection to the next or previous enabled tab, wrapping at either end. Upstream reaches here from
    /// its Ctrl+Tab and Ctrl+Shift+Tab accelerators; the step is exposed so the cycle is testable without a
    /// keyboard event, which this build cannot synthesize (spike/TabViewProbe section C).
    /// </summary>
    internal void MoveSelection(int step)
    {
        var enabled = TabItems.Where(item => item.IsEnabled).ToArray();
        if (enabled.Length == 0)
        {
            return;
        }

        var from = Array.FindIndex(enabled, item => item.IsSelected);
        var target = (from + step + enabled.Length) % enabled.Length;
        SelectItem(enabled[target]);
        enabled[target].BringIntoView();
    }
}
