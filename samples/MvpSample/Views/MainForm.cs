using WindowsFormsLifetime;

namespace MvpSample.Views;

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
    private readonly IGuiContext _guiContext;
    private INotesListView? _notesListView ;
    private INoteView? _noteView;

    public MainForm(IGuiContext guiContext)
    {
        InitializeComponent();
        _guiContext = guiContext;
    }

    public Form This => this;

    public void SetNotesList(INotesListView notesListView)
    {
        _guiContext.Invoke(() =>
        {
            _notesListView = notesListView;
            _notesListView.This.Dock = DockStyle.Fill;
            MenuPanel.Controls.Clear();
            MenuPanel.Controls.Add(_notesListView.This);
        });
    }

    public void SetNoteView(INoteView noteView)
    {
        _guiContext.Invoke(() =>
        {
            _noteView = noteView;
            _noteView.This.Dock = DockStyle.Fill;
            BodyPanel.Controls.Clear();
            BodyPanel.Controls.Add(_noteView.This);
        });
    }

    public void SetNoteViewVisibility(bool visible)
    {
        _guiContext.Invoke(() =>
        {
            BodyPanel.Visible = visible;
        });
    }
}