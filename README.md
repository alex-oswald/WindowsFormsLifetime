# WindowsFormsLifetime

[![Test](https://github.com/alex-oswald/WindowsFormsLifetime/actions/workflows/test.yml/badge.svg?branch=main)](https://github.com/alex-oswald/WindowsFormsLifetime/actions/workflows/test.yml?query=branch%3Amain)
[![Nuget](https://img.shields.io/nuget/v/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)
[![Nuget](https://img.shields.io/nuget/dt/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)

A Windows Forms lifetime integration for the [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host).
It runs `Application.Run` on a dedicated STA UI thread and coordinates Windows Forms application exit with the
host lifetime.

- Use Windows Forms with dependency injection, configuration, logging, and hosted services.
- Stop the host when the application context exits, and close the main form when the host stops.
- Create forms and marshal UI work safely from background services.

## Requirements

The package supports `net8.0-windows`, `net9.0-windows`, and `net10.0-windows`. The consuming project must
enable Windows Forms:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

## Install

Install the `OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime` package from NuGet.

Using the Package Manager Console

```powershell
Install-Package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

Using the .NET CLI

```
dotnet add package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
``` 

## Quick start

Create a Windows Forms app and replace `Program.cs` with the following:

```csharp
using Microsoft.Extensions.Hosting;
using WinFormsApp1;
using WindowsFormsLifetime;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<Form1>();

IHost app = builder.Build();
app.Run();
```

`UseWindowsFormsLifetime<TStartForm>` registers the startup form, an `ApplicationContext`, the Windows Forms
host lifetime, `IFormProvider`, and `IGuiContext`. The startup form and its dependencies are constructed through
dependency injection on the UI thread. Closing the startup form ends the application context and stops the host.

## Application contexts

Use a custom `ApplicationContext` when the application lifetime is not defined by a single main form. The context
can be constructed by dependency injection:

```csharp
builder.UseWindowsFormsLifetime<TrayApplicationContext>();
```

To construct an application context from a startup form, use the two-type-parameter overload:

```csharp
builder.UseWindowsFormsLifetime<TrayApplicationContext, MainForm>(
    mainForm => new TrayApplicationContext(mainForm));
```

Factory overloads are also available when the application context needs an `IServiceProvider`.

## Web application builders

The lifetime can also be configured through an `IHostBuilder`, including the host exposed by
`WebApplicationBuilder`. This is useful for applications such as Blazor Hybrid:

```csharp
using Microsoft.AspNetCore.Builder;
using WindowsFormsLifetime;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>();

WebApplication app = builder.Build();
app.Run();
```

## Additional forms

Register forms that should be resolved from the container:

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddTransient<Form2>();
```

Inject `IFormProvider` into a form to create another registered form. `GetFormAsync<T>` creates the form on the
UI thread and gives it its own DI scope. Scoped and transient dependencies created for the form are disposed when
the form is disposed.

```csharp
public partial class Form1 : Form
{
    private readonly IFormProvider _formProvider;

    public Form1(IFormProvider formProvider)
    {
        InitializeComponent();
        _formProvider = formProvider;
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        Form2 form = await _formProvider.GetFormAsync<Form2>();
        form.Show();
    }
}
```

For forms with runtime constructor values, use one of the parameterized overloads. They support up to eight
explicit constructor parameters and resolve the remaining constructor dependencies from DI:

```csharp
DocumentForm form = await _formProvider.GetFormAsync<DocumentForm, Document>(document);
```

The synchronous `GetForm` overloads must only be called from the UI thread.

## Designer-created user controls

Forms can use constructor injection, but the designer creates child controls with `new MyControl()`, outside DI.
Enable optional runtime property injection to keep drag-and-drop designer support:

```csharp
builder.UseWindowsFormsLifetime<MainForm>();
builder.Services.AddSingleton<ICustomerService, CustomerService>();
builder.Services.AddWindowsFormsControlInjection();
```

Keep dependencies in the control's ordinary `.cs` file, not in `.Designer.cs`. Keep its parameterless constructor
and mark each required service property with `[InjectService]` and both designer attributes:

```csharp
using System.ComponentModel;
using WindowsFormsLifetime;

public partial class CustomerControl : UserControl, IOnServicesInjected
{
    public CustomerControl()
    {
        InitializeComponent();
    }

    [InjectService]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ICustomerService CustomerService { get; set; } = null!;

    public void OnServicesInjected()
    {
        refreshButton.Enabled = true;
    }
}
```

Do not register the user control itself with DI. The library initializes the existing designer-created instance,
including controls nested inside panels, tab pages, and other user controls. Properties must be public instance
properties with public ordinary setters, not indexers or `init` properties. Nullable service properties are still
required registrations.

### Initialization and ownership

Injection runs on the UI thread after form construction, before returning a form from any `IFormProvider`
creation overload. The startup main form is initialized before `preApplicationRunAction` and `Application.Run`.
Every pending property in the tree is assigned before `IOnServicesInjected` callbacks run, children first.
The callback is optional and runs once per control. Repeated injection skips initialized instances but can
initialize new descendants. Constructor code, `InitializeComponent`, and lifecycle events triggered during
construction must not access injected services.

Service resolution uses the same provider passed to form creation. Additional forms use the existing
library-owned scope, or the caller's scope when provided. Controls borrow services; they must not dispose them.
Unsubscribe from service events when the control is disposed, and use `IGuiContext` for background UI updates.

The startup form remains a root-resolved singleton. The singleton service in the example matches that lifetime;
this feature does not create a main-form scope or make root resolution of scoped services safe. For scope
validation, configure the host's normal `ServiceProviderOptions.ValidateScopes`. Do not move initialized
controls between unrelated scopes or expect another injection call to rebind their services.

Missing services and invalid declarations throw descriptive exceptions. Setter/callback failures are not
transactional: recreate affected controls rather than retrying their side effects. The helper never disposes
caller-owned controls or providers. The existing asynchronous startup contract is unchanged: `Host.StartAsync`
does not await UI readiness, and startup exceptions are not message-loop `OnThreadException` events.

### Controls created later

The library does not monitor `ControlAdded` or scan `Application.OpenForms`. For application-owned forms,
custom contexts that show a form during construction, or controls added later, use `IControlServiceInjector`
explicitly on the UI thread after construction and before displaying or attaching the new tree:

```csharp
public void AddCustomerControl(
    IControlServiceInjector injector, IServiceProvider formServices, Control parent)
{
    CustomerControl control = new();
    try
    {
        injector.Inject(control, formServices);
        parent.Controls.Add(control);
    }
    catch
    {
        control.Dispose();
        throw;
    }
}
```

`formServices` must be the owning form's provider, not an unrelated root provider. Its scope must outlive the
control. Controls created inside readiness callbacks or `preApplicationRunAction` also need explicit injection.
Keep callbacks synchronous and short; perform asynchronous work in later application operations.

## User-control analyzers

The NuGet package includes C# Roslyn diagnostics and designer-attribute code fixes. No separate analyzer package
or Visual Studio extension is required. The rules apply to this library's `[InjectService]` contract, not to
unrelated controls or properties, and do not rewrite generated designer code.

### WFLDI001

**Error:** An injected property must belong to a `UserControl` and be a public instance property with a public,
non-init setter and no index parameters. Correct the declaration manually; changing accessibility or constructor
semantics is not a safe automatic fix.

### WFLDI002

**Warning:** An injected property needs effective `[Browsable(false)]` metadata so it stays out of the Properties
window. The code fix adds or corrects that attribute.

### WFLDI003

**Warning:** An injected property needs effective
`[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]` metadata so the designer does not
serialize a runtime service. The code fix adds or corrects that attribute. Both metadata fixes support Fix All.

### WFLDI004

**Warning:** A user control constructor directly reads its own injected property before services are assigned.
Move that work to `OnServicesInjected` or later runtime interaction. This rule does not trace arbitrary helper
methods or every lifecycle event, and ignores deferred lambda/local-function bodies, `nameof`, and simple writes.

Configure severities or suppress deliberate exceptions through the normal `.editorconfig` mechanism:

```ini
[*.cs]
dotnet_diagnostic.WFLDI002.severity = error
dotnet_diagnostic.WFLDI003.severity = error
```

These diagnostics do not prove complete DI configuration. Registrations may be in another assembly, a custom
extension, a factory, or a conditional startup path. Remember to call `AddWindowsFormsControlInjection`;
the analyzer does not infer that it is missing from an absence of visible calls. Runtime injection and normal
container scope validation remain responsible for required services, provider lifetimes, and UI-thread rules.

## UI-thread work from background services

Although `GetFormAsync` creates a form on the UI thread, operations that interact with that form must also run
there. Inject `IGuiContext` into a hosted service and use it to marshal UI operations:

```csharp
Form2 form = await _formProvider.GetFormAsync<Form2>();
_guiContext.Invoke(() => form.Show());
```

`IGuiContext.InvokeAsync` is available when a UI-thread operation needs to return a result.

## Options and UI-thread exceptions

Configure the lifetime by passing an `Action<WindowsFormsLifetimeOptions>`:

```csharp
builder.UseWindowsFormsLifetime<Form1>(options =>
{
    options.EnableConsoleShutdown = true;
    options.OnThreadException = exception =>
    {
        Console.Error.WriteLine(exception);
    };
});
```

| Option | Default | Description |
| --- | --- | --- |
| `HighDpiMode` | `HighDpiMode.SystemAware` | The Windows Forms high-DPI mode. |
| `EnableVisualStyles` | `true` | Enables visual styles before the application starts. |
| `CompatibleTextRenderingDefault` | `false` | Sets the compatible text rendering default. |
| `SuppressStatusMessages` | `false` | Suppresses standard host lifetime status messages. |
| `EnableConsoleShutdown` | `false` | Maps Ctrl+C to host shutdown for console-enabled applications. |
| `OnThreadException` | `null` | Receives unhandled exceptions raised on the Windows Forms UI thread. |

`OnThreadException` is specific to the Windows Forms UI thread; it is not a process-wide exception handler.

## Console output in Debug configurations

Set `OutputType` to `Exe` for Debug builds when console logging or Ctrl+C shutdown is useful, and to `WinExe`
for Release builds when no console window should be shown:

```xml
<PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
  <OutputType>Exe</OutputType>
</PropertyGroup>

<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
  <OutputType>WinExe</OutputType>
</PropertyGroup>
```

## Samples

| Sample | Description |
| --- | --- |
| [SampleApp](samples/SampleApp) | Forms, hosted services, and nested designer-created user controls with injected services. |
| [AppContext](samples/AppContext) | A custom `ApplicationContext` with a hidden startup form. |
| [BlazorHybrid](samples/BlazorHybrid) | A Blazor Hybrid application configured through `WebApplicationBuilder`. |

## Credits

The layout of the `WindowsFormsLifetime` class is based on .NET Core's
[ConsoleLifetime](https://github.com/dotnet/extensions/blob/b83b27d76439497459fe9cf7337d5128c900eb5a/src/Hosting/Hosting/src/Internal/ConsoleLifetime.cs).

[ExecutionContext vs SynchronizationContext](https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/)

[Implementing a SynchronizationContext.SendAsync method](https://devblogs.microsoft.com/pfxteam/implementing-a-synchronizationcontext-sendasync-method/)
