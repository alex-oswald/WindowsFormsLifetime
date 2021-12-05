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
            this.CreatedOnValueLabel = new System.Windows.Forms.Label();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NoteTextBox
            // 
            this.NoteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NoteTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NoteTextBox.Location = new System.Drawing.Point(0, 0);
            this.NoteTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.NoteTextBox.Multiline = true;
            this.NoteTextBox.Name = "NoteTextBox";
            this.NoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.NoteTextBox.Size = new System.Drawing.Size(1113, 930);
            this.NoteTextBox.TabIndex = 0;
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveButton.Location = new System.Drawing.Point(28, 960);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(6);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(202, 75);
            this.SaveButton.TabIndex = 1;
            this.SaveButton.Text = "Save Note";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // CreatedOnLabel
            // 
            this.CreatedOnLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreatedOnLabel.AutoSize = true;
            this.CreatedOnLabel.Location = new System.Drawing.Point(548, 981);
            this.CreatedOnLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.CreatedOnLabel.Name = "CreatedOnLabel";
            this.CreatedOnLabel.Size = new System.Drawing.Size(141, 32);
            this.CreatedOnLabel.TabIndex = 2;
            this.CreatedOnLabel.Text = "Created On:";
            // 
            // CreatedOnValueLabel
            // 
            this.CreatedOnValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreatedOnValueLabel.AutoSize = true;
            this.CreatedOnValueLabel.Location = new System.Drawing.Point(689, 981);
            this.CreatedOnValueLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.CreatedOnValueLabel.Name = "CreatedOnValueLabel";
            this.CreatedOnValueLabel.Size = new System.Drawing.Size(0, 32);
            this.CreatedOnValueLabel.TabIndex = 3;
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteButton.Location = new System.Drawing.Point(241, 960);
            this.DeleteButton.Margin = new System.Windows.Forms.Padding(6);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(202, 75);
            this.DeleteButton.TabIndex = 2;
            this.DeleteButton.Text = "Delete Note";
            this.DeleteButton.UseVisualStyleBackColor = true;
            // 
            // NoteView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.CreatedOnValueLabel);
            this.Controls.Add(this.CreatedOnLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.NoteTextBox);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "NoteView";
            this.Size = new System.Drawing.Size(1114, 1067);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox NoteTextBox;
        private Button SaveButton;
        private Label CreatedOnLabel;
        private Label CreatedOnValueLabel;
        private Button DeleteButton;
    }
}
