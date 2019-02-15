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

        public List<Guid> SelectedPreparationIds = new List<Guid>();

        public FormSelectExistingPreps(Guid labId, Guid sampId)
        {
            InitializeComponent();

            LaboratoryId = labId;
            SampleId = sampId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                string query = @"
select 
    p.id,
    p.number as 'Name',
    pm.name as 'Prep.meth',
    pg.name as 'Geom.'
from preparation p
    inner join preparation_method pm on pm.id = p.preparation_method_id
    left outer join preparation_geometry pg on pg.id = p.preparation_geometry_id
where p.sample_id = @sample_id and p.laboratory_id = @laboratory_id 
order by p.number";

                gridPreparations.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text,
                    new SqlParameter("@sample_id", SampleId),
                    new SqlParameter("@laboratory_id", LaboratoryId));

                gridPreparations.Columns["id"].Visible = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedPreparationIds.Clear();

            foreach (DataGridViewRow row in gridPreparations.SelectedRows)
            {
                SelectedPreparationIds.Add(Guid.Parse(row.Cells["id"].Value.ToString()));
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
