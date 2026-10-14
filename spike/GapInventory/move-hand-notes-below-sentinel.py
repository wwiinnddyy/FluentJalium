"""Restructure adaptation/05 so the hand-written notes live below a sentinel the generator preserves.

Why: tools/Report-Control-Vacuum.ps1 emits sections A-E plus the standing caveat tail and knows nothing
about what a later batch appended by hand (the section D staleness warning and the whole section F
re-measurement). Re-running it deleted those findings - which is how the file stayed stale for a stage.
This moves both hand blocks below a sentinel line; the generator is updated in the same batch to keep
everything from that line down verbatim.

Pre- and post-conditions abort rather than guess. Run from the repository root.
"""
import io
import sys

PATH = 'docs/astra/adaptation/05-native-control-vacuum.md'
SENTINEL = '<!-- hand notes: 生成器（tools/Report-Control-Vacuum.ps1）原样保留这一行往下的一切内容 -->'

s = io.open(PATH, encoding='utf-8', newline='').read()
assert SENTINEL not in s, 'already restructured'
assert '## F. 2026-09-19' in s, 'section F missing - nothing to preserve'

# 1. lift the section D warning out of section D
warn_anchor = "**这份名单已过期**：`MenuFlyout`、`MenuFlyoutPresenter`、`FlyoutBase` 三条是假阴性，见下面 F 节的复测。"
assert s.count(warn_anchor) == 1, 'D warning not found exactly once'
s = s.replace('\n' + warn_anchor, '', 1)

# 2. the generated caveat tail must end the generated body; hand notes go below it, after the sentinel.
#    The previous batch restored section F *above* the caveat tail, so this reorders the two.
tail_head = '## 这份清单证明不了什么'
f_start = s.index('## F. 2026-09-19')
i_caveat = s.index(tail_head)
if f_start < i_caveat:
    f_block = s[f_start:i_caveat].rstrip()
    caveat_block = s[i_caveat:].rstrip()
    head = s[:f_start].rstrip()
else:
    head = s[:f_start].rstrip()
    caveat_block = s[f_start:i_caveat].rstrip()
    f_block = s[i_caveat:].rstrip()
assert '## F. 2026-09-19' in f_block and tail_head in caveat_block, 'reorder lost a block'

# 3. the warning becomes the first bullet of the hand block, reworded for its new position below D
f_body = f_block.split('\n')
insert_at = next(k for k, line in enumerate(f_body) if line.startswith('- '))
bullet = ('- **这一条原先手写在 §D 段里**：`MenuFlyout`、`MenuFlyoutPresenter`、`FlyoutBase` 三条是假阴性，'
          '以本节的复测为准（搬到这里，是为了让生成器重跑时不再把它删掉）。')
f_body.insert(insert_at, bullet + '\n')
f_block = '\n'.join(f_body).rstrip()

out = head + '\n\n' + caveat_block + '\n\n' + SENTINEL + '\n\n' + f_block + '\n'
io.open(PATH, 'w', encoding='utf-8', newline='').write(out)

chk = io.open(PATH, encoding='utf-8', newline='').read()
checks = {
    'sentinel present once': chk.count(SENTINEL) == 1,
    'caveat tail above sentinel': chk.index(tail_head) < chk.index(SENTINEL),
    'section F below sentinel': chk.index('## F. 2026-09-19') > chk.index(SENTINEL),
    'D warning folded into hand block': chk.count('三条是假阴性') == 2 and '见下面 F 节的复测' not in chk,
    'no orphan blank run': '\n\n\n\n' not in chk,
    'findings kept': 'MenuFlyoutPresenter(MenuFlyout)' in chk and 'UseTemplateContentManagement' in chk,
    'fresh counts kept': '| 其中我们已声明隐式样式 | 30 |' in chk,
}
bad = [k for k, v in checks.items() if not v]
print('\n'.join(f'{"ok  " if v else "FAIL"} {k}' for k, v in checks.items()))
if bad:
    sys.exit(f'postcondition failed: {bad}')
print(f'lines={len(chk.splitlines())}')
