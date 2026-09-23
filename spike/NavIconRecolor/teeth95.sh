#!/usr/bin/env bash
# Four legs: each cuts one leg of the icon-ink hand-off, rebuilds the CONSUMER project, witnesses that the
# rebuilt FluentJalium.dll is the one the host loads, and must turn exactly its own fact red.
# A build failure aborts the leg before the test runs, otherwise the previous binary would answer for the mutant.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/mut-95.log
: > "$LOG"

FACTS_nobind=AstraNavigationTests.A_live_theme_switch_moves_a_pane_icon_onto_the_ink_its_item_moved_to
FACTS_noguard=AstraNavigationTests.A_pane_icon_that_arrives_with_its_own_ink_keeps_it
FACTS_noappbarink=AstraIconFamilyTests.A_template_that_hosts_an_icon_hands_it_the_carrier_ink
FACTS_notoggleink=AstraNavigationTests.The_pane_toggle_glyph_carries_the_button_ink_through_a_live_switch

echo "=== baseline, no mutation: the four facts on the shipped wiring" | tee -a "$LOG"
dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
echo "baseline-build-exit=$?" | tee -a "$LOG"
# Seed the digest of the shipped binary so each leg's `changed=true` compares against a real predecessor.
python spike/NavIconRecolor/mutate.py hash | tee -a "$LOG"
dotnet test tests/FluentJalium.Tests --no-build \
  --filter "FullyQualifiedName~A_live_theme_switch_moves_a_pane_icon|FullyQualifiedName~A_pane_icon_that_arrives_with_its_own_ink|FullyQualifiedName~The_pane_toggle_glyph_carries|FullyQualifiedName~A_template_that_hosts_an_icon" \
  2>&1 | grep -aE "失败|通过|错误|Passed!|Failed!" | tail -5 | tee -a "$LOG"
echo "baseline-test-exit=${PIPESTATUS[0]}" | tee -a "$LOG"

for name in nobind noguard noappbarink notoggleink; do
  fact="FACTS_$name"
  fact="${!fact}"
  {
    echo ""
    echo "=== LEG $name -> $fact"
    date -u +%FT%TZ
  } >> "$LOG"

  python spike/NavIconRecolor/mutate.py apply "$name" >> "$LOG" 2>&1
  apply=$?
  if [ "$apply" -ne 0 ]; then
    echo "$name APPLY-FAILED exit=$apply, leg skipped" | tee -a "$LOG"
    continue
  fi

  dotnet build tests/FluentJalium.Tests >> "$LOG" 2>&1
  build=$?
  if [ "$build" -ne 0 ]; then
    echo "$name BUILD-FAILED exit=$build - mutant never ran, leg aborted" | tee -a "$LOG"
    python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
    continue
  fi

  python spike/NavIconRecolor/mutate.py hash | tee -a "$LOG"
  dotnet test tests/FluentJalium.Tests --no-build --filter "FullyQualifiedName~$fact" >> "$LOG" 2>&1
  test=$?
  echo "$name TEST-exit=$test (1 expected: the fact is red without its mechanism)" | tee -a "$LOG"

  python spike/NavIconRecolor/mutate.py revert "$name" >> "$LOG" 2>&1
  echo "$name REVERT-exit=$?" | tee -a "$LOG"
done

echo "" | tee -a "$LOG"
echo "=== tree after the run (must show only the fix, no MUTANT markers)" | tee -a "$LOG"
grep -arn "MUTANT" src/FluentJalium | tee -a "$LOG"
echo "marker-grep-exit=$? (1 = none left)" | tee -a "$LOG"
git status --porcelain src >> "$LOG" 2>&1
