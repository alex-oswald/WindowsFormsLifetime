using MvpSample.Data;
using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace MvpSample.Views
{
    public interface INoteView
    {
        void SetNote(Note note);

        string GetNoteText();

        bool SaveButtonEnabled { get; set; }

        bool DeleteButtonEnabled { get; set; }

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

        public bool SaveButtonEnabled
        {
            get => SaveButton.Enabled;
            set => _guiContext.Invoke(() => SaveButton.Enabled == value);
        }

        public bool DeleteButtonEnabled
        {
            get => DeleteButton.Enabled;
            set => _guiContext.Invoke(() => DeleteButton.Enabled == value);
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