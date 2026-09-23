#!/usr/bin/env bash
# Show that the two pane-icon facts depend on the two halves of the fix, by mutating each half in turn.
# Usage: spike/NavIconRecolor/teeth.sh [name ...]
set -u
cd "$(dirname "$0")/../.." || exit 1
out=spike/NavIconRecolor
names=${*:-nobind noguard}

for name in $names; do
  python "$out/mutate.py" apply "$name" || { echo "SETUP FAILED $name - STOP"; exit 1; }
  # Building the test project is what refreshes the FluentJalium.dll the run loads; building src alone leaves the
  # consumer on the previous binary, which is how one reading of this fix looked vacuous.
  dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-$name.log" 2>&1
  build_exit=$?
  echo "=== mutant=$name build_exit=$build_exit"
  if [ "$build_exit" != "0" ]; then
    grep "error CS" "$out/build-$name.log" | head -4
    python "$out/mutate.py" revert "$name"
    echo "BUILD FAILED $name - the test would have run on the previous binary, so this leg is dropped"
    continue
  fi
  grep -c "error" "$out/build-$name.log" | sed 's/^/    build_errors=/'
  python "$out/mutate.py" hash
  dotnet test tests/FluentJalium.Tests -c Debug --no-build --no-restore \
    --filter "FullyQualifiedName~AstraNavigationTests.A_live_theme_switch_moves_a_pane_icon|FullyQualifiedName~AstraNavigationTests.A_pane_icon_that_arrives_with_its_own_ink" \
    > "$out/mut-$name.log" 2>&1
  echo "=== mutant=$name test_exit=$?"
  grep -E "\[FAIL\]|carries|失败!|已通过!" "$out/mut-$name.log" | head -6
  python "$out/mutate.py" revert "$name" || { echo "REVERT FAILED $name - STOP"; exit 1; }
done

dotnet build tests/FluentJalium.Tests -c Debug --no-restore > "$out/build-restore.log" 2>&1
echo "=== restored build_exit=$? (0 expected) and the facts again:"
dotnet test tests/FluentJalium.Tests -c Debug --no-build --no-restore   --filter "FullyQualifiedName~AstraNavigationTests.A_live_theme_switch_moves_a_pane_icon|FullyQualifiedName~AstraNavigationTests.A_pane_icon_that_arrives_with_its_own_ink"   > "$out/mut-restored.log" 2>&1
echo "restored_test_exit=$?"
grep -aE "\[FAIL\]|carries" "$out/mut-restored.log" | head -4
  tail -c 200 "$out/mut-restored.log"
python "$out/mutate.py" hash
git diff --stat -- src/FluentJalium/Controls/Navigation/FluentNavigationItem.cs
