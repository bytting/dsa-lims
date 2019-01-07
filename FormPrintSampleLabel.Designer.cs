namespace DSA_lims
{
    partial class FormPrintSampleLabel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrintSampleLabel));
            this.cboxPrinters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.cboxPaperSizes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbLandscape = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboxPrinters
            // 
            this.cboxPrinters.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPrinters.FormattingEnabled = true;
            this.cboxPrinters.Location = new System.Drawing.Point(33, 44);
            this.cboxPrinters.Name = "cboxPrinters";
            this.cboxPrinters.Size = new System.Drawing.Size(339, 21);
            this.cboxPrinters.TabIndex = 0;
            this.cboxPrinters.SelectedIndexChanged += new System.EventHandler(this.cboxPrinters_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select printer";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 306);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(403, 28);
            this.panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(203, 0);
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
            this.btnOk.Location = new System.Drawing.Point(303, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cboxPaperSizes
            // 
            this.cboxPaperSizes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPaperSizes.FormattingEnabled = true;
            this.cboxPaperSizes.Location = new System.Drawing.Point(33, 95);
            this.cboxPaperSizes.Name = "cboxPaperSizes";
            this.cboxPaperSizes.Size = new System.Drawing.Size(339, 21);
            this.cboxPaperSizes.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Paper sizes";
            // 
            // cbLandscape
            // 
            this.cbLandscape.AutoSize = true;
            this.cbLandscape.Location = new System.Drawing.Point(33, 132);
            this.cbLandscape.Name = "cbLandscape";
            this.cbLandscape.Size = new System.Drawing.Size(79, 17);
            this.cbLandscape.TabIndex = 11;
            this.cbLandscape.Text = "Landscape";
            this.cbLandscape.UseVisualStyleBackColor = true;
            // 
            // FormPrintSampleLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 334);
            this.Controls.Add(this.cbLandscape);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboxPaperSizes);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboxPrinters);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPrintSampleLabel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSALims- Print sample label";
            this.Load += new System.EventHandler(this.FormPrintSampleLabel_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboxPrinters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cboxPaperSizes;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbLandscape;
    }
}