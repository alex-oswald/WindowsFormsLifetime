namespace MvpSample.Views
{
    public interface INoteView
    {
        void SetNote(Note note);

        UserControl This { get; }

        event EventHandler SaveNoteClicked;
    }

    public partial class NoteView : UserControl, INoteView
    {
        public NoteView()
        {
            InitializeComponent();
        }

        public UserControl This => this;

        public void SetNote(Note note)
        {
            NoteTextBox.Text = note.Notes;
            CreatedOnLabel.Text = $"Created On: {note.CreatedOn:G}";
        }

        public event EventHandler SaveNoteClicked
        {
            add => SaveButton.Click += value;
            remove => SaveButton.Click -= value;
        }
    }
}