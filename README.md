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
| [SampleApp](samples/SampleApp) | A Generic Host application with forms and hosted services. |
| [AppContext](samples/AppContext) | A custom `ApplicationContext` with a hidden startup form. |
| [BlazorHybrid](samples/BlazorHybrid) | A Blazor Hybrid application configured through `WebApplicationBuilder`. |

## Credits

The layout of the `WindowsFormsLifetime` class is based on .NET Core's
[ConsoleLifetime](https://github.com/dotnet/extensions/blob/b83b27d76439497459fe9cf7337d5128c900eb5a/src/Hosting/Hosting/src/Internal/ConsoleLifetime.cs).

[ExecutionContext vs SynchronizationContext](https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/)

[Implementing a SynchronizationContext.SendAsync method](https://devblogs.microsoft.com/pfxteam/implementing-a-synchronizationcontext-sendasync-method/)
