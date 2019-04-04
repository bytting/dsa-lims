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
    public partial class FormPrepAnalAddAnal : Form
    {
        private Preparation mPrep = null;

        public FormPrepAnalAddAnal(Preparation prep)
        {
            InitializeComponent();

            mPrep = prep;

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void FormPrepAnalAddAnal_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_analysis_methods_for_laboratory_short", new[] {
                    new SqlParameter("laboratory_id", Common.LabId),
                    new SqlParameter("preparation_method_id", mPrep.PreparationMethodId),
                    new SqlParameter("instance_status_level", InstanceStatus.Active)
                }, cboxAnalMethods);
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
            if (!Utils.IsValidGuid(cboxAnalMethods.SelectedValue))
            {
                MessageBox.Show("Analysis method is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Count is mandatory");
                return;
            }

            int count = Convert.ToInt32(tbCount.Text.Trim());
            if (count == 0)
            {
                MessageBox.Show("Count can not be zero");
                return;
            }

            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                int nextAnalNumber = DB.GetNextAnalysisNumber(connection, transaction, mPrep.Id);

                SqlCommand cmd = new SqlCommand("csp_insert_analysis", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                while (count > 0)
                {
                    Guid newAnalId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newAnalId);
                    cmd.Parameters.AddWithValue("@number", nextAnalNumber++);
                    cmd.Parameters.AddWithValue("@assignment_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@laboratory_id", Common.LabId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@preparation_id", mPrep.Id);
                    cmd.Parameters.AddWithValue("@analysis_method_id", cboxAnalMethods.SelectedValue, Guid.Empty);
                    cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                    cmd.Parameters.AddWithValue("@specter_reference", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sigma_act", 0d);
                    cmd.Parameters.AddWithValue("@sigma_mda", 0d);
                    cmd.Parameters.AddWithValue("@nuclide_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@mda_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

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
