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
    public partial class FormOrdersAssignUsers : Form
    {
        Guid mAssignmentId = Guid.Empty;
        
        public FormOrdersAssignUsers(Guid aid, string aname)
        {
            InitializeComponent();

            mAssignmentId = aid;
            lblOrder.Text = aname;
        }

        private void FormOrdersAssignUsers_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();                

                DataTable dt = DB.GetDataTable(conn, null, "select id, name from cv_account where email is not NULL order by name", CommandType.Text);

                DataColumn assignedColumn = new DataColumn("Assigned", typeof(bool));
                assignedColumn.DefaultValue = false;
                dt.Columns.Add(assignedColumn);

                gridUsers.DataSource = dt;

                gridUsers.ReadOnly = false;
                gridUsers.Columns["name"].ReadOnly = true;
                gridUsers.Columns["Assigned"].ReadOnly = false;

                gridUsers.Columns["id"].Visible = false;

                gridUsers.Columns["name"].HeaderText = "User";

                List<Guid> assignedUserIds = new List<Guid>();
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "select account_id from assignment_x_account where assignment_id = @aid", CommandType.Text, 
                    new SqlParameter("@aid", mAssignmentId)))
                {
                    while (reader.Read())
                        assignedUserIds.Add(reader.GetGuid("account_id"));
                }

                foreach (DataGridViewRow row in gridUsers.Rows)
                {
                    Guid uid = Utils.MakeGuid(row.Cells["id"].Value);
                    if (assignedUserIds.Contains(uid))
                    {
                        row.Cells["Assigned"].Value = true;
                    }
                }                
            }
            catch(Exception ex)
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

                SqlCommand cmd = new SqlCommand("delete from assignment_x_account where assignment_id = @aid", conn, trans);
                cmd.Parameters.AddWithValue("@aid", mAssignmentId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into assignment_x_account values(@aid, @accid)";
                foreach (DataGridViewRow row in gridUsers.Rows)
                {
                    if ((bool)row.Cells["Assigned"].Value == true)
                    {
                        Guid uid = Utils.MakeGuid(row.Cells["id"].Value);

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", mAssignmentId);
                        cmd.Parameters.AddWithValue("@accid", uid);
                        cmd.ExecuteNonQuery();
                    }
                }

                trans.Commit();
            }
            catch (Exception ex)
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
