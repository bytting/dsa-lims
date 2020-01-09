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
    public partial class FormPrepAnalAddPrep : Form
    {
        Sample mSample = null;

        public FormPrepAnalAddPrep(Sample sample)
        {
            InitializeComponent();

            mSample = sample;

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void FormPrepAnalAddPrep_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleTypePrepMeth(conn, mSample.SampleTypeId, Common.LabId, cboxPrepMethods);
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
            if(count == 0)
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

                int nextPrepNumber = DB.GetNextPreparationNumber(connection, transaction, mSample.Id);

                SqlCommand cmd = new SqlCommand("csp_insert_preparation", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                while (count > 0)
                {
                    Guid newPrepId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newPrepId);
                    cmd.Parameters.AddWithValue("@sample_id", mSample.Id, Guid.Empty);
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
                    cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@volume_l", DBNull.Value);
                    cmd.Parameters.AddWithValue("@preprocessing_volume_l", DBNull.Value);

                    cmd.ExecuteNonQuery();
                    count--;
                }

                mSample.LoadFromDB(connection, transaction, mSample.Id);

                string json = JsonConvert.SerializeObject(mSample);
                DB.AddAuditMessage(connection, transaction, "sample", mSample.Id, AuditOperationType.Update, json, "");

                transaction.Commit();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                DialogResult = DialogResult.Abort;
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
            
            Close();
        }        
    }
}
