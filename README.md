# FluentJalium

FluentJalium is a Fluent Design System theme and control library for Jalium.UI on .NET 10.

The first milestone provides a Fluent resource layer, Fluent default styles for core Jalium controls, FW-prefixed FluentJalium controls, and a small gallery application for visual validation.

## Architecture

FluentJalium develops in two parallel tracks:

- Theme overlay: `FluentThemeManager.Apply(app)` appends Fluent resources and replaces the default visual style of Jalium-owned controls without modifying `Jalium.UI`.
- FluentJalium controls: controls owned by this library use the `FW` prefix, such as `FWButton` and `FWTextBox`. These controls live in `FluentJalium.Controls`, implement `IFluentJaliumControl`, and are intended to become the Fluent Design System control surface for WinUI, Community Toolkit, and UI.WPF.Modern style ports.

The first FW batch mirrors the base styled Jalium controls: `FWButton`, `FWRepeatButton`, `FWHyperlinkButton`, `FWTextBox`, `FWPasswordBox`, `FWCheckBox`, `FWRadioButton`, `FWToggleButton`, `FWToggleSwitch`, `FWSlider`, `FWProgressBar`, `FWProgressRing`, and `FWComboBox`. `FWProgressRing` is owned by FluentJalium because Jalium.UI does not currently expose a concrete ProgressRing control.

The current second batch adds FW-prefixed surfaces for `InfoBar`, `NumberBox`, `SplitButton`, `Expander`, `NavigationView`, `CommandBar`, `AppBarButton`, `AppBarToggleButton`, `AppBarSeparator`, `MenuBar`, `MenuBarItem`, `MenuFlyoutItem`, `TabControl`, `ListView`, `TreeView`, `Calendar`, `DatePicker`, and `TimePicker`. Controls with complete Fluent templates are covered by the theme overlay now; larger ports continue in later milestones.

The first independent WinUI-style ports are `FWDropDownButton`, a FluentJalium-owned button that opens a Jalium `FlyoutBase`; `FWToggleSplitButton`, a checked split-button surface with `IsChecked`, `Toggle()`, and `IsCheckedChanged`; `FWInfoBadge`, a lightweight dot, value, or icon badge with Fluent severity resources; and `FWRatingControl`, a WinUI-style rating input with placeholder, clear, read-only, mouse, and keyboard behavior.

## Usage

```csharp
using FluentJalium.Controls.Themes;
using Jalium.UI;

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
<fluent:FWTextBox PlaceholderText="Search" />
```

## Build

```powershell
dotnet build FluentJalium.slnx -c Debug
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj -c Debug
```

By default the repository references the sibling `../Jalium.UI` source tree. To use NuGet packages instead:

```powershell
dotnet build FluentJalium.slnx -c Debug /p:UseJaliumSourceReferences=false
```

When the gallery uses sibling Jalium.UI source references, it still copies the
matching Jalium.UI.Interop native runtime from the configured NuGet package by
default. This keeps local runs from loading stale native binaries from
`../Jalium.UI/src/native/bin/native/Debug`, which can otherwise fail with
missing native entry points after the managed interop surface changes. After
rebuilding the sibling Jalium.UI native binaries, this fallback can be disabled:

```powershell
dotnet run --project samples/FluentJalium.Gallery -c Debug /p:UseJaliumPackageNativeRuntime=false
```
