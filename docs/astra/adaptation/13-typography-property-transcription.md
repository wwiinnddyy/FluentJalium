# 13 · 排印层逐项转录：八个属性、三条数值路、一处不做的选择

`Catalog.json` 里唯一挂着 `ported` 的行是 `TextBlock` → `ThemeResources/Typography.jalxaml`，
理由写的是"字号阶梯没有逐键对齐上游"。要对齐就得先知道本运行时（NuGet **26.10.9**）到底能表达
上游 `CommonStyles/TextBlock_themeresources.xaml`（`19e3bdc3`）写的哪几个属性——这不是风格选择，
上游那八个 setter 里有几个在这台机器上根本不存在成员。同理，那七个 `x:Double` 字号行能不能带住数字，
决定转录出来的是"键名 + 行"还是"字面量 + 一张缺口账"。

权威：上游文件逐行（`CommonStyles/TextBlock_themeresources.xaml` blob
`08cbcf9cc6a9146b7d192f1f294b949dd2841cfd` @`19e3bdc3`，`:3-51`）。方法参照 `../ModernWpf` 的
`Styles/TextStyles.xaml`（blob `202e5161adf223f5554e301cb7e8173b265485a4` @`23555a6c`）`:7-48`——
它把 `OpticalMarginAlignment` 注释掉、把 `XamlAutoFontFamily` 和 `TextLineBounds` 整个删掉、保留
`sys:Double` 行，正是"宿主表达不了的删掉、表达得了的逐键抄"这条路；最后一项差别（WPF 带得住数字、
本运行时带不住）就是结论三的内容。

## 原始读数

`spike/DoubleRowProbe` 加了一个 `typography` 模式（同一套 boot/合并/开窗/pump 管路，问法换成排印）。
每个属性**单独成字典**，因为一个本运行时没有的 Setter 会让整份字典在加载时失败——八个一起放就是
"一个缺陷七个伤亡"，读不出是谁。样式 setter 走 `{ThemeResource}` 时，值在挂载后从活的 `TextBlock`
上读回；`LineStackingStrategy` 用上游**不用**的那个值探，因为 `MaxHeight` 恰好也是宿主默认值，
读回 `MaxHeight` 分不出"落上了"和"没这条"。

```
census: present: FontFamily,FontSize,FontWeight,TextTrimming,TextWrapping,LineStackingStrategy
census: absent: TextLineBounds,OpticalMarginAlignment
detached: FontFamily=Microsoft YaHei UI FontSize=14 FontWeight=Normal TextTrimming=None TextWrapping=NoWrap LineStackingStrategy=MaxHeight
mounted:  FontFamily=Microsoft YaHei UI FontSize=14 FontWeight=Normal TextTrimming=None TextWrapping=Wrap   LineStackingStrategy=MaxHeight
mounted.ink: #FFFFFFFF primary-token=True TextPrimary-name=True TextSecondary-name=False
setter-FontFamily:            -> FontFamily=XamlAutoFontFamily  ... ink=primary-token=True
setter-FontSize:              -> FontSize=18
setter-FontWeight:            -> FontWeight=SemiBold
setter-TextTrimming:          -> TextTrimming=CharacterEllipsis
setter-TextWrapping:          -> TextWrapping=Wrap
setter-LineStackingStrategy:  -> LineStackingStrategy=BlockLineHeight
setter-TextLineBounds:        LOAD-FAIL XamlParseException: Setter.Property 'TextLineBounds' cannot be resolved
                                   for the style target type when TargetName is not set. StyleTargetType='Jalium.UI.Controls.TextBlock'
setter-OpticalMarginAlignment: LOAD-FAIL，同上
route-literal:        resolves=<KeyNotFound> -> FontSize=18
route-sysrow:         resolves=Double=0      -> FontSize=14
route-coderow:        resolves=Double=18     -> FontSize=18
route-coderow-string: resolves=String=18     -> FontSize=14
```

`detached` 与 `mounted` 只差一个 `TextWrapping`——这一层隐式 `TextBlock` 样式（上游没有那一条）在
本运行时实际只做了一件事：把默认 `NoWrap` 翻成 `Wrap`。`mounted.ink` 里三个 `True` 是同义反复：
别名层发货之后 `TextPrimary` 这个名字解析出的就是 `TextFillColorPrimaryBrush` 那一个实例。

## 结论一：八个属性里六个能落，两个宿主没有成员

| 上游 setter | 成员存在 | 宿主默认 | setter 落得上 | 处理 |
| --- | --- | --- | --- | --- |
| `FontFamily="XamlAutoFontFamily"` | 是（属性） | `Microsoft YaHei UI` | 值按字面存成族名 `XamlAutoFontFamily` | **删**：本运行时没有这个标记值，抄过去等于把系统 UI 字体换成一个不存在的族名，落到 fallback |
| `FontSize="{StaticResource *TextBlockFontSize}"` | 是 | 14 | 字面量落得上（18→18） | **留**，数值用字面量，理由见结论三 |
| `FontWeight="SemiBold"`（Base） | 是 | Normal | 落得上 | **留**（逐风格继承，见下表） |
| `TextTrimming="CharacterEllipsis"` | 是 | None | 落得上 | **留** |
| `TextWrapping="Wrap"` | 是 | NoWrap | 落得上 | **留** |
| `LineStackingStrategy="MaxHeight"` | 是 | MaxHeight | 落得上（`BlockLineHeight` 探出） | **留**：值与宿主默认相同，读回分不出，故只在形状断言里出现，不进到达断言 |
| `TextLineBounds="Full"` | **否** | — | 加载即失败 | **删**，写进行注释 |
| `OpticalMarginAlignment="TrimSideBearings"` | **否** | — | 加载即失败 | **删**，写进行注释 |

落到风格上就是上游的十个键（`Base` / `Caption` / `Body` / `BodyStrong` / `BodyLarge` /
`BodyLargeStrong` / `Subtitle` / `Title` / `TitleLarge` / `Display`）逐个挂到 `Base` 上，尺寸与字重按
上游表——旧文件的链是 `Caption`→`Body`、`Subtitle`/`Title`/`LargeTitle`→`BodyStrong`，且少了四个键、
把 `TitleLargeTextBlockStyle` 叫成 `LargeTitleTextBlockStyle`（零消费点，直接改名不留别名）。
`AstraTypographyTests` 两条 Theory 分别钉形状（每个样式的 setter 名单必须逐字等于上游那份，
多一条少一条都红）和到达（挂载后 `FontSize`/`FontWeight`/`TextTrimming`/`TextWrapping` 读回）。
牙口是改坏验出来的，四处临时改动各自的红：

- `Base` 的 `SemiBold` 拍平成 `Normal` → 7 条到达红（凡靠继承拿粗体的样式一起红，正说明继承链在跑）；
- `Caption` 的 `12` 改成 `13` → 1 条到达红；
- 给 `Caption` 塞回 `Foreground` 行 → 1 条形状红 + 墨迹事实红；
- 给 `Base` 塞 `FontFamily` 行 → 1 条形状红。

改回来后 `AstraTypographyTests` 31/31。复现读数：`dotnet run --project spike/DoubleRowProbe -- typography`。

## 结论二：删掉我们自造的 `Foreground` 行之后，墨还是我们的

旧文件在 `Body` 上钉 `TextFillColorPrimaryBrush`、在 `Caption` 上钉 `TextFillColorSecondaryBrush`；
上游这两个样式**一条墨都不写**（`TextBlock_themeresources.xaml:10-51` 里没有 `Foreground`）。
删掉之后一个样式不写 `Foreground` 的块读回的是 `TextFillColorPrimaryBrush` 那一个实例——也就是
宿主自己按 `TextPrimary` 这个名字现查，而别名层已把那个名字指向我们的令牌（`A2`，见 ROADMAP #12）。
这条由 `A_block_whose_style_writes_no_ink_still_carries_our_primary_token` 钉住，并且带
`NotSame(secondary)` 那一腿。

**后果要如实说**：`CaptionTextBlockStyle` 在 Gallery 有 211 个消费点，它们从次级灰变成初级墨——
这才是上游的 Caption。字形的墨本来就到不了捕获通路（#50），所以这一条只是 DP 层的主张，
不是像素主张；如果某个页面确实要次级灰，该由那个页面点名 `TextFillColorSecondaryBrush`，
不该由共享样式替全库决定。

顺带一条边界：`Setter Property="Foreground"` 在样式层，压不过元素上的局部值/模板绑定，所以
删掉这行只会把继承路径让回来，不会盖掉任何一处已经显式写好的前景（#51/#55 那批修的就是这个形状）。
"样式只管尺寸、墨由用处点名"不是我们的发明，是上游自己的用法：`CommandBarFlyout_themeresources.xaml:299`
写 `Style="{ThemeResource CaptionTextBlockStyle}" Foreground="{TemplateBinding Foreground}"`，
`:302` 写 `Style=…CaptionTextBlockStyle … Foreground={ThemeResource CommandBarFlyoutAppBarButtonKeyboardTextLabelForeground}`
——同一个样式在两处配两种墨，正是共享样式**不写**墨才可能有的形状。

## 结论三：数值行是 reader 的洞，不是管线的洞

三条路都量了，落点都是 `FontSize`：

| 路 | 行读回 | 落到 `FontSize` |
| --- | --- | --- |
| setter 里写字面量 18 | — | **18** |
| `<sys:Double x:Key=…>18</sys:Double>`（markup） | `Double=0` | 14（默认） |
| 同 key 由 C# 放 `18d` | `Double=18` | **18** |
| 同 key 由 C# 放 `"18"`（String） | `String=18` | 14（默认） |

外加 `<x:Double>` 直接加载失败（`Cannot resolve type 'Double' in namespace
'http://schemas.microsoft.com/winfx/2006/xaml'`），`sys:Int32` 同样归零（§S1-r pertheme），
`sys:String` 能保住文本但过不了到 Double 属性。也就是说：**这条路上没有一种 markup 写法能把数字
带到一个 Double 依赖属性上**，而 `Thickness` 行既保得住值又落得上 `Padding`（§S1-r 正对照），
所以坏的只是"reader 读数值元素"，`{ThemeResource}` → setter → DP 这一段是活的——C# 放的 Double
照样落得上。

于是七个上游键名（`CaptionTextBlockFontSize` … `DisplayTextBlockFontSize`）在这层只能继续空缺，
`An_upstream_size_row_is_not_published` 逐名钉住。这和 `adaptation/01:149` 那句"`x:Double` 进不了字典"
是同一件事，现在有了逐项读数和一条 `A_numeric_row_written_in_markup_loses_its_number_while_one_written_in_code_does_not`。

**边界（#12 第八轮补，2026-09-22）**：本节坏的是**把数字写进标记**这一形。转发形
（`<StaticResource x:Key="A" ResourceKey="B"/>`，也就是别名层出货的全部形状）不在这一形里——实测能带住数字并
落到挂载 `FontSize` 上，代价是它带的是解析期那一份快照，源改了别名不跟。产品侧读数与测试名见
`ROADMAP.md` 的"#12 A2 别名层第八轮"与 `AstraFrameworkNameResolutionTests
.A_redirect_row_carries_a_number_to_a_live_font_size_and_freezes_it_there`。也就是说：结论三封住的是
"标记里造一个数字"，没有封住"下面那条路（代码种数字）一旦开了，标记里能不能按名字转发它"。

## 结论四：一个想清楚再做的选择，没做

C# 能放 Double，那么让 `FluentThemeManager` 在 `Apply` 时把这七个数字当资源行种进
`Application.Resources`，样式就能改成 `{ThemeResource BodyTextBlockFontSize}`，键名与形状同时到位。
**这一批没做**，理由记在这里以免变成沉默的偏好：

1. `src/FluentJalium` 代码侧碰资源表只有一条路：把**整份字典**并进去
   （`Themes/FluentThemeManager.cs:117` 的 `MergedDictionaries.Add`，配合 `:105` 的卸载），
   没有任何一处按名字往表里种单行；`:166` 的 `Resources[key] as Style` 是读。这七行会开一个新机制，
   而这一层的规则是"上游逐键转录 + `{ThemeResource}`"，不是"由主题门面补字典"。
2. `docs/astra/audits/keys.md` 是从 `.jalxaml` 生成的（`tools/Report-AstraResourceKeys.ps1`），
   代码种的行不在清单里，公开键账本会开始漏账——除非先给工具加一条代码侧来源，那是另一批的活。
3. 这七个数字不随主题变，字面量承载的值与行承载的值相同；差别只在"消费方按名字能不能查到"。

代价是明确的：拿 `TryFindResource("BodyTextBlockFontSize")` 的消费方查不到东西。想反悔时，
上面那条测试就是同一份证据的入口。
