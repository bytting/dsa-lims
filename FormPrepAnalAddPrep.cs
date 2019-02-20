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
    public partial class FormPrepAnalAddPrep : Form
    {
        Guid SampleId = Guid.Empty;

        public FormPrepAnalAddPrep(Guid sampleId)
        {
            InitializeComponent();

            SampleId = sampleId;

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void FormPrepAnalAddPrep_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_preparation_methods_short", new[] {
                    new SqlParameter("instance_status_level", InstanceStatus.Active)
                }, cboxPrepMethods);
            }

            tbCount.Text = "1";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxPrepMethods.SelectedValue))
            {
                MessageBox.Show("Preparation method is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Count is mandatory");
                return;
            }

            int count = Convert.ToInt32(tbCount.Text.Trim());

            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                int nextPrepNumber = DB.GetNextPreparationNumber(connection, transaction, SampleId);

                SqlCommand cmd = new SqlCommand("csp_insert_preparation", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                while (count > 0)
                {
                    Guid newPrepId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newPrepId);
                    cmd.Parameters.AddWithValue("@sample_id", SampleId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@number", nextPrepNumber++);
                    cmd.Parameters.AddWithValue("@assignment_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@laboratory_id", Common.LabId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@preparation_geometry_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@preparation_method_id", cboxPrepMethods.SelectedValue, Guid.Empty);
                    cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                    cmd.Parameters.AddWithValue("@amount", DBNull.Value);
                    cmd.Parameters.AddWithValue("@prep_unit_id", 0);
                    cmd.Parameters.AddWithValue("@quantity", DBNull.Value);
                    cmd.Parameters.AddWithValue("@quantity_unit_id", 0);
                    cmd.Parameters.AddWithValue("@fill_height_mm", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username, String.Empty);

                    cmd.ExecuteNonQuery();
                    count--;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
