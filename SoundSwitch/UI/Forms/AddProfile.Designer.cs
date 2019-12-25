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
            this.addProfileGroupBox = new System.Windows.Forms.GroupBox();
            this.recordingRemoveButton = new System.Windows.Forms.Button();
            this.playbackRemoveButton = new System.Windows.Forms.Button();
            this.hotKeyTextBox = new SoundSwitch.UI.UserControls.HotKeyTextBox();
            this.recordingLabel = new System.Windows.Forms.Label();
            this.playbackLabel = new System.Windows.Forms.Label();
            this.recordingComboBox = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.playbackComboBox = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.selectProgramButton = new System.Windows.Forms.Button();
            this.programLabel = new System.Windows.Forms.Label();
            this.programTextBox = new System.Windows.Forms.TextBox();
            this.hotKeyLabel = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.selectProgramDialog = new System.Windows.Forms.OpenFileDialog();
            this.createButton = new System.Windows.Forms.Button();
            this.addProfileGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // addProfileGroupBox
            // 
            this.addProfileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.addProfileGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addProfileGroupBox.Controls.Add(this.recordingRemoveButton);
            this.addProfileGroupBox.Controls.Add(this.playbackRemoveButton);
            this.addProfileGroupBox.Controls.Add(this.hotKeyTextBox);
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
            this.addProfileGroupBox.Location = new System.Drawing.Point(14, 14);
            this.addProfileGroupBox.Name = "addProfileGroupBox";
            this.addProfileGroupBox.Size = new System.Drawing.Size(440, 282);
            this.addProfileGroupBox.TabIndex = 0;
            this.addProfileGroupBox.TabStop = false;
            this.addProfileGroupBox.Text = "Profiles";
            // 
            // recordingRemoveButton
            // 
            this.recordingRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingRemoveButton.Image = global::SoundSwitch.Properties.Resources.delete;
            this.recordingRemoveButton.Location = new System.Drawing.Point(385, 232);
            this.recordingRemoveButton.Name = "recordingRemoveButton";
            this.recordingRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.recordingRemoveButton.TabIndex = 13;
            this.recordingRemoveButton.UseVisualStyleBackColor = true;
            this.recordingRemoveButton.Visible = false;
            this.recordingRemoveButton.Click += new System.EventHandler(this.recordingRemoveButton_Click);
            // 
            // playbackRemoveButton
            // 
            this.playbackRemoveButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackRemoveButton.Image = global::SoundSwitch.Properties.Resources.delete;
            this.playbackRemoveButton.Location = new System.Drawing.Point(385, 175);
            this.playbackRemoveButton.Name = "playbackRemoveButton";
            this.playbackRemoveButton.Size = new System.Drawing.Size(26, 25);
            this.playbackRemoveButton.TabIndex = 12;
            this.playbackRemoveButton.UseVisualStyleBackColor = true;
            this.playbackRemoveButton.Visible = false;
            this.playbackRemoveButton.Click += new System.EventHandler(this.playbackRemoveButton_Click);
            // 
            // hotKeyTextBox
            // 
            this.hotKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hotKeyTextBox.Location = new System.Drawing.Point(238, 99);
            this.hotKeyTextBox.Name = "hotKeyTextBox";
            this.hotKeyTextBox.Size = new System.Drawing.Size(139, 23);
            this.hotKeyTextBox.TabIndex = 11;
            this.hotKeyTextBox.HotKeyChanged += new System.EventHandler<SoundSwitch.UI.UserControls.HotKeyTextBox.Event>(this.hotKeyTextBox_HotKeyChanged);
            // 
            // recordingLabel
            // 
            this.recordingLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingLabel.AutoSize = true;
            this.recordingLabel.Location = new System.Drawing.Point(3, 213);
            this.recordingLabel.Name = "recordingLabel";
            this.recordingLabel.Size = new System.Drawing.Size(61, 15);
            this.recordingLabel.TabIndex = 10;
            this.recordingLabel.Text = "Recording";
            // 
            // playbackLabel
            // 
            this.playbackLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackLabel.AutoSize = true;
            this.playbackLabel.Location = new System.Drawing.Point(3, 157);
            this.playbackLabel.Name = "playbackLabel";
            this.playbackLabel.Size = new System.Drawing.Size(54, 15);
            this.playbackLabel.TabIndex = 9;
            this.playbackLabel.Text = "Playback";
            // 
            // recordingComboBox
            // 
            this.recordingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingComboBox.DataSource = null;
            this.recordingComboBox.DisplayMember = "Tag";
            this.recordingComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.recordingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recordingComboBox.FormattingEnabled = true;
            this.recordingComboBox.Location = new System.Drawing.Point(7, 232);
            this.recordingComboBox.Name = "recordingComboBox";
            this.recordingComboBox.Size = new System.Drawing.Size(370, 24);
            this.recordingComboBox.TabIndex = 8;
            this.recordingComboBox.ValueMember = "Tag";
            this.recordingComboBox.SelectedIndexChanged += new System.EventHandler(this.recordingComboBox_SelectedIndexChanged);
            // 
            // playbackComboBox
            // 
            this.playbackComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackComboBox.DataSource = null;
            this.playbackComboBox.DisplayMember = "Tag";
            this.playbackComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.playbackComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playbackComboBox.FormattingEnabled = true;
            this.playbackComboBox.Location = new System.Drawing.Point(7, 175);
            this.playbackComboBox.Name = "playbackComboBox";
            this.playbackComboBox.Size = new System.Drawing.Size(370, 24);
            this.playbackComboBox.TabIndex = 7;
            this.playbackComboBox.ValueMember = "Tag";
            this.playbackComboBox.SelectedIndexChanged += new System.EventHandler(this.playbackComboBox_SelectedIndexChanged);
            // 
            // selectProgramButton
            // 
            this.selectProgramButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectProgramButton.Location = new System.Drawing.Point(162, 98);
            this.selectProgramButton.Name = "selectProgramButton";
            this.selectProgramButton.Size = new System.Drawing.Size(28, 23);
            this.selectProgramButton.TabIndex = 6;
            this.selectProgramButton.Text = "...";
            this.selectProgramButton.UseVisualStyleBackColor = true;
            this.selectProgramButton.Click += new System.EventHandler(this.selectProgramButton_Click);
            // 
            // programLabel
            // 
            this.programLabel.AutoSize = true;
            this.programLabel.Location = new System.Drawing.Point(3, 81);
            this.programLabel.Name = "programLabel";
            this.programLabel.Size = new System.Drawing.Size(53, 15);
            this.programLabel.TabIndex = 5;
            this.programLabel.Text = "Program";
            // 
            // programTextBox
            // 
            this.programTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.programTextBox.Location = new System.Drawing.Point(7, 98);
            this.programTextBox.Name = "programTextBox";
            this.programTextBox.Size = new System.Drawing.Size(147, 23);
            this.programTextBox.TabIndex = 4;
            // 
            // hotKeyLabel
            // 
            this.hotKeyLabel.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hotKeyLabel.AutoSize = true;
            this.hotKeyLabel.Location = new System.Drawing.Point(234, 81);
            this.hotKeyLabel.Name = "hotKeyLabel";
            this.hotKeyLabel.Size = new System.Drawing.Size(50, 15);
            this.hotKeyLabel.TabIndex = 3;
            this.hotKeyLabel.Text = "Hotkeys";
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 16);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(39, 15);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(7, 35);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(168, 23);
            this.nameTextBox.TabIndex = 0;
            // 
            // createButton
            // 
            this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location = new System.Drawing.Point(366, 303);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(87, 27);
            this.createButton.TabIndex = 1;
            this.createButton.Text = "Add";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // AddProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 344);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.addProfileGroupBox);
            this.MinimumSize = new System.Drawing.Size(464, 345);
            this.Name = "AddProfile";
            this.addProfileGroupBox.ResumeLayout(false);
            this.addProfileGroupBox.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox addProfileGroupBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label hotKeyLabel;
        private System.Windows.Forms.OpenFileDialog selectProgramDialog;
        private System.Windows.Forms.Label programLabel;
        private System.Windows.Forms.TextBox programTextBox;
        private System.Windows.Forms.Button selectProgramButton;
        private System.Windows.Forms.Label playbackLabel;
        private System.Windows.Forms.Label recordingLabel;
        private System.Windows.Forms.Button recordingRemoveButton;
        private System.Windows.Forms.Button playbackRemoveButton;
        private SoundSwitch.UI.UserControls.HotKeyTextBox hotKeyTextBox;
        private System.Windows.Forms.Button createButton;
        private SoundSwitch.UI.UserControls.IconTextComboBox playbackComboBox;
        private SoundSwitch.UI.UserControls.IconTextComboBox recordingComboBox;
    }
}