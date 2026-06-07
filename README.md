# FluentJalium

FluentJalium is a Fluent Design System theme and control library for Jalium.UI on .NET 10.

The current milestone provides a Fluent resource layer, Fluent default styles for core Jalium controls, FW-prefixed controls, and a small gallery application for visual validation.

## Architecture

FluentJalium develops in two parallel tracks:

- Theme overlay: `FluentThemeManager.Apply(app)` appends Fluent resources and replaces the default visual style of Jalium-owned controls without modifying `Jalium.UI`.
- FluentJalium controls: controls owned by this library use the `FW` prefix, such as `FWButton`. These controls live in `FluentJalium.Controls`, implement `IFluentJaliumControl`, and are intended to become the Fluent Design System control surface for WinUI, Community Toolkit, and UI.WPF.Modern style ports.

The first FW batch covers button and command-button controls: `FWButton`, `FWRepeatButton`, `FWHyperlinkButton`, `FWDropDownButton`, `FWSplitButton`, `FWToggleSplitButton`, `FWAppBarButton`, `FWAppBarToggleButton`, and `FWAppBarSeparator`.

The second FW batch covers switch controls: `FWToggleButton` and `FWToggleSwitch`.

The third FW batch covers drag and range controls: `FWSlider`, `FWRangeSlider`, `FWProgressBar`, and `FWProgressRing`. `FWProgressRing` is owned by FluentJalium because Jalium.UI does not currently expose a concrete ProgressRing control.

The fourth FW batch covers selection controls: `FWCheckBox`, `FWRadioButton`, `FWComboBox`, and `FWComboBoxItem`.

The fifth FW batch covers collection and table controls: `FWListBox`, `FWListBoxItem`, `FWListView`, `FWListViewItem`, `FWTreeView`, `FWTreeViewItem`, `FWDataGrid`, and `FWTreeDataGrid`.

The sixth FW batch covers navigation controls: `FWNavigationView`, `FWNavigationViewItem`, `FWNavigationViewItemHeader`, `FWNavigationViewItemSeparator`, `FWTabControl`, `FWTabItem`, and `FWFrame`.

The seventh FW batch covers date and time controls: `FWDatePicker`, `FWTimePicker`, and `FWCalendar`.

The eighth FW batch covers notification and status controls: `FWInfoBar`, `FWInfoBadge`, `FWToastNotificationItem`, `FWToastNotificationHost`, `FWStatusBar`, and `FWStatusBarItem`.

The ninth FW batch covers text input controls: `FWTextBox`, `FWPasswordBox`, `FWNumberBox`, `FWAutoCompleteBox`, and `FWRichTextBox`.

The tenth FW batch covers menu and popup command controls: `FWMenuBar`, `FWMenuBarItem`, `FWMenu`, `FWMenuItem`, `FWContextMenu`, `FWMenuFlyoutItem`, `FWToggleMenuFlyoutItem`, and `FWMenuFlyoutSeparator`. `MenuFlyout`, `MenuFlyoutSubItem`, and `CommandBarFlyout` remain Jalium.UI sealed flyout types, so FluentJalium styles their item surface instead of pretending to inherit them.

The eleventh FW batch covers disclosure, tooltip, dialog, and lightweight container controls: `FWExpander`, `FWToolTip`, `FWContentDialog`, and `FWGroupBox`.

The twelfth FW batch covers visual and icon foundation controls: `FWImage`, `FWFontIcon`, `FWSymbolIcon`, `FWPathIcon`, `FWViewbox`, `FWLabel`, and `FWSeparator`.

The thirteenth FW batch covers interaction and scrolling controls: `FWScrollViewer`, `FWSwipeControl`, and `FWGridSplitter`. `SwipeItem` and `SwipeItems` remain Jalium.UI sealed behavior types, so FluentJalium styles and hosts them through `FWSwipeControl` rather than wrapping them.

The fourteenth FW batch covers advanced input and media controls: `FWColorPicker`, `FWInkCanvas`, `FWInkPresenter`, and `FWMediaElement`.

The fifteenth FW batch covers content and layout foundation controls: `FWTextBlock`, `FWAccessText`, `FWBorder`, `FWContentControl`, `FWContentPresenter`, `FWStackPanel`, `FWWrapPanel`, and `FWGrid`.

The sixteenth FW batch covers advanced WinUI 3 controls: `FWAnimatedIcon`, `FWAnimatedVisualPlayer`, `FWItemsRepeater`, `FWRefreshContainer`, `FWScroller`, `FWAnnotatedScrollBar`, and `FWBackdrop`.

The gallery application uses a sidebar paged layout so each control batch can be reviewed independently, similar to WinUI Gallery, Community Toolkit Gallery, UI.WPF.Modern Gallery, and Jalium UI Gallery.

## New Controls (Batch 16)

### Motion and Animation
- **FWAnimatedIcon**: Displays animated Fluent icons with state-based transitions
- **FWAnimatedVisualPlayer**: Plays Lottie-like vector animations with playback controls

### Advanced Collections
- **FWItemsRepeater**: High-performance virtualizing list control with flexible layouts
  - Supports custom `VirtualizingLayout` strategies
  - Built-in `StackLayout` and `UniformGridLayout`
  - Element animations support

### Interaction
- **FWRefreshContainer**: Pull-to-refresh functionality for scrollable content
  - Supports multiple pull directions (TopToBottom, BottomToTop, etc.)
  - Custom refresh visualizers
  - Async deferral support
- **FWScroller**: Advanced scrolling with snap points and chaining
  - Scroll mode configuration (Enabled/Disabled/Auto)
  - Chaining and railing modes
  - Zoom support with min/max factors
  - Snap points (Optional, Mandatory, OptionalSingle, MandatorySingle)
- **FWAnnotatedScrollBar**: Enhanced scrollbar with position markers
  - Visual labels at scroll positions
  - Detail label events
  - Custom label types (Default, Warning, Error, Info)

### Materials and Effects
- **FWBackdrop**: Background material effects
  - Acrylic: Semi-transparent blur effect
  - Mica: Subtle texture material
  - MicaAlt: Darker variant for contrast
  - Tabbed: Optimized for tabbed interfaces
- **FWAcrylicBrush**: Reusable acrylic brush with tint and luminosity controls

## Usage

Call `FluentThemeManager.Apply(app)` after creating your Jalium `Application`:

```csharp
var app = new Application();
FluentThemeManager.Apply(app);
```

JALXAML consumers can also merge the theme dictionary directly:

```xml
<ResourceDictionary Source="/FluentJalium;component/Themes/Generic.jalxaml" />
```

FW controls can be used from the Fluent namespace:

```xml
<fluent:FWButton Content="Save" />
<fluent:FWDropDownButton Content="More" />
<fluent:FWTextBox PlaceholderText="Enter text" />
<fluent:FWPasswordBox PlaceholderText="Password" />
<fluent:FWNumberBox Minimum="0" Maximum="100" Value="42" />
<fluent:FWAutoCompleteBox PlaceholderText="Search" />
<fluent:FWRichTextBox />
<fluent:FWToggleSwitch Header="Sync" IsOn="True" />
<fluent:FWProgressRing IsIndeterminate="False" Value="72" />
<fluent:FWComboBox PlaceholderText="Choose an item" />
<fluent:FWListBox SelectedIndex="0" />
<fluent:FWDataGrid AutoGenerateColumns="True" />
<fluent:FWNavigationView PaneTitle="FluentJalium" />
<fluent:FWTabControl SelectedIndex="0" />
<fluent:FWFrame />
<fluent:FWMenuBar />
<fluent:FWMenu />
<fluent:FWContextMenu />
<fluent:FWMenuFlyoutItem Text="Open" />
<fluent:FWToggleMenuFlyoutItem Text="Show details" IsChecked="True" />
<fluent:FWExpander Header="Details" IsExpanded="True" />
<fluent:FWToolTip Content="Helpful detail" />
<fluent:FWContentDialog Title="Save changes?" PrimaryButtonText="Save" CloseButtonText="Cancel" />
<fluent:FWGroupBox Header="Options" />
<fluent:FWImage Stretch="UniformToFill" />
<fluent:FWFontIcon Glyph="&#xE72D;" />
<fluent:FWSymbolIcon Symbol="Save" />
<fluent:FWPathIcon />
<fluent:FWViewbox Stretch="Uniform" />
<fluent:FWLabel Content="Name" />
<fluent:FWSeparator />
<fluent:FWScrollViewer VerticalScrollBarVisibility="Auto" />
<fluent:FWSwipeControl />
<fluent:FWGridSplitter ResizeDirection="Columns" />
<fluent:FWColorPicker />
<fluent:FWInkCanvas />
<fluent:FWInkPresenter />
<fluent:FWMediaElement LoadedBehavior="Manual" />
<fluent:FWTextBlock Text="Hello FluentJalium" />
<fluent:FWAccessText Text="_Open" />
<fluent:FWBorder CornerRadius="6" Padding="12" />
<fluent:FWContentControl Content="Hosted content" />
<fluent:FWContentPresenter Content="{Binding}" />
<fluent:FWStackPanel Spacing="8" />
<fluent:FWWrapPanel HorizontalSpacing="8" VerticalSpacing="8" />
<fluent:FWGrid RowSpacing="8" ColumnSpacing="8" />
<fluent:FWAnimatedIcon Source="{StaticResource MyAnimatedIcon}" AutoPlay="True" />
<fluent:FWAnimatedVisualPlayer Source="{StaticResource LottieAnimation}" IsLooping="True" />
<fluent:FWItemsRepeater ItemsSource="{Binding Items}">
  <fluent:FWItemsRepeater.Layout>
    <fluent:UniformGridLayout MinItemWidth="200" MinItemHeight="150" />
  </fluent:FWItemsRepeater.Layout>
</fluent:FWItemsRepeater>
<fluent:FWRefreshContainer RefreshRequested="OnRefreshRequested">
  <ScrollViewer>
    <!-- Content -->
  </ScrollViewer>
</fluent:FWRefreshContainer>
<fluent:FWScroller VerticalSnapPointsType="Mandatory" ZoomMode="Enabled">
  <fluent:FWScroller.Content>
    <!-- Zoomable content -->
  </fluent:FWScroller.Content>
</fluent:FWScroller>
<fluent:FWAnnotatedScrollBar Orientation="Vertical" Labels="{Binding ScrollLabels}" />
<fluent:FWBackdrop Type="Acrylic" TintColor="#F3F3F3" TintOpacity="0.8" />
<fluent:FWDatePicker Header="Date" SelectedDateFormat="Long" />
<fluent:FWTimePicker Header="Time" MinuteIncrement="15" />
<fluent:FWCalendar FirstDayOfWeek="Monday" />
<fluent:FWInfoBar Title="Saved" Message="Your changes were applied." Severity="Success" />
<fluent:FWInfoBadge Value="42" Severity="Informational" />
<fluent:FWToastNotificationHost MaxVisibleToasts="3" />
<fluent:FWToastNotificationItem Title="Build complete" Severity="Success" />
<fluent:FWStatusBar>
  <fluent:FWStatusBarItem Content="Ready" />
</fluent:FWStatusBar>
```

---

## Documentation

### Core Documentation
- **[Getting Started](docs/GETTING_STARTED.md)** - Installation, setup, and first steps
- **[API Reference](docs/API_REFERENCE.md)** - Complete API documentation with examples
- **[Performance Testing](PERFORMANCE.md)** - Benchmarks and optimization strategies
- **[Development Progress](PROGRESS.md)** - Feature roadmap and completion status

### Guides & Tutorials
- **Migration Guide** - Migrating from standard Jalium.UI controls
- **Theming Guide** - Customizing colors, density, and visual styles
- **Best Practices** - Performance, accessibility, and code quality

---

## Development Status

FluentJalium is currently in **active development** (v0.1.0-preview.1). 

**Current Phase**: Batch 16 completed with 150+ controls
- ✅ Unit tests for all Batch 16 controls
- ✅ Gallery demonstration pages
- ✅ Control template implementations
- ✅ Performance testing guide
- ✅ Complete API documentation

See [PROGRESS.md](PROGRESS.md) for detailed roadmap and completed features.

---

## Build

```powershell
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj -c Debug
dotnet build samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj -c Debug
```

By default the repository references the sibling `../Jalium.UI` source tree. To validate against published packages instead:

```powershell
dotnet build FluentJalium.slnx -c Debug /p:UseJaliumSourceReferences=false
```

When the gallery uses sibling Jalium.UI source references, it still copies the matching Jalium.UI.Interop native runtime from the configured NuGet package by default. This keeps local runs from loading stale native binaries from `../Jalium.UI/src/native/bin/native/Debug`, which can otherwise fail with missing native entry points after the managed interop surface changes. After rebuilding the sibling Jalium.UI native binaries, this fallback can be disabled:

```powershell
dotnet run --project samples/FluentJalium.Gallery -c Debug /p:UseJaliumPackageNativeRuntime=false
```
