# 审计：ScrollViewer（滚动宿主）

- 上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
  - `controls/dev/CommonStyles/ScrollViewer_themeresources.xaml`，blob `800d5040a5e73c28b75c5c5d2f4af40c7a48256b`，223 行
- 分级：**底座，代码搭部件 + 隐式样式可达宿主本身**。
  滚动条本体（部件、状态、颜色通路）在 `audits/scrollbar.md`；这一份只管宿主。
- 全部结论来自 `AstraScrollHostTests` 的读回与像素计数，以及同一轮里三次一次性探针
  （探针代码不留仓库，读数记在下面）。

## 26.10.9 的宿主形状（反射 + 建树实测）

`ScrollViewer : ContentControl : Control`，不是 WinUI 的"`Control` + `ScrollContentPresenter` 组合"。
它的部件树是代码搭的：

```
Border > { ScrollBar > RepeatButton>Border>Path, Track>RepeatButton>Border>Thumb>Border,
                          RepeatButton>Border, RepeatButton>Border>Path,
           ScrollBar > ... }
```

- `Template` 默认为 **null**（框架没有通用主题，见 `adaptation/00`），宿主自己画 `Background`。
- 类型 `ScrollContentPresenter : ContentPresenter` **存在**，但**不在这棵树里**——
  内容直接由宿主呈现。所以上游模板的结构（`Root` Border → Grid →
  `ScrollContentPresenter` + 两条 ScrollBar + 分隔条）**无法照搬**，照搬等于换一个假的宿主。
- 宿主上可读的公开面（供 1.0 的 CLR API 清单用）：
  `CanContentScroll`、`CanScrollHorizontally/Vertically`、`ComputedHorizontal/VerticalScrollBarVisibility`、
  `ContentHorizontal/VerticalOffset`、`Extent/Viewport/Scrollable{Width,Height}`、
  `Horizontal/VerticalScrollBarVisibility`、`IsAtHorizontal/VerticalEnd`、`IsDeferredScrollingEnabled`、
  `IsOverlayScrollBarEnabled`、`IsScrollBarAutoHideEnabled`、`IsScrollInertiaEnabled`、
  `PanningMode`、`PanningDeceleration`、`PanningRatio`、`ScrollInertiaDurationMs`。
  `ScrollBar` 另有 `Track`、`ThumbStyle`、`IsThumbSlim`、`ViewportSize`；`Thumb` 另有 `ShowGrip`、`IsDragging`。

## 上游 7 个公开键的处置

| 上游键 | 上游值 | 处置 |
|---|---|---|
| `ScrollViewerScrollBarSeparatorBackground` | `ControlFillColorTransparentBrush`（HC：`SystemControlTransparentBrush`） | **不声明**：三分支都是透明，且我们没有那条分隔条的绘制点 |
| `ScrollViewerScrollBarMargin` | `Thickness` 1 | **不声明**：两条 ScrollBar 由代码摆放，样式的这个键没有消费点 |
| `ScrollViewerSeparatorExpand/Contract*`（5 个 `x:String` 时长） | 0.1s/0.4s/2s 等 | **不声明**：它们喂给 `VisualStateGroup.Transitions`，而标记版 VSM 在本运行时直接抛（`adaptation/00`） |

也就是说：**上游这个控件没有颜色面**（宿主透明、分隔条透明），
它的可见部分全在滚动条上——所以颜色工作的全部产出在 `audits/scrollbar.md` 那两条钩子里。

## 上游样式设置项的可达性（逐项实测）

| 上游 setter | 我们的处置 | 依据 |
|---|---|---|
| `IsTabStop=False` | **落地**（框架默认是 `True`，这是唯一真正的行为差） | 落地后从宿主读回 `False` |
| `Padding=0`、`BorderThickness=0` | 落地（框架默认已经是 0，写下来是钉住契约） | 读回 `0,0,0,0` |
| `BorderBrush=Transparent`、`Background=Transparent` | 落地（框架默认是 `null`） | 读回 `SolidColorBrush(Transparent)` |
| `HorizontalContentAlignment=Left`、`VerticalContentAlignment=Top` | 落地（与框架默认相同） | 读回 |
| `HorizontalScrollMode`、`VerticalScrollMode` | **属性不存在**（全仓 dll 里查无此名） | 反射清单里没有 |
| `IsVerticalRailEnabled`、`IsHorizontalRailEnabled` | **属性不存在** | 同上 |
| `UseSystemFocusVisuals` | **属性不存在** | 同上 |
| `ZoomMode` | 存在（dll 里有该名字），上游值就是 `Disabled`，不写 | — |
| `VerticalScrollBarVisibility=Visible` | **故意不落地**，保持框架的 `Auto` | 见下 |
| `MaxWidth`/`MinWidth` 之类 | 不涉及 | — |

`VerticalScrollBarVisibility` 是唯一一个"上游有、我们也有、但决定不照抄"的项：
上游靠 `ScrollingIndicatorStates` 让"Visible 的滚动条"在静止时不可见；
26.10.9 的静止画面是**两条灰色箭头常驻**（`#D2D2D2`，实测 80 px，且主题无关）。
照抄 `Visible` 只会把 `audits/scrollbar.md` 记录的那个缺陷铺到每一个可滚动表面上。

## 三条实测出来的"够不着"

1. **`Application.Resources[typeof(ScrollBar)] = style` 连值都落不上**：
   setter 写 `Padding=7`，建出来的 ScrollBar 读回仍是框架代码设的 `2`。
   这把 `audits/scrollbar.md` 里"隐式样式测不动像素"升级为"隐式样式根本不生效"。
2. **拇指的样式由框架自己持有**：`ScrollBar.ThumbStyle?.TargetType == typeof(Thumb)`，
   且拇指的 `Background` 就是调色板的 `ScrollBarThumb`（Light 读回 `#72000000`）。
   这解释了为什么只有 `ScrollBarThumb`/`ScrollBarTrack` 两个名字能推动像素：
   框架内部那份 ThumbStyle 读的就是它们。
3. **箭头没有任何可用的应用级资源名**：dll 元数据里有
   `ScrollBarArrowBrushKey`/`ScrollBarArrowHoverBrushKey`/`ScrollBarArrowPressedBrushKey`/
   `ScrollBarBrushKey`/`ScrollBarColorKey` 这类内部键名，但把它们指向的公开名
   （`ScrollBarArrow`、`ScrollBarArrowFill`、`ScrollBarArrowBrush`、`ScrollBarArrowHover`、
   `ScrollBarArrowPressed`、`ScrollBarThumbHover`、`ScrollBarBrush`、`ScrollBarColor`、
   `ScrollBarBackground`、`ScrollBarSeparator`）**逐个**塞进 `Application.Resources` 后，
   静止灰色一个都没动（10 次测量全部 `sentinel=0, grey=80`）。
   内部键名不在公开面上，符合 `adaptation/00` 的"主题机器多数是 internal"。

## 两条实测出来的"能按的开关"（都还没用）

- `IsOverlayScrollBarEnabled = true` → 静止灰色 **80 → 0**，整幅只剩内容。
  这是目前唯一能复现 WinUI 静止外观（平时看不见滚动条）的杠杆。
  **没有随本项落地**：我们还没有"悬停/滚动时它会回来"的证据（输入证据要在 Button 批补），
  而在没有证据前把唯一的滚动提示藏掉是更糟的赌注。
- `IsScrollBarAutoHideEnabled` 开与关 → 静止画面**完全一致**，说明它不是那条路。
- `VerticalScrollBarVisibility = Hidden` → ScrollBar 的 `Visibility` 变 `Collapsed`（会整条消失，
  不是上游的"淡出"）。

## 一条必须记住的框架陷阱

给宿主设**不透明** `Background` 后，整幅捕获只剩那一种颜色（`8800/8800` 全哨兵），
灰色箭头为 0——也就是说宿主的背景把代码搭的滚动条盖掉了。
所以我们的宿主样式只能给 `Transparent`，任何实底都会吃掉滚动条。
（机制未证，只证了现象；上游的模板里背景在部件之下，所以对它不是问题。）

同轮另有一条：**往 `Application.Resources` 增删条目会让框架重新解析 `{ThemeResource}`，
从此不指向我们调色板那些实例**。表现为随后的 Button 像素覆盖断言失败
（`ControlFillColorDefaultBrush`/`AccentFillColorDefaultBrush` 的覆盖不再落到像素，
回落到框架默认与系统强调色 `#0078D4`）。这就是门面只改刷对象颜色、
从不往字典里塞键的原因，测试也不许塞。

## 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行全绿（35/35，0 skip），调色板漂移 checked=True。
- **行为**：`AstraScrollHostTests.The_implicit_style_reaches_the_scroll_host`
  （`IsTabStop=False`、`Padding=0`、`BorderThickness=0`、`Background=Transparent`、
  并钉住"故意不落地"的 `VerticalScrollBarVisibility=Auto`）；
  `The_framework_owns_the_scroll_bar_thumb_style`（框架自持 ThumbStyle + 拇指底色是调色板钩子）。
- **视觉**：`The_resting_scroll_bar_still_paints_through_the_scroll_host_style`
  （宿主样式给了可命中测试的透明背景后，静止箭头仍在）；
  `An_opaque_scroll_host_background_covers_its_own_bar`（8800 全哨兵、灰色 0）；
  `Overlay_scroll_bars_take_the_resting_arrows_away`、`Auto_hide_does_not_change_the_resting_picture`。
- **硬件输入**：**未做**。滚动条的悬停/拖拽/滚轮与 `PanningMode` 触摸路径都要真的指针输入，
  归 Button 批的输入证据一起做；这一项只证明了样式与开关的有效值。

## Known Gaps（不许用相邻证据替代）

1. 宿主没有可替换的视觉树：上游模板结构（含 `ScrollContentPresenter` 与分隔条）搬不过来。
2. `ZoomMode`、rail、`UseSystemFocusVisuals`、`Horizontal/VerticalScrollMode` 四类上游 setter
   在本运行时**没有对应属性**，属于 API 形状缺口，1.0 的 CLR API 清单必须逐条列出。
3. 静止滚动条与 WinUI 不一致（灰色箭头常驻，主题无关）；`IsOverlayScrollBarEnabled`
   是唯一已知杠杆，缺悬停证据所以未启用。
4. 上游的 `Visible` + indicator 状态组合不可复现，我们保留 `Auto`。
5. 分隔条/`ScrollViewerScrollBarMargin` 两条键无消费点，未声明。
6. 宿主背景与滚动条的层序关系只测了现象（不透明背景吃掉滚动条），机制未证。
7. 触摸惯性/`PanningMode` 与真实滚轮行为未测。
