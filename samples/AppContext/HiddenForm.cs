using Microsoft.Extensions.Logging;

namespace AppContext;

public partial class HiddenForm : Form
{
    public HiddenForm(ILogger<HiddenForm> logger)
    {
        logger.LogInformation("HiddenForm constructor invoked");
    }
}