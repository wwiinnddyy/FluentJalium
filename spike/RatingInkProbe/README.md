# Rating cell probe (#98), landed 2026-09-23

Mounts four `FluentRatingControl`s (Value 3.5 / 5 / 2 / 4.7) in a light-themed window, walks every horizontal
`StackPanel` in the tree, and prints one line per cell: host width, whether the host crops, the run's desired and
arranged widths, its `FontFamily`, its margin, its scale, and the run's box transformed into control space.
Then it writes `rating-ready.flag` and holds until `rating-stop.flag` appears, so `grab-picture.ps1` can shoot the
same window the numbers came from.

```
dotnet build spike/RatingInkProbe/RatingInkProbe.csproj
powershell -NoProfile -ExecutionPolicy Bypass -File spike/RatingInkProbe/grab-picture.ps1 `
  -Out spike/RatingInkProbe/rating-after.png
```

`FluentThemeManager.Apply(application)` + `ApplyTheme(Light)` are load-bearing: without them the control mounts
`children=0` at height 0 and every reading is a reading of an empty tree. The grabber is DPI-aware for the reason
`spike/GlyphInkProbe/README.md` records (this box is 175%); an unaware grab reads a top-left crop of the window.

## What the numbers said

`probe-before-fix.log` (run laid out with the star directly inside the cropping host) and `probe-after.log` (the lane)
differ in one column pair — the run's arranged width and where its box lands:

| | arranged | ink box in cell 0 | cell 4 |
| --- | --- | --- | --- |
| no lane | 25.5 (= host 17 + inset 8.5) | x=-2.13 w=12.75 | x=97.88 w=12.75 |
| lane | 34 (the run's own advance) | x=0 w=17 | x=100 w=17 |

A 17-wide `Grid` host hands its child 17 + 8.5 of measure width, so the run never reached its natural 34; the 0.5
scale then pivoted on the clamped box's centre (4.25) instead of the cell centre (8.5). The fractional cells show the
same clamp from the other side: host 8.5 → arranged 17 → ink 8.5, and host 0 → arranged 8.5 → ink 4.25, i.e. a
half-star was drawn as a sliver of itself.

## The pictures, and one that is not evidence

`rating-before.png` / `rating-after.png` are an A/B pair captured in one sitting: same `Program.cs`, same build
command, and the only difference is `mutate-lane.ps1` replacing the lane with `host.Children.Add(item)`.
`scan-row.ps1 -BlueOnly` reads the fill layer straight off them at the row's mid-height (y=145):

```
rating-before.png: 47..65 w=19  90..109 w=20  134..153 w=20  178..186 w=9   (half cell)
rating-after.png:  50..71 w=22  93..115 w=23  137..159 w=23  181..193 w=13  (half cell)
```

The runs start 3 px (≈ 1.8 DIP) earlier and end 6 px short — the left lean and the missing right arm, matching the
tree numbers. `sample-colors.ps1` counts the star band by colour: before `blue=5487 gray=5378`, after
`blue=6162 gray=5373`.

`rating-anomaly-noaccent.png` is **not** part of that pair. It is an earlier capture of the same no-lane shape whose
star band holds 9 549 grey pixels and **zero** blue ones — the fill wearing the inherited text colour instead of the
accent. Four later runs (the pair above plus `rating-repeat1.png` / `rating-repeat2.png`, `blue=5620` / `blue=5477`)
do not reproduce it, and the lane cannot explain it, so it stays on disk as an unreproduced single observation rather
than as a finding.

`probe-mutant.log` is the no-lane geometry re-measured while the regression test was being checked for teeth; the
new assertion failed exactly on `Expected: 34 / Actual: 25.5` and the file was restored from the fixed source
(sha1 `f26db7bb69cce8409aa7f955bd67881fd9914495`).
