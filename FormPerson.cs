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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPerson : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid PersonId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string PersonName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormPerson()
        {
            InitializeComponent();

            Text = "New person";
        }

        public FormPerson(Guid pid)
        {
            InitializeComponent();

            Text = "Edit person";
            p["id"] = pid;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_person", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Person with ID " + p["id"] + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();                    
                    tbEmail.Text = reader["email"].ToString();
                    tbPhone.Text = reader["phone"].ToString();
                    tbAddress.Text = reader["address"].ToString();                    
                    p["create_date"] = reader["create_date"];                    
                    p["update_date"] = reader["update_date"];
                }
            }
        }

        private void FormPerson_Load(object sender, EventArgs e)
        {
            //
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

            p["name"] = tbName.Text.Trim();            
            p["email"] = tbEmail.Text.Trim();
            p["phone"] = tbPhone.Text.Trim();
            p["address"] = tbAddress.Text.Trim();

            bool success;
            if (!p.ContainsKey("id"))
                success = InsertPerson();
            else
                success = UpdatePerson();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertPerson()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["create_date"] = DateTime.Now;                
                p["update_date"] = DateTime.Now;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_person", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@email", p["email"]);
                cmd.Parameters.AddWithValue("@phone", p["phone"]);
                cmd.Parameters.AddWithValue("@address", p["address"]);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);                
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "person", (Guid)p["id"], AuditOperationType.Insert, JsonConvert.SerializeObject(p));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdatePerson()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["update_date"] = DateTime.Now;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_person", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@email", p["email"]);
                cmd.Parameters.AddWithValue("@phone", p["phone"]);
                cmd.Parameters.AddWithValue("@address", p["address"]);                
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "person", (Guid)p["id"], AuditOperationType.Update, JsonConvert.SerializeObject(p));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
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
