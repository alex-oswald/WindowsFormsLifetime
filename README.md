# Windows Forms Lifetime

[![Build Status](https://dev.azure.com/oswaldtechnologies/WindowsFormsLifetime/_apis/build/status/alex-oswald.WindowsFormsLifetime?branchName=main)](https://dev.azure.com/oswaldtechnologies/WindowsFormsLifetime/_build/latest?definitionId=21&branchName=main)
[![Nuget](https://img.shields.io/nuget/v/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)
[![Nuget](https://img.shields.io/nuget/dt/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)](https://www.nuget.org/packages/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime/)

A Windows Forms hosting extension for .NET Core's generic host. Enables you to configure the generic host to use the lifetime of Windows Forms.
When configured, the generic host will start an `IHostedService` that runs Windows Forms in a separate thread.

## Getting Started

Install the `OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime` package from NuGet.

Using powershell

```powershell
Install-Package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

Using the .NET CLI

```
dotnet add package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

With .NET 6, this is super simple! Start a Windows Forms app running the .NET generic host with 4 lines of code!

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>();
var app = builder.Build();
app.Run();
```


#### Setup the `Program` class

When creating a new Windows Forms .NET Core project your `Program` class will look like the following:

```csharp
static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }
}
```

We will transform this to look very similar to an ASP.NET Core Web Application's `Program` class. We then chain the
`UseWindowsFormsLifeTime<TStartForm>` extension method onto the `IHostBuilder`. Replace `TStartForm` with your startup `Form`.
The below is an example of an updated `Program` class using the default builder.

```csharp
static class Program
{
    static void Main()
    {
        CreateHostBuilder().Build().Run();
    }

    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder(Array.Empty<string>())
            .UseWindowsFormsLifetime<Form1>()
            .ConfigureServices((hostContext, services) =>
            {
                    
            });
}
```

If you choose not to use the default builder, here is an example with only the Windows Forms lifetime configured.

```csharp
static class Program
{
    static void Main()
    {
        new HostBuilder().UseWindowsFormsLifetime<Form1>().Build().Run();
    }
}
```

### Passing options

You can further configure the Windows Forms lifetime by passing `Action<WindowsFormsLifeTimeOptions>`. For example,
with the default options:

```csharp
static class Program
{
    static void Main()
    {
        CreateHostBuilder().Build().Run();
    }

    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder(Array.Empty<string>())
            .UseWindowsFormsLifetime<Form1>(options =>
            {
                options.HighDpiMode = HighDpiMode.SystemAware;
                options.EnableVisualStyles = true;
                options.CompatibleTextRenderingDefault = false;
                options.SuppressStatusMessages = false;
                options.EnableConsoleShutdown = true;
            })
            .ConfigureServices((hostContext, services) =>
            {
                    
            });
}
```

`EnableConsoleShutdown`
Allows the use of Ctrl+C to shutdown the host while the console is being used.



### Instantiating and Showing Forms

Add more forms to the DI container.

To get a form use the `IFormProvider`. The form provider fetches an instance of the form from the DI container on the GUI thread. `IFormProvider` has one
method, `GetFormAsync<T>` used to fetch a form instance.

In this example, we inject `IFormProvider` into the main form, and use that to instantiate a new instance of `Form`, then show the form.

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

Sometimes you need to invoke an action on the GUI thread. Say you want to spawn a form from a background service. Use the `IGuiContext` to invoke
actions on the GUI thread.

In this example, a form is fetched and shown, in an action that is invoked on the GUI thread. Then a second form is shown. This example shows how
the GUI does not lock up during this process.

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


## Model-View-Presenter

The MVP pattern is a good pattern to use to develop a Windows Forms application. The `WindowsFormsLifetime.Mvp`
library contains some extension methods and classes to assist in developing an MVP app with `WindowsFormsLifetime
and the generic host.

>See the **MvpBasicSample** sample app

Create a new Windows Forms application. Add a button and label to the form. Edit the form and add a view interface.
Hook up some controls to the view interface. In this example, I've hooked up a button click to an `EventHandler`
and a labels value to an `int`. Let the `Form` inherit from the view interface.

```csharp
namespace MvpBasicSample
{
    public interface IMainView
    {
        int Count { get; set; }

        event EventHandler OnIncrementClicked;
    }

    public partial class MainForm : Form, IMainView
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public int Count
        {
            get => Convert.ToInt32(CountLabel.Text);
            set => CountLabel.Text = $"{value}";
        }

        public event EventHandler OnIncrementClicked
        {
            add => IncrementButton.Click += value;
            remove => IncrementButton.Click -= value;
        }
    }
}
```

Next we create the views presenter. The main forms presenter should inherit from `BaseMainFormPresenter`.
Then we can add the forms logic to the presenter. Initialize the count to 0. Hook up an event handler method to
increment the count.

```csharp
using WindowsFormsLifetime.Mvp;

namespace MvpBasicSample
{
    internal class MainFormPresenter : BaseMainFormPresenter<IMainView>
    {
        public MainFormPresenter(ApplicationContext applicationContext)
            : base(applicationContext)
        {
            View.OnIncrementClicked += OnIncrementClicked;
            View.Count = 0;
        }

        private void OnIncrementClicked(object? sender, EventArgs e)
        {
            View.Count++;
        }
    }
}
```

With those created, we can update `Program.cs`. Wipe out what the template created.
To use the Minimal API update the sdk in your `csproj` to `Microsoft.NET.Sdk.Web`.

```csharp
using MvpBasicSample;
using WindowsFormsLifetime.Mvp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<MainForm, IMainView, MainFormPresenter>();
var app = builder.Build();
app.Run();
```

Pretty simple!


## Only use the Console while debugging

I like to configure my `csproj` so that the `Console` runs only while my configuration is set to `Debug`, and doesn't
run when set to `Release`. Here is an example of how to do this. Setting the `OutputType` to `Exe` will run the console,
while setting it to `WinExe` will not.

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

[Stephen's blog post on ExecutionContext vs SynchronizationContext](https://devblogs.microsoft.com/pfxteam/executioncontext-vs-synchronizationcontext/)

https://devblogs.microsoft.com/pfxteam/implementing-a-synchronizationcontext-sendasync-method/
