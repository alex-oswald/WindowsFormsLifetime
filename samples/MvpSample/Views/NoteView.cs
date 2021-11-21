using MvpSample.Data;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace MvpSample.Views
{
    public interface INoteView
    {
        void SetNote(Note note);

        string GetNoteText();

        UserControl This { get; }

        event EventHandler SaveNoteClicked;

        event EventHandler DeleteNoteClicked;
    }

    public partial class NoteView : UserControl, INoteView
    {
        private readonly IGuiContext _guiContext;

        public NoteView(IGuiContext guiContext)
        {
            InitializeComponent();
            _guiContext = guiContext;

            DeleteNoteClicked += OnDeleteNoteClicked;
        }

        private void OnDeleteNoteClicked(object? sender, EventArgs e)
        {
            NoteTextBox.Text = null;
            CreatedOnValueLabel.Text = null;
        }

        public UserControl This => this;

        public void SetNote(Note note)
        {
            _guiContext.Invoke(() =>
            {
                NoteTextBox.Text = note.Notes;
                CreatedOnValueLabel.Text = note.CreatedOn.ToString("G");
            });
        }

        public string GetNoteText() => NoteTextBox.Text;

        public event EventHandler SaveNoteClicked
        {
            add => SaveButton.Click += value;
            remove => SaveButton.Click -= value;
        }

        public event EventHandler DeleteNoteClicked
        {
            add => DeleteButton.Click += value;
            remove => DeleteButton.Click -= value;
        }
    }
}