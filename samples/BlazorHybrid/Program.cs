using BlazorHybrid;
using MudBlazor.Services;
using WindowsFormsLifetime;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>();
builder.Services.AddWindowsFormsBlazorWebView();
builder.Services.AddMudServices();

var app = builder.Build();
app.Run();