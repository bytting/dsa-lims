namespace DSA_lims
{
    partial class FormOrderAddPrepMeth
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOrderAddPrepMeth));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboxPreparationMethod = new System.Windows.Forms.ComboBox();
            this.cbPrepsAlreadyExists = new System.Windows.Forms.CheckBox();
            this.cboxPrepMethLaboratory = new System.Windows.Forms.ComboBox();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 309);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(469, 28);
            this.panel1.TabIndex = 7;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(269, 0);
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
            this.btnOk.Location = new System.Drawing.Point(369, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Preparation method";
            // 
            // cboxPreparationMethod
            // 
            this.cboxPreparationMethod.DisplayMember = "Name";
            this.cboxPreparationMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPreparationMethod.FormattingEnabled = true;
            this.cboxPreparationMethod.Location = new System.Drawing.Point(136, 90);
            this.cboxPreparationMethod.Name = "cboxPreparationMethod";
            this.cboxPreparationMethod.Size = new System.Drawing.Size(286, 21);
            this.cboxPreparationMethod.TabIndex = 9;
            this.cboxPreparationMethod.ValueMember = "Id";
            // 
            // cbPrepsAlreadyExists
            // 
            this.cbPrepsAlreadyExists.AutoSize = true;
            this.cbPrepsAlreadyExists.Location = new System.Drawing.Point(136, 26);
            this.cbPrepsAlreadyExists.Name = "cbPrepsAlreadyExists";
            this.cbPrepsAlreadyExists.Size = new System.Drawing.Size(227, 17);
            this.cbPrepsAlreadyExists.TabIndex = 10;
            this.cbPrepsAlreadyExists.Text = "Preparation(s) already exists at laboratory...";
            this.cbPrepsAlreadyExists.UseVisualStyleBackColor = true;
            this.cbPrepsAlreadyExists.CheckedChanged += new System.EventHandler(this.cbPrepsAlreadyExists_CheckedChanged);
            // 
            // cboxPrepMethLaboratory
            // 
            this.cboxPrepMethLaboratory.DisplayMember = "Name";
            this.cboxPrepMethLaboratory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxPrepMethLaboratory.FormattingEnabled = true;
            this.cboxPrepMethLaboratory.Location = new System.Drawing.Point(136, 49);
            this.cboxPrepMethLaboratory.Name = "cboxPrepMethLaboratory";
            this.cboxPrepMethLaboratory.Size = new System.Drawing.Size(286, 21);
            this.cboxPrepMethLaboratory.TabIndex = 11;
            this.cboxPrepMethLaboratory.ValueMember = "Id";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(136, 145);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(286, 132);
            this.tbComment.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(79, 148);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Comment";
            // 
            // tbCount
            // 
            this.tbCount.Location = new System.Drawing.Point(136, 118);
            this.tbCount.MaxLength = 32;
            this.tbCount.Name = "tbCount";
            this.tbCount.Size = new System.Drawing.Size(286, 20);
            this.tbCount.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Count";
            // 
            // FormOrderAddPrepMeth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 337);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.cboxPrepMethLaboratory);
            this.Controls.Add(this.cbPrepsAlreadyExists);
            this.Controls.Add(this.cboxPreparationMethod);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOrderAddPrepMeth";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Order prep. methods";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxPreparationMethod;
        private System.Windows.Forms.CheckBox cbPrepsAlreadyExists;
        private System.Windows.Forms.ComboBox cboxPrepMethLaboratory;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbCount;
        private System.Windows.Forms.Label label3;
    }
}