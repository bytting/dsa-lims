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
    public partial class FormSampleComponent : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid SampleComponentId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string SampleComponentName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormSampleComponent(Guid sampleTypeId, string sampleTypeName)
        {
            InitializeComponent();
            tbSampleType.Text = sampleTypeName;
            p["sample_type_id"] = sampleTypeId;            
        }

        public FormSampleComponent(Guid sampleTypeId, string sampleTypeName, Guid sampleComponentId)
        {
            InitializeComponent();
            tbSampleType.Text = sampleTypeName;
            p["id"] = sampleComponentId;                        
        }

        private void FormSampleComponent_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                if (p.ContainsKey("id"))
                {                    
                    SqlCommand cmd = new SqlCommand("csp_select_sample_component", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", p["id"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            throw new Exception("Sample component with ID " + p["id"] + " was not found");

                        reader.Read();

                        tbName.Text = reader.GetString("name");
                        p["sample_type_id"] = reader.GetGuid("sample_type_id");
                        p["create_date"] = reader["create_date"];
                        p["create_id"] = reader["create_id"];
                        p["update_date"] = reader["update_date"];
                        p["update_id"] = reader["update_id"];
                    }                
                }
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

            ActiveControl = tbName;
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
                MessageBox.Show("Geomery name is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();
                
                string query = "select count(*) from sample_component where name = @name and sample_type_id = @stid";
                SqlCommand cmd = new SqlCommand(query, connection, transaction);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@stid", p["sample_type_id"]);
                if (p.ContainsKey("id"))
                {
                    query += " and id not in(@exId)";
                    cmd.Parameters.AddWithValue("@exId", SampleComponentId);
                }

                int cnt = (int)cmd.ExecuteScalar();
                if (cnt > 0)
                {
                    MessageBox.Show("The sample component  '" + p["name"] + "' already exists for this sample type");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertSampleComponent(connection, transaction);
                else
                    UpdateSampleComponent(connection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                transaction?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private void InsertSampleComponent(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["create_id"] = Common.UserId;
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;        

            SqlCommand cmd = new SqlCommand("csp_insert_sample_component", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@sample_type_id", p["sample_type_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdateSampleComponent(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;

            SqlCommand cmd = new SqlCommand("csp_update_sample_component", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                        
        }        
    }
}
