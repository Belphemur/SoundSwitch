using System.Windows.Forms;
using SoundSwitch.Localization;
using SoundSwitch.UI.UserControls;

namespace SoundSwitch.UI.Forms
{
    partial class AddProfile
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
            SoundSwitch.Common.WinApi.Keyboard.HotKey hotKey1 = new SoundSwitch.Common.WinApi.Keyboard.HotKey();
            this.addProfileGroupBox    = new System.Windows.Forms.GroupBox();
            this.hotKeyControl         = new SoundSwitch.UI.UserControls.HotKeyControl.HotKeyControl();
            this.recordingRemoveButton = new System.Windows.Forms.Button();
            this.playbackRemoveButton  = new System.Windows.Forms.Button();
            this.recordingLabel        = new System.Windows.Forms.Label();
            this.playbackLabel         = new System.Windows.Forms.Label();
            this.recordingComboBox     = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.playbackComboBox      = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.selectProgramButton   = new System.Windows.Forms.Button();
            this.programLabel          = new System.Windows.Forms.Label();
            this.programTextBox        = new System.Windows.Forms.TextBox();
            this.hotKeyLabel           = new System.Windows.Forms.Label();
            this.nameLabel             = new System.Windows.Forms.Label();
            this.nameTextBox           = new System.Windows.Forms.TextBox();
            this.selectProgramDialog   = new System.Windows.Forms.OpenFileDialog();
            this.createButton          = new System.Windows.Forms.Button();
            this.addProfileGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // addProfileGroupBox
            // 
            this.addProfileGroupBox.Anchor       = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.addProfileGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addProfileGroupBox.Controls.Add(this.hotKeyControl);
            this.addProfileGroupBox.Controls.Add(this.recordingRemoveButton);
            this.addProfileGroupBox.Controls.Add(this.playbackRemoveButton);
            this.addProfileGroupBox.Controls.Add(this.recordingLabel);
            this.addProfileGroupBox.Controls.Add(this.playbackLabel);
            this.addProfileGroupBox.Controls.Add(this.recordingComboBox);
            this.addProfileGroupBox.Controls.Add(this.playbackComboBox);
            this.addProfileGroupBox.Controls.Add(this.selectProgramButton);
            this.addProfileGroupBox.Controls.Add(this.programLabel);
            this.addProfileGroupBox.Controls.Add(this.programTextBox);
            this.addProfileGroupBox.Controls.Add(this.hotKeyLabel);
            this.addProfileGroupBox.Controls.Add(this.nameLabel);
            this.addProfileGroupBox.Controls.Add(this.nameTextBox);
            this.addProfileGroupBox.Location = new System.Drawing.Point(12, 12);
            this.addProfileGroupBox.Name     = "addProfileGroupBox";
            this.addProfileGroupBox.Size     = new System.Drawing.Size(526, 244);
            this.addProfileGroupBox.TabIndex = 0;
            this.addProfileGroupBox.TabStop  = false;
            this.addProfileGroupBox.Text     = "Profiles";
            // 
            // hotKeyControl
            // 
            this.hotKeyControl.Anchor         =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hotKeyControl.BackColor      =  System.Drawing.Color.Transparent;
            this.hotKeyControl.Font           =  new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.hotKeyControl.ForceModifiers =  false;
            hotKey1.Enabled                   =  true;
            hotKey1.Keys                      =  System.Windows.Forms.Keys.None;
            hotKey1.Modifier                  =  SoundSwitch.Common.WinApi.Keyboard.HotKey.ModifierKeys.None;
            this.hotKeyControl.HotKey         =  hotKey1;
            this.hotKeyControl.Location       =  new System.Drawing.Point(224, 87);
            this.hotKeyControl.Margin         =  new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.hotKeyControl.Name           =  "hotKeyControl";
            this.hotKeyControl.Size           =  new System.Drawing.Size(222, 23);
            this.hotKeyControl.TabIndex       =  14;
            this.hotKeyControl.ToolTip        =  null;
            this.hotKeyControl.HotKeyIsSet    += new SoundSwitch.UI.UserControls.HotKeyControl.HotKeyIsSetEventHandler(this.hotKeyControl_HotKeyIsSet);
            // 
            // recordingRemoveButton
            // 
            this.recordingRemoveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingRemoveButton.Location                =  new System.Drawing.Point(424, 199);
            this.recordingRemoveButton.Name                    =  "recordingRemoveButton";
            this.recordingRemoveButton.Size                    =  new System.Drawing.Size(22, 22);
            this.recordingRemoveButton.TabIndex                =  13;
            this.recordingRemoveButton.UseVisualStyleBackColor =  true;
            this.recordingRemoveButton.Visible                 =  false;
            this.recordingRemoveButton.Click                   += new System.EventHandler(this.recordingRemoveButton_Click);
            // 
            // playbackRemoveButton
            // 
            this.playbackRemoveButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackRemoveButton.Location                =  new System.Drawing.Point(424, 152);
            this.playbackRemoveButton.Name                    =  "playbackRemoveButton";
            this.playbackRemoveButton.Size                    =  new System.Drawing.Size(22, 22);
            this.playbackRemoveButton.TabIndex                =  12;
            this.playbackRemoveButton.UseVisualStyleBackColor =  true;
            this.playbackRemoveButton.Visible                 =  false;
            this.playbackRemoveButton.Click                   += new System.EventHandler(this.playbackRemoveButton_Click);
            // 
            // recordingLabel
            // 
            this.recordingLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingLabel.AutoSize = true;
            this.recordingLabel.Location = new System.Drawing.Point(3, 185);
            this.recordingLabel.Name     = "recordingLabel";
            this.recordingLabel.Size     = new System.Drawing.Size(56, 13);
            this.recordingLabel.TabIndex = 10;
            this.recordingLabel.Text     = "Recording";
            // 
            // playbackLabel
            // 
            this.playbackLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackLabel.AutoSize = true;
            this.playbackLabel.Location = new System.Drawing.Point(3, 136);
            this.playbackLabel.Name     = "playbackLabel";
            this.playbackLabel.Size     = new System.Drawing.Size(51, 13);
            this.playbackLabel.TabIndex = 9;
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
            this.recordingComboBox.Location             =  new System.Drawing.Point(6, 201);
            this.recordingComboBox.Name                 =  "recordingComboBox";
            this.recordingComboBox.Size                 =  new System.Drawing.Size(412, 21);
            this.recordingComboBox.TabIndex             =  8;
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
            this.playbackComboBox.Location             =  new System.Drawing.Point(6, 152);
            this.playbackComboBox.Name                 =  "playbackComboBox";
            this.playbackComboBox.Size                 =  new System.Drawing.Size(412, 21);
            this.playbackComboBox.TabIndex             =  7;
            this.playbackComboBox.ValueMember          =  "Tag";
            this.playbackComboBox.SelectedIndexChanged += new System.EventHandler(this.playbackComboBox_SelectedIndexChanged);
            // 
            // selectProgramButton
            // 
            this.selectProgramButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectProgramButton.Location                =  new System.Drawing.Point(159, 85);
            this.selectProgramButton.Name                    =  "selectProgramButton";
            this.selectProgramButton.Size                    =  new System.Drawing.Size(24, 20);
            this.selectProgramButton.TabIndex                =  6;
            this.selectProgramButton.Text                    =  "...";
            this.selectProgramButton.UseVisualStyleBackColor =  true;
            this.selectProgramButton.Click                   += new System.EventHandler(this.selectProgramButton_Click);
            // 
            // programLabel
            // 
            this.programLabel.AutoSize = true;
            this.programLabel.Location = new System.Drawing.Point(3, 70);
            this.programLabel.Name     = "programLabel";
            this.programLabel.Size     = new System.Drawing.Size(46, 13);
            this.programLabel.TabIndex = 5;
            this.programLabel.Text     = "Program";
            // 
            // programTextBox
            // 
            this.programTextBox.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.programTextBox.Location = new System.Drawing.Point(6, 85);
            this.programTextBox.Name     = "programTextBox";
            this.programTextBox.Size     = new System.Drawing.Size(147, 20);
            this.programTextBox.TabIndex = 4;
            // 
            // hotKeyLabel
            // 
            this.hotKeyLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.hotKeyLabel.AutoSize = true;
            this.hotKeyLabel.Location = new System.Drawing.Point(221, 70);
            this.hotKeyLabel.Name     = "hotKeyLabel";
            this.hotKeyLabel.Size     = new System.Drawing.Size(46, 13);
            this.hotKeyLabel.TabIndex = 3;
            this.hotKeyLabel.Text     = "Hotkeys";
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor   = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 14);
            this.nameLabel.Name     = "nameLabel";
            this.nameLabel.Size     = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text     = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(6, 30);
            this.nameTextBox.Name     = "nameTextBox";
            this.nameTextBox.Size     = new System.Drawing.Size(145, 20);
            this.nameTextBox.TabIndex = 0;
            // 
            // createButton
            // 
            this.createButton.Anchor                  =  ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location                =  new System.Drawing.Point(463, 356);
            this.createButton.Name                    =  "createButton";
            this.createButton.Size                    =  new System.Drawing.Size(75, 23);
            this.createButton.TabIndex                =  1;
            this.createButton.Text                    =  "Add";
            this.createButton.UseVisualStyleBackColor =  true;
            this.createButton.Click                   += new System.EventHandler(this.createButton_Click);
            // 
            // AddProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize          = new System.Drawing.Size(550, 391);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.addProfileGroupBox);
            this.MinimumSize = new System.Drawing.Size(400, 304);
            this.Name        = "AddProfile";
            this.addProfileGroupBox.ResumeLayout(false);
            this.addProfileGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox addProfileGroupBox;
        private System.Windows.Forms.Button createButton;
        private SoundSwitch.UI.UserControls.HotKeyControl.HotKeyControl hotKeyControl;
        private System.Windows.Forms.Label hotKeyLabel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private SoundSwitch.UI.UserControls.IconTextComboBox playbackComboBox;
        private System.Windows.Forms.Label playbackLabel;
        private System.Windows.Forms.Button playbackRemoveButton;
        private System.Windows.Forms.Label programLabel;
        private System.Windows.Forms.TextBox programTextBox;
        private SoundSwitch.UI.UserControls.IconTextComboBox recordingComboBox;
        private System.Windows.Forms.Label recordingLabel;
        private System.Windows.Forms.Button recordingRemoveButton;
        private System.Windows.Forms.Button selectProgramButton;
        private System.Windows.Forms.OpenFileDialog selectProgramDialog;

        #endregion
    }
}