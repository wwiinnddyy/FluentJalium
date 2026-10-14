# 弹层圆角与内容溢出批（2026-09-20）

用户第二句抱怨："这种下拉框的控件，或者说 flyout 这类型的……有的这种控件，它的圆角好像都不对。"
上一批结掉了左右边距（S0-u），这一批结圆角。结论分两半：**基座不裁剪子元素**（一半的"圆角不对"跟样式无关），
以及顺着这条基座量出来的一个真缺陷——**建议列表被框架的 accent 绿渐变铺满**。

## 1 · 上游审计

| 表面 | 上游半径 | 上游让开圆弧的方式 |
| --- | --- | --- |
| `MenuFlyoutPresenter` | `OverlayCornerRadius`=8 | 条目 `MenuFlyoutItemMargin` `4,2,4,2`，行自身 r=4 |
| `ComboBox` 下拉 | `OverlayCornerRadius`=8 | `ComboBoxDropdownBorderPadding` 0 + `ComboBoxDropdownContentMargin` `0,4` + 行 `Margin=5,2,5,2`（`ComboBox_themeresources.xaml:614`） |
| `AutoSuggestBox` 建议列表 | `OverlayCornerRadius`=8 | `SuggestionsContainer` `Padding={AutoSuggestListMargin}`，内层 `ListView Margin={AutoSuggestListPadding}` |
| `Expander` | `ControlCornerRadius`=4 | 头部/内容接缝用 `Top/BottomCornerRadiusFilterConverter` 磨平 |
| `ToolTip` | `ControlCornerRadius`=4 | 纯文本，无方形内容 |

上游文件与提交：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。
本库这几个表面的半径 token 与上游逐字相同（读回见 §3），**所以缺陷不在"抄错了半径"这一侧**。

## 2 · 基座：圆角表面不裁剪子元素

`spike/RightGapProbe --open corner`（原始输出 `docs/astra/adaptation/s0w-corner-probe-raw.txt`）。
三张已知颜色的图，把"半径没生效 / 填充圆了但内容盖住 / 采样器本身"三种解释分开：

| 图 | (0,0) (1,1) (2,2) | (4,4) | (30,30) |
| --- | --- | --- | --- |
| 白表面 r=12，无子元素 | 蓝 蓝 蓝 | 白 | 白 |
| 白表面 r=12，塞一个 60x60 红子 Border | **红 红 红** | 红 | 红 |
| 白表面 r=0，塞红子元素（对照） | 红 红 红 | 红 | 红 |

填充自己是圆的（第一行），子元素会盖掉圆弧（第二行），对照组证明不是采样器的错（第三行）。
`Clip` 读回为空。**样式里写任何半径都赢不过一个铺到角的内容。**
闸口用例 `A_rounded_border_rounds_its_fill_but_never_clips_its_child` 在 harness 里读到同样六个像素。

## 3 · 逐个表面的几何复核（属性读回，`--open flyout` 全树）

* `ComboBox`：`PART_PopupBorder` r=8,8,8,8；行 `Grid Margin=5,2,5,2` + `ItemsPresenter` 0,4 → 行离左 5、
  离顶 6 DIP。r=8 的弧在 x=5 处只吃掉 y≈1.1 DIP → **行在弧内，安全**。
* `MenuFlyout`：读回 `LayoutRoot 210.5x34 margin=4,2,4,2 padding=11,8,11,9 radius=4,4,4,4`，
  表面 r=8 + presenter padding 0,2 → 行离边 4、离顶 4，同样在弧内 → **安全**（与 S0-u·4 的同一次量）。
* `Expander`：`PART_HeaderBorder` r=4 + `IsExpanded=True` 格子改成 `4,4,0,0`，
  `PART_ContentBorder` `0,0,4,4` → 接缝两侧都是方的，**安全**（S0-m 落的那格）。
* `AutoCompleteBox`：`SuggestionsContainer` r=8、padding `0,2,0,2`、内层 `Margin=-1,0,-1,0` →
  行**左右没有让开**，正落在弧上。这一处是唯一"内容会盖住圆角"的表面。
* 顺带排除一条：`ComboBox` 的 `PART_ToggleButton` 读回 `radius=10,10,10,10`，看着像错值，但它的模板是
  `Grid + Path`，没有任何填充或描边跟着这个半径 → 惰属性，不是缺陷。

## 4 · 四类证据

* **构建**：`dotnet build` 0 警告 0 错误（新增 3 个用例文件编译进去）。串行闸口见 §6。
* **行为 / 属性**：修前 `SuggestionsContainer` 260x41.78、行 `ComboBoxItem` 260x35.78（**与表面同宽**）、
  `Background` 本地值 `LinearGradientBrush`、`CornerRadius=3`。
  修后同一处：行宽高不变、本地值仍是 `LinearGradientBrush`（属性还归框架），
  四个角仍读回未上色（圆弧没被内容盖住，因为行离边 5 DIP 是模板里 `Margin=5,2,5,2` 给的）。
* **视觉**：`PixelHarness.Chrome(SuggestionsContainer)` + 新加的 `PixelHarness.PixelAt` 单点读回。
  * 修前：`#F9F9F9x2020 #1D733Cx450 #2B804Ax450 #1E743Dx420 #2A7F49x420 #1F753Ex390 #297E48x390 …`，
    中点 (130,20) = `#247A43`——一条 accent 绿对角渐变铺在 Fluent 弹层里。
  * 修后：`#F9F9F9x10012 #DDDDDDx302 #F6F6F6x254 #000000x42 …`，中点 (130,20) = `#F9F9F9`，
    绿通道占优（G > R+16 且 G > B+16）的像素 **0 个**，表面填充占裁剪区 10 012/10 862。
* **硬件输入**：**本批为零**。全部驱动仍是进程内（`Text` setter、`Chrome` 裁剪）。没有一条真指针路径。

## 5 · 修复

`src/FluentJalium/Styles/Selection.jalxaml`，`DefaultComboBoxItemStyle` 模板里 `LayoutRoot` 的
`Background`：`{TemplateBinding Background}` → `{ThemeResource ComboBoxItemBackground}`。
九个状态格子本来就在往 `LayoutRoot` 写 token，所以 `ComboBox` 自己的行一格没变（`AstraComboBoxTests`
全绿）。代价写在模板注释里：**消费者给 `ComboBoxItem.Background` 设的本地值不再进像素**——上游会进。
这条正好是路线图阶段 2 起手必修那句"模板根 Border 不吃本地 Background"的同一类，只是发生在条目容器上。

为什么不能用别的办法：本地值排在 setter / trigger 之上，样式与触发器都改不动；
`SuggestionsContainer.Resources` 里放隐式 `ComboBoxItem` 样式同样被本地值压过；
把行内缩改成非零只能救圆角、救不了颜色。**能救的只有"模板不去读那个属性"。**

## 6 · Known Gaps

1. `AutoSuggestBox` 建议行的**选中**语义没查：修前那条绿渐变是框架给"选中/命中"写的哪一种状态，
   没量过（只量到 resting + 过滤后自动选中的第一行）。修后选中行是 Fluent 灰（`ComboBoxItemBackgroundSelected`
   一族），这与上游 `ListViewItemBackgroundSelected`（`ListLowColor`，也是灰）一致，但没有逐状态对照。
2. 建议列表左右没有内缩（§3 最后一条），本批**没有改**：行的填充现在是透明的 token 底色，
   所以盖不住圆角了；但一旦某格写上不透明底色，缺陷会回来。真正的解是运行时给 `Border` 加裁剪，或阶段 5
   做 `ListBoxItem` 时给行带 `Margin` + r=4。
3. `MenuBar` 的下拉表面归框架（S0-u·4），它的半径本批没测——测不了：那个弹层开不上屏也走不到我们的样式。
4. 圆角只量了 Light 主题、96dpi 逻辑像素下的 `RenderTargetBitmap` 重画；**没有一条上屏帧证据**，
   混合 DPI / 减动效 / 触摸下未测。
5. 类型清单（S0-v）只是"有没有这个类型"，不等于"这个类型可模板化 / 部件树契约是什么"。
   `ContentDialog` 的九步出口还没做，那张表只把"必须起自有类型"这个前提改掉了。

## 7 · 不声称

不声称"所有弹层圆角已核对完毕"：`ComboBox` / `MenuFlyout` / `Expander` 三处有几何读数，
`ToolTip` 只有 token 对照，`MenuBar` 与 `ContentDialog` 没测。
不声称建议行的选中状态与上游等价：只证明了品牌绿不再出现在像素里。
不声称任何 hover / press / 触摸状态：本批硬件输入为零。
不声称上屏帧：§4 的视觉全部走 `RenderTargetBitmap`，它按属性重画，看不见滞后的那一帧。
