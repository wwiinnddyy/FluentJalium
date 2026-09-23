#!/usr/bin/env bash
# Measure an assertion's teeth by mutation: drop one style row, rebuild, run the facts, put the row back.
# Usage: teeth.sh <title|subtitle|combo> [...]
set -u
cd "$(dirname "$0")/../.." || exit 1
out=spike/ForegroundArrival
phase=${PHASE:-carrier}

for name in "$@"; do
  python "$out/mutate.py" remove "$name" || { echo "SETUP FAILED $name"; continue; }
  dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-$name.log" 2>&1
  echo "=== mutant=$name build_exit=$?"
  dotnet test tests/FluentJalium.Tests -c Debug --no-build --no-restore \
    --filter "FullyQualifiedName~AstraForegroundArrival" > "$out/mut-$phase-$name.log" 2>&1
  echo "=== mutant=$name test_exit=$?"
  grep -E "\[FAIL\]|已跳过|失败!|已通过!" "$out/mut-$phase-$name.log" | head -8
  python "$out/mutate.py" restore "$name" || echo "RESTORE FAILED $name"
done

dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-restore.log" 2>&1
echo "=== restored build_exit=$?"
