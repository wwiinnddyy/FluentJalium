#!/usr/bin/env bash
# Measure an assertion's teeth by mutation: drop or re-point one style row, rebuild, run the facts, put it back.
# Usage: teeth.sh <name> [...]   with PHASE=<tag> FILTER=<xunit filter>
set -u
cd "$(dirname "$0")/../.." || exit 1
out=spike/ForegroundArrival
phase=${PHASE:-carrier}

for name in "$@"; do
  # A mutation left in the tree contaminates every later mutant, so a failed revert is fatal rather than a
  # line in the log: that is how a stale cell once faked a coupling between two independent facts.
  python "$out/mutate.py" apply "$name" || { echo "SETUP FAILED $name - STOP"; exit 1; }
  dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-$name.log" 2>&1
  echo "=== mutant=$name build_exit=$?"
  dotnet test tests/FluentJalium.Tests -c Debug --no-build --no-restore \
    --filter "${FILTER:-FullyQualifiedName~AstraForegroundArrival}" > "$out/mut-$phase-$name.log" 2>&1
  echo "=== mutant=$name test_exit=$?"
  grep -E "\[FAIL\]|reads #|失败!|已通过!" "$out/mut-$phase-$name.log" | head -8
  python "$out/mutate.py" revert "$name" || { echo "REVERT FAILED $name - STOP, tree still mutated"; exit 1; }
done

dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-restore.log" 2>&1
echo "=== restored build_exit=$?"
