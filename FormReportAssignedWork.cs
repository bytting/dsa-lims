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
    public partial class FormReportAssignedWork : Form
    {
        Guid mLabId = Guid.Empty;

        public FormReportAssignedWork(Guid labId)
        {
            InitializeComponent();

            mLabId = labId;
        }

        private void FormReportAssignedWork_Load(object sender, EventArgs e)
        {
            assignmentTableAdapter.Connection.ConnectionString = DB.ConnectionString;
            assignmentTableAdapter.Fill(this.DSAssignedWork.assignment, mLabId);
            
            reportViewerAssignedWork.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.FullPage;
            reportViewerAssignedWork.RefreshReport();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
