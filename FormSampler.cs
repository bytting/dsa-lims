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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace DSA_lims
{
    public partial class FormSampler : Form
    {
        private ILog mLog = null;

        public SamplerModel Sampler = new SamplerModel();

        public FormSampler(ILog log)
        {
            InitializeComponent();
            mLog = log;
            Text = "Create sampler";
            cbInUse.Checked = true;
        }

        public FormSampler(ILog log, Guid sid)
        {
            InitializeComponent();
            mLog = log;
            Sampler.Id = sid;
            Text = "Update sampler";

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_sampler", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Sampler.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Sampler with ID " + Sampler.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbAddress.Text = reader["address"].ToString();
                    tbEmail.Text = reader["email"].ToString();
                    tbPhone.Text = reader["phone"].ToString();
                    cbInUse.Checked = InstanceStatus.IsActive(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    Sampler.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Sampler.CreatedBy = reader["created_by"].ToString();
                    Sampler.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Sampler.UpdatedBy = reader["updated_by"].ToString();
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

            Sampler.Name = tbName.Text.Trim();
            Sampler.Address = tbAddress.Text.Trim();
            Sampler.Email = tbEmail.Text.Trim();
            Sampler.Phone = tbPhone.Text.Trim();
            Sampler.InstanceStatusId = cbInUse.Checked ? 1 : 2;
            Sampler.Comment = tbComment.Text.Trim();

            bool success;
            if (Sampler.Id == Guid.Empty)
                success = InsertSampler();
            else
                success = UpdateSampler();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertSampler()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Sampler.CreateDate = DateTime.Now;
                Sampler.CreatedBy = Common.Username;
                Sampler.UpdateDate = DateTime.Now;
                Sampler.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_sampler", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Sampler.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Sampler.Id);
                cmd.Parameters.AddWithValue("@name", Sampler.Name);
                cmd.Parameters.AddWithValue("@address", Sampler.Address);
                cmd.Parameters.AddWithValue("@email", Sampler.Email);
                cmd.Parameters.AddWithValue("@phone", Sampler.Phone);
                cmd.Parameters.AddWithValue("@instance_status_id", Sampler.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Sampler.Comment);
                cmd.Parameters.AddWithValue("@create_date", Sampler.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Sampler.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Sampler.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Sampler.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sampler", Sampler.Id, AuditOperation.Insert, JsonConvert.SerializeObject(Sampler));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateSampler()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Sampler.UpdateDate = DateTime.Now;
                Sampler.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_sampler", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Sampler.Id);
                cmd.Parameters.AddWithValue("@name", Sampler.Name);
                cmd.Parameters.AddWithValue("@address", Sampler.Address);
                cmd.Parameters.AddWithValue("@email", Sampler.Email);
                cmd.Parameters.AddWithValue("@phone", Sampler.Phone);
                cmd.Parameters.AddWithValue("@instance_status_id", Sampler.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Sampler.Comment);
                cmd.Parameters.AddWithValue("@update_date", Sampler.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Sampler.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sampler", Sampler.Id, AuditOperation.Update, JsonConvert.SerializeObject(Sampler));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
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
