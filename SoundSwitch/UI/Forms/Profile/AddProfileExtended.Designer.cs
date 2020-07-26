namespace SoundSwitch.UI.Forms.Profile
{
    partial class AddProfileExtended
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
            this.triggerBox = new System.Windows.Forms.GroupBox();
            this.availableTriggerBox = new System.Windows.Forms.ComboBox();
            this.addTriggerButton = new System.Windows.Forms.Button();
            this.availableTriggersText = new System.Windows.Forms.Label();
            this.setTriggerBox = new System.Windows.Forms.ListBox();
            this.activeTriggerLabel = new System.Windows.Forms.Label();
            this.deleteButton = new System.Windows.Forms.Button();
            this.triggerBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // triggerBox
            // 
            this.triggerBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.triggerBox.Controls.Add(this.deleteButton);
            this.triggerBox.Controls.Add(this.activeTriggerLabel);
            this.triggerBox.Controls.Add(this.availableTriggerBox);
            this.triggerBox.Controls.Add(this.addTriggerButton);
            this.triggerBox.Controls.Add(this.availableTriggersText);
            this.triggerBox.Controls.Add(this.setTriggerBox);
            this.triggerBox.Location = new System.Drawing.Point(12, 12);
            this.triggerBox.Name = "triggerBox";
            this.triggerBox.Size = new System.Drawing.Size(690, 187);
            this.triggerBox.TabIndex = 0;
            this.triggerBox.TabStop = false;
            this.triggerBox.Text = "Triggers";
            // 
            // availableTriggerBox
            // 
            this.availableTriggerBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.availableTriggerBox.FormattingEnabled = true;
            this.availableTriggerBox.Location = new System.Drawing.Point(10, 37);
            this.availableTriggerBox.Name = "availableTriggerBox";
            this.availableTriggerBox.Size = new System.Drawing.Size(121, 21);
            this.availableTriggerBox.TabIndex = 3;
            // 
            // addTriggerButton
            // 
            this.addTriggerButton.Location = new System.Drawing.Point(10, 73);
            this.addTriggerButton.Name = "addTriggerButton";
            this.addTriggerButton.Size = new System.Drawing.Size(75, 23);
            this.addTriggerButton.TabIndex = 2;
            this.addTriggerButton.Text = "Add";
            this.addTriggerButton.UseVisualStyleBackColor = true;
            this.addTriggerButton.Click += new System.EventHandler(this.addTriggerButton_Click);
            // 
            // availableTriggersText
            // 
            this.availableTriggersText.AutoSize = true;
            this.availableTriggersText.Location = new System.Drawing.Point(7, 20);
            this.availableTriggersText.Name = "availableTriggersText";
            this.availableTriggersText.Size = new System.Drawing.Size(91, 13);
            this.availableTriggersText.TabIndex = 1;
            this.availableTriggersText.Text = "Available Triggers";
            // 
            // setTriggerBox
            // 
            this.setTriggerBox.FormattingEnabled = true;
            this.setTriggerBox.Location = new System.Drawing.Point(154, 37);
            this.setTriggerBox.Name = "setTriggerBox";
            this.setTriggerBox.Size = new System.Drawing.Size(120, 95);
            this.setTriggerBox.TabIndex = 0;
            this.setTriggerBox.SelectedIndexChanged += new System.EventHandler(this.setTriggerBox_SelectedIndexChanged);
            // 
            // activeTriggerLabel
            // 
            this.activeTriggerLabel.AutoSize = true;
            this.activeTriggerLabel.Location = new System.Drawing.Point(154, 19);
            this.activeTriggerLabel.Name = "activeTriggerLabel";
            this.activeTriggerLabel.Size = new System.Drawing.Size(78, 13);
            this.activeTriggerLabel.TabIndex = 4;
            this.activeTriggerLabel.Text = "Active Triggers";
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(175, 138);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 5;
            this.deleteButton.Text = "Remove";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // AddProfileExtended
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 412);
            this.Controls.Add(this.triggerBox);
            this.Name = "AddProfileExtended";
            this.Text = "AddProfileExtended";
            this.triggerBox.ResumeLayout(false);
            this.triggerBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox triggerBox;
        private System.Windows.Forms.ListBox setTriggerBox;
        private System.Windows.Forms.Label availableTriggersText;
        private System.Windows.Forms.Button addTriggerButton;
        private System.Windows.Forms.ComboBox availableTriggerBox;
        private System.Windows.Forms.Label activeTriggerLabel;
        private System.Windows.Forms.Button deleteButton;
    }
}