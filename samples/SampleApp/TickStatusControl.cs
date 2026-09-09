using Microsoft.Extensions.Logging;
using System.ComponentModel;
using WindowsFormsLifetime;

namespace SampleApp;

public partial class TickStatusControl : UserControl, IOnServicesInjected
{
    public TickStatusControl()
    {
        InitializeComponent();
    }

    [InjectService]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TickBag TickBag { get; set; } = null!;

    [InjectService]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ILogger<TickStatusControl> Logger { get; set; } = null!;

    public void OnServicesInjected()
    {
        UpdateTick();
        refreshButton.Enabled = true;
    }

    private void RefreshButton_Click(object? sender, EventArgs e)
    {
        UpdateTick();
        Logger.LogInformation("Refreshed the nested user control on UI thread {ThreadId}", Environment.CurrentManagedThreadId);
    }

    private void UpdateTick()
    {
        tickLabel.Text = $"Nested control tick: {TickBag.CurrentTick}";
    }
}
