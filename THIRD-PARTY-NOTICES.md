# Third-party notices

FluentJalium reproduces resource values, state names and template structure from
external projects. All of them are MIT-licensed; the pinned commit identifies exactly
which revision each value came from, and `docs/astra/audits/` records the per-file evidence.

## Microsoft UI.Xaml (WinUI 3)

- Repository: <https://github.com/microsoft/microsoft-ui-xaml>
- Pinned commit: `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`
- License: MIT
- Used for: theme resource values (`controls/dev/CommonStyles/Common_themeresources_any.xaml`,
  generated into `src/FluentJalium/Astra/Resources/Light.jalxaml` and `Dark.jalxaml` by
  `tools/Sync-AstraPalette.ps1`), control styles and templates, visual-state and template-part
  names, glyph resources, and animation timings.

Copyright (c) Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute,
sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## ModernWpf

- Repository: <https://github.com/Kinnara/ModernWpf>
- Pinned commit: `23555a6c00623b2f80e67f20d7f1df49a1d28ad8` (`v1.0.0-rc.1`)
- License: MIT
- Used for: the adaptation *method* only — resource/state separation, preserving upstream key
  names verbatim, documenting host substitutions, and resource-key inventories. No ModernWpf
  source is compiled or translated into this repository.

Copyright (c) 2019 ModernWpf Contributors

MIT license text as above.

## Fluent System Icons

- Repository: <https://github.com/microsoft/fluentui-system-icons>
- License: MIT
- Files: `src/FluentJalium/Fonts/FluentSystemIcons-Regular.ttf`,
  `src/FluentJalium/Fonts/FluentSystemIcons-Filled.ttf`.
- Status: **not referenced by the Astra implementation.** Codepoints above `0xFFFF` render as
  empty boxes through Jalium's text stack, so iconography uses the system Segoe tables instead.
  These files are retained pending a decision and are not part of the visual contract.

Copyright (c) 2020 Microsoft

MIT license text as above.

## LanStartWrite.Inkcanvas

`Astra/Controls/Input/FluentToggleSwitch.cs` and `Astra/Controls/Layout/FluentSettingsRow.cs`
originate in the author's separate Jalium application
`LanStartWrite.Inkcanvas`, which explored the same WinUI-on-Jalium problem. They are the same
author's code rather than a third-party dependency; they are listed here because they are not
original to this repository's history.
