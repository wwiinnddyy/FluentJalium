#!/usr/bin/env bash
# Proves tools/Report-Control-Vacuum.ps1 -Check has teeth, both directions, and that hand notes survive.
# Every exit code is taken from PowerShell itself (no pipe), because a piped run reports tail's status.
set -u
cd "$(dirname "$0")/../.."
DOC=docs/astra/adaptation/05-native-control-vacuum.md
PS=(powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/Report-Control-Vacuum.ps1)
out=spike/GapInventory/check-teeth.log
: > "$out"

say() { echo "== $*" | tee -a "$out"; }
run() { "${PS[@]}" "$@" >>"$out" 2>&1; local rc=$?; echo "   exit=$rc" | tee -a "$out"; return $rc; }

fail=0

say "1. regenerate clean, then snapshot"
run || fail=1
cp "$DOC" /tmp/05-snap.md || fail=1

say "2. -Check on the fresh file must be 0"
run -Check || { echo "FAIL: green check went red" | tee -a "$out"; fail=1; }

say "3. hand edit inside the generated body must go red"
python - "$DOC" <<'PY'
import io, sys
p = sys.argv[1]
b = io.open(p, 'rb').read()
assert b.count('| 30 |'.encode()) == 1, 'anchor not unique - refusing to mutate'
io.open(p, 'wb').write(b.replace('| 30 |'.encode(), '| 29 |'.encode()))
PY
if run -Check; then echo "FAIL: hand edit of the generated body was not caught" | tee -a "$out"; fail=1; fi
grep -a "first difference at line" "$out" | tail -1

say "4. restore from the byte snapshot must go green again"
cp /tmp/05-snap.md "$DOC"
run -Check || { echo "FAIL: restore did not return to green" | tee -a "$out"; fail=1; }

say "5. a line below the sentinel is hand notes: a re-run must keep it, and -Check must stay green"
python - "$DOC" <<'PY'
import io, sys
p = sys.argv[1]
s = io.open(p, encoding='utf-8', newline='').read()
marker = '原样保留这一行往下的一切内容 -->'
i = s.index(marker) + len(marker)
io.open(p, 'w', encoding='utf-8', newline='').write(s[:i] + '\r\n\r\n- 临时注记（本步自己加，验完删）\r\n' + s[i:])
PY
run                                              # re-run the generator over the note
kept=$(grep -c '临时注记' "$DOC")
if [ "$kept" != "1" ]; then
  echo "FAIL: the re-run deleted the hand note (occurrences: $kept)" | tee -a "$out"; fail=1
fi
run -Check || { echo "FAIL: hand notes wrongly read as staleness" | tee -a "$out"; fail=1; }
cp /tmp/05-snap.md "$DOC"

say "6. source-side drift (a type disappears from the census denominator) must go red"
python - spike/GapInventory/fake-census.txt <<'PY'
import io, sys
src = 'docs/astra/adaptation/01-census-raw-output.txt'
lines = io.open(src, encoding='utf-8').read().splitlines()
keep, dropped = [], None
for line in lines:
    if dropped is None and line.startswith('[TYPE] ToolBar '):
        dropped = 'ToolBar'
        continue
    keep.append(line)
assert dropped, 'expected a [TYPE] ToolBar row to drop'
io.open(sys.argv[1], 'w', encoding='utf-8').write('\n'.join(keep) + '\n')
print('dropped', dropped)
PY
"${PS[@]}" -Census spike/GapInventory/fake-census.txt -Out /tmp/05-fake.md >/dev/null 2>&1
if diff -q /tmp/05-fake.md "$DOC" >/dev/null; then
  echo "FAIL: dropping a census type changed nothing in the report" | tee -a "$out"; fail=1
else
  echo "   generated body differs from the committed report: $(diff /tmp/05-fake.md "$DOC" | grep -c '^[<>]') line(s)" | tee -a "$out"
fi

say "7. final state must be the snapshot"
cmp -s /tmp/05-snap.md "$DOC" && echo "   ok, document restored byte-for-byte" || { echo "FAIL: document left modified" | tee -a "$out"; fail=1; }

echo
if [ "$fail" -eq 0 ]; then echo "TEETH OK"; else echo "TEETH FAILED ($fail step(s))"; fi
exit "$fail"
