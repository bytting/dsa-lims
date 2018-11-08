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
    public partial class FormOrderNew : Form
    {
        public Guid OrderId = Guid.Empty;
        public string OrderName = String.Empty;

        public FormOrderNew()
        {
            InitializeComponent();
        }

        private void FormOrderNew_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {                
                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxLaboratory);
                
                UI.PopulateComboBoxes(conn, "csp_select_customers", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxCustomer);

                cboxRequestedSigma.DataSource = DB.GetSigmaValues();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxResponsible.SelectedValue))
            {
                MessageBox.Show("Responsible is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbDeadline.Text))
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxRequestedSigma.SelectedValue))
            {
                MessageBox.Show("Requested sigma is mandatory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                string customerName, customerAddress, customerEmail, customerPhone;
                Guid custId = Guid.Parse(cboxCustomer.SelectedValue.ToString());
                Guid labId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());
                string username = cboxResponsible.SelectedValue.ToString();

                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select * from customer where id = @id", CommandType.Text, 
                    new SqlParameter("@id", custId)))
                {
                    reader.Read();
                    customerName = reader["name"].ToString();
                    customerAddress = reader["address"].ToString();
                    customerEmail = reader["email"].ToString();
                    customerPhone = reader["phone"].ToString();
                }
                
                string labPrefix = DB.GetOrderPrefix(conn, trans, labId);
                int orderCount = DB.GetNextOrderCount(conn, trans, labId);
                OrderName = labPrefix + "-" + DateTime.Now.ToString("yyyy") + "-" + orderCount;

                SqlCommand cmd = new SqlCommand("csp_insert_assignment", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                OrderId = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", OrderId);
                cmd.Parameters.AddWithValue("@name", OrderName);
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), labId));                
                cmd.Parameters.AddWithValue("@account_id", username);
                cmd.Parameters.AddWithValue("@deadline", (DateTime)tbDeadline.Tag);
                cmd.Parameters.AddWithValue("@requested_sigma", cboxRequestedSigma.SelectedValue);
                cmd.Parameters.AddWithValue("@customer_name", customerName);
                cmd.Parameters.AddWithValue("@customer_address", customerAddress);
                cmd.Parameters.AddWithValue("@customer_email", customerEmail);
                cmd.Parameters.AddWithValue("@customer_phone", customerPhone);
                cmd.Parameters.AddWithValue("@customer_contact_name", DBNull.Value);
                cmd.Parameters.AddWithValue("@customer_contact_email", DBNull.Value);
                cmd.Parameters.AddWithValue("@customer_contact_phone", DBNull.Value);
                cmd.Parameters.AddWithValue("@approved_customer", 0);
                cmd.Parameters.AddWithValue("@approved_laboratory", 0);
                cmd.Parameters.AddWithValue("@content_comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@report_comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
                cmd.Parameters.AddWithValue("@closed_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                cmd.Parameters.AddWithValue("@locked_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();
                trans.Commit();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
            }
            finally
            {
                conn?.Close();
            }
            
            Close();
        }

        private void cboxLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
                return;

            Guid labId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, labId, InstanceStatus.Active, cboxResponsible);
            }
        }

        private void btnSelectDeadline_Click(object sender, EventArgs e)
        {
            FormSelectDate form = new FormSelectDate();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            DateTime selectedDate = form.SelectedDate;
            tbDeadline.Tag = selectedDate;
            tbDeadline.Text = selectedDate.ToString(Utils.DateFormatNorwegian);
        }
    }
}
