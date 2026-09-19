# ContentDialog 审计（阶段 4 第四段）

上游：`microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- `controls/dev/CommonStyles/ContentDialog_themeresources.xaml`，blob `9cd8150fff8c216319e2d5515b538cfab5b397d5`（267 行：Light/Default 行 5-19、High Contrast 22-36、Dark 39-53、样式与模板 55 起、六组 VisualState 123-219、模板 220-250）
- 模板关键线：`222` `SmokeLayerBackground` Rectangle、`223` `BackgroundElement` Border、`227` `DialogSpace`、`232` `ContentScrollViewer`、`238` `Title` ContentControl（`FontSize="20" FontWeight="SemiBold"`）、`241` 嵌套 `ContentPresenter MaxLines="2"`、`CommandSpace` 五列 `*,0,0,8,*`。
- 对照：`dxaml/test/resources/res/native/controls/ContentDialog/ContentDialogTemplate.xaml` 与 `ContentDialogWithSmokeBackgroundPart.xaml`（上游自己的部件命名测试，佐证 `BackgroundElement` / `SmokeBackground` 是上游的名字，不是本运行时的名字）。

运行时：NuGet Jalium.UI **26.10.9**。测量：`spike/RightGapProbe`（`dialog` pass 反射成员与框架自建的树；`shown` pass 真开一次对话框，原始日志 `right-gap-shown.txt`）+ `tests/FluentJalium.Tests/AstraContentDialogTests.cs`（38 条）。

## 0. 先量后写：`shown` pass 量到的事实

1. **计划里的「自有类型」作废**：`Jalium.UI.Controls.ContentDialog` 原生存在，成员面完整（`Title`/`TitleTemplate`、三组 `*ButtonText`/`*ButtonStyle`/`*ButtonCommand`、`IsPrimary|SecondaryButtonEnabled`、`DefaultButton`、`FullSizeDesired`、`ShowAsync()`/`ShowAsync(ContentDialogPlacement)`、`Hide()`、`Opened`/`Closing`/`Closed` + 三个 `*ButtonClick`）。按 AGENTS.md 走原生重模板。
2. **只是「放进树里」的对话框不是对话框**：挂载但没打开的实例是 `Visibility=Collapsed`、`ActualWidth×Height=0×0`。本批第一版全部断言读的就是这棵 0×0 的树——部件名与样式的值读得到，几何、像素、点击全读不到（`painted=0`、`titleStyle=null`、`clicks=0` 都是这个原因）。**判据**：对话框类控件必须真开一次再量。
3. **打开的通路**：`ShowAsync()` 把控件搬进窗口的 `ContentDialogOverlayHost`；已经挂在树里的实例调用它会同步抛 `InvalidOperationException("Popup-hosted ContentDialog must not already be attached to the visual tree.")`。所以 Gallery 页面上没有 `<ContentDialog>` 元素，点按钮时才 new。
4. **卡片宽度是控件的**：控件把自身的 Min/Max 盒映射到名为 `PART_DialogCard` 的元素上，并用「宿主宽 − 48」封顶（实测：宿主 886.3 → `card.MaxWidth` 本地值 838.2857…；应用写 `MaxWidth=548` → 本地值就是 548；`MinWidth=320` 同理）。本地值压过所有格子，所以 `MaxWidth="548"` 字面量放在 `PART_DialogCard` 上必然失效。本实现把外层留给 `PART_DialogCard`（只带 24 DIP 外边距、不画任何东西），内层 `DialogSurface` 承载 320/548/184/756 与全部填充行——控件按名字找得到外层，就写不到内层的上限。
5. **格子能写什么（八种文本组合全表实测）**：
   - 能写：具名元素上的 `Visibility`、`Grid.Column`、`VerticalAlignment`/`HorizontalAlignment`；**不带 `TargetName` 的格子能写模板父级自身的属性**（这是默认按钮强调色的通路，见 0.6）。
   - 不能写：**具名 `ColumnDefinition` 的 `Width`**。两种取值形状都试过——`Value="0"`（读回 `Double(0)`）与 `Value="0*"`（真 `GridLength`）——五列在八种组合下始终读回 `*,8,*,8,*`。所以模板里一条列宽格子都不写。
   - 不能写：`Style`。三个按钮的 `Style` 是 `TemplateBinding` 写出的本地值（`local=set` 前后都是），格子压不过本地值。
6. **默认按钮的强调色换个写法就通了**：格子上不写 `PART_PrimaryButton.Style`，改写 `ContentDialog.PrimaryButtonStyle`（无 `TargetName`），绑定再把它带到按钮上。实测：`buttonStyleChanged=True`、`dialog.PrimaryButtonStyle` 就是格子里那个 `AccentButtonStyle` 实例、`DefaultButton=None` 后 `backToRest=True`。
7. **`UniformGrid` 不是替代方案**：三按钮 `35.9/34.8/35.4`，折叠中间那个之后仍是 `35.9/35.4`——它不把折叠子元素让出的格子收回来自家子元素，所以两按钮场景不会变成上游的两半。
8. **标题字号被生成元素吃掉**：在 `PART_TitleHost`（`ContentControl`）上写 `FontSize="20" FontWeight="SemiBold"`，生成的文本元素读回 `weight=SemiBold` 但 `size=14`，`style=null`——权重继承得下来、字号被框架写死在生成的文本元素上（与 NumberBox 批「生成的文本元素带本地 `Foreground`」同一类）。`ContentPresenter.Resources` 里的隐式 `TextBlock` 样式在这里也读不到（`style=null`），与 `ComboBoxItem` 那条 NoWrap 的作用域不同。可行的杠杆是把 20/SemiBold 放进 **`TitleTemplate` 的默认模板**（自建元素带自建本地值），实测 `size=20 weight=SemiBold localSize=20`。
9. **部件名这次是真契约**：`PART_Root`、`PART_Overlay`、`PART_DialogCard`、`PART_TitleHost`、`PART_ContentScrollViewer`、`PART_ButtonPanel`、`PART_Primary|Secondary|CloseButton` 全部取自框架自建的那棵树。行为读回：调用我们模板里 `PART_PrimaryButton` 的 Invoke，`PrimaryButtonClick`、`Closed` 各一次，`ShowAsync` 以 `Primary` 完成，`Visibility` 回到 `Collapsed`——四件事同时成立，说明模板没切断控件自己的连线。禁用那个按钮后再 Invoke，事件一次也不发（peer 抛 `InvalidOperationException` 与静默两种都被这条断言容下，只锁「无点击」）。

## 1. 行去向表（上游 15 条 → 本实现 6 别名 + 4 Thickness，5 条不发布）

| 上游组 | 条数 | 落到 | 未落地的原因 |
| --- | --- | --- | --- |
| 六个画刷别名（Foreground/Background/SmokeFill/TopOverlay/BorderBrush/SeparatorBorderBrush） | 6 | `ThemeResources/ContentDialog.jalxaml` 同名别名，逐字指向上游目标行 | — |
| 四条 Thickness（BorderWidth/TitleMargin/Padding/SeparatorThickness） | 4 | 同名发布，值与上游一致（`1`/`0,0,0,12`/`24`/`0,0,0,1`） | — |
| MinWidth / MaxWidth / MinHeight / MaxHeight（`x:Double`） | 4 | **不发布**，值以字面量落在 `DialogSurface` | 本读取器解析不了 `x:Double` 行（adaptation/00）；且控件会用自己的本地值盖掉卡片上限（0.4），发布名字等于承诺一个拉不住的手柄 |
| ButtonSpacing（`GridLength`） | 1 | **不发布**，8 DIP 字面量在列定义上 | `GridLength` 行同样解析不了 |

High Contrast 分支（上游 22-36）不转录：本运行时无公开高对比入口（`ThemeVariant` 只有 `{Dark, Light}`），高对比是我们门面里的逐键重映射，不声称上游对齐。

## 2. 宿主替换清单

| 上游 | 本实现 | 原因 |
| --- | --- | --- |
| 一层 `BackgroundElement` 同时是卡片表面与尺寸盒 | 两层：`PART_DialogCard`（控件的尺寸盒槽位）+ `DialogSurface`（表面与上限） | 0.4：控件按名字写本地 `MaxWidth`，字面量上限放不住 |
| `Title` ContentControl 上 `FontSize=20`/`FontWeight=SemiBold` | 样式级 `TitleTemplate` 里自建 `TextBlock` 带这两个本地值 | 0.8：生成的文本元素丢掉继承来的字号 |
| `Title` 嵌套模板里的 `ContentPresenter MaxLines="2"` | 未落地 | 未量过本运行时 `TextBlock.MaxLines`，不写证不到的格子（Known Gaps 3） |
| VSM 改 `PART_PrimaryButton.Style` | 格子改 `ContentDialog.PrimaryButtonStyle` | 0.5：按钮 `Style` 是 `TemplateBinding` 本地值 |
| VSM 改列宽实现两半/三等分 | 五列固定，移动按钮的 `Grid.Column` | 0.5 + 0.7：列宽格子无效，UniformGrid 不回收 |
| `CommandSpace` 基态 `*,0,0,8,*`（两按钮为基态）+ `AllVisible` 展开成三等分 | 基态即 `*,8,*,8,*`（三按钮），退化组合靠格子写 | 「文本非空」无法作为触发条件表达，只能反过来在 null 上写 |
| 控件自绘烟幕 + 模板 `SmokeLayerBackground` | 模板 `PART_Overlay` 一层，宿主 `ContentDialogOverlayHost` 无 `Background` 属性可读 | 0.3：宿主的画法只能算像素，未算（Known Gaps 5） |

## 3. 状态映射（六组 VisualState → 十格）

| 上游组 / 状态 | 本实现的格子 | 读回 |
| --- | --- | --- |
| `DialogSizingStates/FullDialogSizing` | `FullSizeDesired=True` → 卡片与表面 `VerticalAlignment=Stretch`（+水平） | `A_shown…` 之外的 `FullSizeDesired_stretches_the_card…`：高度确实长大 |
| `DefaultButtonStates/Primary|Secondary|Close` | 三格写父级 `*ButtonStyle` | `The_default_button_takes_the_accent_style_and_gives_it_back` |
| `ButtonsVisibilityStates/NoneVisible` | 三文本全 null → `PART_ButtonArea` 折叠 | 组合表 `null,null,null → ""` |
| `AllVisible` / `PrimaryAndCloseVisible` / `SecondaryAndCloseVisible` / `PrimaryAndSecondaryVisible` | 基态 + 三格单 null | 组合表 `P/S/C→0:2:4`、`-/S/C→2:4`、`P/-/C→2:4`、`P/S/-→0:2` |
| `PrimaryVisible` / `SecondaryVisible` / `CloseVisible` | 两格双 null 把幸存者移到第 4 列 | 组合表 `P/-/-→4`、`-/S/-→4`、`-/-/C→4` |
| `DialogBorderStates/AccentColorBorder` | 不落地 | 上游该状态只在 Toast/权限路径出现，本运行时无对应入口；未量即不写 |

## 4. 四类证据

- **构建**：`tools/Test-AstraGates.ps1` 串行 restore→build→test→调色板漂移，本批收口时 **666/666 全绿、0 skip、0 错误**，三档 checked=True；两份新字典（`ThemeResources/ContentDialog.jalxaml`、`Styles/ContentDialog.jalxaml`）进 `Themes/Manifest.txt`。首跑曾被一条**闸口自身的缺陷**判红：`The_gallery_project_carries_the_catalog_it_reads` 比对 `bin/**` 下所有配置的 catalog 副本，于是别的配置留下的旧副本能让一次 Debug 闸口失败——已改成只比对本次构建配置（由测试程序集路径推出）并要求该副本必须存在。
- **行为**：`AstraContentDialogTests` 38 条——部件树与类型、八种文本组合的列位置与等宽、按钮折叠、区域折叠、默认按钮换/还、`FullSizeDesired`、启用位下传、`ShowAsync`→点击→`Closed`→结果四件套、已挂载实例拒开、禁用按钮不产点击、十格清单、卡片四角。
- **视觉**：哨兵像素两条——`ContentDialogBackground` 调色行换成品红后卡片确实画出品红（>5 000 px，且品牌绿 0 px）；Light 与 Dark 各开一张卡，`Top(3)` 直方图不同且两张都画满。几何另有一条像素级：`DialogSurface` 圆角 8、卡片不裁剪子元素（沿用 `corner-radius.md` 的基座结论）。
  这一条在本批真的抓到一个缺陷，与前一批「圆角不对」同源：`TitleStrip` 和 `PART_ButtonArea` 都是填到卡片边缘的方形 `Border`，于是把 r=8 的弧覆盖成方角。Light、`DialogSurface` 328.8×190、`fill=#F3F3F3`、`strip=#FFFFFF`，逐像素读左上 8×8（行=y、列=x）：

  | | 修复前（strip 无半径） | 修复后（`8,8,0,0`） |
  | --- | --- | --- |
  | (1,1) | `#FFFFFF` ← strip 自己的刷 | `#000000` ← 弧外的烟幕 |
  | (2,1) | `#FFFFFF` | `#000000` |
  | (1,2) | `#FFFFFF` | `#000000` |
  | (3,3) 起的斜坡 | 已被抹平 | `#929292 #D2D2D2 #FCFCFC #FFFFFF` 一路入白 |

  左下 8×8 是同一缺陷的第二半，且更隐蔽：`PART_ButtonArea` 画的是卡片自己的刷色，所以在表面上看不出差别，只有弧外那格会露出方形「耳」。修复前 (1,h-2) 与 (2,h-2) 读 `#F3F3F3`（等于 `ContentDialogBackground`），加 `CornerRadius="0,0,8,8"` 后同一区读回完整斜坡：

  ```
  #000000 #000000 #A0A0A0 #CFCFCF #F0F0F0 #F3F3F3 #F3F3F3 #F3F3F3   (y=h-2)
  #000000 #000000 #000000 #8B8B8B #B4B4B4 #DADADA #E8E8E8 #F0F0F0   (y=h-1)
  ```

  断言在 `The_two_inner_surfaces_leave_the_cards_round_corners_alone`：它钉住三层半径（表面 8、strip `8,8,0,0`、
  命令行 `0,0,8,8`）与六个弧外点——左上、右上、左下、右下各一进，加左列的两格。右上/右下与左侧同结论。
  一条判读上的坑要写下来：`PART_ButtonArea` 与卡片同色，所以**它旁边那格 (2,h-2) 在修复前后都落在抗锯齿斜坡上
  （#D0D0D0 → #A0A0A0），本身不构成判据**；真正判别的是 (1,h-2)。凡是内表面与外表面同色的圆角检查，都要按这条
  挑点。
- **硬件输入**：**0 条**。三个按钮的悬停/按下/键盘/触摸沿用 Button 批证据；本批只有自动化 peer 的 Invoke，不算指针。
- **宿主冒烟**：`tools/Test-AstraGallerySmoke.ps1 -Page surfaces,menus,inputs,overview` 四页各自上屏、优雅退出、无残留进程（脚本故意不截屏）。这条只声称 Surfaces 页带着新卡片不崩——卡片本身在折叠线以下，未目视，也没有点过它上面的按钮。

## 5. Known Gaps

1. **两按钮不是上游的两半**：本实现是「两列 1/3 + 一端空 1/3」，单按钮是 1/3 而非右侧半宽。原因见 0.5/0.7（列宽格子无效、UniformGrid 不回收）。可选解法两条都还没做：给列宽接一个 `IValueConverter`，或按 AGENTS.md 起一个自有面板类型——需要先证明这条差异值得多一个类型。
2. **卡片宽度上限不是资源行**：`ContentDialogMaxWidth` 未发布，应用只能通过 `ContentDialog.MaxWidth` 影响控件写出的那个本地值；`DialogSurface` 的 548 是模板字面量，改它要改模板。
3. **标题两行封顶（`MaxLines=2`）未落地**：本运行时 `TextBlock.MaxLines` 是否存在未量，长标题目前会一直折行。
4. **开合动画为 0**：上游 `ContentDialogShowTimer`/`ContentDialogHideTimer` 的淡入与 0.93 缩放在 markup VSM 无效的前提下没有替代实现；`Opened`/`Closed` 事件里有像素读回的稳定性损失（`Chrome` 对含文本的弹窗裁剪并不总是收敛，本批因此把稳定性放在哨兵那条、把明暗差异只按调色与绘制面积声称）。
5. **烟幕是否被画两次未结**：`ContentDialogOverlayHost` 不暴露 `Background` 属性，属性面读不到第二层暗底；是否真有一层需要上屏帧才能判定——正是用户报的「重影」这一类，暂不声称已排除。
6. **硬件输入为零**（见第 4 节）。
7. **触摸路径**：`ShowAsync(ContentDialogPlacement)` 的第二种放置、以及 `IsPrimary|SecondaryButtonEnabled` 的触摸等价，都未测。
8. **两张内表面的半径是字面量**：`TitleStrip` 的 `8,8,0,0` 与 `PART_ButtonArea` 的 `0,0,8,8` 没有跟 `CornerRadius`
   绑定（那需要 `TemplateBinding` 到一个 `{ThemeResource OverlayCornerRadius}` 上再逐角拆，本运行时未量过这种拆法）。
   应用若把 `ContentDialog.CornerRadius` 改成别的值，`DialogSurface` 会跟着变而两张内表面不变——上游靠裁剪没有这个问题。

## 6. 不声称

- 不声称对话框外观与 WinUI 1:1：两按钮与单按钮的行几何已知不同（5.1）。
- 不声称开合过渡、焦点循环（上游 `TabFocusNavigation=Cycle` 未转录）与模态行为等价。
- 不声称三个按钮的悬停/按下/键盘/触摸像素——那是 Button 批的证据，本批只是复用其样式。
- 不声称烟幕只有一层（5.5），也不声称卡片圆角裁剪了内容（基座事实：`Border` 不裁剪子元素）。
- 不声称四角在所有形状下都成立：上面那六个弧外点是在 **Light 档、默认三按钮、非 `FullSizeDesired`** 的一张卡上读的；
  Dark 档、`FullSizeDesired` 拉伸后、以及内容高到触到 `MaxHeight=756` 出滚动条时都未逐角复测。
- 不声称 `ContentDialogResult` 全流程（`PrimaryButtonClick` 与 `Hide()` 之外的 `Closing` 取消路径、`*ButtonCommand` 通路）已覆盖。
