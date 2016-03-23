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
            this.startOkButton = new System.Windows.Forms.Button();
            this.startCancelButton = new System.Windows.Forms.Button();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.durationUpDown = new System.Windows.Forms.NumericUpDown();
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.durationUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // urlLabel
            // 
            this.urlLabel.AutoSize = true;
            this.urlLabel.Location = new System.Drawing.Point(53, 30);
            this.urlLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.urlLabel.Name = "urlLabel";
            this.urlLabel.Size = new System.Drawing.Size(134, 17);
            this.urlLabel.TabIndex = 0;
            this.urlLabel.Text = "Boggle Server URL:";
            this.urlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // serverUrlBox
            // 
            this.serverUrlBox.Location = new System.Drawing.Point(197, 26);
            this.serverUrlBox.Margin = new System.Windows.Forms.Padding(4);
            this.serverUrlBox.Name = "serverUrlBox";
            this.serverUrlBox.Size = new System.Drawing.Size(493, 22);
            this.serverUrlBox.TabIndex = 1;
            this.serverUrlBox.Text = "http://bogglecs3500s16.azurewebsites.net";
            // 
            // playerNameLabel
            // 
            this.playerNameLabel.AutoSize = true;
            this.playerNameLabel.Location = new System.Drawing.Point(96, 59);
            this.playerNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.playerNameLabel.Name = "playerNameLabel";
            this.playerNameLabel.Size = new System.Drawing.Size(93, 17);
            this.playerNameLabel.TabIndex = 2;
            this.playerNameLabel.Text = "Player Name:";
            this.playerNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // durationLabel
            // 
            this.durationLabel.AutoSize = true;
            this.durationLabel.Location = new System.Drawing.Point(16, 91);
            this.durationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.durationLabel.Name = "durationLabel";
            this.durationLabel.Size = new System.Drawing.Size(175, 17);
            this.durationLabel.TabIndex = 3;
            this.durationLabel.Text = "Game Duration (seconds):";
            this.durationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // playerNameBox
            // 
            this.playerNameBox.Location = new System.Drawing.Point(197, 55);
            this.playerNameBox.Margin = new System.Windows.Forms.Padding(4);
            this.playerNameBox.Name = "playerNameBox";
            this.playerNameBox.Size = new System.Drawing.Size(132, 22);
            this.playerNameBox.TabIndex = 2;
            // 
            // startOkButton
            // 
            this.startOkButton.Location = new System.Drawing.Point(499, 122);
            this.startOkButton.Margin = new System.Windows.Forms.Padding(4);
            this.startOkButton.Name = "startOkButton";
            this.startOkButton.Size = new System.Drawing.Size(100, 28);
            this.startOkButton.TabIndex = 4;
            this.startOkButton.Text = "OK";
            this.startOkButton.UseVisualStyleBackColor = true;
            this.startOkButton.Click += new System.EventHandler(this.startOkButton_Click);
            // 
            // startCancelButton
            // 
            this.startCancelButton.Location = new System.Drawing.Point(607, 122);
            this.startCancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.startCancelButton.Name = "startCancelButton";
            this.startCancelButton.Size = new System.Drawing.Size(100, 28);
            this.startCancelButton.TabIndex = 5;
            this.startCancelButton.Text = "Cancel";
            this.startCancelButton.UseVisualStyleBackColor = true;
            this.startCancelButton.Click += new System.EventHandler(this.startCancelButton_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(723, 28);
            this.menuStrip.TabIndex = 8;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            this.contentsToolStripMenuItem.Size = new System.Drawing.Size(142, 26);
            this.contentsToolStripMenuItem.Text = "Contents";
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.contentsToolStripMenuItem_Click);
            // 
            // durationUpDown
            // 
            this.durationUpDown.Location = new System.Drawing.Point(197, 87);
            this.durationUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.durationUpDown.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.durationUpDown.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.durationUpDown.Name = "durationUpDown";
            this.durationUpDown.Size = new System.Drawing.Size(55, 22);
            this.durationUpDown.TabIndex = 9;
            this.durationUpDown.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 165);
            this.Controls.Add(this.durationUpDown);
            this.Controls.Add(this.startCancelButton);
            this.Controls.Add(this.startOkButton);
            this.Controls.Add(this.playerNameBox);
            this.Controls.Add(this.durationLabel);
            this.Controls.Add(this.playerNameLabel);
            this.Controls.Add(this.serverUrlBox);
            this.Controls.Add(this.urlLabel);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "StartForm";
            this.Text = "Start Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StartForm_FormClosing);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.durationUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label urlLabel;
        private System.Windows.Forms.TextBox serverUrlBox;
        private System.Windows.Forms.Label playerNameLabel;
        private System.Windows.Forms.Label durationLabel;
        private System.Windows.Forms.TextBox playerNameBox;
        private System.Windows.Forms.Button startOkButton;
        private System.Windows.Forms.Button startCancelButton;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown durationUpDown;
    }
}