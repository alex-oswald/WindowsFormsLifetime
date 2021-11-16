using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace SampleApp
{
    public partial class Form2 : Form
    {
        private readonly IFormProvider _formProvider;

        public Form2(IFormProvider formProvider)
        {
            InitializeComponent();
            _formProvider = formProvider;

            ThreadLabel.Text = $"{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                this.Invoke(new Action(() =>
                {
                    button1.Text = new Random().Next(1, 10).ToString();
                }));
            });
        }
    }
}