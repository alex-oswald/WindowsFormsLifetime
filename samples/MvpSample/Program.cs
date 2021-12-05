using Microsoft.EntityFrameworkCore;
using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Presenters;
using MvpSample.Views;
using WindowsFormsLifetime;
using WindowsFormsLifetime.Mvp;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsFormsLifetime<MainForm, IMainForm, MainFormPresenter>();

builder.Services.AddPresenterWithView<INoteView, NoteView, NotePresenter>();
builder.Services.AddPresenterWithView<INotesListView, NotesListView, NotesListPresenter>();

builder.Services.AddSingleton<IEventService, EventService>();

builder.Services.AddScoped<IRepository<Note>, EntityFrameworkRepository<Note, SqliteDbContext>>();
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Create the database
var db = app.Services.GetService<SqliteDbContext>();
db?.Database.EnsureCreated();

app.Run();