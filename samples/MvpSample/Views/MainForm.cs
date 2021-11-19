namespace MvpSample.Views
{
    public interface IMainForm
    {
        event EventHandler Load;

        event FormClosedEventHandler FormClosed;

        void SetNotesList(INotesListView notesListView);

        void SetNoteView(INoteView noteView);

        void SetNoteViewVisibility(bool visible);

        Form This { get; }
    }

    public partial class MainForm : Form, IMainForm
    {
        private INotesListView _notesListView;
        private INoteView _noteView;

        public MainForm()
        {
            InitializeComponent();
        }

        public Form This => this;

        public void SetNotesList(INotesListView notesListView)
        {
            _notesListView = notesListView;
            _notesListView.This.Dock = DockStyle.Fill;
            MenuPanel.Controls.Clear();
            MenuPanel.Controls.Add(_notesListView.This);
        }

        public void SetNoteView(INoteView noteView)
        {
            _noteView = noteView;
            _noteView.This.Dock = DockStyle.Fill;
            BodyPanel.Controls.Clear();
            BodyPanel.Controls.Add(_noteView.This);
        }

        public void SetNoteViewVisibility(bool visible)
        {
            BodyPanel.Visible = visible;
        }
    }
}