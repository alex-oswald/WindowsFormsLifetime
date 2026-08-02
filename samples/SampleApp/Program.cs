using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleApp;
using WindowsFormsLifetime;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.UseWindowsFormsLifetime<Form1>();
builder.Services.AddHostedService<FormSpawnHostedService>();
builder.Services.AddHostedService<TickingHostedService>();
builder.Services.AddTransient<Form2>();
builder.Services.AddSingleton<TickBag>();

IHost app = builder.Build();
app.Run();