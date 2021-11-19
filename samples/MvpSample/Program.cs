using MvpSample.Presenters;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<MainForm>()
    .AddMvpServices();
builder.Services.AddSingleton<MainFormPresenter>();

builder.Services.AddSingleton<INotesListView, NotesListView>();
builder.Services.AddSingleton<NotesListPresenter>();

builder.Services.AddSingleton<INoteView, NoteView>();
builder.Services.AddSingleton<NotePresenter>();

var app = builder.Build();
var mainFormPresenter = app.Services.GetService<MainFormPresenter>();
app.Run();