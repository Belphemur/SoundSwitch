using System.Windows.Forms;
using SoundSwitch.Framework.Audio.Device;
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
            this.recordingLabel = new System.Windows.Forms.Label();
            this.playbackLabel = new System.Windows.Forms.Label();
            this.recordingComboBox = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.playbackComboBox = new SoundSwitch.UI.UserControls.IconTextComboBox();
            this.selectProgramButton = new System.Windows.Forms.Button();
            this.programLabel = new System.Windows.Forms.Label();
            this.programTextBox = new System.Windows.Forms.TextBox();
            this.hotKeyLabel = new System.Windows.Forms.Label();
            this.hotKeyTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.selectProgramDialog = new System.Windows.Forms.OpenFileDialog();
            this.createButton = new System.Windows.Forms.Button();
            this.addProfileGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // addProfileGroupBox
            // 
            this.addProfileGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addProfileGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addProfileGroupBox.Controls.Add(this.recordingLabel);
            this.addProfileGroupBox.Controls.Add(this.playbackLabel);
            this.addProfileGroupBox.Controls.Add(this.recordingComboBox);
            this.addProfileGroupBox.Controls.Add(this.playbackComboBox);
            this.addProfileGroupBox.Controls.Add(this.selectProgramButton);
            this.addProfileGroupBox.Controls.Add(this.programLabel);
            this.addProfileGroupBox.Controls.Add(this.programTextBox);
            this.addProfileGroupBox.Controls.Add(this.hotKeyLabel);
            this.addProfileGroupBox.Controls.Add(this.hotKeyTextBox);
            this.addProfileGroupBox.Controls.Add(this.nameLabel);
            this.addProfileGroupBox.Controls.Add(this.nameTextBox);
            this.addProfileGroupBox.Location = new System.Drawing.Point(12, 12);
            this.addProfileGroupBox.Name = "addProfileGroupBox";
            this.addProfileGroupBox.Size = new System.Drawing.Size(360, 244);
            this.addProfileGroupBox.TabIndex = 0;
            this.addProfileGroupBox.TabStop = false;
            this.addProfileGroupBox.Text = "Profile";
            // 
            // recordingLabel
            // 
            this.recordingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingLabel.AutoSize = true;
            this.recordingLabel.Location = new System.Drawing.Point(3, 185);
            this.recordingLabel.Name = "recordingLabel";
            this.recordingLabel.Size = new System.Drawing.Size(56, 13);
            this.recordingLabel.TabIndex = 10;
            this.recordingLabel.Text = "Recording";
            // 
            // playbackLabel
            // 
            this.playbackLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackLabel.AutoSize = true;
            this.playbackLabel.Location = new System.Drawing.Point(3, 136);
            this.playbackLabel.Name = "playbackLabel";
            this.playbackLabel.Size = new System.Drawing.Size(51, 13);
            this.playbackLabel.TabIndex = 9;
            this.playbackLabel.Text = "Playback";
            // 
            // recordingComboBox
            // 
            this.recordingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.recordingComboBox.DataSource = null;
            this.recordingComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.recordingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recordingComboBox.FormattingEnabled = true;
            this.recordingComboBox.Location = new System.Drawing.Point(6, 201);
            this.recordingComboBox.Name = "recordingComboBox";
            this.recordingComboBox.Size = new System.Drawing.Size(301, 21);
            this.recordingComboBox.TabIndex = 8;
            // 
            // playbackComboBox
            // 
            this.playbackComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.playbackComboBox.DataSource = null;
            this.playbackComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.playbackComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.playbackComboBox.FormattingEnabled = true;
            this.playbackComboBox.Location = new System.Drawing.Point(6, 152);
            this.playbackComboBox.Name = "playbackComboBox";
            this.playbackComboBox.Size = new System.Drawing.Size(301, 21);
            this.playbackComboBox.TabIndex = 7;
            // 
            // selectProgramButton
            // 
            this.selectProgramButton.Location = new System.Drawing.Point(127, 85);
            this.selectProgramButton.Name = "selectProgramButton";
            this.selectProgramButton.Size = new System.Drawing.Size(24, 20);
            this.selectProgramButton.TabIndex = 6;
            this.selectProgramButton.Text = "...";
            this.selectProgramButton.UseVisualStyleBackColor = true;
            this.selectProgramButton.Click += new System.EventHandler(this.selectProgramButton_Click);
            // 
            // programLabel
            // 
            this.programLabel.AutoSize = true;
            this.programLabel.Location = new System.Drawing.Point(3, 70);
            this.programLabel.Name = "programLabel";
            this.programLabel.Size = new System.Drawing.Size(46, 13);
            this.programLabel.TabIndex = 5;
            this.programLabel.Text = "Program";
            // 
            // programTextBox
            // 
            this.programTextBox.Location = new System.Drawing.Point(6, 85);
            this.programTextBox.Name = "programTextBox";
            this.programTextBox.Size = new System.Drawing.Size(115, 20);
            this.programTextBox.TabIndex = 4;
            // 
            // hotKeyLabel
            // 
            this.hotKeyLabel.AutoSize = true;
            this.hotKeyLabel.Location = new System.Drawing.Point(189, 70);
            this.hotKeyLabel.Name = "hotKeyLabel";
            this.hotKeyLabel.Size = new System.Drawing.Size(42, 13);
            this.hotKeyLabel.TabIndex = 3;
            this.hotKeyLabel.Text = "HotKey";
            // 
            // hotKeyTextBox
            // 
            this.hotKeyTextBox.Location = new System.Drawing.Point(192, 86);
            this.hotKeyTextBox.Name = "hotKeyTextBox";
            this.hotKeyTextBox.Size = new System.Drawing.Size(115, 20);
            this.hotKeyTextBox.TabIndex = 2;
            // 
            // nameLabel
            // 
            this.nameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(3, 14);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 1;
            this.nameLabel.Text = "Name";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(6, 30);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(145, 20);
            this.nameTextBox.TabIndex = 0;
            // 
            // selectProgramDialog
            // 
            this.selectProgramDialog.FileName = "program";
            // 
            // createButton
            // 
            this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createButton.Location = new System.Drawing.Point(297, 262);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(75, 23);
            this.createButton.TabIndex = 1;
            this.createButton.Text = "Add";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // AddProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 297);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.addProfileGroupBox);
            this.MinimumSize = new System.Drawing.Size(400, 304);
            this.Name = "AddProfile";
            this.Text = "AddProfile";
            this.addProfileGroupBox.ResumeLayout(false);
            this.addProfileGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox addProfileGroupBox;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label hotKeyLabel;
        private System.Windows.Forms.TextBox hotKeyTextBox;
        private System.Windows.Forms.OpenFileDialog selectProgramDialog;
        private System.Windows.Forms.Label programLabel;
        private System.Windows.Forms.TextBox programTextBox;
        private System.Windows.Forms.Button selectProgramButton;
        private System.Windows.Forms.Label playbackLabel;
        private System.Windows.Forms.Label recordingLabel;
        private IconTextComboBox recordingComboBox;
        private IconTextComboBox playbackComboBox;
        private Button createButton;
    }
}