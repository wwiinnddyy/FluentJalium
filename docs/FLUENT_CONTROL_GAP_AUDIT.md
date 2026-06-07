# FluentJalium Fluent control gap audit

Snapshot date: 2026-06-08

This audit compares the current FluentJalium `FW*` surface with WinUI, WPF UI, UI.WPF.Modern, FluentAvalonia, and Community Toolkit patterns. The current direction is not raw control count. Most first-wave and second-wave controls now exist; the remaining work is to deepen WinUI-style semantics, improve Gallery discoverability, and turn metadata into useful navigation/source/sample-code experiences.

## Reference baselines

- WinUI controls catalog: https://learn.microsoft.com/en-us/windows/apps/develop/ui/controls/
- WPF UI NavigationView and SnackbarService: https://wpfui.lepo.co/documentation/navigation-view.html, https://wpfui.lepo.co/api/Wpf.Ui.SnackbarService.html
- UI.WPF.Modern NavigationView: https://docs.inkore.net/en-us/ui-wpf-modern/components/navigation/navigation-view/
- FluentAvalonia SettingsExpander and InfoBar: https://amwx.github.io/FluentAvaloniaDocs/pages/Controls/SettingsExpander, https://amwx.github.io/FluentAvaloniaDocs/pages/Controls/InfoBar
- Community Toolkit SettingsExpander: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/windows/settingscontrols/settingsexpander

## Current strengths

- Buttons, selection, status, layout basics, materials, shell, range/progress, text input, charts, and visual primitives all have `FW*` public surfaces.
- Gallery has a central catalog, grouped NavigationView shell, All/New/Updated/Preview/Diagnostic filters, visible source/sample metadata, and real sample pages for many control groups.
- Material work is a FluentJalium strength: DWM/window-material profiles, surface presets, Mica/Acrylic/LiquidGlass recipes, and WinUI-compatible aliases are already deeper than a flat token skin.
- Recent work added first-wave WinUI semantic gaps, second-wave ecosystem controls, visible Gallery samples, chart coverage, Navigation shell coverage, Date/Time and GridView coverage, and promoted `FWRelativePanel` from a Grid wrapper to a real relative layout panel.

## High-value gaps

### Gallery visibility gaps

- Text depth: `FWAutoSuggestBox` is visible by WinUI name, but query/text/suggestion-submission semantics should continue to deepen beyond the current `AutoCompleteBox` compatibility layer.
- Data depth: DataGrid/List/GridView now have direct Gallery coverage, but richer loading, empty, grouped, and high-density comparison states would make the collection story stronger.

### Semantic-depth gaps

- `FWSettingsCard`: add Click/Command, keyboard invocation, action alignment, and command-state semantics.
- `FWSettingsExpander`: evolve beyond an Expander with extra header metadata into a settings item collection host.
- `FWSnackbar`: add host/service semantics, queueing, auto-dismiss timer, placement, and action command support.
- `FWTaskDialog`: add awaitable result flow, modal host/overlay behavior, default/cancel button handling, and focus/escape behavior.
- `FWAutoSuggestBox`: deepen query/text/suggestion-submission semantics beyond naming compatibility.
- `FWItemsRepeater`: the API exists, but virtualization/recycling behavior remains the larger WinUI-style gap.
- NavigationView shell: consider WPF UI style navigation service/page service/history patterns for app-shell ergonomics.

## Next implementation batches

1. AutoSuggestBox semantics package: deepen query/text/suggestion-submission behavior beyond the current Jalium `AutoCompleteBox` compatibility layer.
2. Settings semantics package: continue deepening `FWSettingsCard` command/click behavior and `FWSettingsExpander` item-host semantics where the current API is still thin.
3. Snackbar/TaskDialog semantics package: continue deepening service/host result-flow ergonomics, especially modal focus, escape/cancel, and queue lifetime behavior.
4. Collection state package: extend FWListView/FWGridView/FWDataGrid samples with empty/loading/grouped/high-density states once the current direct `FWGridView` coverage is verified.

## Recently completed batches

- Visuals coverage: `FWPersonPicture`, `FWMarkdown`, `FWQRCode`, and shape controls are visible in Gallery.
- Navigation Gallery coverage: `FWBreadcrumbBar`, `FWPipsPager`, `FWSelectorBar`, `FWTabView`, and `FWTitleBar` now have direct examples.
- Charts Gallery coverage: the `FW*Chart` family has a dedicated Gallery entry/page.
- Catalog metadata activation: `SourcePath`, `SampleCodeKey`, API/base/related controls, docs, and registry-backed sample code are visible in `GalleryHostPage`.
- Date/Time and Collections coverage: `FWCalendarDatePicker`, `FWCalendarView`, `FWGridView`, and `FWGridViewItem` have direct Gallery samples and catalog metadata.
- Text input visibility: `FWAutoSuggestBox` has direct Gallery coverage, catalog metadata, and registry-backed sample code while preserving `FWAutoCompleteBox` as the Jalium base.
- Disclosure coverage: `FWTeachingTip` has a default popup presenter style, targeted tests, catalog metadata, and registry-backed sample code for placement/action/close states.
- Materials coverage: `FWFluentWindowSurface` and derived material surfaces such as `FWLayerSurface`, `FWMicaSurface`, `FWAcrylicSurface`, `FWCardSurface`, `FWFlyoutSurface`, and `FWFocusGlassSurface` are visible in Gallery with catalog metadata and copyable recipes.
