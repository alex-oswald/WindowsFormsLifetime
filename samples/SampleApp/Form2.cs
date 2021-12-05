using WindowsFormsLifetime;

namespace SampleApp
{
    public partial class Form2 : Form
    {
        private readonly ILogger<Form2> _logger;
        private readonly IGuiContext _guiContext;

        public Form2(
            ILogger<Form2> logger,
            IGuiContext guiContext)
        {
            InitializeComponent();
            _logger = logger;
            _guiContext = guiContext;

            ThreadLabel.Text = $"{Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.Name}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Running a task on the thread pool means that to update a control on the form,
            // we must invoke on the thread that created the control, or we get a cross thread exception
            // Use IGuiContext to invoke actions on the gui thread
            Task.Run(() =>
            {
                _logger.LogInformation($"Task.Run Thread {Thread.CurrentThread.ManagedThreadId}  {Thread.CurrentThread.Name}");
                _guiContext.Invoke(new Action(() =>
                {
                    _logger.LogInformation($"GuiContext Thread {Thread.CurrentThread.ManagedThreadId}  {Thread.CurrentThread.Name}");
                    button1.Text = new Random().Next(1, 10).ToString();
                }));
            });
        }
    }
}