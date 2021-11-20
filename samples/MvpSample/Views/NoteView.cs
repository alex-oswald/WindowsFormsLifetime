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
    }

    public partial class NoteView : UserControl, INoteView
    {
        private readonly IGuiContext _guiContext;

        public NoteView(IGuiContext guiContext)
        {
            InitializeComponent();
            _guiContext = guiContext;
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
    }
}