using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;

namespace WindowsFormsLifetime.Mvp
{
    public class MvpUserControlBase : UserControl
    {
        public MvpUserControlBase(
            IGuiContext guiContext)
        {
            //InitializeComponent();
            GuiContext = guiContext;
        }

        protected IGuiContext GuiContext { get; }
    }
}
