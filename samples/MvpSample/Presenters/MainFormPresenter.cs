using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class MainFormPresenter : BaseMainFormPresenter<MainForm, IMainForm>
    {
        private readonly ILogger<MainFormPresenter> _logger;
        private readonly IEventService _eventService;
        private readonly NotesListPresenter _notesListPresenter;
        private readonly INotesListView _notesListView;
        private readonly NotePresenter _notePresenter;
        private readonly INoteView _noteView;

        public MainFormPresenter(
            ApplicationContext applicationContext,
            ILogger<MainFormPresenter> logger,
            IEventService eventService,
            NotesListPresenter notesListPresenter,
            INotesListView notesListView,
            NotePresenter notePresenter,
            INoteView noteView)
            : base(applicationContext)
        {
            _logger = logger;
            _eventService = eventService;
            _notesListPresenter = notesListPresenter;
            _notesListView = notesListView;
            _notePresenter = notePresenter;
            _noteView = noteView;

            View.Load += OnLoad;

            eventService.Subscribe<NoteCreatedEvent>(e =>
            {
                View.SetNoteViewVisibility(true);
            });

            eventService.Subscribe<SelectedNoteChangedEvent>(e =>
            {
                View.SetNoteViewVisibility(e.SelectedNote is not null);
            });
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            _logger.LogInformation(nameof(OnLoad));
            View.SetNotesList(_notesListView);
            View.SetNoteView(_noteView);
            View.SetNoteViewVisibility(false);

            _eventService.Publish<RefreshListEvent>(new());
        }
    }
}