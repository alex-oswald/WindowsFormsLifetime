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
            this.NoteTextBox.Multiline = true;
            this.NoteTextBox.Name = "NoteTextBox";
            this.NoteTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.NoteTextBox.Size = new System.Drawing.Size(559, 290);
            this.NoteTextBox.TabIndex = 0;
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SaveButton.Location = new System.Drawing.Point(15, 306);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(109, 35);
            this.SaveButton.TabIndex = 1;
            this.SaveButton.Text = "Save Note";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // CreatedOnLabel
            // 
            this.CreatedOnLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreatedOnLabel.AutoSize = true;
            this.CreatedOnLabel.Location = new System.Drawing.Point(295, 316);
            this.CreatedOnLabel.Name = "CreatedOnLabel";
            this.CreatedOnLabel.Size = new System.Drawing.Size(70, 15);
            this.CreatedOnLabel.TabIndex = 2;
            this.CreatedOnLabel.Text = "Created On:";
            // 
            // CreatedOnValueLabel
            // 
            this.CreatedOnValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CreatedOnValueLabel.AutoSize = true;
            this.CreatedOnValueLabel.Location = new System.Drawing.Point(371, 316);
            this.CreatedOnValueLabel.Name = "CreatedOnValueLabel";
            this.CreatedOnValueLabel.Size = new System.Drawing.Size(0, 15);
            this.CreatedOnValueLabel.TabIndex = 3;
            // 
            // DeleteButton
            // 
            this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DeleteButton.Location = new System.Drawing.Point(130, 306);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(109, 35);
            this.DeleteButton.TabIndex = 2;
            this.DeleteButton.Text = "Delete Note";
            this.DeleteButton.UseVisualStyleBackColor = true;
            // 
            // NoteView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.CreatedOnValueLabel);
            this.Controls.Add(this.CreatedOnLabel);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.NoteTextBox);
            this.Name = "NoteView";
            this.Size = new System.Drawing.Size(559, 356);
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
