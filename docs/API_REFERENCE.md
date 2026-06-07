# FluentJalium API Reference

## Overview

Complete API documentation for FluentJalium controls, focusing on Batch 16 advanced controls. This reference provides detailed information about classes, properties, methods, events, and usage patterns.

**Version**: 0.1.0-preview.1  
**Target Framework**: .NET 10  
**UI Framework**: Jalium.UI

---

## Table of Contents

1. [Motion Controls](#motion-controls)
   - [FWAnimatedIcon](#fwanimatedicon)
   - [FWAnimatedVisualPlayer](#fwanimatedvisualplayer)
2. [Collection Controls](#collection-controls)
   - [FWItemsRepeater](#fwitemsrepeater)
   - [Layouts](#layouts)
3. [Interaction Controls](#interaction-controls)
   - [FWRefreshContainer](#fwrefreshcontainer)
   - [FWScroller](#fwscroller)
   - [FWAnnotatedScrollBar](#fwannotatedscrollbar)
4. [Material Controls](#material-controls)
   - [FWBackdrop](#fwbackdrop)
   - [FWAcrylicBrush](#fwacrylicbrush)
5. [Common Interfaces](#common-interfaces)
6. [Enumerations](#enumerations)

---

## Motion Controls

### FWAnimatedIcon

State-driven animated icon control with smooth transitions between visual states.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → FWAnimatedIcon
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWAnimatedIcon()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Source** | `IAnimatedVisualSource?` | `null` | The source that provides the animated visual content |
| **State** | `string?` | `null` | The current state name that drives the animation |
| **AutoPlay** | `bool` | `true` | Whether to automatically play the animation when State changes |
| **FallbackIconSource** | `string?` | `null` | Icon to display when animation is unavailable |
| **MirroredWhenRightToLeft** | `bool` | `false` | Whether to mirror the icon in RTL layouts |

#### Methods

```csharp
public void Play()
```
Starts or resumes the animation for the current state.

```csharp
public void Stop()
```
Stops the animation and resets to the initial frame.

#### Usage Example

```csharp
var icon = new FWAnimatedIcon
{
    Source = new MyAnimatedIconSource(),
    State = "Hover",
    AutoPlay = true,
    MirroredWhenRightToLeft = true,
    FallbackIconSource = "Icons/PlayIcon.svg"
};

// Manually control animation
icon.State = "Pressed";
icon.Play();
```

#### XAML Usage

```xml
<fw:FWAnimatedIcon
    Source="{StaticResource PlayPauseIcon}"
    State="{Binding CurrentState}"
    AutoPlay="True"
    Width="32"
    Height="32" />
```

---

### FWAnimatedVisualPlayer

Lottie-style vector animation player with full playback control.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → FWAnimatedVisualPlayer
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWAnimatedVisualPlayer()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Source** | `IAnimatedVisualSource?` | `null` | The animated visual content source |
| **AutoPlay** | `bool` | `false` | Whether to automatically start playback on load |
| **IsLooping** | `bool` | `false` | Whether to loop the animation indefinitely |
| **IsPlaying** | `bool` | `false` | Gets the current playback state (read-only) |
| **PlaybackRate** | `double` | `1.0` | Playback speed multiplier (0.1 to 10.0) |
| **Stretch** | `Stretch` | `Uniform` | How the animation scales within the player bounds |
| **Duration** | `TimeSpan` | — | Gets the total animation duration (read-only) |
| **Position** | `TimeSpan` | `TimeSpan.Zero` | Gets the current playback position (read-only) |

#### Methods

```csharp
public void Play()
```
Starts playback from the current position.

```csharp
public void Pause()
```
Pauses playback at the current position.

```csharp
public void Stop()
```
Stops playback and resets to the beginning.

```csharp
public void Resume()
```
Resumes playback from the paused position.

```csharp
public void SetProgress(double progress)
```
Seeks to a specific position in the animation.
- **progress**: Value between 0.0 (start) and 1.0 (end)

#### Events

```csharp
public event EventHandler? PlaybackStarted
```
Raised when playback begins.

```csharp
public event EventHandler? PlaybackPaused
```
Raised when playback is paused.

```csharp
public event EventHandler? PlaybackStopped
```
Raised when playback stops or completes.

#### Usage Example

```csharp
var player = new FWAnimatedVisualPlayer
{
    Source = new LottieAnimationSource("animation.json"),
    AutoPlay = true,
    IsLooping = true,
    PlaybackRate = 1.5,
    Stretch = Stretch.UniformToFill
};

player.PlaybackStarted += (s, e) => Console.WriteLine("Animation started");
player.PlaybackStopped += (s, e) => Console.WriteLine("Animation completed");

// Seek to 50%
player.SetProgress(0.5);

// Change speed
player.PlaybackRate = 2.0;
player.Play();
```

#### XAML Usage

```xml
<fw:FWAnimatedVisualPlayer
    Source="{StaticResource LoadingAnimation}"
    AutoPlay="True"
    IsLooping="True"
    PlaybackRate="1.0"
    Width="200"
    Height="200"
    PlaybackStarted="OnAnimationStarted" />
```

---

## Collection Controls

### FWItemsRepeater

High-performance virtualizing container with pluggable layout system.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Panel → FWItemsRepeater
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWItemsRepeater()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **ItemsSource** | `IEnumerable?` | `null` | The data source for items |
| **ItemTemplate** | `DataTemplate?` | `null` | Template for rendering each item |
| **Layout** | `VirtualizingLayout?` | `null` | Layout strategy (StackLayout, UniformGridLayout, etc.) |
| **HorizontalCacheLength** | `double` | `200.0` | Horizontal cache buffer in pixels |
| **VerticalCacheLength** | `double` | `200.0` | Vertical cache buffer in pixels |
| **Animator** | `ElementAnimator?` | `null` | Animator for add/remove transitions |

#### Methods

```csharp
public FrameworkElement? TryGetElement(int index)
```
Gets the realized element at the specified index.
- **index**: Zero-based item index
- **Returns**: The element, or `null` if not realized

```csharp
public int GetElementIndex(FrameworkElement element)
```
Gets the item index for a realized element.
- **element**: The framework element
- **Returns**: Zero-based index, or -1 if not found

```csharp
public void InvalidateArrange()
```
Invalidates the layout and triggers re-arrangement.

#### Usage Example

```csharp
var repeater = new FWItemsRepeater
{
    ItemsSource = myDataCollection,
    ItemTemplate = new DataTemplate(() => new MyItemControl()),
    Layout = new StackLayout
    {
        Orientation = Orientation.Vertical,
        Spacing = 8
    },
    HorizontalCacheLength = 400,
    VerticalCacheLength = 400
};

// Switch to grid layout
repeater.Layout = new UniformGridLayout
{
    MinItemWidth = 200,
    MinItemHeight = 150,
    MinColumnSpacing = 12,
    MinRowSpacing = 12
};

// Access realized elements
var element = repeater.TryGetElement(5);
if (element != null)
{
    var index = repeater.GetElementIndex(element);
}
```

#### XAML Usage

```xml
<fw:FWItemsRepeater
    ItemsSource="{Binding Items}"
    HorizontalCacheLength="400"
    VerticalCacheLength="400">
    <fw:FWItemsRepeater.Layout>
        <fw:StackLayout Orientation="Vertical" Spacing="8" />
    </fw:FWItemsRepeater.Layout>
    <fw:FWItemsRepeater.ItemTemplate>
        <DataTemplate>
            <Border Background="White" Padding="12">
                <TextBlock Text="{Binding Title}" />
            </Border>
        </DataTemplate>
    </fw:FWItemsRepeater.ItemTemplate>
</fw:FWItemsRepeater>
```

---

### Layouts

#### StackLayout

Linear layout with optional spacing.

##### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Orientation** | `Orientation` | `Vertical` | Stack direction (Vertical or Horizontal) |
| **Spacing** | `double` | `0.0` | Space between items in pixels |

##### Usage Example

```csharp
var layout = new StackLayout
{
    Orientation = Orientation.Vertical,
    Spacing = 12
};
repeater.Layout = layout;
```

---

#### UniformGridLayout

Grid layout with uniformly-sized cells.

##### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Orientation** | `Orientation` | `Horizontal` | Primary fill direction |
| **MinItemWidth** | `double` | `50.0` | Minimum width per item |
| **MinItemHeight** | `double` | `50.0` | Minimum height per item |
| **MinColumnSpacing** | `double` | `0.0` | Minimum space between columns |
| **MinRowSpacing** | `double` | `0.0` | Minimum space between rows |
| **MaximumRowsOrColumns** | `int` | `-1` | Maximum rows/columns (-1 = unlimited) |
| **ItemsStretch** | `UniformGridLayoutItemsStretch` | `None` | How items stretch to fill cells |
| **ItemsJustification** | `UniformGridLayoutItemsJustification` | `Start` | Item alignment within cells |

##### Usage Example

```csharp
var layout = new UniformGridLayout
{
    Orientation = Orientation.Horizontal,
    MinItemWidth = 150,
    MinItemHeight = 120,
    MinColumnSpacing = 16,
    MinRowSpacing = 16,
    MaximumRowsOrColumns = 4
};
repeater.Layout = layout;
```

---

## Interaction Controls

### FWRefreshContainer

Pull-to-refresh functionality for scrollable content.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → ContentControl → FWRefreshContainer
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWRefreshContainer()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **PullDirection** | `RefreshPullDirection` | `TopToBottom` | Direction for pull gesture |
| **Visualizer** | `RefreshVisualizer?` | `null` | Custom visualizer for refresh indicator |

#### Events

```csharp
public event EventHandler<RefreshRequestedEventArgs>? RefreshRequested
```
Raised when the user initiates a refresh action.

#### Methods

```csharp
public void RequestRefresh()
```
Programmatically triggers a refresh action.

#### Usage Example

```csharp
var refreshContainer = new FWRefreshContainer
{
    PullDirection = RefreshPullDirection.TopToBottom,
    Content = myScrollableContent
};

refreshContainer.RefreshRequested += async (s, e) =>
{
    var deferral = e.GetDeferral();
    
    try
    {
        await LoadNewDataAsync();
    }
    finally
    {
        deferral.Complete();
    }
};
```

#### XAML Usage

```xml
<fw:FWRefreshContainer
    PullDirection="TopToBottom"
    RefreshRequested="OnRefreshRequested">
    <ScrollViewer>
        <StackPanel>
            <!-- Content here -->
        </StackPanel>
    </ScrollViewer>
</fw:FWRefreshContainer>
```

---

### FWScroller

Advanced scrolling control with snap points, zoom, and smooth inertia.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → ContentControl → FWScroller
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWScroller()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **HorizontalScrollMode** | `ScrollMode` | `Auto` | Horizontal scroll behavior |
| **VerticalScrollMode** | `ScrollMode` | `Auto` | Vertical scroll behavior |
| **HorizontalScrollChainingMode** | `ChainingMode` | `Auto` | Chaining to parent scrollers |
| **VerticalScrollChainingMode** | `ChainingMode` | `Auto` | Chaining to parent scrollers |
| **HorizontalScrollRailingMode** | `RailingMode` | `Enabled` | Lock to horizontal axis |
| **VerticalScrollRailingMode** | `RailingMode` | `Enabled` | Lock to vertical axis |
| **ZoomMode** | `ZoomMode` | `Disabled` | Zoom/pinch behavior |
| **MinZoomFactor** | `double` | `0.1` | Minimum zoom level |
| **MaxZoomFactor** | `double` | `10.0` | Maximum zoom level |
| **ZoomFactor** | `double` | `1.0` | Current zoom level |
| **HorizontalSnapPointsType** | `SnapPointsType` | `None` | Horizontal snap point behavior |
| **VerticalSnapPointsType** | `SnapPointsType` | `None` | Vertical snap point behavior |
| **HorizontalOffset** | `double` | — | Current horizontal scroll position (read-only) |
| **VerticalOffset** | `double` | — | Current vertical scroll position (read-only) |

#### Methods

```csharp
public void ScrollTo(double horizontalOffset, double verticalOffset)
```
Scrolls to an absolute position instantly.

```csharp
public void ScrollBy(double horizontalDelta, double verticalDelta)
```
Scrolls by a relative amount from the current position.

```csharp
public void ZoomTo(double zoomFactor, Point? centerPoint = null)
```
Zooms to a specific factor, optionally around a center point.

#### Events

```csharp
public event EventHandler<ScrollEventArgs>? Scrolled
```
Raised when the scroll position changes.

```csharp
public event EventHandler<ZoomEventArgs>? Zoomed
```
Raised when the zoom factor changes.

#### Usage Example

```csharp
var scroller = new FWScroller
{
    VerticalScrollMode = ScrollMode.Enabled,
    HorizontalScrollMode = ScrollMode.Disabled,
    VerticalSnapPointsType = SnapPointsType.Mandatory,
    ZoomMode = ZoomMode.Enabled,
    MinZoomFactor = 0.5,
    MaxZoomFactor = 4.0,
    Content = myLargeContent
};

// Programmatic scrolling
scroller.ScrollTo(0, 500);
scroller.ScrollBy(0, 100);

// Zoom to 200%
scroller.ZoomTo(2.0, new Point(100, 100));
```

#### XAML Usage

```xml
<fw:FWScroller
    VerticalScrollMode="Enabled"
    HorizontalScrollMode="Disabled"
    VerticalSnapPointsType="Mandatory"
    ZoomMode="Enabled"
    MinZoomFactor="0.5"
    MaxZoomFactor="4.0">
    <Image Source="large-image.png" />
</fw:FWScroller>
```

---

### FWAnnotatedScrollBar

Enhanced scrollbar with visual markers for important content positions.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → ScrollBar → FWAnnotatedScrollBar
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWAnnotatedScrollBar()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Labels** | `IList<ScrollBarLabel>?` | `null` | Collection of position markers |

#### Events

```csharp
public event EventHandler<DetailLabelRequestedEventArgs>? DetailLabelRequested
```
Raised when hovering over a label to show additional details.

#### Usage Example

```csharp
var scrollBar = new FWAnnotatedScrollBar
{
    Orientation = Orientation.Vertical,
    Labels = new List<ScrollBarLabel>
    {
        new ScrollBarLabel
        {
            ScrollOffset = 100,
            Content = "Important",
            Type = ScrollBarLabelType.Warning,
            Background = Brushes.Yellow
        },
        new ScrollBarLabel
        {
            ScrollOffset = 500,
            Content = "Error",
            Type = ScrollBarLabelType.Error,
            Background = Brushes.Red
        }
    }
};

scrollBar.DetailLabelRequested += (s, e) =>
{
    e.Content = $"Line {e.ScrollOffset}: {e.Label.Content}";
};
```

#### XAML Usage

```xml
<fw:FWAnnotatedScrollBar Orientation="Vertical">
    <fw:FWAnnotatedScrollBar.Labels>
        <fw:ScrollBarLabel
            ScrollOffset="100"
            Content="Warning"
            Type="Warning" />
        <fw:ScrollBarLabel
            ScrollOffset="500"
            Content="Error"
            Type="Error" />
    </fw:FWAnnotatedScrollBar.Labels>
</fw:FWAnnotatedScrollBar>
```

---

## Material Controls

### FWBackdrop

Background material effects (Acrylic, Mica, MicaAlt, Tabbed).

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Visual → UIElement → FrameworkElement → Control → FWBackdrop
```

#### Implements
- `IFluentJaliumControl`

#### Constructor
```csharp
public FWBackdrop()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Type** | `FWBackdropType` | `None` | Material effect type |
| **TintColor** | `Color` | `White` | Tint color overlay |
| **TintOpacity** | `double` | `0.8` | Opacity of tint layer (0.0 to 1.0) |
| **LuminosityOpacity** | `double` | `0.85` | Luminosity blend opacity (0.0 to 1.0) |
| **FallbackColor** | `Color` | `LightGray` | Color when material is unavailable |
| **AlwaysUseFallback** | `bool` | `false` | Force fallback rendering |

#### Usage Example

```csharp
var backdrop = new FWBackdrop
{
    Type = FWBackdropType.Acrylic,
    TintColor = Color.FromRgb(0xF3, 0xF3, 0xF3),
    TintOpacity = 0.7,
    LuminosityOpacity = 0.85
};

// Switch material types
backdrop.Type = FWBackdropType.Mica;
backdrop.Type = FWBackdropType.MicaAlt;
backdrop.Type = FWBackdropType.Tabbed;
```

#### XAML Usage

```xml
<fw:FWBackdrop
    Type="Acrylic"
    TintColor="#F3F3F3"
    TintOpacity="0.7"
    LuminosityOpacity="0.85" />
```

---

### FWAcrylicBrush

Reusable acrylic brush for semi-transparent blur effects.

#### Namespace
```csharp
FluentJalium.Controls
```

#### Inheritance
```
Object → DependencyObject → Freezable → Animatable → Brush → FWAcrylicBrush
```

#### Constructor
```csharp
public FWAcrylicBrush()
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **TintColor** | `Color` | `White` | Tint color overlay |
| **TintOpacity** | `double` | `0.8` | Opacity of tint (0.0 to 1.0) |
| **TintLuminosityOpacity** | `double?` | `null` | Optional luminosity opacity override |
| **BackgroundSource** | `AcrylicBackgroundSource` | `Backdrop` | Source for blur (Backdrop or HostBackdrop) |
| **FallbackColor** | `Color` | `LightGray` | Solid color fallback |
| **AlwaysUseFallback** | `bool` | `false` | Force fallback mode |

#### Usage Example

```csharp
var brush = new FWAcrylicBrush
{
    TintColor = Color.FromRgb(0x00, 0x78, 0xD4),
    TintOpacity = 0.6,
    BackgroundSource = AcrylicBackgroundSource.Backdrop
};

var border = new Border
{
    Background = brush,
    Child = myContent
};
```

#### XAML Usage

```xml
<Border>
    <Border.Background>
        <fw:FWAcrylicBrush
            TintColor="#0078D4"
            TintOpacity="0.6"
            BackgroundSource="Backdrop" />
    </Border.Background>
    <TextBlock Text="Content with acrylic background" />
</Border>
```

---

## Common Interfaces

### IFluentJaliumControl

Marker interface implemented by all FluentJalium controls.

```csharp
public interface IFluentJaliumControl
{
    // Marker interface - no members
}
```

---

### IAnimatedVisualSource

Provides animated visual content for FWAnimatedIcon and FWAnimatedVisualPlayer.

```csharp
public interface IAnimatedVisualSource
{
    FrameworkElement? TryCreateAnimatedVisual();
}
```

#### Methods

- **TryCreateAnimatedVisual()**: Creates and returns the animated visual element, or `null` if creation fails.

---

### IAnimatedVisual

Represents a single animated visual with render capabilities.

```csharp
public interface IAnimatedVisual
{
    TimeSpan Duration { get; }
    Size Size { get; }
    void SetProgress(double progress);
    void Render(DrawingContext drawingContext, Size size);
}
```

#### Properties

- **Duration**: Total animation duration
- **Size**: Natural size of the animation

#### Methods

- **SetProgress(double)**: Sets the animation position (0.0 to 1.0)
- **Render(DrawingContext, Size)**: Renders the current frame

---

## Enumerations

### FWBackdropType

Material effect types for FWBackdrop.

```csharp
public enum FWBackdropType
{
    None = 0,        // No material effect
    Acrylic = 1,     // Semi-transparent blur
    Mica = 2,        // Subtle texture
    MicaAlt = 3,     // Darker variant
    Tabbed = 4       // Optimized for tabs
}
```

---

### RefreshPullDirection

Pull gesture direction for FWRefreshContainer.

```csharp
public enum RefreshPullDirection
{
    TopToBottom = 0,   // Pull down from top
    BottomToTop = 1,   // Pull up from bottom
    LeftToRight = 2,   // Pull right from left
    RightToLeft = 3    // Pull left from right
}
```

---

### ScrollMode

Scroll behavior for FWScroller.

```csharp
public enum ScrollMode
{
    Disabled = 0,   // Scrolling disabled
    Enabled = 1,    // Scrolling enabled
    Auto = 2        // Automatic based on content
}
```

---

### SnapPointsType

Snap point behavior for FWScroller.

```csharp
public enum SnapPointsType
{
    None = 0,             // No snap points
    Optional = 1,         // Snaps if close
    Mandatory = 2,        // Always snaps
    OptionalSingle = 3,   // Snap one at a time
    MandatorySingle = 4   // Force single snap
}
```

---

### ScrollBarLabelType

Label types for FWAnnotatedScrollBar.

```csharp
public enum ScrollBarLabelType
{
    Default = 0,   // Normal label
    Warning = 1,   // Warning indicator
    Error = 2,     // Error indicator
    Info = 3       // Information indicator
}
```

---

### AcrylicBackgroundSource

Background source for acrylic blur.

```csharp
public enum AcrylicBackgroundSource
{
    Backdrop = 0,        // Blur content behind control
    HostBackdrop = 1     // Blur app background
}
```

---

## Event Arguments

### RefreshRequestedEventArgs

Event arguments for FWRefreshContainer.RefreshRequested.

```csharp
public class RefreshRequestedEventArgs : EventArgs
{
    public RefreshRequestedDeferral GetDeferral();
}
```

#### Methods

- **GetDeferral()**: Returns a deferral object for async operations. Call `deferral.Complete()` when finished.

---

### ScrollEventArgs

Event arguments for FWScroller.Scrolled.

```csharp
public class ScrollEventArgs : EventArgs
{
    public double HorizontalOffset { get; }
    public double VerticalOffset { get; }
}
```

---

### ZoomEventArgs

Event arguments for FWScroller.Zoomed.

```csharp
public class ZoomEventArgs : EventArgs
{
    public double ZoomFactor { get; }
    public Point CenterPoint { get; }
}
```

---

### DetailLabelRequestedEventArgs

Event arguments for FWAnnotatedScrollBar.DetailLabelRequested.

```csharp
public class DetailLabelRequestedEventArgs : EventArgs
{
    public ScrollBarLabel Label { get; }
    public double ScrollOffset { get; }
    public object? Content { get; set; }
}
```

---

## Best Practices

### Performance

1. **Use appropriate cache lengths** for FWItemsRepeater based on item count
2. **Limit animation complexity** in FWAnimatedVisualPlayer (< 50 layers)
3. **Enable hardware acceleration** for FWScroller when available
4. **Use fallback colors** for FWBackdrop on low-end hardware

### Accessibility

1. **Provide FallbackIconSource** for FWAnimatedIcon
2. **Ensure snap points** are keyboard-navigable in FWScroller
3. **Add semantic labels** to ScrollBarLabel items
4. **Test with screen readers** for all interactive controls

### Threading

1. **Use RefreshRequestedDeferral** for async refresh operations
2. **Offload heavy operations** from animation render loops
3. **Update UI properties** on the dispatcher thread only
4. **Avoid blocking operations** in layout measure/arrange

---

## Version History

- **0.1.0-preview.1** (2026-06-07): Initial Batch 16 release with 7 advanced controls

---

**Documentation Last Updated**: 2026-06-07  
**For More Information**: See [README.md](../README.md) and [PROGRESS.md](../PROGRESS.md)
