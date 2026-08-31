namespace Microsoft.UI.Xaml.Controls;

/// <summary>
/// Code-behind base classes for WinUI pages.
/// </summary>
/// <remarks>
/// Markup resolves elements by simple name, so <see cref="FluentJalium.Controls.FWButton"/> and
/// friends need no CLR presence here — <c>XamlTypeRegistry</c> aliases cover them. These do,
/// because a WinUI <c>x:Class</c> code-behind inherits from the base <em>in C#</em>:
/// <c>public sealed partial class Foo : UserControl</c> with <c>using Microsoft.UI.Xaml.Controls;</c>
/// has to compile against a real type in this namespace.
///
/// Each of these is also registered as a markup alias (see ControlAliases) so that an element and
/// its code-behind base resolve to the same type rather than to two unrelated ones.
/// </remarks>
public class Page : Jalium.UI.Controls.Page
{
}

/// <summary>Code-behind base class for WinUI user controls.</summary>
public class UserControl : Jalium.UI.Controls.UserControl
{
}

/// <summary>
/// Code-behind base class for WinUI dialogs. Derives from FluentJalium's control rather than
/// Jalium's, since a dialog is constructed in code and shown with <c>ShowAsync()</c> rather than
/// authored as a markup element.
/// </summary>
public class ContentDialog : FluentJalium.Controls.FWContentDialog
{
}
