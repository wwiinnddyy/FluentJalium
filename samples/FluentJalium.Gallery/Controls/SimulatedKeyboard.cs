using Jalium.UI;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace FluentJalium.Gallery.Controls;

/// <summary>
/// Creates routed <see cref="KeyEventArgs"/> for controls that consume keyboard events from the
/// event router rather than from a physical device.
/// </summary>
/// <remarks>
/// <see cref="KeyEventArgs"/> only exposes a device-backed constructor and derives both
/// <see cref="KeyEventArgs.KeyStates"/> and <see cref="KeyEventArgs.KeyboardModifiers"/> from the
/// supplied device, so the device below reports the states the caller asks for.
/// </remarks>
internal static class SimulatedKeyboard
{
    public static KeyEventArgs CreateKeyDown(Key key, ModifierKeys modifiers = ModifierKeys.None) =>
        Create(UIElement.KeyDownEvent, key, modifiers, isDown: true);

    public static KeyEventArgs CreateKeyUp(Key key, ModifierKeys modifiers = ModifierKeys.None) =>
        Create(UIElement.KeyUpEvent, key, modifiers, isDown: false);

    private static KeyEventArgs Create(RoutedEvent routedEvent, Key key, ModifierKeys modifiers, bool isDown)
    {
        var keyboard = new SyntheticKeyboardDevice(key, modifiers, isDown);
        return new KeyEventArgs(keyboard, keyboard.ActiveSource!, Environment.TickCount, key)
        {
            RoutedEvent = routedEvent
        };
    }

    private static Key GetModifierLeftKey(ModifierKeys modifiers) => modifiers switch
    {
        ModifierKeys.Alt => Key.LeftAlt,
        ModifierKeys.Control => Key.LeftCtrl,
        ModifierKeys.Shift => Key.LeftShift,
        ModifierKeys.Windows => Key.LWin,
        _ => Key.None
    };

    private sealed class SyntheticKeyboardDevice : KeyboardDevice
    {
        private readonly SyntheticPresentationSource _source = new();
        private readonly Key _key;
        private readonly Key _modifierKey;
        private readonly bool _isDown;

        public SyntheticKeyboardDevice(Key key, ModifierKeys modifiers, bool isDown)
        {
            _key = key;
            _modifierKey = GetModifierLeftKey(modifiers);
            _isDown = isDown;
        }

        public override IInputElement? Target => null!;

        public override PresentationSource? ActiveSource => _source;

        protected override KeyStates GetKeyStatesFromSystem(Key key)
        {
            if (key == _modifierKey && key != Key.None)
            {
                return KeyStates.Down;
            }

            return key == _key && _isDown ? KeyStates.Down : KeyStates.None;
        }
    }

    private sealed class SyntheticPresentationSource : PresentationSource
    {
        public override Visual? RootVisual { get; set; }

        public override bool IsDisposed => false;

        protected override CompositionTarget? GetCompositionTargetCore() => null;
    }
}
