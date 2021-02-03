using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace HiddenAppConsoleContext
{
    public class HiddenContext : ApplicationContext
    {
        private readonly ILogger<HiddenContext> _logger;

        public HiddenContext(ILogger<HiddenContext> logger, IHiddenForm hiddenForm)
        {
            _logger = logger;
            MainForm = hiddenForm.Form;
            ThreadExit += OnThreadExit;
            ApplicationContextLoaded();
        }

        private void OnThreadExit(object? sender, EventArgs e)
        {
            _logger.LogInformation("Application Context Thread Exited");
        }

        private void ApplicationContextLoaded()
        {
            _logger.LogInformation("Application Context Loaded");
        }
    }
}