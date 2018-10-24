using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSelectExistingPreps : Form
    {
        private Guid LaboratoryId = Guid.Empty;
        private Guid SampleId = Guid.Empty;

        public FormSelectExistingPreps(Guid labId, Guid sampId)
        {
            InitializeComponent();

            LaboratoryId = labId;
            SampleId = sampId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                string query = "select * from preparation where sample_id = @sample_id and laboratory_id = @laboratory_id order by create_date asc";
                gridPreparations.DataSource = DB.GetDataTable(conn, query, CommandType.Text,
                    new SqlParameter("@sample_id", SampleId),
                    new SqlParameter("@laboratory_id", LaboratoryId));
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
