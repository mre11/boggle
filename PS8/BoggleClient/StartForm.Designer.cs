namespace BoggleClient
{
    partial class StartForm
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
            this.urlLabel = new System.Windows.Forms.Label();
            this.serverUrlBox = new System.Windows.Forms.TextBox();
            this.playerNameLabel = new System.Windows.Forms.Label();
            this.durationLabel = new System.Windows.Forms.Label();
            this.playerNameBox = new System.Windows.Forms.TextBox();
            this.durationBox = new System.Windows.Forms.TextBox();
            this.startOkButton = new System.Windows.Forms.Button();
            this.startCancelButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // urlLabel
            // 
            this.urlLabel.AutoSize = true;
            this.urlLabel.Location = new System.Drawing.Point(40, 24);
            this.urlLabel.Name = "urlLabel";
            this.urlLabel.Size = new System.Drawing.Size(102, 13);
            this.urlLabel.TabIndex = 0;
            this.urlLabel.Text = "Boggle Server URL:";
            this.urlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // serverUrlBox
            // 
            this.serverUrlBox.Location = new System.Drawing.Point(148, 21);
            this.serverUrlBox.Name = "serverUrlBox";
            this.serverUrlBox.Size = new System.Drawing.Size(371, 20);
            this.serverUrlBox.TabIndex = 1;
            this.serverUrlBox.Text = "http://bogglecs3500s16.azurewebsites.net/BoggleService.svc/";
            // 
            // playerNameLabel
            // 
            this.playerNameLabel.AutoSize = true;
            this.playerNameLabel.Location = new System.Drawing.Point(72, 48);
            this.playerNameLabel.Name = "playerNameLabel";
            this.playerNameLabel.Size = new System.Drawing.Size(70, 13);
            this.playerNameLabel.TabIndex = 2;
            this.playerNameLabel.Text = "Player Name:";
            this.playerNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // durationLabel
            // 
            this.durationLabel.AutoSize = true;
            this.durationLabel.Location = new System.Drawing.Point(12, 74);
            this.durationLabel.Name = "durationLabel";
            this.durationLabel.Size = new System.Drawing.Size(130, 13);
            this.durationLabel.TabIndex = 3;
            this.durationLabel.Text = "Game Duration (seconds):";
            this.durationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // playerNameBox
            // 
            this.playerNameBox.Location = new System.Drawing.Point(148, 45);
            this.playerNameBox.Name = "playerNameBox";
            this.playerNameBox.Size = new System.Drawing.Size(100, 20);
            this.playerNameBox.TabIndex = 2;
            // 
            // durationBox
            // 
            this.durationBox.Location = new System.Drawing.Point(148, 71);
            this.durationBox.Name = "durationBox";
            this.durationBox.Size = new System.Drawing.Size(44, 20);
            this.durationBox.TabIndex = 3;
            // 
            // startOkButton
            // 
            this.startOkButton.Location = new System.Drawing.Point(374, 99);
            this.startOkButton.Name = "startOkButton";
            this.startOkButton.Size = new System.Drawing.Size(75, 23);
            this.startOkButton.TabIndex = 4;
            this.startOkButton.Text = "OK";
            this.startOkButton.UseVisualStyleBackColor = true;
            // 
            // startCancelButton
            // 
            this.startCancelButton.Location = new System.Drawing.Point(455, 99);
            this.startCancelButton.Name = "startCancelButton";
            this.startCancelButton.Size = new System.Drawing.Size(75, 23);
            this.startCancelButton.TabIndex = 5;
            this.startCancelButton.Text = "Cancel";
            this.startCancelButton.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(542, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.contentsToolStripMenuItem.Text = "Contents";
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 134);
            this.Controls.Add(this.startCancelButton);
            this.Controls.Add(this.startOkButton);
            this.Controls.Add(this.durationBox);
            this.Controls.Add(this.playerNameBox);
            this.Controls.Add(this.durationLabel);
            this.Controls.Add(this.playerNameLabel);
            this.Controls.Add(this.serverUrlBox);
            this.Controls.Add(this.urlLabel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "StartForm";
            this.Text = "Start Game";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label urlLabel;
        private System.Windows.Forms.TextBox serverUrlBox;
        private System.Windows.Forms.Label playerNameLabel;
        private System.Windows.Forms.Label durationLabel;
        private System.Windows.Forms.TextBox playerNameBox;
        private System.Windows.Forms.TextBox durationBox;
        private System.Windows.Forms.Button startOkButton;
        private System.Windows.Forms.Button startCancelButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
    }
}