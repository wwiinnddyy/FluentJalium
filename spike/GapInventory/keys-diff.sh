#!/usr/bin/env bash
# Recomputes the upstream-vs-Astra theme-key diff that docs/astra/audits/gap-inventory.md quotes.
# Read-only against both trees: it greps the pinned WinUI checkout and our generated keys ledger,
# and writes only next to itself. Run from anywhere: paths are resolved from this file's location.
#
#   bash spike/GapInventory/keys-diff.sh > spike/GapInventory/keys-diff-raw.txt
set -euo pipefail

US=$(cd "$(dirname "$0")/../.." && pwd)            # FluentJalium
UP=$US/../microsoft-ui-xaml                        # pinned WinUI reference (read-only)
LEDGER=$US/docs/astra/audits/keys.md
WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT

echo "== inputs =="
echo "Astra HEAD            : $(git -C "$US" rev-parse --short HEAD)"
echo "WinUI checkout        : $(git -C "$UP" rev-parse --short HEAD)"
echo "WinUI working tree    : $(git -C "$UP" status --porcelain | wc -l) modified path(s) - reference must stay clean"
echo "ledger canonical line : $(grep -o 'canonical-lines=[0-9]*' "$LEDGER" | head -1)"
echo

# 1. every x:Key name upstream declares in a *_themeresources.xaml, with its row type.
echo "== upstream rows: controls/dev/**/*_themeresources.xaml =="
grep -rhoE 'x:Key="[A-Za-z0-9_.]+"' "$UP/controls/dev" --include='*_themeresources.xaml' \
  | sed -E 's/x:Key="//; s/"//' | sort -u > "$WORK/up_names.txt"
grep -rhoE '<[A-Za-z_]+(:[A-Za-z]+)? x:Key="[A-Za-z0-9_.]+"' "$UP/controls/dev" \
  --include='*_themeresources.xaml' \
  | sed -E 's/^[<]//; s/ x:Key="/|/; s/"$//' | sort -u > "$WORK/up_typed.txt"
echo "unique key names      : $(wc -l < "$WORK/up_names.txt")"
echo "typed declarations    : $(wc -l < "$WORK/up_typed.txt")  (a name can be declared by several files)"
echo

# 2. every name our dictionaries publish, taken from the second column of keys.md.
awk -F'|' '/^\| [A-Za-z]+ \|/ {k=$3; gsub(/[ `]/,"",k); if (length(k)) print k}' "$LEDGER" \
  | sort -u > "$WORK/ours_names.txt"
echo "== Astra published names (keys.md column 2, de-duplicated) =="
echo "unique names          : $(wc -l < "$WORK/ours_names.txt")"
echo

comm -23 "$WORK/up_names.txt" "$WORK/ours_names.txt" > "$WORK/missing.txt"
comm -12 "$WORK/up_names.txt" "$WORK/ours_names.txt" > "$WORK/overlap.txt"
comm -13 "$WORK/up_names.txt" "$WORK/ours_names.txt" > "$WORK/extra.txt"
echo "== diff =="
echo "overlapping names     : $(wc -l < "$WORK/overlap.txt")"
echo "upstream, not ours    : $(wc -l < "$WORK/missing.txt")"
echo "ours, not an upstream theme key (palette colors, our aliases, own-type keys) : $(wc -l < "$WORK/extra.txt")"
echo

echo "== the un-transcribed names, by upstream row type =="
echo "counts overlap: a name declared with two row types in two files is counted in both buckets"
for t in StaticResource SolidColorBrush Color x:Double Thickness x:String CornerRadius FontWeight Style ControlTemplate AcrylicBrush x:Boolean VerticalAlignment HorizontalAlignment; do
  grep -rhoE "<([A-Za-z_0-9]+:)?$t[ >][^>]*x:Key=\"[A-Za-z0-9_.]+\"" "$UP/controls/dev" --include='*_themeresources.xaml' \
    | sed -E 's/.*x:Key="//; s/"//' | sort -u > "$WORK/t.txt"
  total=$(wc -l < "$WORK/t.txt")
  gone=$(comm -12 "$WORK/missing.txt" "$WORK/t.txt" | wc -l)
  printf "%-18s upstream=%-5s un-transcribed=%s\n" "$t" "$total" "$gone"
done
echo

echo "== the un-transcribed names, by control-family prefix =="
grep -oE '^(Accent|AnnotatedScrollBar|AppBar|AutoSuggestBox|BreadcrumbBar|Button|Calendar|CheckBox|ColorPicker|CommandBar|CommandBarFlyout|ContentDialog|ContextMenu|DatePicker|Divider|Expander|Flyout|GridView|Hyperlink|InfoBadge|InfoBar|ItemContainer|ItemsView|ListBox|ListView|Menu|MenuBar|NavigationView|NumberBox|PagerControl|PersonPicture|PipsPager|ProgressBar|ProgressRing|RadioButton|RadioButtons|Rating|RepeatButton|SelectorBar|Slider|SplitButton|SplitView|TabView|TeachingTip|Text|Thumb|TimePicker|TitleBar|Toggle|ToggleButton|ToggleSwitch|ToolTip|TreeView|ScrollView|Swipe)' \
  "$WORK/missing.txt" | sort | uniq -c | sort -rn
echo
echo "material-shaped names (acrylic|reveal|smoke|mica anywhere in the name) : $(grep -Eic '(acrylic|reveal|smoke|mica)' "$WORK/missing.txt" || true)"
echo

echo "== the 82-ish upstream named Styles we do not publish =="
grep -rhoE '<([A-Za-z_0-9]+:)?Style[ >][^>]*x:Key="[A-Za-z0-9_.]+"' "$UP/controls/dev" --include='*_themeresources.xaml' \
  | sed -E 's/.*x:Key="//; s/"//' | sort -u > "$WORK/up_styles.txt"
echo "upstream named styles : $(wc -l < "$WORK/up_styles.txt")"
comm -12 "$WORK/missing.txt" "$WORK/up_styles.txt" > "$WORK/missing_styles.txt"
echo "un-transcribed        : $(wc -l < "$WORK/missing_styles.txt")"
tr '\n' ' ' < "$WORK/missing_styles.txt"; echo
echo
echo "Reveal-hover styles among them : $(grep -ci reveal "$WORK/missing_styles.txt")"
echo "NavigationView presenter styles among them : $(grep -c 'MUX_NavigationViewItemPresenter\|NavigationView' "$WORK/missing_styles.txt")"
echo
echo "== Jalium.UI.Managed exported types that do not exist (exact-name match, one per line) =="
INV=$US/docs/astra/adaptation/s0v-runtime-type-inventory-raw.txt
for n in ItemsRepeater ItemsView LayoutPanel StackLayout UniformGridLayout FlowLayout SplitView \
         TwoPaneView GridView Card CardAction FlyoutPresenter MenuScroller PagerControl PersonPicture \
         SelectorBar AnnotatedScrollBar ThemeShadow AcrylicBrush AnimatedIcon RadioMenuFlyoutItem \
         SplitMenuFlyoutItem CommandBarFlyout MenuFlyout FlyoutBase WrapPanel VisualStateManager; do
  printf "%-22s %s\n" "$n" "$(grep -cE "\.$n\$" "$INV")"
done
echo
echo "== the vacuum report's section D (28 names) re-measured the way adaptation/05 section F prescribes =="
echo "the tool matches names against the Control census; this looks each name up in the whole exported-type inventory"
for n in AnnotatedScrollBar AutoSuggestBox BreadcrumbBar CommandBarFlyout DropDownButton Flyout GridView \
         Hyperlink InfoBadge ItemContainer ItemsView LayoutPanel MenuFlyout NavigationBackButton Page \
         PagerControl PersonPicture ProgressRing RadioButtons RadioMenuItem RatingControl Repeater \
         SelectorBar SplitView TabView TeachingTip TwoPaneView WrapPanel; do
  hit=$(grep -E "(\.|:)$n\$" "$INV" | head -1 | tr -d ' ' || true)
  printf "%-22s %s\n" "$n" "${hit:-absent}"
done

