using SampleApp;
using WindowsFormsLifetime;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>();
builder.Services.AddHostedService<FormSpawnHostedService>();
builder.Services.AddHostedService<TickingHostedService>();
builder.Services.AddTransient<Form2>();

var app = builder.Build();
app.Run();