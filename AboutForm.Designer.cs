namespace BoBar
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            titleLabel = new Label();
            versionLabel = new Label();
            developerLabel = new Label();
            linkLabel = new LinkLabel();
            okButton = new Button();
            SuspendLayout();
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            titleLabel.Location = new Point(56, 64);
            titleLabel.Margin = new Padding(6, 0, 6, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(147, 59);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "BoBar";
            // 
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Font = new Font("Segoe UI", 10F);
            versionLabel.ForeColor = SystemColors.GrayText;
            versionLabel.Location = new Point(56, 139);
            versionLabel.Margin = new Padding(6, 0, 6, 0);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(147, 37);
            versionLabel.TabIndex = 1;
            versionLabel.Text = "Version 0.1";
            // 
            // developerLabel
            // 
            developerLabel.AutoSize = true;
            developerLabel.Location = new Point(56, 213);
            developerLabel.Margin = new Padding(6, 0, 6, 0);
            developerLabel.Name = "developerLabel";
            developerLabel.Size = new Size(268, 32);
            developerLabel.TabIndex = 2;
            developerLabel.Text = "by bbbjames + you? <3";
            developerLabel.Click += developerLabel_Click;
            // 
            // linkLabel
            // 
            linkLabel.AutoSize = true;
            linkLabel.Location = new Point(56, 267);
            linkLabel.Margin = new Padding(6, 0, 6, 0);
            linkLabel.Name = "linkLabel";
            linkLabel.Size = new Size(239, 32);
            linkLabel.TabIndex = 3;
            linkLabel.TabStop = true;
            linkLabel.Text = "bobjames.dev/twitter";
            linkLabel.LinkClicked += linkLabel_LinkClicked;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Location = new Point(455, 352);
            okButton.Margin = new Padding(6);
            okButton.Name = "okButton";
            okButton.Size = new Size(139, 64);
            okButton.TabIndex = 4;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(650, 459);
            Controls.Add(okButton);
            Controls.Add(linkLabel);
            Controls.Add(developerLabel);
            Controls.Add(versionLabel);
            Controls.Add(titleLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About BoBar";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label titleLabel;
        private Label versionLabel;
        private Label developerLabel;
        private LinkLabel linkLabel;
        private Button okButton;
    }
}
