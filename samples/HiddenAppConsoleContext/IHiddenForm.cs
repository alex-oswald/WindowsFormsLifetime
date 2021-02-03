using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HiddenAppConsoleContext
{
    public interface IHiddenForm
    {
        Form Form { get; }
        Task ExecuteAsync();
        Task ConnectAsync();
        Task DisconnectAsync();
        void OnHiddenFormClosing(object sender, CancelEventArgs e);
        void OnHiddenFormLoad(object? sender, EventArgs e);
    }
}