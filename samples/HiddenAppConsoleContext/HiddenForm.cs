using System.Windows.Forms;

namespace HiddenAppConsoleContext
{
    public class HiddenForm : Form
    {
        protected override CreateParams CreateParams => base.CreateParams.Hidden();
    }
}