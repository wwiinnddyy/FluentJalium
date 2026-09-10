using FluentJalium.WinUI;
using Jalium.UI.Markup;

// Bind WinUI's `xmlns:X="using:Microsoft.UI.Xaml.*"` spelling to real CLR namespaces that
// exist in this assembly. WinUI-Gallery code-behind declares `using Microsoft.UI.Xaml.Controls;`
// and derives from `Page`, so those names have to resolve for C# as well as for markup.
//
// Jalium's parser reaches these through XmlnsDefinitionRegistry.ScanAssembly, which subscribes
// to AppDomain.AssemblyLoad — no explicit registration call is needed. The source generator's
// XmlnsTypeResolver reads the same attributes, so the mapping holds at compile time too.
[assembly: XmlnsDefinition(WinUiXmlNamespaces.Controls, "Microsoft.UI.Xaml.Controls")]
[assembly: XmlnsDefinition(WinUiXmlNamespaces.Xaml, "Microsoft.UI.Xaml")]

// Namespaces we do NOT mirror with real types redirect to Jalium's canonical presentation
// namespace, where the framework's own 34 CLR mappings live. Element lookup is by simple name
// and namespace-agnostic (XamlReader.ResolveTypeUncached step 2), so in practice most of these
// already resolve without the redirect; it is the fallback for names not in XamlTypeRegistry yet.
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.Primitives, JalxamlNamespaces.Presentation)]
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.Media, JalxamlNamespaces.Presentation)]
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.Data, JalxamlNamespaces.Presentation)]
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.Interactivity, JalxamlNamespaces.Presentation)]
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.XamlMarkup, JalxamlNamespaces.XamlMarkup)]

// UWP-era spellings still present in older samples.
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.LegacyControls, JalxamlNamespaces.Presentation)]
[assembly: XmlnsCompatibleWith(WinUiXmlNamespaces.LegacyXaml, JalxamlNamespaces.Presentation)]
