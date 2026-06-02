# FluentJalium

FluentJalium is a Fluent Design System theme and control library for Jalium.UI on .NET 10.

The first milestone provides a Fluent resource layer, Fluent default styles for core Jalium controls, FW-prefixed button controls, and a small gallery application for visual validation.

## Architecture

FluentJalium develops in two parallel tracks:

- Theme overlay: `FluentThemeManager.Apply(app)` appends Fluent resources and replaces the default visual style of Jalium-owned controls without modifying `Jalium.UI`.
- FluentJalium controls: controls owned by this library use the `FW` prefix, such as `FWButton`. These controls live in `FluentJalium.Controls`, implement `IFluentJaliumControl`, and are intended to become the Fluent Design System control surface for WinUI, Community Toolkit, and UI.WPF.Modern style ports.

The first FW batch covers button and command-button controls: `FWButton`, `FWRepeatButton`, `FWHyperlinkButton`, `FWDropDownButton`, `FWSplitButton`, `FWToggleSplitButton`, `FWAppBarButton`, `FWAppBarToggleButton`, and `FWAppBarSeparator`.

The second FW batch covers switch controls: `FWToggleButton` and `FWToggleSwitch`.

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
<fluent:FWToggleSwitch Header="Sync" IsOn="True" />
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
