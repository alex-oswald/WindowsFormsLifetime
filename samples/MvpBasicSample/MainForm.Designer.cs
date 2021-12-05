namespace MvpBasicSample
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.IncrementButton = new System.Windows.Forms.Button();
            this.CountLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // IncrementButton
            // 
            this.IncrementButton.Location = new System.Drawing.Point(70, 80);
            this.IncrementButton.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.IncrementButton.Name = "IncrementButton";
            this.IncrementButton.Size = new System.Drawing.Size(129, 46);
            this.IncrementButton.TabIndex = 0;
            this.IncrementButton.Text = "Increment";
            this.IncrementButton.UseVisualStyleBackColor = true;
            // 
            // CountLabel
            // 
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new System.Drawing.Point(283, 88);
            this.CountLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(78, 30);
            this.CountLabel.TabIndex = 1;
            this.CountLabel.Text = "{count}";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 30F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 267);
            this.Controls.Add(this.CountLabel);
            this.Controls.Add(this.IncrementButton);
            this.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button IncrementButton;
        private Label CountLabel;
    }
}