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
    public partial class FormProjectSubXUsers : Form
    {
        Guid mProjectSubId = Guid.Empty;
        List<Guid> mExistingUsers = null;

        public FormProjectSubXUsers(Guid projectSubId, List<Guid> existingUsers)
        {
            InitializeComponent();

            mProjectSubId = projectSubId;
            mExistingUsers = existingUsers;
        }

        private void FormProjectSubXUsers_Load(object sender, EventArgs e)
        {            
            SqlConnection conn = null;
            try
            {
                var uArr = from item in mExistingUsers select "'" + item + "'";
                string exceptIds = string.Join(",", uArr);

                conn = DB.OpenConnection();
                string query;
                if (String.IsNullOrEmpty(exceptIds))
                    query = "select id, name, username from cv_account where instance_status_id < 2 and email is not NULL";
                else
                    query = "select id, name, username from cv_account where id not in(" + exceptIds + ") and instance_status_id < 2 and email is not NULL";

                gridUsers.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text);

                gridUsers.Columns["id"].Visible = false;

                gridUsers.Columns["name"].HeaderText = "Name";
                gridUsers.Columns["username"].HeaderText = "Username";
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

                SqlCommand cmd = new SqlCommand("insert into project_sub_x_account values(@psid, @aid)", conn, trans);                
                foreach(DataGridViewRow row in gridUsers.SelectedRows)
                {
                    Guid aid = Utils.MakeGuid(row.Cells["id"].Value);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@psid", mProjectSubId);
                    cmd.Parameters.AddWithValue("@aid", aid);
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
