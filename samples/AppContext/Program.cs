using AppContext;
using WindowsFormsLifetime;

var builder = Host.CreateApplicationBuilder(args);
// Pass in a factory lambda that constructs an ApplicationContext using the start form
builder.Services.AddWindowsFormsLifetime<ExampleApplicationContext, HiddenForm>(
    startForm => new ExampleApplicationContext(startForm));

var app = builder.Build();
app.Run();