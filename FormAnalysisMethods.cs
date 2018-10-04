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
    public partial class FormAnalysisMethods : Form
    {
        public AnalysisMethodModel AnalysisMethod = new AnalysisMethodModel();

        public FormAnalysisMethods()
        {
            InitializeComponent();
            Text = "Create analysis method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormAnalysisMethods(Guid amid)
        {
            InitializeComponent();
            AnalysisMethod.Id = amid;
            Text = "Update analysis method";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_analysis_method", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", AnalysisMethod.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Analysis method with ID " + AnalysisMethod.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbDescriptionLink.Text = reader["description_link"].ToString();
                    tbSpecRefRegExp.Text = reader["specter_reference_regexp"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    AnalysisMethod.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    AnalysisMethod.CreatedBy = reader["created_by"].ToString();
                    AnalysisMethod.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    AnalysisMethod.UpdatedBy = reader["updated_by"].ToString();
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

            AnalysisMethod.Name = tbName.Text.Trim();
            AnalysisMethod.DescriptionLink = tbDescriptionLink.Text.Trim();
            AnalysisMethod.SpecterReferenceRegExp = tbSpecRefRegExp.Text.Trim();
            AnalysisMethod.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            AnalysisMethod.Comment = tbComment.Text.Trim();

            bool success;
            if (AnalysisMethod.Id == Guid.Empty)
                success = InsertAnalysisMethod();
            else
                success = UpdateAnalysisMethod();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertAnalysisMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                AnalysisMethod.CreateDate = DateTime.Now;
                AnalysisMethod.CreatedBy = Common.Username;
                AnalysisMethod.UpdateDate = DateTime.Now;
                AnalysisMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_analysis_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                AnalysisMethod.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", AnalysisMethod.Id);
                cmd.Parameters.AddWithValue("@name", AnalysisMethod.Name);
                cmd.Parameters.AddWithValue("@description_link", AnalysisMethod.DescriptionLink);
                cmd.Parameters.AddWithValue("@specter_reference_regexp", AnalysisMethod.SpecterReferenceRegExp);
                cmd.Parameters.AddWithValue("@instance_status_id", AnalysisMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", AnalysisMethod.Comment);
                cmd.Parameters.AddWithValue("@create_date", AnalysisMethod.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", AnalysisMethod.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", AnalysisMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", AnalysisMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "analysis_method", AnalysisMethod.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(AnalysisMethod));

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

        private bool UpdateAnalysisMethod()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                AnalysisMethod.UpdateDate = DateTime.Now;
                AnalysisMethod.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_analysis_method", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", AnalysisMethod.Id);
                cmd.Parameters.AddWithValue("@name", AnalysisMethod.Name);
                cmd.Parameters.AddWithValue("@description_link", AnalysisMethod.DescriptionLink);
                cmd.Parameters.AddWithValue("@specter_reference_regexp", AnalysisMethod.SpecterReferenceRegExp);
                cmd.Parameters.AddWithValue("@instance_status_id", AnalysisMethod.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", AnalysisMethod.Comment);
                cmd.Parameters.AddWithValue("@update_date", AnalysisMethod.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", AnalysisMethod.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "analysis_method", AnalysisMethod.Id, AuditOperationType.Update, JsonConvert.SerializeObject(AnalysisMethod));

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
