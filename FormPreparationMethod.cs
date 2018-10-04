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
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormPreparationMethod : Form
    {
        public PreparationMethodModel PreparationMethod = new PreparationMethodModel();

        public FormPreparationMethod()
        {
            InitializeComponent();
            Text = "Create preparation method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormPreparationMethod(Guid pmid)
        {
            InitializeComponent();
            PreparationMethod.Id = pmid;
            Text = "Update preparation method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_preparation_method", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", PreparationMethod.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Preparation method with ID " + PreparationMethod.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbDescriptionLink.Text = reader["description_link"].ToString();                    
                    cbDestructive.Checked = Convert.ToBoolean(reader["destructive"]);
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    PreparationMethod.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    PreparationMethod.CreatedBy = reader["created_by"].ToString();
                    PreparationMethod.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    PreparationMethod.UpdatedBy = reader["updated_by"].ToString();
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Name is mandatory");
                return;
            }

            PreparationMethod.Name = tbName.Text.Trim();
            PreparationMethod.DescriptionLink = tbDescriptionLink.Text.Trim();
            PreparationMethod.Destructive = cbDestructive.Checked;
            PreparationMethod.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            PreparationMethod.Comment = tbComment.Text.Trim();

            bool success;
            if (PreparationMethod.Id == Guid.Empty)
                success = InsertPreparationMethod();
            else
                success = UpdatePreparationMethod();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private bool InsertPreparationMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                PreparationMethod.CreateDate = DateTime.Now;
                PreparationMethod.CreatedBy = Common.Username;
                PreparationMethod.UpdateDate = DateTime.Now;
                PreparationMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_preparation_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                PreparationMethod.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", PreparationMethod.Id);
                cmd.Parameters.AddWithValue("@name", PreparationMethod.Name);
                cmd.Parameters.AddWithValue("@description_link", PreparationMethod.DescriptionLink);
                cmd.Parameters.AddWithValue("@destructive", PreparationMethod.Destructive);
                cmd.Parameters.AddWithValue("@instance_status_id", PreparationMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", PreparationMethod.Comment);
                cmd.Parameters.AddWithValue("@create_date", PreparationMethod.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", PreparationMethod.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", PreparationMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", PreparationMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "preparation_method", PreparationMethod.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(PreparationMethod));

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

        private bool UpdatePreparationMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                PreparationMethod.UpdateDate = DateTime.Now;
                PreparationMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_preparation_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", PreparationMethod.Id);
                cmd.Parameters.AddWithValue("@name", PreparationMethod.Name);
                cmd.Parameters.AddWithValue("@description_link", PreparationMethod.DescriptionLink);
                cmd.Parameters.AddWithValue("@destructive", PreparationMethod.Destructive);
                cmd.Parameters.AddWithValue("@instance_status_id", PreparationMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", PreparationMethod.Comment);
                cmd.Parameters.AddWithValue("@update_date", PreparationMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", PreparationMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "preparation_method", PreparationMethod.Id, AuditOperationType.Update, JsonConvert.SerializeObject(PreparationMethod));

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
