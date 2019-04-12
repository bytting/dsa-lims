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
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormMunicipality : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid MunicipalityId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string MunicipalityName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormMunicipality(Guid cid, string countyName)
        {
            InitializeComponent();  
                      
            p["county_id"] = cid;
            Text = "Create municipality";
            tbCounty.Text = countyName;
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);
            }
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormMunicipality(Guid cid, Guid mid, string countyName)
        {
            InitializeComponent();

            p["county_id"] = cid;
            p["id"] = mid;
            Text = "Update municipality";
            tbCounty.Text = countyName;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                SqlCommand cmd = new SqlCommand("select name from county where id = @id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", p["county_id"]);
                tbCounty.Text = cmd.ExecuteScalar().ToString();

                cmd.CommandText = "csp_select_municipality";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Municipality with ID " + p["id"] + " was not found");

                    reader.Read();

                    tbName.Text = reader.GetString("name");
                    tbNumber.Text = reader.GetString("municipality_number");
                    cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                    p["create_date"] = reader["create_date"];
                    p["create_id"] = reader["create_id"];
                    p["update_date"] = reader["update_date"];
                    p["update_id"] = reader["update_id"];
                }
            }
        }

        private void FormMunicipality_Load(object sender, EventArgs e)
        {
            tbNumber.KeyPress += CustomEvents.Integer_KeyPress;
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
                MessageBox.Show("Name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbNumber.Text))
            {
                MessageBox.Show("Number is mandatory");
                return;
            }

            if (!Utils.IsValidInteger(tbNumber.Text))
            {
                MessageBox.Show("Number must be a number");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["number"] = Convert.ToInt32(tbNumber.Text);
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("", connection, transaction);
                string query = "select count(*) from municipality where name = @name and county_id = @cid";
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@cid", p["county_id"]);
                if (p.ContainsKey("id"))
                {
                    query += " and id not in(@exId)";
                    cmd.Parameters.AddWithValue("@exId", MunicipalityId);
                }
                cmd.CommandText = query;

                int cnt = (int)cmd.ExecuteScalar();
                if (cnt > 0)
                {
                    MessageBox.Show("The municipality '" + p["name"] + "' already exists");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertMunicipality(connection, transaction);
                else
                    UpdateMunicipality(connection, transaction);

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

        private void InsertMunicipality(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["create_id"] = Common.UserId;
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;        

            SqlCommand cmd = new SqlCommand("csp_insert_municipality", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@county_id", p["county_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@municipality_number", p["number"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdateMunicipality(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.Username;                

            SqlCommand cmd = new SqlCommand("csp_update_municipality", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@municipality_number", p["number"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                        
        }        
    }
}
