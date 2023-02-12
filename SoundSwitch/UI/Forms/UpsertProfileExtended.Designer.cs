namespace SoundSwitch.UI.Forms
{
    partial class UpsertProfileExtended
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpsertProfileExtended));
            this.triggerBox = new System.Windows.Forms.GroupBox();
            this.triggerLabel = new System.Windows.Forms.Label();
            this.selectProgramButton = new System.Windows.Forms.Button();
            this.descriptionBox = new System.Windows.Forms.GroupBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.textInput = new System.Windows.Forms.TextBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.hotKeyControl = new SoundSwitch.UI.Component.HotKeyTextBox();
            this.activeTriggerLabel = new System.Windows.Forms.Label();
            this.availableTriggerBox = new System.Windows.Forms.ComboBox();
            this.addTriggerButton = new System.Windows.Forms.Button();
            this.availableTriggersText = new System.Windows.Forms.Label();
            this.setTriggerBox = new System.Windows.Forms.ListBox();
            this.restoreDevicesCheckBox = new System.Windows.Forms.CheckBox();
            this.selectProgramDialog = new System.Windows.Forms.OpenFileDialog();
            this.profileBox = new System.Windows.Forms.GroupBox();
            this.notifyCheckbox = new System.Windows.Forms.CheckBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.switchDefaultCheckBox = new System.Windows.Forms.CheckBox();
            this.recordingRemoveButton = new System.Windows.Forms.Button();
            this.playbackRemoveButton = new System.Windows.Forms.Button();
            this.recordingLabel = new System.Windows.Forms.Label();
            this.playbackLabel = new System.Windows.Forms.Label();
            this.recordingComboBox = new SoundSwitch.UI.Component.IconTextComboBox();
            this.playbackComboBox = new SoundSwitch.UI.Component.IconTextComboBox();
            this.communicationRecordingRemoveButton = new System.Windows.Forms.Button();
            this.communicationRecordingLabel = new System.Windows.Forms.Label();
            this.communicationRecordingComboBox = new SoundSwitch.UI.Component.IconTextComboBox();
            this.communicationRemoveButton = new System.Windows.Forms.Button();
            this.communicationLabel = new System.Windows.Forms.Label();
            this.communicationComboBox = new SoundSwitch.UI.Component.IconTextComboBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.communicationBox = new System.Windows.Forms.GroupBox();
            this.switchForegroundCheckbox = new System.Windows.Forms.CheckBox();
            this.triggerBox.SuspendLayout();
            this.descriptionBox.SuspendLayout();
            this.profileBox.SuspendLayout();
            this.communicationBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // triggerBox
            // 
            this.triggerBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.triggerBox.Controls.Add(this.triggerLabel);
            this.triggerBox.Controls.Add(this.selectProgramButton);
            this.triggerBox.Controls.Add(this.descriptionBox);
            this.triggerBox.Controls.Add(this.textInput);
            this.triggerBox.Controls.Add(this.deleteButton);
            this.triggerBox.Controls.Add(this.hotKeyControl);
            this.triggerBox.Controls.Add(this.activeTriggerLabel);
            this.triggerBox.Controls.Add(this.availableTriggerBox);
            this.triggerBox.Controls.Add(this.addTriggerButton);
            this.triggerBox.Controls.Add(this.availableTriggersText);
            this.triggerBox.Controls.Add(this.setTriggerBox);
            this.triggerBox.Location = new System.Drawing.Point(14, 382);
            this.triggerBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.triggerBox.Name = "triggerBox";
            this.triggerBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.triggerBox.Size = new System.Drawing.Size(668, 239);
            this.triggerBox.TabIndex = 0;
            this.triggerBox.TabStop = false;
            this.triggerBox.Text = "Triggers";
            // 
            // triggerLabel
            // 
            this.triggerLabel.AutoSize = true;
            this.triggerLabel.Location = new System.Drawing.Point(326, 74);
            this.triggerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.triggerLabel.Name = "triggerLabel";
            this.triggerLabel.Size = new System.Drawing.Size(43, 15);
            this.triggerLabel.TabIndex = 10;
            this.triggerLabel.Text = "Trigger";
            // 
            // selectProgramButton
            // 
            this.selectProgramButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.selectProgramButton.Location = new System.Drawing.Point(618, 179);
            this.selectProgramButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.selectProgramButton.Name = "selectProgramButton";
            this.selectProgramButton.Size = new System.Drawing.Size(28, 23);
            this.selectProgramButton.TabIndex = 9;
            this.selectProgramButton.Text = "...";
            this.selectProgramButton.UseVisualStyleBackColor = true;
            this.selectProgramButton.Click += new System.EventHandler(this.selectProgramButton_Click);
            // 
            // descriptionBox
            // 
            this.descriptionBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionBox.AutoSize = true;
            this.descriptionBox.Controls.Add(this.descriptionLabel);
            this.descriptionBox.Location = new System.Drawing.Point(318, 92);
            this.descriptionBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.descriptionBox.Name = "descriptionBox";
            this.descriptionBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.descriptionBox.Size = new System.Drawing.Size(331, 81);
            this.descriptionBox.TabIndex = 8;
            this.descriptionBox.TabStop = false;
            this.descriptionBox.Text = "Description";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionLabel.Location = new System.Drawing.Point(4, 19);
            this.descriptionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.descriptionLabel.Size = new System.Drawing.Size(323, 59);
            this.descriptionLabel.TabIndex = 8;
            this.descriptionLabel.Text = "Description of the selected trigger";
            // 
            // textInput
            // 
            this.textInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textInput.Location = new System.Drawing.Point(318, 179);
            this.textInput.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textInput.Name = "textInput";
            this.textInput.Size = new System.Drawing.Size(292, 23);
            this.textInput.TabIndex = 7;
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteButton.Location = new System.Drawing.Point(213, 206);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(88, 27);
            this.deleteButton.TabIndex = 5;
            this.deleteButton.Text = "Remove";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // hotKeyControl
            // 
            this.hotKeyControl.ListenToHotkey = false;
            this.hotKeyControl.Location = new System.Drawing.Point(318, 42);
            this.hotKeyControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hotKeyControl.Name = "hotKeyControl";
            this.hotKeyControl.Size = new System.Drawing.Size(160, 23);
            this.hotKeyControl.TabIndex = 6;
            // 
            // activeTriggerLabel
            // 
            this.activeTriggerLabel.AutoSize = true;
            this.activeTriggerLabel.Location = new System.Drawing.Point(9, 74);
            this.activeTriggerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.activeTriggerLabel.Name = "activeTriggerLabel";
            this.activeTriggerLabel.Size = new System.Drawing.Size(84, 15);
            this.activeTriggerLabel.TabIndex = 4;
            this.activeTriggerLabel.Text = "Active Triggers";
            // 
            // availableTriggerBox
            // 
            this.availableTriggerBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.availableTriggerBox.FormattingEnabled = true;
            this.availableTriggerBox.Location = new System.Drawing.Point(13, 42);
            this.availableTriggerBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.availableTriggerBox.Name = "availableTriggerBox";
            this.availableTriggerBox.Size = new System.Drawing.Size(193, 23);
            this.availableTriggerBox.TabIndex = 3;
            // 
            // addTriggerButton
            // 
            this.addTriggerButton.Location = new System.Drawing.Point(214, 40);
            this.addTriggerButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.addTriggerButton.Name = "addTriggerButton";
            this.addTriggerButton.Size = new System.Drawing.Size(88, 27);
            this.addTriggerButton.TabIndex = 2;
            this.addTriggerButton.Text = "Add";
            this.addTriggerButton.UseVisualStyleBackColor = true;
            this.addTriggerButton.Click += new System.EventHandler(this.addTriggerButton_Click);
            // 
            // availableTriggersText
            // 
            this.availableTriggersText.AutoSize = true;
            this.availableTriggersText.Location = new System.Drawing.Point(9, 22);
            this.availableTriggersText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.availableTriggersText.Name = "availableTriggersText";
            this.availableTriggersText.Size = new System.Drawing.Size(99, 15);
            this.availableTriggersText.TabIndex = 1;
            this.availableTriggersText.Text = "Available Triggers";
            // 
            // setTriggerBox
            // 
            this.setTriggerBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.setTriggerBox.FormattingEnabled = true;
            this.setTriggerBox.ItemHeight = 15;
            this.setTriggerBox.Location = new System.Drawing.Point(13, 92);
            this.setTriggerBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.setTriggerBox.Name = "setTriggerBox";
            this.setTriggerBox.Size = new System.Drawing.Size(288, 109);
            this.setTriggerBox.TabIndex = 0;
            this.setTriggerBox.SelectedIndexChanged += new System.EventHandler(this.setTriggerBox_SelectedIndexChanged);
            // 
            // restoreDevicesCheckBox
            // 
            this.restoreDevicesCheckBox.AutoSize = true;
            this.restoreDevicesCheckBox.Location = new System.Drawing.Point(13, 90);
            this.restoreDevicesCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.restoreDevicesCheckBox.Name = "restoreDevicesCheckBox";
            this.restoreDevicesCheckBox.Size = new System.Drawing.Size(107, 19);
            this.restoreDevicesCheckBox.TabIndex = 11;
            this.restoreDevicesCheckBox.Text = "Restore devices";
            this.restoreDevicesCheckBox.UseVisualStyleBackColor = true;
            this.restoreDevicesCheckBox.CheckedChanged += new System.EventHandler(this.restoreDevicesCheckBox_CheckedChanged);
            // 
            // profileBox
            // 
            this.profileBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileBox.Controls.Add(this.switchForegroundCheckbox);
            this.profileBox.Controls.Add(this.restoreDevicesCheckBox);
            this.profileBox.Controls.Add(this.notifyCheckbox);
            this.profileBox.Controls.Add(this.nameLabel);
            this.profileBox.Controls.Add(this.nameTextBox);
            this.profileBox.Controls.Add(this.switchDefaultCheckBox);
            this.profileBox.Controls.Add(this.recordingRemoveButton);
            this.profileBox.Controls.Add(this.playbackRemoveButton);
            this.profileBox.Controls.Add(this.recordingLabel);
            this.profileBox.Controls.Add(this.playbackLabel);
            this.profileBox.Controls.Add(this.recordingComboBox);
            this.profileBox.Controls.Add(this.playbackComboBox);
            this.profileBox.Location = new System.Drawing.Point(14, 14);
            this.profileBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.profileBox.Name = "profileBox";
            this.profileBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.profileBox.Size = new System.Drawing.Size(668, 235);
            this.profileBox.TabIndex = 1;
            this.profileBox.TabStop = false;
            this.profileBox.Text = "Profile";
            // 
            // notifyCheckbox
            // 
            this.notifyCheckbox.AutoSize = true;
            this.notifyCheckbox.Location = new System.Drawing.Point(13, 110);
            this.notifyCheckbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.notifyCheckbox.Name = "notifyCheckbox";
            this.notifyCheckbox.Size = new System.Drawing.Size(190, 19);
            this.notifyCheckbox.TabIndex = 28;
            this.notifyCheckbox.Text = "Notify when profile is triggered";
            this.notifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(9, 23);
            this.nameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(39, 15);
            this.nameLabel.TabIndex = 27;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(13, 42);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(322, 23);
            this.nameTextBox.TabIndex = 26;
            // 
            // switchDefaultCheckBox
            // 
            this.switchDefaultCheckBox.AutoSize = true;
            this.switchDefaultCheckBox.Location = new System.Drawing.Point(13, 72);
            this.switchDefaultCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.switchDefaultCheckBox.Name = "switchDefaultCheckBox";
            this.switchDefaultCheckBox.Size = new System.Drawing.Size(163, 19);
            this.switchDefaultCheckBox.TabIndex = 22;
            this.switchDefaultCheckBox.Text = "Also switch default device";
            this.switchDefaultCheckBox.UseVisualStyleBackColor = true;
            this.switchDefaultCheckBox.CheckedChanged += new System.EventHandler(this.switchDefaultCheckBox_CheckedChanged);
            // 
            // recordingRemoveButton
            // 
            this.recordingRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingRemoveButton.Image = ((System.Drawing.Image)(resources.GetObject("recordingRemoveButton.Image")));
            this.recordingRemoveButton.Location = new System.Drawing.Point(629, 206);
            this.recordingRemoveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.recordingRemoveButton.Name = "recordingRemoveButton";
            this.recordingRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.recordingRemoveButton.TabIndex = 21;
            this.recordingRemoveButton.UseVisualStyleBackColor = true;
            this.recordingRemoveButton.Visible = false;
            this.recordingRemoveButton.Click += new System.EventHandler(this.recordingRemoveButton_Click);
            // 
            // playbackRemoveButton
            // 
            this.playbackRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackRemoveButton.Image = ((System.Drawing.Image)(resources.GetObject("playbackRemoveButton.Image")));
            this.playbackRemoveButton.Location = new System.Drawing.Point(629, 155);
            this.playbackRemoveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.playbackRemoveButton.Name = "playbackRemoveButton";
            this.playbackRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.playbackRemoveButton.TabIndex = 20;
            this.playbackRemoveButton.UseVisualStyleBackColor = true;
            this.playbackRemoveButton.Visible = false;
            this.playbackRemoveButton.Click += new System.EventHandler(this.playbackRemoveButton_Click);
            // 
            // recordingLabel
            // 
            this.recordingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingLabel.AutoSize = true;
            this.recordingLabel.Location = new System.Drawing.Point(8, 183);
            this.recordingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.recordingLabel.Name = "recordingLabel";
            this.recordingLabel.Size = new System.Drawing.Size(61, 15);
            this.recordingLabel.TabIndex = 19;
            this.recordingLabel.Text = "Recording";
            // 
            // playbackLabel
            // 
            this.playbackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackLabel.AutoSize = true;
            this.playbackLabel.Location = new System.Drawing.Point(8, 131);
            this.playbackLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.playbackLabel.Name = "playbackLabel";
            this.playbackLabel.Size = new System.Drawing.Size(54, 15);
            this.playbackLabel.TabIndex = 18;
            this.playbackLabel.Text = "Playback";
            // 
            // recordingComboBox
            // 
            this.recordingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingComboBox.DataSource = null;
            this.recordingComboBox.DisplayMember = "Tag";
            this.recordingComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.recordingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recordingComboBox.FormattingEnabled = true;
            this.recordingComboBox.Location = new System.Drawing.Point(12, 206);
            this.recordingComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.recordingComboBox.Name = "recordingComboBox";
            this.recordingComboBox.Size = new System.Drawing.Size(610, 24);
            this.recordingComboBox.TabIndex = 17;
            this.recordingComboBox.ValueMember = "Tag";
            this.recordingComboBox.SelectedIndexChanged += new System.EventHandler(this.recordingComboBox_SelectedIndexChanged);
            // 
            // playbackComboBox
            // 
            this.playbackComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackComboBox.DataSource = null;
            this.playbackComboBox.DisplayMember = "Tag";
            this.playbackComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.playbackComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playbackComboBox.FormattingEnabled = true;
            this.playbackComboBox.Location = new System.Drawing.Point(12, 155);
            this.playbackComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.playbackComboBox.Name = "playbackComboBox";
            this.playbackComboBox.Size = new System.Drawing.Size(610, 24);
            this.playbackComboBox.TabIndex = 16;
            this.playbackComboBox.ValueMember = "Tag";
            this.playbackComboBox.SelectedIndexChanged += new System.EventHandler(this.playbackComboBox_SelectedIndexChanged);
            // 
            // communicationRecordingRemoveButton
            // 
            this.communicationRecordingRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationRecordingRemoveButton.Image = ((System.Drawing.Image)(resources.GetObject("communicationRecordingRemoveButton.Image")));
            this.communicationRecordingRemoveButton.Location = new System.Drawing.Point(629, 91);
            this.communicationRecordingRemoveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.communicationRecordingRemoveButton.Name = "communicationRecordingRemoveButton";
            this.communicationRecordingRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.communicationRecordingRemoveButton.TabIndex = 31;
            this.communicationRecordingRemoveButton.UseVisualStyleBackColor = true;
            this.communicationRecordingRemoveButton.Visible = false;
            this.communicationRecordingRemoveButton.Click += new System.EventHandler(this.communicationRecordingRemoveButton_Click);
            // 
            // communicationRecordingLabel
            // 
            this.communicationRecordingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationRecordingLabel.AutoSize = true;
            this.communicationRecordingLabel.Location = new System.Drawing.Point(5, 68);
            this.communicationRecordingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.communicationRecordingLabel.Name = "communicationRecordingLabel";
            this.communicationRecordingLabel.Size = new System.Drawing.Size(151, 15);
            this.communicationRecordingLabel.TabIndex = 30;
            this.communicationRecordingLabel.Text = "Recording Communication";
            // 
            // communicationRecordingComboBox
            // 
            this.communicationRecordingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationRecordingComboBox.DataSource = null;
            this.communicationRecordingComboBox.DisplayMember = "Tag";
            this.communicationRecordingComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.communicationRecordingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.communicationRecordingComboBox.FormattingEnabled = true;
            this.communicationRecordingComboBox.Location = new System.Drawing.Point(9, 91);
            this.communicationRecordingComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.communicationRecordingComboBox.Name = "communicationRecordingComboBox";
            this.communicationRecordingComboBox.Size = new System.Drawing.Size(613, 24);
            this.communicationRecordingComboBox.TabIndex = 29;
            this.communicationRecordingComboBox.ValueMember = "Tag";
            this.communicationRecordingComboBox.SelectedIndexChanged += new System.EventHandler(this.communicationRecordingComboBox_SelectedIndexChanged);
            // 
            // communicationRemoveButton
            // 
            this.communicationRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationRemoveButton.Image = ((System.Drawing.Image)(resources.GetObject("communicationRemoveButton.Image")));
            this.communicationRemoveButton.Location = new System.Drawing.Point(630, 41);
            this.communicationRemoveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.communicationRemoveButton.Name = "communicationRemoveButton";
            this.communicationRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.communicationRemoveButton.TabIndex = 25;
            this.communicationRemoveButton.UseVisualStyleBackColor = true;
            this.communicationRemoveButton.Visible = false;
            this.communicationRemoveButton.Click += new System.EventHandler(this.communicationRemoveButton_Click);
            // 
            // communicationLabel
            // 
            this.communicationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationLabel.AutoSize = true;
            this.communicationLabel.Location = new System.Drawing.Point(6, 18);
            this.communicationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.communicationLabel.Name = "communicationLabel";
            this.communicationLabel.Size = new System.Drawing.Size(94, 15);
            this.communicationLabel.TabIndex = 24;
            this.communicationLabel.Text = "Communication";
            // 
            // communicationComboBox
            // 
            this.communicationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationComboBox.DataSource = null;
            this.communicationComboBox.DisplayMember = "Tag";
            this.communicationComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.communicationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.communicationComboBox.FormattingEnabled = true;
            this.communicationComboBox.Location = new System.Drawing.Point(10, 41);
            this.communicationComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.communicationComboBox.Name = "communicationComboBox";
            this.communicationComboBox.Size = new System.Drawing.Size(613, 24);
            this.communicationComboBox.TabIndex = 23;
            this.communicationComboBox.ValueMember = "Tag";
            this.communicationComboBox.SelectedIndexChanged += new System.EventHandler(this.communicationComboBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(621, 627);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(60, 30);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // communicationBox
            // 
            this.communicationBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationBox.Controls.Add(this.communicationRecordingRemoveButton);
            this.communicationBox.Controls.Add(this.communicationRecordingComboBox);
            this.communicationBox.Controls.Add(this.communicationRecordingLabel);
            this.communicationBox.Controls.Add(this.communicationComboBox);
            this.communicationBox.Controls.Add(this.communicationRemoveButton);
            this.communicationBox.Controls.Add(this.communicationLabel);
            this.communicationBox.Location = new System.Drawing.Point(14, 255);
            this.communicationBox.Name = "communicationBox";
            this.communicationBox.Size = new System.Drawing.Size(668, 121);
            this.communicationBox.TabIndex = 3;
            this.communicationBox.TabStop = false;
            this.communicationBox.Text = "Communication";
            // 
            // switchForegroundCheckbox
            // 
            this.switchForegroundCheckbox.AutoSize = true;
            this.switchForegroundCheckbox.Location = new System.Drawing.Point(318, 72);
            this.switchForegroundCheckbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.switchForegroundCheckbox.Name = "switchForegroundCheckbox";
            this.switchForegroundCheckbox.Size = new System.Drawing.Size(126, 19);
            this.switchForegroundCheckbox.TabIndex = 29;
            this.switchForegroundCheckbox.Text = "Switch Foreground";
            this.switchForegroundCheckbox.UseVisualStyleBackColor = true;
            // 
            // UpsertProfileExtended
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 660);
            this.Controls.Add(this.communicationBox);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.profileBox);
            this.Controls.Add(this.triggerBox);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(710, 640);
            this.Name = "UpsertProfileExtended";
            this.Text = "UpsertProfileExtended";
            this.triggerBox.ResumeLayout(false);
            this.triggerBox.PerformLayout();
            this.descriptionBox.ResumeLayout(false);
            this.profileBox.ResumeLayout(false);
            this.profileBox.PerformLayout();
            this.communicationBox.ResumeLayout(false);
            this.communicationBox.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label activeTriggerLabel;
        private System.Windows.Forms.Button addTriggerButton;
        private System.Windows.Forms.ComboBox availableTriggerBox;
        private System.Windows.Forms.Label availableTriggersText;
        private SoundSwitch.UI.Component.IconTextComboBox communicationComboBox;
        private System.Windows.Forms.Label communicationLabel;
        private System.Windows.Forms.Button communicationRemoveButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.GroupBox descriptionBox;
        private System.Windows.Forms.Label descriptionLabel;
        private SoundSwitch.UI.Component.HotKeyTextBox hotKeyControl;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.CheckBox notifyCheckbox;
        private SoundSwitch.UI.Component.IconTextComboBox playbackComboBox;
        private System.Windows.Forms.Label playbackLabel;
        private System.Windows.Forms.Button playbackRemoveButton;
        private System.Windows.Forms.GroupBox profileBox;
        private SoundSwitch.UI.Component.IconTextComboBox recordingComboBox;
        private System.Windows.Forms.Label recordingLabel;
        private System.Windows.Forms.Button recordingRemoveButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button selectProgramButton;
        private System.Windows.Forms.OpenFileDialog selectProgramDialog;
        private System.Windows.Forms.ListBox setTriggerBox;
        private System.Windows.Forms.CheckBox switchDefaultCheckBox;
        private System.Windows.Forms.TextBox textInput;
        private System.Windows.Forms.GroupBox triggerBox;
        private System.Windows.Forms.Label triggerLabel;

        #endregion

        private System.Windows.Forms.CheckBox restoreDevicesCheckBox;
        private System.Windows.Forms.Button communicationRecordingRemoveButton;
        private System.Windows.Forms.Label communicationRecordingLabel;
        private Component.IconTextComboBox communicationRecordingComboBox;
        private System.Windows.Forms.GroupBox communicationBox;
        private System.Windows.Forms.CheckBox switchForegroundCheckbox;
    }
}