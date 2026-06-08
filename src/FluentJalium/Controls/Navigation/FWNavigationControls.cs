using Jalium.UI;
using Jalium.UI.Controls;
using System.Diagnostics.CodeAnalysis;

namespace FluentJalium.Controls;

/// <summary>
/// FluentJalium density presets for navigation and tab surfaces.
/// </summary>
public enum FWNavigationDensity
{
    Compact,
    Comfortable,
    Spacious
}

/// <summary>
/// Resolves the concrete page type that should be used for a navigation route.
/// </summary>
public delegate Type? FWNavigationPageTypeProvider(FWNavigationRoute route, object? parameter);

/// <summary>
/// Route metadata used by <see cref="FWNavigationService"/>.
/// </summary>
public sealed class FWNavigationRoute
{
    public FWNavigationRoute(string routeKey, Type pageType, object? parameter = null, FWNavigationViewItem? item = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeKey);
        ArgumentNullException.ThrowIfNull(pageType);

        if (!typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException("Navigation routes must target a Page type.", nameof(pageType));
        }

        RouteKey = routeKey;
        PageType = pageType;
        Parameter = parameter;
        Item = item;
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string RouteKey { get; }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public Type PageType { get; }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public object? Parameter { get; }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public FWNavigationViewItem? Item { get; }
}

/// <summary>
/// Snapshot of <see cref="FWNavigationService"/> shell routing and history state.
/// </summary>
public readonly record struct FWNavigationServiceDiagnostics(
    bool IsAttached,
    int RouteCount,
    string? CurrentRouteKey,
    Type? CurrentPageType,
    bool CanGoBack,
    bool CanGoForward,
    int BackStackDepth,
    bool IsSynchronizingSelection,
    bool HasPageTypeProvider);

/// <summary>
/// Lightweight NavigationView + Frame coordinator for app-shell routing.
/// </summary>
public sealed class FWNavigationService
{
    private readonly Dictionary<string, FWNavigationRoute> _routes = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _resolvedPageRouteKeys = new();
    private FWNavigationView? _navigationView;
    private FWFrame? _frame;
    private bool _isSynchronizingSelection;

    public event EventHandler<FWNavigationRoute>? Navigated;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public bool IsAttached => _navigationView != null && _frame != null;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public int RouteCount => _routes.Count;

    [DevToolsPropertyCategory(DevToolsPropertyCategory.State)]
    public string? CurrentRouteKey { get; private set; }

    public IReadOnlyCollection<FWNavigationRoute> Routes => _routes.Values;

    /// <summary>
    /// Gets or sets an optional route page-type resolver for DI-backed or generated app shells.
    /// </summary>
    [DevToolsPropertyCategory(DevToolsPropertyCategory.Behavior)]
    public FWNavigationPageTypeProvider? PageTypeProvider { get; set; }

    public void Attach(FWNavigationView navigationView, FWFrame frame)
    {
        ArgumentNullException.ThrowIfNull(navigationView);
        ArgumentNullException.ThrowIfNull(frame);

        Detach();

        _navigationView = navigationView;
        _frame = frame;
        _navigationView.SelectionChanged += OnNavigationSelectionChanged;
        _navigationView.BackRequested += OnNavigationBackRequested;
        _frame.Navigated += OnFrameNavigated;
        UpdateNavigationViewBackState();
        SynchronizeSelectionFromFrame();
    }

    public void Detach()
    {
        if (_navigationView != null)
        {
            _navigationView.SelectionChanged -= OnNavigationSelectionChanged;
            _navigationView.BackRequested -= OnNavigationBackRequested;
        }

        if (_frame != null)
        {
            _frame.Navigated -= OnFrameNavigated;
        }

        _navigationView = null;
        _frame = null;
        _isSynchronizingSelection = false;
        CurrentRouteKey = null;
    }

    public FWNavigationRoute RegisterRoute(string routeKey, Type pageType, object? parameter = null)
    {
        var route = new FWNavigationRoute(routeKey, pageType, parameter);
        RemoveResolvedRouteKeys(route.RouteKey);
        _routes[route.RouteKey] = route;
        return route;
    }

    public FWNavigationRoute RegisterRoute(FWNavigationViewItem item, Type pageType, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(item);

        var routeKey = string.IsNullOrWhiteSpace(item.RouteKey)
            ? ResolveRouteKey(item, pageType)
            : item.RouteKey;
        item.RouteKey = routeKey;

        var route = new FWNavigationRoute(routeKey, pageType, parameter, item);
        RemoveResolvedRouteKeys(route.RouteKey);
        _routes[route.RouteKey] = route;
        return route;
    }

    public bool UnregisterRoute(string routeKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeKey);

        if (!_routes.Remove(routeKey))
        {
            return false;
        }

        RemoveResolvedRouteKeys(routeKey);
        if (string.Equals(CurrentRouteKey, routeKey, StringComparison.Ordinal))
        {
            CurrentRouteKey = null;
        }

        return true;
    }

    [RequiresUnreferencedCode("Navigates a Frame by Page type. Keep registered page constructors reachable.")]
    public bool NavigateToRoute(string routeKey, object? parameter = null)
    {
        return NavigateToRoute(routeKey, parameter, updateSelection: true);
    }

    [RequiresUnreferencedCode("Navigates a Frame by Page type. Keep registered page constructors reachable.")]
    public bool GoBack()
    {
        if (_frame?.GoBack() == true)
        {
            UpdateNavigationViewBackState();
            return true;
        }

        return false;
    }

    [RequiresUnreferencedCode("Navigates a Frame by Page type. Keep registered page constructors reachable.")]
    public bool GoForward()
    {
        if (_frame?.GoForward() == true)
        {
            UpdateNavigationViewBackState();
            return true;
        }

        return false;
    }

    public FWNavigationServiceDiagnostics GetDiagnostics()
    {
        return new FWNavigationServiceDiagnostics(
            IsAttached,
            RouteCount,
            CurrentRouteKey,
            _frame?.SourcePageType,
            _frame?.CanGoBack ?? false,
            _frame?.CanGoForward ?? false,
            _frame?.BackStackDepth ?? 0,
            _isSynchronizingSelection,
            PageTypeProvider != null);
    }

    [RequiresUnreferencedCode("Navigates a Frame by Page type. Keep registered page constructors reachable.")]
    private bool NavigateToRoute(string routeKey, object? parameter, bool updateSelection)
    {
        if (_frame == null || !_routes.TryGetValue(routeKey, out var route))
        {
            return false;
        }

        var navigationParameter = parameter ?? route.Parameter ?? route.RouteKey;
        var pageType = ResolvePageType(route, navigationParameter);
        if (pageType == null)
        {
            UpdateNavigationViewBackState();
            return false;
        }

        _resolvedPageRouteKeys[pageType] = route.RouteKey;
        if (!_frame.Navigate(pageType, navigationParameter))
        {
            _resolvedPageRouteKeys.Remove(pageType);
            UpdateNavigationViewBackState();
            return false;
        }

        CurrentRouteKey = route.RouteKey;
        if (updateSelection)
        {
            SynchronizeSelection(route);
        }

        UpdateNavigationViewBackState();
        return true;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "FWNavigationService routes through registered Page types; public navigation APIs carry the trimming annotation.")]
    private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (_isSynchronizingSelection || e.SelectedItem is not FWNavigationViewItem item)
        {
            return;
        }

        var routeKey = ResolveRouteKey(item);
        if (!string.IsNullOrWhiteSpace(routeKey))
        {
            NavigateToRoute(routeKey, null, updateSelection: false);
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "FWNavigationService routes through registered Page types; public navigation APIs carry the trimming annotation.")]
    private void OnNavigationBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        e.Handled = GoBack();
    }

    private void OnFrameNavigated(object? sender, NavigationEventArgs e)
    {
        CurrentRouteKey = ResolveRouteKey(e.SourcePageType);
        UpdateNavigationViewBackState();
        SynchronizeSelectionFromFrame();

        if (CurrentRouteKey != null && _routes.TryGetValue(CurrentRouteKey, out var route))
        {
            Navigated?.Invoke(this, route);
        }
    }

    private Type? ResolvePageType(FWNavigationRoute route, object? parameter)
    {
        var pageType = PageTypeProvider?.Invoke(route, parameter) ?? route.PageType;
        return pageType != null && typeof(Page).IsAssignableFrom(pageType) ? pageType : null;
    }

    private void RemoveResolvedRouteKeys(string routeKey)
    {
        foreach (var resolvedPageType in _resolvedPageRouteKeys
            .Where(pair => string.Equals(pair.Value, routeKey, StringComparison.Ordinal))
            .Select(pair => pair.Key)
            .ToArray())
        {
            _resolvedPageRouteKeys.Remove(resolvedPageType);
        }
    }

    private void SynchronizeSelectionFromFrame()
    {
        var routeKey = _frame?.SourcePageType != null ? ResolveRouteKey(_frame.SourcePageType) : null;
        if (routeKey != null && _routes.TryGetValue(routeKey, out var route))
        {
            CurrentRouteKey = route.RouteKey;
            SynchronizeSelection(route);
            return;
        }

        CurrentRouteKey = null;
    }

    private void SynchronizeSelection(FWNavigationRoute route)
    {
        if (_navigationView == null || route.Item == null || ReferenceEquals(_navigationView.SelectedItem, route.Item))
        {
            return;
        }

        _isSynchronizingSelection = true;
        try
        {
            _navigationView.SelectedItem = route.Item;
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    private void UpdateNavigationViewBackState()
    {
        if (_navigationView != null && _frame != null)
        {
            _navigationView.IsBackEnabled = _frame.CanGoBack;
        }
    }

    private string? ResolveRouteKey(Type? pageType)
    {
        if (pageType == null)
        {
            return null;
        }

        foreach (var route in _routes.Values)
        {
            if (route.PageType == pageType)
            {
                return route.RouteKey;
            }
        }

        if (_resolvedPageRouteKeys.TryGetValue(pageType, out var resolvedRouteKey) &&
            _routes.ContainsKey(resolvedRouteKey))
        {
            return resolvedRouteKey;
        }

        return null;
    }

    private static string? ResolveRouteKey(FWNavigationViewItem item)
    {
        return string.IsNullOrWhiteSpace(item.RouteKey)
            ? item.Content?.ToString()
            : item.RouteKey;
    }

    private static string ResolveRouteKey(FWNavigationViewItem item, Type pageType)
    {
        var contentKey = item.Content?.ToString();
        return string.IsNullOrWhiteSpace(contentKey) ? pageType.Name : contentKey;
    }
}

/// <summary>
/// FluentJalium NavigationView control.
/// </summary>
public class FWNavigationView : NavigationView, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWNavigationView),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWNavigationView()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double OpenPaneLength, double CompactPaneLength) GetPaneMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (240.0, 40.0),
            FWNavigationDensity.Spacious => (320.0, 56.0),
            _ => (280.0, 48.0)
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNavigationView navigationView && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(navigationView, density);
        }
    }

    private static void ApplyDensity(FWNavigationView navigationView, FWNavigationDensity density)
    {
        var (openPaneLength, compactPaneLength) = GetPaneMetrics(density);
        navigationView.OpenPaneLength = openPaneLength;
        navigationView.CompactPaneLength = compactPaneLength;
    }
}

/// <summary>
/// FluentJalium NavigationViewItem control.
/// </summary>
public class FWNavigationViewItem : NavigationViewItem, IFluentJaliumControl
{
    public static readonly DependencyProperty RouteKeyProperty =
        DependencyProperty.Register(nameof(RouteKey), typeof(string), typeof(FWNavigationViewItem),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWNavigationViewItem),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWNavigationViewItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Content)]
    public string RouteKey
    {
        get => (string)(GetValue(RouteKeyProperty) ?? string.Empty);
        set => SetValue(RouteKeyProperty, value);
    }

    internal static (double MinHeight, Thickness Margin) GetItemMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(4, 1, 4, 1)),
            FWNavigationDensity.Spacious => (44.0, new Thickness(8, 2, 8, 2)),
            _ => (36.0, new Thickness(6, 2, 6, 2))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWNavigationViewItem item && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(item, density);
        }
    }

    private static void ApplyDensity(FWNavigationViewItem item, FWNavigationDensity density)
    {
        var (minHeight, margin) = GetItemMetrics(density);
        item.MinHeight = minHeight;
        item.Margin = margin;
    }
}

/// <summary>
/// FluentJalium NavigationViewItemHeader control.
/// </summary>
public class FWNavigationViewItemHeader : NavigationViewItemHeader, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium NavigationViewItemSeparator control.
/// </summary>
public class FWNavigationViewItemSeparator : NavigationViewItemSeparator, IFluentJaliumControl
{
}

/// <summary>
/// FluentJalium TabControl control.
/// </summary>
public class FWTabControl : TabControl, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWTabControl),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWTabControl()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static double GetTabStripHeight(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => 36.0,
            FWNavigationDensity.Spacious => 48.0,
            _ => 40.0
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabControl tabControl && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(tabControl, density);
        }
    }

    private static void ApplyDensity(FWTabControl tabControl, FWNavigationDensity density)
    {
        tabControl.TabStripHeight = GetTabStripHeight(density);
    }
}

/// <summary>
/// FluentJalium TabItem control.
/// </summary>
public class FWTabItem : TabItem, IFluentJaliumControl
{
    public static readonly DependencyProperty DensityProperty =
        DependencyProperty.Register(nameof(Density), typeof(FWNavigationDensity), typeof(FWTabItem),
            new PropertyMetadata(FWNavigationDensity.Comfortable, OnDensityChanged));

    public FWTabItem()
    {
        ApplyDensity(this, Density);
    }

    [DevToolsPropertyCategory(DevToolsPropertyCategory.Layout)]
    public FWNavigationDensity Density
    {
        get => (FWNavigationDensity)GetValue(DensityProperty)!;
        set => SetValue(DensityProperty, value);
    }

    internal static (double MinHeight, Thickness Padding) GetTabItemMetrics(FWNavigationDensity density)
    {
        return density switch
        {
            FWNavigationDensity.Compact => (32.0, new Thickness(12, 7, 12, 7)),
            FWNavigationDensity.Spacious => (44.0, new Thickness(18, 12, 18, 12)),
            _ => (36.0, new Thickness(16, 9, 16, 9))
        };
    }

    private static void OnDensityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FWTabItem tabItem && e.NewValue is FWNavigationDensity density)
        {
            ApplyDensity(tabItem, density);
        }
    }

    private static void ApplyDensity(FWTabItem tabItem, FWNavigationDensity density)
    {
        var (minHeight, padding) = GetTabItemMetrics(density);
        tabItem.MinHeight = minHeight;
        tabItem.Padding = padding;
    }
}

/// <summary>
/// FluentJalium Frame control.
/// </summary>
public class FWFrame : Frame, IFluentJaliumControl
{
}
