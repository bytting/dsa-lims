﻿namespace DSA_lims
{
    partial class FormGeometry
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGeometry));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbMinFillHeight = new System.Windows.Forms.TextBox();
            this.tbMaxFillHeight = new System.Windows.Forms.TextBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tbVolume = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbRadius = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Minimum fillheight (mm)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Maximum fillheight (mm)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 183);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Comment";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 344);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(398, 28);
            this.panel1.TabIndex = 5;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(198, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(298, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Name";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(141, 22);
            this.tbName.MaxLength = 80;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(234, 20);
            this.tbName.TabIndex = 0;
            // 
            // tbMinFillHeight
            // 
            this.tbMinFillHeight.Location = new System.Drawing.Point(141, 48);
            this.tbMinFillHeight.MaxLength = 32;
            this.tbMinFillHeight.Name = "tbMinFillHeight";
            this.tbMinFillHeight.Size = new System.Drawing.Size(234, 20);
            this.tbMinFillHeight.TabIndex = 1;
            // 
            // tbMaxFillHeight
            // 
            this.tbMaxFillHeight.Location = new System.Drawing.Point(141, 74);
            this.tbMaxFillHeight.MaxLength = 32;
            this.tbMaxFillHeight.Name = "tbMaxFillHeight";
            this.tbMaxFillHeight.Size = new System.Drawing.Size(234, 20);
            this.tbMaxFillHeight.TabIndex = 2;
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(141, 180);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(234, 137);
            this.tbComment.TabIndex = 4;
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(141, 153);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(234, 21);
            this.cboxInstanceStatus.TabIndex = 3;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 156);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Status";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 103);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Volume (L)";
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(141, 100);
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(234, 20);
            this.tbVolume.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 130);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Radius (mm)";
            // 
            // tbRadius
            // 
            this.tbRadius.Location = new System.Drawing.Point(141, 127);
            this.tbRadius.Name = "tbRadius";
            this.tbRadius.Size = new System.Drawing.Size(234, 20);
            this.tbRadius.TabIndex = 16;
            // 
            // FormGeometry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 372);
            this.Controls.Add(this.tbRadius);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbVolume);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboxInstanceStatus);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.tbMaxFillHeight);
            this.Controls.Add(this.tbMinFillHeight);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGeometry";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Geometry";
            this.Load += new System.EventHandler(this.FormGeometry_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbMinFillHeight;
        private System.Windows.Forms.TextBox tbMaxFillHeight;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbVolume;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbRadius;
    }
}