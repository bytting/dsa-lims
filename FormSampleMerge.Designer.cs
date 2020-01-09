namespace DSA_lims
{
    partial class FormSampleMerge
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSampleMerge));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.gridSamples = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.cboxSampleType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cboxSampleComponent = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbComment = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dtSamplingTime = new System.Windows.Forms.DateTimePicker();
            this.dtSamplingDate = new System.Windows.Forms.DateTimePicker();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dtReferenceTime = new System.Windows.Forms.DateTimePicker();
            this.dtReferenceDate = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            this.tbExternalId = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 405);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(557, 28);
            this.panel1.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(357, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(457, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 28);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // gridSamples
            // 
            this.gridSamples.AllowUserToAddRows = false;
            this.gridSamples.AllowUserToDeleteRows = false;
            this.gridSamples.AllowUserToResizeRows = false;
            this.gridSamples.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridSamples.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.gridSamples.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridSamples.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.Location = new System.Drawing.Point(0, 0);
            this.gridSamples.MultiSelect = false;
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ReadOnly = true;
            this.gridSamples.RowHeadersVisible = false;
            this.gridSamples.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridSamples.Size = new System.Drawing.Size(557, 231);
            this.gridSamples.TabIndex = 10;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cboxSampleType, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.cboxSampleComponent, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.tbComment, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbExternalId, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 231);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(557, 174);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 28);
            this.label2.TabIndex = 2;
            this.label2.Text = "Sample type";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboxSampleType
            // 
            this.cboxSampleType.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboxSampleType.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboxSampleType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxSampleType.FormattingEnabled = true;
            this.cboxSampleType.Location = new System.Drawing.Point(170, 31);
            this.cboxSampleType.Name = "cboxSampleType";
            this.cboxSampleType.Size = new System.Drawing.Size(384, 21);
            this.cboxSampleType.TabIndex = 3;
            this.cboxSampleType.SelectedIndexChanged += new System.EventHandler(this.cboxSampleType_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(161, 28);
            this.label3.TabIndex = 4;
            this.label3.Text = "Sample component";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboxSampleComponent
            // 
            this.cboxSampleComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxSampleComponent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSampleComponent.FormattingEnabled = true;
            this.cboxSampleComponent.Location = new System.Drawing.Point(170, 59);
            this.cboxSampleComponent.Name = "cboxSampleComponent";
            this.cboxSampleComponent.Size = new System.Drawing.Size(384, 21);
            this.cboxSampleComponent.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 28);
            this.label1.TabIndex = 6;
            this.label1.Text = "Reference date";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 28);
            this.label4.TabIndex = 7;
            this.label4.Text = "Sampling date";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 140);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 28);
            this.label5.TabIndex = 8;
            this.label5.Text = "Sample comment";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbComment
            // 
            this.tbComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbComment.Location = new System.Drawing.Point(170, 143);
            this.tbComment.MaxLength = 1000;
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(384, 20);
            this.tbComment.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dtSamplingTime);
            this.panel2.Controls.Add(this.dtSamplingDate);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(170, 87);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(384, 22);
            this.panel2.TabIndex = 10;
            // 
            // dtSamplingTime
            // 
            this.dtSamplingTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtSamplingTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtSamplingTime.Location = new System.Drawing.Point(183, 0);
            this.dtSamplingTime.Name = "dtSamplingTime";
            this.dtSamplingTime.Size = new System.Drawing.Size(201, 20);
            this.dtSamplingTime.TabIndex = 1;
            // 
            // dtSamplingDate
            // 
            this.dtSamplingDate.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtSamplingDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtSamplingDate.Location = new System.Drawing.Point(0, 0);
            this.dtSamplingDate.Name = "dtSamplingDate";
            this.dtSamplingDate.Size = new System.Drawing.Size(183, 20);
            this.dtSamplingDate.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dtReferenceTime);
            this.panel3.Controls.Add(this.dtReferenceDate);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(170, 115);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(384, 22);
            this.panel3.TabIndex = 11;
            // 
            // dtReferenceTime
            // 
            this.dtReferenceTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtReferenceTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dtReferenceTime.Location = new System.Drawing.Point(183, 0);
            this.dtReferenceTime.Name = "dtReferenceTime";
            this.dtReferenceTime.Size = new System.Drawing.Size(201, 20);
            this.dtReferenceTime.TabIndex = 1;
            // 
            // dtReferenceDate
            // 
            this.dtReferenceDate.Dock = System.Windows.Forms.DockStyle.Left;
            this.dtReferenceDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtReferenceDate.Location = new System.Drawing.Point(0, 0);
            this.dtReferenceDate.Name = "dtReferenceDate";
            this.dtReferenceDate.Size = new System.Drawing.Size(183, 20);
            this.dtReferenceDate.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(161, 28);
            this.label6.TabIndex = 12;
            this.label6.Text = "External Id";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExternalId
            // 
            this.tbExternalId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbExternalId.Location = new System.Drawing.Point(170, 3);
            this.tbExternalId.MaxLength = 128;
            this.tbExternalId.Name = "tbExternalId";
            this.tbExternalId.Size = new System.Drawing.Size(384, 20);
            this.tbExternalId.TabIndex = 13;
            // 
            // FormSampleMerge
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 433);
            this.Controls.Add(this.gridSamples);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSampleMerge";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Merge samples";
            this.Load += new System.EventHandler(this.FormSampleMerge_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.DataGridView gridSamples;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboxSampleType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cboxSampleComponent;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbComment;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DateTimePicker dtSamplingTime;
        private System.Windows.Forms.DateTimePicker dtSamplingDate;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DateTimePicker dtReferenceTime;
        private System.Windows.Forms.DateTimePicker dtReferenceDate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbExternalId;
    }
}