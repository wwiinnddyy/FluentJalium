# FluentJalium Performance Testing Guide

## Overview

This document describes the performance testing methodology, benchmarks, and optimization strategies for FluentJalium controls, with a focus on Batch 16 advanced controls.

---

## Performance Testing Tools

### Testing Framework
- **BenchmarkDotNet**: For micro-benchmarking control operations
- **Visual Studio Profiler**: For memory and CPU profiling
- **PerfView**: For detailed ETW trace analysis
- **Jalium.UI DevTools**: For visual tree inspection

### Key Metrics
1. **Rendering Performance**: Frame rate (target: 60 FPS)
2. **Memory Usage**: Heap allocations and GC pressure
3. **Startup Time**: Control initialization overhead
4. **Virtualization Efficiency**: Items per second (for collections)
5. **Animation Smoothness**: Frame drops during transitions

---

## Batch 16 Control Performance Benchmarks

### FWItemsRepeater Virtualization

#### Target Performance
- **Render 1000 items**: < 50ms initial layout
- **Scroll performance**: 60 FPS with cache enabled
- **Memory per item**: < 500 bytes baseline allocation

#### Benchmark Results (Expected)

```csharp
| Layout Type      | Items | Layout Time | Memory (MB) | FPS  |
|------------------|-------|-------------|-------------|------|
| StackLayout      | 100   | 5ms         | 2.1         | 60   |
| StackLayout      | 1000  | 45ms        | 18.5        | 60   |
| UniformGridLayout| 100   | 8ms         | 2.3         | 60   |
| UniformGridLayout| 1000  | 62ms        | 21.2        | 58   |
```

#### Optimization Strategies

1. **Cache Length Tuning**
```csharp
// Recommended cache settings based on item count
var repeater = new FWItemsRepeater
{
    HorizontalCacheLength = itemCount > 500 ? 400 : 200,
    VerticalCacheLength = itemCount > 500 ? 400 : 200
};
```

2. **Item Template Optimization**
```csharp
// ❌ Bad: Complex template with many bindings
<Border Background="{Binding Color}" 
        BorderBrush="{Binding Border}"
        Opacity="{Binding Opacity}">
    <StackPanel>
        <TextBlock Text="{Binding Title}" />
        <TextBlock Text="{Binding Description}" />
        <Image Source="{Binding Thumbnail}" />
    </StackPanel>
</Border>

// ✅ Good: Simplified template, minimal bindings
<Border Background="White" BorderBrush="#E0E0E0">
    <TextBlock Text="{Binding DisplayText}" />
</Border>
```

3. **Layout Selection**
- Use **StackLayout** for simple lists (vertical/horizontal)
- Use **UniformGridLayout** only when grid display is needed
- Implement custom **VirtualizingLayout** for complex scenarios

---

### FWAnimatedVisualPlayer Animation Performance

#### Target Performance
- **Playback at 1.0x**: Consistent 60 FPS
- **Playback at 5.0x**: Minimum 30 FPS
- **Memory per animation**: < 2MB for typical vector animations

#### Optimization Strategies

1. **Animation Complexity Management**
```csharp
// Limit animation complexity
const int MAX_ANIMATION_LAYERS = 10;
const int MAX_KEYFRAMES_PER_LAYER = 100;
```

2. **Playback Rate Throttling**
```csharp
// Clamp playback rate to prevent excessive CPU usage
player.PlaybackRate = Math.Clamp(rate, 0.1, 5.0);
```

3. **Background Rendering**
```csharp
// Consider using background thread for complex animations
// (Implementation depends on Jalium.UI threading model)
```

---

### FWScroller Smooth Scrolling Performance

#### Target Performance
- **Scroll inertia**: 60 FPS throughout deceleration
- **Snap animation**: < 200ms to snap point
- **Zoom operations**: 60 FPS during pinch/zoom

#### Optimization Strategies

1. **Hardware Acceleration**
```csharp
// Enable composition-based scrolling if available
scroller.UseCompositionScrolling = true;
```

2. **Snap Point Calculation**
```csharp
// Pre-calculate snap points instead of dynamic calculation
var snapPoints = GenerateSnapPoints(contentSize, itemSize);
scroller.VerticalSnapPoints = snapPoints;
```

3. **Zoom Level Caching**
```csharp
// Cache rendered content at common zoom levels
var zoomCache = new Dictionary<double, RenderTargetBitmap>
{
    { 1.0, null },
    { 1.5, null },
    { 2.0, null }
};
```

---

### FWRefreshContainer Pull Performance

#### Target Performance
- **Pull gesture tracking**: < 16ms per frame (60 FPS)
- **Refresh animation**: Smooth at 60 FPS
- **Memory overhead**: < 100KB

#### Optimization Strategies

1. **Gesture Throttling**
```csharp
// Throttle pull distance updates
private DateTime _lastPullUpdate;
private const int PULL_UPDATE_THROTTLE_MS = 16;

private void OnMouseMove(object sender, MouseEventArgs e)
{
    var now = DateTime.UtcNow;
    if ((now - _lastPullUpdate).TotalMilliseconds < PULL_UPDATE_THROTTLE_MS)
        return;
    
    _lastPullUpdate = now;
    UpdatePullDistance();
}
```

2. **Visualizer Optimization**
```csharp
// Use simple visualizer during pull gesture
public class OptimizedRefreshVisualizer : RefreshVisualizer
{
    public override void UpdateProgress(double progress)
    {
        // Avoid expensive visual updates
        Opacity = progress;
        // Skip complex animations during pull
    }
}
```

---

### FWBackdrop Material Effects Performance

#### Target Performance
- **Acrylic rendering**: < 5ms per frame
- **Material switching**: < 100ms transition
- **Memory overhead**: < 500KB per backdrop instance

#### Optimization Strategies

1. **Material Caching**
```csharp
// Cache material brushes
private static readonly Dictionary<FWBackdropType, Brush> MaterialCache 
    = new Dictionary<FWBackdropType, Brush>();

private Brush GetCachedMaterial(FWBackdropType type)
{
    if (!MaterialCache.ContainsKey(type))
    {
        MaterialCache[type] = CreateBackdropBrush();
    }
    return MaterialCache[type];
}
```

2. **Blur Effect Optimization**
```csharp
// Use lower blur radius for better performance
const double OPTIMIZED_BLUR_RADIUS = 30; // Instead of 60
const double OPTIMIZED_BLUR_AMOUNT = 0.6; // Instead of 1.0
```

3. **Fallback Strategy**
```csharp
// Use fallback on low-end hardware
if (IsLowEndHardware())
{
    backdrop.AlwaysUseFallback = true;
}
```

---

## Memory Optimization

### General Strategies

1. **Object Pooling**
```csharp
// Pool frequently created objects
public class ControlElementPool<T> where T : FrameworkElement, new()
{
    private readonly Stack<T> _pool = new Stack<T>();
    private const int MAX_POOL_SIZE = 100;

    public T Rent()
    {
        return _pool.Count > 0 ? _pool.Pop() : new T();
    }

    public void Return(T element)
    {
        if (_pool.Count < MAX_POOL_SIZE)
        {
            // Reset element state
            element.DataContext = null;
            _pool.Push(element);
        }
    }
}
```

2. **Weak Event Patterns**
```csharp
// Avoid memory leaks from event subscriptions
public class WeakEventHelper
{
    public static void AddWeakHandler<TEventArgs>(
        object source,
        string eventName,
        EventHandler<TEventArgs> handler)
        where TEventArgs : EventArgs
    {
        // Implementation using WeakReference
    }
}
```

3. **Dispose Pattern**
```csharp
public class FWItemsRepeater : Panel, IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        
        // Clean up resources
        _realizedElements?.Clear();
        
        if (ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnItemsCollectionChanged;
        }
        
        _disposed = true;
    }
}
```

---

## CPU Optimization

### Rendering Pipeline

1. **Avoid Unnecessary Invalidations**
```csharp
// Cache calculated values
private Size? _cachedMeasureSize;
private Size _lastAvailableSize;

protected override Size MeasureOverride(Size availableSize)
{
    if (_cachedMeasureSize.HasValue && 
        _lastAvailableSize == availableSize)
    {
        return _cachedMeasureSize.Value;
    }

    _lastAvailableSize = availableSize;
    _cachedMeasureSize = PerformMeasure(availableSize);
    return _cachedMeasureSize.Value;
}
```

2. **Batch Updates**
```csharp
// Batch multiple property updates
using (var batch = BeginUpdateBatch())
{
    Property1 = value1;
    Property2 = value2;
    Property3 = value3;
} // Updates applied here in single pass
```

3. **Async Operations**
```csharp
// Offload heavy operations to background threads
public async Task LoadItemsAsync(IEnumerable<object> items)
{
    var processedItems = await Task.Run(() => 
        items.Select(ProcessItem).ToList());
    
    // Update UI on dispatcher thread
    Dispatcher.Invoke(() =>
    {
        ItemsSource = processedItems;
    });
}
```

---

## Performance Testing Checklist

### Before Release
- [ ] Run BenchmarkDotNet tests on all Batch 16 controls
- [ ] Profile memory usage with 10,000+ items in FWItemsRepeater
- [ ] Test animation performance at 0.1x, 1.0x, and 5.0x rates
- [ ] Measure scroll performance with FWScroller snap points
- [ ] Verify 60 FPS rendering on target hardware
- [ ] Test memory leaks with long-running Gallery application
- [ ] Profile startup time with all controls loaded
- [ ] Measure GC pressure during typical usage scenarios

### Regression Testing
- [ ] Compare performance against previous versions
- [ ] Verify no performance degradation in existing controls
- [ ] Test on low-end hardware (4GB RAM, integrated graphics)
- [ ] Profile on high-DPI displays (4K, 8K)
- [ ] Test with accessibility features enabled

---

## Known Performance Limitations

### Current Limitations (v0.1.0-preview.1)

1. **FWAnimatedVisualPlayer**: Complex animations (>50 layers) may drop frames
2. **FWItemsRepeater**: Initial layout of 5000+ items may take >100ms
3. **FWBackdrop**: Acrylic effect not hardware-accelerated on all platforms
4. **FWScroller**: Zoom operations may lag on integrated graphics

### Future Optimizations

1. Implement hardware-accelerated blur for Acrylic
2. Add progressive rendering for large FWItemsRepeater collections
3. Optimize FWAnimatedVisualPlayer with GPU rendering
4. Implement virtual scrolling for FWScroller with large content

---

## Profiling Tools Setup

### Visual Studio Profiler
```bash
# Launch with profiling
dotnet run --project samples/FluentJalium.Gallery --configuration Release --launch-profile "Performance Profiling"
```

### BenchmarkDotNet
```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net100)]
public class ItemsRepeaterBenchmarks
{
    private FWItemsRepeater _repeater;
    
    [GlobalSetup]
    public void Setup()
    {
        _repeater = new FWItemsRepeater();
    }
    
    [Benchmark]
    public void LayoutStack_100Items()
    {
        _repeater.ItemsSource = GenerateItems(100);
        _repeater.Measure(new Size(800, double.PositiveInfinity));
    }
}
```

---

## Performance Best Practices Summary

1. ✅ **Use virtualization** for lists with >50 items
2. ✅ **Minimize data bindings** in item templates
3. ✅ **Enable caching** (HorizontalCacheLength, VerticalCacheLength)
4. ✅ **Throttle animations** to maintain 60 FPS
5. ✅ **Profile early and often** during development
6. ✅ **Test on target hardware** before release
7. ✅ **Implement object pooling** for frequently created elements
8. ✅ **Use weak event patterns** to prevent memory leaks
9. ✅ **Batch property updates** to reduce invalidations
10. ✅ **Offload heavy operations** to background threads

---

**Last Updated**: 2026-06-07  
**Version**: 0.1.0-preview.1  
**Target**: 60 FPS @ 1080p on mid-range hardware
