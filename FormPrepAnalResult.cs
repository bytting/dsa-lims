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

using Newtonsoft.Json;
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
    public partial class FormPrepAnalResult : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();
        private Guid mUnitId = Guid.Empty;

        public Guid AnalysisResultId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public FormPrepAnalResult(Guid analId, Guid unitId, List<string> nuclides)
        {
            InitializeComponent();

            Text = "DSA-Lims - Add nuclide";
            cboxNuclides.DataSource = nuclides;
            cboxNuclides.Text = "";
            p["analysis_id"] = analId;
            mUnitId = unitId;
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxSigmaActivity.DataSource = DB.GetSigmaValues(conn);
                cboxSigmaMDA.DataSource = DB.GetSigmaMDAValues(conn);
            }
        }

        public FormPrepAnalResult(Guid resultId, Guid unitId, string nuclideName)
        {
            InitializeComponent();

            Text = "DSA-Lims - Edit nuclide";
            p["id"] = resultId;
            mUnitId = unitId;
            cboxNuclides.Items.Add(nuclideName);
            cboxNuclides.Text = nuclideName;
            cboxNuclides.Enabled = false;            

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxSigmaActivity.DataSource = DB.GetSigmaValues(conn);
                cboxSigmaMDA.DataSource = DB.GetSigmaMDAValues(conn);

                SqlDataReader reader = DB.GetDataReader(conn, "csp_select_analysis_result", CommandType.StoredProcedure, new SqlParameter("@id", resultId));
                if(reader.HasRows)
                {
                    reader.Read();                    
                    tbActivity.Text = reader["activity"].ToString();
                    tbUncertainty.Text = reader["activity_uncertainty_abs"].ToString();
                    cbActivityApproved.Checked = Convert.ToBoolean(reader["activity_approved"]);
                    tbDetectionLimit.Text = reader["detection_limit"].ToString();
                    cbDetectionLimitApproved.Checked = Convert.ToBoolean(reader["detection_limit_approved"]);
                    cbAccredited.Checked = Convert.ToBoolean(reader["accredited"]);
                    cbReportable.Checked = Convert.ToBoolean(reader["reportable"]);
                }                
            }
        }

        private void FormPrepAnalResult_Load(object sender, EventArgs e)
        {
            tbActivity.KeyPress += CustomEvents.Numeric_KeyPress;
            tbUncertainty.KeyPress += CustomEvents.Numeric_KeyPress;
            tbDetectionLimit.KeyPress += CustomEvents.Numeric_KeyPress;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbActivity.Text.Trim()))
            {
                MessageBox.Show("The activity field is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbUncertainty.Text.Trim()))
            {
                MessageBox.Show("The uncertainty field is mandatory");
                return;
            }

            if (!p.ContainsKey("id"))
            {
                if(String.IsNullOrEmpty(cboxNuclides.Text))
                {
                    MessageBox.Show("Nuclide is mandatory");
                    return;
                }

                using (SqlConnection conn = DB.OpenConnection())
                {
                    p["nuclide_id"] = DB.GetScalar(conn, "select id from nuclide where name = @nuclName", CommandType.Text, new SqlParameter("@nuclName", cboxNuclides.Text));
                }
            }

            p["activity"] = Convert.ToDouble(tbActivity.Text);
            p["activity_uncertainty_abs"] = Convert.ToDouble(tbUncertainty.Text);
            p["activity_approved"] = cbActivityApproved.Checked;
            p["detection_limit"] = Convert.ToDouble(tbDetectionLimit.Text);
            p["detection_limit_approved"] = cbDetectionLimitApproved.Checked;
            p["accredited"] = cbAccredited.Checked;
            p["reportable"] = cbReportable.Checked;

            bool success;
            if (!p.ContainsKey("id"))            
                success = InsertAnalysisResult();
            else
                success = UpdateAnalysisResult();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertAnalysisResult()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["instance_status_id"] = 1;
                p["create_date"] = DateTime.Now;
                p["created_by"] = Common.Username;
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_analysis_result", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@analysis_id", p["analysis_id"]);
                cmd.Parameters.AddWithValue("@nuclide_id", p["nuclide_id"]);
                cmd.Parameters.AddWithValue("@activity", p["activity"]);
                cmd.Parameters.AddWithValue("@activity_uncertainty_abs", p["activity_uncertainty_abs"]);
                cmd.Parameters.AddWithValue("@activity_approved", p["activity_approved"]);
                double uAct = -1.0;
                int uActUnitId = -1;
                if (Utils.IsValidGuid(mUnitId))
                {                    
                    DB.GetUniformActivity(connection, transaction, Convert.ToDouble(p["activity"]), mUnitId, out uAct, out uActUnitId);
                }
                cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                cmd.Parameters.AddWithValue("@detection_limit", p["detection_limit"]);
                cmd.Parameters.AddWithValue("@detection_limit_approved", p["detection_limit_approved"]);
                cmd.Parameters.AddWithValue("@accredited", p["accredited"]);
                cmd.Parameters.AddWithValue("@reportable", p["reportable"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
                cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "analysis_result", (Guid)p["id"], AuditOperationType.Insert, JsonConvert.SerializeObject(p));

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

        private bool UpdateAnalysisResult()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["instance_status_id"] = 1;                
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_analysis_result", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@activity", p["activity"]);
                cmd.Parameters.AddWithValue("@activity_uncertainty_abs", p["activity_uncertainty_abs"]);
                cmd.Parameters.AddWithValue("@activity_approved", p["activity_approved"]);
                double uAct = -1.0;
                int uActUnitId = -1;
                if (Utils.IsValidGuid(mUnitId))
                {
                    DB.GetUniformActivity(connection, transaction, Convert.ToDouble(p["activity"]), mUnitId, out uAct, out uActUnitId);
                }
                cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                cmd.Parameters.AddWithValue("@detection_limit", p["detection_limit"]);
                cmd.Parameters.AddWithValue("@detection_limit_approved", p["detection_limit_approved"]);
                cmd.Parameters.AddWithValue("@accredited", p["accredited"]);
                cmd.Parameters.AddWithValue("@reportable", p["reportable"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "analysis_result", (Guid)p["id"], AuditOperationType.Update, JsonConvert.SerializeObject(p));

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
