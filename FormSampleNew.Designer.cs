namespace DSA_lims
{
    partial class FormSampleNew
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSampleNew));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCreate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboxSampleType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cboxLaboratory = new System.Windows.Forms.ComboBox();
            this.cboxProjectMain = new System.Windows.Forms.ComboBox();
            this.cboxProjectSub = new System.Windows.Forms.ComboBox();
            this.btnSelectSampleType = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cboxSampleComponent = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnCreate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 155);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(637, 28);
            this.panel1.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(403, 0);
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
            this.btnCreate.Location = new System.Drawing.Point(520, 0);
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
            this.label1.Location = new System.Drawing.Point(22, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Sample type";
            // 
            // cboxSampleType
            // 
            this.cboxSampleType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboxSampleType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboxSampleType.DisplayMember = "Name";
            this.cboxSampleType.FormattingEnabled = true;
            this.cboxSampleType.Location = new System.Drawing.Point(128, 24);
            this.cboxSampleType.Name = "cboxSampleType";
            this.cboxSampleType.Size = new System.Drawing.Size(448, 21);
            this.cboxSampleType.TabIndex = 11;
            this.cboxSampleType.ValueMember = "Id";
            this.cboxSampleType.SelectedIndexChanged += new System.EventHandler(this.cboxSampleType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Project/Sub-Project";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Laboratory";
            // 
            // cboxLaboratory
            // 
            this.cboxLaboratory.DisplayMember = "Name";
            this.cboxLaboratory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLaboratory.FormattingEnabled = true;
            this.cboxLaboratory.Location = new System.Drawing.Point(128, 108);
            this.cboxLaboratory.Name = "cboxLaboratory";
            this.cboxLaboratory.Size = new System.Drawing.Size(234, 21);
            this.cboxLaboratory.TabIndex = 14;
            this.cboxLaboratory.ValueMember = "Id";
            // 
            // cboxProjectMain
            // 
            this.cboxProjectMain.DisplayMember = "Name";
            this.cboxProjectMain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxProjectMain.FormattingEnabled = true;
            this.cboxProjectMain.Location = new System.Drawing.Point(128, 79);
            this.cboxProjectMain.Name = "cboxProjectMain";
            this.cboxProjectMain.Size = new System.Drawing.Size(234, 21);
            this.cboxProjectMain.TabIndex = 15;
            this.cboxProjectMain.ValueMember = "Id";
            this.cboxProjectMain.SelectedIndexChanged += new System.EventHandler(this.cboxProjectMain_SelectedIndexChanged);
            // 
            // cboxProjectSub
            // 
            this.cboxProjectSub.DisplayMember = "Name";
            this.cboxProjectSub.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxProjectSub.FormattingEnabled = true;
            this.cboxProjectSub.Location = new System.Drawing.Point(368, 79);
            this.cboxProjectSub.Name = "cboxProjectSub";
            this.cboxProjectSub.Size = new System.Drawing.Size(244, 21);
            this.cboxProjectSub.TabIndex = 16;
            this.cboxProjectSub.ValueMember = "Id";
            // 
            // btnSelectSampleType
            // 
            this.btnSelectSampleType.Location = new System.Drawing.Point(577, 23);
            this.btnSelectSampleType.Name = "btnSelectSampleType";
            this.btnSelectSampleType.Size = new System.Drawing.Size(35, 23);
            this.btnSelectSampleType.TabIndex = 17;
            this.btnSelectSampleType.Text = "...";
            this.btnSelectSampleType.UseVisualStyleBackColor = true;
            this.btnSelectSampleType.Click += new System.EventHandler(this.btnSelectSampleType_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Sample component";
            // 
            // cboxSampleComponent
            // 
            this.cboxSampleComponent.DisplayMember = "Name";
            this.cboxSampleComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSampleComponent.FormattingEnabled = true;
            this.cboxSampleComponent.Location = new System.Drawing.Point(128, 50);
            this.cboxSampleComponent.Name = "cboxSampleComponent";
            this.cboxSampleComponent.Size = new System.Drawing.Size(234, 21);
            this.cboxSampleComponent.TabIndex = 19;
            this.cboxSampleComponent.ValueMember = "Id";
            // 
            // FormSampleNew
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 183);
            this.Controls.Add(this.cboxSampleComponent);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnSelectSampleType);
            this.Controls.Add(this.cboxProjectSub);
            this.Controls.Add(this.cboxProjectMain);
            this.Controls.Add(this.cboxLaboratory);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cboxSampleType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSampleNew";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Create sample";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboxSampleType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboxLaboratory;
        private System.Windows.Forms.ComboBox cboxProjectMain;
        private System.Windows.Forms.ComboBox cboxProjectSub;
        private System.Windows.Forms.Button btnSelectSampleType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboxSampleComponent;
    }
}