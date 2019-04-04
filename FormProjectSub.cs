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
    public partial class FormProjectSub : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid ProjectSubId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string ProjectSubName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }        

        public FormProjectSub(string pname, Guid pid)
        {
            InitializeComponent();            
            p["project_main_id"] = pid;            
            Text = "Create new sub project";
            tbMainProjectName.Text = pname;
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);
            }            
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormProjectSub(string pname, Guid pid, Guid spid)
        {
            InitializeComponent();            
            p["id"] = spid;
            p["project_main_id"] = pid;            
            Text = "Update sub project";
            tbMainProjectName.Text = pname;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                SqlCommand cmd = new SqlCommand("csp_select_project_sub", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Project with ID " + p["id"] + " was not found");

                    reader.Read();

                    tbName.Text = reader.GetString("name");
                    cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                    tbComment.Text = reader.GetString("comment");
                    p["create_date"] = reader["create_date"];
                    p["create_id"] = reader["create_id"];
                    p["update_date"] = reader["update_date"];
                    p["update_id"] = reader["update_id"];
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Project name is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("", connection, transaction);
                string query = "select count(*) from project_sub where name = @name and project_main_id = @pmid";
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@pmid", p["project_main_id"]);
                if (p.ContainsKey("id"))
                {
                    query += " and id not in(@exId)";
                    cmd.Parameters.AddWithValue("@exId", ProjectSubId);
                }
                cmd.CommandText = query;

                int cnt = (int)cmd.ExecuteScalar();            
                if(cnt > 0)
                {
                    MessageBox.Show("The sub project '" + p["name"] + "' already exists");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertSubProject(connection, transaction);
                else
                    UpdateSubProject(connection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                transaction?.Rollback();
                Common.Log.Error(ex);                
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private void InsertSubProject(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["create_id"] = Common.UserId;
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;                

            SqlCommand cmd = new SqlCommand("csp_insert_project_sub", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@project_main_id", p["project_main_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();        
        }

        private void UpdateSubProject(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;

            SqlCommand cmd = new SqlCommand("csp_update_project_sub", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@project_main_id", p["project_main_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();
        }
    }
}
