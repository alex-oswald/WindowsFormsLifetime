using AppContext;
using WindowsFormsLifetime;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// Pass in a factory lambda that constructs an ApplicationContext using the start form
builder.Host.UseWindowsFormsLifetime<ExampleApplicationContext, HiddenForm>(
    startForm => new(startForm));

WebApplication app = builder.Build();
app.Run();