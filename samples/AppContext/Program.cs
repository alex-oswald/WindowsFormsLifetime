using AppContext;
using WindowsFormsLifetime;

var builder = WebApplication.CreateBuilder(args);
// Pass in a factory lambda that constructs an ApplicationContext using the start form
builder.Host.UseWindowsFormsLifetime<ExampleApplicationContext, HiddenForm>(
    startForm => new ExampleApplicationContext(startForm));

var app = builder.Build();
app.Run();