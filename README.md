# OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime

![Nuget](https://img.shields.io/nuget/v/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)
![Nuget](https://img.shields.io/nuget/dt/OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime)

A Windows Forms hosting extension for .NET Core's generic host. Enables you to configure the generic host to use the lifetime of Windows Forms.
When configured, the generic host will start an `IHostedService` that runs Windows Forms in a separate thread.

### Getting Started

Install the `OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime` package from NuGet.

Using powershell

```powershell
Install-Package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
```

Using the .NET CLI

```
dotnet add package OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime
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

#### Passing options

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


#### Only use the Console while debugging

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

#### Credits

The layout of the `WindowsFormsLifetime` class is based on .NET Core's
[ConsoleLifetime](https://github.com/dotnet/extensions/blob/b83b27d76439497459fe9cf7337d5128c900eb5a/src/Hosting/Hosting/src/Internal/ConsoleLifetime.cs).