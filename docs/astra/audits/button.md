# 审计：Button 族（第一段 Default/Accent/Subtle + 第二段 Toggle/Repeat/Hyperlink）

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/Button_themeresources.xaml`，blob `e63c32389e599c2277061d9ce29483516e6927a3`
  - 该文件里只有 **36 条别名 + 1 条 `ButtonBorderThemeThickness` + 10 条遗留 `*ThemeBrush`**，
    样式也只有 **3 个**：`DefaultButtonStyle`、`AccentButtonStyle`、`SubtleButtonStyle`。
- 分级：**原生控件 + 我们的重模板**，颜色层是上游别名层的原样转录。
- 本批分次落地：这一段只做 `Button` 的三个样式；`ToggleButton`、`RepeatButton`、
  `HyperlinkButton` 各自的别名块与状态映射留同一批的下一段（它们在 `*_themeresources.xaml`
  里是**另外的文件与另外的 blob**，不混在一份审计里冒充同一份证据）。

## 先更正计划里的一个凭印象项

阶段 2 的队列写着 "Default/Accent/Subtle/**Compound**/Link/Repeat/Toggle"。
这个 commit 的上游**没有** `CompoundButtonStyle`，也没有 `CompactButtonStyle`
（`controls` 全树里查不到这两个键名）。`CompoundButton` 在 WinUI 里是
`CheckBox`/`RadioButton`/`ToggleSwitch` 的**基类**，不是按钮样式。
所以 Button 批的样式清单以这一份为准：**Default、Accent、Subtle** 三个 +
`RepeatButton`/`ToggleButton`/`HyperlinkButton` 三个控件自己的样式；
"SplitButton/DropDownButton" 已结清（收尾批，`audits/splitbutton.md`）：`SplitButton` 本运行时**有原生类型**，走的是原生重模板；只有 `DropDownButton` 需要自有类型，`ToggleSplitButton` 至今没有，因此 13 条 `*Checked*` 行随之推迟。

## 上游 `DefaultButtonStyle` 的形状

根是**一个 `ContentPresenter`**（不是 Border），上面直接挂
`Background`/`BorderBrush`/`BorderThickness`/`CornerRadius`/`Padding`，加
`BackgroundTransition`（BrushTransition，83ms）与一个 `VisualStateManager.VisualStateGroups`：
`CommonStates` 的 `PointerOver`/`Pressed`/`Disabled` 三个状态用
`ObjectAnimationUsingKeyFrames` 改 `Background`/`BorderBrush`/`Foreground`，
并且每帧都附带一个 `controls:AnimatedIcon.State` 的 setter（图标随状态换帧）。
焦点框不在模板里——上游靠 `UseSystemFocusVisuals` + `FocusVisualMargin=-3` 交给系统画。

## 状态映射表（WinUI VisualState → 本运行时）

标记版 VSM 在本运行时直接抛（`adaptation/00`），所以状态一律走 `Style.Triggers`。
每条都断言了"该状态确实有一个触发器，且它带的是上游那一个键名"：

| 上游状态 | 我们的触发器 | 带的键（逐个断言） |
|---|---|---|
| `PointerOver` | `Trigger IsMouseOver=True` | `ButtonBackgroundPointerOver`、`ButtonBorderBrushPointerOver` |
| `Pressed` | `Trigger IsPressed=True` | `ButtonBackgroundPressed`、`ButtonForegroundPressed`、`ButtonBorderBrushPressed` |
| `Disabled` | `Trigger IsEnabled=False` | `ButtonBackgroundDisabled`、`ButtonForegroundDisabled`、`ButtonBorderBrushDisabled` |
| `CommonStates/Normal` | 样式本体（无触发器） | `ButtonBackground`、`ButtonForeground`、`ButtonBorderBrush` |
| `AnimatedIcon.State` | **无法映射**：`AnimatedIcon` 这个名字在 26.10.9 的 61 个 dll 里查不到 | — |
| 焦点框 | 2026-09-22 改：样式里 `FocusVisualStyle={ThemeResource FocusVisualRingStyle}`，环本体在 `Styles/FocusVisuals.jalxaml`。原来是模板里的 `FocusOutline` Border + `Trigger IsKeyboardFocused=True`——那一格鼠标点也满足，所以点击出环（`audits/focus-visual.md`） | `FocusStrokeColorOuterBrush`/`Inner`（两枚令牌未变） |
| `BackgroundTransition` | `Border.TransitionProperty="Background, BorderBrush"` + `TransitionDuration=0:0:0.083`（**字面量**，B5 要键化） | — |

## setter 逐项处置（上游有、我们有没有）

| 上游 setter | 处置 | 依据 |
|---|---|---|
| `Background`/`Foreground`/`BorderBrush`/`BorderThickness`/`Padding`/`CornerRadius` | **落地**，且改吃上游别名键 | 4 条 `Assert.Same` + 布局读回 |
| `HorizontalAlignment=Left`、`VerticalAlignment=Center`、`FontWeight=Normal` | **本段补上**（原来缺） | 读回 `Left`/`Center`/`Normal`；这三条是**布局可见**的：静止按钮不再横向拉满 |
| `BackgroundSizing=InnerBorderEdge` | **属性不存在**（61 个 dll 查无此名） | 边框内侧着色的效果做不到，1px 边框会吃掉内容 1px |
| `UseSystemFocusVisuals`、`FocusVisualMargin` | **属性不存在** | 焦点框只能自绘（见上表） |
| `ContentTransitions` | **属性不存在** | — |
| `FontFamily={ThemeResource ContentControlThemeFontFamily}`、`FontSize={ThemeResource ControlContentThemeFontSize}` | 上游那两个键**不在控件资源里**（在系统资源里），`ControlContentThemeFontSize` 是 `x:Double`，本运行时标记读不了 → `FontSize=14` 保持字面量 | 与 ToolTip 字号同一原因 |
| `MinWidth=0`、`MinHeight=32` | **我们自加**（上游样式里没有这两条；32 是 Padding+14px 行高自然得到的） | 记在这里，等 Gallery 页做逐像素对照时决定是否去掉 |

## 别名层：36 行逐字，5 行偏差

`ThemeResources/Button.jalxaml`（新增，进 Manifest）。
31 行是原样转录（键名与目标调色板键都与上游一致）；偏差 5 行，每行在文件里就地标注：

1. `AccentButtonForegroundDisabled`：上游指向 `TextOnAccentFillColorDisabled`（一个 **Color**），
   本运行时没有"Color→Brush"的槽位转换证据，改指同一 token 的 `…DisabledBrush` 形式。
2. `ButtonBorderBrush`、`ButtonBorderBrushPointerOver`：上游是 `ControlElevationBorderBrush`——
   一条 `MappingMode=Absolute, 0,0→0,3` 的两段渐变（`ControlStrokeColorSecondary` → `ControlStrokeColorDefault`，
   Dark 还带 `ScaleY=-1` 翻转）。渐变停止点写 `{ThemeResource}` 会在解析期冻住（`adaptation/00`），
   跟不上"同一批对象就地改色"的模型，所以取上游自己按下的那一档实底 `ControlStrokeColorDefaultBrush`。
   旁证：上游 `HighContrast` 分支里这三条 elevation 刷**本来就是实底** `SystemColorWindowTextColor`。
3. `AccentButtonBorderBrush`、`AccentButtonBorderBrushPointerOver`：同理取
   `ControlStrokeColorOnAccentDefaultBrush`。

**10 条遗留 `*ThemeBrush`（phone 时代）不声明**：上游自己的样式也不消费它们，与 ToolTip 那三条同一处置。

## 第二段：ToggleButton / RepeatButton / HyperlinkButton（另三份 blob）

三个控件各自一份字典，各自一个 blob，不混进 `Button.jalxaml` 冒充同一份证据：

| 新文件 | 上游 blob | 上游 Light 分支 | 逐字 | 偏差 |
|---|---|---|---|---|
| `ThemeResources/ToggleButton.jalxaml` | `868646a15b5a5062046b1edcc003f44288793b52` | 36 条别名 + `ToggleButtonBorderThemeThickness` | 29 | 7 |
| `ThemeResources/RepeatButton.jalxaml` | `80add1b94686ed1e408ce05afde9d02249b5e229` | 12 条别名 + `RepeatButtonBorderThemeThickness` | 10 | 2 |
| `ThemeResources/HyperlinkButton.jalxaml` | `93b5efd391803a229e63e55c315e5675fef4362e` | 12 条别名 + `HyperlinkButtonBorderThemeThickness` | 12 | **0** |

偏差仍是同两类老原因（`…ForegroundCheckedDisabled` 的 Color→Brush；elevation 渐变改实底），
每行在文件里就地标注。HyperlinkButton **一条偏差都没有**：它要的刷全在调色板里，
包括 Button 家用不上的 `AccentTextFillColorDisabledBrush`。

样式侧的处置：

- `DefaultToggleButtonStyle` 从"直接吃调色板键"改吃 `ToggleButton*` 键，并把上游 `Checked` 家族
  补齐（原来勾选态只有 Background/Foreground，现在三件套 + Checked×{PointerOver,Pressed,Disabled}
  的 MultiTrigger 全带上游键名）。样式名与上游一致。
- 新增 `DefaultRepeatButtonStyle`（上游同名），隐式 `RepeatButton` 从"借用 `DefaultButtonStyle`"
  改成自己的样式：`RepeatButton*` 键与 `Button*` 键是**两套公开契约**，借用会让应用按上游名字
  覆盖无效。上游这条样式自己吃 `ButtonBorderThemeThickness` 与 `ButtonPadding`，
  那两个键因此仍只声明在 `Button.jalxaml` 里（跨文件依赖，不是重复）。
- `DefaultHyperlinkButtonStyle` 不再 `BasedOn SubtleButtonStyle`，改为 `ButtonLayoutStyle` +
  `HyperlinkButton*` 全家族。上游样式里 `Padding` 用 `{ThemeResource ButtonPadding}`
  而同文件别处用 `{StaticResource …}`；我们统一按 `ButtonLayoutStyle` 的 `{StaticResource}` 走，
  因为 `ButtonPadding` 不在主题分支里，`{ThemeResource}` 只会多一次无谓的解析。
- **`IsChecked=null`（三态）**：本节写作时只有键名证据，`{x:Null}` 条件是否命中没有取过证。
  选择批收尾时**已量穿**：样式格子里的 null 条件命中且可逆，混合态四组消费点全部落地，
  连"真实点击循环能不能走到 null"也一起有了答案（走得到，走框架的 `IToggleProvider`）。
  同时更正本节的一个提法：混合态与休息态在上游**同色**，所以那条像素归因问题不存在可测的差。
  全部读数、映射表与新的不声称清单在 `audits/togglebutton.md`。

一条命名空间事实（`The_button_family_is_split_across_two_namespaces` 逐条断言）：
`Button`、`HyperlinkButton` 在 `Jalium.UI.Controls`，`ToggleButton`、`RepeatButton` 在
**`Jalium.UI.Controls.Primitives`**（`ButtonBase` 也在 Primitives）——上游四个全在
`Microsoft.UI.Xaml.Controls`。抄 WinUI 的 using 清单写出来的代码在这里编译不过，
1.0 的 CLR API 清单要按运行时形状记。

## 五条新的框架事实（对后面每一批都管用）

1. **样式 setter 里的 `{ThemeResource X}` 存的是惰性引用**，不是刷对象：
   读 `Trigger.Setters[i].Value` 拿到的是 `DynamicResourceReference { ResourceKey = "X" }`。
   这解释了翻主题能重绘已经应用过的样式，也意味着"触发器带的值"只能按**键名**断言，
   按键→对象的解析由别名测试那一半负责。
2. **`FluentThemeManager.GetBrush` 只看得见调色板键**，别名键要走应用级查找
   （`Application.TryFindResource`）。测试里两套查法的区别就在这里。
3. **状态换刷这条路是通的，而且与过渡层不冲突**：带 `TransitionProperty="Background"` 的模板面
   在首帧之后仍然采纳新的刷对象（`swap` 读数 `transitionedAdopted=True`，与无过渡模板一致）。
   代价是**捕获时机会采到中间值**——`#E482E4`（品红↔绿）就是这么来的。
   见 `audits/button-input-raw.txt` 与 `adaptation/06` 里"重复捕获直到两张图一致"那条规则。
4. **触发器的 `Value` 保留标记里的字符串形态**：`Condition.Value` 读出来是 `"True"` 而不是 `true`，
   所以按键匹配触发器要按文本比。运行时仍然拿它对上 `bool?` 属性——勾选态的像素断言就是证据
   （`IsChecked=true` 的按钮整片显强调色）。
5. **文字不在这条像素通路里**：只画文字的捕获写 0 个像素，而且慢到能挂住 UI 线程
   （实测一次让 fixture 的 60 s 上限耗尽，连坐 4 条测试）；即使底下有不透明实底，
   glyph 也不进直方图（红底 HyperlinkButton 的 8800 px 里 8788 是底色）。
   结论：**像素断言只能断"面"，文字色必须走读回**（新增 `PixelHarness.Build`：建树、推帧、不光栅化）。
   这一条限制的是测量通路，不是说上屏看不见字——Gallery 里字是看得见的。

## 指针输入通路（这一批建立，后面每一批都用）

`spike/PointerProbe`（默认只做 hover，不发送任何按键）。输入走 user32 `SetCursorPos`，
框架看到的是真实 `WM_MOUSEMOVE`，不是谁把属性写成 `True`。跑通的链条：

```
SetCursorPos(按钮中心) → UIElement.IsMouseOver=True → 样式触发器（owner 正是 UIElement，不是被遮蔽的副本）
→ 控件 Background 换成悬停刷 → 模板面 Background 同步 → 稳定捕获里 8296 px 是悬停哨兵色
```

读数与误读都留在 `audits/button-input-raw.txt`（5 次 hover run + 1 次 swap run）。两点必须记住：

- **这条通路不能进常跑闸口**：run 3/4 的"属性绿、像素品红"不是框架缺陷，是物理光标在推帧期间
  自己走掉了（`asked=(1237,634)`，而 `actual=(1478,702)` / `(4,4)`）——同一时间有人在用这只鼠标。
  常跑测试因此换成**不需要指针**的版本：
  `A_swapped_brush_object_reaches_the_transitioning_surface` 直接换 `Background` 刷对象，
  走的正是触发器给控件换刷之后的同半段（换对象 → 模板面 → 像素）。
- **按下/释放、键盘激活、触摸都没有证据**：`press` 模式要 `SendInput` 真按键，会点到用户桌面上
  任何东西，未经同意不运行（该模式从未执行，所以原始读数里连橙色一行都没有）。

## 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行全绿（restore → build 0 警告 → 54/54、0 skip →
  调色板漂移 checked=True，102 刷）。
- **行为**：`AstraButtonTests` 5 条——别名身份（5 个 `Assert.Same` + `ButtonBorderThemeThickness`
  读回 `1`）、**全家 73 行别名逐行身份**（`Every_button_family_alias_resolves_to_its_target_instance`，
  四份字典一次扫完）、状态映射（Button 6 条 + Toggle/Repeat/Hyperlink 13 条触发器逐条对上上游键名）、
  布局默认值（8 项读回）、命名空间形状（4 条）。
- **视觉**：`A_disabled_button_paints_the_upstream_disabled_fill`
  （覆盖 `ControlFillColorDisabledBrush` 为哨兵后，禁用按钮 >1000 像素命中、启用按钮 0 命中——
  既证明禁用分支真的落到像素，也证明别名没有变成副本）；
  `A_checked_toggle_and_a_resting_repeat_button_paint_their_upstream_fills`
  （勾选态强调色 >6000 px、未勾选 0 px；`RepeatButton` 自己的休息色 >6000 px）；
  `A_swapped_brush_object_reaches_the_transitioning_surface`（首帧后换一个新的 `Background` 刷对象，
  稳定捕获显新色 >4000 px——把"过渡层会不会吞掉换刷"这条风险钉成常跑断言）；
  `The_implicit_hyperlink_style_reaches_the_text_element_it_builds`（文字色走读回：
  生成出来的文字元素的 `Foreground` 就是 `HyperlinkButtonForeground` 那个调色板实例）；
  换别名层之后，原有的 `The_implicit_astra_style_reaches_a_native_button` 与
  `The_accent_token_reaches_an_explicit_button_style` 仍然全绿，这本身就是"就地改色穿过别名"的证据。
- **硬件输入**：**悬停这一段有像素证据，按下/键盘/触摸没有**。
  `PointerProbe` 用 `SetCursorPos` 走进真实 `WM_MOUSEMOVE`，一次 run 走通
  `IsMouseOver=True → 触发器 → 悬停刷 → 8296 px 悬停哨兵色 → 离手回休息位`
  （`audits/button-input-raw.txt` run 5，`RESULT pass`）。
  这条不常跑（依赖物理鼠标，run 3/4 被人同时用鼠标打断过），常跑的是它的无指针等价段。
  `PointerOver` 因此从"只有结构证据"升级为"结构 + 一次性真指针像素"；`Pressed` 仍只有结构证据。

## Known Gaps（不许用相邻证据替代）

1. **按下/释放、键盘激活（空格/回车）、触摸路径的像素未证**。悬停有一次性真指针证据，但它不可复现地
   依赖物理鼠标与"这段时间没人动鼠标"，所以没有进闸口；`press` 模式要 `SendInput` 真按键、
   会点到用户桌面上任何东西，未经同意不运行。常跑的换刷断言只覆盖"换对象→像素"这半段，
   **不能**替代"输入→状态"这半段（除悬停外）。
2. `BackgroundSizing=InnerBorderEdge` 无对应属性：1px 边框与内容的相对位置与上游不同。
3. elevation 边框（4 行）用实底近似上游 3DIP 渐变，顶边/底边不会出现上游的深浅分层。
4. `AnimatedIcon` 状态图标整条缺失：内容里有 `SymbolIcon`/`FontIcon` 的按钮不会有换帧动画。
5. 系统焦点框的属性缺失，我们把自绘的环交给框架自己的焦点视觉（`FocusVisualStyle`，门是 `FocusVisualManager.ShowFocusCues`），偏移进环的模板，因为本运行时不公开 `FocusVisualMargin`。与上游 `FocusVisualMargin=-3` 的框仍不逐位一致。（2026-09-22 改，`audits/focus-visual.md`）
6. `MinWidth=0`/`MinHeight=32` 是我们自加的约束，上游样式没有。
7. `ContentTransitions`、`TransitionDuration` 仍是字面量（B5 未完成）。
8. **三态通路已量穿**（原为"12 条 `ToggleButton*Indeterminate*` 键声明了但无消费点"）：
   `{x:Null}` 在模板格子与**样式格子**里都实测命中且可逆，11 格 × 3 属性 = 33 个消费点
   由 `AstraToggleButtonTests` 按条件顺序逐个断言；真实点击循环走到 null 也已断言
   （框架的 `IToggleProvider.Toggle()`，两态/三态各一条，禁用时抛 `InvalidOperationException`）。
   同一批更正了两句话：混合态与休息态在上游**同色**，所以"混合条像素归因"不存在可测的差；
   而"仍未测"的那半段（点击循环）现已测完，剩下没测的是 `Pressed`/键盘/触摸的像素。
   另记一条会咬人的实测：`IsChecked="{x:Null}"` **写在标记里会得到 `false`**，三态只能在代码里设
   （Gallery 的三态示例已按此改）。
   `RepeatButton`/`HyperlinkButton` 的别名块与状态映射本段已转录完毕；
   本批另把 Button 全族四份字典纳入键消费闸口，抓出 4 条无人读的 `…ForegroundPointerOver`
   与 1 条上游自己就不读的 `RepeatButtonBorderThemeThickness`（见 `audits/togglebutton.md` S7/S8）。
9. **文字色不能用像素断**：只画文字的捕获写 0 像素并会拖垮 UI 线程，字形不进直方图。
   本库的文字色一律走"建树 + 读回实例"（`PixelHarness.Build`），
   所以"某个文字令牌在上屏画面里真的是这个颜色"这句话，我们的闸口证明不了，只能证明到"实例接上了"。
10. 上游 10 条遗留 `*ThemeBrush` 不声明，若某个应用真的按老名字取刷，这里会落空。
11. **Gallery 页已落地（九步第 7 步），但它本身不是外观证据**。`samples/FluentJalium.Gallery`：
    Buttons 页补了 Toggle/Repeat/Hyperlink 家族卡与按住连发读数，Overview 页加了内嵌 ScrollViewer
    与静置 ToolTip 表面卡；底部 parity 条与"未声称"行由 `Catalog.json` 驱动，
    四个按钮族在目录里的 parity 是 `audited`，各自的欠账逐条写在该文件里。
    仍然欠的是这一页的**可见结果**：运行时冒烟只目视到 Overview 一页，家族卡没有断言、也没看到，
    且屏幕坐标点击已被判定不可用（见 `adaptation/10`）→ 逐页证据归 E4 的进程内渲染。
