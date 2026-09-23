#!/usr/bin/env bash
# Re-measure the shipped route (two live bindings, no visual-tree walk) over all 13 pages: the census table in
# ROADMAP #95 quotes this file, so it has to be the binary the tree builds, not the one the walk route left.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/census-route-b.log
: > "$LOG"

# Source-scoped and binary-skipped: an -a grep also reads the shipped DLLs under bin/, and one of them
# (System.Diagnostics.EventLog.dll) carries the byte string MUTANT - a clean tree would abort here. See teeth95.sh.
if grep -rn --binary-files=without-match --include='*.cs' --include='*.jalxaml' "MUTANT" src/FluentJalium > /dev/null 2>&1; then
  grep -rn --binary-files=without-match --include='*.cs' --include='*.jalxaml' "MUTANT" src/FluentJalium | tee -a "$LOG"
  echo "PREFLIGHT FAILED: a mutant is in the tree, aborting" | tee -a "$LOG"
  exit 1
fi

dotnet build samples/FluentJalium.Gallery >> "$LOG" 2>&1
build=$?
echo "gallery-build-exit=$build" | tee -a "$LOG"
[ "$build" -ne 0 ] && exit 1

bash spike/NavIconRecolor/census.sh 2>&1 | tee -a "$LOG"

# The test binary has to answer for the same source the census just measured, or the next test run reads a mutant.
dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
echo "tests-rebuild-exit=$?" | tee -a "$LOG"
