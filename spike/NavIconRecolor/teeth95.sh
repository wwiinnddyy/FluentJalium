#!/usr/bin/env bash
# Five legs: each cuts one leg of the icon-ink hand-off, rebuilds the CONSUMER project, witnesses that the rebuilt
# FluentJalium.dll is the one the host loads, and must turn its own fact red.
#
# Two things this script learned the hard way and now enforces:
#  - a leg whose apply step failed must not run: the mutation is already in the file when the postcondition
#    rejects it, so the next leg would measure it (that is how one leg appeared to redden another's fact);
#  - the log must carry the assertion text, not just the test names, or a poisoned reading stays invisible.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/mut-95b.log
: > "$LOG"
SCOPE="FullyQualifiedName~AstraNavigationTests|FullyQualifiedName~AstraIconFamilyTests"

# Look for the marker in the sources a mutant can live in, and skip binaries. An `-a` grep over a build-output
# tree does neither: shipped copies of System.Diagnostics.EventLog.dll contain the byte string MUTANT, so
# widening the scope to tests/ or samples/ makes a clean tree read as "a mutant is in the tree" (observed this
# session), and piping that output into another grep answers "Binary file (standard input) matches" - a marker
# check that can show no line and still not honestly report none.
marker_grep() {
  grep -rn --binary-files=without-match --include='*.cs' --include='*.jalxaml' "MUTANT" src/FluentJalium
}

preflight() {
  if marker_grep > /dev/null 2>&1; then
    marker_grep | tee -a "$LOG"
    echo "PREFLIGHT FAILED: a mutant is still in the tree, aborting before measuring anything" | tee -a "$LOG"
    exit 1
  fi
}

echo "=== baseline, no mutation: the five icon facts on the shipped wiring" | tee -a "$LOG"
preflight
dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
echo "baseline-build-exit=$?" | tee -a "$LOG"
python spike/NavIconRecolor/mutate.py hash | tee -a "$LOG"
dotnet test tests/FluentJalium.Tests --no-build --filter "$SCOPE" 2>&1 | grep -aE "net10.0\)|失败 Fluent|carries" | tail -4 | tee -a "$LOG"

for name in nobind noguard onceonly nocarrierink notoggleink; do
  {
    echo ""
    echo "=== LEG $name"
    date -u +%FT%TZ
  } >> "$LOG"
  preflight

  python spike/NavIconRecolor/mutate.py apply "$name" >> "$LOG" 2>&1
  apply=$?
  if [ "$apply" -ne 0 ]; then
    echo "$name APPLY-FAILED exit=$apply - reverting and aborting the run" | tee -a "$LOG"
    python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
    exit 1
  fi

  dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
  build=$?
  if [ "$build" -ne 0 ]; then
    echo "$name BUILD-FAILED exit=$build - mutant never ran, reverting and aborting" | tee -a "$LOG"
    python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
    exit 1
  fi

  python spike/NavIconRecolor/mutate.py hash | tee -a "$LOG"
  dotnet test tests/FluentJalium.Tests --no-build --filter "$SCOPE" 2>&1 |
    grep -aE "net10.0\)|失败 Fluent|carries|Assert\.|Expected|Actual" | tail -12 | tee -a "$LOG"

  python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
  echo "$name REVERT-exit=$?" | tee -a "$LOG"
done

{
  echo ""
  echo "=== tree after the run (no MUTANT lines, and only the fix modified)"
  marker_grep
  echo "marker-grep-exit=$? (1 = none left)"
  git status --porcelain src
} >> "$LOG" 2>&1
tail -6 "$LOG"
