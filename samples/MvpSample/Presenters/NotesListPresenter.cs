using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class NotesListPresenter
    {
        private readonly INotesListView _view;
        private readonly IEventService _eventService;

        public NotesListPresenter(
            INotesListView view,
            IEventService eventService)
        {
            _view = view;
            _eventService = eventService;

            _view.CreateNoteClicked += OnCreateNoteClicked;
        }

        private void OnCreateNoteClicked(object? sender, EventArgs e)
        {
            _eventService.PublishAsync<CreateNoteEvent>(new());
        }
    }
}