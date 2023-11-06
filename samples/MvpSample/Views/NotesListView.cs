using MvpSample.Data;
using WindowsFormsLifetime;

namespace MvpSample.Views;

public interface INotesListView
{
    void SetNotes(List<Note> notes);

    void SelectNote(Guid noteId);

    Note? SelectedNote { get; }

    UserControl This { get; }

    event EventHandler CreateNoteClicked;

    event EventHandler SelectedNoteChanged;
}

public partial class NotesListView : UserControl, INotesListView
{
    private readonly IGuiContext _guiContext;

    public NotesListView(IGuiContext guiContext)
    {
        InitializeComponent();
        _guiContext = guiContext;
    }

    public UserControl This => this;

    public void SetNotes(List<Note> notes)
    {
        _guiContext.Invoke(() =>
        {
            NotesListBox.DataSource = notes;
            NotesListBox.DisplayMember = nameof(Note.Notes);
            NotesListBox.ValueMember = nameof(Note.Id);
        });
    }

    public void SelectNote(Guid noteId)
    {
        _guiContext.Invoke(() =>
        {
            NotesListBox.SelectedValue = noteId;
        });
    }

    public Note? SelectedNote => NotesListBox.SelectedItem as Note;

    public event EventHandler CreateNoteClicked
    {
        add => CreateButton.Click += value;
        remove => CreateButton.Click -= value;
    }

    public event EventHandler SelectedNoteChanged
    {
        add => NotesListBox.SelectedValueChanged += value;
        remove => NotesListBox.SelectedValueChanged -= value;
    }
}