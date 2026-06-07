using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Controls.Primitives;
using Jalium.UI.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium BreadcrumbBar control for hierarchical navigation.
/// </summary>
public class FWBreadcrumbBar : Control, IFluentJaliumControl
{
    private ItemsControl? _itemsControl;

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(FWBreadcrumbBar),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(FWBreadcrumbBar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWBreadcrumbBar),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public static readonly DependencyProperty MaxItemsProperty =
        DependencyProperty.Register(nameof(MaxItems), typeof(int), typeof(FWBreadcrumbBar),
            new PropertyMetadata(5));

    public static readonly RoutedEvent ItemClickedEvent =
        EventManager.RegisterRoutedEvent(nameof(ItemClicked), RoutingStrategy.Bubble,
            typeof(ItemClickEventHandler), typeof(FWBreadcrumbBar));

    /// <summary>
    /// Initializes a new instance of the <see cref="FWBreadcrumbBar"/> class.
    /// </summary>
    public FWBreadcrumbBar()
    {
        ApplyDensity(this, Density);
    }

    /// <summary>
    /// Gets or sets the items source for the breadcrumb trail.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the data template for breadcrumb items.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the density preset for the breadcrumb bar.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum number of items to display before collapsing.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public int MaxItems
    {
        get => (int)GetValue(MaxItemsProperty)!;
        set => SetValue(MaxItemsProperty, value);
    }

    /// <summary>
    /// Occurs when a breadcrumb item is clicked.
    /// </summary>
    public event ItemClickEventHandler ItemClicked
    {
        add => AddHandler(ItemClickedEvent, value);
        remove => RemoveHandler(ItemClickedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild("PART_ItemsControl") as ItemsControl;
        UpdateItems();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWBreadcrumbBar breadcrumbBar)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= breadcrumbBar.OnItemsCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += breadcrumbBar.OnItemsCollectionChanged;
            }

            breadcrumbBar.UpdateItems();
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateItems();
    }

    private void UpdateItems()
    {
        if (_itemsControl == null)
            return;

        _itemsControl.Items.Clear();

        if (ItemsSource == null)
            return;

        var items = new List<object>();
        foreach (var item in ItemsSource)
        {
            items.Add(item);
        }

        // If we have too many items, collapse the middle ones
        if (items.Count > MaxItems && MaxItems > 2)
        {
            // Keep first item, add ellipsis, keep last (MaxItems - 2) items
            _itemsControl.Items.Add(CreateBreadcrumbItem(items[0], 0));
            _itemsControl.Items.Add(CreateEllipsisItem());

            int startIndex = items.Count - (MaxItems - 2);
            for (int i = startIndex; i < items.Count; i++)
            {
                _itemsControl.Items.Add(CreateBreadcrumbItem(items[i], i));
            }
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                _itemsControl.Items.Add(CreateBreadcrumbItem(items[i], i));
            }
        }
    }

    private FrameworkElement CreateBreadcrumbItem(object data, int index)
    {
        var button = new Button
        {
            Content = data,
            ContentTemplate = ItemTemplate,
            Tag = index
        };

        button.Click += OnBreadcrumbItemClick;

        return button;
    }

    private FrameworkElement CreateEllipsisItem()
    {
        return new Button
        {
            Content = "...",
            IsEnabled = false
        };
    }

    private void OnBreadcrumbItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var args = new ItemClickEventArgs(ItemClickedEvent, this)
            {
                ClickedItem = button.Content,
                Index = (int)button.Tag!
            };
            RaiseEvent(args);
        }
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWBreadcrumbBar breadcrumbBar && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(breadcrumbBar, density);
        }
    }

    private static void ApplyDensity(FWBreadcrumbBar breadcrumbBar, FWNavigationDensity density)
    {
        var (minHeight, padding) = GetDensityMetrics(density);
        breadcrumbBar.MinHeight = minHeight;
        breadcrumbBar.Padding = padding;
    }

    internal static (double MinHeight, Thickness Padding) GetDensityMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(8, 4, 8, 4)),
            FWNavigationDensity.Spacious => (44.0, new Thickness(12, 8, 12, 8)),
            _ => (36.0, new Thickness(10, 6, 10, 6))
        };
    }
}

/// <summary>
/// Delegate for breadcrumb item click events.
/// </summary>
public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

/// <summary>
/// Event arguments for breadcrumb item click.
/// </summary>
public class ItemClickEventArgs : RoutedEventArgs
{
    public ItemClickEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    /// <summary>
    /// Gets the clicked item data.
    /// </summary>
    public object? ClickedItem { get; init; }

    /// <summary>
    /// Gets the index of the clicked item.
    /// </summary>
    public int Index { get; init; }
}
