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
- Gallery has a central catalog, grouped NavigationView shell, All/New/Updated/Preview/Diagnostic filters, and real sample pages for many control groups.
- Material work is a FluentJalium strength: DWM/window-material profiles, surface presets, Mica/Acrylic/LiquidGlass recipes, and WinUI-compatible aliases are already deeper than a flat token skin.
- Recent work added first-wave WinUI semantic gaps, second-wave ecosystem controls, visible Gallery samples, and promoted `FWRelativePanel` from a Grid wrapper to a real relative layout panel.

## High-value gaps

### Gallery visibility gaps

- Navigation/Shell: `FWBreadcrumbBar`, `FWPipsPager`, `FWSelectorBar`, `FWTabView`, `FWTitleBar`.
- Date/Time: `FWCalendarDatePicker`, `FWCalendarView`.
- Collections: `FWGridView` and richer DataGrid/List/Grid states.
- Text: `FWAutoSuggestBox`.
- Disclosure: `FWTeachingTip`.
- Visuals: `FWPersonPicture`, `FWMarkdown`, `FWQRCode`, shape controls.
- Charts: the full `FW*Chart` family needs a dedicated Gallery entry/page.
- Materials: convenience surfaces such as `FWLayerSurface`, `FWMicaSurface`, `FWAcrylicSurface`, `FWCardSurface`, `FWFlyoutSurface`, `FWFocusGlassSurface`, and `FWFluentWindowSurface` need catalog and visible examples.

### Semantic-depth gaps

- `FWSettingsCard`: add Click/Command, keyboard invocation, action alignment, and command-state semantics.
- `FWSettingsExpander`: evolve beyond an Expander with extra header metadata into a settings item collection host.
- `FWSnackbar`: add host/service semantics, queueing, auto-dismiss timer, placement, and action command support.
- `FWTaskDialog`: add awaitable result flow, modal host/overlay behavior, default/cancel button handling, and focus/escape behavior.
- `FWAutoSuggestBox`: deepen query/text/suggestion-submission semantics beyond naming compatibility.
- `FWItemsRepeater`: the API exists, but virtualization/recycling behavior remains the larger WinUI-style gap.
- NavigationView shell: consider WPF UI style navigation service/page service/history patterns for app-shell ergonomics.

## Next implementation batches

1. Visuals coverage package: `FWPersonPicture` style/tests/Gallery, plus `FWMarkdown`, `FWQRCode`, and shape samples.
2. Navigation Gallery package: `FWBreadcrumbBar`, `FWPipsPager`, `FWSelectorBar`, `FWTabView`, `FWTitleBar` samples and catalog metadata.
3. Settings semantics package: `FWSettingsCard` command/click behavior and `FWSettingsExpander` item-host semantics.
4. Snackbar/TaskDialog semantics package: `FWSnackbarHost` or service and `FWTaskDialog.ShowAsync` style result flow.
5. Charts Gallery package: one dedicated Charts catalog entry with the existing chart family shown as scan-friendly samples.
6. Catalog metadata activation: make `SourcePath`, `SampleCodeKey`, and `RelatedControls` visible, navigable, and useful in `GalleryHostPage` or a shared sample-code registry.

