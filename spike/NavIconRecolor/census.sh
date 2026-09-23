#!/usr/bin/env bash
# Frozen-ink census over every Gallery page: for each page, grab the same window before and after a live
# Light->Dark flip and count pixels whose ink did not move. A host whose glyph ink is resolved at draw time and
# never invalidated shows up here; #94 (pane item icons) and the pane toggle glyph were both found this way.
# Usage: spike/NavIconRecolor/census.sh [page ...]
set -u
cd "$(dirname "$0")/../.." || exit 1
pages=${*:-"overview tokens materials motion buttons inputs selection navigation surfaces status menus command-bar settings"}

for page in $pages; do
  powershell -NoProfile -ExecutionPolicy Bypass -File spike/NavIconRecolor/shoot.ps1 -Page "$page" \
    -StartTheme light -FlipTo dark > "spike/NavIconRecolor/census-$page-shoot.log" 2>&1
  shoot_exit=$?
  if [ "$shoot_exit" != "0" ]; then
    echo "$page SHOOT FAILED exit=$shoot_exit"
    continue
  fi
  # Window interior only: the rect includes the rounded corners and the taskbar, which are dark in both grabs
  # and would otherwise read as 58k pixels of frozen ink. The mask and the crop name the worst bucket, so a
  # reading can be looked at instead of argued about.
  out=$(powershell -NoProfile -ExecutionPolicy Bypass -File spike/NavIconRecolor/frozen.ps1 \
    -Before spike/NavIconRecolor/before.png -After spike/NavIconRecolor/after.png \
    -Left 353 -Top 90 -Right 2230 -Bottom 1330 \
    -Mask "spike/NavIconRecolor/census-$page-mask.png" \
    -Crop "spike/NavIconRecolor/census-$page-worst" 2>&1 | tr -d '\r')
  echo "== $page $(echo "$out" | grep -a 'area')"
  echo "$out" | grep -aE 'at x=|^crop' | head -14
done
