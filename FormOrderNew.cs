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

                if(Utils.IsValidGuid(Common.LabId))
                {
                    cboxLaboratory.SelectedValue = Common.LabId;
                }

                cboxRequestedSigma.DataSource = DB.GetSigmaValues();
                cboxRequestedSigmaMDA.DataSource = DB.GetSigmaMDAValues();
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

            if (tbDeadline.Tag == null)
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }            

            DateTime dl = (DateTime)tbDeadline.Tag;
            if(dl < DateTime.Now)
            {
                MessageBox.Show("Deadline can not be in the past");
                return;
            }

            if(tbCustomer.Tag == null)
            {
                MessageBox.Show("Customer is mandatory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                CustomerModel cust = (CustomerModel)tbCustomer.Tag;
                Guid labId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());

                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                
                string labPrefix = DB.GetOrderPrefix(conn, trans, labId);
                int orderCount = DB.GetNextOrderCount(conn, trans, labId);
                OrderName = labPrefix + "-" + DateTime.Now.ToString("yyyy") + "-" + orderCount;

                SqlCommand cmd = new SqlCommand("csp_insert_assignment", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                OrderId = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", OrderId);
                cmd.Parameters.AddWithValue("@name", OrderName);
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), labId));
                cmd.Parameters.AddWithValue("@account_id", DB.MakeParam(typeof(Guid), cboxResponsible.SelectedValue));
                cmd.Parameters.AddWithValue("@deadline", (DateTime)tbDeadline.Tag);
                cmd.Parameters.AddWithValue("@requested_sigma_act", DB.MakeParam(typeof(double), cboxRequestedSigma.SelectedValue));
                cmd.Parameters.AddWithValue("@requested_sigma_mda", DB.MakeParam(typeof(double), cboxRequestedSigmaMDA.SelectedValue));
                cmd.Parameters.AddWithValue("@customer_name", DB.MakeParam(typeof(string), cust.Name));                                
                cmd.Parameters.AddWithValue("@customer_email", DB.MakeParam(typeof(string), cust.Email));
                cmd.Parameters.AddWithValue("@customer_phone", DB.MakeParam(typeof(string), cust.Phone));
                cmd.Parameters.AddWithValue("@customer_address", DB.MakeParam(typeof(string), cust.Address));
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
            {
                cboxResponsible.SelectedValue = Guid.Empty;
                return;
            }

            Guid labId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_accounts_for_laboratory", new[] {
                    new SqlParameter("@laboratory_id", labId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxResponsible);
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

        private void btnSelectCustomer_Click(object sender, EventArgs e)
        {
            FormSelectCustomer form = new FormSelectCustomer();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbCustomer.Text = form.SelectedCustomer.Name;
            tbCustomer.Tag = form.SelectedCustomer;
        }
    }
}
