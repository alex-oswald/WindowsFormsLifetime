using HiddenAppConsoleContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Specshell.WinForm.HiddenForm;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, collection) => { collection.AddSingleton<IHiddenMainForm, Main>(); })
    .UseWindowsFormsLifetimeAppContext<HiddenContext>(options =>
    {
        options.EnableVisualStyles = false;
        options.CompatibleTextRenderingDefault = false;
        options.SuppressStatusMessages = false;
        options.EnableConsoleShutdown = false;
    })
    .RunConsoleAsync();