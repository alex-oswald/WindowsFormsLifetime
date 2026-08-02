using BlazorHybrid;
using MudBlazor.Services;
using WindowsFormsLifetime;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<Form1>();
builder.Services.AddWindowsFormsBlazorWebView();
builder.Services.AddMudServices();

WebApplication app = builder.Build();
app.Run();