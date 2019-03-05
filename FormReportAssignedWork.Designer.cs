namespace DSA_lims
{
    partial class FormReportAssignedWork
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
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormReportAssignedWork));
            this.assignmentBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.DSAssignedWork = new DSA_lims.DSAssignedWork();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.reportViewerAssignedWork = new Microsoft.Reporting.WinForms.ReportViewer();
            this.assignmentTableAdapter = new DSA_lims.DSAssignedWorkTableAdapters.assignmentTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.assignmentBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DSAssignedWork)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // assignmentBindingSource
            // 
            this.assignmentBindingSource.DataMember = "assignment";
            this.assignmentBindingSource.DataSource = this.DSAssignedWork;
            // 
            // DSAssignedWork
            // 
            this.DSAssignedWork.DataSetName = "DSAssignedWork";
            this.DSAssignedWork.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 537);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(836, 28);
            this.panel1.TabIndex = 9;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Location = new System.Drawing.Point(716, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // reportViewerAssignedWork
            // 
            this.reportViewerAssignedWork.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "DataSet1";
            reportDataSource1.Value = this.assignmentBindingSource;
            this.reportViewerAssignedWork.LocalReport.DataSources.Add(reportDataSource1);
            this.reportViewerAssignedWork.LocalReport.ReportEmbeddedResource = "DSA_lims.ReportAssignedWork.rdlc";
            this.reportViewerAssignedWork.Location = new System.Drawing.Point(0, 0);
            this.reportViewerAssignedWork.Name = "reportViewerAssignedWork";
            this.reportViewerAssignedWork.ServerReport.BearerToken = null;
            this.reportViewerAssignedWork.Size = new System.Drawing.Size(836, 537);
            this.reportViewerAssignedWork.TabIndex = 10;
            // 
            // assignmentTableAdapter
            // 
            this.assignmentTableAdapter.ClearBeforeFill = true;
            // 
            // FormReportAssignedWork
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 565);
            this.Controls.Add(this.reportViewerAssignedWork);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormReportAssignedWork";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSALims - Assigned work";
            this.Load += new System.EventHandler(this.FormReportAssignedWork_Load);
            ((System.ComponentModel.ISupportInitialize)(this.assignmentBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DSAssignedWork)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewerAssignedWork;
        private System.Windows.Forms.BindingSource assignmentBindingSource;
        private DSAssignedWork DSAssignedWork;
        private DSAssignedWorkTableAdapters.assignmentTableAdapter assignmentTableAdapter;
    }
}