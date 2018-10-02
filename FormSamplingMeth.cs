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
    public partial class FormSamplingMeth : Form
    {
        public SamplingMethodModel SamplingMethod = new SamplingMethodModel();

        public FormSamplingMeth()
        {
            InitializeComponent();
            Text = "Create sampling method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormSamplingMeth(Guid smid)
        {
            InitializeComponent();
            SamplingMethod.Id = smid;
            Text = "Update sampling method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_sampling_method", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SamplingMethod.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Sampler with ID " + SamplingMethod.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    SamplingMethod.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    SamplingMethod.CreatedBy = reader["created_by"].ToString();
                    SamplingMethod.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    SamplingMethod.UpdatedBy = reader["updated_by"].ToString();
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
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Name is mandatory");
                return;
            }

            SamplingMethod.Name = tbName.Text.Trim();
            SamplingMethod.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            SamplingMethod.Comment = tbComment.Text.Trim();

            bool success;
            if (SamplingMethod.Id == Guid.Empty)
                success = InsertSamplingMethod();
            else
                success = UpdateSamplingMethod();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;            
            Close();
        }

        private bool InsertSamplingMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SamplingMethod.CreateDate = DateTime.Now;
                SamplingMethod.CreatedBy = Common.Username;
                SamplingMethod.UpdateDate = DateTime.Now;
                SamplingMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_sampling_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                SamplingMethod.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", SamplingMethod.Id);
                cmd.Parameters.AddWithValue("@name", SamplingMethod.Name);
                cmd.Parameters.AddWithValue("@instance_status_id", SamplingMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SamplingMethod.Comment);
                cmd.Parameters.AddWithValue("@create_date", SamplingMethod.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", SamplingMethod.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", SamplingMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SamplingMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sampling_method", SamplingMethod.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(SamplingMethod));

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

        private bool UpdateSamplingMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SamplingMethod.UpdateDate = DateTime.Now;
                SamplingMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_sampling_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SamplingMethod.Id);
                cmd.Parameters.AddWithValue("@name", SamplingMethod.Name);                
                cmd.Parameters.AddWithValue("@instance_status_id", SamplingMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SamplingMethod.Comment);
                cmd.Parameters.AddWithValue("@update_date", SamplingMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SamplingMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sampling_method", SamplingMethod.Id, AuditOperationType.Update, JsonConvert.SerializeObject(SamplingMethod));

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
