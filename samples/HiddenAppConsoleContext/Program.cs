using HiddenAppConsoleContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, collection) => { collection.AddSingleton<IHiddenForm, MainHiddenForm>(); })
    .UseWindowsFormsLifetimeAppContext<HiddenContext>(options =>
    {
        options.EnableVisualStyles = false;
        options.CompatibleTextRenderingDefault = false;
        options.SuppressStatusMessages = false;
        options.EnableConsoleShutdown = false;
    })
    .RunConsoleAsync();