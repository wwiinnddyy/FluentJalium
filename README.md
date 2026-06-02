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
