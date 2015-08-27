namespace SoundSwitch.UI.Forms
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Active", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Available", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.deviceListView = new System.Windows.Forms.ListView();
            this.RunAtStartup = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHotkey = new System.Windows.Forms.TextBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.communicationCheckbox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.deviceListView);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(469, 195);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Devices selection";
            // 
            // deviceListView
            // 
            this.deviceListView.CheckBoxes = true;
            this.deviceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewGroup1.Header = "Active";
            listViewGroup1.Name = "selectedGroup";
            listViewGroup2.Header = "Available";
            listViewGroup2.Name = "unSelectedGroup";
            this.deviceListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.deviceListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.deviceListView.Location = new System.Drawing.Point(3, 16);
            this.deviceListView.Name = "deviceListView";
            this.deviceListView.Size = new System.Drawing.Size(463, 176);
            this.deviceListView.TabIndex = 0;
            this.deviceListView.UseCompatibleStateImageBehavior = false;
            this.deviceListView.View = System.Windows.Forms.View.Details;
            // 
            // RunAtStartup
            // 
            this.RunAtStartup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RunAtStartup.AutoSize = true;
            this.RunAtStartup.Location = new System.Drawing.Point(12, 240);
            this.RunAtStartup.Name = "RunAtStartup";
            this.RunAtStartup.Size = new System.Drawing.Size(93, 17);
            this.RunAtStartup.TabIndex = 7;
            this.RunAtStartup.Text = "Run at startup";
            this.RunAtStartup.UseVisualStyleBackColor = true;
            this.RunAtStartup.CheckedChanged += new System.EventHandler(this.RunAtStartup_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Hotkeys";
            // 
            // txtHotkey
            // 
            this.txtHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtHotkey.Location = new System.Drawing.Point(64, 214);
            this.txtHotkey.Name = "txtHotkey";
            this.txtHotkey.Size = new System.Drawing.Size(132, 20);
            this.txtHotkey.TabIndex = 10;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(409, 226);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 11;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // communicationCheckbox
            // 
            this.communicationCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.communicationCheckbox.AutoSize = true;
            this.communicationCheckbox.Location = new System.Drawing.Point(111, 240);
            this.communicationCheckbox.Name = "communicationCheckbox";
            this.communicationCheckbox.Size = new System.Drawing.Size(158, 17);
            this.communicationCheckbox.TabIndex = 12;
            this.communicationCheckbox.Text = "Set also as Communications";
            this.communicationCheckbox.UseVisualStyleBackColor = true;
            this.communicationCheckbox.CheckedChanged += new System.EventHandler(this.communicationCheckbox_CheckedChanged);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(496, 261);
            this.Controls.Add(this.communicationCheckbox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.txtHotkey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RunAtStartup);
            this.Controls.Add(this.groupBox1);
            this.MinimumSize = new System.Drawing.Size(400, 200);
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
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.ListView deviceListView;
        private System.Windows.Forms.CheckBox communicationCheckbox;
    }
}