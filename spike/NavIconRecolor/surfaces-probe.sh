#!/usr/bin/env bash
# Is the one-off big census reading (surfaces 28325, settings 33755 in an earlier run) our window or something
# drawn over it? The census grabs the whole screen, so anything overlapping the window counts as "frozen ink".
# Re-measure the two pages that did it, twice each, with every markup hand-off cut, and keep the worst-bucket crop.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/surfaces-probe.log
: > "$LOG"
MUTANTS="noappbarink nomenusink notabink notoggleink"

for name in $MUTANTS; do
  python spike/NavIconRecolor/mutate.py apply "$name" >> "$LOG" 2>&1
  echo "$name apply-exit=$?" >> "$LOG"
done

dotnet build samples/FluentJalium.Gallery >> "$LOG" 2>&1
build=$?
echo "gallery-build-exit=$build" | tee -a "$LOG"
if [ "$build" -ne 0 ]; then
  echo "ABORT: mutant Gallery never ran, reverting" | tee -a "$LOG"
  for name in $MUTANTS; do python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1; done
  exit 1
fi

for round in 1 2; do
  for page in surfaces settings; do
    bash spike/NavIconRecolor/census.sh "$page" 2>&1 | sed "s/^/round$round /" | tee -a "$LOG"
    mv "spike/NavIconRecolor/census-$page-worst.before.png" "spike/NavIconRecolor/probe-r$round-$page-worst.before.png" 2>/dev/null
    mv "spike/NavIconRecolor/census-$page-worst.after.png" "spike/NavIconRecolor/probe-r$round-$page-worst.after.png" 2>/dev/null
    mv "spike/NavIconRecolor/census-$page-mask.png" "spike/NavIconRecolor/probe-r$round-$page-mask.png" 2>/dev/null
  done
done

for name in $MUTANTS; do
  python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
  echo "$name revert-exit=$?" >> "$LOG"
done
echo "marker-grep-lines=$(grep -rn --binary-files=without-match --include=*.cs --include=*.jalxaml MUTANT src/FluentJalium | wc -l) (0 = clean; source-scoped and binary-skipped, see teeth95.sh)" | tee -a "$LOG"
dotnet build samples/FluentJalium.Gallery >> "$LOG" 2>&1
echo "gallery-rebuild-exit=$?" | tee -a "$LOG"
ls -la spike/NavIconRecolor | grep -a "probe-r"
