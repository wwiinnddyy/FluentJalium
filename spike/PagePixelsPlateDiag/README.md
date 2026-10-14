# Plate diagnostic for the page-pixel gate (#90 / #93), landed 2026-09-23

`plate-diagnostic.patch` was the parked draft: it added the child count and type names of `host` at the moment the
slot is captured, plus the top colour of a plate that still has paint. The patch itself never compiled
(`host.Children.Select(...)` breaks type inference on Jalium's child collection - CS0411) and never produced a
reading, so it stayed out of the tree a gate measures.

What shipped instead is the same reading written with an index loop, directly in `tools/AstraPagePixels/Program.cs`:
a leg whose plate is empty prints nothing, a dirty one prints `plate children N [Types] top #RRGGBB:px`. Two runs on
2026-09-23 (`report-93.log`, `judge-93.log`) left every one of the 39 legs clean; the reading and what it does and
does not settle for #90/#93 is in `docs/astra/ROADMAP.md`, section `## #93 一手`. This directory now keeps the dead
draft for reference only - the patch is not meant to be applied any more.
