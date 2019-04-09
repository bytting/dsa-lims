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
    public partial class FormSampleNew : Form
    {
        private TreeView TreeSampleTypes;

        public Guid SampleId = Guid.Empty;
        public int SampleNumber;

        public FormSampleNew(TreeView treeSampleTypes)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;
            UI.PopulateSampleTypes(TreeSampleTypes, cboxSampleType);
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectMain(conn, null, Common.UserId, InstanceStatus.Active, cboxProjectMain);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxLaboratory);

                if(Utils.IsValidGuid(Common.LabId))
                {
                    cboxLaboratory.SelectedValue = Common.LabId;                    
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxProjectMain.SelectedValue))
            {
                MessageBox.Show("Main project is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxProjectSub.SelectedValue))
            {
                MessageBox.Show("Sub project is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }                                    

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                
                SampleNumber = DB.GetNextSampleCount(conn, trans);

                SqlCommand cmdCheck = new SqlCommand("select count(*) from sample where number = @number", conn, trans);
                cmdCheck.Parameters.AddWithValue("@number", SampleNumber);
                int cnt = (int)cmdCheck.ExecuteScalar();
                if (cnt > 0)
                {
                    MessageBox.Show("Sample number '" + SampleNumber + "' already exists");
                    return;
                }

                SqlCommand cmd = new SqlCommand("csp_insert_sample", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                SampleId = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", SampleId);
                cmd.Parameters.AddWithValue("@number", SampleNumber);
                cmd.Parameters.AddWithValue("@laboratory_id", cboxLaboratory.SelectedValue, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_type_id", cboxSampleType.SelectedValue, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_storage_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@sample_component_id", cboxSampleComponent.SelectedValue, Guid.Empty);
                cmd.Parameters.AddWithValue("@project_sub_id", cboxProjectSub.SelectedValue, Guid.Empty);
                cmd.Parameters.AddWithValue("@station_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@sampler_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@sampling_method_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@transform_from_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@transform_to_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@imported_from", DBNull.Value);
                cmd.Parameters.AddWithValue("@imported_from_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@municipality_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@location_type", DBNull.Value);
                cmd.Parameters.AddWithValue("@location", DBNull.Value);
                cmd.Parameters.AddWithValue("@latitude", DBNull.Value);
                cmd.Parameters.AddWithValue("@longitude", DBNull.Value);
                cmd.Parameters.AddWithValue("@altitude", DBNull.Value);
                cmd.Parameters.AddWithValue("@sampling_date_from", DBNull.Value);                
                cmd.Parameters.AddWithValue("@sampling_date_to", DBNull.Value);
                cmd.Parameters.AddWithValue("@reference_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@external_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@wet_weight_g", DBNull.Value);
                cmd.Parameters.AddWithValue("@dry_weight_g", DBNull.Value);
                cmd.Parameters.AddWithValue("@volume_l", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_weight_start", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_weight_end", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_temperature", DBNull.Value);
                cmd.Parameters.AddWithValue("@confidential", 0);                
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                cmd.Parameters.AddWithValue("@locked_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@create_id", Common.UserId);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_id", Common.UserId);

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

        private void btnSelectSampleType_Click(object sender, EventArgs e)
        {
            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleType.SelectedValue = form.SelectedSampleTypeId;
        }

        private void cboxProjectMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxProjectMain.SelectedValue))
            {
                cboxProjectSub.DataSource = null;
                return;
            }

            Guid projectMainId = Utils.MakeGuid(cboxProjectMain.SelectedValue);

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectSub(conn, null, projectMainId, Common.UserId, InstanceStatus.Active, cboxProjectSub);
            }
        }

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Utils.MakeGuid(cboxSampleType.SelectedValue);
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleComponent);                
            }
        }
    }
}
