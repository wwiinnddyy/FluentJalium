#!/usr/bin/env bash
# Does the one-off big census reading (surfaces 28325 in one run, settings 33755 in an earlier one) come back on
# the shipped wiring? Two rounds of the two pages that did it, with the worst-bucket crop kept per round.
# The Gallery binary on disk is the clean one, so this measures the tree as it will be committed.
set -u
cd C:/git/Jalium/FluentJalium || exit 2

LOG=spike/NavIconRecolor/surfaces-probe.log
: > "$LOG"

for round in 1 2; do
  bash spike/NavIconRecolor/census.sh surfaces settings 2>&1 | sed "s/^/round$round /" | tee -a "$LOG"
  for page in surfaces settings; do
    for kind in before after; do
      mv "spike/NavIconRecolor/census-$page-worst.$kind.png" "spike/NavIconRecolor/probe-r$round-$page-worst.$kind.png" 2>/dev/null
    done
    mv "spike/NavIconRecolor/census-$page-mask.png" "spike/NavIconRecolor/probe-r$round-$page-mask.png" 2>/dev/null
  done
done

echo "=== artifacts" | tee -a "$LOG"
ls -la spike/NavIconRecolor | grep -a "probe-r" | tee -a "$LOG"
