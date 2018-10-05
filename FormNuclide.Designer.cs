namespace DSA_lims
{
    partial class FormNuclide
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormNuclide));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbNumberOfProtons = new System.Windows.Forms.TextBox();
            this.tbNumberOfNeutrons = new System.Windows.Forms.TextBox();
            this.tbHalflife = new System.Windows.Forms.TextBox();
            this.tbHalflifeUncertainty = new System.Windows.Forms.TextBox();
            this.cboxDecayTypes = new System.Windows.Forms.ComboBox();
            this.tbKXrayEnergy = new System.Windows.Forms.TextBox();
            this.tbFluorescenceYield = new System.Windows.Forms.TextBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 471);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(476, 30);
            this.panel1.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(276, 0);
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
            this.btnOk.Location = new System.Drawing.Point(376, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 30);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Nuclide name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Number of protons";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Number of neutrons";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Halflife (Years)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Halflife unc.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 198);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(61, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Decay type";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(26, 227);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(70, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "KXray energy";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(26, 256);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Fluorescence yield";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(26, 321);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(51, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Comment";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(173, 45);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(271, 20);
            this.tbName.TabIndex = 10;
            // 
            // tbNumberOfProtons
            // 
            this.tbNumberOfProtons.Location = new System.Drawing.Point(173, 75);
            this.tbNumberOfProtons.Name = "tbNumberOfProtons";
            this.tbNumberOfProtons.Size = new System.Drawing.Size(271, 20);
            this.tbNumberOfProtons.TabIndex = 11;
            // 
            // tbNumberOfNeutrons
            // 
            this.tbNumberOfNeutrons.Location = new System.Drawing.Point(173, 105);
            this.tbNumberOfNeutrons.Name = "tbNumberOfNeutrons";
            this.tbNumberOfNeutrons.Size = new System.Drawing.Size(271, 20);
            this.tbNumberOfNeutrons.TabIndex = 12;
            // 
            // tbHalflife
            // 
            this.tbHalflife.Location = new System.Drawing.Point(173, 135);
            this.tbHalflife.Name = "tbHalflife";
            this.tbHalflife.Size = new System.Drawing.Size(271, 20);
            this.tbHalflife.TabIndex = 13;
            // 
            // tbHalflifeUncertainty
            // 
            this.tbHalflifeUncertainty.Location = new System.Drawing.Point(173, 165);
            this.tbHalflifeUncertainty.Name = "tbHalflifeUncertainty";
            this.tbHalflifeUncertainty.Size = new System.Drawing.Size(271, 20);
            this.tbHalflifeUncertainty.TabIndex = 14;
            // 
            // cboxDecayTypes
            // 
            this.cboxDecayTypes.DisplayMember = "Name";
            this.cboxDecayTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxDecayTypes.FormattingEnabled = true;
            this.cboxDecayTypes.Location = new System.Drawing.Point(173, 195);
            this.cboxDecayTypes.Name = "cboxDecayTypes";
            this.cboxDecayTypes.Size = new System.Drawing.Size(271, 21);
            this.cboxDecayTypes.TabIndex = 15;
            this.cboxDecayTypes.ValueMember = "Id";
            // 
            // tbKXrayEnergy
            // 
            this.tbKXrayEnergy.Location = new System.Drawing.Point(173, 226);
            this.tbKXrayEnergy.Name = "tbKXrayEnergy";
            this.tbKXrayEnergy.Size = new System.Drawing.Size(271, 20);
            this.tbKXrayEnergy.TabIndex = 16;
            // 
            // tbFluorescenceYield
            // 
            this.tbFluorescenceYield.Location = new System.Drawing.Point(173, 256);
            this.tbFluorescenceYield.Name = "tbFluorescenceYield";
            this.tbFluorescenceYield.Size = new System.Drawing.Size(271, 20);
            this.tbFluorescenceYield.TabIndex = 17;
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(173, 318);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(271, 125);
            this.tbComment.TabIndex = 18;
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(173, 287);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(271, 21);
            this.cboxInstanceStatus.TabIndex = 19;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(26, 290);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(37, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Status";
            // 
            // FormNuclide
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 501);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.cboxInstanceStatus);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.tbFluorescenceYield);
            this.Controls.Add(this.tbKXrayEnergy);
            this.Controls.Add(this.cboxDecayTypes);
            this.Controls.Add(this.tbHalflifeUncertainty);
            this.Controls.Add(this.tbHalflife);
            this.Controls.Add(this.tbNumberOfNeutrons);
            this.Controls.Add(this.tbNumberOfProtons);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormNuclide";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add/Edit nuclide";
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
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbNumberOfProtons;
        private System.Windows.Forms.TextBox tbNumberOfNeutrons;
        private System.Windows.Forms.TextBox tbHalflife;
        private System.Windows.Forms.TextBox tbHalflifeUncertainty;
        private System.Windows.Forms.ComboBox cboxDecayTypes;
        private System.Windows.Forms.TextBox tbKXrayEnergy;
        private System.Windows.Forms.TextBox tbFluorescenceYield;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label10;
    }
}