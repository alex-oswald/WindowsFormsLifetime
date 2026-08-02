# WindowsFormsLifetime

[![Build Status](https://dev.azure.com/oswaldtechnologies/WindowsFormsLifetime/_apis/build/status/alex-oswald.WindowsFormsLifetime?branchName=main)](https://dev.azure.com/oswaldtechnologies/WindowsFormsLifetime/_build/latest?definitionId=21&branchName=main)
[![Nuget](https://img.shields.io/nuget/v/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)
[![Nuget](https://img.shields.io/nuget/dt/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)

A Windows Forms hosting extension for the [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host).
This library enables you to configure the generic host to use the lifetime of Windows Forms. When configured,
the generic host will start an `IHostedService` that runs Windows Forms in a separate UI specific thread.

- The Generic Host will use Windows Forms as it's lifetime (when the main form closes, the host shuts down)
- All the benefits of .NET and the Generic Host, dependency injection, configuration, logging...
- Easier multi-threading in Windows Forms

## Quick Start

Install the `OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime` package from NuGet.

Using Powershell

```powershell
Install-Package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

Using the .NET CLI

```
dotnet add package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
``` 

Create a new **Windows Forms App**.

Replace the contents of `Program.cs` with the following.

```csharp
using Microsoft.Extensions.Hosting;
using WinFormsApp1;
using WindowsFormsLifetime;

var builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<Form1>();

var app = builder.Build();
app.Run();
```

**Run the app!**

**Your Windows Forms app is now running with the Generic Host!**

### Passing options

You can further configure the Windows Forms lifetime by passing `Action<WindowsFormsLifeTimeOptions>`.
For example, with the default options:

```csharp
builder.UseWindowsFormsLifetime<Form1>(options =>
{
    options.HighDpiMode = HighDpiMode.SystemAware;
    options.EnableVisualStyles = true;
    options.CompatibleTextRenderingDefault = false;
    options.SuppressStatusMessages = false;
    options.EnableConsoleShutdown = true;
});
```

`EnableConsoleShutdown`
Allows the use of Ctrl+C to shutdown the host while the console is being used.

### Instantiating and Showing Forms

Add more forms to the DI container.

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<Form1>();
builder.Services.AddTransient<Form2>();
var app = builder.Build();
app.Run();
```

To get a form, use the `IFormProvider`. The form provider fetches an instance of the form from the DI
container on the GUI thread. `IFormProvider` has the method, `GetFormAsync<T>`, used to fetch a form
instance.

In this example, we inject `IFormProvider` into the main form, and use that to instantiate a new
instance of `Form2`, then show the form.

```csharp
public partial class Form1 : Form
{
    private readonly ILogger<Form1> _logger;
    private readonly IFormProvider _formProvider;

    public Form1(ILogger<Form1> logger, IFormProvider formProvider)
    {
        InitializeComponent();
        _logger = logger;
        _formProvider = formProvider;
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        _logger.LogInformation("Show Form2");
        var form = await _formProvider.GetFormAsync<Form2>();
        form.Show();
    }
}
```

### Invoking on the GUI thread

Sometimes you need to invoke an action on the GUI thread. Say you want to spawn a form from a background
service. Use the `IGuiContext` to invoke actions on the GUI thread.

In this example, a form is fetched and shown, in an action that is invoked on the GUI thread. Then a second
form is shown. This example shows how the GUI does not lock up during this process.

```csharp
public class HostedService1 : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IFormProvider _fp;
    private readonly IGuiContext _guiContext;

    public HostedService1(
        ILogger<HostedService1> logger,
        IFormProvider formProvider,
        IGuiContext guiContext)
    {
        _logger = logger;
        _fp = formProvider;
        _guiContext = guiContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int count = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
            if (count < 5)
            {
                await _guiContext.InvokeAsync(async () =>
                {
                    var form = await _fp.GetFormAsync<Form2>();
                    form.Show();
                });
            }
            count++;
            _logger.LogInformation("HostedService1 Tick 1000ms");
        }
    }
}
```

## Only use the Console while debugging

I like to configure my `csproj` so that the `Console` runs only while my configuration is set to `Debug`,
and doesn't run when set to `Release`. Here is an example of how to do this. Setting the `OutputType` to
`Exe` will run the console, while setting it to `WinExe` will not.

```xml
<PropertyGroup Condition=" '$(Configuration)' == 'Debug' ">
  <OutputType>Exe</OutputType>
</PropertyGroup>

<PropertyGroup Condition=" '$(Configuration)' == 'Release' ">
  <OutputType>WinExe</OutputType>
</PropertyGroup>
```

## Credits

The layout of the `WindowsFormsLifetime` class is based on .NET Core's
[ConsoleLifetime](https://github.com/dotnet/extensions/blob/b83b27d76439497459fe9cf7337d5128c900eb5a/src/Hosting/Hosting/src/Internal/ConsoleLifetime.cs).

[ExecutionContext vs SynchronizationContext](https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/)

[Implementing a SynchronizationContext.SendAsync method](https://devblogs.microsoft.com/pfxteam/implementing-a-synchronizationcontext-sendasync-method/)
