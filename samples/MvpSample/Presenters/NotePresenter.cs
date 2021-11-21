using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Views;
using System.Text.Json;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class NotePresenter
    {
        private readonly ILogger<NotePresenter> _logger;
        private readonly INoteView _view;
        private readonly IEventService _eventService;
        private readonly IRepository<Note> _noteRepository;
        private Note? _currentNote = null;

        public NotePresenter(
            ILogger<NotePresenter> logger,
            INoteView view,
            IEventService eventService,
            IRepository<Note> noteRepository)
        {
            _logger = logger;
            _view = view;
            _eventService = eventService;
            _noteRepository = noteRepository;

            _view.SaveNoteClicked += OnSaveNoteClicked;
            _view.DeleteNoteClicked += OnDeleteNoteClicked;

            _eventService.Subscribe<NoteCreatedEvent>(e =>
            {
                _currentNote = new Note();
                _logger.LogInformation(nameof(NoteCreatedEvent), JsonSerializer.Serialize(_currentNote));
                _view.SetNote(_currentNote);
            });

            _eventService.Subscribe<SelectedNoteChangedEvent>(e =>
            {
                _currentNote = e.SelectedNote;
                _logger.LogInformation(nameof(SelectedNoteChangedEvent), JsonSerializer.Serialize(_currentNote));
                if (_currentNote is not null)
                {
                    _view.SetNote(_currentNote);
                }
            });
        }

        private async void OnSaveNoteClicked(object? sender, EventArgs e)
        {
            if (_currentNote is not null)
            {
                var current = await _noteRepository.GetAsync(_currentNote.Id);
                // If it doesnt exist, create
                if (current is null)
                {
                    _currentNote.Notes = _view.GetNoteText();
                    await _noteRepository.InsertAsync(_currentNote);
                }
                else
                {
                    current.Notes = _view.GetNoteText();
                    _currentNote = current;
                    await _noteRepository.UpdateAsync(_currentNote);
                }
                _eventService.Publish<RefreshListEvent>(new(_currentNote));
            }
        }

        private async void OnDeleteNoteClicked(object? sender, EventArgs e)
        {
            if (_currentNote is not null)
            {
                var current = await _noteRepository.GetAsync(_currentNote.Id);
                await _noteRepository.DeleteAsync(current);
                _currentNote = null;
                _eventService.Publish<RefreshListEvent>(new());
            }
        }
    }
}