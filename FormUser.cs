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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormUser : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid UserId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string UserName
        {
            get { return p.ContainsKey("username") ? p["username"].ToString() : String.Empty; }
        }

        public FormUser()
        {
            InitializeComponent();

            Text = "DSA-Lims - Create user";

            tbUsername.ReadOnly = false;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] { },  cboxPersons);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Inactive)
                }, cboxLaboratory);
            }
            
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormUser(Guid uid)
        {
            InitializeComponent();

            Text = "DSA-Lims - Edit user";

            p["id"] = uid;

            tbUsername.ReadOnly = true;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] { }, cboxPersons);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxLaboratory);

                SqlCommand cmd = new SqlCommand("csp_select_account", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Account with id " + p["id"] + " was not found");

                    reader.Read();
                    tbUsername.Text = reader["username"].ToString();
                    cboxPersons.SelectedValue = reader["person_id"];
                    cboxLaboratory.SelectedValue = reader["laboratory_id"];
                    cboxLanguage.Text = reader["language_code"].ToString();
                    cboxInstanceStatus.SelectedValue = reader["instance_status_id"];
                    p["create_date"] = reader["create_date"];
                    p["update_date"] = reader["update_date"];
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
            if (String.IsNullOrEmpty(tbUsername.Text.Trim()))
            {
                MessageBox.Show("Username is mandatory");
                return;
            }

            if(!Utils.IsValidGuid(cboxPersons.SelectedValue))
            {
                MessageBox.Show("Person is mandatory");
                return;
            }

            p["person_id"] = cboxPersons.SelectedValue;
            p["username"] = tbUsername.Text;
            p["laboratory_id"] = cboxLaboratory.SelectedValue;
            p["language_code"] = cboxLanguage.Text.Trim();
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;

            bool success;
            if (!p.ContainsKey("id"))            
                success = InsertAccount();            
            else            
                success = UpdateAccount();            

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertAccount()
        {
            SqlConnection connection = null;

            try
            {
                p["create_date"] = DateTime.Now;                
                p["update_date"] = DateTime.Now;

                connection = DB.OpenConnection();

                SqlCommand cmd = new SqlCommand("csp_insert_account", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();                
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@username", p["username"]);
                cmd.Parameters.AddWithValue("@person_id", DB.MakeParam(typeof(Guid), p["person_id"]));
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), p["laboratory_id"]));
                cmd.Parameters.AddWithValue("@language_code", p["language_code"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@password_hash", "");
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);                
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {                
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateAccount()
        {
            SqlConnection connection = null;            

            try
            {
                p["update_date"] = DateTime.Now;                

                connection = DB.OpenConnection();

                SqlCommand cmd = new SqlCommand("csp_update_account", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), p["laboratory_id"]));
                cmd.Parameters.AddWithValue("@language_code", p["language_code"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {                
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }
    }
}
