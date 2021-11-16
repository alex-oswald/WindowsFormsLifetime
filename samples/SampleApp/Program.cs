using SampleApp;
using WindowsFormsLifetime.Sample;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>(options =>
{
    options.HighDpiMode = HighDpiMode.SystemAware;
    options.EnableVisualStyles = true;
    options.CompatibleTextRenderingDefault = false;
    options.SuppressStatusMessages = false;
    options.EnableConsoleShutdown = false;
});
builder.Services.AddHostedService<HostedService1>();
builder.Services.AddHostedService<HostedService2>();
builder.Services.AddTransient<Form2>();

var app = builder.Build();

app.Run();