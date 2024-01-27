using MvpSample.Data;
using MvpSample.Events;
using MvpSample.Views;
using WindowsFormsLifetime;

namespace MvpSample.Presenters;

internal class NotePresenter
{
    private readonly ILogger<NotePresenter> _logger;
    private readonly INoteView _view;
    private readonly IEventService _eventService;
    private readonly IRepository<Note> _noteRepository;
    private readonly IGuiContext _guiContext;
    private Note? _currentNote = null;
    private bool _saved = false;

    public NotePresenter(
        ILogger<NotePresenter> logger,
        INoteView view,
        IEventService eventService,
        IRepository<Note> noteRepository,
        IGuiContext guiContext)
    {
        _logger = logger;
        _view = view;
        _eventService = eventService;
        _noteRepository = noteRepository;
        _guiContext = guiContext;

        _view.SaveNoteClicked += OnSaveNoteClicked;
        _view.DeleteNoteClicked += OnDeleteNoteClicked;

        _eventService.Subscribe<NoteCreatedEvent>(e =>
        {
            _currentNote = new Note();
            _logger.LogInformation(nameof(NoteCreatedEvent));
            _view.SetNote(_currentNote);
            _saved = false;
            _view.SaveButtonEnabled = true;
            _view.DeleteButtonEnabled = false;
        });

        _eventService.Subscribe<SelectedNoteChangedEvent>(e =>
        {
            _currentNote = e.SelectedNote;
            _logger.LogInformation(nameof(SelectedNoteChangedEvent));
            if (_currentNote is not null)
            {
                _logger.LogWarning("current not is not null");
                _view.SetNote(_currentNote);
                _saved = true;
                _view.SaveButtonEnabled = false;
                _view.DeleteButtonEnabled = true;
            }
        });
    }

    private async void OnSaveNoteClicked(object? sender, EventArgs e)
    {
        if (_currentNote is null) return;

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

    private async void OnDeleteNoteClicked(object? sender, EventArgs e)
    {
        if (_currentNote is null) return;

        await _guiContext.InvokeAsync(async () =>
        {
            DialogResult msg = MessageBox.Show("Delete selected note?", "Delete Note", MessageBoxButtons.YesNo);
            if (msg == DialogResult.Yes)
            {
                var current = await _noteRepository.GetAsync(_currentNote.Id);
                await _noteRepository.DeleteAsync(current);
                _currentNote = null;
                _eventService.Publish<RefreshListEvent>(new());
            }
        });
    }
}