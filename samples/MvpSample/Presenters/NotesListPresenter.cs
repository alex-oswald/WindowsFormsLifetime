﻿using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime;

namespace MvpSample.Presenters
{
    internal class NotesListPresenter
    {
        private readonly ILogger<NotesListPresenter> _logger;
        private readonly INotesListView _view;
        private readonly IEventService _eventService;
        private readonly IRepository<Note> _noteRepository;
        private readonly IGuiContext _guiContext;

        public NotesListPresenter(
            ILogger<NotesListPresenter> logger,
            INotesListView view,
            IEventService eventService,
            IRepository<Note> noteRepository,
            IGuiContext guiContext)
        {
            _logger = logger;
            _view = view;
            _eventService = eventService;
            _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
            _guiContext = guiContext;

            _view.CreateNoteClicked += OnCreateNoteClicked;
            _view.SelectedNoteChanged += OnSelectedNoteChanged;

            _eventService.Subscribe<RefreshListEvent>(OnRefreshList);
        }

        private void OnSelectedNoteChanged(object? sender, EventArgs e)
        {
            _logger.LogInformation(nameof(OnSelectedNoteChanged));
            var selectedNote = _view.SelectedNote;
            _eventService.Publish<SelectedNoteChangedEvent>(new(selectedNote));
        }

        private void OnCreateNoteClicked(object? sender, EventArgs e)
        {
            _logger.LogInformation(nameof(OnCreateNoteClicked));

            _guiContext.Invoke(() =>
            {
                var result = MessageBox.Show("Create a new note?", "Create Note", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Deselect note before creating
                    _view.SelectNote(Guid.Empty);
                    _eventService.Publish<NoteCreatedEvent>(new());
                }
            });
        }

        private async void OnRefreshList(RefreshListEvent e)
        {
            _logger.LogInformation(nameof(OnRefreshList));
            var notes = (await _noteRepository.GetAllAsync())?.ToList() ?? new();
            _view.SetNotes(notes);
            if (e.SelectedNote is not null)
            {
                _view.SelectNote(e.SelectedNote.Id);
            }
        }
    }
}