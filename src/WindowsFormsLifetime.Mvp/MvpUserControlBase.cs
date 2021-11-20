using OswaldTechnologies.Extensions.Hosting.WindowsFormsLifetime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
