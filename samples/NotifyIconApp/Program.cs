using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotifyIconApp;
using WindowsFormsLifetime;

var builder = Host.CreateDefaultBuilder(args);

builder.UseWindowsFormsLifetime<NotifyIconAppContext>();

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
    services.AddHostedService<Worker>();
});

var app = builder.Build();

app.Run();
