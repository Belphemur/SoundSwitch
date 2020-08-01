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
            this.triggerBox                = new System.Windows.Forms.GroupBox();
            this.triggerLabel              = new System.Windows.Forms.Label();
            this.selectProgramButton       = new System.Windows.Forms.Button();
            this.descriptionBox            = new System.Windows.Forms.GroupBox();
            this.descriptionLabel          = new System.Windows.Forms.Label();
            this.textInput                 = new System.Windows.Forms.TextBox();
            this.hotKeyControl             = new SoundSwitch.UI.Component.HotKeyTextBox();
            this.deleteButton              = new System.Windows.Forms.Button();
            this.activeTriggerLabel        = new System.Windows.Forms.Label();
            this.availableTriggerBox       = new System.Windows.Forms.ComboBox();
            this.addTriggerButton          = new System.Windows.Forms.Button();
            this.availableTriggersText     = new System.Windows.Forms.Label();
            this.setTriggerBox             = new System.Windows.Forms.ListBox();
            this.selectProgramDialog       = new System.Windows.Forms.OpenFileDialog();
            this.profileBox                = new System.Windows.Forms.GroupBox();
            this.notifyCheckbox            = new System.Windows.Forms.CheckBox();
            this.nameLabel                 = new System.Windows.Forms.Label();
            this.nameTextBox               = new System.Windows.Forms.TextBox();
            this.communicationRemoveButton = new System.Windows.Forms.Button();
            this.communicationLabel        = new System.Windows.Forms.Label();
            this.communicationComboBox     = new SoundSwitch.UI.Component.IconTextComboBox();
            this.switchDefaultCheckBox     = new System.Windows.Forms.CheckBox();
            this.recordingRemoveButton     = new System.Windows.Forms.Button();
            this.playbackRemoveButton      = new System.Windows.Forms.Button();
            this.recordingLabel            = new System.Windows.Forms.Label();
            this.playbackLabel             = new System.Windows.Forms.Label();
            this.recordingComboBox         = new SoundSwitch.UI.Component.IconTextComboBox();
            this.playbackComboBox          = new SoundSwitch.UI.Component.IconTextComboBox();
            this.saveButton                = new System.Windows.Forms.Button();
            this.triggerBox.SuspendLayout();
            this.descriptionBox.SuspendLayout();
            this.profileBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // triggerBox
            // 
            this.triggerBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.triggerBox.Controls.Add(this.triggerLabel);
            this.triggerBox.Controls.Add(this.selectProgramButton);
            this.triggerBox.Controls.Add(this.descriptionBox);
            this.triggerBox.Controls.Add(this.textInput);
            this.triggerBox.Controls.Add(this.hotKeyControl);
            this.triggerBox.Controls.Add(this.deleteButton);
            this.triggerBox.Controls.Add(this.activeTriggerLabel);
            this.triggerBox.Controls.Add(this.availableTriggerBox);
            this.triggerBox.Controls.Add(this.addTriggerButton);
            this.triggerBox.Controls.Add(this.availableTriggersText);
            this.triggerBox.Controls.Add(this.setTriggerBox);
            this.triggerBox.Location = new System.Drawing.Point(12, 12);
            this.triggerBox.Name     = "triggerBox";
            this.triggerBox.Size     = new System.Drawing.Size(694, 258);
            this.triggerBox.TabIndex = 0;
            this.triggerBox.TabStop  = false;
            this.triggerBox.Text     = "Triggers";
            // 
            // triggerLabel
            // 
            this.triggerLabel.AutoSize = true;
            this.triggerLabel.Location = new System.Drawing.Point(352, 57);
            this.triggerLabel.Name     = "triggerLabel";
            this.triggerLabel.Size     = new System.Drawing.Size(40, 13);
            this.triggerLabel.TabIndex = 10;
            this.triggerLabel.Text     = "Trigger";
            // 
            // selectProgramButton
            // 
            this.selectProgramButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectProgramButton.Location                =  new System.Drawing.Point(650, 73);
            this.selectProgramButton.Name                    =  "selectProgramButton";
            this.selectProgramButton.Size                    =  new System.Drawing.Size(24, 20);
            this.selectProgramButton.TabIndex                =  9;
            this.selectProgramButton.Text                    =  "...";
            this.selectProgramButton.UseVisualStyleBackColor =  true;
            this.selectProgramButton.Click                   += new System.EventHandler(this.selectProgramButton_Click);
            // 
            // descriptionBox
            // 
            this.descriptionBox.Anchor   = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionBox.AutoSize = true;
            this.descriptionBox.Controls.Add(this.descriptionLabel);
            this.descriptionBox.Location = new System.Drawing.Point(154, 167);
            this.descriptionBox.Name     = "descriptionBox";
            this.descriptionBox.Size     = new System.Drawing.Size(162, 69);
            this.descriptionBox.TabIndex = 8;
            this.descriptionBox.TabStop  = false;
            this.descriptionBox.Text     = "Description";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize    = true;
            this.descriptionLabel.Location    = new System.Drawing.Point(6, 23);
            this.descriptionLabel.MaximumSize = new System.Drawing.Size(150, 0);
            this.descriptionLabel.Name        = "descriptionLabel";
            this.descriptionLabel.Size        = new System.Drawing.Size(136, 26);
            this.descriptionLabel.TabIndex    = 8;
            this.descriptionLabel.Text        = "Description of the selected trigger";
            // 
            // textInput
            // 
            this.textInput.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.textInput.Location = new System.Drawing.Point(355, 73);
            this.textInput.Name     = "textInput";
            this.textInput.Size     = new System.Drawing.Size(289, 20);
            this.textInput.TabIndex = 7;
            // 
            // hotKeyControl
            // 
            this.hotKeyControl.Location = new System.Drawing.Point(355, 37);
            this.hotKeyControl.Name     = "hotKeyControl";
            this.hotKeyControl.Size     = new System.Drawing.Size(140, 20);
            this.hotKeyControl.TabIndex = 6;
            // 
            // deleteButton
            // 
            this.deleteButton.Location                =  new System.Drawing.Point(181, 138);
            this.deleteButton.Name                    =  "deleteButton";
            this.deleteButton.Size                    =  new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex                =  5;
            this.deleteButton.Text                    =  "Remove";
            this.deleteButton.UseVisualStyleBackColor =  true;
            this.deleteButton.Click                   += new System.EventHandler(this.deleteButton_Click);
            // 
            // activeTriggerLabel
            // 
            this.activeTriggerLabel.AutoSize = true;
            this.activeTriggerLabel.Location = new System.Drawing.Point(154, 19);
            this.activeTriggerLabel.Name     = "activeTriggerLabel";
            this.activeTriggerLabel.Size     = new System.Drawing.Size(78, 13);
            this.activeTriggerLabel.TabIndex = 4;
            this.activeTriggerLabel.Text     = "Active Triggers";
            // 
            // availableTriggerBox
            // 
            this.availableTriggerBox.DropDownStyle     = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.availableTriggerBox.FormattingEnabled = true;
            this.availableTriggerBox.Location          = new System.Drawing.Point(10, 37);
            this.availableTriggerBox.Name              = "availableTriggerBox";
            this.availableTriggerBox.Size              = new System.Drawing.Size(121, 21);
            this.availableTriggerBox.TabIndex          = 3;
            // 
            // addTriggerButton
            // 
            this.addTriggerButton.Location                =  new System.Drawing.Point(10, 73);
            this.addTriggerButton.Name                    =  "addTriggerButton";
            this.addTriggerButton.Size                    =  new System.Drawing.Size(75, 23);
            this.addTriggerButton.TabIndex                =  2;
            this.addTriggerButton.Text                    =  "Add";
            this.addTriggerButton.UseVisualStyleBackColor =  true;
            this.addTriggerButton.Click                   += new System.EventHandler(this.addTriggerButton_Click);
            // 
            // availableTriggersText
            // 
            this.availableTriggersText.AutoSize = true;
            this.availableTriggersText.Location = new System.Drawing.Point(7, 20);
            this.availableTriggersText.Name     = "availableTriggersText";
            this.availableTriggersText.Size     = new System.Drawing.Size(91, 13);
            this.availableTriggersText.TabIndex = 1;
            this.availableTriggersText.Text     = "Available Triggers";
            // 
            // setTriggerBox
            // 
            this.setTriggerBox.FormattingEnabled    =  true;
            this.setTriggerBox.Location             =  new System.Drawing.Point(154, 37);
            this.setTriggerBox.Name                 =  "setTriggerBox";
            this.setTriggerBox.Size                 =  new System.Drawing.Size(158, 95);
            this.setTriggerBox.TabIndex             =  0;
            this.setTriggerBox.SelectedIndexChanged += new System.EventHandler(this.setTriggerBox_SelectedIndexChanged);
            // 
            // profileBox
            // 
            this.profileBox.Controls.Add(this.notifyCheckbox);
            this.profileBox.Controls.Add(this.nameLabel);
            this.profileBox.Controls.Add(this.nameTextBox);
            this.profileBox.Controls.Add(this.communicationRemoveButton);
            this.profileBox.Controls.Add(this.communicationLabel);
            this.profileBox.Controls.Add(this.communicationComboBox);
            this.profileBox.Controls.Add(this.switchDefaultCheckBox);
            this.profileBox.Controls.Add(this.recordingRemoveButton);
            this.profileBox.Controls.Add(this.playbackRemoveButton);
            this.profileBox.Controls.Add(this.recordingLabel);
            this.profileBox.Controls.Add(this.playbackLabel);
            this.profileBox.Controls.Add(this.recordingComboBox);
            this.profileBox.Controls.Add(this.playbackComboBox);
            this.profileBox.Location = new System.Drawing.Point(12, 297);
            this.profileBox.Name     = "profileBox";
            this.profileBox.Size     = new System.Drawing.Size(690, 264);
            this.profileBox.TabIndex = 1;
            this.profileBox.TabStop  = false;
            this.profileBox.Text     = "Profile";
            // 
            // notifyCheckbox
            // 
            this.notifyCheckbox.AutoSize                = true;
            this.notifyCheckbox.Location                = new System.Drawing.Point(11, 85);
            this.notifyCheckbox.Name                    = "notifyCheckbox";
            this.notifyCheckbox.Size                    = new System.Drawing.Size(167, 17);
            this.notifyCheckbox.TabIndex                = 28;
            this.notifyCheckbox.Text                    = "Notify when profile is triggered";
            this.notifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(8, 20);
            this.nameLabel.Name     = "nameLabel";
            this.nameLabel.Size     = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 27;
            this.nameLabel.Text     = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(11, 36);
            this.nameTextBox.Name     = "nameTextBox";
            this.nameTextBox.Size     = new System.Drawing.Size(182, 20);
            this.nameTextBox.TabIndex = 26;
            // 
            // communicationRemoveButton
            // 
            this.communicationRemoveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationRemoveButton.Image                   =  global::SoundSwitch.Properties.Resources.delete;
            this.communicationRemoveButton.Location                =  new System.Drawing.Point(483, 218);
            this.communicationRemoveButton.Name                    =  "communicationRemoveButton";
            this.communicationRemoveButton.Size                    =  new System.Drawing.Size(22, 22);
            this.communicationRemoveButton.TabIndex                =  25;
            this.communicationRemoveButton.UseVisualStyleBackColor =  true;
            this.communicationRemoveButton.Visible                 =  false;
            this.communicationRemoveButton.Click                   += new System.EventHandler(this.communicationRemoveButton_Click);
            // 
            // communicationLabel
            // 
            this.communicationLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationLabel.AutoSize = true;
            this.communicationLabel.Location = new System.Drawing.Point(7, 203);
            this.communicationLabel.Name     = "communicationLabel";
            this.communicationLabel.Size     = new System.Drawing.Size(79, 13);
            this.communicationLabel.TabIndex = 24;
            this.communicationLabel.Text     = "Communication";
            // 
            // communicationComboBox
            // 
            this.communicationComboBox.Anchor               =  ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.communicationComboBox.DataSource           =  null;
            this.communicationComboBox.DisplayMember        =  "Tag";
            this.communicationComboBox.DrawMode             =  System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.communicationComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.communicationComboBox.FormattingEnabled    =  true;
            this.communicationComboBox.Location             =  new System.Drawing.Point(10, 219);
            this.communicationComboBox.Name                 =  "communicationComboBox";
            this.communicationComboBox.Size                 =  new System.Drawing.Size(467, 21);
            this.communicationComboBox.TabIndex             =  23;
            this.communicationComboBox.ValueMember          =  "Tag";
            this.communicationComboBox.SelectedIndexChanged += new System.EventHandler(this.communicationComboBox_SelectedIndexChanged);
            // 
            // switchDefaultCheckBox
            // 
            this.switchDefaultCheckBox.AutoSize                = true;
            this.switchDefaultCheckBox.Location                = new System.Drawing.Point(11, 62);
            this.switchDefaultCheckBox.Name                    = "switchDefaultCheckBox";
            this.switchDefaultCheckBox.Size                    = new System.Drawing.Size(149, 17);
            this.switchDefaultCheckBox.TabIndex                = 22;
            this.switchDefaultCheckBox.Text                    = "Also switch default device";
            this.switchDefaultCheckBox.UseVisualStyleBackColor = true;
            // 
            // recordingRemoveButton
            // 
            this.recordingRemoveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingRemoveButton.Image                   =  global::SoundSwitch.Properties.Resources.delete;
            this.recordingRemoveButton.Location                =  new System.Drawing.Point(483, 170);
            this.recordingRemoveButton.Name                    =  "recordingRemoveButton";
            this.recordingRemoveButton.Size                    =  new System.Drawing.Size(22, 22);
            this.recordingRemoveButton.TabIndex                =  21;
            this.recordingRemoveButton.UseVisualStyleBackColor =  true;
            this.recordingRemoveButton.Visible                 =  false;
            this.recordingRemoveButton.Click                   += new System.EventHandler(this.recordingRemoveButton_Click);
            // 
            // playbackRemoveButton
            // 
            this.playbackRemoveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackRemoveButton.Image                   =  global::SoundSwitch.Properties.Resources.delete;
            this.playbackRemoveButton.Location                =  new System.Drawing.Point(483, 121);
            this.playbackRemoveButton.Name                    =  "playbackRemoveButton";
            this.playbackRemoveButton.Size                    =  new System.Drawing.Size(22, 22);
            this.playbackRemoveButton.TabIndex                =  20;
            this.playbackRemoveButton.UseVisualStyleBackColor =  true;
            this.playbackRemoveButton.Visible                 =  false;
            this.playbackRemoveButton.Click                   += new System.EventHandler(this.playbackRemoveButton_Click);
            // 
            // recordingLabel
            // 
            this.recordingLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingLabel.AutoSize = true;
            this.recordingLabel.Location = new System.Drawing.Point(7, 155);
            this.recordingLabel.Name     = "recordingLabel";
            this.recordingLabel.Size     = new System.Drawing.Size(56, 13);
            this.recordingLabel.TabIndex = 19;
            this.recordingLabel.Text     = "Recording";
            // 
            // playbackLabel
            // 
            this.playbackLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackLabel.AutoSize = true;
            this.playbackLabel.Location = new System.Drawing.Point(7, 106);
            this.playbackLabel.Name     = "playbackLabel";
            this.playbackLabel.Size     = new System.Drawing.Size(51, 13);
            this.playbackLabel.TabIndex = 18;
            this.playbackLabel.Text     = "Playback";
            // 
            // recordingComboBox
            // 
            this.recordingComboBox.Anchor               =  ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingComboBox.DataSource           =  null;
            this.recordingComboBox.DisplayMember        =  "Tag";
            this.recordingComboBox.DrawMode             =  System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.recordingComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recordingComboBox.FormattingEnabled    =  true;
            this.recordingComboBox.Location             =  new System.Drawing.Point(10, 170);
            this.recordingComboBox.Name                 =  "recordingComboBox";
            this.recordingComboBox.Size                 =  new System.Drawing.Size(467, 21);
            this.recordingComboBox.TabIndex             =  17;
            this.recordingComboBox.ValueMember          =  "Tag";
            this.recordingComboBox.SelectedIndexChanged += new System.EventHandler(this.recordingComboBox_SelectedIndexChanged);
            // 
            // playbackComboBox
            // 
            this.playbackComboBox.Anchor               =  ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackComboBox.DataSource           =  null;
            this.playbackComboBox.DisplayMember        =  "Tag";
            this.playbackComboBox.DrawMode             =  System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.playbackComboBox.DropDownStyle        =  System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playbackComboBox.FormattingEnabled    =  true;
            this.playbackComboBox.Location             =  new System.Drawing.Point(10, 122);
            this.playbackComboBox.Name                 =  "playbackComboBox";
            this.playbackComboBox.Size                 =  new System.Drawing.Size(467, 21);
            this.playbackComboBox.TabIndex             =  16;
            this.playbackComboBox.ValueMember          =  "Tag";
            this.playbackComboBox.SelectedIndexChanged += new System.EventHandler(this.playbackComboBox_SelectedIndexChanged);
            // 
            // saveButton
            // 
            this.saveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location                =  new System.Drawing.Point(627, 567);
            this.saveButton.Name                    =  "saveButton";
            this.saveButton.Size                    =  new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex                =  2;
            this.saveButton.Text                    =  "Save";
            this.saveButton.UseVisualStyleBackColor =  true;
            this.saveButton.Click                   += new System.EventHandler(this.saveButton_Click);
            // 
            // UpsertProfileExtended
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(718, 602);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.profileBox);
            this.Controls.Add(this.triggerBox);
            this.Name = "UpsertProfileExtended";
            this.Text = "UpsertProfileExtended";
            this.triggerBox.ResumeLayout(false);
            this.triggerBox.PerformLayout();
            this.descriptionBox.ResumeLayout(false);
            this.descriptionBox.PerformLayout();
            this.profileBox.ResumeLayout(false);
            this.profileBox.PerformLayout();
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
    }
}