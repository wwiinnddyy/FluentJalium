# FluentJalium control backlog matrix

Snapshot date: 2026-06-09

This matrix turns the Fluent control gap audit into an executable backlog. WinUI and WinUI Gallery remain the semantic baseline; WPF UI, UI.WPF.Modern, FluentAvalonia, and Community Toolkit settings controls are reference inputs for desktop app patterns. They are not copy targets. FluentJalium keeps its own `FW*` public surface, Jalium base-control restyling, and real material/window behavior.

## Reference roles

| Reference | What FluentJalium should borrow | What FluentJalium should avoid |
| --- | --- | --- |
| WinUI / WinUI Gallery | Control taxonomy, accessible states, naming, Gallery sample metadata, and app-pattern coverage for Fluent Design System controls. | Treating every WinUI-only type as mandatory when Jalium has no native base or when a composition pattern is better. |
| WPF UI | App-shell ergonomics: NavigationView, BreadcrumbBar, SnackbarService-style service wiring, settings/about footer patterns. | Overfitting WPF service APIs into controls that already have better Jalium-native lifetimes. |
| UI.WPF.Modern | Adaptive NavigationView behavior, compact/minimal pane thinking, WinUI-style naming in WPF desktop apps. | Importing API shape without matching Jalium layout/template capabilities. |
| FluentAvalonia | SettingsCard/SettingsExpander semantics, InfoBar-style status affordances, pseudostate discipline, Gallery API tables. | Copying Avalonia-only pseudoclass or template-part contracts verbatim. |
| Community Toolkit | Settings rows, expanders, form recipes, and composable helper patterns around WinUI controls. | Turning helper recipes into heavy framework dependencies. |

## Priority model

| Priority | Meaning | Merge expectation |
| --- | --- | --- |
| P0 | Blocks a credible FluentJalium Gallery or breaks core WinUI parity expectations. | Implement or harden with focused tests before broad visual expansion. |
| P1 | High-value Fluent ecosystem coverage or quality polish for already-public controls. | Batch by control family with Gallery and catalog updates. |
| P2 | Useful compatibility aliases, recipes, or app-pattern pages. | Add when it reduces user friction without expanding maintenance too far. |
| Evaluate | Candidate exists in WinUI/reference libraries, but Jalium base support or FluentJalium fit must be proven first. | Write a short design note or prototype before public API. |

## Implementation backlog

| Priority | Area | Current FluentJalium state | Next work | Suggested files | Test evidence |
| --- | --- | --- | --- | --- | --- |
| P0 | TaskDialog real-window QA | `FWTaskDialog`, host, automation, keyboard diagnostics, and Gallery samples exist. | Validate real app-window focus restore/trap, modal layering, default button focus, Escape/cancel, and light-dismiss behavior in a Gallery-hosted scenario. | `src/FluentJalium/Controls/Disclosure/FWDisclosureControls.cs`, `samples/FluentJalium.Gallery/Pages/GalleryDisclosurePage.cs`, `Themes/Controls/DisclosureControls.jalxaml` | Headless template tests plus a targeted Gallery smoke path that exercises host show/close/focus diagnostics. |
| P0 | ItemsRepeater visual QA | `FWItemsRepeater` has manual/viewport realization, recycler diagnostics, ScrollViewer/FWScroller attachment, and Gallery coverage. | Stress large lists, horizontal virtualization, reattachment, cache tuning, and visual stability in a richer Gallery shell. | `src/FluentJalium/Controls/Collections/FWItemsRepeater.cs`, `samples/FluentJalium.Gallery/Pages/AdvancedCollectionsPage.cs` | Targeted `FWItemsRepeater` tests for reattachment and range stability; Gallery catalog sample-code assertions. |
| P1 | Settings visual QA | `FWSettingsCard` and `FWSettingsExpander` have command, interaction, automation, item host, typography, and Gallery coverage. | Validate final Win11-settings feel: icon/action alignment, focus visuals, disabled rows, hover/click states, and dense settings pages. | `src/FluentJalium/Controls/Layout/FWLayoutControls.cs`, `Themes/Controls/LayoutControls.jalxaml`, `samples/FluentJalium.Gallery/Pages/GalleryDisclosurePage.cs` | Theme style assertions, interaction diagnostics tests, and Gallery page output checks. |
| P1 | Navigation app-shell recipes | `FWNavigationService`, NavigationView route metadata, selector/tab diagnostics, BreadcrumbBar/PipsPager/TitleBar samples exist. | Add a cohesive app-shell Gallery walkthrough combining route provider, breadcrumb, tab document area, footer settings, and search/autosuggest entry. | `samples/FluentJalium.Gallery/Pages/GalleryNavigationPage.cs`, `GalleryCatalog.cs`, `GallerySampleCodeRegistry.cs` | Catalog metadata tests plus navigation-service diagnostics after provider-backed route changes. |
| P1 | Material/window QA | DWM/material/window surfaces are a strength and now have Gallery/test coverage for requested profile, actual `Window.SystemBackdrop`, surface role, material kind, high contrast fallback, inactive-window material state, and unsupported host guards. | Use the existing diagnostics in broader real-window smoke paths, comparing requested/applied backdrop behavior across OS, theme, and host variations without treating the whole material goal as complete. | `src/FluentJalium/Controls/Materials/FWMaterials.cs`, `samples/FluentJalium.Gallery/Pages/GalleryWindowBackdropsPage.cs`, `samples/FluentJalium.Gallery/Pages/GalleryMaterialsPage.cs` | Existing high contrast/inactive/unsupported-host coverage plus theme resource tests, registry sample assertions, and guarded runtime diagnostics for unsupported DWM/material hosts. |
| P1 | Advanced interaction completion | `FWRefreshContainer`, `FWScroller`, and `FWAnnotatedScrollBar` now expose diagnostics and Gallery/test output for refresh deferral cancellation, snap state, annotation hover, and detail presenter state. | Keep extending combined gesture stories and visual explanation across scroller, refresh, and annotation scenarios without marking the broader interaction polish goal complete. | `src/FluentJalium/Controls/Interaction`, `samples/FluentJalium.Gallery/Pages/InteractionControlsPage.cs` | Existing interaction tests for refresh deferral cancellation, snap state, annotation hover/detail presenter, plus event/diagnostic tests for gesture state transitions. |
| P1 | Forms and validation pattern | Forms now has a Gallery scenario page combining labels, text input, AutoSuggestBox, RadioButtons, InfoBar validation, SettingsCard rows, switches, density, and submit/reset commands. | Add real-window visual QA for dense forms, disabled states, focus paths, and async submit progress before considering any reusable `FWForm` abstraction. | `samples/FluentJalium.Gallery/Pages/GalleryFormsPage.cs`, `GalleryCatalog.cs`, `GalleryLocalizationService.cs` | Catalog metadata, pattern sample-code, and control-index ownership tests. |
| P1 | Collection navigation depth | `FWListView`, `FWGridView`, state matrix, `FWItemsRepeater`, and `FWAnnotatedScrollBar` exist; `AdvancedCollectionsPage` now carries a Gallery recipe evaluation matrix for `FWItemsView`, `FWFlipView`, and `FWSemanticZoom` because Jalium does not expose native `SemanticZoom` or `FlipView` bases for thin FW wrappers. | Keep the candidates as Gallery recipes/prototypes while the matrix validates API shape. Selection, paging, grouping, viewport, and virtualization semantics are proven at recipe depth; public controls should wait for stronger selection-model ownership, gesture/animation behavior, two-view synchronization, and automation contracts. | `src/FluentJalium/Controls/Collections`, `samples/FluentJalium.Gallery/Pages/AdvancedCollectionsPage.cs` | Prototype tests should continue proving keyboard navigation, selected item semantics, paging state, grouped views, viewport behavior, virtualization behavior, two-view sync, gesture/animation state, and automation metadata before public API. |
| P2 | Media and web surface | Image, animated visual, media, ink canvas/presenter exist. | Evaluate `FWWebView`, `FWContactCard`, and `FWInkToolbar` only if Jalium base support is available or a wrapper can remain thin. | `src/FluentJalium/Controls/Input`, `src/FluentJalium/Controls/Visuals` | Constructor/style-key tests first; Gallery sample only after runtime host behavior is reliable. |
| P2 | Compatibility aliases | `FWCanvas`, `FWRelativePanel`, `FWBitmapIcon`, `FWImageIcon`, `FWRichTextBlock`, shapes, and visual primitives exist. | Keep alias controls documented as low-risk compatibility and ensure each appears in All Controls with source/sample metadata. | `src/FluentJalium/Controls/Layout`, `src/FluentJalium/Controls/Visuals`, `GalleryCatalog.cs` | Gallery control-index tests for source path, related controls, and sample-code fallback. |
| Evaluate | Data entry helpers | `FWPropertyGrid`, data inspectors, Date/Time, NumberBox, AutoSuggestBox exist. | Decide whether FluentJalium needs explicit validation helpers, masked input, or a DataForm-style recipe. | `src/FluentJalium/Controls/TextInput`, `samples/FluentJalium.Gallery/Pages` | Start with Gallery recipe tests; avoid public API until repeated scenarios emerge. |

## Gallery backlog

| Priority | Gallery deliverable | Why it matters | Acceptance signal |
| --- | --- | --- | --- |
| P0 | Keep All/New/Updated/Preview/Diagnostic filters complete | WinUI Gallery-style discovery is now part of the product surface. | Every `GalleryCatalog` entry has factory, related controls, docs links, source path, and a registered sample key when applicable. |
| P1 | Add scenario pages, not only control cards | Fluent Design System is about usable app patterns: navigation, settings, forms, status, data, and material layering. | Scenario pages combine 3-6 `FW*` controls and expose useful diagnostics without becoming marketing pages. |
| P1 | Code/sample split | FluentAvalonia and WinUI Gallery both make samples inspectable; FluentJalium should keep registry code accurate. | `GallerySampleCodeRegistry` has assertions for every catalog `SampleCodeKey` and does not drift into fallback-generated samples. |
| P1 | Visual QA checklist per control family | Many remaining gaps are visual/state confidence, not missing types. | Visual QA coverage Gallery entry/metadata keeps each family guarded by state, evidence, and sample key; each family has normal, hover, pressed, selected, disabled, focus, light/dark/high contrast, and density coverage where meaningful. |
| P2 | Reference/source links | Users should jump from Gallery to docs/source quickly. | `GalleryControlInfo` exposes current source path, namespace, base classes, related controls, and docs links for all control families. |

## First candidate batches after this matrix

1. TaskDialog real-window QA: narrow scope, high confidence impact, validates modal/focus behavior against Fluent expectations.
2. Navigation app-shell walkthrough: demonstrates WPF UI/UI.WPF.Modern-inspired shell ergonomics with FluentJalium-native route diagnostics.
3. Forms pattern Gallery page visual QA pass: validate dense, disabled, focus, validation, and async submit states after the baseline scenario page.
4. Collection navigation evaluation: use the `AdvancedCollectionsPage` Gallery recipe evaluation matrix to keep `FWSemanticZoom`, `FWFlipView`, and `FWItemsView` as prototypes until the selection model, gesture/animation, two-view sync, and automation contract are public-API ready.

## Guardrails

- New public controls must implement `IFluentJaliumControl`, expose `FW*` names, and have style/theme tests when styled.
- Gallery changes must include catalog metadata, localization, sample code, and focused tests when a page or sample key is added.
- Material/window work must preserve real DWM/material/blur/glass behavior and must not regress into token-only skinning.
- Reference libraries are design evidence, not implementation source. FluentJalium should feel native to Jalium while staying legible to WinUI/WPF UI/FluentAvalonia users.
