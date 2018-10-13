namespace VisualWindowsManager
{
    partial class Main
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.renderPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // renderPanel
            // 
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(945, 504);
            this.renderPanel.TabIndex = 1;
            this.renderPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.renderPanel_Paint);
            this.renderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseDown);
            this.renderPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseUp);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 504);
            this.Controls.Add(this.renderPanel);
            this.KeyPreview = true;
            this.Name = "Main";
            this.Text = "Visual Windows Commander";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel renderPanel;
    }
}

