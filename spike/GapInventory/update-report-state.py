"""Update gap-inventory.md to the state after the batch implemented §6.1 and §6.2.

The report originally said "this batch did not change these"; both have since been implemented and
measured, so leaving those sentences would make the document claim a non-claim that is no longer true.
Each replacement is anchored and asserted, so a drifted file aborts instead of half-editing.
"""
import io
import sys

PATH = 'docs/astra/audits/gap-inventory.md'
s = io.open(PATH, encoding='utf-8', newline='').read()

NEW_61 = """### 6.1 `adaptation/05` 为什么能过期一整个阶段，以及现在怎么防止

`keys.md` 与 `known-gaps.md` 各有生成器 + `-Check`，串行闸口每次重算；**真空清单原本两样都没有**。
落库的那份停在"已上隐式样式 13 / 真空 41"，而当前树现算是 **30 / 23**。原因很具体：
**生成器只发 A-E 五段与那个"证明不了什么"的尾巴，而后来有人在同一个文件上手写了 §D 的失效警告与
整段 §F 复测（2026-09-19 对着 26.10.9 量出三条假阴性）——再跑一次工具，这两块就被删了。**
这份文件因此处在"不跑就过期、跑就丢结论"的两难里，而过期是无声的那一支。

本批把两难拆开了，三条都已落地：

| 落地 | 内容 |
|---|---|
| 生成器 | 哨兵 `<!-- hand notes: … 原样保留这一行往下的一切内容 -->` 以下**逐字带回**；生成的尾巴上加一行 `<!-- generated-lines=N sha256=… -->`，只盖生成段，所以改手写注记永远不会被当成过期 |
| `-Check` | **整文档比较**，不是只比戳。第一版写的就是只比戳，验牙当场量出它的瞎处：手改生成段里那个 `30`→`29`，戳没动、重跑生成的体也没动，`-Check` 照样绿——正是它该抓的那种漂移 |
| 闸口 | `Test-AstraGates.ps1` 末尾加一步 `==> control vacuum inventory`（缺 `../ModernWpf` 参考仓时**响亮地失败**，不给自己一个空绿的余地） |

验牙：`bash spike/GapInventory/check-teeth.sh` 七步 `TEETH OK`——①重跑并快照 ②`-Check` 绿
③**手改生成段必红**（报"first difference at line 12"）④按字节快照还原必绿
⑤哨兵以下加一条注记 → 重跑必须还在、`-Check` 必须仍绿 ⑥把 `ToolBar` 从分母里删掉，生成段
42 行不同（源侧漂移也红）⑦末态与快照字节相同。当前读数：`69 generated lines, 21 hand lines`，
文档 116 行、行尾统一 CRLF。剩下没做的一件事：`spike/ControlCensus` 那份普查输出仍是
2026-09-18 的，重跑它才算把"分母也跟上运行时"这条闭上。"""

NEW_62 = """### 6.2 文案与账本相互矛盾的一处（本批已结）

Gallery 的弹层那张卡悬停提示原本写着 **"Flyout surface, acrylic fill"**
（`samples/FluentJalium.Gallery/MainWindow.jalxaml:186`），而 `audits/flyout-presenter.md:85` 第 2 条
逐字是**"弹层材质是实底，不是 acrylic"**——界面向用户主张的比账本乐观。
已按账本改口为 "solid fill (this runtime has no AcrylicBrush; the key holds upstream's own FallbackColor)"，
同一句里另外两个数先各自核过再留下：描边 1 是 `ThemeResources/ToolTip.jalxaml` 那条
`ToolTipBorderThemeThickness`（每个分支都是 1），320 是上游 `ToolTipMaxWidth` 因 `x:Double`
载不动而落成的样式字面量（`audits/tooltip.md:27,72`）。

证据分两栏，不许互相顶：**构建** = `dotnet build samples/FluentJalium.Gallery -c Debug`
`0 个警告 / 0 个错误`（1 m 18 s，增量重跑读数相同）。**冒烟未拿到** =
`tools/Test-AstraGallerySmoke.ps1` 在同一步 `WaitForInputIdle(30s)` 上，**本批未动的 `surfaces`
与动过的 `overview` 一样红**，所以这条落 #47/#35/#90 那一族（机器负载下的等待判据），
不归给这次改动；这一句的判据是那次 A/B，不是推测。"""

pairs = [
    # §6.1 and §6.2 replaced wholesale
    ("""### 6.1 `adaptation/05` 为什么能过期一整个阶段：重跑它会吃掉手写复测""",
     """### 6.2 文案与账本相互矛盾的一处""", NEW_61 + "\n\n"),
    ("""### 6.2 文案与账本相互矛盾的一处""",
     """### 6.3 材质工作流（C 段）几乎没开工""", NEW_62 + "\n\n"),
    ("""本批只动文档与 `spike/`，**没有跑构建与测试**。串行闸口上一次全绿是 `feb45af` 的树""",
     """，其后 #96/#97/#98""", None),  # placeholder, handled below
]

# apply the two section rewrites by [start, next-heading) slicing
for start, end, replacement in pairs[:2]:
    assert s.count(start) == 1, 'section start not unique: ' + start[:30]
    assert s.count(end) == 1, 'section end not unique: ' + end[:30]
    i, j = s.index(start), s.index(end)
    assert i < j, 'section order wrong: ' + start[:30]
    s = s[:i] + replacement + s[j:]

old_64_head = ("本批只动文档与 `spike/`，**没有跑构建与测试**。串行闸口上一次全绿是 `feb45af` 的树")
new_64_head = ("本批跑到的：三条 `-Check` 各自绿（`known-gaps.md is current: 1022`、"
               "`keys.md is current: 1312`、`control-vacuum.md is current: 69 generated + 21 hand`）"
               "与一次 Gallery 构建（`0 个警告 / 0 个错误`）。**没跑** `dotnet test` 套件，也没跑串行闸口"
               "与页级像素闸。串行闸口上一次全绿是 `feb45af` 的树")
assert s.count(old_64_head) == 1
s = s.replace(old_64_head, new_64_head)

old_item3 = ("   顺手给 `adaptation/05` 加 `-Check`（§6.1）。")
new_item3 = ("   （§6.1 那三项本批已落，`-Check` 与哨兵保留不再是待办。）")
assert s.count(old_item3) == 1
s = s.replace(old_item3, new_item3)

old_last = ("- 本批未跑构建、未跑测试、未跑串行闸口与页级像素闸；§6.4 引的是 `feb45af` 与前三次红的既有读数，不是本批的读数。")
new_last = ("- 本批没跑 `dotnet test` 套件、没跑串行闸口与页级像素闸；§6.4 的三条 `-Check` 与那次 Gallery 构建是本批自己的读数，"
            "而 `feb45af` 全绿与其后三次红是引来的旧读数，两者不要混着引用。\n"
            "- Gallery 冒烟在本批**未通过判据**，且已按 A/B 判给仪器：`surfaces`（未动）与 `overview`（动过）"
            "在同一步 `WaitForInputIdle(30s)` 上一样红；这句不是“这次改动没问题”的证明，只是“它不是那条红的原因”。")
assert s.count(old_last) == 1
s = s.replace(old_last, new_last)

io.open(PATH, 'w', encoding='utf-8', newline='').write(s)

chk = io.open(PATH, encoding='utf-8', newline='').read()
checks = {
    '6.1 rewritten': 'TEETH OK' in chk and '整文档比较' in chk,
    '6.2 rewritten': 'solid fill (this runtime has no AcrylicBrush' in chk,
    '6.3 still there': '### 6.3 材质工作流' in chk,
    '6.4 still there': chk.count('### 6.4 闸口读数（不粉饰）') == 1,
    'all four 6.x headings, once each': all(chk.count(h) == 1 for h in
                                            ('### 6.1', '### 6.2', '### 6.3', '### 6.4')),
    '6.4 updated': '三条 `-Check` 各自绿' in chk,
    'no stale claim': '本批已用工具重跑并落库' not in chk and '本批没改，改它要过构建与闸口' not in chk,
    'item 3 updated': '不再是待办' in chk,
    'section 7 intact': '## 7. 如果按这份清点继续' in chk,
    'section 8 intact': '## 8. Known Gaps' in chk,
}
bad = [k for k, v in checks.items() if not v]
print('\n'.join(f'{"ok  " if v else "FAIL"} {k}' for k, v in checks.items()))
if bad:
    sys.exit('postcondition failed: ' + repr(bad))
print('lines=%d' % len(chk.splitlines()))
