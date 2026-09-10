using System.Collections.Specialized;
using FluentJalium.Controls.Themes;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using System.Collections.ObjectModel;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium native NavigationViewItem - 100% custom implementation following WinUI 3,
/// ModernWPF, and WPF-UI Fluent Design Systems.
///
/// Features:
/// - 36px / 40px minimum item height with pill inset and corner radius.
/// - Selection indicator: 3x16 / 3x20 accent pill on the left edge, vertically centered.
/// - Compact mode: automatically hides content text, chevron, and nested children to cleanly display 48px icons.
/// - Multi-mode layout support via <see cref="ItemStyle"/> (<see cref="FluentNavigationItemStyle.Tree"/> and <see cref="FluentNavigationItemStyle.Fluent"/>).
/// - 31px階梯式子项缩进与箭头 180° 旋转。
/// </summary>
public class FluentNavigationViewItem : Control
{
    // WinUI metrics (NavigationView_themeresources.xaml / NavigationViewItemBase.h)
    internal const double ItemMinHeight = 36;
    internal const double FluentItemMinHeight = 40;
    internal const double IconBoxWidth = 40;
    internal const double IconSize = 16;
    internal const double SelectionIndicatorWidth = 3;
    internal const double SelectionIndicatorHeight = 16;
    internal const double FluentSelectionIndicatorHeight = 20;
    internal const double SelectionIndicatorRadius = 2;
    internal const double ChildIndentation = 31;
    internal const double ChevronHostWidth = 36;
    internal const double ChevronGlyphSize = 12;
    internal const double TextRightInset = 12;

    private static readonly Thickness s_treeLayoutRootMargin = new(4, 2, 4, 2);
    private static readonly Thickness s_fluentLayoutRootMargin = new(6, 2, 6, 2);

    private StackPanel? _rootPanel;
    private Border? _layoutRoot;
    private Grid? _contentGrid;
    private Border? _selectionIndicator;
    private ContentPresenter? _iconPresenter;
    private TextBlock? _contentTextBlock;
    private TextBlock? _chevronText;
    private Border? _chevronHost;
    private StackPanel? _childrenPanel;
    private bool _isMouseOver;
    private bool _isPressed;
    private bool _isSelected;
    private bool _isCompact;
    private FluentNavigationItemStyle _itemStyle = FluentNavigationItemStyle.Tree;

    public ObservableCollection<FluentNavigationViewItem> MenuItems { get; } = new();

    private FluentNavigationView? _parentNavigationView;

    internal FluentNavigationView? ParentNavigationView
    {
        get => _parentNavigationView;
        set
        {
            _parentNavigationView = value;
            if (value != null)
            {
                ItemStyle = value.ItemStyle;
            }
            foreach (var child in MenuItems)
            {
                child.ParentNavigationView = value;
            }
        }
    }

    /// <summary>Hierarchy depth; indents the content grid by 31px per level.</summary>
    internal int Depth { get; set; }

    #region Dependency Properties

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(FluentNavigationViewItem),
            new PropertyMetadata(null, OnIconChanged));

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(FluentNavigationViewItem),
            new PropertyMetadata(null, OnContentPropertyChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(FluentNavigationViewItem),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(FluentNavigationViewItem),
            new PropertyMetadata(false, OnIsExpandedChanged));

    public static readonly DependencyProperty SelectsOnInvokedProperty =
        DependencyProperty.Register(nameof(SelectsOnInvoked), typeof(bool), typeof(FluentNavigationViewItem),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ItemStyleProperty =
        DependencyProperty.Register(nameof(ItemStyle), typeof(FluentNavigationItemStyle), typeof(FluentNavigationViewItem),
            new PropertyMetadata(FluentNavigationItemStyle.Tree, OnItemStyleChanged));

    #endregion

    #region Properties

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty)!;
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty)!;
        set => SetValue(IsExpandedProperty, value);
    }

    public bool SelectsOnInvoked
    {
        get => (bool)GetValue(SelectsOnInvokedProperty)!;
        set => SetValue(SelectsOnInvokedProperty, value);
    }

    public FluentNavigationItemStyle ItemStyle
    {
        get => (FluentNavigationItemStyle)GetValue(ItemStyleProperty)!;
        set => SetValue(ItemStyleProperty, value);
    }

    public new object? Tag { get; set; }

    /// <summary>
    /// Raised when an item with children is invoked.
    /// </summary>
    public event EventHandler? Invoked;

    #endregion

    public FluentNavigationViewItem()
    {
        MinHeight = ItemMinHeight;
        Margin = new Thickness(0);
        Cursor = Cursors.Hand;

        BuildVisualTree();
        MenuItems.CollectionChanged += OnMenuItemsChanged;
        FluentThemeManager.ThemeChanged += OnThemeChanged;
        Unloaded += OnUnloaded;
        ApplyItemStyle(ItemStyle);
        UpdateVisualState();
    }

    private void BuildVisualTree()
    {
        // Standard horizontal content grid: [40px icon box] [* text] [Auto chevron]
        _contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(IconBoxWidth) },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Margin = new Thickness(0, 0, TextRightInset, 0)
        };

        _iconPresenter = new ContentPresenter
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_iconPresenter, 0);
        _contentGrid.Children.Add(_iconPresenter);

        _contentTextBlock = new TextBlock
        {
            FontSize = 14,
            FontFamily = FluentThemeManager.CurrentBodyFontFamily,
            Foreground = ResolveBrush("NavigationViewItemForeground"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(_contentTextBlock, 1);
        _contentGrid.Children.Add(_contentTextBlock);

        _chevronText = new TextBlock
        {
            Text = "\uE70D",
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = ChevronGlyphSize,
            Foreground = ResolveBrush("NavigationViewItemForeground"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = new Point(0.5, 0.5)
        };
        _chevronHost = new Border
        {
            Width = ChevronHostWidth - TextRightInset,
            Child = _chevronText,
            Visibility = Visibility.Collapsed,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_chevronHost, 2);
        _contentGrid.Children.Add(_chevronHost);

        // Selection indicator: 3x16 (or 3x20 in Fluent) accent pill
        _selectionIndicator = new Border
        {
            Width = SelectionIndicatorWidth,
            Height = SelectionIndicatorHeight,
            CornerRadius = new CornerRadius(SelectionIndicatorRadius),
            Background = ResolveBrush("NavigationViewSelectionIndicatorForeground"),
            Opacity = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        var overlayGrid = new Grid();
        overlayGrid.Children.Add(_contentGrid);
        overlayGrid.Children.Add(_selectionIndicator);

        _layoutRoot = new Border
        {
            Margin = s_treeLayoutRootMargin,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(Colors.Transparent),
            Child = overlayGrid
        };

        _childrenPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Visibility = Visibility.Collapsed
        };

        _rootPanel = new StackPanel { Orientation = Orientation.Vertical };
        _rootPanel.Children.Add(_layoutRoot);
        _rootPanel.Children.Add(_childrenPanel);

        AddVisualChild(_rootPanel);

        MouseEnter += OnMouseEnterHandler;
        MouseLeave += OnMouseLeaveHandler;
        MouseLeftButtonDown += OnMouseLeftButtonDownHandler;
        MouseLeftButtonUp += OnMouseLeftButtonUpHandler;
    }

    private void ApplyItemStyle(FluentNavigationItemStyle style)
    {
        _itemStyle = style;
        if (_layoutRoot == null || _selectionIndicator == null) return;

        switch (style)
        {
            case FluentNavigationItemStyle.Fluent:
                MinHeight = FluentItemMinHeight;
                _layoutRoot.Margin = s_fluentLayoutRootMargin;
                _layoutRoot.CornerRadius = new CornerRadius(6);
                _selectionIndicator.Height = FluentSelectionIndicatorHeight;
                break;

            case FluentNavigationItemStyle.Tree:
            default:
                MinHeight = ItemMinHeight;
                _layoutRoot.Margin = s_treeLayoutRootMargin;
                _layoutRoot.CornerRadius = new CornerRadius(4);
                _selectionIndicator.Height = SelectionIndicatorHeight;
                break;
        }

        foreach (var child in MenuItems)
        {
            child.ApplyItemStyle(style);
        }
    }

    #region Interaction

    private void OnMouseEnterHandler(object? sender, MouseEventArgs e)
    {
        _isMouseOver = true;
        UpdateVisualState();
    }

    private void OnMouseLeaveHandler(object? sender, MouseEventArgs e)
    {
        _isMouseOver = false;
        _isPressed = false;
        UpdateVisualState();
    }

    private void OnMouseLeftButtonDownHandler(object? sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled) return;
        _isPressed = true;
        UpdateVisualState();
        e.Handled = true;
    }

    private void OnMouseLeftButtonUpHandler(object? sender, MouseButtonEventArgs e)
    {
        if (_isPressed && _isMouseOver && IsEnabled)
        {
            OnItemClicked();
        }
        _isPressed = false;
        UpdateVisualState();
        e.Handled = true;
    }

    private void OnItemClicked()
    {
        if (MenuItems.Count > 0 && !_isCompact)
        {
            IsExpanded = !IsExpanded;
            Invoked?.Invoke(this, EventArgs.Empty);
        }

        if (SelectsOnInvoked)
        {
            IsSelected = true;
            ParentNavigationView?.NotifyItemSelected(this);
        }
    }

    #endregion

    #region Visual states

    private void UpdateVisualState()
    {
        if (_layoutRoot == null || _selectionIndicator == null) return;

        string backgroundKey;
        string foregroundKey;
        double indicatorOpacity;

        if (!IsEnabled)
        {
            backgroundKey = _isSelected
                ? "NavigationViewItemBackgroundSelected"
                : "NavigationViewItemBackground";
            foregroundKey = "NavigationViewItemForegroundDisabled";
            indicatorOpacity = _isSelected ? 1.0 : 0.0;
        }
        else if (_isSelected)
        {
            indicatorOpacity = 1.0;
            if (_isPressed)
            {
                backgroundKey = "NavigationViewItemBackgroundSelectedPressed";
                foregroundKey = "NavigationViewItemForegroundSecondary";
            }
            else if (_isMouseOver)
            {
                backgroundKey = "NavigationViewItemBackgroundSelectedHover";
                foregroundKey = "NavigationViewItemForeground";
            }
            else
            {
                backgroundKey = "NavigationViewItemBackgroundSelected";
                foregroundKey = "NavigationViewItemForeground";
            }
        }
        else if (_isPressed)
        {
            backgroundKey = "NavigationViewItemBackgroundPressed";
            foregroundKey = "NavigationViewItemForegroundSecondary";
            indicatorOpacity = 0.0;
        }
        else if (_isMouseOver)
        {
            backgroundKey = "NavigationViewItemBackgroundHover";
            foregroundKey = "NavigationViewItemForeground";
            indicatorOpacity = 0.0;
        }
        else
        {
            backgroundKey = "NavigationViewItemBackground";
            foregroundKey = "NavigationViewItemForeground";
            indicatorOpacity = 0.0;
        }

        _layoutRoot.Background = ResolveBrush(backgroundKey);
        var foreground = ResolveBrush(foregroundKey);
        if (_contentTextBlock != null) _contentTextBlock.Foreground = foreground;
        if (_chevronText != null) _chevronText.Foreground = foreground;
        _selectionIndicator.Background = ResolveBrush("NavigationViewSelectionIndicatorForeground");
        _selectionIndicator.Opacity = indicatorOpacity;
    }

    internal void UpdateCompactState(bool isCompact)
    {
        if (_isCompact == isCompact) return;
        _isCompact = isCompact;

        if (_contentTextBlock != null)
        {
            _contentTextBlock.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_childrenPanel != null)
        {
            _childrenPanel.Visibility = (!isCompact && IsExpanded) ? Visibility.Visible : Visibility.Collapsed;
        }

        ApplyIndentation();
        ApplyChevronState();

        foreach (var child in MenuItems)
        {
            child.UpdateCompactState(isCompact);
        }
    }

    private void ApplyIndentation()
    {
        if (_contentGrid == null) return;
        var indent = _isCompact ? 0 : Depth * ChildIndentation;
        _contentGrid.Margin = new Thickness(indent, 0, _isCompact ? 0 : TextRightInset, 0);
    }

    private void ApplyChevronState()
    {
        if (_chevronHost == null || _chevronText == null) return;
        var hasChildren = MenuItems.Count > 0;
        _chevronHost.Visibility = hasChildren && !_isCompact ? Visibility.Visible : Visibility.Collapsed;
        if (_contentGrid != null)
        {
            var indent = _isCompact ? 0 : Depth * ChildIndentation;
            _contentGrid.Margin = new Thickness(indent, 0, (hasChildren && !_isCompact) ? 0 : TextRightInset, 0);
        }
        _chevronText.RenderTransform = new RotateTransform(IsExpanded ? 180 : 0);
    }

    private void OnThemeChanged() => UpdateVisualState();

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        FluentThemeManager.ThemeChanged -= OnThemeChanged;
        Unloaded -= OnUnloaded;
    }

    #endregion

    #region Children

    private void OnMenuItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildChildren();

    private void RebuildChildren()
    {
        if (_childrenPanel == null) return;
        _childrenPanel.Children.Clear();
        foreach (var child in MenuItems)
        {
            child.Depth = Depth + 1;
            child.ParentNavigationView = ParentNavigationView;
            child._isCompact = _isCompact;
            child.ItemStyle = ItemStyle;
            child.ApplyIndentation();
            _childrenPanel.Children.Add(child);
        }
        ApplyChevronState();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item && item._iconPresenter != null)
        {
            item._iconPresenter.Content = e.NewValue;
        }
    }

    private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item && item._contentGrid != null)
        {
            var textBlock = item._contentTextBlock!;
            var existingElement = item._contentGrid.Children
                .OfType<UIElement>()
                .FirstOrDefault(child => Grid.GetColumn(child) == 1 && child != textBlock);

            if (existingElement != null)
            {
                item._contentGrid.Children.Remove(existingElement);
            }

            if (e.NewValue is string text)
            {
                if (!item._contentGrid.Children.Contains(textBlock))
                {
                    Grid.SetColumn(textBlock, 1);
                    item._contentGrid.Children.Add(textBlock);
                }
                textBlock.Text = text;
            }
            else if (e.NewValue is UIElement element && element != textBlock)
            {
                if (item._contentGrid.Children.Contains(textBlock))
                {
                    item._contentGrid.Children.Remove(textBlock);
                }
                Grid.SetColumn(element, 1);
                item._contentGrid.Children.Add(element);
            }
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item)
        {
            item._isSelected = (bool)e.NewValue!;
            item.UpdateVisualState();
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item)
        {
            var isExpanded = (bool)e.NewValue!;
            if (item._childrenPanel != null)
            {
                item._childrenPanel.Visibility = (!item._isCompact && isExpanded) ? Visibility.Visible : Visibility.Collapsed;
            }
            item.ApplyChevronState();
        }
    }

    private static void OnItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item && e.NewValue is FluentNavigationItemStyle style)
        {
            item.ApplyItemStyle(style);
        }
    }

    #endregion

    #region Layout

    protected override Size MeasureOverride(Size availableSize)
    {
        _rootPanel?.Measure(availableSize);
        return _rootPanel?.DesiredSize ?? new Size(availableSize.Width, MinHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootPanel?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    protected override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _rootPanel : null;
    }

    protected override int VisualChildrenCount => _rootPanel != null ? 1 : 0;

    #endregion

    internal static Brush ResolveBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Transparent);
    }
}

/// <summary>
/// Separator for NavigationView menu items. WinUI: 1px DividerStrokeColorDefault line,
/// margin 0,3,0,4, stretching the full pane width.
/// </summary>
public class FluentNavigationViewItemSeparator : Control
{
    private Border? _line;

    public FluentNavigationViewItemSeparator()
    {
        Height = 1;
        Margin = new Thickness(0, 3, 0, 4);

        _line = new Border
        {
            Height = 1,
            Background = ResolveBrush(),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        AddVisualChild(_line);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _line?.Measure(availableSize);
        return new Size(availableSize.Width, Height + Margin.Top + Margin.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _line?.Arrange(new Rect(Margin.Left, Margin.Top,
            Math.Max(0, finalSize.Width - Margin.Left - Margin.Right), Height));
        return finalSize;
    }

    protected override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _line : null;
    }

    protected override int VisualChildrenCount => _line != null ? 1 : 0;

    private static Brush ResolveBrush()
    {
        if (Application.Current?.Resources.TryGetValue("DividerStrokeColorDefaultBrush", out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
    }
}
