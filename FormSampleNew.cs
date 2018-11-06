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
                UI.PopulateProjectsMain(conn, cboxProjectMain);
                UI.PopulateLaboratories(conn, InstanceStatus.Active, cboxLaboratory);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (cboxSampleType.SelectedItem == null)
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if (cboxProjectMain.SelectedItem == null)
            {
                MessageBox.Show("Main project is mandatory");
                return;
            }

            if (cboxProjectSub.SelectedItem == null)
            {
                MessageBox.Show("Sub project is mandatory");
                return;
            }

            if (cboxLaboratory.SelectedItem == null)
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

                Lemma<Guid, string> lab = cboxLaboratory.SelectedItem as Lemma<Guid, string>;
                SampleNumber = DB.GetNextSampleCount(conn, trans);

                SqlCommand cmd = new SqlCommand("csp_insert_sample", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                SampleId = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", SampleId);
                cmd.Parameters.AddWithValue("@number", SampleNumber);
                cmd.Parameters.AddWithValue("@laboratory_id", Lemma<Guid, string>.IdParam(cboxLaboratory.SelectedItem));
                cmd.Parameters.AddWithValue("@sample_type_id", Lemma<Guid, string>.IdParam(cboxSampleType.SelectedItem));
                cmd.Parameters.AddWithValue("@sample_storage_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@sample_component_id", Lemma<Guid, string>.IdParam(cboxSampleComponent.SelectedItem));
                cmd.Parameters.AddWithValue("@project_sub_id", Lemma<Guid, string>.IdParam(cboxProjectSub.SelectedItem));
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
                cmd.Parameters.AddWithValue("@sampling_date_from", DateTime.Now);
                cmd.Parameters.AddWithValue("@use_sampling_date_to", 0);
                cmd.Parameters.AddWithValue("@sampling_date_to", DateTime.Now);
                cmd.Parameters.AddWithValue("@reference_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@external_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@wet_weight_g", DBNull.Value);
                cmd.Parameters.AddWithValue("@dry_weight_g", DBNull.Value);
                cmd.Parameters.AddWithValue("@volume_l", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_weight_start", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_weight_end", DBNull.Value);
                cmd.Parameters.AddWithValue("@lod_temperature", DBNull.Value);
                cmd.Parameters.AddWithValue("@confidential", 0);
                cmd.Parameters.AddWithValue("@parameters", DBNull.Value);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                cmd.Parameters.AddWithValue("@locked_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

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

            cboxSampleType.SelectedIndex = cboxSampleType.FindStringExact(
                form.SelectedSampleTypeName + " -> " + form.SelectedSampleTypePath);
        }

        private void cboxProjectMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cboxProjectMain.SelectedItem == null)
            {
                cboxProjectSub.SelectedIndex = -1;
                return;
            }

            Lemma<Guid, string> projectMain = cboxProjectMain.SelectedItem as Lemma<Guid, string>;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectsSub(conn, projectMain.Id, cboxProjectSub);
            }
        }

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboxSampleComponent.Items.Clear();

            if (cboxSampleType.SelectedItem == null)
                return;

            var sampleType = cboxSampleType.SelectedItem as Lemma<Guid, string>;
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleType.Id.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                AddSampleTypeComponents(conn, sampleType.Id, tnodes[0]);
            }

            cboxSampleComponent.SelectedIndex = -1;
        }

        private void AddSampleTypeComponents(SqlConnection conn, Guid sampleTypeId, TreeNode tnode)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    cboxSampleComponent.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = new Guid(tnode.Parent.Name);
                AddSampleTypeComponents(conn, parentId, tnode.Parent);
            }
        }
    }
}
