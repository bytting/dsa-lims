namespace DSA_lims
{
    partial class FormUser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUser));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.label44 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbFullname = new System.Windows.Forms.TextBox();
            this.tbEmail = new System.Windows.Forms.TextBox();
            this.tbPhone = new System.Windows.Forms.TextBox();
            this.label48 = new System.Windows.Forms.Label();
            this.cboxLaboratory = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cboxLanguage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboxInstanceStatus = new System.Windows.Forms.ComboBox();
            this.lbRoles = new System.Windows.Forms.ListBox();
            this.toolStrip25 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddRole = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveRole = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.toolStrip25.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 462);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(403, 28);
            this.panel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(303, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel8);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbRoles);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip25);
            this.splitContainer1.Size = new System.Drawing.Size(403, 462);
            this.splitContainer1.SplitterDistance = 217;
            this.splitContainer1.TabIndex = 3;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel8.Controls.Add(this.label44, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.label45, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.label46, 0, 3);
            this.tableLayoutPanel8.Controls.Add(this.label47, 0, 4);
            this.tableLayoutPanel8.Controls.Add(this.tbUsername, 1, 1);
            this.tableLayoutPanel8.Controls.Add(this.tbFullname, 1, 2);
            this.tableLayoutPanel8.Controls.Add(this.tbEmail, 1, 3);
            this.tableLayoutPanel8.Controls.Add(this.tbPhone, 1, 4);
            this.tableLayoutPanel8.Controls.Add(this.label48, 0, 6);
            this.tableLayoutPanel8.Controls.Add(this.cboxLaboratory, 1, 6);
            this.tableLayoutPanel8.Controls.Add(this.label1, 0, 5);
            this.tableLayoutPanel8.Controls.Add(this.cboxLanguage, 1, 5);
            this.tableLayoutPanel8.Controls.Add(this.label2, 0, 7);
            this.tableLayoutPanel8.Controls.Add(this.cboxInstanceStatus, 1, 7);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 9;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(403, 217);
            this.tableLayoutPanel8.TabIndex = 3;
            // 
            // label44
            // 
            this.label44.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label44.Location = new System.Drawing.Point(3, 42);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(74, 26);
            this.label44.TabIndex = 0;
            this.label44.Text = "Full name";
            this.label44.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label45
            // 
            this.label45.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label45.Location = new System.Drawing.Point(3, 16);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(74, 26);
            this.label45.TabIndex = 1;
            this.label45.Text = "Username";
            this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label46
            // 
            this.label46.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label46.Location = new System.Drawing.Point(3, 68);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(74, 26);
            this.label46.TabIndex = 2;
            this.label46.Text = "Email";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label47
            // 
            this.label47.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label47.Location = new System.Drawing.Point(3, 94);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(74, 26);
            this.label47.TabIndex = 3;
            this.label47.Text = "Phone";
            this.label47.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbUsername
            // 
            this.tbUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbUsername.Location = new System.Drawing.Point(83, 19);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.ReadOnly = true;
            this.tbUsername.Size = new System.Drawing.Size(317, 20);
            this.tbUsername.TabIndex = 10;
            // 
            // tbFullname
            // 
            this.tbFullname.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFullname.Location = new System.Drawing.Point(83, 45);
            this.tbFullname.Name = "tbFullname";
            this.tbFullname.Size = new System.Drawing.Size(317, 20);
            this.tbFullname.TabIndex = 11;
            // 
            // tbEmail
            // 
            this.tbEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbEmail.Location = new System.Drawing.Point(83, 71);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(317, 20);
            this.tbEmail.TabIndex = 12;
            // 
            // tbPhone
            // 
            this.tbPhone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPhone.Location = new System.Drawing.Point(83, 97);
            this.tbPhone.Name = "tbPhone";
            this.tbPhone.Size = new System.Drawing.Size(317, 20);
            this.tbPhone.TabIndex = 13;
            // 
            // label48
            // 
            this.label48.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label48.Location = new System.Drawing.Point(3, 146);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(74, 26);
            this.label48.TabIndex = 4;
            this.label48.Text = "Laboratory";
            this.label48.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboxLaboratory
            // 
            this.cboxLaboratory.DisplayMember = "Name";
            this.cboxLaboratory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxLaboratory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLaboratory.FormattingEnabled = true;
            this.cboxLaboratory.Location = new System.Drawing.Point(83, 149);
            this.cboxLaboratory.Name = "cboxLaboratory";
            this.cboxLaboratory.Size = new System.Drawing.Size(317, 21);
            this.cboxLaboratory.TabIndex = 5;
            this.cboxLaboratory.ValueMember = "Id";
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 26);
            this.label1.TabIndex = 18;
            this.label1.Text = "Language";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboxLanguage
            // 
            this.cboxLanguage.DisplayMember = "Name";
            this.cboxLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLanguage.FormattingEnabled = true;
            this.cboxLanguage.Items.AddRange(new object[] {
            "nb",
            "en"});
            this.cboxLanguage.Location = new System.Drawing.Point(83, 123);
            this.cboxLanguage.Name = "cboxLanguage";
            this.cboxLanguage.Size = new System.Drawing.Size(317, 21);
            this.cboxLanguage.TabIndex = 19;
            this.cboxLanguage.ValueMember = "Id";
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 172);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 26);
            this.label2.TabIndex = 17;
            this.label2.Text = "Status";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboxInstanceStatus
            // 
            this.cboxInstanceStatus.DisplayMember = "Name";
            this.cboxInstanceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxInstanceStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxInstanceStatus.FormattingEnabled = true;
            this.cboxInstanceStatus.Location = new System.Drawing.Point(83, 175);
            this.cboxInstanceStatus.Name = "cboxInstanceStatus";
            this.cboxInstanceStatus.Size = new System.Drawing.Size(317, 21);
            this.cboxInstanceStatus.TabIndex = 16;
            this.cboxInstanceStatus.ValueMember = "Id";
            // 
            // lbRoles
            // 
            this.lbRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRoles.FormattingEnabled = true;
            this.lbRoles.Location = new System.Drawing.Point(0, 25);
            this.lbRoles.Name = "lbRoles";
            this.lbRoles.Size = new System.Drawing.Size(403, 216);
            this.lbRoles.TabIndex = 3;
            // 
            // toolStrip25
            // 
            this.toolStrip25.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip25.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel5,
            this.toolStripSeparator1,
            this.btnAddRole,
            this.btnRemoveRole});
            this.toolStrip25.Location = new System.Drawing.Point(0, 0);
            this.toolStrip25.Name = "toolStrip25";
            this.toolStrip25.Size = new System.Drawing.Size(403, 25);
            this.toolStrip25.TabIndex = 2;
            this.toolStrip25.Text = "toolStrip25";
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(37, 22);
            this.toolStripLabel5.Text = "Roles";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAddRole
            // 
            this.btnAddRole.Image = global::DSA_lims.Properties.Resources._new;
            this.btnAddRole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddRole.Name = "btnAddRole";
            this.btnAddRole.Size = new System.Drawing.Size(49, 22);
            this.btnAddRole.Text = "Add";
            // 
            // btnRemoveRole
            // 
            this.btnRemoveRole.Image = global::DSA_lims.Properties.Resources.delete;
            this.btnRemoveRole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveRole.Name = "btnRemoveRole";
            this.btnRemoveRole.Size = new System.Drawing.Size(70, 22);
            this.btnRemoveRole.Text = "Remove";
            // 
            // FormUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 490);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormUser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - User";
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.toolStrip25.ResumeLayout(false);
            this.toolStrip25.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox cboxLaboratory;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbFullname;
        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.TextBox tbPhone;
        private System.Windows.Forms.ToolStrip toolStrip25;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ListBox lbRoles;
        private System.Windows.Forms.ToolStripButton btnAddRole;
        private System.Windows.Forms.ToolStripButton btnRemoveRole;
        private System.Windows.Forms.ComboBox cboxInstanceStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxLanguage;
    }
}