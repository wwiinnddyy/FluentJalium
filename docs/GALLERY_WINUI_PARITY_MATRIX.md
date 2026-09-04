# Gallery WinUI Parity Matrix (Phase 0 Baseline)

Pixel-perfect 1:1 target: FluentJalium Gallery reproduces the WinUI Gallery UI/UX,
adapted only where the control set differs (FW-prefixed controls). Search stays in
the content-area header for Phases 1-6; moving it into a custom TitleBar is
deferred to a later phase (see §7).

---

## 1. Frozen baselines

| Side | Commit | Date | Note |
|------|--------|------|------|
| WinUI-Gallery (`C:\git\Jalium\WinUI-Gallery`) | `8854551b0464b6ad824d6b9e1c79933888f14acc` | 2026-08-14 | `Add ListView ScrollIntoView and ListViewPersistenceHelper samples (#2179)` |
| FluentJalium (`C:\git\Jalium\FluentJalium`, branch `new`) | `594a697455f8695e395c3d48a21839d91eeded18` | 2026-09-04 | `spike(winui): probe x-bind interception via bind extension`; tree clean at freeze |

WinUI reference scope at freeze: `WinUIGallery/MainWindow.xaml`, `Pages/` (Home,
Item, AllControls, Section, SearchResults, Settings), `Controls/` (ControlExample,
PageHeader, SampleCodePresenter, CopyButton, HomePage/, DesignGuidance/,
HorizontalScrollContainer), `Models/ControlInfoData.cs (+ Category.cs, IconData.cs)`,
`Samples/` (**121 sample directories, 480 `.txt` SampleDefinition files**).

---

## 2. Shell / MainWindow

| # | WinUI source | Fluent counterpart | Gap at freeze | Phase |
|---|--------------|--------------------|---------------|-------|
| S1 | `MainWindow.xaml:12-14` `MicaBackdrop` | `samples/FluentJalium.Gallery/MainWindow.cs:24-30` `FWBackdrop{Mica}` | Close; keep | — |
| S2 | `MainWindow.xaml:23-60` TitleBar (icon, back, pane toggle, `AutoSuggestBox MaxWidth=580`, Ctrl+F) | `Shell/GalleryShell.cs:289-310` content-area `FWAutoSuggestBox Width=580` | No custom TitleBar; agreed: keep content-area search through Phase 6 | 7 (deferred) |
| S3 | `MainWindow.xaml:137-146` `NavigationView + Frame` | `Shell/GalleryShell.cs:102-149` `FWNavigationView{Left, OpenPaneLength=320, CompactPaneLength=48} + FWFrame{CacheSize=1} + FWTransitioningContentControl{Entrance}` | Structure mirrors; style states need alignment | 1 |
| S4 | `MainWindow.xaml:152-292` nav tree: Home / Fundamentals(7) / Design(5) / Accessibility(3) / `NavigationViewItemHeader "Controls"` / All | `Models/GalleryNavigationGroup.cs:7-30` custom groups (Home, Catalog, Design, Control surfaces, Input, Layout and media, Collections and data, Materials, Motion, App structure; Diagnostics footer-only) + `Models/GalleryCatalog.cs:24-69` | No Fundamentals / Accessibility groups, no "Controls" header, `navigation` entry commented out (`GalleryCatalog.cs:54`, `GalleryCatalogService.cs:65-66`) | 1 |
| S5 | `MainWindow.xaml:53-57` Ctrl+F accelerator; back/forward convention | `Shell/GalleryShell.cs:506-534` Ctrl+F focus, Alt+Left/Right history; `<980 → LeftCompact` (`490-504`) | Behavior mirrors; keep metrics | 1 |
| S6 | `MainWindow.xaml:62-135` automation helpers (`__CurrentPage`, `__GoBackInvoker`, `__CloseAppInvoker`, `__WaitForIdleInvoker`, `__IdleStateEnteredCheckBox`, `__ErrorReportingTextBox`, `__LogReportingTextBox`, `__ViewScalingCheckBox`, `__WaitForDebuggerInvokerButton`, `__DebuggerAttachedCheckBox`, `__UnhandledExceptionReportingTextBox`, `__TestContentLoadedCheckBox`) | None | No hidden UIA test hooks | 1 |
| S7 | Live nav path `NavigationView → Frame(ItemPage/SectionPage/SearchResultsPage)` | `Shell/GalleryShell.cs:448-460` `SelectPage → FWFrame.Navigate(GalleryItemHostPage)`; `Shell/GalleryItemHostPage.cs:13-17` lazy Content assignment (d7b3000) | Live item path is `GalleryItemHostPage + GalleryItemView`; richer `GalleryHostPage.cs` metrics exist but are not on the live path (see §4) | 1, 3 |

---

## 3. Home

| # | WinUI source | Fluent counterpart | Gap at freeze | Phase |
|---|--------------|--------------------|---------------|-------|
| H1 | `Pages/HomePage.xaml:59` `HomePageHeader` (`Controls/HomePage/HomePageHeader.xaml`) | `Pages/GalleryOverviewPage.cs:90-157` custom hero banner | No `HomePageHeader`; hero is Fluent-custom, not WinUI layout | 2 |
| H2 | `Pages/HomePage.xaml:61-77` `SelectorBar` Recent/Favorites (`TokenViewSelectorBarStyle`) | None | No Recent/Favorites switcher | 2 |
| H3 | `Pages/HomePage.xaml:82-128` Recently-visited horizontal scroller + Recently-added-or-updated `GridView` | `Pages/GalleryOverviewPage.cs:51-88` static quick-link strip | No visited/updated rails, no `HorizontalScrollContainer` equivalent | 2 |
| H4 | `Pages/HomePage.xaml:129-165` Favorites `GridView` + `No favorites yet` fallback; `170-187` VSM show/hide states | `Services/GalleryRecentSamplesService.cs` visits only | No `GalleryFavoritesService` (persisted favorites), no empty states | 2 |
| H5 | `Controls/HomePage/Tile.xaml` + `GridViewItemStyle` 8px (`HomePage.xaml:23`, `AllControlsPage.xaml:55`) | `Controls/GalleryTile.jalxaml(.cs)` | Tile exists; GridView item style/padding parity unverified side-by-side | 2 |

---

## 4. Item page / PageHeader / Section / Search

| # | WinUI source | Fluent counterpart | Gap at freeze | Phase |
|---|--------------|--------------------|---------------|-------|
| I1 | `Pages/ItemPage.xaml:37-40` header margin `36,24,36,0`; `:53-61` description `MaxWidth=1064`; `:62` content frame; `:69-74` wide content `1028` left; `:76-91` narrow margins `16,12,16,0`, padding `16,0,16,16` (`Breakpoint640Plus`) | Live: `Shell/GalleryItemView.jalxaml:8` content margin `36,0,36,36`, `DescriptionPresenter MaxWidth=1064 (:45-50)` — header margin `36,24,36,0` missing, no narrow-state overrides. Rich (off-path): `Shell/GalleryHostPage.cs:76-104` matches header `36,24,36,0` + content `MaxWidth=1028` left | Live view misses header top margin + narrow states; `GalleryHostPage` metrics are the reference for porting | 3 |
| I2 | `Controls/PageHeader.xaml:35-41` Title (HeadingLevel1) | `Shell/GalleryItemView.jalxaml:11-20` title + `?` button | Title present; API affordance is a `?` toggle, not WinUI flyout | 3 |
| I3 | `Controls/PageHeader.xaml:42-94` API flyout: Namespace + `BreadcrumbBar` BaseClasses | `Shell/GalleryItemView.jalxaml:22-37` `InfoExpander` with plain-text namespace/inheritance | No flyout, no BreadcrumbBar chain | 3 |
| I4 | `Controls/PageHeader.xaml:98-121` Documentation dropdown (`Item.Docs`) | `Shell/GalleryHostPage.cs:190-208` copy-URL buttons (off-path) | Live view has no Docs dropdown | 3 |
| I5 | `Controls/PageHeader.xaml:122-221` Source dropdown (control source + page XAML/C# GitHub links) | `Models/GalleryCatalog.cs` `SourcePath` metadata only | No Source dropdown on live path | 3 |
| I6 | `Controls/PageHeader.xaml:224-231` per-sample ThemeButton | `Shell/GalleryItemView.jalxaml:39-43` + `.cs:59-67` global theme cycle (Dark→Light→HighContrast) | Button exists but cycles the app theme instead of the sample scope (`SampleThemeListener` equivalent missing) | 3 |
| I7 | `Controls/PageHeader.xaml:237-262` CopyLink + TeachingTip; `:263-275` Favorite toggle | None on live path | No deep-link copy, no favorites | 3 |
| I8 | `Pages/SectionPage.xaml:25-49` group `GridView` (title `36,24,16,24`, padding `36,0,36,0`) | None | No section page | 3 |
| I9 | `Pages/SearchResultsPage.xaml:13-50` Top-pane filter nav + `ResultsGridView` padding `36,24,36,36` + `No results` state | `Shell/GallerySearchEmptyPage.cs:19-64` text-only empty state; `Shell/GalleryShell.cs:312-367` search filters nav + ≤8 suggestions | No results grid, no filter nav | 3 |
| I10 | Deep link `winui3gallery://` style routing | None (`fluentjalium://sample?uniqueId=` proposed, not implemented) | No protocol/deep-link handling | 3 |

---

## 5. ControlExample / SampleCodePresenter

| # | WinUI source | Fluent counterpart | Gap at freeze | Phase |
|---|--------------|--------------------|---------------|-------|
| C1 | `Controls/ControlExample.xaml:69-74` header (HeadingLevel3) | `Controls/ControlExample.jalxaml:8-10` header text | Present; parity visual check pending | 4 |
| C2 | `Controls/ControlExample.xaml:102-155` Example / Output (`MaxWidth=320`) / Options (`MaxWidth=320`) 3-column grid | `Controls/ControlExample.jalxaml:18-56` 3-column grid | Structure mirrors; Options width cap + responsive behavior missing (see C7) | 4 |
| C3 | `Controls/ControlExample.xaml:159-167` `Expander{Source code}` + `:173-191` XAML/C# `SelectorBar` | `Controls/ControlExample.jalxaml:59-77` `FWExpander` + `FWSelectorBar` + plain `TextBlock` | Selector exists but never auto-hides empty tabs and never selects first visible (WinUI `ControlExample.xaml.cs:206-240`) | 4 |
| C4 | `Controls/ControlExample.xaml.cs:104-137` `Xaml/XamlSource/CSharp/CSharpSource/SampleDefinition/Substitutions/ExampleHeight/.../SourceCodeVisibility` DPs | `Controls/ControlExample.jalxaml.cs:12-76` `HeaderText/Example/Output/Options/XamlCode/CSharpCode` only | Missing two-source (inline/file), `SampleDefinition`, `Substitutions $(Key)`, `ExampleHeight`, `SourceCodeVisibility` | 4 |
| C5 | `Controls/ControlExample.xaml.cs:253-339` `.txt` SampleDefinition loader (`--- header/xaml/c#` sections) | None (WinUI has 480 `.txt`; Fluent has no SampleDefinition assets) | No file-driven samples | 4, 6 |
| C6 | `Controls/SampleCodePresenter.xaml:18-43` scrollable code + right-top Copy overlay; `:44-55` XAML/C#/Inline states | `Controls/CodeViewer.cs:12-114` plain `TextBlock` + 📋/✓ copy | No syntax highlighting, no language states, no overlay styling. (Note: `GALLERY_RENOVATION_REPORT.md` describes a VS Code palette highlighter; the checked-in `CodeViewer` renders plain monospace — report ≠ implementation.) | 4 |
| C7 | `Controls/ControlExample.xaml:221-242` `PhoneLayout<740`: Options drops to `Row=1, ColSpan=2` | None | No responsive stacking | 4 |
| C8 | `Controls/ControlExample.xaml:77-86` version-gated ErrorBlock; `SampleThemeListener` wrapper | None | Missing both | 4 |
| C9 | `Controls/OptionControls.cs` Numeric/Boolean/Enum + binding | WinUI has no direct equivalent (Fluent extension — keep) | Keep; wire into migrated samples | 4, 6 |

---

## 6. Design / Settings / Catalog

| # | WinUI source | Fluent counterpart | Gap at freeze | Phase |
|---|--------------|--------------------|---------------|-------|
| D1 | `Controls/DesignGuidance/ColorSections/` Background/Fill/Signal/Text/Stroke/HighContrast + `ColorTile`, `ColorPageExample`, `InlineColorPicker` | `Pages/GalleryColorsPage.cs` (single page) | Not split into WinUI color sections | 5 |
| D2 | `Controls/DesignGuidance/TypographyControl.xaml` type ramp | `Pages/GalleryTypographyPage.cs` (custom ramp) | Replace with `TypographyControl` equivalent | 5 |
| D3 | Design nav: Color, Geometry, **Iconography**, **Spacing**, Typography (`MainWindow.xaml:212-258`) | `Pages/GalleryGeometryPage.cs`, colors, typography, motion tokens; no Iconography/Spacing pages | Two design pages missing | 5 |
| D4 | `Pages/SettingsPage.xaml:11-68`: `SettingsCardSpacing=4`, header `MaxWidth=1064` margin `36,24,36,0`, content `Padding 36,0,36,0 / MaxWidth 1064`, App-theme ComboBox (`Light/Dark/Use system setting`, AutomationId `themeModeComboBox`), Navigation-style ComboBox, Sound expander | `Pages/GallerySettingsPage.cs:68-121` Theme/Accent/Language/Diagnostics/QA cards | Layout, theme selector pattern, and sound/navigation sections differ; language card is a Fluent extension (keep as extra section) | 5 |
| D5 | `Pages/AllControlsPage.xaml:39-57` `GridView{Padding 24,16,24,36, IndentedGridViewItemStyle, ControlItemTemplate}`; narrow state `:58-76` | `Pages/GalleryCatalogFilterPage.cs:35-52` pills + `FWWrapPanel` cards | No GridView presentation; counts/summary are Fluent extras (keep inside WinUI layout) | 6 |
| D6 | `Models/ControlInfoData.cs` (+ `Category.cs`, `IconData.cs`) | `Models/GalleryCatalog.cs`, `GalleryPageInfo.cs`, `GalleryPage.cs`, `GalleryControlInfo.cs`, `GalleryControlGapCatalog.cs` | Metadata model is richer on Fluent side; mapping to `ControlInfoData` semantics (Docs/BaseClasses/ApiNamespace) needs a conformance note during migration | 6 |
| D7 | i18n: WinUI ships English-only strings | `Services/GalleryLocalizationService.cs`, `Services/LocalizationService.cs`, `Resources/Strings.*`, `I18N_IMPLEMENTATION.md` (en-US/zh-CN/zh-TW) | Fluent extension — keep; Phase 5 extends coverage to all pages without changing WinUI layout | 5 |

---

## 7. Explicitly deferred (not in Phases 1-6)

- Custom TitleBar hosting the search box (`MainWindow.xaml:23-60`). Blocked on
  `Jalium.UI Window` TitleBar extensibility; content-area search stays until then.
- FW controls with no WinUI counterpart (charts, data inspectors, selectors):
  keep the existing Fluent catalog sections; do not force them into WinUI groups.

## 8. Screenshot checklist (manual, side-by-side per phase)

- [ ] P1: nav tree order/groups, pane open/compact (320/48), search suggest ≤8, empty-search state, `<980` compact switch, back/forward keys
- [ ] P2: Home header, Recent/Favorites switch, visited rail scroll, updated grid, favorites add/remove + empty states
- [ ] P3: item header margins, Docs/Source menus, API chain, theme toggle scope, copy-link, favorite, section grid, search-results grid + no-results
- [ ] P4: example/output/options proportions, XAML/C# switch, copy feedback, empty-source collapse, `<740` stacking, `.txt` sample render
- [ ] P5: color sections, typography ramp, iconography grid, spacing ruler, settings max-width + card spacing
- [ ] P6: migrated sample pages vs WinUI originals, All grid density, `dotnet build/test` green

*Doc freeze: this file describes the tree at §1. Later phase commits must not
rewrite history here; append a short "Delta" note per phase instead.*

## 9. Phase deltas

### Phase 1 — Shell / Navigation (branch `new`)
- `GalleryNavigationGroup`: added `Fundamentals` + `Accessibility`, ordered ahead of
  `Catalog`; added `FirstControlsGroup` marker. 10 Preview shells
  (resources/styles/binding/templates/customusercontrols/xamlconditions/scratchpad/
  screenreadersupport/keyboardsupport/colorcontrast) with factories via the new
  `Pages/GalleryPlaceholderPage.cs`.
- `navigation` entry re-enabled; factory renders the new
  `Pages/GalleryNavigationPage.cs` (preview shell + covered FW surface index). The
  embedded page file also makes the `navigation.*` sample key resolvable, so the
  VisualQA `navigation` family now passes (baseline: missing page).
- `FluentNavigationView.MenuItems/FooterMenuItems` widened to
  `ObservableCollection<Control>` (WinUI `IList<object>` semantics); shell inserts a
  `FWNavigationViewItemSeparator` + localized "Controls" header ahead of `Catalog`.
- Hidden UIA hooks `__CurrentPage` (UniqueId, updated on every navigation) and
  `__GoBackInvoker` added to the shell root (zero-size, hit-test invisible).
- Search placeholder aligned to WinUI ("Search controls and samples...") in
  resx ×3 + localization dict. (`FWAutoSuggestBox` has no QueryIcon API — gap stays.)
- Verification: Gallery build 0 errors; `FluentGalleryCatalogTests` 17/21 pass on a
  zh-CN runner (baseline-referenced pre-existing failures: EN-literal assertions under
  zh locale, plus source-registry filename mismatches for `layout`,
  `controlgapmatrix`, `visualqacoverage` — all reproduced on the stashed baseline).
  New `GalleryCatalog_ShouldExposeWinUiFundamentalsAndAccessibilityShells` is
  locale-proof and green in both cultures.

### Phase 2 — Home (branch `new`)
- `Pages/GalleryOverviewPage.cs` rewritten to the WinUI Home structure: hero banner
  (title + subtitle, dead buttons removed), localized 6-tile quick-link strip,
  centered `FWSelectorBar` Recent/Favorites switcher, "Recently visited" horizontal
  rail (collapses when empty, like WinUI's NoRecent state), "Recently added or
  updated" grid from `IsNew/IsUpdated` metadata, favorites grid plus the "No
  favorites yet" fallback. Removed theme/accent/type-ramp/material demos (already
  covered by Settings/Typography/Materials pages).
- New `Services/GalleryFavoritesService.cs`: UniqueId-keyed favorites persisted to
  `%LocalAppData%/FluentJalium/Gallery/favorites`, most-recent-first, with change
  notification. The star toggle that writes to it arrives with the Phase 3 PageHeader.
- Rails use `GalleryControlCard` (same card as the filter pages) with WinUI grid
  AutomationIds (`RecentlyVisitedGridView`, `RecentlyAddedAndUpdatedGridView`,
  `FavoriteSamplesGridView`); rail data flows through the headless-testable
  `GalleryHomeSnapshot`.
- Verification: Gallery build 0 errors, warnings unchanged at 30;
  `GalleryHomeTilesTests` 5/5 green; `FluentGalleryCatalogTests` still 17/21 with
  the same 4 pre-existing failures.
