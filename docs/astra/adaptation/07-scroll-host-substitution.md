# ScrollBar / ScrollViewer：26.10.9 实际暴露的面（底座批开工前）

写这份是因为一个会反复咬人的差异：**同级的 `../Jalium.UI` 源码树比 NuGet 运行时新**。
按 AGENTS.md，运行时权威是 NuGet **26.10.9**；从源码树读到的成员名不能直接当事实用。

## 对照：源码树有、26.10.9 没有

| 键 / 成员 | 26.10.9 里在不在 | 怎么量的 |
|---|---|---|
| `ScrollBarStyle` | **在** | 在 `Jalium.UI.Managed.dll` 元数据串里命中 1 次 |
| `ScrollBarTrack` / `ScrollBarThumb` / `ScrollBarArrow` | **在** | 同上，各 1 次 |
| `ScrollBarLineButtonStyle` | **不在** | 同上，0 次 |
| `ScrollBarPageButtonStyle` | **不在** | 同上，0 次 |
| `ScrollBarThumbStyle` | **不在** | 同上，0 次 |
| `ScrollViewerScrollBarlessTemplate` | **不在** | 同上，0 次 |
| `IsOverlayScrollBarEnabled` | **在** | 同上，1 次 |

所以"给 ScrollBar 塞四个子样式键"这条路在 26.10.9 上不存在，只能按下面这份公开面做。

## 26.10.9 的公开面（反射自运行时实际加载的装配）

- `Controls.Primitives.ScrollBar : RangeBase : Control` —
  意味着它有 `Control.Template`、`Background`、`Foreground`，**隐式样式这条路和 Button 一样可用**
  （`06-pixel-attribution.md` 已证 Button 那条通）。
  自有成员：`Orientation`、`ThumbStyle`（`Style` 类型 DP）、`IsThumbSlim`、`Track Track { get; }`、
  `Scroll` 路由事件、一整套 `ScrollTo*` RoutedCommand。
  **没有** `ExtentSize`/`ViewportSize`（那两个在 `ScrollViewer` 一侧，且名字不同）。
- `Controls.ScrollViewer : ContentControl` —
  `Horizontal/VerticalScrollBarVisibility`、`CanContentScroll`、`IsScrollInertiaEnabled`、
  `ScrollInertiaDurationMs`、`IsDeferredScrollingEnabled`、`IsScrollBarAutoHideEnabled`、
  `IsOverlayScrollBarEnabled`、`ScrollableWidth/Height`、`Computed{H,V}ScrollBarVisibility`、
  附加属性 `ScrollViewer.CanContentScroll/HorizontalScrollBarVisibility/IsDeferredScrollingEnabled`。
- `Controls.Primitives.Thumb : Control` — 只有 `OnApplyTemplate()` 出现在声明面上（其余在基类）。

## 这一批的做法与已知风险

1. ScrollBar 走**隐式样式 + `ControlTemplate`**，与上游 `ScrollBar_themeresources.xaml@19e3bdc`
   的键名逐字对齐（`ScrollBarBackground*`、`ScrollBarThumbFill*`、`ScrollBarTrackFill*`、
   `ScrollBarTrackStroke*`、`ScrollBarPanningThumbBackground*` 等 26 个刷键 + `ScrollBarSize`、
   `ScrollBarExpandDuration`、`ScrollBarContractDelay` 等时长/尺寸键）。
3. 上游状态组（`CommonStates`、`ConsciousStates`、`ScrollingIndicatorStates`）在 Jalium 里
   只能映射成 `ControlTemplate.Triggers`——标记内 VSM 会抛异常（`00` S0-d）。
   `ScrollingIndicatorStates` 需要 `Touch`/`Pen` 指示来源，Jalium 侧没有对应输入信号 → 预期进 Known Gaps。
3. 绘制事实要防：census 记 `ScrollBar`/`ScrollViewer` 为 `style=False render=True`
   （`01-census-raw-output.txt:133,134,148`），即它们**自己画**；
   `02-render-ceiling.md` 还记着 `s_defaultTrackBrush = new(ThemeColors.ScrollBarTrack)` 这类冻结默认。
   给上隐式样式能不能真的接管，**必须像素断言**，不能靠"样式声明了"。
   `06` 也证过 `Hosted_surfaces_show_no_brand_emerald` 里 Slider/ProgressBar 已是 0 px 品牌绿，
   但那是"没有品牌绿"，不等于"我们的样式在画"。
4. 上游 4 个"源码树有、运行时没有"的键**不要**做进样式表：写了就是静默无效键，
   闸口"每个被引用的键都有声明"会直接红。
