namespace SampleApp;

partial class TickStatusControl
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
        tickLabel = new Label();
        refreshButton = new Button();
        SuspendLayout();
        //
        // tickLabel
        //
        tickLabel.AutoSize = true;
        tickLabel.Location = new Point(8, 12);
        tickLabel.Name = "tickLabel";
        tickLabel.Size = new Size(177, 15);
        tickLabel.TabIndex = 0;
        tickLabel.Text = "Nested control: services not ready";
        //
        // refreshButton
        //
        refreshButton.Enabled = false;
        refreshButton.Location = new Point(8, 34);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(140, 28);
        refreshButton.TabIndex = 1;
        refreshButton.Text = "Refresh nested control";
        refreshButton.UseVisualStyleBackColor = true;
        refreshButton.Click += RefreshButton_Click;
        //
        // TickStatusControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(refreshButton);
        Controls.Add(tickLabel);
        Name = "TickStatusControl";
        Size = new Size(400, 72);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label tickLabel;
    private Button refreshButton;
}
