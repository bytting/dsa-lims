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
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormPreparationMethod : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid PreparationMethodId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string PreparationMethodName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormPreparationMethod()
        {
            InitializeComponent();
            Text = "Create preparation method";
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");
            }            
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormPreparationMethod(Guid pmid)
        {
            InitializeComponent();
            p["id"] = pmid;
            Text = "Update preparation method";            

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");

                SqlCommand cmd = new SqlCommand("csp_select_preparation_method", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Preparation method with ID " + p["id"] + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbShortName.Text = reader["name_short"].ToString();
                    tbDescriptionLink.Text = reader["description_link"].ToString();                    
                    cbDestructive.Checked = Convert.ToBoolean(reader["destructive"]);
                    cboxInstanceStatus.SelectedValue = reader["instance_status_id"];
                    tbComment.Text = reader["comment"].ToString();
                    p["create_date"] = reader["create_date"];
                    p["created_by"] = reader["created_by"];
                    p["update_date"] = reader["update_date"];
                    p["updated_by"] = reader["updated_by"];
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbShortName.Text.Trim()))
            {
                MessageBox.Show("Short name is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["name_short"] = tbShortName.Text.Trim();
            p["description_link"] = tbDescriptionLink.Text.Trim();
            p["destructive"] = cbDestructive.Checked;
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                if (DB.NameExists(connection, transaction, "preparation_method", p["name"].ToString(), PreparationMethodId))
                {
                    MessageBox.Show("Preparation method '" + p["name"] + "' already exists");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertPreparationMethod(connection, transaction);
                else
                    UpdatePreparationMethod(connection, transaction);

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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InsertPreparationMethod(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["created_by"] = Common.Username;
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;        

            SqlCommand cmd = new SqlCommand("csp_insert_preparation_method", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_short", p["name_short"]);
            cmd.Parameters.AddWithValue("@description_link", p["description_link"]);
            cmd.Parameters.AddWithValue("@destructive", p["destructive"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"]);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdatePreparationMethod(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;        

            SqlCommand cmd = new SqlCommand("csp_update_preparation_method", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_short", p["name_short"]);
            cmd.Parameters.AddWithValue("@description_link", p["description_link"]);
            cmd.Parameters.AddWithValue("@destructive", p["destructive"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                
        }
    }
}
