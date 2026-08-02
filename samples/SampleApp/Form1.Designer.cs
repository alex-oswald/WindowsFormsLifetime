namespace SampleApp
{
    partial class Form1
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
            button1 = new Button();
            button2 = new Button();
            ThreadLabel = new Label();
            TickLabel = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(150, 63);
            button1.TabIndex = 0;
            button1.Text = "Open Form2";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(168, 12);
            button2.Name = "button2";
            button2.Size = new Size(150, 63);
            button2.TabIndex = 1;
            button2.Text = "Exit";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // ThreadLabel
            // 
            ThreadLabel.AutoSize = true;
            ThreadLabel.Location = new Point(49, 108);
            ThreadLabel.Name = "ThreadLabel";
            ThreadLabel.Size = new Size(46, 15);
            ThreadLabel.TabIndex = 2;
            ThreadLabel.Text = "Thread:";
            // 
            // TickLabel
            // 
            TickLabel.AutoSize = true;
            TickLabel.Location = new Point(49, 147);
            TickLabel.Name = "TickLabel";
            TickLabel.Size = new Size(31, 15);
            TickLabel.TabIndex = 3;
            TickLabel.Text = "Tick:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(446, 244);
            Controls.Add(TickLabel);
            Controls.Add(ThreadLabel);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label ThreadLabel;
        private Label TickLabel;
    }
}

