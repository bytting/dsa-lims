using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormReportSampleSummary : Form
    {
        Guid mAssignmentId = Guid.Empty;

        public FormReportSampleSummary(Guid assignmentId)
        {
            InitializeComponent();
            mAssignmentId = assignmentId;
        }

        private void FormReportSampleSummary_Load(object sender, EventArgs e)
        {            
            DataTable1TableAdapter.Fill(this.DSSampleSummary.DataTable1, mAssignmentId);

            reportViewerSampleSummary.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            reportViewerSampleSummary.RefreshReport();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
