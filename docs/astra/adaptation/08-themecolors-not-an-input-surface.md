# ThemeColors 不是输入面（一次测清，别再回锅）

起因：ScrollBar 批发现"改一个叫 `ScrollBarThumb` 的资源就把框架自绘的拇指刷成哨兵色"，
而运行时元数据里 `ThemeColors.ScrollBarThumb`、`ThemeColors.SliderThumb` 这类属性名跟钩子名一模一样。
如果 `ThemeColors.X` 就是"读名叫 X 的资源"，那它就是整个控件批的通用钩子表。
**答案是：不是。** 原始读数在同目录 `08-themecolors-raw.txt`（`spike/PixelAttribution` 的
`themecolors` / `themevalues` / `colours` 三个模式，Light、Astra 门面已装载）。

## 量到的三件事

1. **`Jalium.UI.Controls.Themes.ThemeColors` 是 public、71 个 `Color` 属性，全都没有 setter**
   （反射连非公开 setter 一起看：`set=no`）。想写它没有入口，"文档里说能设"不成立。
2. **71 个名字里有 54 个在 `Application.Resources` 里能找到同名笔刷**——但那是
   **Jalium 自带主题字典的名字碰撞**，不是输入通路：往应用级资源装同名哨兵刷之后，
   `ThemeColors.SliderThumb` 仍然报 `#FF207245`，`ToggleCheckedBackground`/`CheckMark`/`SelectionBackground`
   一字未变，Slider / CheckBox / ToggleButton 的像素也一字未变。
3. **`Application` 上没有任何 `Accent*` / `*Color*` 属性**（反射枚举为空），
   所以"A3 强调色三态"推不到框架自绘的部件上，这条得记成硬边界而不是待办。

## 由此得到的判别法（后面每个控件先问这个）

框架画一个部件时读的是哪一层，决定我们能不能驱动它：

| 绘制读取的层 | 能不能驱动 | 证据 |
|---|---|---|
| 应用级合并资源（`TryFindResource`） | **能**，且我们赢（后装载者胜） | `ScrollBarThumb`/`ScrollBarTrack` 刷成哨兵即落到拇指与轨道像素 |
| `ThemeColors.X`（Jalium 自己的字典） | **不能** | 同名哨兵刷不改 `ThemeColors` 值，也不改 Slider/Toggle 像素 |
| 系统强调色 | 由框架自己跟随，我们插不进 | 未样式的 Slider/CheckBox/ToggleButton 画 `#0078D4`（Windows 默认强调），**不是**品牌绿 |

顺带把 `02`/`05` 里"未样式 Slider 画品牌绿"的旧读数钉死：现在整条 Slider 裁剪里
`#000078D4 x346`，没有 `#207245`。品牌绿只出现在 `ThemeColors` 的**报表值**里，
不在像素里——这也是为什么"看 ThemeColors 判断外观"会骗人。

## 对本批的实际影响

- ScrollBar 的可达面（见 `audits/scrollbar.md`）不变：两条命名刷。
- Slider / ToggleButton / CheckBox 的强调色**已经**跟系统强调色，与 WinUI 默认行为同侧；
  要让它们跟应用指定的强调色走，只能等这些控件自己进样式表（D3/D4 批），
  或在审计里写"不可推入框架自绘部件"。
- ROADMAP D 的渲染分级因此有了可执行的问法：**先试同名资源刷，再看像素动不动**，
  一步就能把控件分成 `resource-driven` / `themecolors-frozen` / `system-accent` 三类，
  不需要每个控件都重跑一遍这次的探索。
