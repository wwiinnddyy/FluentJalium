# Pane-icon recolor: real-pixel A/B (spike/NavIconRecolor)

Instrument: `spike/NavIconRecolor/shoot.ps1` launches the Gallery with the start theme pinned before the window is
built (`ASTRA_START_THEME`) and flips it on a live window after `ASTRA_FLIP_MS` into `ASTRA_FLIP_TO`, grabs the
monitor twice through `spike/VisualQA/grab-screen.ps1`, and crops the pane from the window's own rect. Colours are
counted by `spike/NavIconRecolor/hist.ps1` over the icon column only (physical x 349-415, y 200-610 of the grab;
six 16-DIP glyphs), so the strip holds nothing but pane background, the selection pill and the glyph ink.

An in-process `RenderTargetBitmap` cannot see this defect: it re-runs the render pass, and `IconElement` resolves a
missing foreground by walking to an ancestor `Control` *while drawing*, so a live capture always reports the current
theme. Only the monitor answers what the user sees.

## Without the fix (`nobind` mutant: the hand-off line replaced by an empty block)

| leg | pane background | glyph ink | reading |
|---|---|---|---|
| start Light, before any flip | `#F3F3F3` 21567 | `#1A1A1A` 463 | correct at mount |
| same window after a live flip to Dark | `#202020` 21563 | `#030303` 463, `#0B0B0B` 129, `#050505` 125 | **black glyphs on a dark pane** |

The second row is the reported defect: the labels re-tinted, the icons kept the ink of the theme they first drew
under. The mirror case shows up when the app's first frames are in Dark - an early version of this script applied
the start theme after `window.Show()`, and every icon then measured `#FFFFFF 636` on a `#F3F3F3` pane, i.e. white
glyphs on a light pane. Same defect, opposite polarity: the ink is frozen at first paint.

## With the fix (same commands, hand-off in place)

| leg | pane background | glyph ink | reading |
|---|---|---|---|
| start Light -> flip Dark | `#F3F3F3` 21567 / `#202020` 21563 | `#1A1A1A` 463 -> `#FFFFFF` 545 | follows the theme |
| start Dark -> flip Light | `#202020` 21563 / `#F3F3F3` 21567 | `#FFFFFF` 545 -> `#1A1A1A` 463 | follows the theme |

Both directions were run; the counts line up row for row with the broken leg above (`545 / 127 / 119 / 103 / 91 / 89`),
which is the geometry check that the two legs measured the same pixels.

## What the icon resolves, with and without a redraw (`spike/NavIconRecolor/probe-ink.log`)

A probe that calls the protected `GetEffectiveForeground()` from a derived `SymbolIcon` (`class InkProbe : SymbolIcon
{ Brush Ink() => GetEffectiveForeground(); }`), on one live tree, mounted Light:

    mounted-light      item.Foreground=#E4000000  icon.Foreground=<null>  resolved=#E4000000
    flipped-dark       item.Foreground=#FFFFFFFF  icon.Foreground=<null>  resolved=#FFFFFFFF
    flipped-back-light item.Foreground=#E4000000  icon.Foreground=<null>  resolved=#E4000000

So the ancestor walk is not the bug - the value it returns is already right in both themes. The bug is that nothing
invalidates the icon when that answer changes, and an icon that never redraws keeps the pixels of the last one it
drew. That is also why the fix hands the item's ink to the icon's own `Foreground` property rather than calling
`InvalidateVisual()`: setting the property is the only route in this runtime that both changes the value and marks
the visual dirty, and it leaves a readable value behind for a regression fact.

## Mutation witness (`spike/NavIconRecolor/teeth.sh`)

Each fact is reddened by exactly its own mutant, and the digest of the `FluentJalium.dll` inside the test project's
output is printed per leg to prove the rebuilt mutant was the binary the run loaded:

| mutant | what it removes | result |
|---|---|---|
| `nobind` | the hand-off itself | `A_live_theme_switch_moves_a_pane_icon_onto_the_ink_its_item_moved_to` FAIL: "the pane icon carries `<null>` while its item moved to `#E4000000`"; the own-ink fact stays green |
| `noguard` | the "icon carries its own ink" test | `A_pane_icon_that_arrives_with_its_own_ink_keeps_it` FAIL; the theme-switch fact stays green |

An earlier run of this A/B looked vacuous because it rebuilt only `src/FluentJalium` and then ran the tests with
`--no-build`: the test project loaded the previous `FluentJalium.dll`, so the mutant never reached the process under
test. Building `tests/FluentJalium.Tests` is what refreshes the consumed copy, and the digest line is the witness.
