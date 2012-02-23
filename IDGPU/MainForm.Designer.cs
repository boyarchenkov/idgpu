namespace IDGPU
{
    partial class MainForm
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
            this.textBoxOut = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.buttonPause = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.status = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxOut
            // 
            this.textBoxOut.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOut.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxOut.Location = new System.Drawing.Point(23, 0);
            this.textBoxOut.Multiline = true;
            this.textBoxOut.Name = "textBoxOut";
            this.textBoxOut.ReadOnly = true;
            this.textBoxOut.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxOut.Size = new System.Drawing.Size(587, 104);
            this.textBoxOut.TabIndex = 1;
            this.textBoxOut.DoubleClick += new System.EventHandler(this.textBoxOut_DoubleClick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Experiment files|*.xp|All files|*.*";
            this.openFileDialog.RestoreDirectory = true;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Experiment files|*.xp|All files|*.*";
            this.saveFileDialog1.RestoreDirectory = true;
            // 
            // buttonPause
            // 
            this.buttonPause.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonPause.Location = new System.Drawing.Point(0, 0);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(23, 104);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.Text = "=";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.status});
            this.statusStrip1.Location = new System.Drawing.Point(0, 104);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(610, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // status
            // 
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 126);
            this.Controls.Add(this.textBoxOut);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(450, 160);
            this.Name = "MainForm";
            this.Text = "MD - Loading...";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormClosed);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxOut;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel status;
    }
}