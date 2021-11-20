using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class NotesListPresenter
    {
        private readonly ILogger<NotesListPresenter> _logger;
        private readonly INotesListView _view;
        private readonly IEventService _eventService;
        private readonly InMemoryDbContext _appDbContext;

        public NotesListPresenter(
            ILogger<NotesListPresenter> logger,
            INotesListView view,
            IEventService eventService,
            InMemoryDbContext appDbContext)
        {
            _logger = logger;
            _view = view;
            _eventService = eventService;
            _appDbContext = appDbContext;

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
            _eventService.Publish<NoteCreatedEvent>(new());
        }

        private void OnRefreshList(RefreshListEvent e)
        {
            _logger.LogInformation(nameof(OnRefreshList));
            var notes = _appDbContext.Notes.ToList();
            _view.SetNotes(notes);
            if (e.SelectedNote is not null)
            {
                _view.SelectNote(e.SelectedNote.Id);
            }
        }
    }
}