# WindowsFormsLifetime

[![NuGet](https://img.shields.io/nuget/v/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)
[![NuGet downloads](https://img.shields.io/nuget/dt/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)

WindowsFormsLifetime integrates Windows Forms with the [.NET Generic Host](https://learn.microsoft.com/dotnet/core/extensions/generic-host). It registers Windows Forms as the host lifetime, runs the application loop on a dedicated STA UI thread, and makes the application's forms available through dependency injection.

Closing the startup form or ending the `ApplicationContext` stops the host. Stopping the host also closes the main form when it is still open.

## Requirements

- Windows
- .NET 9 (`net9.0-windows`) or .NET 10 (`net10.0-windows`)
- A project with Windows Forms enabled

```xml
<PropertyGroup>
  <TargetFramework>net9.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

## Install

```powershell
dotnet add package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

## Quick start

Use `Host.CreateApplicationBuilder` for a Windows Forms application that uses the Generic Host:

```csharp
using Microsoft.Extensions.Hosting;
using WindowsFormsLifetime;

var builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<MainForm>();

using var host = builder.Build();
host.Run();
```

`UseWindowsFormsLifetime<MainForm>()` registers `MainForm` and an `ApplicationContext` in the service container, starts the Windows Forms message loop, and connects its lifetime to the host.

The library also supports `IHostBuilder`:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .UseWindowsFormsLifetime<MainForm>()
    .Build();

host.Run();
```

### WebApplication and Blazor Hybrid hosts

For `WebApplicationBuilder`, configure its underlying host:

```csharp
using WindowsFormsLifetime;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<MainForm>();

builder.Services.AddWindowsFormsBlazorWebView();

var app = builder.Build();
app.Run();
```

See [samples/BlazorHybrid](samples/BlazorHybrid) for a complete Blazor Hybrid example.

## Application contexts

Use an `ApplicationContext` when the application lifetime should not be controlled by a single startup form, such as a notification-area application:

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.UseWindowsFormsLifetime<TrayApplicationContext, MainForm>(
    mainForm => new TrayApplicationContext(mainForm));

using var host = builder.Build();
host.Run();
```

The available registration patterns are:

| Pattern | Use case |
| --- | --- |
| `UseWindowsFormsLifetime<TStartForm>()` | Start with a form managed by the default `ApplicationContext`. |
| `UseWindowsFormsLifetime<TAppContext>()` | Resolve a custom `ApplicationContext` from DI or a factory. |
| `UseWindowsFormsLifetime<TAppContext, TStartForm>(...)` | Create a custom application context using the DI-managed startup form. |

The same generic overloads are available directly on `IServiceCollection` through `AddWindowsFormsLifetime`.

## Configuration

Pass a `WindowsFormsLifetimeOptions` delegate to configure Windows Forms initialization and host behavior:

```csharp
builder.UseWindowsFormsLifetime<MainForm>(options =>
{
    options.HighDpiMode = HighDpiMode.PerMonitorV2;
    options.EnableConsoleShutdown = true;
});
```

| Option | Default | Description |
| --- | --- | --- |
| `HighDpiMode` | `HighDpiMode.SystemAware` | DPI-awareness mode passed to `Application.SetHighDpiMode`. |
| `EnableVisualStyles` | `true` | Calls `Application.EnableVisualStyles` before the message loop starts. |
| `CompatibleTextRenderingDefault` | `false` | Value passed to `Application.SetCompatibleTextRenderingDefault`. |
| `SuppressStatusMessages` | `false` | Suppresses Generic Host startup and shutdown log messages. |
| `EnableConsoleShutdown` | `false` | Lets Ctrl+C stop the host when the application has a console. |

## Forms, dependency injection, and the UI thread

Register secondary forms with the container before building the host:

```csharp
builder.Services.AddTransient<SettingsForm>();
```

`IFormProvider` creates forms on the Windows Forms UI thread. In a form event handler, resolve and show another form as follows:

```csharp
public partial class MainForm : Form
{
    private readonly IFormProvider _forms;

    public MainForm(IFormProvider forms)
    {
        InitializeComponent();
        _forms = forms;
    }

    private async void showSettingsButton_Click(object? sender, EventArgs e)
    {
        var settings = await _forms.GetFormAsync<SettingsForm>();
        settings.Show();
    }
}
```

When code runs away from the UI thread, use `IGuiContext` to interact with controls:

```csharp
public sealed class SettingsLauncher(
    IFormProvider formProvider,
    IGuiContext guiContext)
{
    public async Task ShowAsync()
    {
        var settings = await formProvider.GetFormAsync<SettingsForm>();
        guiContext.Invoke(settings.Show);
    }
}
```

`IFormProvider.GetForm<T>()` is available for code already running on the UI thread. Use `GetFormAsync<T>()` from background code. The parameterless `GetForm` and `GetFormAsync` methods create a DI scope for the form and dispose that scope when the form is disposed. Overloads that accept an `IServiceScope` allow callers to share and own a scope explicitly.

The library registers these services:

| Service | Purpose |
| --- | --- |
| `IFormProvider` | Creates DI-managed forms on the UI thread and retrieves the main form. |
| `IGuiContext` | Dispatches work to the UI thread. |
| `IWindowsFormsSynchronizationContextProvider` | Exposes the Windows Forms synchronization context for advanced integrations. |

## Shutdown behavior

- Closing the startup form or ending the `ApplicationContext` calls `StopApplication` on the host.
- Cancelling or stopping the host closes and disposes the main form when its handle still exists.
- Set `EnableConsoleShutdown` to `true` to treat Ctrl+C as a host shutdown request.

## Samples

- [SampleApp](samples/SampleApp) demonstrates a Generic Host, background services, DI-managed forms, and UI-thread invocation.
- [AppContext](samples/AppContext) demonstrates a custom `ApplicationContext` with a startup form.
- [BlazorHybrid](samples/BlazorHybrid) demonstrates Windows Forms hosted from a `WebApplicationBuilder`.

## License

MIT. See [LICENSE.txt](LICENSE.txt).
