namespace DSA_lims
{
    partial class FormOrderNew
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOrderNew));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboxLaboratory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboxResponsible = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panelDeadline = new System.Windows.Forms.Panel();
            this.tbDeadline = new System.Windows.Forms.TextBox();
            this.btnSelectDeadline = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cboxRequestedSigma = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelCustomer = new System.Windows.Forms.Panel();
            this.tbCustomer = new System.Windows.Forms.TextBox();
            this.btnSelectCustomer = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.panelDeadline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectDeadline)).BeginInit();
            this.panelCustomer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectCustomer)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnCreate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 234);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(436, 28);
            this.panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(202, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(117, 28);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnCreate
            // 
            this.btnCreate.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCreate.Location = new System.Drawing.Point(319, 0);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(117, 28);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 9;
            this.label1.Text = "Laboratory";
            // 
            // cboxLaboratory
            // 
            this.cboxLaboratory.DisplayMember = "Name";
            this.cboxLaboratory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLaboratory.FormattingEnabled = true;
            this.cboxLaboratory.Location = new System.Drawing.Point(138, 59);
            this.cboxLaboratory.Name = "cboxLaboratory";
            this.cboxLaboratory.Size = new System.Drawing.Size(270, 23);
            this.cboxLaboratory.TabIndex = 10;
            this.cboxLaboratory.ValueMember = "Id";
            this.cboxLaboratory.SelectedIndexChanged += new System.EventHandler(this.cboxLaboratory_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Responsible";
            // 
            // cboxResponsible
            // 
            this.cboxResponsible.DisplayMember = "Name";
            this.cboxResponsible.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxResponsible.FormattingEnabled = true;
            this.cboxResponsible.Location = new System.Drawing.Point(138, 88);
            this.cboxResponsible.Name = "cboxResponsible";
            this.cboxResponsible.Size = new System.Drawing.Size(270, 23);
            this.cboxResponsible.TabIndex = 12;
            this.cboxResponsible.ValueMember = "Id";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 120);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 15);
            this.label3.TabIndex = 13;
            this.label3.Text = "Deadline";
            // 
            // panelDeadline
            // 
            this.panelDeadline.Controls.Add(this.tbDeadline);
            this.panelDeadline.Controls.Add(this.btnSelectDeadline);
            this.panelDeadline.Location = new System.Drawing.Point(138, 117);
            this.panelDeadline.Name = "panelDeadline";
            this.panelDeadline.Size = new System.Drawing.Size(270, 24);
            this.panelDeadline.TabIndex = 15;
            // 
            // tbDeadline
            // 
            this.tbDeadline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbDeadline.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.tbDeadline.Location = new System.Drawing.Point(0, 0);
            this.tbDeadline.Name = "tbDeadline";
            this.tbDeadline.ReadOnly = true;
            this.tbDeadline.Size = new System.Drawing.Size(244, 21);
            this.tbDeadline.TabIndex = 1;
            // 
            // btnSelectDeadline
            // 
            this.btnSelectDeadline.BackgroundImage = global::DSA_lims.Properties.Resources.datetime_16;
            this.btnSelectDeadline.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSelectDeadline.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSelectDeadline.InitialImage = null;
            this.btnSelectDeadline.Location = new System.Drawing.Point(244, 0);
            this.btnSelectDeadline.Name = "btnSelectDeadline";
            this.btnSelectDeadline.Size = new System.Drawing.Size(26, 24);
            this.btnSelectDeadline.TabIndex = 2;
            this.btnSelectDeadline.TabStop = false;
            this.btnSelectDeadline.Click += new System.EventHandler(this.btnSelectDeadline_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 15);
            this.label4.TabIndex = 16;
            this.label4.Text = "Requested sigma";
            // 
            // cboxRequestedSigma
            // 
            this.cboxRequestedSigma.DisplayMember = "Name";
            this.cboxRequestedSigma.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxRequestedSigma.FormattingEnabled = true;
            this.cboxRequestedSigma.Location = new System.Drawing.Point(138, 146);
            this.cboxRequestedSigma.Name = "cboxRequestedSigma";
            this.cboxRequestedSigma.Size = new System.Drawing.Size(270, 23);
            this.cboxRequestedSigma.TabIndex = 17;
            this.cboxRequestedSigma.ValueMember = "Id";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 179);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 15);
            this.label5.TabIndex = 18;
            this.label5.Text = "Customer";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(27, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(129, 15);
            this.label6.TabIndex = 20;
            this.label6.Text = "Create new order...";
            // 
            // panelCustomer
            // 
            this.panelCustomer.Controls.Add(this.tbCustomer);
            this.panelCustomer.Controls.Add(this.btnSelectCustomer);
            this.panelCustomer.Location = new System.Drawing.Point(138, 175);
            this.panelCustomer.Name = "panelCustomer";
            this.panelCustomer.Size = new System.Drawing.Size(270, 24);
            this.panelCustomer.TabIndex = 21;
            // 
            // tbCustomer
            // 
            this.tbCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbCustomer.Location = new System.Drawing.Point(0, 0);
            this.tbCustomer.Name = "tbCustomer";
            this.tbCustomer.ReadOnly = true;
            this.tbCustomer.Size = new System.Drawing.Size(246, 21);
            this.tbCustomer.TabIndex = 1;
            // 
            // btnSelectCustomer
            // 
            this.btnSelectCustomer.BackgroundImage = global::DSA_lims.Properties.Resources.user_16;
            this.btnSelectCustomer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSelectCustomer.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSelectCustomer.Location = new System.Drawing.Point(246, 0);
            this.btnSelectCustomer.Name = "btnSelectCustomer";
            this.btnSelectCustomer.Size = new System.Drawing.Size(24, 24);
            this.btnSelectCustomer.TabIndex = 0;
            this.btnSelectCustomer.TabStop = false;
            this.btnSelectCustomer.Click += new System.EventHandler(this.btnSelectCustomer_Click);
            // 
            // FormOrderNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 262);
            this.Controls.Add(this.panelCustomer);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cboxRequestedSigma);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panelDeadline);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboxResponsible);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboxLaboratory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOrderNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Create order";
            this.Load += new System.EventHandler(this.FormOrderNew_Load);
            this.panel1.ResumeLayout(false);
            this.panelDeadline.ResumeLayout(false);
            this.panelDeadline.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectDeadline)).EndInit();
            this.panelCustomer.ResumeLayout(false);
            this.panelCustomer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectCustomer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxLaboratory;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboxResponsible;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelDeadline;
        private System.Windows.Forms.TextBox tbDeadline;
        private System.Windows.Forms.PictureBox btnSelectDeadline;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboxRequestedSigma;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panelCustomer;
        private System.Windows.Forms.PictureBox btnSelectCustomer;
        private System.Windows.Forms.TextBox tbCustomer;
    }
}