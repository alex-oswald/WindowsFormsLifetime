namespace MvpSample.Views
{
    public interface INotesListView
    {
        void AddNote(Note note);

        UserControl This { get; }

        event EventHandler CreateNoteClicked;
    }

    public partial class NotesListView : UserControl, INotesListView
    {
        public NotesListView()
        {
            InitializeComponent();
            NotesListBox.DisplayMember = nameof(Note.Summary);
        }

        public UserControl This => this;

        public void AddNote(Note note)
        {
            NotesListBox.Items.Add(note);
        }

        public event EventHandler CreateNoteClicked
        {
            add => CreateButton.Click += value;
            remove => CreateButton.Click -= value;
        }
    }
}