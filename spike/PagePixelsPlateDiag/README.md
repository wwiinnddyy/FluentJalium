# Plate diagnostic for the page-pixel gate (#90 / #93), parked 2026-09-23

`plate-diagnostic.patch` adds, in `tools/AstraPagePixels/Program.cs`, the reading #93 needs to tell a dirty
"empty plate" apart from a real one: the child count and types of `host` when the slot is captured, plus the
top colour of a plate that still has paint. It is **unverified** - the version here does not compile
(`host.Children.Select(...)` on an `ImmutableArray<UIElement>` fails type inference, CS0411), and it has never
produced a reading, so it does not belong in the tree a gate measures.

To pick #93 back up: `git apply spike/PagePixelsPlateDiag/plate-diagnostic.patch`, fix the type argument, then
run `dotnet run --project tools/AstraPagePixels -- --report` and compare the `materials HighContrast` leg against
`spike/ForegroundArrival/gate-91.log` (that run left 337249 bright points and 41 colours in a slot that should
have been empty, while `gate-92` did not reproduce it). The slot width also jumps 980 / 788 / 1000 between runs,
and that mechanism is still unmeasured.
