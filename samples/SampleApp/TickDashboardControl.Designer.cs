namespace SampleApp;

partial class TickDashboardControl
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        tickStatusControl = new TickStatusControl();
        SuspendLayout();
        //
        // tickStatusControl
        //
        tickStatusControl.Dock = DockStyle.Fill;
        tickStatusControl.Location = new Point(0, 0);
        tickStatusControl.Name = "tickStatusControl";
        tickStatusControl.Size = new Size(400, 72);
        tickStatusControl.TabIndex = 0;
        //
        // TickDashboardControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(tickStatusControl);
        Name = "TickDashboardControl";
        Size = new Size(400, 72);
        ResumeLayout(false);
    }

    #endregion

    private TickStatusControl tickStatusControl;
}
