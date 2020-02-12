using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SampleApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsLifetime.Sample
{
    public partial class Form1 : Form
    {
        private readonly ILogger<Form1> _logger;

        public Form1(ILogger<Form1> logger)
        {
            InitializeComponent();
            _logger = logger;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("Show Form2");
            new Form2().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _logger.LogInformation("Close Form1");
            this.Close();
        }
    }
}