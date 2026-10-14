# Glyph-ink grid for #96 / #97, landed 2026-09-23

Asks one question per run: does this codepoint put ink on the screen, and what shape is that ink. 764 `Symbol`
values are laid on a 24-DIP white grid inside a red box, the window is grabbed off the monitor, and every cell is
counted twice - a dark count (R/G/B all < 128) and a light count (any channel < 215) - plus an 8x8 shape signature.
Two control cells ride along: a 16 px `TextBlock` that must carry ink and a blank `Border` that must not, so a
grid-wide zero reads as a broken instrument instead of as a finding about fonts.

Four variants differ only in who paints the codepoint:

| `-Variant` | element | where the font family comes from |
| --- | --- | --- |
| `symbol` | `SymbolIcon{Symbol}` | nowhere - the type has no `FontFamily` |
| `fluent` | `FontIcon{Glyph}` | code writes `"Segoe Fluent Icons"` |
| `mdl2` | `FontIcon{Glyph}` | code writes `"Segoe MDL2 Assets"` |
| `markup` | `FontIcon` parsed from `.jalxaml` | a literal `FontFamily=` attribute |

```
dotnet build spike/GlyphInkProbe
powershell -File spike/GlyphInkProbe/shoot-count.ps1 -Variant symbol    # and fluent, mdl2, markup
python spike/GlyphInkProbe/compare-variants.py                          # cell-by-cell agreement
python spike/GlyphInkProbe/match-cmap.py                                # blank set vs installed font cmaps
```

What it settled (full readings in `docs/astra/audits/icon-family.md` §9 and `docs/astra/ROADMAP.md`): ink reaches
the monitor for 644/764 cells under `SymbolIcon`, 761 under Fluent, 762 under MDL2, and `markup` agrees with the
code-set family on **all 761** shared cells - so a template can carry a font family, which narrows the old "markup
drops FontFamily" claim to the resource-key spellings. The shipped assembly's own defaults are the answer to the
user's "these are Windows 10 icons": `SymbolIcon.SymbolFontFamily` and `FontIcon.DefaultFontFamily` both read
`'Segoe MDL2 Assets'`.

Three instrument traps this directory paid for, all of which will lie the same way again:

- `attempt-clipped.log` is the first run: the grabber was DPI-unaware, so `GetWindowRect` gave virtualised
  coordinates while `CopyFromScreen` copied physical pixels and every "box" was a top-left crop. Display is 175%.
  `shoot-count.ps1` now calls `SetProcessDPIAware()` and refuses a reading whose red box disagrees with the scale
  the window reports.
- The red box is found by long red rows/columns, not by a bounding box over red pixels: one stray red pixel moved
  the bounds from 1680x840 to 1807x984 while the red count stayed the same.
- The first `markup` run painted 0/764 because the parsed icon had no foreground: `IconElement` falls back to a
  `"TextPrimary"` resource lookup that resolves to nothing in a bare window. Setting only the ink (never the family)
  is what produced the numbers above.

`inspect-grab.ps1` and `screen-info.ps1` are the two one-off probes used to diagnose the grab itself - the first
says which edges of the red box actually landed on screen, the second prints the monitor bounds and DPI.
`probe-<variant>.log` keeps each run's own geometry and reflection readings next to the picture it describes.
