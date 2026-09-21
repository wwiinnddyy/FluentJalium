# Divider 审计（阶段 6 第三段）

运行时权威：NuGet Jalium.UI 26.10.9。WinUI 参考树：`../microsoft-ui-xaml` @ `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`。
方法参考：`../ModernWpf` @ `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`。参考树只读，blob 用 `git rev-parse HEAD:<path>` 现取并核对与工作区一致。
原始读数：`docs/astra/adaptation/s1q-divider-raw.txt`（`spike/DividerProbe`，模式 `surface` / `ink` / `alpha` / `mount` / `shape`）。
跨控件结论：`adaptation/00` §S1-q。

这一份与阶段 6 其他审计不一样：**它没有可以照抄的上游控件**。九步出口的第一出口在这里只能以"搜过什么、没搜到什么"的形式交付。

## 1. 权威缺失的记录（先说没有的东西）

| 搜索 | 命令 | 结果 |
| --- | --- | --- |
| 文件名 | `git ls-files \| grep -i divider` | 0 命中（同一命令对 `progressring` 有 125 命中，证明树是完整的） |
| 控件类型 | `git grep -n "\bDivider\b" HEAD -- '*.idl' '*.h'` | 只有 `LayoutsTestHooks.idl:62`（`…PerLineDivider` 测试钩子）、`ResizeGripper.idl:60`、`TableView.idl:451` 三处注释 |
| 发布说明 | 仓库内 changelog / release-notes | 树里没有这类文件，`docs/publishing/*` 只讲发布渠道 |

因此**不能声称**：本层这个外观对齐 WinUI 的 `Divider` 控件；也不能给出"上游从哪个版本开始有它"——参考树里查不到，就不写进文档。
`adaptation/05` §F 早就按 `Type.Name` 全程序集量过 `Divider`：运行时就这个类型也不存在。两头都没有。

## 2. 唯一可用的几何权威：同角色分隔线七处

上游把"一条分隔线"这件事散在别的控件模板里。以下七处全部读到源文件行号，**共同点只有一个：跨轴厚度是 1**（颜色一律来自 `DividerStrokeColorDefaultBrush` 的别名行）。

| 角色 | 文件（blob） | 要素 | 跨轴 | 另一轴 | Margin / Padding | 色键 |
| --- | --- | --- | --- | --- | --- | --- |
| 应用栏分隔 | `CommonStyles/AppBarSeparator_themeresources.xaml`（`51801458b679a563d21471423f4ebb94f0b13ad0`）:14,16,18,49 | `Rectangle` | `Width=1`（`AppBarSeparatorWidth`） | `VerticalAlignment=Stretch` | `2,8,2,8` | `AppBarSeparatorForeground` |
| 菜单分隔 | `CommonStyles/MenuFlyout_themeresources.xaml`（`6f9f3fd322583d2d537dc30c439e62accf1efbd4`）:254,258,733 | `Rectangle` | `Height=1`（`MenuFlyoutSeparatorHeight`） | 宽拉伸 | `-4,1,-4,1` | `MenuFlyoutSeparatorBackground` |
| 分裂项竖线 | 同文件 :262,606 | `Border` | `Width=1` 字面 | `Height=18` 绑定 | 无 | `SplitMenuFlyoutItemButtonDividerBrush` |
| 日期选择器垫条 | `CommonStyles/DatePicker_themeresources.xaml`（`8d19f3001019aa7f14ab05ea9a182d201420294b`）:291-304 | `Rectangle` | `Width=1` | 高拉伸 / 横向换 `Height=1` | 无 | `DatePickerFlyoutPresenterSpacerFill` |
| 时间选择器垫条 | `CommonStyles/TimePicker_themeresources.xaml`（`2cfb8a7df11925a858a9c877c0886c2ccc3231bf`）:294-305 | `Rectangle` | 同上 | 同上 | 无 | `TimePickerFlyoutPresenterSpacerFill` |
| 侧栏条目分隔 | `NavigationView/NavigationView.xaml`（`a21c87b48e29d0dd25beacaa4ae62167947e14fe`）:572-605，模板要素 `Rectangle` :600 | `Rectangle` | `Height=1` | 宽拉伸 | `0,3,0,4` | `NavigationViewItemSeparatorForeground` |
| 标签页分隔 | `TabView/TabView.xaml`（`c0e732a4604036f8cce7fee1aed757a9fc4bacdb`）:553 | `Border` | `Width=1` 字面 | 高拉伸 | `0,8,0,8` | `TabViewItemSeparator` |

两点必须说清：

- **朝向不是一处属性能表达的东西**。七处里没有一处有朝向属性：`NavigationViewItemSeparator` 用 VSM 状态（`:589-597` 的 `VerticalLine` 改 `Height=24`、宽换成 `TopNavigationViewItemSeparatorWidth=1`、margin 换 `3,0,4,0`、色键换掉），`AppBarSeparator` 的 `Overflow` 状态（`:40-45`）从竖翻横，日期/时间选择器直接在模板里放两套要素。本层的宿主自己带 `Orientation`（见 §3），所以不需要抄这套。
- **各处 margin 互不相同**，没有任何一个是"通用分隔线"的，因此本层一个都不抄（§6 第 3 条）。

令牌行本身：`DividerStrokeColorDefault` = Light `#0F000000`（`Common_themeresources_any.xaml`:257，blob `ca07acb962a7c018a174bf28dcd0e945b13ddb4d`）/ Dark `#15FFFFFF`（:53），刷 :143/:347，高对比在 :471 把刷重指向 `SystemColorWindowTextColor`。这三行**本层调色板早就有**（keys.md:59/150/248/339/445），本段一行都不新发。

## 3. 基型：原生 `Separator`，不起自有类型，也不带模板

`spike/ControlCensus` 补测的名字表 + `spike/DividerProbe` 模式 `surface`：

| 类型 | 结果 |
| --- | --- |
| `Divider` | ABSENT |
| `Separator` | `Jalium.UI.Controls.Separator : Control`，自有 DP 三枚 `Orientation` / `StrokeBrush` / `StrokeThickness`，**自己声明 `OnRender`**，出厂 `Style=null`、`Template=null`、`IsHitTestVisible=False` |
| `MenuFlyoutSeparator` / `AppBarSeparator` | 都在，也都自己 `OnRender`（菜单批已量过 `MenuFlyoutSeparator.OnRender` 自绘） |
| `BitmapIcon` / `IconSource` 族 | ABSENT（留给图标段） |

于是 AGENTS.md 的"原生控件优先"这一条**成立**，起自有类型反而违规。更省的一步是模式 `shape` 量到"不套模板也能换色"：出厂的线读 `#A3A3A4`，把 `StrokeBrush` 设成刷、`StrokeThickness` 设成 4，同一位置就印出 4 行、颜色跟着变（`stroke3` 那一格印出红色四行）。所以 `Styles/Divider.jalxaml` 只有两条 setter，没有 `ControlTemplate`——套模板也被量过是通的（模式 `mount` 里 `DividerLine` 部件实例化、印 604），但那是把框架已经会画的东西换成自己画，多一层而没有新事实。

模式 `ink` 另两条读数决定了别用 `Rectangle` 抄上游：1 DIP 的 `Rectangle` 在 top/center/bottom 三种对齐和描边写法下**全部印 0**，同盒子的 `Border` 印 600，`Line` 印 604，`Rectangle` 到 4 DIP 才恢复（4 行）、3 DIP 仍只印 2 行。为什么这样不知道，机制没有证据；后果是明确的（§6 第 4 条 + `adaptation/00` S1-q）。

## 4. 键账：发布 0 行，消费 1 行

`Styles/Divider.jalxaml` 不发布任何行（它整个文件就是一个隐式 `Style`，`TargetType="Separator"`），消费的是调色板已有的 `DividerStrokeColorDefaultBrush`，键名逐字照抄上游。高对比不在这里做重指向——上游那行重指向本身就住在刷的定义处（:471），本层同一位置同一处置。

反向闸口：`AstraDividerTests.Nothing_in_the_library_publishes_a_divider_only_alias` 钉住五个"新控件可能带、但这层不许自造"的名字（`DividerBackground`/`DividerStroke`/`DividerThickness`/`DividerMargin`/`DividerCornerRadius`）都取不到东西。

## 5. 状态映射：这一格是空的，原因要写出来

上游没有这个控件，因此**没有 VisualState 可映射**，本层也就没有 Trigger。这不是"未测"，是不存在（`AstraDividerTests.The_divider_file_carries_exactly_two_setters_and_no_template` 把"两条 setter、零模板"钉成文本形状）。运行时侧 `Separator` 是静态元素，唯一会变的是宿主给的 `Orientation`，而那是控件自己的属性。

## 6. Known Gaps（不声称清单）

1. 本层的"分隔线"外观**无法与上游逐字对齐**，因为 §1 那条搜索：上游参考 commit 里没有这个控件。目录里这一行的 parity 用的是新加的 `no-upstream-control`，不是 `audited`。
2. 盒子高度 12 DIP 是运行时对 `Separator` 自己的度量（模式 `surface`/`mount`：横 300x12、竖 12x100），不是上游数字——上游各处同角色线的总高分别是 3（菜单）、8（侧栏）、20（应用栏），本层一个都没采用，也没有声称该采用哪个。
3. 不设 margin：七处各写各的，没有一处能代表通用分隔线；页面自己排版。
4. 上游用 `Rectangle` 画这条线，这台运行时 1 DIP 的 `Rectangle` 不印任何东西。本层因此把线交给控件自绘。**已发布模板里还有五处这种 1 DIP `Rectangle`**（`Styles/AppBar.jalxaml:194`、`Styles/DataGrid.jalxaml:53/59/60`、`Styles/TreeDataGrid.jalxaml:52`），它们现在印不出墨；这是一条独立缺陷，不在本段处理（另开一批，`AstraDividerTests.A_one_dip_rectangle_is_not_a_route_this_layer_can_take` 先把读数钉住）。
5. `Separator` 的 `Background` 属性存在，但自绘路径读的是 `StrokeBrush`；`Background` 在这个类型上到底画不画东西**没量**，因此本层不写这一条 setter。
6. 运行时默认线色 `#A3A3A4` 是从哪儿来的（哪一行令牌、还是硬编码）没查；只知道换掉它可行。
7. 高对比没有控件级动作，与全库同一条账（§4）。
8. 硬件输入证据为零：`IsHitTestVisible=False` 是出厂值并被断言，所以这条控件没有可测的输入臂——这不等于指针通路已验证（Task #13 照欠）。
9. 闸口在**同一棵树上连跑三遍得到三种结果**（绿 / `AstraContentDialogTests` 整类 13 名红 + 一条菜单子菜单红 / 一条 NavigationView 像素增量红），三遍之间源码只差 `Catalog.json` 的一处空白，且第一遍已经带着本段新增的 13 条用例——所以这三条都不是本段改出来的，但也不能当作"没发生"。逐条读数与排查方向记在 §7 与任务 #35、#47、#48；本段的 13 条用例在三遍里**没有一次进过失败名单**。

## 7. 四类证据

**构建**：`Styles/Divider.jalxaml` 进清单（清单字典数 59 → **60**，实测 `grep -vE '^\s*(#|$)'` 两遍），闸口里的 Debug 生成 **0 警告 / 0 错误**（整套读数见下面"闸口读数"）。附带一条运行时约束：清单与嵌入资源是互相校验的——把行注掉而留着文件，主题加载直接抛 `Styles/Divider.jalxaml is not listed in Themes/Manifest.txt`（这条在做 A/B 时撞到，正好证明"发出去的文件必须登记"）。`keys.md` 重出：1256 → **1257** 行、61 → **62** 份字典，新增的一行就是 `Styles/Divider.jalxaml - 1 rows` 的 `ImplicitStyle → Separator`。

**行为**（`AstraDividerTests` 13 条）：隐式样式落地用**实例身份**读（`Assert.Same(palette brush, separator.StrokeBrush)`，并断它不等于出厂灰 `#A3A3A4`）；`StrokeBrush` 必须是刷子不是颜色；`StrokeThickness==1`；`IsHitTestVisible=False`；换向后两条 setter 仍在；两条消费行的类型 theory；五个自造名的反向 theory；两条 setter 且零模板的文本形状闸口。
**A/B 有牙**：删掉 `Styles/Divider.jalxaml` 里的 `<Style>` 整块（先确认 `grep -c '<Style TargetType="Separator">'` 从 1 变 0）→ **5 红 / 8 绿**：红的正是"隐式样式落地""刷子不是颜色""换轴后 setter 仍在""Light 令牌色"四条加上文本形状闸口；还原后 13/13 绿。这条 A/B 同时量出另一件事：三条几何像素断言（600/600/200）在没有样式的构建里**照样绿**，因为框架那条灰线也印同样多的墨——所以几何不是令牌的证据，令牌的主张只住在颜色断言里。

**视觉**：三条像素主张全部按 `Render()` 的 DIP 像素算——横线 600 = 两行 × 300、竖线 200 = 两列 × 100（都是实测，不是挑的阈值）；Light 那两行读 `#F8F8F8`（= `#0F000000` 覆在白卡上）且出厂灰 `#A3A3A4` 计数 0；Dark 换深卡后线行比卡亮、且两档线色不同；两档品牌绿 `#207245` 计数 0。Gallery：Status 页新增 Divider 卡（两条正文夹一条横线 + 一行左中右带竖线），冒烟实测 `-Page status` 单页 `closed cleanly in 10.7 s` + `1 page(s) mounted and closed; no Gallery process left.`，随后全 10 页一遍 **exit 0**（status 5.7 s，最慢 command-bar 11.2 s，逐页 mounted and closed、无残留进程）。**故意不截屏**（理由记在 `adaptation/06`），所以这一页只主张"带分隔线的 markup 入树不崩、窗口关得掉"，颜色与几何不从这里取。

**硬件输入**：仍为零，理由见 §6 第 8 条。

**闸口读数**（串行 `tools/Test-AstraGates.ps1`，**四遍，三种结果**）：

| 遍 | 结果 | 读数 |
| --- | --- | --- |
| 1 | **全绿** | Debug 0 警告 / 0 错误（2.51 s，三个工程重出 dll）；**1288/1288、0 失败、0 跳过**（5 m 6 s）；调色板三行 `checked=True`（Light/Dark 各 83 源色 + 101 刷子，HighContrast 101 映射 + 3 条按住）；`keys.md is current: 1257 canonical lines.`；`All Astra gates passed.` |
| 2 | 22 失败 / 1266 通过（5 m 52 s） | `AstraContentDialogTests` **整类 13 个名字**全红，异常逐字 `System.InvalidOperationException : ContentDialog could not resolve a host window.`（栈 `AstraContentDialogTests.cs:606 Shown → ContentDialog.ShowAsync()`）；另加 `AstraMenuTests.A_submenu_surface_wears_upstreams_radius_from_the_framework_itself:637` 读回 `IsSubmenuOpen=False`。测试步以 **exit 1** 停止，调色板与清单两步这一遍没跑 |
| 3 | 1 失败 / 1287 通过（5 m 48 s） | `AstraNavigationTests.Selecting_a_pane_item_moves_pixels_without_a_brand_green:258`：`selecting added 1306 pixels of the pill colour; selected … #EAEAEAx9346 resting #EAEAEAx8040`。**形状不是增量偏小，是静止那张仍然带着 pill**（该类注释记下满 pill 应是 7 993~8 220），即 `SelectedItem = null` 之后那一次 `Host()` 捕获没拿到"取消选中"的画面 |
| 4 | 1 失败 / 1287 通过（5 m 43 s） | `AstraTeachingTipTests.A_side_with_no_room_for_the_card_loses_to_one_that_has`（4 s）：`Expected: Bottom / Actual: Top`，附读 `-185.9`——翻转后的落边选择判定这遍选了另一侧 |

四遍之间**产品源码与测试源码完全相同**：第 1 遍已经带着本段新增的 13 条用例，第 2~4 遍相对它只差 `Catalog.json` 的一处四空格缩进（纯空白）。HEAD 全程是 `e425208`，没有别的会话往树里提交过东西。**本段的 13 条用例在四遍里一次都没有进过失败名单**，第 2~4 遍红的都是别段的时序敏感用例（已分别登记为 #35 整类宿主窗口、#47 子菜单展开、#48 pill 增量与 TeachingTip 落边这一族）。

决定性的一条排查：把第 3、4 遍与第 2 遍里那三条一起单跑（`--filter` 三条，12 s）→ **2 通过 / 1 失败**，红的仍是 pill 那条，且形状与第 3 遍相同。所以它不是"只有整套顺序跑才会碰到"的罕见竞态，而是**这条断言当前在这台机器上不稳**——`Host()` 连拍两次的第二张没等到状态真正回落。这一条按 #48 处理（要把固定 `Settle` 换成"轮询到增量稳定或超时"，并把两张采样的直方图与帧数写进失败消息），**不在本段顺手改**：本段没有动过 `PixelHarness.Host`、NavigationView 或 TeachingTip 的任何一处，把别人的用例缝进 Divider 提交正是"用相邻证据替代"要避免的那种事。

第二遍的红不是重复劳动带出来的：第 1 遍全绿之后，本段对 `Catalog.json` 做了一次**四个空格的缩进订正**（拼接进去的新行开头少了缩进）。单跑目录那组测试时 `The_gallery_project_carries_the_catalog_it_reads` 当场报**源码 vs Gallery 输出副本不一致**（1 失败 / 5 通过）——它比的是随包复制件，而 `dotnet test tests/…` 不重建 Gallery。这正是 `adaptation/00` §S1-f 第 10 条早已写下的规矩："改过 `.jalxaml` 或 `Catalog.json` 之后，唯一算数的读数是把整解重建的串行闸口再跑一遍"。订正本身只动空白：改前改后都是 `json.loads` 58 行、BOM 在、CRLF 42 / LF 623 计数不变。

Gallery 冒烟（读数在上面"视觉"）：`-Page status` 单页一次 + 全 10 页一遍 exit 0，都在第 1 遍之后跑，不受上面四遍影响。
