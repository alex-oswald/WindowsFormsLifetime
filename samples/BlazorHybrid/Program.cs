using BlazorHybrid;
using MudBlazor.Services;
using WindowsFormsLifetime;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsFormsLifetime<Form1>();
builder.Services.AddWindowsFormsBlazorWebView();
builder.Services.AddMudServices();

var app = builder.Build();
app.Run();