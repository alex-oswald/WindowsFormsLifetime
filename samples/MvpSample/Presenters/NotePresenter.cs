using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime.Mvp;

namespace MvpSample.Presenters
{
    internal class NotePresenter
    {
        private readonly INoteView _view;
        private readonly IEventService _eventService;
        private Note? _note;

        public NotePresenter(
            INoteView view,
            IEventService eventService)
        {
            _view = view;
            _eventService = eventService;

            _view.SaveNoteClicked += OnSaveNoteClicked;

            _eventService.SubscribeAsync<CreateNoteEvent>(e =>
            {
                _note = new Note();
                _view.SetNote(_note);
            });
        }

        private void OnSaveNoteClicked(object? sender, EventArgs e)
        {
            
        }
    }
}