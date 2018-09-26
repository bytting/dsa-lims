namespace DSA_lims
{
    partial class FormEnergyLine
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEnergyLine));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbNuclide = new System.Windows.Forms.TextBox();
            this.tbTransFrom = new System.Windows.Forms.TextBox();
            this.tbTransTo = new System.Windows.Forms.TextBox();
            this.tbEnergy = new System.Windows.Forms.TextBox();
            this.tbEnergyUnc = new System.Windows.Forms.TextBox();
            this.tbIntensity = new System.Windows.Forms.TextBox();
            this.tbIntensityUnc = new System.Windows.Forms.TextBox();
            this.tbProbOfDecay = new System.Windows.Forms.TextBox();
            this.tbProbOfDecayUnc = new System.Windows.Forms.TextBox();
            this.tbTotInternalConv = new System.Windows.Forms.TextBox();
            this.tbKShellConv = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Nuclide";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Transmission from";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Transmission to";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Energy";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Energy uncertainty";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 146);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Intensity";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 172);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Intensity uncertainty";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 198);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(99, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Probability of decay";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(30, 224);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(152, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Probabilty of decay uncertainty";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(30, 250);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(123, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Total internal conversion";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(33, 276);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(92, 13);
            this.label11.TabIndex = 10;
            this.label11.Text = "KShell conversion";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(33, 349);
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(343, 111);
            this.tbComment.TabIndex = 12;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(33, 333);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "Comment";
            // 
            // tbNuclide
            // 
            this.tbNuclide.Location = new System.Drawing.Point(200, 12);
            this.tbNuclide.Name = "tbNuclide";
            this.tbNuclide.ReadOnly = true;
            this.tbNuclide.Size = new System.Drawing.Size(176, 20);
            this.tbNuclide.TabIndex = 14;
            // 
            // tbTransFrom
            // 
            this.tbTransFrom.Location = new System.Drawing.Point(200, 38);
            this.tbTransFrom.Name = "tbTransFrom";
            this.tbTransFrom.Size = new System.Drawing.Size(176, 20);
            this.tbTransFrom.TabIndex = 15;
            // 
            // tbTransTo
            // 
            this.tbTransTo.Location = new System.Drawing.Point(200, 64);
            this.tbTransTo.Name = "tbTransTo";
            this.tbTransTo.Size = new System.Drawing.Size(176, 20);
            this.tbTransTo.TabIndex = 16;
            // 
            // tbEnergy
            // 
            this.tbEnergy.Location = new System.Drawing.Point(200, 90);
            this.tbEnergy.Name = "tbEnergy";
            this.tbEnergy.Size = new System.Drawing.Size(176, 20);
            this.tbEnergy.TabIndex = 17;
            // 
            // tbEnergyUnc
            // 
            this.tbEnergyUnc.Location = new System.Drawing.Point(200, 117);
            this.tbEnergyUnc.Name = "tbEnergyUnc";
            this.tbEnergyUnc.Size = new System.Drawing.Size(176, 20);
            this.tbEnergyUnc.TabIndex = 18;
            // 
            // tbIntensity
            // 
            this.tbIntensity.Location = new System.Drawing.Point(200, 143);
            this.tbIntensity.Name = "tbIntensity";
            this.tbIntensity.Size = new System.Drawing.Size(176, 20);
            this.tbIntensity.TabIndex = 19;
            // 
            // tbIntensityUnc
            // 
            this.tbIntensityUnc.Location = new System.Drawing.Point(200, 169);
            this.tbIntensityUnc.Name = "tbIntensityUnc";
            this.tbIntensityUnc.Size = new System.Drawing.Size(176, 20);
            this.tbIntensityUnc.TabIndex = 20;
            // 
            // tbProbOfDecay
            // 
            this.tbProbOfDecay.Location = new System.Drawing.Point(200, 195);
            this.tbProbOfDecay.Name = "tbProbOfDecay";
            this.tbProbOfDecay.Size = new System.Drawing.Size(176, 20);
            this.tbProbOfDecay.TabIndex = 21;
            // 
            // tbProbOfDecayUnc
            // 
            this.tbProbOfDecayUnc.Location = new System.Drawing.Point(200, 221);
            this.tbProbOfDecayUnc.Name = "tbProbOfDecayUnc";
            this.tbProbOfDecayUnc.Size = new System.Drawing.Size(176, 20);
            this.tbProbOfDecayUnc.TabIndex = 22;
            // 
            // tbTotInternalConv
            // 
            this.tbTotInternalConv.Location = new System.Drawing.Point(200, 247);
            this.tbTotInternalConv.Name = "tbTotInternalConv";
            this.tbTotInternalConv.Size = new System.Drawing.Size(176, 20);
            this.tbTotInternalConv.TabIndex = 23;
            // 
            // tbKShellConv
            // 
            this.tbKShellConv.Location = new System.Drawing.Point(200, 273);
            this.tbKShellConv.Name = "tbKShellConv";
            this.tbKShellConv.Size = new System.Drawing.Size(176, 20);
            this.tbKShellConv.TabIndex = 24;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 485);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(409, 30);
            this.panel1.TabIndex = 25;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(209, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(309, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 30);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(200, 300);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(176, 21);
            this.cboxInstanceStatus.TabIndex = 26;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(35, 303);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 13);
            this.label13.TabIndex = 27;
            this.label13.Text = "Status";
            // 
            // FormEnergyLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 515);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.cboxInstanceStatus);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbKShellConv);
            this.Controls.Add(this.tbTotInternalConv);
            this.Controls.Add(this.tbProbOfDecayUnc);
            this.Controls.Add(this.tbProbOfDecay);
            this.Controls.Add(this.tbIntensityUnc);
            this.Controls.Add(this.tbIntensity);
            this.Controls.Add(this.tbEnergyUnc);
            this.Controls.Add(this.tbEnergy);
            this.Controls.Add(this.tbTransTo);
            this.Controls.Add(this.tbTransFrom);
            this.Controls.Add(this.tbNuclide);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEnergyLine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA Lims - Energy line";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbNuclide;
        private System.Windows.Forms.TextBox tbTransFrom;
        private System.Windows.Forms.TextBox tbTransTo;
        private System.Windows.Forms.TextBox tbEnergy;
        private System.Windows.Forms.TextBox tbEnergyUnc;
        private System.Windows.Forms.TextBox tbIntensity;
        private System.Windows.Forms.TextBox tbIntensityUnc;
        private System.Windows.Forms.TextBox tbProbOfDecay;
        private System.Windows.Forms.TextBox tbProbOfDecayUnc;
        private System.Windows.Forms.TextBox tbTotInternalConv;
        private System.Windows.Forms.TextBox tbKShellConv;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label13;
    }
}