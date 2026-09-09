# FluentJalium Gallery

This sample is a clean Gallery implementation built from the FluentJalium controls themselves.
It follows Fluent Design System principles: NavigationView shell, Mica window backdrop, semantic
theme resources, 4 px spacing rhythm, responsive pane behavior, visible keyboard focus, and
material surfaces for content hierarchy.

The visual tree is Jalxaml-first. Window chrome, NavigationView shell, page frame, example cards,
catalog cards, and representative samples are declared in `.jalxaml`; code-behind is reserved for
catalog discovery, data-series setup, command wiring, and state changes. This mirrors the separation
used by the Jalium.UI Gallery while following the information architecture of WinUI Gallery and
FluentAvalonia's controls gallery.

## Structure

- `MainWindow.cs` — Mica-backed desktop window.
- `Shell/GalleryShell.cs` — responsive NavigationView shell and route host.
- `Pages/GalleryPages.cs` — small, discoverable catalog of real control examples.
- `Controls/GalleryHero.cs` and `Controls/GalleryLinkCard.cs` — reusable home-page surfaces.
- `Controls/GalleryIcon.cs` — first-frame-safe icon adapter. It uses FluentJalium `FluentIcon`
  controls with the Windows compatibility font for shell chrome, avoiding a DirectWrite font
  collection race while embedded Fluent System Icons are installed.
- `Pages/BasicInputSample.jalxaml`, `ChartSample.jalxaml`, `MenuSample.jalxaml`, and
  `VisualSample.jalxaml` — representative interactive samples with code-behind only for state
  and data setup.

Run with:

```powershell
dotnet run --project samples/FluentJalium.Gallery/FluentJalium.Gallery.csproj
```
