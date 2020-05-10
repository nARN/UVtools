﻿namespace PrusaSL1Viewer
{
    partial class FrmLoading
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLoading));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lbDescription = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.lbElapsedTime = new System.Windows.Forms.Label();
            this.lbWait = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar.Location = new System.Drawing.Point(0, 74);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(432, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 0;
            // 
            // lbDescription
            // 
            this.lbDescription.AutoSize = true;
            this.lbDescription.Location = new System.Drawing.Point(12, 18);
            this.lbDescription.Name = "lbDescription";
            this.lbDescription.Size = new System.Drawing.Size(35, 13);
            this.lbDescription.TabIndex = 1;
            this.lbDescription.Text = "label1";
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // lbElapsedTime
            // 
            this.lbElapsedTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lbElapsedTime.AutoSize = true;
            this.lbElapsedTime.Location = new System.Drawing.Point(12, 46);
            this.lbElapsedTime.Name = "lbElapsedTime";
            this.lbElapsedTime.Size = new System.Drawing.Size(30, 13);
            this.lbElapsedTime.TabIndex = 2;
            this.lbElapsedTime.Text = "Time";
            // 
            // lbWait
            // 
            this.lbWait.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbWait.AutoSize = true;
            this.lbWait.Location = new System.Drawing.Point(350, 46);
            this.lbWait.Name = "lbWait";
            this.lbWait.Size = new System.Drawing.Size(70, 13);
            this.lbWait.TabIndex = 3;
            this.lbWait.Text = "Please wait...";
            // 
            // FrmLoading
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 97);
            this.ControlBox = false;
            this.Controls.Add(this.lbWait);
            this.Controls.Add(this.lbElapsedTime);
            this.Controls.Add(this.lbDescription);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLoading";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Loading";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lbDescription;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label lbElapsedTime;
        private System.Windows.Forms.Label lbWait;
    }
}