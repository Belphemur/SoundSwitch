namespace SoundSwitch.Forms
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstDevices = new System.Windows.Forms.CheckedListBox();
            this.RunAtStartup = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHotkey = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstDevices);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(355, 155);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Devices selection";
            // 
            // lstDevices
            // 
            this.lstDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstDevices.FormattingEnabled = true;
            this.lstDevices.Location = new System.Drawing.Point(3, 16);
            this.lstDevices.Margin = new System.Windows.Forms.Padding(10);
            this.lstDevices.Name = "lstDevices";
            this.lstDevices.Size = new System.Drawing.Size(349, 136);
            this.lstDevices.TabIndex = 12;
            // 
            // RunAtStartup
            // 
            this.RunAtStartup.AutoSize = true;
            this.RunAtStartup.Location = new System.Drawing.Point(12, 200);
            this.RunAtStartup.Name = "RunAtStartup";
            this.RunAtStartup.Size = new System.Drawing.Size(93, 17);
            this.RunAtStartup.TabIndex = 7;
            this.RunAtStartup.Text = "Run at startup";
            this.RunAtStartup.UseVisualStyleBackColor = true;
            this.RunAtStartup.CheckedChanged += new System.EventHandler(this.RunAtStartup_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 177);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Hotkeys";
            // 
            // txtHotkey
            // 
            this.txtHotkey.Location = new System.Drawing.Point(64, 174);
            this.txtHotkey.Name = "txtHotkey";
            this.txtHotkey.Size = new System.Drawing.Size(132, 20);
            this.txtHotkey.TabIndex = 10;
            // 
            // closeButton
            // 
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(295, 186);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 11;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(382, 221);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.txtHotkey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RunAtStartup);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Settings";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox RunAtStartup;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtHotkey;
        private System.Windows.Forms.CheckedListBox lstDevices;
        private System.Windows.Forms.Button closeButton;
    }
}