﻿using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class NotePresenter
    {
        private readonly INoteView _view;
        private readonly IEventService _eventService;
        private readonly InMemoryDbContext _appDbContext;
        private Note? _currentNote = null;
        private bool _saved = false;

        public NotePresenter(
            INoteView view,
            IEventService eventService,
            InMemoryDbContext appDbContext)
        {
            _view = view;
            _eventService = eventService;
            _appDbContext = appDbContext;

            _view.SaveNoteClicked += OnSaveNoteClicked;

            _eventService.Subscribe<NoteCreatedEvent>(e =>
            {
                _saved = false;
                _currentNote = new Note();
                _view.SetNote(_currentNote);
            });

            _eventService.Subscribe<SelectedNoteChangedEvent>(e =>
            {
                _currentNote = e.SelectedNote;
                if (_currentNote is not null)
                {
                    _view.SetNote(_currentNote);
                }
            });
        }

        private void OnSaveNoteClicked(object? sender, EventArgs e)
        {
            _currentNote.Notes = _view.GetNoteText();
            _appDbContext.Notes.Update(_currentNote);
            _appDbContext.SaveChanges();
            _eventService.Publish<RefreshListEvent>(new(_currentNote));
        }
    }
}