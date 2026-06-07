using Jalium.UI;
using Jalium.UI.Controls;
using System.Collections;
using System.Collections.Specialized;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium ItemsRepeater control for flexible, virtualizing list layouts.
/// </summary>
public class FWItemsRepeater : Panel, IFluentJaliumControl
{
    private IList? _realizedElements;

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
        _realizedElements = new List<FrameworkElement>();
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

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    public FrameworkElement? TryGetElement(int index)
    {
        if (_realizedElements != null && index >= 0 && index < _realizedElements.Count)
        {
            return _realizedElements[index] as FrameworkElement;
        }
        return null;
    }

    /// <summary>
    /// Gets the index of the element for the specified item.
    /// </summary>
    public int GetElementIndex(FrameworkElement element)
    {
        return _realizedElements?.IndexOf(element) ?? -1;
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
            repeater.RefreshItems();
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
        Children.Clear();
        _realizedElements?.Clear();

        if (ItemsSource == null || ItemTemplate == null)
            return;

        foreach (var item in ItemsSource)
        {
            var element = ItemTemplate.LoadContent() as FrameworkElement;
            if (element != null)
            {
                element.DataContext = item;
                Children.Add(element);
                _realizedElements?.Add(element);
            }
        }

        InvalidateMeasure();
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
