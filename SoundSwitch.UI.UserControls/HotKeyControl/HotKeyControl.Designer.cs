namespace SoundSwitch.UI.UserControls.HotKeyControl
{
    public sealed partial class HotKeyControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components      = new System.ComponentModel.Container();
            this.TextBox         = new System.Windows.Forms.TextBox();
            this.ToolTipProvider = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // TextBox
            // 
            this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) |
                                                                         System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox.Location         =  new System.Drawing.Point(0, 0);
            this.TextBox.Name             =  "TextBox";
            this.TextBox.ShortcutsEnabled =  false;
            this.TextBox.Size             =  new System.Drawing.Size(181, 23);
            this.TextBox.TabIndex         =  0;
            this.TextBox.KeyDown          += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyDown);
            this.TextBox.KeyUp            += new System.Windows.Forms.KeyEventHandler(this.TextBox_KeyUp);
            this.TextBox.Leave            += new System.EventHandler(this.TextBox_Leave);
            this.TextBox.AutoSize          = true;
            // 
            // HotKeyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor           = System.Drawing.Color.Transparent;
            this.Controls.Add(this.TextBox);
            this.Font   = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name   = "HotKeyControl";
            this.Size   = new System.Drawing.Size(181, 27);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox TextBox;
        private System.Windows.Forms.ToolTip ToolTipProvider;
    }
}
