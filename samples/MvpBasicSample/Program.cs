using Microsoft.Extensions.Hosting;
using MvpBasicSample;
using WindowsFormsLifetime.Mvp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsFormsLifetime<MainForm, IMainView, MainFormPresenter>();

var app = builder.Build();
app.Run();