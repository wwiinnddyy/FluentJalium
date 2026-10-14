# The launcher

`samples/FluentJalium.Launcher` is the transition host: a small rectangular window that picks a renderer, a 2D path
engine, an anti-aliasing mode, a theme and a language, shows the stage reacting to the ones that can act live, and
then hands the whole set to the Gallery. It exists so the product has somewhere to put its startup choices before
the product itself exists; the main window it opens is still the Gallery.

## What the five settings actually control

The settings divide by when they can take effect, and that split is the runtime's, not this window's.

| Setting | Control | Applies | Why |
|---|---|---|---|
| Renderer | `RenderBackend` — Auto / Vulkan / DirectX (D3D12) | at the handoff | The backend belongs to the process-wide `RenderContext` (`RenderContext.cs:206`). 26.10.9 exposes no per-window backend override — `Window._renderBackendOverride` is private (`Window.cs:383`) — so a live swap would mean retiring the context under the launcher's own render target, which is the pin that keeps it alive. |
| Path engine | `RenderingEngine` — Auto / Vello / Impeller | immediately | `Window.SetRenderingEngineOverride` consumes the change at a safe native draw boundary and invalidates the window (`Window.cs:14090`). DevTools drives the same three-way choice the same way (`DevToolsWindow.cs:6561`). |
| Rendering quality | `PathAntiAliasing` — analytic / 4× / 8× / 16× | at the handoff | `Window.EnsureRenderTarget` reads `RenderQualityOptions.Current.PathAntiAliasing` once, while creating the swap chain (`Window.cs:5769`). Changing it afterwards would change nothing until the next window. |
| Theme | `FluentThemeManager.ApplyTheme` | immediately | The theme manager retints live brush instances in place. |
| Reduce motion | `FluentThemeManager.ReduceMotion` | immediately | It rewrites the published duration rows, which every control re-resolves. The launcher's stage is the one thing that visibly stops. |
| Language | `UiText` | immediately | The catalog and the sweeper are plain managed state. |

The status line shows the running configuration and the pending one side by side rather than implying that a radio
click already moved the renderer. `Auto` is spelled out rather than printed as an enum member, because "what will be
asked for" and "what will run" are different questions.

## The handoff, and why the order is that order

`LauncherWindow.Handoff`:

1. Snapshot the settings and persist them.
2. `new MainWindow()` — the Gallery — and stage its theme and start page.
3. `Application.Current.MainWindow = gallery`. **Before** the close, because the application runs with
   `ShutdownMode.OnMainWindowClose`; the launcher's close would otherwise drop the window count to zero and end the
   process (`Application.cs:221`, `Application.cs:1138`).
4. `Close()`. This stops the render thread and disposes the render target (`Window.cs:3488`), which is the
   `RegisterRenderTarget` pin on the current context. Until it happens the old context cannot be retired.
5. `RenderContext.GetOrCreateCurrent(backend, forceReplace: true)`. Only now is the pin gone, so the displaced
   context is genuinely destroyed instead of merely retired.
6. `DefaultRenderingEngine` and `PathAntiAliasing`, then `gallery.Show()`.

Auto is deliberately not forced at step 5: forcing a replace would retire a working context and rebuild the device
for nothing. An explicit request replaces a context on a different backend, and — because `GetOrCreateCurrent` only
enforces a backend it found available (`RenderContext.cs:228`) — leaves the current one untouched when the machine
cannot provide what was asked for. The status line then reports the backend that actually won.

`MainWindow.SetTheme` is new on the Gallery. The theme is a private field because the window owns it, and a host
that shows the window in-process has no command line to pass a preference through. It is staged rather than applied,
for the same reason `SetStartPage` is: the constructor ends by applying `FluentThemeVariant.System`.

## The stage

`Rendering/RenderPlayground.cs` rebuilds a cubic-bezier rosette, a miter-joined star and four rounded rectangles from
scratch on every frame, and reports the frame rate.

Two reasons it is a full re-tessellation rather than a transform: this runtime does not tick a transform — the
Gallery records that at `samples/FluentJalium.Gallery/MainWindow.jalxaml:104` — and a transform would be folded into
the vertices and never reach the path engine at all, so the frame rate would measure nothing the renderer does.

What is drawn is chosen against the engine boundary rather than for looks: a plain rectangle is drawn by the backend
directly and no engine ever sees it, so the stage is built from cubic fills, a gradient fill, a miter-joined stroke
and corner geometry.

The gradient is `LinearGradientBrush` rather than a flat fill because both engines do real work with one: Impeller
samples it during tessellation, Vello carries it through its compute pipeline. Note that the endpoints are in the
0..1 space of *whatever is being filled* — `RenderTargetDrawingContext` maps them against each geometry's own bounds
(`RenderTargetDrawingContext.cs:1847`) — so the same brush ramps the rosette and the chip independently.

**Known runtime limitation this stage exposes rather than hides:** under Vulkan, the compute-mode Vello path
supports one scene per frame, and a rectangle, text or bitmap drawn after a path in the same frame is painted before
the composite and then covered by it (`vulkan_render_target.cpp:16820`). If the rosette, star and chips ever disagree
in z-order, that is this, not the launcher.

## Localization

Neither Jalium.UI nor FluentJalium shipped any localization facility, so `src/FluentJalium.Localization` adds the
minimum: a catalog, a tree sweep, and a settings file.

**English is the key.** `UiText.T("Renderer")` returns the key unless a translation is registered, so an
untranslated string degrades to readable English rather than to a blank or a resource key, and the coverage gate can
name exactly what is still missing. A second translation for one key is rejected rather than silently accepted,
because that shape of mistake — a copy-pasted row — is invisible at runtime and shows up as one wrong page.

**The sweeper matches the authored English rather than an element name.** The alternative, an `x:Name`-keyed table,
would have meant naming and registering 296 elements and editing that many attributes in an 885-line page. Keying on
the text the markup already declares leaves the English where it belongs, next to the control it describes, and covers
the code-behind's generated menu, command bar and tab labels through the same catalog. Each element's authored text is
captured on the first sweep and remembered, so a language change writes from the original — which is what makes
switching back to English lossless.

**The walk is logical, not visual.** `VisualTreeHelper` was the obvious choice and is the wrong one: the Gallery builds
all thirteen pages in its constructor and attaches one at a time, so a visual walk from the window reaches one page and
misses twelve. `LogicalTreeWalker` reads each container's own child accessor, so it reaches detached subtrees too, and
it cannot cycle.

**Content on objects rather than in the tree** — a `MenuFlyout`'s items, a `ContextMenu`'s, the command bar's generated
buttons — is not reachable by any walk. The Gallery holds those in `_dynamicTextWriters` and rewrites them on a
language change without rebuilding the collections, which would duplicate every row.

**Settings persistence** is `fluentjalium.settings.json` beside the executable, written by name rather than by enum
number so a reordered enum cannot repoint a saved value. Every operation is best effort: a missing, unreadable,
truncated or hand-mangled file all resolve to the shipped defaults, and a value this build does not know costs that one
field rather than the document. A launcher that refuses to start because it could not save a preference is worse than
one that forgets it. A first run with no file takes the operating system's UI language; `--lang` overrides both.

**Technical identifiers stay in English** — token names, control type names, resource keys, enum members, file paths
and version numbers are the vocabulary the rest of `docs/astra` is written in, and translating them would break every
cross-reference. The prose around them is translated.

### The gate

`tests/FluentJalium.Tests/Localization/LocalizationCoverageTests.cs` reads both `.jalxaml` files, collects every
`Text` / `Content` / `Header` / `Title` / `Label` / `Message` literal, and fails on any that has no 简体中文 entry. It
reads the markup rather than a built tree so it needs no window and runs in CI, and it catches the case the sweeper
cannot see at runtime: a string added to the page and never registered. Its reader is a second implementation on
purpose — sharing the product's would let both drift from the file in the same direction, which is the argument
`AstraGalleryCatalogTests` makes about its catalog reader.

A literal that is entirely a numeric character reference (`&#xE713;` on a menu item's glyph) is an icon code point,
not prose, and is excluded.

## Evidence

- Solution builds serially with 0 warnings, 0 errors.
- `dotnet test tests/FluentJalium.Tests --configuration Release` — 1630 passed, 0 failed. The suite takes about
  nine minutes; a shorter timeout reads as a hang, not a failure.
- Launcher run, both languages, screen captured: custom title bar with the product name at the top left, Mica
  backdrop, the stage's three figures and the live frame rate, all five settings, and the start button.
- Handoff driven through UI Automation's `InvokePattern` on the start button: the process kept running and the window
  title changed from `Fluent Design System in Jalium.UI` to `FluentJalium Gallery` — one process, the launcher closed,
  the Gallery shown, no exit in between.
- Chinese text renders with no fallback boxes. The D3D12 glyph atlas keys its cache on `run.fontFace`
  (`d3d12_glyph_atlas.cpp:2140`), so DirectWrite's per-run fallback hands CJK to Microsoft YaHei UI.

## Switches

`--page <id>` chooses the Gallery page to land on, `--lang zh|en` overrides the saved language, and `--skip-launcher`
goes straight to the Gallery on the saved settings — the QA path for testing a handoff without clicking through.

## Open items

- The Gallery is localized, but `Catalog.json` remains the English source of record for the parity tests. The Chinese
  page titles live in `GalleryText`; a data-file second copy would be a second thing to drift.
- The report lines the Gallery composes at runtime are translated where the format string could be reached; a handful
  of interpolated readouts assembled from several fragments are still English in Chinese mode. They are the
  diagnostics rather than the page text, and the gate covers the markup, not the composition.
- The title bar strip paints the framework's own `TitleBarBackground`, so the Mica is visible in the client area but
  not behind the caption. Making the caption translucent is a `TitleBar.jalxaml` change and belongs with a change to
  the control, not to a host.
- `Window.ApplySystemBackdrop` sets `DWMWA_USE_IMMERSIVE_DARK_MODE` to 1 unconditionally
  (`Window.cs:5311`), so the Mica tint does not follow the light theme. That is a runtime observation, recorded here
  rather than worked around.
