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

- Workflow depth: Gallery now covers the first-wave control/state matrix broadly; future visibility work should focus on longer end-to-end app patterns and diagnostics rather than raw page presence.
- Metadata parity: first-wave Gallery pages now carry catalog metadata and registered sample-code keys across buttons, switches, range, input/media, menus, selectors/properties, data inspectors, and motion/transitions; remaining Gallery work should focus on richer app-pattern walkthroughs and visual QA rather than basic metadata.

### Semantic-depth gaps

- `FWSettingsCard`: Click/Command, keyboard invocation, hover click mode, command-state restoration, focus/pressed visuals, interaction diagnostics, automation peer metadata, and InvokePattern support now exist; remaining work is mostly action alignment and default template polish.
- `FWSettingsExpander`: item-host APIs now expose direct content rows, item count, collection change events, and add/remove/clear helpers; remaining work is richer item container styling and default data-row templates.
- `FWTwoPaneView` and `FWParallaxView`: actual layout mode, visible-pane state, progress-driven offset state, and diagnostics now exist; remaining work is deeper ScrollViewer/FWScroller source synchronization and visual QA.
- `FWSnackbar`: host queueing, auto-dismiss, action command support, close reasons, result-style async flow, pause-on-hover/focus lifetime, closing cancellation, host/service async result APIs, top/bottom host placement with spacing, overlay/root host presentation, transition timing hooks, queue diagnostics, and animated presenter choreography now exist; remaining work is visual QA and root-window integration refinements.
- `FWTaskDialog`: awaitable result flow, button command DPs, `CommandExecuted` request metadata, default/cancel requests, Escape-to-cancel, default focus, empty/disabled button states, focus target helpers, automation peers, button automation metadata, `FWTaskDialogAutomationDiagnostics`, template visual-tree focus traversal, and `FWTaskDialogHost` modal overlay with light dismiss, focus restore/trap hooks, keyboard diagnostics, and host reentry guards now exist; remaining work is visual QA and root-window focus integration.
- `FWItemsRepeater`: explicit range realization, recycle-pool reuse, realized-index lookup, and diagnostics now exist; remaining work is deeper viewport/scroll-source virtualization on top of the current `Panel`-based API.
- NavigationView shell: consider WPF UI style navigation service/page service/history patterns for app-shell ergonomics.

## Next implementation batches

1. TaskDialog visual QA package: validate real app-window focus restore/trap behavior and modal layering after the automation and template focus traversal base.
2. ItemsRepeater viewport virtualization package: connect realization windows to ScrollViewer/FWScroller viewport state and decide how cache lengths map onto the Jalium.UI virtualizing panel pipeline without breaking the current `Panel` API.
3. Settings visual polish: refine action alignment/default data-row templates after the command, item-host, interaction-state, and automation semantics stabilize.

## Recently completed batches

- Visuals coverage: `FWPersonPicture`, `FWMarkdown`, `FWQRCode`, and shape controls are visible in Gallery.
- Navigation Gallery coverage: `FWBreadcrumbBar`, `FWPipsPager`, `FWSelectorBar`, `FWTabView`, and `FWTitleBar` now have direct examples.
- Charts Gallery coverage: the `FW*Chart` family has a dedicated Gallery entry/page.
- Catalog metadata activation: `SourcePath`, `SampleCodeKey`, API/base/related controls, docs, and registry-backed sample code are visible in `GalleryHostPage`.
- Date/Time and Collections coverage: `FWCalendarDatePicker`, `FWCalendarView`, `FWGridView`, and `FWGridViewItem` have direct Gallery samples and catalog metadata.
- Text input coverage: `FWAutoSuggestBox` has direct Gallery coverage, catalog metadata, registry-backed sample code, `QuerySubmitted`, `SuggestionChosen`, and text-change reason APIs while preserving `FWAutoCompleteBox` as the Jalium base.
- Disclosure coverage: `FWTeachingTip` has a default popup presenter style, targeted tests, catalog metadata, and registry-backed sample code for placement/action/close states.
- Materials coverage: `FWFluentWindowSurface` and derived material surfaces such as `FWLayerSurface`, `FWMicaSurface`, `FWAcrylicSurface`, `FWCardSurface`, `FWFlyoutSurface`, and `FWFocusGlassSurface` are visible in Gallery with catalog metadata and copyable recipes.
- Collection state coverage: `FWListView`, `FWGridView`, `FWListBox`, and `FWDataGrid` now show empty, loading, grouped, compact, comfortable, and spacious states in Gallery with updated catalog search metadata and registry-backed sample code.
- Settings semantics coverage: `FWSettingsCard` now exposes read-only `CanExecute`, restores command-disabled state when commands are removed, supports `ClickMode.Hover`, and has keyboard/command tests; `FWSettingsExpander` now has direct content rows, `ItemCount`, `ItemsChanged`, and add/remove/clear helpers with Gallery and registry coverage.
- Settings interaction coverage: `FWSettingsCard` now exposes `FWSettingsCardDiagnostics`, `IsPointerPressed`, `IsKeyboardPressed`, and `IsInteractionPressed`; the default style consumes pressed, focus, and disabled states so settings rows provide WinUI-style interaction feedback without stale pressed visuals.
- Settings automation coverage: `FWSettingsCard` now exposes `FWSettingsCardAutomationPeer`, `FWSettingsCardAutomationDiagnostics`, resolved automation name/help text, and InvokePattern support for command rows, with Gallery and registry examples showing the automation state.
- Adaptive layout diagnostics coverage: `FWTwoPaneView` now exposes `ActualMode`, `VisiblePane`, `FWTwoPaneViewDiagnostics`, and pane-priority-aware single-pane styling; `FWParallaxView` now exposes `Progress`, `CurrentOffset`, and diagnostics while applying the resolved offset to template content, with Gallery surfacing the state.
- Snackbar result/service coverage: `FWSnackbar` now exposes `FWSnackbarCloseReason`, `LastCloseReason`, and `ShowForResultAsync`; `FWSnackbarHost.Clear` marks visible and pending items as host-cleared; `FWSnackbarService` routes service-style show/enqueue/close/clear calls into a configured host with Gallery, registry, and tests updated.
- TaskDialog command/focus coverage: `FWTaskDialog` now exposes button command DPs and command parameters, records `CommandExecuted` on button request events, respects command `CanExecute`, routes Escape through the configured cancel button, focuses the default template button when opened, and hides/disables empty or unavailable buttons.
- TaskDialog modal host coverage: `FWTaskDialogHost` now hosts a single dialog in an overlay template, routes light dismiss and host Escape through the dialog cancel button, exposes focus restore/trap knobs, restores an explicit focus target on close, and keeps `FWTaskDialog.ShowAsync` result semantics intact.
- TaskDialog automation diagnostics coverage: `FWTaskDialog` and `FWTaskDialogHost` now expose dedicated automation peers, `FWTaskDialogAutomationDiagnostics`, `FWTaskDialogButtonAutomationMetadata`, stable button `AutomationId`/`Name`/`HelpText` metadata, and Gallery/registry examples for inspecting automation state.
- TaskDialog visual-tree focus coverage: `FWTaskDialog` now exposes first/last template focus traversal helpers, keeps default-button focus as the opening priority, lets `FWTaskDialogHost` Tab/Shift+Tab trap walk real focusable content and command buttons, and tests the template path with a headless focus provider.
- ItemsRepeater realization coverage: `FWItemsRepeater` now supports explicit `RealizeRange` windows, `ResetRealizationWindow`, real item-index lookup for realized elements, recycle-pool reuse, and `FWItemsRepeaterDiagnostics` for Gallery/tests while preserving full realization as the default.
- Gallery metadata parity coverage: `buttons`, `switches`, `range`, `inputandmedia`, `menus`, `selectorsandproperties`, `datainspectors`, and `motionandtransitions` now expose `SourcePath`, `BaseClasses`, `ApiNamespace`, `RelatedControls`, and `SampleCodeKey` entries with registry snippets for command surfaces, boolean states, slider/progress, color/ink/media, flyout/command bar, selector/property grid, data inspector, and motion workflows.
- Snackbar lifetime coverage: `FWSnackbar` now exposes cancelable `Closing`, `RequestClose`, pointer/focus/manual auto-dismiss pause state, `WaitForCloseAsync`, and host/service `ShowForResultAsync` / `EnqueueForResultAsync` APIs; Gallery shows pause/resume, close cancellation, service queues, and result-task completion.
- Snackbar host placement coverage: `FWSnackbarHost.Placement` now drives top/bottom content alignment, `Spacing` controls the generated snackbar stack panel, and Gallery exposes placement/alignment/spacing diagnostics with registry and catalog metadata updated.
- Snackbar transition diagnostics coverage: `FWSnackbarHost` now exposes `TransitionProfile`, `SnackbarTransitionDuration`, `TransitionOffset`, `TransitionRequested`, `QueueChanged`, and `GetDiagnostics()` so Gallery and app hosts can observe queue layout and wire Fluent motion without taking over host lifetime semantics.
- Snackbar overlay host coverage: `FWSnackbarOverlayHost` now reuses snackbar host queue/result semantics while presenting through a popup overlay with `OverlayTarget`, `OverlayPlacement`, `IsOverlayOpen`, and `IsOverlayAutoOpenEnabled`; Gallery metadata, sample code, localization keywords, and tests cover root-host service routing.
- TaskDialog host diagnostics coverage: `FWTaskDialogHost` now exposes `FWTaskDialogHostDiagnostics`, `LastKeyboardRequest`, `LastKeyboardRequestHandled`, same-dialog show task reuse, different-dialog rejection tests, and Tab/Shift+Tab focus-trap diagnostics while keeping the modal host template stable.
- Snackbar presenter motion coverage: `FWSnackbar` now exposes `FWSnackbarPresenterState`, presenter opacity/offset/duration/placement diagnostics, delayed template attachment for entrance motion, and close-motion deferral so hosted snackbars remain in the queue until their presenter exit completes; Gallery surfaces the current presenter state.
