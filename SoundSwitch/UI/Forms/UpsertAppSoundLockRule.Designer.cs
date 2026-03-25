namespace SoundSwitch.UI.Forms
{
    partial class UpsertAppSoundLockRule
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
            this.lblMatchMode = new System.Windows.Forms.Label();
            this.cmbMatchMode = new System.Windows.Forms.ComboBox();
            this.lblPattern = new System.Windows.Forms.Label();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.btnSelectProcess = new System.Windows.Forms.Button();
            this.lblRegexNotice = new System.Windows.Forms.Label();
            this.lblPlayback = new System.Windows.Forms.Label();
            this.cmbPlayback = new SoundSwitch.UI.Component.IconTextComboBox();
            this.lblRecording = new System.Windows.Forms.Label();
            this.cmbRecording = new SoundSwitch.UI.Component.IconTextComboBox();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.chkNotify = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnPlaybackReset = new System.Windows.Forms.Button();
            this.btnRecordingReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMatchMode
            // 
            this.lblMatchMode.AutoSize = true;
            this.lblMatchMode.Location = new System.Drawing.Point(12, 15);
            this.lblMatchMode.Name = "lblMatchMode";
            this.lblMatchMode.Size = new System.Drawing.Size(73, 15);
            this.lblMatchMode.TabIndex = 2;
            this.lblMatchMode.Text = string.Empty;
            // 
            // cmbMatchMode
            // 
            this.cmbMatchMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMatchMode.FormattingEnabled = true;
            this.cmbMatchMode.Location = new System.Drawing.Point(110, 12);
            this.cmbMatchMode.Name = "cmbMatchMode";
            this.cmbMatchMode.Size = new System.Drawing.Size(360, 23);
            this.cmbMatchMode.TabIndex = 3;
            // 
            // lblPattern
            // 
            this.lblPattern.AutoSize = true;
            this.lblPattern.Location = new System.Drawing.Point(12, 45);
            this.lblPattern.Name = "lblPattern";
            this.lblPattern.Size = new System.Drawing.Size(45, 15);
            this.lblPattern.TabIndex = 4;
            this.lblPattern.Text = string.Empty;
            // 
            // txtPattern
            // 
            this.txtPattern.Location = new System.Drawing.Point(110, 42);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(326, 23);
            this.txtPattern.TabIndex = 5;
            // 
            // btnSelectProcess
            // 
            this.btnSelectProcess.Location = new System.Drawing.Point(442, 42);
            this.btnSelectProcess.Name = "btnSelectProcess";
            this.btnSelectProcess.Size = new System.Drawing.Size(28, 23);
            this.btnSelectProcess.TabIndex = 6;
            this.btnSelectProcess.Text = "...";
            this.btnSelectProcess.UseVisualStyleBackColor = true;
            this.btnSelectProcess.Click += new System.EventHandler(this.BtnSelectProcess_Click);
            // 
            // lblRegexNotice
            // 
            this.lblRegexNotice.AutoSize = true;
            this.lblRegexNotice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblRegexNotice.Location = new System.Drawing.Point(110, 68);
            this.lblRegexNotice.Name = "lblRegexNotice";
            this.lblRegexNotice.Size = new System.Drawing.Size(176, 15);
            this.lblRegexNotice.TabIndex = 7;
            this.lblRegexNotice.Text = string.Empty;
            // 
            // lblPlayback
            // 
            this.lblPlayback.AutoSize = true;
            this.lblPlayback.Location = new System.Drawing.Point(12, 100);
            this.lblPlayback.Name = "lblPlayback";
            this.lblPlayback.Size = new System.Drawing.Size(54, 15);
            this.lblPlayback.TabIndex = 8;
            this.lblPlayback.Text = string.Empty;
            // 
            // cmbPlayback
            // 
            this.cmbPlayback.DataSource = null;
            this.cmbPlayback.DisplayMember = "Tag";
            this.cmbPlayback.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPlayback.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlayback.FormattingEnabled = true;
            this.cmbPlayback.Location = new System.Drawing.Point(110, 97);
            this.cmbPlayback.Name = "cmbPlayback";
            this.cmbPlayback.Size = new System.Drawing.Size(326, 24);
            this.cmbPlayback.TabIndex = 9;
            this.cmbPlayback.ValueMember = "Tag";
            this.cmbPlayback.SelectedIndexChanged += new System.EventHandler(this.CmbPlayback_SelectedIndexChanged);
            // 
            // btnPlaybackReset
            // 
            this.btnPlaybackReset.Image = global::SoundSwitch.Properties.Resources.delete;
            this.btnPlaybackReset.Location = new System.Drawing.Point(442, 97);
            this.btnPlaybackReset.Name = "btnPlaybackReset";
            this.btnPlaybackReset.Size = new System.Drawing.Size(28, 23);
            this.btnPlaybackReset.TabIndex = 16;
            this.btnPlaybackReset.UseVisualStyleBackColor = true;
            this.btnPlaybackReset.Visible = false;
            this.btnPlaybackReset.Click += new System.EventHandler(this.BtnPlaybackReset_Click);
            // 
            // lblRecording
            // 
            this.lblRecording.AutoSize = true;
            this.lblRecording.Location = new System.Drawing.Point(12, 130);
            this.lblRecording.Name = "lblRecording";
            this.lblRecording.Size = new System.Drawing.Size(61, 15);
            this.lblRecording.TabIndex = 10;
            this.lblRecording.Text = string.Empty;
            // 
            // cmbRecording
            // 
            this.cmbRecording.DataSource = null;
            this.cmbRecording.DisplayMember = "Tag";
            this.cmbRecording.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbRecording.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecording.FormattingEnabled = true;
            this.cmbRecording.Location = new System.Drawing.Point(110, 127);
            this.cmbRecording.Name = "cmbRecording";
            this.cmbRecording.Size = new System.Drawing.Size(326, 24);
            this.cmbRecording.TabIndex = 11;
            this.cmbRecording.ValueMember = "Tag";
            this.cmbRecording.SelectedIndexChanged += new System.EventHandler(this.CmbRecording_SelectedIndexChanged);
            // 
            // btnRecordingReset
            // 
            this.btnRecordingReset.Image = global::SoundSwitch.Properties.Resources.delete;
            this.btnRecordingReset.Location = new System.Drawing.Point(442, 127);
            this.btnRecordingReset.Name = "btnRecordingReset";
            this.btnRecordingReset.Size = new System.Drawing.Size(28, 23);
            this.btnRecordingReset.TabIndex = 17;
            this.btnRecordingReset.UseVisualStyleBackColor = true;
            this.btnRecordingReset.Visible = false;
            this.btnRecordingReset.Click += new System.EventHandler(this.BtnRecordingReset_Click);
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(110, 157);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(68, 19);
            this.chkEnabled.TabIndex = 12;
            this.chkEnabled.Text = string.Empty;
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // chkNotify
            // 
            this.chkNotify.AutoSize = true;
            this.chkNotify.Location = new System.Drawing.Point(200, 157);
            this.chkNotify.Name = "chkNotify";
            this.chkNotify.Size = new System.Drawing.Size(58, 19);
            this.chkNotify.TabIndex = 13;
            this.chkNotify.Text = string.Empty;
            this.chkNotify.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(314, 190);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 25);
            this.btnSave.TabIndex = 14;
            this.btnSave.Text = string.Empty;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(395, 190);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = string.Empty;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // UpsertAppSoundLockRule
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(484, 231);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkNotify);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.cmbRecording);
            this.Controls.Add(this.lblRecording);
            this.Controls.Add(this.cmbPlayback);
            this.Controls.Add(this.lblPlayback);
            this.Controls.Add(this.lblRegexNotice);
            this.Controls.Add(this.btnSelectProcess);
            this.Controls.Add(this.txtPattern);
            this.Controls.Add(this.lblPattern);
            this.Controls.Add(this.cmbMatchMode);
            this.Controls.Add(this.btnRecordingReset);
            this.Controls.Add(this.btnPlaybackReset);
            this.Controls.Add(this.lblMatchMode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpsertAppSoundLockRule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = string.Empty;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMatchMode;
        private System.Windows.Forms.ComboBox cmbMatchMode;
        private System.Windows.Forms.Label lblPattern;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.Button btnSelectProcess;
        private System.Windows.Forms.Label lblRegexNotice;
        private System.Windows.Forms.Label lblPlayback;
        private SoundSwitch.UI.Component.IconTextComboBox cmbPlayback;
        private System.Windows.Forms.Label lblRecording;
        private SoundSwitch.UI.Component.IconTextComboBox cmbRecording;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkNotify;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnPlaybackReset;
        private System.Windows.Forms.Button btnRecordingReset;
    }
}
