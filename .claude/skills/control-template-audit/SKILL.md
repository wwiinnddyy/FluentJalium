---
name: control-template-audit
description: Audit a FluentJalium custom Control for the hand-built-tree + aliased-ControlTemplate double-render bug, or diagnose an existing "重影/ghost/doubled controls" visual report. Use when adding or modifying any class under src/FluentJalium/Controls that overrides GetVisualChild/VisualChildrenCount/MeasureOverride/ArrangeOverride (i.e. builds its own visual tree by hand), when adding an entry to FluentThemeManager's AliasStyle<> list, or when a user reports overlapping/ghosted/duplicated controls in the Gallery or any FluentJalium app.
---

## Background: the bug this skill exists to prevent

On 2026-09-10 the Gallery showed heavily ghosted controls — two full navigation UIs
overlaid at different pane/content offsets. Root cause:

`FluentNavigationView` (base of `FWNavigationView`) is a `Control` that builds its
ENTIRE visual tree by hand in `BuildVisualTree()` (`_rootGrid`) and overrides
`VisualChildrenCount` / `GetVisualChild` / `MeasureOverride` / `ArrangeOverride` to
drive that hand-built tree. It never suppressed the base `Control`'s templating
path. Meanwhile `FluentThemeManager.AliasStyle<FWNavigationView, NavigationView>()`
applies the stock `NavigationView` `Style` — which carries a full pane+content
`ControlTemplate` — onto `FWNavigationView`. `Control.ApplyTemplateCore` then builds
a SECOND subtree (`_templateRoot`), and `Control.RenderTemplatedBackground` paints
it every frame on top of the hand-built `_rootGrid`. Two complete, differently
arranged trees = the ghost.

Fix applied to `FluentNavigationView`
(`src/FluentJalium/Controls/Navigation/FluentNavigationView.cs`):

```csharp
// Hand-built controls must never expand a ControlTemplate — see control-template-audit skill.
protected override bool ApplyTemplateCore() => false;
protected override void RenderTemplatedBackground(DrawingContext drawingContext) { }
```

Any other FluentJalium control with the same shape (hand-built tree + aliased to a
templated Jalium style) has the same latent bug. This skill is the repeatable check.

## Part 1 — Static audit (run before committing a new/changed control)

1. **Identify hand-built-tree controls.** A class under `src/FluentJalium/Controls/**`
   is hand-built if it overrides any of: `GetVisualChild`, `VisualChildrenCount`,
   `MeasureOverride`, `ArrangeOverride`, and constructs its own root visual (commonly
   via a `BuildVisualTree()` method calling `AddVisualChild`) from its constructor —
   rather than relying on `Template`/`ApplyTemplate` like a normal styled `Control`.

   ```bash
   grep -rn "protected override .*GetVisualChild\|protected override .*VisualChildrenCount" src/FluentJalium/Controls
   ```

2. **Check whether it is aliased to a templated base style.** Search
   `src/FluentJalium/Themes/FluentThemeManager.cs` for
   `AliasStyle<TheControl, TheJaliumBase>(dictionary)`. If found, open
   `src/FluentJalium/Themes/Controls/*.jalxaml` and check whether the `Style
   TargetType="TheJaliumBase"` (or the FluentJalium-side style it's `BasedOn`) sets a
   `<Setter Property="Template">` with a `<ControlTemplate>`. If the aliased base has a
   `ControlTemplate` AND the FluentJalium subclass hand-builds its tree, that is the
   bug pattern.

3. **If the pattern matches, verify the guard exists** on the hand-built class:

   ```bash
   grep -n "ApplyTemplateCore\|RenderTemplatedBackground" src/FluentJalium/Controls/<Path>/<Control>.cs
   ```

   It must override both:
   ```csharp
   protected override bool ApplyTemplateCore() => false;
   protected override void RenderTemplatedBackground(DrawingContext drawingContext) { }
   ```
   If either is missing, add them. `ApplyTemplateCore() => false` stops
   `Control.ApplyTemplateCore` from calling `Template.LoadContent()` and attaching a
   second subtree at all; the `RenderTemplatedBackground` no-op is defense in depth in
   case a future styling path (a different alias, a user-supplied `Template=` in
   Jalxaml) still manages to set `_templateRoot`.

4. **Do not "fix" this by leaving `Template` unset instead.** `AliasStyle` sets the
   Style (and its `Template` setter) on the *type*, not the instance — an unset
   `Template` property on one instance does not stop the Style from applying it. The
   override on the class is the only reliable guard.

5. Rebuild and run the visual regression protocol in Part 2 before considering the
   audit complete.

## Part 2 — Diagnostic playbook (when a ghost/duplicate-controls report comes in)

Work top-down; each step should change your belief about "compositor artifact" vs
"genuine duplicate draw in the tree" before you move to the next.

1. **Reproduce and screenshot** the reported state (initial load, and after whatever
   interaction — pane toggle, resize, navigation — the report mentions).

2. **Rule out capture artifacts before touching the framework.** `BitBlt`/
   `CopyFromScreen` can show stale/torn pixels on a flip-model DirectComposition
   swapchain that the real compositor has already replaced. Re-capture with
   `PrintWindow(hwnd, dc, PW_RENDERFULLCONTENT /* = 2 */)`, which reads the swapchain
   directly:

   ```powershell
   # PrintWindow(hwnd, memDC, 2) into a compatible bitmap, save as PNG.
   # See git history 2026-09-10 for a full working script if one isn't already
   # checked into diag/ — do not hand-roll BitBlt-based capture for this purpose.
   ```

   If the ghost disappears under `PrintWindow`, it was a capture-side artifact, not a
   framework bug — stop here.

3. **If the ghost survives PrintWindow, rule out the render pipeline's optimizations**
   one at a time (each is a documented env-var escape hatch in Jalium.UI):
   - `JALIUM_DISABLE_RENDER_CACHE=1` — bypasses the retained-drawing cache.
   - `JALIUM_DISABLE_RETAINED_LAYERS=1` — bypasses the GPU retained-layer composite path.
   - `JALIUM_D3D12_FORCE_FULL_REPLAY=1` — forces full-frame re-record every present.
   - `JALIUM_RENDER_THREAD=0` — moves rendering back onto the UI thread, inline.

   If the ghost **disappears** under any single one of these, the bug is in that
   subsystem's damage-tracking / stale-composite logic (Jalium.UI-side, likely
   `Jalium.UI.Core/Visual.cs` layer compositing or `Jalium.UI.Controls/Window.cs`
   dirty-region code) — escalate there, not in FluentJalium.

   If the ghost **persists with all four combined**, it is a genuine duplicate paint
   in the visual tree, not a compositor/caching bug. Go to step 4.

4. **Confirm against a stock Jalium.UI sample.** Run
   `Jalium.UI.samples/Jalium.UI.TransparentBackdropDemo` (or another sample that does
   NOT use FluentJalium) with the same window chrome (transparent background /
   DirectComposition). If it renders cleanly, the bug is FluentJalium-specific — go to
   Part 1 and look for the hand-built-tree + `AliasStyle` pattern on whichever control
   type the ghost's content matches.

5. **Do not trust a `VisualTreeHelper` walk alone** to rule out a duplicate subtree: a
   hand-built control's overridden `GetVisualChild`/`VisualChildrenCount` only exposes
   the tree IT chooses to report. A second `_templateRoot` subtree living on the same
   `Control` instance is invisible to a walk that starts from the hand-built root and
   only follows that override. Instead, add a temporary bounds+text dump keyed off
   `_owner`/the window root (not the suspect control's own root) so any stray subtree
   the base `Control` attached is included.

6. Once the offending control is identified, apply the Part 1 fix, rebuild, and
   re-verify with `PrintWindow` at both the initial layout and after the interaction
   that originally triggered the report (pane collapse/expand, etc.).

7. Revert every temporary diagnostic (env vars, log statements, bounds-dump code,
   locally-copied DLLs into a sample's `bin/`) before finishing. Diagnostics that
   leak into the shipped build (e.g. a `Debug.WriteLine` gated by a prod-facing env
   var) are acceptable only if they already existed before this investigation.
