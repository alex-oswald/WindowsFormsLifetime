namespace MvpSample.Views
{
    partial class NotesListView
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
            this.NotesListBox = new System.Windows.Forms.ListBox();
            this.CreateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // NotesListBox
            // 
            this.NotesListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.NotesListBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NotesListBox.FormattingEnabled = true;
            this.NotesListBox.ItemHeight = 15;
            this.NotesListBox.Location = new System.Drawing.Point(0, 0);
            this.NotesListBox.Name = "NotesListBox";
            this.NotesListBox.Size = new System.Drawing.Size(232, 467);
            this.NotesListBox.TabIndex = 0;
            // 
            // CreateButton
            // 
            this.CreateButton.Location = new System.Drawing.Point(15, 481);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(109, 35);
            this.CreateButton.TabIndex = 1;
            this.CreateButton.Text = "Create Note";
            this.CreateButton.UseVisualStyleBackColor = true;
            // 
            // NotesListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CreateButton);
            this.Controls.Add(this.NotesListBox);
            this.Name = "NotesListView";
            this.Size = new System.Drawing.Size(232, 529);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox NotesListBox;
        private Button CreateButton;
    }
}
