namespace MvpSample.Views
{
    partial class NoteView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NoteTextBox = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CreatedOnLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // NoteTextBox
            // 
            this.NoteTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NoteTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NoteTextBox.Location = new System.Drawing.Point(0, 0);
            this.NoteTextBox.Multiline = true;
            this.NoteTextBox.Name = "NoteTextBox";
            this.NoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.NoteTextBox.Size = new System.Drawing.Size(539, 299);
            this.NoteTextBox.TabIndex = 0;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(29, 345);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(113, 38);
            this.SaveButton.TabIndex = 1;
            this.SaveButton.Text = "Save Note";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // CreatedOnLabel
            // 
            this.CreatedOnLabel.AutoSize = true;
            this.CreatedOnLabel.Location = new System.Drawing.Point(29, 317);
            this.CreatedOnLabel.Name = "CreatedOnLabel";
            this.CreatedOnLabel.Size = new System.Drawing.Size(70, 15);
            this.CreatedOnLabel.TabIndex = 2;
            this.CreatedOnLabel.Text = "Created On:";
            // 
            // NoteView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CreatedOnLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.NoteTextBox);
            this.Name = "NoteView";
            this.Size = new System.Drawing.Size(539, 395);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox NoteTextBox;
        private Button SaveButton;
        private Label CreatedOnLabel;
    }
}
