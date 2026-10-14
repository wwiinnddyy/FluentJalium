# 12 · 侧边栏上屏量尺，以及一次被撤回的动效扩散

日期：2026-09-18。触发：用户指出 Gallery 侧边栏"明显有问题"，并指定
`C:\git\ink\LanStartWrite.inkcanvas` 的设置页侧边栏为"以 ModernWpf 为理念正确移植到 Jalium.UI"的参照。

## 为什么这次必须上屏量

`PixelHarness` 走 `RenderTargetBitmap`，离屏路径不写文字（`audits/button.md` 第 9 条）。
侧边栏恰恰是"图标 + 文字 + 一根指示条"，离屏判据看不见它。所以这一轮的判据换成
**真窗口 + 屏幕区域捕获**，几何用像素扫描量，不靠眼睛估。

## 捕获通路里的第一个坑：DPI 虚拟化

第一版脚本（`Get-Process` 拿 `MainWindowHandle` → `GetWindowRect` → `Graphics.CopyFromScreen`）
产出的图**左缘露出 IDE 内容、右缘把内容卡切断**，看起来像我们的布局溢出。真实原因是调用方进程
不感知 DPI：

```
不感知：win=188,26 1100x820        ← 这是 DIP，被当成物理像素去截
感知后：win=329,46 1925x1435  client=1901x1423  dpi=168   ← 物理像素，175% 缩放
```

`Width="1100"` 是 DIP，175% 下物理宽 1925。按 1100 去截只拿到左上角约 80%，
再叠加 153px 的横向偏移，就截成了"左边是别人的窗口、右边被切"。
**结论：任何屏幕捕获脚本必须先 `SetProcessDpiAwarenessContext(-4)`，并打印 `GetDpiForWindow`。**
本轮之后所有上屏量尺都用这条通路（脚本在 `%TEMP%\shot3.ps1`，逻辑照抄在此文里即可复现）。

第二个坑：这一轮最后一次捕获期间，Windows 安全中心弹出了 `server.exe` 的网络授权对话框，
把 Gallery 罩在一层系统 scrim 下——**强调色像素因此低于阈值，指示条一度"测不到"**。
几何结论不受影响（边界仍清晰），但颜色阈值必须放宽或等对话框消失后重采。
我没有也不会去点那个对话框。

## 量出来的事实（175%，DIP = 物理像素 / 1.75）

| 项 | 改前 | 改后 | 上游/参照值 |
| --- | --- | --- | --- |
| pane 宽 | 240 DIP（x=420 处翻面） | 同 | 我们的 `OpenPaneLength=240` |
| 项行高 | 63px = 36 DIP | 同 | 我们的 `MinHeight=36` |
| 指示条 | x=7..11（2.9 DIP 宽）、y=238..265（16.0 DIP 高），行内垂直居中 | x=7..11、y=175..202（16.0 DIP） | WinUI `SelectionIndicator` 3×16 |
| **第一项顶边** | **y=221 → 126 DIP** | **y=158 → 90 DIP** | WinUI 约 52 DIP（汉堡在标题栏内） |

也就是说：**pane 宽、行高、指示条尺寸和居中全部是对的**，用户看到的"明显有问题"是
**纵向堆了三条带**——原生标题栏（48）+ Gallery 自己那条横跨整窗的
`FluentJalium / Gallery` 头（约 36）+ pane 内的汉堡带（36+12）——第一项被推到 126 DIP，
侧边栏上半截是空的。

## 改了什么

`samples/FluentJalium.Gallery/MainWindow.jalxaml`：删掉窗口根 `Grid` 的那条头部带，
把它移进 NavigationView 的内容列（`Auto/*/Auto` 三行的第 0 行），页面容器顶距
`24` → `24,8,24,24`，Live output 从 `Grid.Row="1"` 移到 `"2"`。
现在 pane 从客户区顶部开始，汉堡带是它的第一条带，与 inkcanvas 的
`SettingsWindow.jalxaml:7-17` 同构；页头留在内容列上方，与 WinUI Gallery 同构。

余下的 90−48=42 DIP 是汉堡带本身。WinUI 把它放进标题栏（`ExtendContentIntoTitleBar`），
我们没有自绘标题栏可用（`audits/window-shell.md` 的 6 条钩子是另一件事），
所以这一档在窗口外壳接上之前不声称能再压。

## 顺带查清的参照事实（inkcanvas，只读）

- 侧边栏不是原生 `NavigationView`，是 `FluentNavigationItem : Button` + 一个
  **跨项共享指示条**（`Canvas` + `IsHitTestVisible=False`），动画器直接改写
  `NavigationView.cpp::PlayIndicatorAnimations` 的两段样条：200ms 朝目的地拉长、400ms 收拢，
  `KeySpline(0.9,0.1,1,0.2)` / `KeySpline(0.1,0.9,0.2,1)`，只动 `Canvas.Top` 与 `Height`。
  我们的 `NavigationIndicatorAnimator` 是同一思路，`FluentNavigationView.cs:306-338` 也已按
  布局坐标（不是序号）取目的地。
- 它同样认定 `x:Double` 资源不可用（`ThemeMetrics.jalxaml:6-8`），尺寸一律字面量；
  同样用 `TransitionProperty/TransitionDuration` 做状态过渡；同样只用 `SymbolIcon + Symbol`。
- 它刻意不用材质：`SystemBackdrop="None"` + 纯色浮层，并写进
  `docs/FLUENT_UI_PARITY.md:47` "not an acrylic/mica/noise simulation"。
- 它的主题切换也是**原地改 `SolidColorBrush.Color`**、靠刷身份存活（`FluentTheme.cs:10`），
  和我们的混合调色板是同一个模型。差别是它全程 `{StaticResource}`，我们 `{ThemeResource}`。

## 一次撤回：把 83ms 过渡扩散到选择/文本控件

我按参照给 `CheckSurface`、`RadioRing`、`PART_MainBorder`、`PART_BackgroundBorder`、
`OuterBorder`、`BottomStroke` 加了 `TransitionProperty="Background, BorderBrush" 0:0:0.083`，
闸口立刻红了，而且红得有价值：

```
Assert.Same() Failure: Values are not the same instance
Expected: SolidColorBrush(#FF0078D4)
Actual:   SolidColorBrush(sc#0.6605243, 0, 0.12252378, 0.429487)
```

**`TransitionProperty` 在过渡期间会把 `Background` 换成一个新的插值画刷实例**，
不是原地改色。这直接顶到 Astra 别名层的核心不变量（覆盖 `ControlFillColorDefaultBrush`
必须处处看到同一个对象，`audits/button.md`）——过渡在飞的 83ms 内该不变量不成立。
而 WinUI 对这些状态填充本来就是 `ObjectAnimationUsingKeyFrames` + `KeyTime="0"` 的
**离散换刷**（`CheckBox_themeresources.xaml` 的 11 个非静态、`RadioButton_themeresources.xaml`
的 5 个态全是 Discrete），根本不带混合。

所以：扩散过渡既偏离权威判据，又破坏我们自己要用实例身份断言的东西。**已全部回退**
（`git checkout` 三个样式字典，工作区现在只剩 Gallery 布局改动）。
Button / NavigationView / ToggleSwitch 上已有的过渡保持原样——它们是既有事实，
本轮不重开这个决定，但要知道：那三处的画刷实例同样只在稳态等于调色板实例。

## 第二轮：折叠态图标右边被遮（用户报）

把窗口压到 700 DIP（<800 阈值）逼出 compact，抬到非置顶窗口最前后截客户区，逐像素分类选中行：

```
改前  pane 48 DIP；高亮框 6.9 … 32.0 DIP（28 宽）；图标 16.6 … 30.9（16 宽，中心 23.75 = pane 中心）
改后  pane 48 DIP；高亮框 6.9 … 44.0 DIP（40 宽）；图标 16.6 … 30.9，完整落在框内
```

图标位置从头到尾是对的（它按 40 DIP 图标列居中，中心正好是 pane 中心 24）。
**错的是高亮框**：它只有 28 宽，右边缘停在 32，于是图标的右半截露在框外，看起来就是"图标右边被遮住"。

根因用一条断言钉死（`AstraNavigationTests.The_bar_mode_decides_whether_the_pane_loses_layout_width`）：
**`ScrollViewer` 的 `VerticalScrollBarVisibility="Auto"` 即使不画滚动条，也要从视口里扣掉 12 DIP**
（48 的视口只给内容 36）。`Hidden` 与 `Disabled` 实测都不扣（48）。
所以展开态是 `240 - 4 - 12 - 4 = 220`（高亮框实测止于 224，含 4 边距，完全吻合），
折叠态剩 `48 - 4 - 12 - 4 = 28`——比它要装的 40 DIP 图标列还窄 12。

修法：`Styles/Navigation.jalxaml` 的 `PART_MenuScrollViewer` 从 `Auto` 改 `Hidden`。
另加一条行为断言 `A_hidden_bar_still_lets_the_pane_scroll`：`ScrollToVerticalOffset(40)` 后
`VerticalOffset == 40`，即扣掉的是**槽**不是**滚动能力**（滚轮/键盘/触摸路径不受影响）。
代价写进目录 gaps：折叠/长列表时没有可拖的滑块，因为本框架没有覆盖式滚动条。

顺带第二条实测（同一个坑的数值版）：`TransitionProperty="Width"` 会把 **`Border.Width` 这个 DP 值本身**
插值——过渡在飞时读回 54.48 而不是 48。所以任何"读回布局/画刷值"的断言都必须先等过渡落地，
`PixelHarness.Settle(frames)` 就是为此加的公开入口（`Capture` 的稳态判据只看像素直方图，
空 pane 的宽度变化不足以让直方图改变，所以它当时没等到）。

## 这一轮不声称

1. 侧边栏的**悬停/按下/键盘焦点**没有上屏证据：本轮只做了几何量尺，且真指针注入在这台机器上
   不可门控（`adaptation/11` 记过），屏幕坐标点击一律不用。
2. **compact 态只量了静息几何**（第二轮已把高亮框与图标的位置钉死，并有 `AstraNavigationTests` 断言）；
   汉堡点击后的**折叠动画**没有逐帧证据，长列表下的"没有可拖滑块"这条代价也未在真窗口里验证。
3. 指示条的**移动动画**（200/400ms 两段）没有上屏逐帧证据，只有 `NavigationIndicatorAnimator`
   的代码路径与离屏断言。
4. 90 DIP 是"当前窗口外壳下能做到的位置"，不是 WinUI 的 52 DIP；余下 42 DIP 归窗口外壳批。
5. NavigationView 的九步出口仍缺 `audits/navigation.md`（上游 blob、键清单、
   VisualState→Trigger 映射表）——本轮是 Gallery 用法与量尺，不是该控件的审计落地。
6. 参照项目里那些"未消费存档键"（`ThemeMetrics.jalxaml:39-41`、`ThemeAnimation.jalxaml`
   全部时长键）说明它自己也有声明未接线的债；不能把它有的一切当成已验证的对。
