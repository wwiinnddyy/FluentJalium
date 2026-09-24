"""Splice the hand-written annotations back into the regenerated 05-native-control-vacuum.md.

The generator (tools/Report-Control-Vacuum.ps1) emits sections A-E plus the standing
"what this list cannot prove" tail, and knows nothing about the two hand blocks that a
later batch appended to the checked-in file: the §D staleness warning and the whole §F
re-measurement. Re-running the tool therefore deletes findings - which is why the file
had stayed stale for an entire stage. This script re-attaches both blocks to the fresh
output, with pre- and post-conditions, and aborts rather than guessing.
"""
import io
import re
import subprocess
import sys

PATH = 'docs/astra/adaptation/05-native-control-vacuum.md'

head = subprocess.run(['git', 'show', f'HEAD:{PATH}'], capture_output=True, check=True).stdout.decode('utf-8')
work = io.open(PATH, encoding='utf-8', newline='').read()

def block(text, start_marker, end_marker, what):
    i = text.index(start_marker)
    j = text.index(end_marker, i)
    out = text[i:j]
    assert out.strip(), f'empty {what} block'
    return out

warning = block(head, '**这份名单已过期**', '\n\n', 'D-warning')
fspec = block(head, '## F. 2026-09-19', '\n## 这份清单证明不了什么', 'section F')

# preconditions: the fresh file has neither block, and has exactly one of each anchor
assert work.count(warning) == 0, 'D-warning already present'
assert work.count(fspec.splitlines()[0]) == 0, 'section F already present'
d_anchor = '补样式解决不了：要么自有类型，要么明确放弃。名单里含 ModernWpf 的工程目录名（非控件），保留不筛。'
tail_anchor = '## 这份清单证明不了什么'
assert work.count(d_anchor) == 1, 'D anchor not unique'
assert work.count(tail_anchor) == 1, 'tail anchor not unique'

work = work.replace(d_anchor, d_anchor + '\n' + warning, 1)
work = work.replace(tail_anchor, fspec.rstrip() + '\n\n' + tail_anchor, 1)

io.open(PATH, 'w', encoding='utf-8', newline='').write(work)

# postconditions - re-read from disk, do not trust the in-memory string
after = io.open(PATH, encoding='utf-8', newline='').read()
checks = {
    'D warning back': after.count('这份名单已过期') == 1,
    'F heading back': after.count('## F. 2026-09-19') == 1,
    'F conclusions back': 'MenuFlyoutPresenter(MenuFlyout)' in after,
    'fresh count 30': '| 其中我们已声明隐式样式 | 30 |' in after,
    'fresh count 23': '**其中真空，且无自有类型顶替** | **23**' in after,
    'tail still last': after.rindex('## 这份清单证明不了什么') > after.rindex('## F. 2026-09-19'),
    'no stale 13/41': '| 其中我们已声明隐式样式 | 13 |' not in after and '**41**' not in after,
}
bad = [k for k, v in checks.items() if not v]
print('\n'.join(f'{"ok  " if v else "FAIL"} {k}' for k, v in checks.items()))
if bad:
    sys.exit(f'postcondition failed: {bad}')
print(f'lines={len(after.splitlines())}')
