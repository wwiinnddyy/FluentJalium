# Astra implementation

This branch is a breaking replacement of the pre-Astra FW facade, not a WPF or WinUI runtime application.
The old implementation remains available on branch `new` at `d03b377`.

- Product code lives in `src/FluentJalium/Astra`. No retired FW types or compatibility parser belongs in the new runtime.
- Use native Jalium controls with `.jalxaml` templates. Add a custom control only for a demonstrated behavior gap.
- Runtime authority is NuGet Jalium.UI **26.10.9**; a sibling source tree is reference only and may be newer.
- WinUI reference: `../microsoft-ui-xaml`, commit `19e3bdc3ccf3361393d623d3a5d2667cb8f33229`.
- Adaptation-method reference: `../ModernWpf`, commit `23555a6c00623b2f80e67f20d7f1df49a1d28ad8`, `v1.0.0-rc.1`.
- Reference repositories are read-only. Never reset, restore, checkout or patch them during a FluentJalium change.
- Templates, input/commands, animation, resources and platform substitution are separate responsibilities.
- Keep mouse, touch and keyboard paths functional. Never replace touch capture with mouse-only DragMove.
- Document resource and public API changes, upstream file evidence, and adaptations under `docs/astra`.
- Build serially. Integration checks must close their windows and must not write into the user's profile.
- Record build, behavior, visual and hardware-input evidence separately. A template or wrapper is not proof of complete WinUI parity.
- No unconditional retemplating, reflection into private framework fields, or per-window resource repair loops.
