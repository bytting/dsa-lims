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
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormSampleStorage : Form
    {
        public SampleStorageModel SampleStorage = new SampleStorageModel();

        public FormSampleStorage()
        {
            InitializeComponent();     
            Text = "Create sample storage";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormSampleStorage(Guid ssid)
        {
            InitializeComponent();            
            SampleStorage.Id = ssid;
            Text = "Update sample storage";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_sample_storage", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SampleStorage.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Sample storage with ID " + SampleStorage.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbAddress.Text = reader["address"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    SampleStorage.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    SampleStorage.CreatedBy = reader["created_by"].ToString();
                    SampleStorage.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    SampleStorage.UpdatedBy = reader["updated_by"].ToString();
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

            SampleStorage.Name = tbName.Text.Trim();
            SampleStorage.Address = tbAddress.Text.Trim();
            SampleStorage.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            SampleStorage.Comment = tbComment.Text.Trim();

            bool success;
            if (SampleStorage.Id == Guid.Empty)
                success = InsertSampleStorage();
            else
                success = UpdateSampleStorage();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertSampleStorage()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SampleStorage.CreateDate = DateTime.Now;
                SampleStorage.CreatedBy = Common.Username;
                SampleStorage.UpdateDate = DateTime.Now;
                SampleStorage.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_sample_storage", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                SampleStorage.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", SampleStorage.Id);
                cmd.Parameters.AddWithValue("@name", SampleStorage.Name);
                cmd.Parameters.AddWithValue("@address", SampleStorage.Address);                
                cmd.Parameters.AddWithValue("@instance_status_id", SampleStorage.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SampleStorage.Comment);
                cmd.Parameters.AddWithValue("@create_date", SampleStorage.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", SampleStorage.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", SampleStorage.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SampleStorage.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sample_storage", SampleStorage.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(SampleStorage));

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

        private bool UpdateSampleStorage()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SampleStorage.UpdateDate = DateTime.Now;
                SampleStorage.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_sample_storage", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SampleStorage.Id);
                cmd.Parameters.AddWithValue("@name", SampleStorage.Name);
                cmd.Parameters.AddWithValue("@address", SampleStorage.Address);                
                cmd.Parameters.AddWithValue("@instance_status_id", SampleStorage.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SampleStorage.Comment);
                cmd.Parameters.AddWithValue("@update_date", SampleStorage.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SampleStorage.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sample_storage", SampleStorage.Id, AuditOperationType.Update, JsonConvert.SerializeObject(SampleStorage));

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
