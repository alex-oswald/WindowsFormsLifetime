using MvpBasicSample;
using WindowsFormsLifetime.Mvp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<MainForm, IMainView, MainFormPresenter>();
var app = builder.Build();
app.Run();