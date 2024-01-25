using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp;
using WindowsFormsLifetime;

var builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<Form1>();
builder.Services.AddHostedService<FormSpawnHostedService>();
builder.Services.AddHostedService<TickingHostedService>();
builder.Services.AddTransient<Form2>();

var app = builder.Build();
app.Run();