﻿namespace DSA_lims
{
    partial class FormStation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormStation));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbLatitude = new System.Windows.Forms.TextBox();
            this.tbLongitude = new System.Windows.Forms.TextBox();
            this.tbAltitude = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSelectCoordsFromMap = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 340);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(370, 28);
            this.panel1.TabIndex = 7;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(170, 0);
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
            this.btnOk.Location = new System.Drawing.Point(270, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Latitude";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Longitude";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Altitude";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(91, 18);
            this.tbName.MaxLength = 128;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(249, 20);
            this.tbName.TabIndex = 0;
            // 
            // tbLatitude
            // 
            this.tbLatitude.Location = new System.Drawing.Point(91, 73);
            this.tbLatitude.MaxLength = 32;
            this.tbLatitude.Name = "tbLatitude";
            this.tbLatitude.Size = new System.Drawing.Size(249, 20);
            this.tbLatitude.TabIndex = 1;
            // 
            // tbLongitude
            // 
            this.tbLongitude.Location = new System.Drawing.Point(91, 101);
            this.tbLongitude.MaxLength = 32;
            this.tbLongitude.Name = "tbLongitude";
            this.tbLongitude.Size = new System.Drawing.Size(249, 20);
            this.tbLongitude.TabIndex = 2;
            // 
            // tbAltitude
            // 
            this.tbAltitude.Location = new System.Drawing.Point(91, 127);
            this.tbAltitude.MaxLength = 8;
            this.tbAltitude.Name = "tbAltitude";
            this.tbAltitude.Size = new System.Drawing.Size(249, 20);
            this.tbAltitude.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 178);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Comment";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(91, 180);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(249, 130);
            this.tbComment.TabIndex = 4;
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(91, 153);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(249, 21);
            this.cboxInstanceStatus.TabIndex = 19;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 151);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Status";
            // 
            // btnSelectCoordsFromMap
            // 
            this.btnSelectCoordsFromMap.Location = new System.Drawing.Point(91, 44);
            this.btnSelectCoordsFromMap.Name = "btnSelectCoordsFromMap";
            this.btnSelectCoordsFromMap.Size = new System.Drawing.Size(249, 23);
            this.btnSelectCoordsFromMap.TabIndex = 21;
            this.btnSelectCoordsFromMap.Text = "Select coordinates from map";
            this.btnSelectCoordsFromMap.UseVisualStyleBackColor = true;
            this.btnSelectCoordsFromMap.Click += new System.EventHandler(this.btnSelectCoordsFromMap_Click);
            // 
            // FormStation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 368);
            this.Controls.Add(this.btnSelectCoordsFromMap);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cboxInstanceStatus);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbAltitude);
            this.Controls.Add(this.tbLongitude);
            this.Controls.Add(this.tbLatitude);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormStation";
            this.Load += new System.EventHandler(this.FormStation_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbLatitude;
        private System.Windows.Forms.TextBox tbLongitude;
        private System.Windows.Forms.TextBox tbAltitude;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSelectCoordsFromMap;
    }
}