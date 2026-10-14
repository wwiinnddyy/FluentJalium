# S0-r correction readings (2026-09-19)

Two instruments, both external PrintWindow grabs of a real window. The claim under test is the spacing
batch's S0-r: "a nested Border whose brush is written through a TransitionProperty never commits its end
value into the composited frame". Both readings below say it does, and the correction says why the earlier
pair of numbers (0 px / 9108 px) could be produced by the same code.

## 1. The 17-cell matrix - spike/TransitionProbe, run through run-probe.ps1

Window 1130x400 DIP, dpi=168 (175%), PrintWindow PW_CLIENTONLY|PW_RENDERFULLCONTENT, three grabs driven by
the probe's own phase marks. Each cell is 90x90 DIP = 24649 exact-colour pixels when it lands (106x106 =
34223, the 90x90 stock buttons read 23392 because their 1 DIP border is counted out, and the app-bar shape
reads 18968 because its highlight is inset 2,6,2,6 inside the cell). 0 would mean the frame never got the
value.

cell                 hex      capture1 capture2 capture3   what differs from its neighbour
01 root+trans        #FF0001    24649    24649    24649   highlight is the template root, transition on
02 root none         #7F0001    24647    24647    24647   same, transition off
03 nested+trans      #FE00FE    24649    24649    24649   one level down, transition on  <- the suspect shape
04 nested none       #0300FF    24649    24649    24649   same, transition off
05 nested zero       #B400B4    24649    24649    24649   transition declared, duration 0
06 nested after      #00B4B4    24647    24647    24647   value written after the window is shown
07 nested themed     #60CDFF    24649    24649    24649   brush from {ThemeResource}, not a literal
08 nested width      #7F7F01    24649    24649    24649   Width transition 30 -> 90 (area, not colour)
09 bound+cell        #FF0101    24649    24649    24649   TemplateBinding AND a trigger setter on one property
13 single+border     #01FF01    23409    23409    23409   border + padding on the transitioning element
14 two names         #0101FF    24647    24647    24647   TransitionProperty "Background, BorderBrush"
15 no space          #EE00EE    24646    24646    24646   same list without the space
16 root two names    #00EEEE    24648    24648    24648   that list on the template root
17 appbar shape      #C86400    18968    18968    18968   the shipping app-bar cell reproduced in full
10 loose border      #FF7F01    34223    34223    34223   a Border in plain parsed content, no template
11 stock button pre  #0FFF5F    23392    23392    23392   shipping Astra Button skin, written before show
12 stock button post #5F0FFF    23392    23392    23392   shipping Astra Button skin, written after show

17/17 in all three grabs, identical to the pixel. Read the per-cell property read-backs in
transition-probe1.txt; the phase wall clocks are in transition-probe1.marks.txt.

Two things this run also produced, both already documented elsewhere and worth repeating because they
 masqueraded as the defect for a day:

- The first version of the matrix kept its nine templates as keyed ControlTemplate resources. Every cell
  then read back the resting colour - the keyed-template Trigger.Property=null defect (audits/slider.md).
  Only after moving each template inline into a keyed Style, i.e. the shipping shape, did the cells fire.
- Phases 2 and 3 of the very first probe run grabbed 0 for every cell at once, including cells with no
  transition at all. That is a blank frame, and it is what an ungated capture reports as "missing fill".

## 2. The original scene, A/B with a blank-frame gate - spike/VisualQA

Same page (`FluentJalium.Gallery.exe --page command-bar`), same instrument (PrintWindow, PW_CLIENTONLY|
PW_RENDERFULLCONTENT, dpi=168, 1925x1435), counts over the whole window via count-colors.ps1. Only
`Styles/AppBar.jalxaml`'s two highlight borders differ.

build                                       #60CDFF   #323232   grabs before a painted frame
no transition (spacing batch commit state)     8832    958227   15
transition restored (upstream 83 ms)           8832    958227    7

out/ab-notransition.png and out/ab-transition.png - in spike/VisualQA/out, not here - are those two
pictures, alongside the matrix's own spike/TransitionProbe/out/transition-capture1..3.png. The counts are
identical, so the transition does not touch the composited frame; the 0 px reading that retired it came from
a grab of a surface that had not drawn yet, which is indistinguishable from an absent fill until the frame
is checked for content first.
