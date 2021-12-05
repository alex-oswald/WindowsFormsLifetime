namespace MvpBasicSample
{
    public interface IMainView
    {
        int Count { get; set; }

        event EventHandler OnIncrementClicked;
    }

    public partial class MainForm : Form, IMainView
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public int Count
        {
            get => Convert.ToInt32(CountLabel.Text);
            set => CountLabel.Text = $"{value}";
        }

        public event EventHandler OnIncrementClicked
        {
            add => IncrementButton.Click += value;
            remove => IncrementButton.Click -= value;
        }
    }
}