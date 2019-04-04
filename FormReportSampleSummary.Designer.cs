namespace DSA_lims
{
    partial class FormReportSampleSummary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormReportSampleSummary));
            this.DataTable1BindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.DSSampleSummary = new DSA_lims.DSSampleSummary();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.reportViewerSampleSummary = new Microsoft.Reporting.WinForms.ReportViewer();
            this.DataTable1TableAdapter = new DSA_lims.DSSampleSummaryTableAdapters.DataTable1TableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.DataTable1BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DSSampleSummary)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataTable1BindingSource
            // 
            this.DataTable1BindingSource.DataMember = "DataTable1";
            this.DataTable1BindingSource.DataSource = this.DSSampleSummary;
            // 
            // DSSampleSummary
            // 
            this.DSSampleSummary.DataSetName = "DSSampleSummary";
            this.DSSampleSummary.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 530);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(689, 28);
            this.panel1.TabIndex = 11;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.Location = new System.Drawing.Point(569, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // reportViewerSampleSummary
            // 
            this.reportViewerSampleSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            reportDataSource1.Name = "DataSet1";
            reportDataSource1.Value = this.DataTable1BindingSource;
            this.reportViewerSampleSummary.LocalReport.DataSources.Add(reportDataSource1);
            this.reportViewerSampleSummary.LocalReport.ReportEmbeddedResource = "DSA_lims.ReportSampleSummary.rdlc";
            this.reportViewerSampleSummary.Location = new System.Drawing.Point(0, 0);
            this.reportViewerSampleSummary.Name = "reportViewerSampleSummary";
            this.reportViewerSampleSummary.ServerReport.BearerToken = null;
            this.reportViewerSampleSummary.Size = new System.Drawing.Size(689, 530);
            this.reportViewerSampleSummary.TabIndex = 12;
            // 
            // DataTable1TableAdapter
            // 
            this.DataTable1TableAdapter.ClearBeforeFill = true;
            // 
            // FormReportSampleSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 558);
            this.Controls.Add(this.reportViewerSampleSummary);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormReportSampleSummary";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DSALims - Sample summary";
            this.Load += new System.EventHandler(this.FormReportSampleSummary_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DataTable1BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DSSampleSummary)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private Microsoft.Reporting.WinForms.ReportViewer reportViewerSampleSummary;
        private System.Windows.Forms.BindingSource DataTable1BindingSource;
        private DSSampleSummary DSSampleSummary;
        private DSSampleSummaryTableAdapters.DataTable1TableAdapter DataTable1TableAdapter;
    }
}