using System.Collections;
using System.Collections.Specialized;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;

namespace FluentJalium.Controls;

/// <summary>
/// A list of mutually exclusive choices in one or more columns, with a header: WinUI's
/// <c>RadioButtons</c> shape. Items become real <see cref="RadioButton"/> controls, and the choice is read back
/// from <see cref="SelectedItem"/> rather than from a selection chrome of its own.
/// </summary>
/// <remarks>
/// <para>
/// Own type because 26.10.9 exports no <c>RadioButtons</c> at all (spike/ItemHostProbe section A counts the name
/// at 0 hits), and upstream's own base is <c>Control</c> plus an <c>ItemsRepeater</c> whose element factory hands
/// back a <see cref="RadioButton"/> (<c>RadioButtons.idl:9</c>, <c>RadioButtonsElementFactory.cpp:56-87</c>) -
/// the runtime has no repeater to copy that from. What it does have is the same pipeline behind
/// <see cref="ItemsControl"/>'s protected overrides, measured in spike/ItemHostProbe sections B and F:
/// a derived host that generates <c>RadioButton</c> containers realizes them through our own template, our
/// implicit <c>RadioButton</c> style reaches those generated containers, and radios under one items host exclude
/// each other with an empty <c>GroupName</c>. Upstream's only template-part lookup is its repeater
/// (<c>RadioButtons.cpp:61</c>), which this host has no need of.
/// </para>
/// <para>
/// The column layout is <see cref="FluentRadioButtonsPanel"/>, whose cells are uniform at the largest item and
/// fill down the first column like upstream's <c>ColumnMajorUniformToLargestGridLayout</c>. What it does not have
/// is upstream's virtualization or its to-largest column widths, and the two spacing metrics are the panel's own
/// defaults rather than theme rows - <c>x:Double</c> rows cannot be published by this reader (adaptation/00 S0-e).
/// docs/astra/audits/radio-buttons.md keeps the full deviation list.
/// </para>
/// </remarks>
public class FluentRadioButtons : ItemsControl
{
    private const string DefaultStyleResourceKey = "DefaultRadioButtonsStyle";
    private const string HeaderPartName = "HeaderContentPresenter";

    private static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(FluentRadioButtons));

    private readonly Dictionary<RadioButton, int> _containerIndexes = [];

    private Style? _appliedDefaultStyle;
    private ContentPresenter? _headerPresenter;
    private bool _updatingSelection;

    /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(FluentRadioButtons),
        new PropertyMetadata(null, OnHeaderChanged));

    /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
        nameof(HeaderTemplate), typeof(DataTemplate), typeof(FluentRadioButtons), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="MaxColumns"/> dependency property.</summary>
    public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register(
        nameof(MaxColumns), typeof(int), typeof(FluentRadioButtons),
        new PropertyMetadata(1, OnMaxColumnsChanged));

    /// <summary>Identifies the <see cref="SelectedIndex"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
        nameof(SelectedIndex), typeof(int), typeof(FluentRadioButtons),
        new PropertyMetadata(-1, OnSelectedIndexChanged));

    /// <summary>Identifies the <see cref="SelectedItem"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(FluentRadioButtons),
        new PropertyMetadata(null, OnSelectedItemChanged));

    /// <summary>Creates a radio-button list and applies its named style when one is available.</summary>
    public FluentRadioButtons()
    {
        ApplyDefaultStyle();
        Loaded += (_, _) => ApplyDefaultStyle();
        UpdatePanel();
    }

    /// <summary>Raised when the chosen item changes, from a pointer, the keyboard or code alike.</summary>
    public event SelectionChangedEventHandler? SelectionChanged
    {
        add
        {
            if (value is not null)
            {
                AddHandler(SelectionChangedEvent, value);
            }
        }
        remove
        {
            if (value is not null)
            {
                RemoveHandler(SelectionChangedEvent, value);
            }
        }
    }

    /// <summary>Gets or sets the content shown above the choices.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Gets or sets the template used for <see cref="Header"/>.</summary>
    public DataTemplate? HeaderTemplate
    {
        get => (DataTemplate?)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>Gets or sets how many columns the choices may spread over; below one counts as one.</summary>
    public int MaxColumns
    {
        get => (int)GetValue(MaxColumnsProperty)!;
        set => SetValue(MaxColumnsProperty, value);
    }

    /// <summary>Gets or sets the position of the chosen item, or -1 while nothing is chosen.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty)!;
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Gets or sets the chosen item, or <c>null</c> while nothing is chosen.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets the radio button that carries the item at <paramref name="index"/>, or <c>null</c> while that item
    /// has no container. Upstream's <c>ContainerFromIndex</c> (<c>RadioButtons.idl:36</c>) answers the same
    /// question, and the containers here are generated rather than supplied, so this is where a test or an app
    /// reaches the visual that stands for an item.
    /// </summary>
    public RadioButton? ContainerFromIndex(int index)
    {
        foreach (var pair in _containerIndexes)
        {
            if (pair.Value == index)
            {
                return pair.Key;
            }
        }

        return null;
    }

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is RadioButton;

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride() => new RadioButton();

    /// <inheritdoc/>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not RadioButton radio)
        {
            return;
        }

        radio.Content ??= item;
        var index = Items.IndexOf(item);
        radio.Checked += OnContainerChecked;
        _containerIndexes[radio] = index;

        // The host owns the choice, so a container that appears after the selection - the usual order when an
        // ItemsSource is assigned before the panel realizes - has to arrive already checked.
        radio.IsChecked = index >= 0 && index == SelectedIndex;
    }

    /// <inheritdoc/>
    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);
        if (element is RadioButton radio)
        {
            radio.Checked -= OnContainerChecked;
            _containerIndexes.Remove(radio);
        }
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs args)
    {
        base.OnItemsChanged(args);

        // Positions shift when the source changes, so the map from container to index is rebuilt by the next
        // prepare pass rather than patched here.
        _containerIndexes.Clear();
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _headerPresenter = GetTemplateChild(HeaderPartName) as ContentPresenter;
        UpdateHeaderVisibility();
    }

    private static void OnHeaderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentRadioButtons)sender).UpdateHeaderVisibility();

    private static void OnMaxColumnsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((FluentRadioButtons)sender).UpdatePanel();

    private static void OnSelectedIndexChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var host = (FluentRadioButtons)sender;
        var index = (int)(args.NewValue ?? -1);
        if (index < 0)
        {
            host.SetSelection(-1, null);
            return;
        }

        host.SetSelection(index, index < host.Items.Count ? host.Items[index] : null);
    }

    private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var host = (FluentRadioButtons)sender;
        host.SetSelection(host.Items.IndexOf(args.NewValue!), args.NewValue);
    }

    private void OnContainerChecked(object? sender, RoutedEventArgs args)
    {
        if (_updatingSelection || sender is not RadioButton radio ||
            !_containerIndexes.TryGetValue(radio, out var index))
        {
            return;
        }

        // SetCurrentValue keeps a pointer check, a keyboard arrow and an app's assignment on the same path
        // without the framework reading it as a local write that survives a later style change.
        SetCurrentValue(SelectedIndexProperty, index);
    }

    private void SetSelection(int index, object? item)
    {
        if (_updatingSelection)
        {
            return;
        }

        _updatingSelection = true;
        try
        {
            var previous = SelectedItem;
            if (!Equals(SelectedItem, item))
            {
                SetCurrentValue(SelectedItemProperty, item);
            }

            if (SelectedIndex != index)
            {
                SetCurrentValue(SelectedIndexProperty, index);
            }

            SyncCheckedState();
            var added = item is null ? Array.Empty<object?>() : new object?[] { item };

            // Nothing was chosen before the first choice, so there is nothing to report as removed: a null in this
            // list would read to a caller as "an item called null was unselected".
            var removed = previous is null || Equals(previous, item)
                ? Array.Empty<object?>()
                : new object?[] { previous };
            RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, (IList)removed, (IList)added)
            {
                Source = this,
            });
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    private void SyncCheckedState()
    {
        var selected = SelectedIndex;
        foreach (var pair in _containerIndexes)
        {
            pair.Key.IsChecked = pair.Value == selected;
        }
    }

    private void UpdatePanel()
    {
        // A new template instance is how a panel change is announced here: the panel reads MaxColumns when it is
        // built, and ItemsPanelTemplate carries nothing but the panel type, so re-assigning is what re-runs it.
        ItemsPanel = new ItemsPanelTemplate { PanelType = typeof(FluentRadioButtonsPanel) };
    }

    private void UpdateHeaderVisibility()
    {
        if (_headerPresenter is not null)
        {
            // Upstream hides the same presenter through IsHidden bound to the header (RadioButtons.xaml:21); a
            // ContentPresenter here keeps its margin even with null content, so the visibility has to be set
            // rather than left to the empty content.
            _headerPresenter.Visibility = Header is null ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void ApplyDefaultStyle()
    {
        // Same fallback as FluentDropDownButton: an implicit style for a derived type is not guaranteed to
        // resolve, while an application-assigned style - including an explicit null - is never replaced.
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
