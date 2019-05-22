/*	
	DSA Lims - Laboratory Information Management System
    Copyright (C) 2018  Norwegian Radiation Protection Authority

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
// Authors: Dag Robole,

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSelectExistingPreps : Form
    {
        private Guid LaboratoryId = Guid.Empty;
        private Sample mSample = null;

        public List<Guid> SelectedPreparationIds = new List<Guid>();

        public FormSelectExistingPreps(Guid labId, Sample sample)
        {
            InitializeComponent();

            LaboratoryId = labId;
            mSample = sample;
        }

        private void FormSelectExistingPreps_Load(object sender, EventArgs e)
        {
            lblSampleInfo.Text = "Sample: " + mSample.Number + ", External Id: " + mSample.ExternalId;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                string query = @"
select 
    p.id,
    p.number as 'Name',
    pm.name as 'Prep.meth',
    pg.name as 'Geom.',
    l.name as 'Laboratory'
from preparation p
    inner join preparation_method pm on pm.id = p.preparation_method_id
    inner join laboratory l on l.id = p.laboratory_id
    left outer join preparation_geometry pg on pg.id = p.preparation_geometry_id
where p.sample_id = @sample_id and p.laboratory_id = @laboratory_id 
order by p.number";

                gridPreparations.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text,
                    new SqlParameter("@sample_id", mSample.Id),
                    new SqlParameter("@laboratory_id", LaboratoryId));

                gridPreparations.Columns["id"].Visible = false;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
                Close();
            }
            finally
            {
                conn?.Close();
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
                SelectedPreparationIds.Add(Utils.MakeGuid(row.Cells["id"].Value));
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
