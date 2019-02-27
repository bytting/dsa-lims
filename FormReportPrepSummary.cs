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
    public partial class FormReportPrepSummary : Form
    {
        Guid mAssignmentId = Guid.Empty;

        public FormReportPrepSummary(Guid assignmentId)
        {
            InitializeComponent();

            mAssignmentId = assignmentId;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FormReportPrepSummary_Load(object sender, EventArgs e)
        {
            DataTable1TableAdapter.Connection.ConnectionString = DB.ConnectionString;
            DataTable1TableAdapter.Fill(this.DSPrepSummary.DataTable1, mAssignmentId);

            reportViewerPrepSummary.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.FullPage;
            reportViewerPrepSummary.RefreshReport();
        }
    }
}
