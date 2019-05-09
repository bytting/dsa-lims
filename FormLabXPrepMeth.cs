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
        List<Guid> mExistingPrepMeths;

        public FormLabXPrepMeth(Guid labId, List<Guid> existingPrepMeths)
        {
            InitializeComponent();

            mLabId = labId;
            mExistingPrepMeths = existingPrepMeths;
        }

        private void FormLabXPrepMeth_Load(object sender, EventArgs e)
        {            
            SqlConnection conn = null;
            try
            {
                var pmArr = from item in mExistingPrepMeths select "'" + item + "'";
                string exceptIds = string.Join(",", pmArr);

                conn = DB.OpenConnection();
                string query;
                if (String.IsNullOrEmpty(exceptIds))                
                    query = "select * from preparation_method where instance_status_id < 2";                
                else                
                    query = "select * from preparation_method where id not in(" + exceptIds + ") and instance_status_id < 2";

                gridPrepMeth.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@lid", mLabId));

                gridPrepMeth.Columns["id"].Visible = false;
                gridPrepMeth.Columns["comment"].Visible = false;
                gridPrepMeth.Columns["instance_status_id"].Visible = false;
                gridPrepMeth.Columns["create_id"].Visible = false;
                gridPrepMeth.Columns["create_date"].Visible = false;
                gridPrepMeth.Columns["update_id"].Visible = false;
                gridPrepMeth.Columns["update_date"].Visible = false;

                gridPrepMeth.Columns["name"].HeaderText = "Name";
                gridPrepMeth.Columns["name_short"].HeaderText = "Abbr.";
                gridPrepMeth.Columns["description_link"].HeaderText = "Desc.link";
                gridPrepMeth.Columns["destructive"].HeaderText = "Destructive";
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
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                
                SqlCommand cmd = new SqlCommand("insert into laboratory_x_preparation_method values(@laboratory_id, @preparation_method_id)", conn, trans);
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
                return;
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
