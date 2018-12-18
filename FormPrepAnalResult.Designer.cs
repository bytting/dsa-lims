namespace DSA_lims
{
    partial class FormPrepAnalResult
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrepAnalResult));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cboxNuclides = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbActivity = new System.Windows.Forms.TextBox();
            this.tbUncertainty = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cbActivityApproved = new System.Windows.Forms.CheckBox();
            this.tbDetectionLimit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbDetectionLimitApproved = new System.Windows.Forms.CheckBox();
            this.cbAccredited = new System.Windows.Forms.CheckBox();
            this.cbReportable = new System.Windows.Forms.CheckBox();
            this.cboxSigmaActivity = new System.Windows.Forms.ComboBox();
            this.cboxSigmaMDA = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 310);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(348, 28);
            this.panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(148, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(248, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cboxNuclides
            // 
            this.cboxNuclides.DisplayMember = "Name";
            this.cboxNuclides.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxNuclides.FormattingEnabled = true;
            this.cboxNuclides.Location = new System.Drawing.Point(117, 28);
            this.cboxNuclides.Name = "cboxNuclides";
            this.cboxNuclides.Size = new System.Drawing.Size(194, 21);
            this.cboxNuclides.TabIndex = 9;
            this.cboxNuclides.ValueMember = "Id";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Nuclide";
            // 
            // tbActivity
            // 
            this.tbActivity.Location = new System.Drawing.Point(117, 57);
            this.tbActivity.Name = "tbActivity";
            this.tbActivity.Size = new System.Drawing.Size(194, 20);
            this.tbActivity.TabIndex = 11;
            // 
            // tbUncertainty
            // 
            this.tbUncertainty.Location = new System.Drawing.Point(117, 83);
            this.tbUncertainty.Name = "tbUncertainty";
            this.tbUncertainty.Size = new System.Drawing.Size(194, 20);
            this.tbUncertainty.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Activity";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Uncertainty Abs.";
            // 
            // cbActivityApproved
            // 
            this.cbActivityApproved.AutoSize = true;
            this.cbActivityApproved.Location = new System.Drawing.Point(117, 201);
            this.cbActivityApproved.Name = "cbActivityApproved";
            this.cbActivityApproved.Size = new System.Drawing.Size(108, 17);
            this.cbActivityApproved.TabIndex = 15;
            this.cbActivityApproved.Text = "Activity approved";
            this.cbActivityApproved.UseVisualStyleBackColor = true;
            // 
            // tbDetectionLimit
            // 
            this.tbDetectionLimit.Location = new System.Drawing.Point(117, 109);
            this.tbDetectionLimit.Name = "tbDetectionLimit";
            this.tbDetectionLimit.Size = new System.Drawing.Size(194, 20);
            this.tbDetectionLimit.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Detection limit";
            // 
            // cbDetectionLimitApproved
            // 
            this.cbDetectionLimitApproved.AutoSize = true;
            this.cbDetectionLimitApproved.Location = new System.Drawing.Point(117, 224);
            this.cbDetectionLimitApproved.Name = "cbDetectionLimitApproved";
            this.cbDetectionLimitApproved.Size = new System.Drawing.Size(140, 17);
            this.cbDetectionLimitApproved.TabIndex = 18;
            this.cbDetectionLimitApproved.Text = "Detection limit approved";
            this.cbDetectionLimitApproved.UseVisualStyleBackColor = true;
            // 
            // cbAccredited
            // 
            this.cbAccredited.AutoSize = true;
            this.cbAccredited.Location = new System.Drawing.Point(117, 247);
            this.cbAccredited.Name = "cbAccredited";
            this.cbAccredited.Size = new System.Drawing.Size(77, 17);
            this.cbAccredited.TabIndex = 19;
            this.cbAccredited.Text = "Accredited";
            this.cbAccredited.UseVisualStyleBackColor = true;
            // 
            // cbReportable
            // 
            this.cbReportable.AutoSize = true;
            this.cbReportable.Location = new System.Drawing.Point(117, 270);
            this.cbReportable.Name = "cbReportable";
            this.cbReportable.Size = new System.Drawing.Size(78, 17);
            this.cbReportable.TabIndex = 20;
            this.cbReportable.Text = "Reportable";
            this.cbReportable.UseVisualStyleBackColor = true;
            // 
            // cboxSigmaActivity
            // 
            this.cboxSigmaActivity.DisplayMember = "Name";
            this.cboxSigmaActivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSigmaActivity.FormattingEnabled = true;
            this.cboxSigmaActivity.Location = new System.Drawing.Point(117, 135);
            this.cboxSigmaActivity.Name = "cboxSigmaActivity";
            this.cboxSigmaActivity.Size = new System.Drawing.Size(194, 21);
            this.cboxSigmaActivity.TabIndex = 21;
            this.cboxSigmaActivity.ValueMember = "Id";
            // 
            // cboxSigmaMDA
            // 
            this.cboxSigmaMDA.DisplayMember = "Name";
            this.cboxSigmaMDA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSigmaMDA.FormattingEnabled = true;
            this.cboxSigmaMDA.Location = new System.Drawing.Point(117, 162);
            this.cboxSigmaMDA.Name = "cboxSigmaMDA";
            this.cboxSigmaMDA.Size = new System.Drawing.Size(194, 21);
            this.cboxSigmaMDA.TabIndex = 22;
            this.cboxSigmaMDA.ValueMember = "Id";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(26, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Sigma Act.";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 165);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(63, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "Sigma MDA";
            // 
            // FormPrepAnalResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 338);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboxSigmaMDA);
            this.Controls.Add(this.cboxSigmaActivity);
            this.Controls.Add(this.cbReportable);
            this.Controls.Add(this.cbAccredited);
            this.Controls.Add(this.cbDetectionLimitApproved);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbDetectionLimit);
            this.Controls.Add(this.cbActivityApproved);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbUncertainty);
            this.Controls.Add(this.tbActivity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboxNuclides);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPrepAnalResult";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Result";
            this.Load += new System.EventHandler(this.FormPrepAnalResult_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cboxNuclides;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbActivity;
        private System.Windows.Forms.TextBox tbUncertainty;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbActivityApproved;
        private System.Windows.Forms.TextBox tbDetectionLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbDetectionLimitApproved;
        private System.Windows.Forms.CheckBox cbAccredited;
        private System.Windows.Forms.CheckBox cbReportable;
        private System.Windows.Forms.ComboBox cboxSigmaActivity;
        private System.Windows.Forms.ComboBox cboxSigmaMDA;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}