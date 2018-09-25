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
            this.panel19 = new System.Windows.Forms.Panel();
            this.tbPassword2 = new System.Windows.Forms.TextBox();
            this.label44 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.cboxLab = new System.Windows.Forms.ComboBox();
            this.label49 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.tbFullname = new System.Windows.Forms.TextBox();
            this.tbEmail = new System.Windows.Forms.TextBox();
            this.tbPhone = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbPassword1 = new System.Windows.Forms.TextBox();
            this.cbInUse = new System.Windows.Forms.CheckBox();
            this.lbRoles = new System.Windows.Forms.ListBox();
            this.toolStrip25 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.btnAddRole = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveRole = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel8.SuspendLayout();
            this.panel19.SuspendLayout();
            this.toolStrip25.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 525);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(480, 32);
            this.panel1.TabIndex = 1;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(280, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(380, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 32);
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
            this.splitContainer1.Size = new System.Drawing.Size(480, 525);
            this.splitContainer1.SplitterDistance = 287;
            this.splitContainer1.TabIndex = 3;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel8.Controls.Add(this.panel19, 1, 7);
            this.tableLayoutPanel8.Controls.Add(this.label44, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.label45, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.label46, 0, 3);
            this.tableLayoutPanel8.Controls.Add(this.label47, 0, 4);
            this.tableLayoutPanel8.Controls.Add(this.label48, 0, 5);
            this.tableLayoutPanel8.Controls.Add(this.cboxLab, 1, 5);
            this.tableLayoutPanel8.Controls.Add(this.label49, 0, 7);
            this.tableLayoutPanel8.Controls.Add(this.tbUsername, 1, 1);
            this.tableLayoutPanel8.Controls.Add(this.tbFullname, 1, 2);
            this.tableLayoutPanel8.Controls.Add(this.tbEmail, 1, 3);
            this.tableLayoutPanel8.Controls.Add(this.tbPhone, 1, 4);
            this.tableLayoutPanel8.Controls.Add(this.label1, 0, 6);
            this.tableLayoutPanel8.Controls.Add(this.tbPassword1, 1, 6);
            this.tableLayoutPanel8.Controls.Add(this.cbInUse, 1, 8);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 10;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(480, 287);
            this.tableLayoutPanel8.TabIndex = 3;
            // 
            // panel19
            // 
            this.panel19.Controls.Add(this.tbPassword2);
            this.panel19.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel19.Location = new System.Drawing.Point(99, 215);
            this.panel19.Name = "panel19";
            this.panel19.Size = new System.Drawing.Size(378, 26);
            this.panel19.TabIndex = 9;
            // 
            // tbPassword2
            // 
            this.tbPassword2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPassword2.Location = new System.Drawing.Point(0, 0);
            this.tbPassword2.Name = "tbPassword2";
            this.tbPassword2.PasswordChar = '*';
            this.tbPassword2.Size = new System.Drawing.Size(378, 20);
            this.tbPassword2.TabIndex = 8;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(3, 52);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(52, 13);
            this.label44.TabIndex = 0;
            this.label44.Text = "Full name";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(3, 20);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(55, 13);
            this.label45.TabIndex = 1;
            this.label45.Text = "Username";
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(3, 84);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(32, 13);
            this.label46.TabIndex = 2;
            this.label46.Text = "Email";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(3, 116);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(38, 13);
            this.label47.TabIndex = 3;
            this.label47.Text = "Phone";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(3, 148);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(57, 13);
            this.label48.TabIndex = 4;
            this.label48.Text = "Laboratory";
            // 
            // cboxLab
            // 
            this.cboxLab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxLab.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLab.FormattingEnabled = true;
            this.cboxLab.Location = new System.Drawing.Point(99, 151);
            this.cboxLab.Name = "cboxLab";
            this.cboxLab.Size = new System.Drawing.Size(378, 21);
            this.cboxLab.TabIndex = 5;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(3, 212);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(82, 13);
            this.label49.TabIndex = 7;
            this.label49.Text = "Password again";
            // 
            // tbUsername
            // 
            this.tbUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbUsername.Location = new System.Drawing.Point(99, 23);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(378, 20);
            this.tbUsername.TabIndex = 10;
            // 
            // tbFullname
            // 
            this.tbFullname.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFullname.Location = new System.Drawing.Point(99, 55);
            this.tbFullname.Name = "tbFullname";
            this.tbFullname.Size = new System.Drawing.Size(378, 20);
            this.tbFullname.TabIndex = 11;
            // 
            // tbEmail
            // 
            this.tbEmail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbEmail.Location = new System.Drawing.Point(99, 87);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(378, 20);
            this.tbEmail.TabIndex = 12;
            // 
            // tbPhone
            // 
            this.tbPhone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPhone.Location = new System.Drawing.Point(99, 119);
            this.tbPhone.Name = "tbPhone";
            this.tbPhone.Size = new System.Drawing.Size(378, 20);
            this.tbPhone.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Password";
            // 
            // tbPassword1
            // 
            this.tbPassword1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbPassword1.Location = new System.Drawing.Point(99, 183);
            this.tbPassword1.Name = "tbPassword1";
            this.tbPassword1.Size = new System.Drawing.Size(378, 20);
            this.tbPassword1.TabIndex = 15;
            // 
            // cbInUse
            // 
            this.cbInUse.AutoSize = true;
            this.cbInUse.Location = new System.Drawing.Point(99, 247);
            this.cbInUse.Name = "cbInUse";
            this.cbInUse.Size = new System.Drawing.Size(57, 17);
            this.cbInUse.TabIndex = 6;
            this.cbInUse.Text = "In Use";
            this.cbInUse.UseVisualStyleBackColor = true;
            // 
            // lbRoles
            // 
            this.lbRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRoles.FormattingEnabled = true;
            this.lbRoles.Location = new System.Drawing.Point(0, 25);
            this.lbRoles.Name = "lbRoles";
            this.lbRoles.Size = new System.Drawing.Size(480, 209);
            this.lbRoles.TabIndex = 3;
            // 
            // toolStrip25
            // 
            this.toolStrip25.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip25.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel5,
            this.btnAddRole,
            this.btnRemoveRole});
            this.toolStrip25.Location = new System.Drawing.Point(0, 0);
            this.toolStrip25.Name = "toolStrip25";
            this.toolStrip25.Size = new System.Drawing.Size(480, 25);
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
            // btnAddRole
            // 
            this.btnAddRole.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAddRole.Image = ((System.Drawing.Image)(resources.GetObject("btnAddRole.Image")));
            this.btnAddRole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAddRole.Name = "btnAddRole";
            this.btnAddRole.Size = new System.Drawing.Size(23, 22);
            this.btnAddRole.Text = "toolStripButton1";
            // 
            // btnRemoveRole
            // 
            this.btnRemoveRole.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemoveRole.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveRole.Image")));
            this.btnRemoveRole.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveRole.Name = "btnRemoveRole";
            this.btnRemoveRole.Size = new System.Drawing.Size(23, 22);
            this.btnRemoveRole.Text = "toolStripButton2";
            // 
            // FormUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 557);
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
            this.panel19.ResumeLayout(false);
            this.panel19.PerformLayout();
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
        private System.Windows.Forms.Panel panel19;
        private System.Windows.Forms.TextBox tbPassword2;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.ComboBox cboxLab;
        private System.Windows.Forms.CheckBox cbInUse;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.TextBox tbFullname;
        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.TextBox tbPhone;
        private System.Windows.Forms.ToolStrip toolStrip25;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ListBox lbRoles;
        private System.Windows.Forms.ToolStripButton btnAddRole;
        private System.Windows.Forms.ToolStripButton btnRemoveRole;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbPassword1;
    }
}