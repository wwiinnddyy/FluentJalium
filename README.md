# FluentJalium

FluentJalium is a Fluent Design System theme and control library for Jalium.UI on .NET 10.

The first milestone provides a Fluent resource layer, Fluent default styles for core Jalium controls, and a small gallery application for visual validation.

## Usage

```csharp
using FluentJalium.Controls.Themes;
using Jalium.UI;

var app = new Application();
FluentThemeManager.Apply(app);
```

JALXAML consumers can also merge the theme dictionary directly:

```xml
<ResourceDictionary Source="/FluentJalium;component/Themes/Generic.jalxaml" />
```

## Build

```powershell
dotnet build FluentJalium.slnx -c Debug
dotnet test tests/FluentJalium.Tests/FluentJalium.Tests.csproj -c Debug
```

By default the repository references the sibling `../Jalium.UI` source tree. To use NuGet packages instead:

```powershell
dotnet build FluentJalium.slnx -c Debug /p:UseJaliumSourceReferences=false
```

