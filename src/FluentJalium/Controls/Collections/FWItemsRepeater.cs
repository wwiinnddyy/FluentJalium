using Jalium.UI;
using Jalium.UI.Controls;
using System.Collections;
using System.Collections.Specialized;

namespace FluentJalium.Controls;

/// <summary>
/// Describes how <see cref="FWItemsRepeater"/> chooses the item indices that are currently realized.
/// </summary>
public enum FWItemsRepeaterRealizationMode
{
    /// <summary>
    /// All items from the current source are realized.
    /// </summary>
    All,

    /// <summary>
    /// Only the explicitly requested item range is realized.
    /// </summary>
    Range
}

/// <summary>
/// Snapshot of realized item and recycle-pool state for <see cref="FWItemsRepeater"/>.
/// </summary>
public readonly record struct FWItemsRepeaterDiagnostics(
    FWItemsRepeaterRealizationMode RealizationMode,
    int ItemCount,
    int RealizedElementCount,
    int RecycledElementCount,
    int FirstRealizedIndex,
    int LastRealizedIndex,
    int RequestedFirstRealizedIndex,
    int RequestedRealizedItemCount,
    double HorizontalCacheLength,
    double VerticalCacheLength,
    int LastCreatedElementCount,
    int LastReusedElementCount)
{
    /// <summary>
    /// Gets a value indicating whether the repeater has any realized elements.
    /// </summary>
    public bool HasRealizedElements => RealizedElementCount > 0;

    /// <summary>
    /// Gets a value indicating whether the repeater currently has reusable elements in its recycle pool.
    /// </summary>
    public bool HasRecycledElements => RecycledElementCount > 0;
}

/// <summary>
/// FluentJalium ItemsRepeater control for flexible, virtualizing list layouts.
/// </summary>
public class FWItemsRepeater : Panel, IFluentJaliumControl
{
    private readonly List<object?> _items = new();
    private readonly Dictionary<int, FrameworkElement> _realizedElements = new();
    private readonly Stack<FrameworkElement> _recyclePool = new();
    private int _realizationStartIndex;
    private int _realizationItemCount = int.MaxValue;
    private int _lastCreatedElementCount;
    private int _lastReusedElementCount;
    private bool _hasRealizationRange;

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FWItemsRepeater),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FWItemsRepeater),
            new PropertyMetadata(null, OnItemTemplateChanged));

    public static readonly DependencyProperty LayoutProperty =
        DependencyProperty.Register(nameof(Layout), typeof(VirtualizingLayout), typeof(FWItemsRepeater),
            new PropertyMetadata(null, OnLayoutChanged));

    public static readonly DependencyProperty HorizontalCacheLengthProperty =
        DependencyProperty.Register(nameof(HorizontalCacheLength), typeof(double), typeof(FWItemsRepeater),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty VerticalCacheLengthProperty =
        DependencyProperty.Register(nameof(VerticalCacheLength), typeof(double), typeof(FWItemsRepeater),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty AnimatorProperty =
        DependencyProperty.Register(nameof(Animator), typeof(ElementAnimator), typeof(FWItemsRepeater),
            new PropertyMetadata(null));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWItemsRepeater"/> class.
    /// </summary>
    public FWItemsRepeater()
    {
    }

    /// <summary>
    /// Gets or sets the data source for items.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to display each item.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the layout strategy for arranging items.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public VirtualizingLayout? Layout
    {
        get => (VirtualizingLayout?)GetValue(LayoutProperty);
        set => SetValue(LayoutProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal cache length for virtualization.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double HorizontalCacheLength
    {
        get => (double)GetValue(HorizontalCacheLengthProperty)!;
        set => SetValue(HorizontalCacheLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical cache length for virtualization.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public double VerticalCacheLength
    {
        get => (double)GetValue(VerticalCacheLengthProperty)!;
        set => SetValue(VerticalCacheLengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the animator for element transitions.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public ElementAnimator? Animator
    {
        get => (ElementAnimator?)GetValue(AnimatorProperty);
        set => SetValue(AnimatorProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public FWItemsRepeaterRealizationMode RealizationMode =>
        _hasRealizationRange ? FWItemsRepeaterRealizationMode.Range : FWItemsRepeaterRealizationMode.All;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int ItemCount => _items.Count;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int RealizedElementCount => _realizedElements.Count;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int RecycledElementCount => _recyclePool.Count;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int FirstRealizedIndex => GetFirstRealizedIndex();

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int LastRealizedIndex => GetLastRealizedIndex();

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int RequestedFirstRealizedIndex => _hasRealizationRange ? _realizationStartIndex : 0;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int RequestedRealizedItemCount => _hasRealizationRange ? _realizationItemCount : _items.Count;

    /// <summary>
    /// Gets the realized element at the specified item index.
    /// </summary>
    public FrameworkElement? TryGetElement(int index)
    {
        return _realizedElements.TryGetValue(index, out var element) ? element : null;
    }

    /// <summary>
    /// Gets the item index represented by the specified realized element.
    /// </summary>
    public int GetElementIndex(FrameworkElement element)
    {
        foreach (var pair in _realizedElements)
        {
            if (ReferenceEquals(pair.Value, element))
            {
                return pair.Key;
            }
        }

        return -1;
    }

    /// <summary>
    /// Realizes only the requested item index window while recycling existing elements.
    /// </summary>
    public void RealizeRange(int startIndex, int itemCount)
    {
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index cannot be negative.");
        }

        if (itemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemCount), "Item count cannot be negative.");
        }

        _hasRealizationRange = true;
        _realizationStartIndex = startIndex;
        _realizationItemCount = itemCount;
        RefreshItems();
    }

    /// <summary>
    /// Returns to realizing the full item set.
    /// </summary>
    public void ResetRealizationWindow()
    {
        if (!_hasRealizationRange)
        {
            return;
        }

        _hasRealizationRange = false;
        _realizationStartIndex = 0;
        _realizationItemCount = int.MaxValue;
        RefreshItems();
    }

    /// <summary>
    /// Gets a snapshot of realization, recycle-pool, and cache diagnostics.
    /// </summary>
    public FWItemsRepeaterDiagnostics GetDiagnostics()
    {
        return new FWItemsRepeaterDiagnostics(
            RealizationMode,
            ItemCount,
            RealizedElementCount,
            RecycledElementCount,
            FirstRealizedIndex,
            LastRealizedIndex,
            RequestedFirstRealizedIndex,
            RequestedRealizedItemCount,
            HorizontalCacheLength,
            VerticalCacheLength,
            _lastCreatedElementCount,
            _lastReusedElementCount);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Layout != null)
        {
            return Layout.Measure(this, availableSize);
        }

        return MeasureDefault(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Layout != null)
        {
            return Layout.Arrange(this, finalSize);
        }

        return ArrangeDefault(finalSize);
    }

    private Size MeasureDefault(Size availableSize)
    {
        double totalHeight = 0;
        double maxWidth = 0;

        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
            totalHeight += child.DesiredSize.Height;
            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
        }

        return new Size(maxWidth, totalHeight);
    }

    private Size ArrangeDefault(Size finalSize)
    {
        double y = 0;

        foreach (UIElement child in Children)
        {
            var childSize = child.DesiredSize;
            child.Arrange(new Rect(0, y, finalSize.Width, childSize.Height));
            y += childSize.Height;
        }

        return finalSize;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWItemsRepeater repeater)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= repeater.OnItemsCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += repeater.OnItemsCollectionChanged;
            }

            repeater.RefreshItems();
        }
    }

    private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWItemsRepeater repeater)
        {
            repeater.RefreshItems(reuseExistingElements: false);
        }
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWItemsRepeater repeater)
        {
            repeater.InvalidateMeasure();
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshItems();
    }

    private void RefreshItems()
    {
        RefreshItems(reuseExistingElements: true);
    }

    private void RefreshItems(bool reuseExistingElements)
    {
        SnapshotItems();
        if (reuseExistingElements)
        {
            RecycleRealizedElements();
        }
        else
        {
            DiscardRealizedElements();
            _recyclePool.Clear();
        }

        _lastCreatedElementCount = 0;
        _lastReusedElementCount = 0;

        if (_items.Count == 0 || ItemTemplate == null)
        {
            InvalidateMeasure();
            InvalidateVisual();
            return;
        }

        var (startIndex, endIndex) = GetRealizationRange();
        for (var index = startIndex; index < endIndex; index++)
        {
            var element = GetOrCreateElement();
            if (element == null)
            {
                continue;
            }

            element.DataContext = _items[index];
            Children.Add(element);
            _realizedElements[index] = element;
            Animator?.OnElementShown(element);
        }

        InvalidateMeasure();
        InvalidateVisual();
    }

    private void SnapshotItems()
    {
        _items.Clear();
        if (ItemsSource == null)
        {
            return;
        }

        foreach (var item in ItemsSource)
        {
            _items.Add(item);
        }
    }

    private (int StartIndex, int EndIndex) GetRealizationRange()
    {
        if (!_hasRealizationRange)
        {
            return (0, _items.Count);
        }

        var startIndex = Math.Min(_realizationStartIndex, _items.Count);
        var realizedCount = Math.Min(_realizationItemCount, _items.Count - startIndex);
        return (startIndex, startIndex + realizedCount);
    }

    private FrameworkElement? GetOrCreateElement()
    {
        if (_recyclePool.Count > 0)
        {
            _lastReusedElementCount++;
            return _recyclePool.Pop();
        }

        var element = ItemTemplate?.LoadContent() as FrameworkElement;
        if (element != null)
        {
            _lastCreatedElementCount++;
        }

        return element;
    }

    private void RecycleRealizedElements()
    {
        foreach (var element in _realizedElements.Values)
        {
            Animator?.OnElementHidden(element);
            _recyclePool.Push(element);
        }

        Children.Clear();
        _realizedElements.Clear();
    }

    private void DiscardRealizedElements()
    {
        foreach (var element in _realizedElements.Values)
        {
            Animator?.OnElementHidden(element);
        }

        Children.Clear();
        _realizedElements.Clear();
    }

    private int GetFirstRealizedIndex()
    {
        if (_realizedElements.Count == 0)
        {
            return -1;
        }

        var first = int.MaxValue;
        foreach (var index in _realizedElements.Keys)
        {
            if (index < first)
            {
                first = index;
            }
        }

        return first;
    }

    private int GetLastRealizedIndex()
    {
        if (_realizedElements.Count == 0)
        {
            return -1;
        }

        var last = int.MinValue;
        foreach (var index in _realizedElements.Keys)
        {
            if (index > last)
            {
                last = index;
            }
        }

        return last;
    }
}

/// <summary>
/// Base class for virtualizing layout strategies.
/// </summary>
public abstract class VirtualizingLayout
{
    /// <summary>
    /// Measures the layout with the given available size.
    /// </summary>
    public abstract Size Measure(FWItemsRepeater context, Size availableSize);

    /// <summary>
    /// Arranges child elements in the final size.
    /// </summary>
    public abstract Size Arrange(FWItemsRepeater context, Size finalSize);
}

/// <summary>
/// Stack layout for ItemsRepeater.
/// </summary>
public class StackLayout : VirtualizingLayout
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public double Spacing { get; set; } = 0;

    public override Size Measure(FWItemsRepeater context, Size availableSize)
    {
        double totalPrimary = 0;
        double maxSecondary = 0;
        bool isHorizontal = Orientation == Orientation.Horizontal;

        foreach (UIElement child in context.Children)
        {
            child.Measure(availableSize);
            var size = child.DesiredSize;

            if (isHorizontal)
            {
                totalPrimary += size.Width;
                maxSecondary = Math.Max(maxSecondary, size.Height);
            }
            else
            {
                totalPrimary += size.Height;
                maxSecondary = Math.Max(maxSecondary, size.Width);
            }
        }

        // Add spacing
        var count = context.Children.Count;
        if (count > 1)
        {
            totalPrimary += Spacing * (count - 1);
        }

        return isHorizontal
            ? new Size(totalPrimary, maxSecondary)
            : new Size(maxSecondary, totalPrimary);
    }

    public override Size Arrange(FWItemsRepeater context, Size finalSize)
    {
        double position = 0;
        bool isHorizontal = Orientation == Orientation.Horizontal;

        foreach (UIElement child in context.Children)
        {
            var size = child.DesiredSize;

            if (isHorizontal)
            {
                child.Arrange(new Rect(position, 0, size.Width, finalSize.Height));
                position += size.Width + Spacing;
            }
            else
            {
                child.Arrange(new Rect(0, position, finalSize.Width, size.Height));
                position += size.Height + Spacing;
            }
        }

        return finalSize;
    }
}

/// <summary>
/// Uniform grid layout for ItemsRepeater.
/// </summary>
public class UniformGridLayout : VirtualizingLayout
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public double MinItemWidth { get; set; } = 100;
    public double MinItemHeight { get; set; } = 100;
    public double MinColumnSpacing { get; set; } = 0;
    public double MinRowSpacing { get; set; } = 0;
    public int MaximumRowsOrColumns { get; set; } = -1;

    public override Size Measure(FWItemsRepeater context, Size availableSize)
    {
        var childCount = context.Children.Count;
        if (childCount == 0)
            return new Size(0, 0);

        bool isVertical = Orientation == Orientation.Vertical;
        double availablePrimary = isVertical ? availableSize.Width : availableSize.Height;
        double itemPrimary = isVertical ? MinItemWidth : MinItemHeight;
        double itemSecondary = isVertical ? MinItemHeight : MinItemWidth;
        double spacing = isVertical ? MinColumnSpacing : MinRowSpacing;

        // Calculate columns/rows
        int columnsOrRows = MaximumRowsOrColumns > 0
            ? MaximumRowsOrColumns
            : Math.Max(1, (int)((availablePrimary + spacing) / (itemPrimary + spacing)));

        int rowsOrColumns = (int)Math.Ceiling((double)childCount / columnsOrRows);

        double totalPrimary = columnsOrRows * itemPrimary + (columnsOrRows - 1) * spacing;
        double totalSecondary = rowsOrColumns * itemSecondary + (rowsOrColumns - 1) * (isVertical ? MinRowSpacing : MinColumnSpacing);

        var childSize = isVertical
            ? new Size(itemPrimary, itemSecondary)
            : new Size(itemSecondary, itemPrimary);

        foreach (UIElement child in context.Children)
        {
            child.Measure(childSize);
        }

        return isVertical
            ? new Size(totalPrimary, totalSecondary)
            : new Size(totalSecondary, totalPrimary);
    }

    public override Size Arrange(FWItemsRepeater context, Size finalSize)
    {
        var childCount = context.Children.Count;
        if (childCount == 0)
            return finalSize;

        bool isVertical = Orientation == Orientation.Vertical;
        double availablePrimary = isVertical ? finalSize.Width : finalSize.Height;
        double itemPrimary = isVertical ? MinItemWidth : MinItemHeight;
        double itemSecondary = isVertical ? MinItemHeight : MinItemWidth;
        double primarySpacing = isVertical ? MinColumnSpacing : MinRowSpacing;
        double secondarySpacing = isVertical ? MinRowSpacing : MinColumnSpacing;

        int columnsOrRows = MaximumRowsOrColumns > 0
            ? MaximumRowsOrColumns
            : Math.Max(1, (int)((availablePrimary + primarySpacing) / (itemPrimary + primarySpacing)));

        int index = 0;
        foreach (UIElement child in context.Children)
        {
            int row = index / columnsOrRows;
            int col = index % columnsOrRows;

            double x = col * (itemPrimary + primarySpacing);
            double y = row * (itemSecondary + secondarySpacing);

            var rect = isVertical
                ? new Rect(x, y, itemPrimary, itemSecondary)
                : new Rect(y, x, itemSecondary, itemPrimary);

            child.Arrange(rect);
            index++;
        }

        return finalSize;
    }
}

/// <summary>
/// Base class for element animators.
/// </summary>
public abstract class ElementAnimator
{
    /// <summary>
    /// Called when an element is about to be shown.
    /// </summary>
    public abstract void OnElementShown(FrameworkElement element);

    /// <summary>
    /// Called when an element is about to be hidden.
    /// </summary>
    public abstract void OnElementHidden(FrameworkElement element);
}
