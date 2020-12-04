namespace NASR_GUI
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.currentAiracSelection = new System.Windows.Forms.RadioButton();
            this.nextAiracSelection = new System.Windows.Forms.RadioButton();
            this.airacLabel = new System.Windows.Forms.Label();
            this.facilityIDLabel = new System.Windows.Forms.Label();
            this.facilityIdTextbox = new System.Windows.Forms.TextBox();
            this.convertLabel = new System.Windows.Forms.Label();
            this.convertNo = new System.Windows.Forms.RadioButton();
            this.convertYes = new System.Windows.Forms.RadioButton();
            this.convertDescriptionLabel = new System.Windows.Forms.Label();
            this.filePathLabel = new System.Windows.Forms.Label();
            this.chooseDirButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.airacCycleGroupBox = new System.Windows.Forms.GroupBox();
            this.convertGroupBox = new System.Windows.Forms.GroupBox();
            this.startGroupBox = new System.Windows.Forms.GroupBox();
            this.processingDataLabel = new System.Windows.Forms.Label();
            this.processingGroupBox = new System.Windows.Forms.GroupBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.runAgainButton = new System.Windows.Forms.Button();
            this.airacCycleGroupBox.SuspendLayout();
            this.convertGroupBox.SuspendLayout();
            this.startGroupBox.SuspendLayout();
            this.processingGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // currentAiracSelection
            // 
            this.currentAiracSelection.AutoSize = true;
            this.currentAiracSelection.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.currentAiracSelection.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentAiracSelection.Location = new System.Drawing.Point(91, 68);
            this.currentAiracSelection.Margin = new System.Windows.Forms.Padding(6);
            this.currentAiracSelection.Name = "currentAiracSelection";
            this.currentAiracSelection.Size = new System.Drawing.Size(128, 25);
            this.currentAiracSelection.TabIndex = 0;
            this.currentAiracSelection.TabStop = true;
            this.currentAiracSelection.Text = "Current AIRAC";
            this.currentAiracSelection.UseVisualStyleBackColor = true;
            this.currentAiracSelection.CheckedChanged += new System.EventHandler(this.currentAiracSelection_CheckedChanged);
            // 
            // nextAiracSelection
            // 
            this.nextAiracSelection.AutoSize = true;
            this.nextAiracSelection.Checked = true;
            this.nextAiracSelection.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.nextAiracSelection.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nextAiracSelection.Location = new System.Drawing.Point(232, 68);
            this.nextAiracSelection.Margin = new System.Windows.Forms.Padding(6);
            this.nextAiracSelection.Name = "nextAiracSelection";
            this.nextAiracSelection.Size = new System.Drawing.Size(107, 25);
            this.nextAiracSelection.TabIndex = 1;
            this.nextAiracSelection.TabStop = true;
            this.nextAiracSelection.Text = "Next AIRAC";
            this.nextAiracSelection.UseVisualStyleBackColor = true;
            this.nextAiracSelection.CheckedChanged += new System.EventHandler(this.nextAiracSelection_CheckedChanged);
            // 
            // airacLabel
            // 
            this.airacLabel.AutoSize = true;
            this.airacLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.airacLabel.Location = new System.Drawing.Point(67, 29);
            this.airacLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.airacLabel.Name = "airacLabel";
            this.airacLabel.Size = new System.Drawing.Size(296, 25);
            this.airacLabel.TabIndex = 2;
            this.airacLabel.Text = "Which AIRAC Cylce do you want? ";
            // 
            // facilityIDLabel
            // 
            this.facilityIDLabel.AutoSize = true;
            this.facilityIDLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.facilityIDLabel.Location = new System.Drawing.Point(35, 29);
            this.facilityIDLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.facilityIDLabel.Name = "facilityIDLabel";
            this.facilityIDLabel.Size = new System.Drawing.Size(212, 25);
            this.facilityIDLabel.TabIndex = 3;
            this.facilityIDLabel.Text = "What is your Facility ID?";
            // 
            // facilityIdTextbox
            // 
            this.facilityIdTextbox.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.facilityIdTextbox.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.facilityIdTextbox.Location = new System.Drawing.Point(40, 64);
            this.facilityIdTextbox.Name = "facilityIdTextbox";
            this.facilityIdTextbox.Size = new System.Drawing.Size(182, 33);
            this.facilityIdTextbox.TabIndex = 4;
            this.facilityIdTextbox.Text = "FAA";
            // 
            // convertLabel
            // 
            this.convertLabel.AutoSize = true;
            this.convertLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.convertLabel.Location = new System.Drawing.Point(33, 29);
            this.convertLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.convertLabel.Name = "convertLabel";
            this.convertLabel.Size = new System.Drawing.Size(385, 25);
            this.convertLabel.TabIndex = 5;
            this.convertLabel.Text = "Would you like to Convert East Coordinates?";
            // 
            // convertNo
            // 
            this.convertNo.AutoSize = true;
            this.convertNo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.convertNo.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.convertNo.Location = new System.Drawing.Point(232, 194);
            this.convertNo.Margin = new System.Windows.Forms.Padding(6);
            this.convertNo.Name = "convertNo";
            this.convertNo.Size = new System.Drawing.Size(48, 25);
            this.convertNo.TabIndex = 7;
            this.convertNo.TabStop = true;
            this.convertNo.Text = "No";
            this.convertNo.UseVisualStyleBackColor = true;
            // 
            // convertYes
            // 
            this.convertYes.AutoSize = true;
            this.convertYes.Checked = true;
            this.convertYes.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.convertYes.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.convertYes.Location = new System.Drawing.Point(148, 194);
            this.convertYes.Margin = new System.Windows.Forms.Padding(6);
            this.convertYes.Name = "convertYes";
            this.convertYes.Size = new System.Drawing.Size(50, 25);
            this.convertYes.TabIndex = 6;
            this.convertYes.TabStop = true;
            this.convertYes.Text = "Yes";
            this.convertYes.UseVisualStyleBackColor = true;
            // 
            // convertDescriptionLabel
            // 
            this.convertDescriptionLabel.AutoSize = true;
            this.convertDescriptionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.convertDescriptionLabel.Location = new System.Drawing.Point(18, 71);
            this.convertDescriptionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.convertDescriptionLabel.Name = "convertDescriptionLabel";
            this.convertDescriptionLabel.Size = new System.Drawing.Size(425, 105);
            this.convertDescriptionLabel.TabIndex = 8;
            this.convertDescriptionLabel.Text = resources.GetString("convertDescriptionLabel.Text");
            this.convertDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // filePathLabel
            // 
            this.filePathLabel.Location = new System.Drawing.Point(6, 100);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(257, 82);
            this.filePathLabel.TabIndex = 9;
            this.filePathLabel.Text = "filePathLabel";
            this.filePathLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.filePathLabel.Visible = false;
            // 
            // chooseDirButton
            // 
            this.chooseDirButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.chooseDirButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chooseDirButton.Location = new System.Drawing.Point(40, 185);
            this.chooseDirButton.Name = "chooseDirButton";
            this.chooseDirButton.Size = new System.Drawing.Size(182, 34);
            this.chooseDirButton.TabIndex = 10;
            this.chooseDirButton.Text = "Choose Output Location";
            this.chooseDirButton.UseVisualStyleBackColor = true;
            this.chooseDirButton.Click += new System.EventHandler(this.chooseDirButton_Click);
            // 
            // startButton
            // 
            this.startButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.startButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startButton.Location = new System.Drawing.Point(40, 302);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(182, 34);
            this.startButton.TabIndex = 11;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // airacCycleGroupBox
            // 
            this.airacCycleGroupBox.Controls.Add(this.airacLabel);
            this.airacCycleGroupBox.Controls.Add(this.currentAiracSelection);
            this.airacCycleGroupBox.Controls.Add(this.nextAiracSelection);
            this.airacCycleGroupBox.Location = new System.Drawing.Point(20, 12);
            this.airacCycleGroupBox.Name = "airacCycleGroupBox";
            this.airacCycleGroupBox.Size = new System.Drawing.Size(452, 125);
            this.airacCycleGroupBox.TabIndex = 12;
            this.airacCycleGroupBox.TabStop = false;
            // 
            // convertGroupBox
            // 
            this.convertGroupBox.Controls.Add(this.convertLabel);
            this.convertGroupBox.Controls.Add(this.convertYes);
            this.convertGroupBox.Controls.Add(this.convertNo);
            this.convertGroupBox.Controls.Add(this.convertDescriptionLabel);
            this.convertGroupBox.Location = new System.Drawing.Point(20, 140);
            this.convertGroupBox.Name = "convertGroupBox";
            this.convertGroupBox.Size = new System.Drawing.Size(452, 237);
            this.convertGroupBox.TabIndex = 13;
            this.convertGroupBox.TabStop = false;
            // 
            // startGroupBox
            // 
            this.startGroupBox.Controls.Add(this.facilityIDLabel);
            this.startGroupBox.Controls.Add(this.facilityIdTextbox);
            this.startGroupBox.Controls.Add(this.startButton);
            this.startGroupBox.Controls.Add(this.filePathLabel);
            this.startGroupBox.Controls.Add(this.chooseDirButton);
            this.startGroupBox.Location = new System.Drawing.Point(491, 12);
            this.startGroupBox.Name = "startGroupBox";
            this.startGroupBox.Size = new System.Drawing.Size(269, 365);
            this.startGroupBox.TabIndex = 14;
            this.startGroupBox.TabStop = false;
            // 
            // processingDataLabel
            // 
            this.processingDataLabel.Enabled = false;
            this.processingDataLabel.Font = new System.Drawing.Font("Segoe UI", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.processingDataLabel.Location = new System.Drawing.Point(6, 15);
            this.processingDataLabel.Name = "processingDataLabel";
            this.processingDataLabel.Size = new System.Drawing.Size(545, 68);
            this.processingDataLabel.TabIndex = 15;
            this.processingDataLabel.Text = "Getting AIRAC date.";
            this.processingDataLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.processingDataLabel.Visible = false;
            // 
            // processingGroupBox
            // 
            this.processingGroupBox.Controls.Add(this.exitButton);
            this.processingGroupBox.Controls.Add(this.runAgainButton);
            this.processingGroupBox.Controls.Add(this.processingDataLabel);
            this.processingGroupBox.Enabled = false;
            this.processingGroupBox.Location = new System.Drawing.Point(116, 125);
            this.processingGroupBox.Name = "processingGroupBox";
            this.processingGroupBox.Size = new System.Drawing.Size(557, 140);
            this.processingGroupBox.TabIndex = 3;
            this.processingGroupBox.TabStop = false;
            this.processingGroupBox.Visible = false;
            // 
            // exitButton
            // 
            this.exitButton.Enabled = false;
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.exitButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitButton.Location = new System.Drawing.Point(315, 86);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(182, 34);
            this.exitButton.TabIndex = 17;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Visible = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // runAgainButton
            // 
            this.runAgainButton.Enabled = false;
            this.runAgainButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.runAgainButton.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runAgainButton.Location = new System.Drawing.Point(60, 86);
            this.runAgainButton.Name = "runAgainButton";
            this.runAgainButton.Size = new System.Drawing.Size(182, 34);
            this.runAgainButton.TabIndex = 16;
            this.runAgainButton.Text = "Run Again";
            this.runAgainButton.UseVisualStyleBackColor = true;
            this.runAgainButton.Visible = false;
            this.runAgainButton.Click += new System.EventHandler(this.runAgainButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ClientSize = new System.Drawing.Size(788, 390);
            this.Controls.Add(this.processingGroupBox);
            this.Controls.Add(this.startGroupBox);
            this.Controls.Add(this.convertGroupBox);
            this.Controls.Add(this.airacCycleGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MainForm";
            this.Text = "NASR 2 SCT";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.airacCycleGroupBox.ResumeLayout(false);
            this.airacCycleGroupBox.PerformLayout();
            this.convertGroupBox.ResumeLayout(false);
            this.convertGroupBox.PerformLayout();
            this.startGroupBox.ResumeLayout(false);
            this.startGroupBox.PerformLayout();
            this.processingGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton currentAiracSelection;
        private System.Windows.Forms.RadioButton nextAiracSelection;
        private System.Windows.Forms.Label airacLabel;
        private System.Windows.Forms.Label facilityIDLabel;
        private System.Windows.Forms.TextBox facilityIdTextbox;
        private System.Windows.Forms.Label convertLabel;
        private System.Windows.Forms.RadioButton convertNo;
        private System.Windows.Forms.RadioButton convertYes;
        private System.Windows.Forms.Label convertDescriptionLabel;
        private System.Windows.Forms.Label filePathLabel;
        private System.Windows.Forms.Button chooseDirButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.GroupBox airacCycleGroupBox;
        private System.Windows.Forms.GroupBox convertGroupBox;
        private System.Windows.Forms.GroupBox startGroupBox;
        private System.Windows.Forms.Label processingDataLabel;
        private System.Windows.Forms.GroupBox processingGroupBox;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button runAgainButton;
    }
}

