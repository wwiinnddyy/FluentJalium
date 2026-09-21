"""Writes the foreground-audit doc trail: raw probe log, the closed clause in adaptation/00, the ROADMAP section.

Byte-level writes only: the astra docs are LF in the working tree (git ls-files --eol says i/lf w/lf), so any
newline translation here would turn a five-line insert into a whole-file diff.
"""

import io
import os

os.chdir(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))

PROBE = "spike/ForegroundProbe/bin/Debug/net10.0-windows/foreground-probe.txt"
LOG = "docs/astra/adaptation/s6-foreground-probe-raw.txt"
CAPABILITIES = "docs/astra/adaptation/00-jalium-theme-capabilities.md"
ROADMAP = "docs/astra/ROADMAP.md"

head = """-- spike/ForegroundProbe 原始读数（2026-09-21，运行时 26.10.9，FluentThemeManager Light/Dark 两档）
-- 判据：具名前景声明从 markup 读，"到不到"由两问决定——键解析出的色是否等于部件自身前景、翻主题后色是否变。
-- 读数全是树上读数（老账 #50：文本字形在任何捕获通路都拿不到墨）。
-- OK/DEAD = 具名声明到不到达；GAP = 探针取不到该部件或该 owner 没有 subject 行；
-- LOWCONTRAST/READABLE = 文本节点对其背景的 WCAG 比。背景先沿祖先找不透明刷，再看同一重叠布局面板里画在它
--   前面的兄弟图元（InfoBar 的严重度圆盘就是兄弟 Ellipse）；"painted by" 点出这块底究竟谁画的，page = subject
--   内没有不透明底。
-- 本文件由那次运行原样拷贝；三次仪器纠错的经过见 audits/foreground.md 第 4 节。

"""

io.open(LOG, "w", encoding="utf-8", newline="\n").write(head + io.open(PROBE, encoding="utf-8").read())
print("raw log ->", LOG)

old = "   **未审的相邻面**：其余自有样式里凡是靠继承拿前景色的文本部件都可能同样哑色——已开任务，不在本段结。"
new = (
    "   **相邻面已审结**（阶段 6 尾批之后，`audits/foreground.md`）：全库 243 条前景声明（attribute 15 + `Setter` 228）\n"
    "   在两档主题下逐条解析，无一死键；42 个可读文本节点全部随主题翻转，InfoBadge 那一类在本层是孤例且已修。灵敏的那道\n"
    "   闸是\"键必须解析成刷\"——令牌与继承墨同色的配对（InfoBar 标题）改坏键名时读数断言不红，这条边界见同文件第 5 节。"
)
data = io.open(CAPABILITIES, "rb").read()
assert data.count(old.encode("utf-8")) == 1, data.count(old.encode("utf-8"))
io.open(CAPABILITIES, "wb").write(data.replace(old.encode("utf-8"), new.encode("utf-8")))
print("clause 4 closed")

section = """## 缺陷批：全库前景继承审计（结清 #51，以及 #55 的前景一半）

结清 `adaptation/00` 第 4 条留下的那句"未审的相邻面"：把这一层写前景的形状列成清单逐条问"到不到"，而不是再逐控件猜。
读数与仪器纠错在 `audits/foreground.md`，原始日志 `adaptation/s6-foreground-probe-raw.txt`，闸口
`tests/FluentJalium.Tests/AstraForegroundAuditTests.cs`（14 条）。

量到的事实：

1. 这一层写前景有三种形状，而之前只数过第一种——attribute 15 行（9 个令牌键 + 6 条 `{TemplateBinding}`）、
   `Setter Property="Foreground"` **228** 行、外加 `DataTemplate` 里 1 条无名行（评级未选星，前两种的扫描漏它）。
2. **无一死键、无一处不到达**：两档主题各把 243 条点名的键解析一遍，全是 `SolidColorBrush`；可达的具名声明逐条比对
   部件自身前景全对；42 个可读文本节点全部随 Light↔Dark 翻色。InfoBadge 那类哑色在本层是孤例，且已经修掉。
3. **低对比只剩 5 条且与上游同源**：评级满星用强调色（Light 4.08 / Dark 9.06），那是强调色文本在浅档页面上的固有值。
4. 钉住两对容易改坏的组合：InfoBar 严重度标记 `#FFFFFF` 画在**兄弟** `Ellipse.IconBackground` 的 `#0078D4` 圆盘上
   （4.53 / 11.67），评级未选星 Light `#9E000000`、Dark `#C5FFFFFF`（上游 61% 黑 / 77% 白）——断言连 alpha 一起钉。
5. 框架事实（省掉后面每一族一次试验）：`Control`、`TextBlock`、`TextElement` 的 `ForegroundProperty` 是**同一个对象**
   （`DependencyProperty.AddOwner` 直接 `return this`，`Jalium.UI.Core/DependencyProperty.cs:397-416`）且
   `inherits: true`，所以一次翻主题能同时判全库，"读部件自己的属性"也不必在两个 DP 之间挑。
6. 仪器自己错了三次，每次都产出一条假缺陷：`ReadLocalValue` 看不见 markup 写；沿祖先链找背景看不见兄弟图元、且共用
   宿主会把别的被测控件当成"前面的兄弟"；打印色不带 alpha 把 61% 黑读成纯黑。第三次顺带揭出"全库只有 16 处写前景"
   这句是错的——那只是 attribute 形状。

四类证据：**构建**见下闸口读数；**行为**新增 14 条（两档键解析、attribute 键清单 + TemplateBinding 计数、InfoBar
三部件两档读数、6 族文本节点随主题翻转、未选星 alpha、标记对圆盘的对比下限）；**视觉**本段没有像素断言（#50 未结，
文本拿不到墨），色身份由树上读数与合成对比承担；**硬件输入**本段不动输入路径。

**牙的验证（A/B）**：把 `Styles/Surfaces.jalxaml:170` 的 `InfoBarTitleForeground` 改成不存在的键，重建后跑这 14 条 →
**3 红**（解析闸 Light/Dark 两档 + 清单闸）、11 绿；改回 → 14/14 绿，该文件 `git diff` 为空。这次同时量出闸口的灵敏
边界：**"读部件自身前景"那条保持绿**，因为标题的令牌色与继承墨本来就同色——只有令牌≠继承色的配对（`IconGlyph`
白对继承黑、InfoBadge 值对强调底）才由读数发现死键，所以真正灵敏的是"键必须解析成刷"那道，读数断言是第二道。

不声称：探针没有 subject 行、因此**未测到达与否**的 7 个 owner（`AppBarToggleButton`、`DataGridCell`、
`DataGridColumnHeader`、`MenuBarItem`、`MenuFlyoutItem`、`MenuFlyoutSubItem`、`ToggleMenuFlyoutItem`），以及 5 处
树上取不到的具名声明（TeachingTip 标题/副标题、`FluentTabViewItem.IconHost`、
`FluentBreadcrumbBarItem.PART_ChevronTextBlock`、`ComboBox.PART_ScrollViewer`）——它们只被"键必解析"覆盖；
兄弟图元判底用的是"重叠布局面板 + 尺寸同阶"的启发式（本运行时没有可用的跨要素变换读数）；高对比档下这些前景走不走
映射本段没测；228 条状态 Setter 只测了键能不能解析，没测"该状态下那一格真被画上"；#46 五处 1 DIP `Rectangle`
无墨仍未结。
"""

data = io.open(ROADMAP, "rb").read()
eol = b"\r\n" if b"\r\n" in data[-4000:] else b"\n"
payload = ("\n\n" + section).replace("\n", eol.decode()).encode("utf-8")
io.open(ROADMAP, "ab").write(payload)
print("ROADMAP appended with eol", eol, "bytes added", len(payload))
