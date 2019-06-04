namespace DSA_lims
{
    partial class FormImportSamplesSampReg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImportSamplesSampReg));
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.gridSamples = new System.Windows.Forms.DataGridView();
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.cboxLaboratory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbProject = new System.Windows.Forms.TextBox();
            this.tbFileID = new System.Windows.Forms.TextBox();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cboxProjectSub = new System.Windows.Forms.ComboBox();
            this.cboxProjectMain = new System.Windows.Forms.ComboBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.cboxSampleTypes = new System.Windows.Forms.ComboBox();
            this.btnSetSampleType = new System.Windows.Forms.Button();
            this.ColumnNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnExternalId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSamplingDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLatitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLongitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnAltitude = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSampleType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnLIMSSampleType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).BeginInit();
            this.tableLayout.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 623);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(890, 28);
            this.panel1.TabIndex = 8;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(656, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(117, 28);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnOk.Location = new System.Drawing.Point(773, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(117, 28);
            this.btnOk.TabIndex = 6;
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
            this.gridSamples.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridSamples.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridSamples.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridSamples.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnNumber,
            this.ColumnExternalId,
            this.ColumnSamplingDate,
            this.ColumnLatitude,
            this.ColumnLongitude,
            this.ColumnAltitude,
            this.ColumnLocation,
            this.ColumnSampleType,
            this.ColumnLIMSSampleType});
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.Location = new System.Drawing.Point(0, 0);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.ReadOnly = true;
            this.gridSamples.RowHeadersVisible = false;
            this.gridSamples.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridSamples.Size = new System.Drawing.Size(890, 386);
            this.gridSamples.TabIndex = 10;
            // 
            // tableLayout
            // 
            this.tableLayout.ColumnCount = 3;
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayout.Controls.Add(this.label3, 0, 5);
            this.tableLayout.Controls.Add(this.cboxLaboratory, 1, 5);
            this.tableLayout.Controls.Add(this.label2, 0, 4);
            this.tableLayout.Controls.Add(this.label1, 0, 3);
            this.tableLayout.Controls.Add(this.label4, 0, 2);
            this.tableLayout.Controls.Add(this.tbProject, 1, 3);
            this.tableLayout.Controls.Add(this.tbFileID, 1, 2);
            this.tableLayout.Controls.Add(this.btnOpenFile, 1, 1);
            this.tableLayout.Controls.Add(this.panel2, 1, 4);
            this.tableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayout.Location = new System.Drawing.Point(0, 0);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.RowCount = 7;
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayout.Size = new System.Drawing.Size(890, 181);
            this.tableLayout.TabIndex = 0;
            this.tableLayout.Resize += new System.EventHandler(this.tableLayout_Resize);
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 136);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 32);
            this.label3.TabIndex = 2;
            this.label3.Text = "LIMS Laboratory";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboxLaboratory
            // 
            this.cboxLaboratory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxLaboratory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxLaboratory.FormattingEnabled = true;
            this.cboxLaboratory.Location = new System.Drawing.Point(163, 139);
            this.cboxLaboratory.Name = "cboxLaboratory";
            this.cboxLaboratory.Size = new System.Drawing.Size(724, 23);
            this.cboxLaboratory.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "LIMS Main/Sub project";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "App project";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(154, 32);
            this.label4.TabIndex = 5;
            this.label4.Text = "App file ID";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbProject
            // 
            this.tbProject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbProject.Location = new System.Drawing.Point(163, 75);
            this.tbProject.Name = "tbProject";
            this.tbProject.ReadOnly = true;
            this.tbProject.Size = new System.Drawing.Size(724, 21);
            this.tbProject.TabIndex = 4;
            // 
            // tbFileID
            // 
            this.tbFileID.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbFileID.Location = new System.Drawing.Point(163, 43);
            this.tbFileID.Name = "tbFileID";
            this.tbFileID.ReadOnly = true;
            this.tbFileID.Size = new System.Drawing.Size(724, 21);
            this.tbFileID.TabIndex = 6;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnOpenFile.Location = new System.Drawing.Point(163, 11);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(156, 26);
            this.btnOpenFile.TabIndex = 3;
            this.btnOpenFile.Text = "Open file";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cboxProjectSub);
            this.panel2.Controls.Add(this.cboxProjectMain);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(163, 107);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(724, 26);
            this.panel2.TabIndex = 10;
            // 
            // cboxProjectSub
            // 
            this.cboxProjectSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboxProjectSub.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxProjectSub.FormattingEnabled = true;
            this.cboxProjectSub.Location = new System.Drawing.Point(198, 0);
            this.cboxProjectSub.Name = "cboxProjectSub";
            this.cboxProjectSub.Size = new System.Drawing.Size(526, 23);
            this.cboxProjectSub.TabIndex = 8;
            // 
            // cboxProjectMain
            // 
            this.cboxProjectMain.Dock = System.Windows.Forms.DockStyle.Left;
            this.cboxProjectMain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxProjectMain.FormattingEnabled = true;
            this.cboxProjectMain.Location = new System.Drawing.Point(0, 0);
            this.cboxProjectMain.Name = "cboxProjectMain";
            this.cboxProjectMain.Size = new System.Drawing.Size(198, 23);
            this.cboxProjectMain.TabIndex = 7;
            this.cboxProjectMain.SelectedIndexChanged += new System.EventHandler(this.cboxProjectMain_SelectedIndexChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.gridSamples);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 181);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(890, 442);
            this.panel3.TabIndex = 11;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnSetSampleType);
            this.panel4.Controls.Add(this.cboxSampleTypes);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 386);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(890, 56);
            this.panel4.TabIndex = 11;
            // 
            // cboxSampleTypes
            // 
            this.cboxSampleTypes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboxSampleTypes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboxSampleTypes.FormattingEnabled = true;
            this.cboxSampleTypes.Location = new System.Drawing.Point(15, 16);
            this.cboxSampleTypes.Name = "cboxSampleTypes";
            this.cboxSampleTypes.Size = new System.Drawing.Size(280, 23);
            this.cboxSampleTypes.TabIndex = 0;
            this.cboxSampleTypes.Leave += new System.EventHandler(this.cboxSampleTypes_Leave);
            // 
            // btnSetSampleType
            // 
            this.btnSetSampleType.Location = new System.Drawing.Point(301, 16);
            this.btnSetSampleType.Name = "btnSetSampleType";
            this.btnSetSampleType.Size = new System.Drawing.Size(139, 23);
            this.btnSetSampleType.TabIndex = 1;
            this.btnSetSampleType.Text = "Set sample type";
            this.btnSetSampleType.UseVisualStyleBackColor = true;
            this.btnSetSampleType.Click += new System.EventHandler(this.btnSetSampleType_Click);
            // 
            // ColumnNumber
            // 
            this.ColumnNumber.HeaderText = "Number";
            this.ColumnNumber.Name = "ColumnNumber";
            this.ColumnNumber.ReadOnly = true;
            // 
            // ColumnExternalId
            // 
            this.ColumnExternalId.HeaderText = "External Id";
            this.ColumnExternalId.Name = "ColumnExternalId";
            this.ColumnExternalId.ReadOnly = true;
            // 
            // ColumnSamplingDate
            // 
            this.ColumnSamplingDate.HeaderText = "Sampl. Date";
            this.ColumnSamplingDate.Name = "ColumnSamplingDate";
            this.ColumnSamplingDate.ReadOnly = true;
            // 
            // ColumnLatitude
            // 
            this.ColumnLatitude.HeaderText = "Latitude";
            this.ColumnLatitude.Name = "ColumnLatitude";
            this.ColumnLatitude.ReadOnly = true;
            // 
            // ColumnLongitude
            // 
            this.ColumnLongitude.HeaderText = "Longitude";
            this.ColumnLongitude.Name = "ColumnLongitude";
            this.ColumnLongitude.ReadOnly = true;
            // 
            // ColumnAltitude
            // 
            this.ColumnAltitude.HeaderText = "Altitude";
            this.ColumnAltitude.Name = "ColumnAltitude";
            this.ColumnAltitude.ReadOnly = true;
            // 
            // ColumnLocation
            // 
            this.ColumnLocation.HeaderText = "Location";
            this.ColumnLocation.Name = "ColumnLocation";
            this.ColumnLocation.ReadOnly = true;
            // 
            // ColumnSampleType
            // 
            this.ColumnSampleType.HeaderText = "Samp. Type";
            this.ColumnSampleType.Name = "ColumnSampleType";
            this.ColumnSampleType.ReadOnly = true;
            // 
            // ColumnLIMSSampleType
            // 
            this.ColumnLIMSSampleType.HeaderText = "LIMS Samp. Type";
            this.ColumnLIMSSampleType.Name = "ColumnLIMSSampleType";
            this.ColumnLIMSSampleType.ReadOnly = true;
            // 
            // FormImportSamplesSampReg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 651);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.tableLayout);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "FormImportSamplesSampReg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSA-Lims - Import app samples";
            this.Load += new System.EventHandler(this.FormImportSamplesSampReg_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridSamples)).EndInit();
            this.tableLayout.ResumeLayout(false);
            this.tableLayout.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.DataGridView gridSamples;
        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox tbProject;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbFileID;
        private System.Windows.Forms.ComboBox cboxProjectMain;
        private System.Windows.Forms.ComboBox cboxProjectSub;
        private System.Windows.Forms.ComboBox cboxLaboratory;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnSetSampleType;
        private System.Windows.Forms.ComboBox cboxSampleTypes;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnExternalId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSamplingDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLatitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLongitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnAltitude;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLocation;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSampleType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnLIMSSampleType;
    }
}