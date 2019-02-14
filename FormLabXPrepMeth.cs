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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLabXPrepMeth : Form
    {
        Guid mLabId = Guid.Empty;

        public FormLabXPrepMeth(Guid labId)
        {
            InitializeComponent();

            mLabId = labId;
        }

        private void FormLabXPrepMeth_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulatePreparationMethods(conn, gridPrepMeth);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                
                SqlCommand cmd = new SqlCommand("delete from laboratory_x_preparation_method where laboratory_id = @lab_id", conn, trans);
                cmd.Parameters.AddWithValue("@lab_id", mLabId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into laboratory_x_preparation_method values(@laboratory_id, @preparation_method_id)";

                foreach (DataGridViewRow row in gridPrepMeth.SelectedRows)
                {                    
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@laboratory_id", mLabId);
                    cmd.Parameters.AddWithValue("@preparation_method_id", Utils.MakeGuid(row.Cells["id"].Value));
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();                
            }
            catch(Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
