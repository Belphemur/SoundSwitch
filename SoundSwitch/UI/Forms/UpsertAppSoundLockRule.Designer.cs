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
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblProcessPath = new System.Windows.Forms.Label();
            this.txtProcessPath = new System.Windows.Forms.TextBox();
            this.btnSelectProcess = new System.Windows.Forms.Button();
            this.lblPattern = new System.Windows.Forms.Label();
            this.txtPattern = new System.Windows.Forms.TextBox();
            this.lblRegexNotice = new System.Windows.Forms.Label();
            this.lblMode = new System.Windows.Forms.Label();
            this.rdoAutomatic = new System.Windows.Forms.RadioButton();
            this.rdoHotkey = new System.Windows.Forms.RadioButton();
            this.lblPlayback = new System.Windows.Forms.Label();
            this.cmbPlayback = new SoundSwitch.UI.Component.IconTextComboBox();
            this.btnPlaybackReset = new System.Windows.Forms.Button();
            this.lblRecording = new System.Windows.Forms.Label();
            this.cmbRecording = new SoundSwitch.UI.Component.IconTextComboBox();
            this.btnRecordingReset = new System.Windows.Forms.Button();
            this.grpHotkey = new System.Windows.Forms.GroupBox();
            this.lblHotKey = new System.Windows.Forms.Label();
            this.hotKeyBox = new SoundSwitch.UI.Component.HotKeyTextBox();
            this.btnHotKeyReset = new System.Windows.Forms.Button();
            this.lblPairCount = new System.Windows.Forms.Label();
            this.cmbPairPlayback = new SoundSwitch.UI.Component.IconTextComboBox();
            this.cmbPairRecording = new SoundSwitch.UI.Component.IconTextComboBox();
            this.btnAddPair = new System.Windows.Forms.Button();
            this.btnNewPair = new System.Windows.Forms.Button();
            this.lvPairs = new SoundSwitch.UI.Component.ListView.IconListView();
            this.btnRemovePair = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.chkCaseSensitive = new System.Windows.Forms.CheckBox();
            this.chkNotify = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpHotkey.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(536, 34);
            this.lblDescription.TabIndex = 18;
            this.lblDescription.Text = string.Empty;
            // 
            // lblProcessPath
            // 
            this.lblProcessPath.AutoSize = true;
            this.lblProcessPath.Location = new System.Drawing.Point(12, 57);
            this.lblProcessPath.Name = "lblProcessPath";
            this.lblProcessPath.Size = new System.Drawing.Size(73, 15);
            this.lblProcessPath.TabIndex = 2;
            this.lblProcessPath.Text = string.Empty;
            // 
            // txtProcessPath
            // 
            this.txtProcessPath.Location = new System.Drawing.Point(110, 54);
            this.txtProcessPath.Name = "txtProcessPath";
            this.txtProcessPath.Size = new System.Drawing.Size(380, 23);
            this.txtProcessPath.TabIndex = 3;
            // 
            // btnSelectProcess
            // 
            this.btnSelectProcess.Location = new System.Drawing.Point(496, 54);
            this.btnSelectProcess.Name = "btnSelectProcess";
            this.btnSelectProcess.Size = new System.Drawing.Size(28, 23);
            this.btnSelectProcess.TabIndex = 6;
            this.btnSelectProcess.Text = "...";
            this.btnSelectProcess.UseVisualStyleBackColor = true;
            this.btnSelectProcess.Click += new System.EventHandler(this.BtnSelectProcess_Click);
            // 
            // lblPattern
            // 
            this.lblPattern.AutoSize = true;
            this.lblPattern.Location = new System.Drawing.Point(12, 87);
            this.lblPattern.Name = "lblPattern";
            this.lblPattern.Size = new System.Drawing.Size(45, 15);
            this.lblPattern.TabIndex = 4;
            this.lblPattern.Text = string.Empty;
            // 
            // txtPattern
            // 
            this.txtPattern.Location = new System.Drawing.Point(110, 84);
            this.txtPattern.Name = "txtPattern";
            this.txtPattern.Size = new System.Drawing.Size(380, 23);
            this.txtPattern.TabIndex = 5;
            // 
            // lblRegexNotice
            // 
            this.lblRegexNotice.AutoSize = true;
            this.lblRegexNotice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblRegexNotice.Location = new System.Drawing.Point(110, 110);
            this.lblRegexNotice.Name = "lblRegexNotice";
            this.lblRegexNotice.Size = new System.Drawing.Size(176, 15);
            this.lblRegexNotice.TabIndex = 7;
            this.lblRegexNotice.Text = string.Empty;
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(12, 142);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(41, 15);
            this.lblMode.TabIndex = 8;
            this.lblMode.Text = string.Empty;
            // 
            // rdoAutomatic
            // 
            this.rdoAutomatic.AutoSize = true;
            this.rdoAutomatic.Checked = true;
            this.rdoAutomatic.Location = new System.Drawing.Point(110, 140);
            this.rdoAutomatic.Name = "rdoAutomatic";
            this.rdoAutomatic.Size = new System.Drawing.Size(84, 19);
            this.rdoAutomatic.TabIndex = 9;
            this.rdoAutomatic.TabStop = true;
            this.rdoAutomatic.Text = string.Empty;
            this.rdoAutomatic.UseVisualStyleBackColor = true;
            this.rdoAutomatic.CheckedChanged += new System.EventHandler(this.ModeChanged);
            // 
            // rdoHotkey
            // 
            this.rdoHotkey.AutoSize = true;
            this.rdoHotkey.Location = new System.Drawing.Point(210, 140);
            this.rdoHotkey.Name = "rdoHotkey";
            this.rdoHotkey.Size = new System.Drawing.Size(68, 19);
            this.rdoHotkey.TabIndex = 10;
            this.rdoHotkey.Text = string.Empty;
            this.rdoHotkey.UseVisualStyleBackColor = true;
            this.rdoHotkey.CheckedChanged += new System.EventHandler(this.ModeChanged);
            // 
            // lblPlayback
            // 
            this.lblPlayback.AutoSize = true;
            this.lblPlayback.Location = new System.Drawing.Point(12, 175);
            this.lblPlayback.Name = "lblPlayback";
            this.lblPlayback.Size = new System.Drawing.Size(54, 15);
            this.lblPlayback.TabIndex = 11;
            this.lblPlayback.Text = string.Empty;
            // 
            // cmbPlayback
            // 
            this.cmbPlayback.DataSource = null;
            this.cmbPlayback.DisplayMember = "Text";
            this.cmbPlayback.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPlayback.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPlayback.FormattingEnabled = true;
            this.cmbPlayback.Location = new System.Drawing.Point(110, 172);
            this.cmbPlayback.Name = "cmbPlayback";
            this.cmbPlayback.Size = new System.Drawing.Size(380, 24);
            this.cmbPlayback.TabIndex = 12;
            this.cmbPlayback.ValueMember = "Tag";
            this.cmbPlayback.SelectedIndexChanged += new System.EventHandler(this.CmbPlayback_SelectedIndexChanged);
            // 
            // btnPlaybackReset
            // 
            this.btnPlaybackReset.Image = global::SoundSwitch.Properties.Resources.delete;
            this.btnPlaybackReset.Location = new System.Drawing.Point(496, 172);
            this.btnPlaybackReset.Name = "btnPlaybackReset";
            this.btnPlaybackReset.Size = new System.Drawing.Size(28, 23);
            this.btnPlaybackReset.TabIndex = 13;
            this.btnPlaybackReset.UseVisualStyleBackColor = true;
            this.btnPlaybackReset.Visible = false;
            this.btnPlaybackReset.Click += new System.EventHandler(this.BtnPlaybackReset_Click);
            // 
            // lblRecording
            // 
            this.lblRecording.AutoSize = true;
            this.lblRecording.Location = new System.Drawing.Point(12, 205);
            this.lblRecording.Name = "lblRecording";
            this.lblRecording.Size = new System.Drawing.Size(61, 15);
            this.lblRecording.TabIndex = 14;
            this.lblRecording.Text = string.Empty;
            // 
            // cmbRecording
            // 
            this.cmbRecording.DataSource = null;
            this.cmbRecording.DisplayMember = "Text";
            this.cmbRecording.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbRecording.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecording.FormattingEnabled = true;
            this.cmbRecording.Location = new System.Drawing.Point(110, 202);
            this.cmbRecording.Name = "cmbRecording";
            this.cmbRecording.Size = new System.Drawing.Size(380, 24);
            this.cmbRecording.TabIndex = 15;
            this.cmbRecording.ValueMember = "Tag";
            this.cmbRecording.SelectedIndexChanged += new System.EventHandler(this.CmbRecording_SelectedIndexChanged);
            // 
            // btnRecordingReset
            // 
            this.btnRecordingReset.Image = global::SoundSwitch.Properties.Resources.delete;
            this.btnRecordingReset.Location = new System.Drawing.Point(496, 202);
            this.btnRecordingReset.Name = "btnRecordingReset";
            this.btnRecordingReset.Size = new System.Drawing.Size(28, 23);
            this.btnRecordingReset.TabIndex = 16;
            this.btnRecordingReset.UseVisualStyleBackColor = true;
            this.btnRecordingReset.Visible = false;
            this.btnRecordingReset.Click += new System.EventHandler(this.BtnRecordingReset_Click);
            // 
            // grpHotkey
            // 
            this.grpHotkey.Controls.Add(this.lblHotKey);
            this.grpHotkey.Controls.Add(this.hotKeyBox);
            this.grpHotkey.Controls.Add(this.btnHotKeyReset);
            this.grpHotkey.Controls.Add(this.lblPairCount);
            this.grpHotkey.Controls.Add(this.cmbPairPlayback);
            this.grpHotkey.Controls.Add(this.cmbPairRecording);
            this.grpHotkey.Controls.Add(this.btnAddPair);
            this.grpHotkey.Controls.Add(this.btnNewPair);
            this.grpHotkey.Controls.Add(this.lvPairs);
            this.grpHotkey.Controls.Add(this.btnRemovePair);
            this.grpHotkey.Controls.Add(this.btnMoveUp);
            this.grpHotkey.Controls.Add(this.btnMoveDown);
            this.grpHotkey.Location = new System.Drawing.Point(12, 168);
            this.grpHotkey.Name = "grpHotkey";
            this.grpHotkey.Size = new System.Drawing.Size(536, 218);
            this.grpHotkey.TabIndex = 17;
            this.grpHotkey.TabStop = false;
            this.grpHotkey.Text = string.Empty;
            this.grpHotkey.Visible = false;
            // 
            // lblHotKey
            // 
            this.lblHotKey.AutoSize = true;
            this.lblHotKey.Location = new System.Drawing.Point(12, 25);
            this.lblHotKey.Name = "lblHotKey";
            this.lblHotKey.Size = new System.Drawing.Size(45, 15);
            this.lblHotKey.TabIndex = 0;
            this.lblHotKey.Text = string.Empty;
            // 
            // hotKeyBox
            // 
            this.hotKeyBox.Location = new System.Drawing.Point(80, 22);
            this.hotKeyBox.Name = "hotKeyBox";
            this.hotKeyBox.Size = new System.Drawing.Size(340, 23);
            this.hotKeyBox.TabIndex = 1;
            // 
            // btnHotKeyReset
            // 
            this.btnHotKeyReset.Image = global::SoundSwitch.Properties.Resources.delete;
            this.btnHotKeyReset.Location = new System.Drawing.Point(426, 22);
            this.btnHotKeyReset.Name = "btnHotKeyReset";
            this.btnHotKeyReset.Size = new System.Drawing.Size(28, 23);
            this.btnHotKeyReset.TabIndex = 2;
            this.btnHotKeyReset.UseVisualStyleBackColor = true;
            this.btnHotKeyReset.Visible = false;
            this.btnHotKeyReset.Click += new System.EventHandler(this.BtnHotKeyReset_Click);
            // 
            // lblPairCount
            // 
            this.lblPairCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPairCount.AutoSize = true;
            this.lblPairCount.Location = new System.Drawing.Point(360, 4);
            this.lblPairCount.Name = "lblPairCount";
            this.lblPairCount.Size = new System.Drawing.Size(120, 15);
            this.lblPairCount.TabIndex = 3;
            this.lblPairCount.Text = string.Empty;
            this.lblPairCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cmbPairPlayback
            // 
            this.cmbPairPlayback.DataSource = null;
            this.cmbPairPlayback.DisplayMember = "Text";
            this.cmbPairPlayback.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPairPlayback.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPairPlayback.FormattingEnabled = true;
            this.cmbPairPlayback.Location = new System.Drawing.Point(12, 55);
            this.cmbPairPlayback.Name = "cmbPairPlayback";
            this.cmbPairPlayback.Size = new System.Drawing.Size(190, 24);
            this.cmbPairPlayback.TabIndex = 4;
            this.cmbPairPlayback.ValueMember = "Tag";
            this.cmbPairPlayback.SelectedIndexChanged += new System.EventHandler(this.CmbPairPlayback_SelectedIndexChanged);
            // 
            // cmbPairRecording
            // 
            this.cmbPairRecording.DataSource = null;
            this.cmbPairRecording.DisplayMember = "Text";
            this.cmbPairRecording.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmbPairRecording.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPairRecording.FormattingEnabled = true;
            this.cmbPairRecording.Location = new System.Drawing.Point(208, 55);
            this.cmbPairRecording.Name = "cmbPairRecording";
            this.cmbPairRecording.Size = new System.Drawing.Size(190, 24);
            this.cmbPairRecording.TabIndex = 5;
            this.cmbPairRecording.ValueMember = "Tag";
            this.cmbPairRecording.SelectedIndexChanged += new System.EventHandler(this.CmbPairRecording_SelectedIndexChanged);
            // 
            // btnAddPair
            // 
            this.btnAddPair.Location = new System.Drawing.Point(404, 54);
            this.btnAddPair.Name = "btnAddPair";
            this.btnAddPair.Size = new System.Drawing.Size(90, 25);
            this.btnAddPair.TabIndex = 6;
            this.btnAddPair.Text = string.Empty;
            this.btnAddPair.UseVisualStyleBackColor = true;
            this.btnAddPair.Click += new System.EventHandler(this.BtnAddPair_Click);
            // 
            // btnNewPair
            // 
            this.btnNewPair.Location = new System.Drawing.Point(404, 184);
            this.btnNewPair.Name = "btnNewPair";
            this.btnNewPair.Size = new System.Drawing.Size(90, 25);
            this.btnNewPair.TabIndex = 11;
            this.btnNewPair.Text = string.Empty;
            this.btnNewPair.UseVisualStyleBackColor = true;
            this.btnNewPair.Click += new System.EventHandler(this.BtnNewPair_Click);
            // 
            // lvPairs
            // 
            this.lvPairs.FullRowSelect = true;
            this.lvPairs.HideSelection = false;
            this.lvPairs.Location = new System.Drawing.Point(12, 88);
            this.lvPairs.MultiSelect = false;
            this.lvPairs.Name = "lvPairs";
            this.lvPairs.OwnerDraw = true;
            this.lvPairs.ShowGroups = false;
            this.lvPairs.Size = new System.Drawing.Size(510, 90);
            this.lvPairs.TabIndex = 7;
            this.lvPairs.UseCompatibleStateImageBehavior = false;
            this.lvPairs.View = System.Windows.Forms.View.Details;
            this.lvPairs.SelectedIndexChanged += new System.EventHandler(this.LvPairs_SelectedIndexChanged);
            // 
            // btnRemovePair
            // 
            this.btnRemovePair.Location = new System.Drawing.Point(12, 184);
            this.btnRemovePair.Name = "btnRemovePair";
            this.btnRemovePair.Size = new System.Drawing.Size(80, 25);
            this.btnRemovePair.TabIndex = 8;
            this.btnRemovePair.Text = string.Empty;
            this.btnRemovePair.UseVisualStyleBackColor = true;
            this.btnRemovePair.Click += new System.EventHandler(this.BtnRemovePair_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Location = new System.Drawing.Point(98, 184);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(80, 25);
            this.btnMoveUp.TabIndex = 9;
            this.btnMoveUp.Text = string.Empty;
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.BtnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Location = new System.Drawing.Point(184, 184);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(80, 25);
            this.btnMoveDown.TabIndex = 10;
            this.btnMoveDown.Text = string.Empty;
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.BtnMoveDown_Click);
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(110, 240);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(68, 19);
            this.chkEnabled.TabIndex = 19;
            this.chkEnabled.Text = string.Empty;
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // chkCaseSensitive
            // 
            this.chkCaseSensitive.AutoSize = true;
            this.chkCaseSensitive.Location = new System.Drawing.Point(210, 240);
            this.chkCaseSensitive.Name = "chkCaseSensitive";
            this.chkCaseSensitive.Size = new System.Drawing.Size(102, 19);
            this.chkCaseSensitive.TabIndex = 20;
            this.chkCaseSensitive.Text = string.Empty;
            this.chkCaseSensitive.UseVisualStyleBackColor = true;
            // 
            // chkNotify
            // 
            this.chkNotify.AutoSize = true;
            this.chkNotify.Location = new System.Drawing.Point(340, 240);
            this.chkNotify.Name = "chkNotify";
            this.chkNotify.Size = new System.Drawing.Size(138, 19);
            this.chkNotify.TabIndex = 21;
            this.chkNotify.Text = string.Empty;
            this.chkNotify.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(378, 272);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 25);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = string.Empty;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(459, 272);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = string.Empty;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // UpsertAppSoundLockRule
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(560, 470);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkNotify);
            this.Controls.Add(this.chkCaseSensitive);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.grpHotkey);
            this.Controls.Add(this.cmbRecording);
            this.Controls.Add(this.btnRecordingReset);
            this.Controls.Add(this.lblRecording);
            this.Controls.Add(this.cmbPlayback);
            this.Controls.Add(this.btnPlaybackReset);
            this.Controls.Add(this.lblPlayback);
            this.Controls.Add(this.rdoHotkey);
            this.Controls.Add(this.rdoAutomatic);
            this.Controls.Add(this.lblMode);
            this.Controls.Add(this.lblRegexNotice);
            this.Controls.Add(this.btnSelectProcess);
            this.Controls.Add(this.txtPattern);
            this.Controls.Add(this.lblPattern);
            this.Controls.Add(this.txtProcessPath);
            this.Controls.Add(this.lblProcessPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpsertAppSoundLockRule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = string.Empty;
            this.grpHotkey.ResumeLayout(false);
            this.grpHotkey.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblProcessPath;
        private System.Windows.Forms.TextBox txtProcessPath;
        private System.Windows.Forms.Button btnSelectProcess;
        private System.Windows.Forms.Label lblPattern;
        private System.Windows.Forms.TextBox txtPattern;
        private System.Windows.Forms.Label lblRegexNotice;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.RadioButton rdoAutomatic;
        private System.Windows.Forms.RadioButton rdoHotkey;
        private System.Windows.Forms.Label lblPlayback;
        private SoundSwitch.UI.Component.IconTextComboBox cmbPlayback;
        private System.Windows.Forms.Button btnPlaybackReset;
        private System.Windows.Forms.Label lblRecording;
        private SoundSwitch.UI.Component.IconTextComboBox cmbRecording;
        private System.Windows.Forms.Button btnRecordingReset;
        private System.Windows.Forms.GroupBox grpHotkey;
        private System.Windows.Forms.Label lblHotKey;
        private SoundSwitch.UI.Component.HotKeyTextBox hotKeyBox;
        private System.Windows.Forms.Button btnHotKeyReset;
        private System.Windows.Forms.Label lblPairCount;
        private SoundSwitch.UI.Component.IconTextComboBox cmbPairPlayback;
        private SoundSwitch.UI.Component.IconTextComboBox cmbPairRecording;
        private System.Windows.Forms.Button btnAddPair;
        private System.Windows.Forms.Button btnNewPair;
        private SoundSwitch.UI.Component.ListView.IconListView lvPairs;
        private System.Windows.Forms.Button btnRemovePair;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkCaseSensitive;
        private System.Windows.Forms.CheckBox chkNotify;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
