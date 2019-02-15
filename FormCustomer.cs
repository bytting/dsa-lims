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
    public partial class FormCustomer : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid CustomerId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }
        
        public FormCustomer()
        {
            InitializeComponent();

            Text = "DSA-Lims - New customer";
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] {}, cboxPerson);

                UI.PopulateComboBoxes(conn, "csp_select_companies_short", new[] {
                    new SqlParameter("instance_status_level", InstanceStatus.Active)
                }, cboxCompany);

                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");
            }            
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormCustomer(Guid customerId)
        {
            InitializeComponent();

            Text = "DSA-Lims - Edit customer";
            p["id"] = customerId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_persons_short", new SqlParameter[] { }, cboxPerson);

                UI.PopulateComboBoxes(conn, "csp_select_companies_short", new[] {
                    new SqlParameter("instance_status_level", InstanceStatus.Active)
                }, cboxCompany);

                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");

                SqlCommand cmd = new SqlCommand("csp_select_customer", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Common.Log.Error("Customer with ID " + p["id"] + " was not found");
                        MessageBox.Show("Customer with ID " + p["id"] + " was not found");
                        Close();
                    }

                    reader.Read();

                    cboxPerson.SelectedValue = reader.GetGuid("person_id");
                    cboxCompany.SelectedValue = reader.GetGuid("company_id");
                    cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                    tbComment.Text = reader.GetString("comment");
                    p["create_date"] = reader.GetDateTime("create_date");
                    p["created_by"] = reader.GetString("created_by");
                    p["update_date"] = reader.GetDateTime("update_date");
                    p["updated_by"] = reader.GetString("updated_by");

                    cboxPerson.Enabled = false;
                }
            }
        }

        private void FormCustomer_Load(object sender, EventArgs e)
        {            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxPerson.SelectedValue))
            {
                MessageBox.Show("Person is mandatory");
                return;
            }

            p["person_id"] = cboxPerson.SelectedValue;
            p["company_id"] = cboxCompany.SelectedValue;
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();
            
                if (!p.ContainsKey("id"))
                    InsertCustomer(connection, transaction);
                else
                    UpdateCustomer(connection, transaction);

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

        private void InsertCustomer(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["created_by"] = Common.Username;
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;        

            SqlCommand cmd = new SqlCommand("csp_insert_customer", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@person_id", p["person_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@company_id", p["company_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdateCustomer(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;        

            SqlCommand cmd = new SqlCommand("csp_update_customer", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@company_id", p["company_id"], Guid.Empty);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                        
        }        
    }
}
