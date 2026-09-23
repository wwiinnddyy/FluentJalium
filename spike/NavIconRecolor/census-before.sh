#!/usr/bin/env bash
# Re-measure the pre-#95 tree on disk: cut every markup hand-off this batch adds (the #94 pane-item route stays,
# it is committed), rebuild the Gallery, and run the frozen-ink census over all 13 pages.
# The point is an offender table the doc can quote from a file rather than from a terminal that has scrolled by.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/census-before.log
: > "$LOG"
MUTANTS="noappbarink nomenusink notabink notoggleink"

for name in $MUTANTS; do
  python spike/NavIconRecolor/mutate.py apply "$name" >> "$LOG" 2>&1
  echo "$name apply-exit=$?" | tee -a "$LOG"
done
grep -arc "IconInk" src/FluentJalium/Styles/*.jalxaml >> "$LOG" 2>&1

dotnet build samples/FluentJalium.Gallery >> "$LOG" 2>&1
build=$?
echo "gallery-build-exit=$build" | tee -a "$LOG"
if [ "$build" -ne 0 ]; then
  echo "ABORT: mutant Gallery never ran, reverting" | tee -a "$LOG"
  for name in $MUTANTS; do python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1; done
  exit 1
fi

bash spike/NavIconRecolor/census.sh 2>&1 | tee -a "$LOG"

for name in $MUTANTS; do
  python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
  echo "$name revert-exit=$?" | tee -a "$LOG"
done

grep -arn "MUTANT" src/FluentJalium >> "$LOG" 2>&1
echo "marker-grep-lines=$(grep -arc MUTANT src/FluentJalium | awk -F: '{s+=$2} END {print s}') (0 = clean)" | tee -a "$LOG"

# Rebuild clean so the binaries on disk answer for the source that is actually in the tree.
dotnet build samples/FluentJalium.Gallery >> "$LOG" 2>&1
echo "gallery-rebuild-exit=$?" | tee -a "$LOG"
dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
echo "tests-rebuild-exit=$?" | tee -a "$LOG"
