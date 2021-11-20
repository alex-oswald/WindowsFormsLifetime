using Microsoft.EntityFrameworkCore;
using MvpSample.Data;
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

builder.Services.AddDbContext<InMemoryDbContext>(options =>
    options.UseSqlite(InMemoryDbContext.CreateConnection()));

var app = builder.Build();

// Create the database
var db = app.Services.GetService<InMemoryDbContext>();
db?.Database.EnsureCreated();

// Get the main presenter to instantiate it
var mainFormPresenter = app.Services.GetService<MainFormPresenter>();
app.Run();