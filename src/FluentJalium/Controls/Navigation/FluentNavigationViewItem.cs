using FluentJalium.Controls.Themes;
using FluentJalium.Icon;
using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;
using Jalium.UI.Media.Animation;
using System.Collections.ObjectModel;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium native NavigationViewItem - 100% custom implementation following WinUI 3 Fluent Design System.
/// Features:
/// - Proper icon display (20x20px)
/// - Selection indicator (3px left accent bar)
/// - Hover/Pressed visual states
/// - No text shifting on selection
/// - Hierarchical menu support
/// </summary>
public class FluentNavigationViewItem : Control
{
    private Border? _rootBorder;
    private Border? _selectionIndicator;
    private Border? _contentBorder;
    private Grid? _contentGrid;
    private ContentPresenter? _iconPresenter;
    private TextBlock? _contentTextBlock;
    private Border? _chevronBorder;
    private StackPanel? _childrenPanel;
    private bool _isMouseOver;
    private bool _isPressed;
    private bool _isSelected;

    public ObservableCollection<FluentNavigationViewItem> MenuItems { get; } = new();

    internal FluentNavigationView? ParentNavigationView { get; set; }

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

    public new object? Tag { get; set; }

    #endregion

    public FluentNavigationViewItem()
    {
        MinHeight = 40;
        Margin = new Thickness(4, 2, 4, 2);
        Cursor = Cursors.Hand;

        BuildVisualTree();
        UpdateVisualState(false);
    }

    private void BuildVisualTree()
    {
        // Root grid with selection indicator
        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(3) },      // Selection indicator
                new ColumnDefinition { Width = GridLength.Star }         // Content
            }
        };

        // Selection indicator (3px accent bar on the left)
        _selectionIndicator = new Border
        {
            Width = 3,
            Background = GetThemeBrush("AccentBrush"),
            Opacity = 0,
            CornerRadius = new CornerRadius(0, 2, 2, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetColumn(_selectionIndicator, 0);
        mainGrid.Children.Add(_selectionIndicator);

        // Content area
        _contentBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Transparent),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(4, 0, 4, 0)
        };
        Grid.SetColumn(_contentBorder, 1);

        // Content grid: [Icon] [Text] [Chevron]
        _contentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(48) },    // Icon area
                new ColumnDefinition { Width = GridLength.Star },       // Text
                new ColumnDefinition { Width = GridLength.Auto }        // Chevron
            },
            Margin = new Thickness(0, 8, 8, 8)
        };

        // Icon presenter (20x20px centered in 48px column)
        _iconPresenter = new ContentPresenter
        {
            Width = 20,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(_iconPresenter, 0);
        _contentGrid.Children.Add(_iconPresenter);

        // Content text
        _contentTextBlock = new TextBlock
        {
            FontSize = 14,
            FontFamily = FluentThemeManager.CurrentBodyFontFamily,
            Foreground = GetThemeBrush("TextPrimary"),
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(_contentTextBlock, 1);
        _contentGrid.Children.Add(_contentTextBlock);

        // Chevron (for expandable items)
        _chevronBorder = new Border
        {
            Width = 12,
            Height = 12,
            Margin = new Thickness(0, 0, 8, 0),
            Visibility = Visibility.Collapsed,
            VerticalAlignment = VerticalAlignment.Center
        };
        // TODO: Add chevron path
        Grid.SetColumn(_chevronBorder, 2);
        _contentGrid.Children.Add(_chevronBorder);

        _contentBorder.Child = _contentGrid;
        mainGrid.Children.Add(_contentBorder);

        // Root border
        _rootBorder = new Border
        {
            Background = new SolidColorBrush(Colors.Transparent),
            Child = mainGrid
        };

        AddVisualChild(_rootBorder);

        // Event handlers
        MouseEnter += OnMouseEnterHandler;
        MouseLeave += OnMouseLeaveHandler;
        MouseLeftButtonDown += OnMouseLeftButtonDownHandler;
        MouseLeftButtonUp += OnMouseLeftButtonUpHandler;
    }

    private void OnMouseEnterHandler(object? sender, MouseEventArgs e)
    {
        _isMouseOver = true;
        UpdateVisualState(true);
    }

    private void OnMouseLeaveHandler(object? sender, MouseEventArgs e)
    {
        _isMouseOver = false;
        _isPressed = false;
        UpdateVisualState(true);
    }

    private void OnMouseLeftButtonDownHandler(object? sender, MouseButtonEventArgs e)
    {
        _isPressed = true;
        UpdateVisualState(true);
        e.Handled = true;
    }

    private void OnMouseLeftButtonUpHandler(object? sender, MouseButtonEventArgs e)
    {
        if (_isPressed && _isMouseOver)
        {
            // Invoke
            OnItemClicked();
        }
        _isPressed = false;
        UpdateVisualState(true);
        e.Handled = true;
    }

    private void OnItemClicked()
    {
        if (SelectsOnInvoked)
        {
            IsSelected = true;
            ParentNavigationView?.NotifyItemSelected(this);
        }

        // Toggle expand for items with children
        if (MenuItems.Count > 0)
        {
            IsExpanded = !IsExpanded;
        }
    }

    private void UpdateVisualState(bool useTransitions)
    {
        if (_contentBorder == null || _selectionIndicator == null) return;

        // Background color based on state
        Color backgroundColor;
        double selectionIndicatorOpacity;

        if (_isSelected)
        {
            // Selected: SubtleFillColorSecondary + visible accent bar
            backgroundColor = GetThemeColor("SubtleFillColorSecondary");
            selectionIndicatorOpacity = 1.0;
        }
        else if (_isPressed)
        {
            // Pressed: SubtleFillColorTertiary
            backgroundColor = GetThemeColor("SubtleFillColorTertiary");
            selectionIndicatorOpacity = 0;
        }
        else if (_isMouseOver)
        {
            // Hover: SubtleFillColorSecondary
            backgroundColor = GetThemeColor("SubtleFillColorSecondary");
            selectionIndicatorOpacity = 0;
        }
        else
        {
            // Normal: Transparent
            backgroundColor = Colors.Transparent;
            selectionIndicatorOpacity = 0;
        }

        // Apply directly without animation for now
        // TODO: Add smooth transitions when Jalium.UI animation support is confirmed
        _contentBorder.Background = new SolidColorBrush(backgroundColor);
        _selectionIndicator.Opacity = selectionIndicatorOpacity;
    }

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
        if (d is FluentNavigationViewItem item && item._contentTextBlock != null)
        {
            if (e.NewValue is string text)
            {
                item._contentTextBlock.Text = text;
            }
            else if (e.NewValue is UIElement element)
            {
                // Replace text with custom content
                if (item._contentGrid != null && item._contentTextBlock != null)
                {
                    item._contentGrid.Children.Remove(item._contentTextBlock);
                    Grid.SetColumn(element, 1);
                    item._contentGrid.Children.Add(element);
                }
            }
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item)
        {
            item._isSelected = (bool)e.NewValue!;
            item.UpdateVisualState(true);
        }
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FluentNavigationViewItem item && item._childrenPanel != null)
        {
            bool isExpanded = (bool)e.NewValue!;
            item._childrenPanel.Visibility = isExpanded ? Visibility.Visible : Visibility.Collapsed;
            // TODO: Update chevron rotation
        }
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        _rootBorder?.Measure(availableSize);
        return _rootBorder?.DesiredSize ?? new Size(availableSize.Width, MinHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _rootBorder?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        return finalSize;
    }

    public override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _rootBorder : null;
    }

    public override int VisualChildrenCount => _rootBorder != null ? 1 : 0;

    private static Brush GetThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Colors.Gray);
    }

    private static Color GetThemeColor(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            if (value is Color color)
            {
                return color;
            }
        }
        return Color.FromArgb(20, 128, 128, 128);
    }
}

/// <summary>
/// Separator for NavigationView menu items.
/// </summary>
public class FluentNavigationViewItemSeparator : Control
{
    private Border? _line;

    public FluentNavigationViewItemSeparator()
    {
        Height = 1;
        Margin = new Thickness(16, 8, 16, 8);

        _line = new Border
        {
            Height = 1,
            Background = GetThemeBrush("DividerStrokeColorDefaultBrush"),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        AddVisualChild(_line);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _line?.Measure(availableSize);
        return new Size(availableSize.Width, Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _line?.Arrange(new Rect(0, 0, finalSize.Width, Height));
        return finalSize;
    }

    public override Visual? GetVisualChild(int index)
    {
        return index == 0 ? _line : null;
    }

    public override int VisualChildrenCount => _line != null ? 1 : 0;

    private static Brush GetThemeBrush(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush)
        {
            return brush;
        }
        return new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
    }
}
