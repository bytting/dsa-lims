namespace DSA_lims
{
    partial class FormPreparationMethod
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPreparationMethod));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbDestructive = new System.Windows.Forms.CheckBox();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbDescriptionLink = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbShortName = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 312);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(359, 28);
            this.panel1.TabIndex = 10;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(159, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(259, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 6;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Description link";
            // 
            // cbDestructive
            // 
            this.cbDestructive.AutoSize = true;
            this.cbDestructive.Location = new System.Drawing.Point(113, 99);
            this.cbDestructive.Name = "cbDestructive";
            this.cbDestructive.Size = new System.Drawing.Size(80, 17);
            this.cbDestructive.TabIndex = 3;
            this.cbDestructive.Text = "Destructive";
            this.cbDestructive.UseVisualStyleBackColor = true;
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(113, 122);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(218, 21);
            this.cboxInstanceStatus.TabIndex = 4;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 23;
            this.label3.Text = "Status";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 152);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(51, 13);
            this.label4.TabIndex = 24;
            this.label4.Text = "Comment";
            // 
            // tbComment
            // 
            this.tbComment.Location = new System.Drawing.Point(113, 149);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Multiline = true;
            this.tbComment.Name = "tbComment";
            this.tbComment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbComment.Size = new System.Drawing.Size(218, 129);
            this.tbComment.TabIndex = 5;
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(113, 21);
            this.tbName.MaxLength = 200;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(218, 20);
            this.tbName.TabIndex = 0;
            // 
            // tbDescriptionLink
            // 
            this.tbDescriptionLink.Location = new System.Drawing.Point(113, 73);
            this.tbDescriptionLink.MaxLength = 1024;
            this.tbDescriptionLink.Name = "tbDescriptionLink";
            this.tbDescriptionLink.Size = new System.Drawing.Size(218, 20);
            this.tbDescriptionLink.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Short name";
            // 
            // tbShortName
            // 
            this.tbShortName.Location = new System.Drawing.Point(113, 47);
            this.tbShortName.MaxLength = 20;
            this.tbShortName.Name = "tbShortName";
            this.tbShortName.Size = new System.Drawing.Size(218, 20);
            this.tbShortName.TabIndex = 1;
            // 
            // FormPreparationMethod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 340);
            this.Controls.Add(this.tbShortName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbDescriptionLink);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.tbComment);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboxInstanceStatus);
            this.Controls.Add(this.cbDestructive);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPreparationMethod";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormPreparationMethod";
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
        private System.Windows.Forms.CheckBox cbDestructive;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbDescriptionLink;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbShortName;
    }
}