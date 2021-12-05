using WindowsFormsLifetime;

namespace SampleApp
{
    public partial class Form1 : Form
    {
        private readonly ILogger<Form1> _logger;
        private readonly IFormProvider _formProvider;

        public Form1(ILogger<Form1> logger, IFormProvider formProvider)
        {
            InitializeComponent();
            _logger = logger;
            _formProvider = formProvider;

            ThreadLabel.Text = $"{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("Show");
            var form = await _formProvider.GetFormAsync<Form2>();
            form.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("Close");
            this.Close();
        }
    }
}