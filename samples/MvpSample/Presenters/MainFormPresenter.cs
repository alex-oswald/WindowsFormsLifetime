using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class MainFormPresenter
    {
        private readonly IMainForm _mainForm;
        private readonly IEventService _eventService;
        private readonly NotesListPresenter _notesListPresenter;
        private readonly INotesListView _notesListView;
        private readonly NotePresenter _notePresenter;
        private readonly INoteView _noteView;

        public MainFormPresenter(
            ApplicationContext applicationContext,
            IEventService eventService,
            NotesListPresenter notesListPresenter,
            INotesListView notesListView,
            NotePresenter notePresenter,
            INoteView noteView)
        {
            _mainForm = applicationContext.MainForm as IMainForm;
            _eventService = eventService;
            _notesListPresenter = notesListPresenter;
            _notesListView = notesListView;
            _notePresenter = notePresenter;
            _noteView = noteView;

            _mainForm.Load += OnLoad;

            eventService.Subscribe<NoteCreatedEvent>(e =>
            {
                _mainForm.SetNoteViewVisibility(true);
            });
        }

        private void OnLoad(object? sender, EventArgs e)
        {
            _mainForm.SetNotesList(_notesListView);
            _mainForm.SetNoteView(_noteView);
            _mainForm.SetNoteViewVisibility(false);

            _eventService.Publish<RefreshListEvent>(new());
        }
    }
}