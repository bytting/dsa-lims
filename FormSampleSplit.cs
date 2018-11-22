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
    public partial class FormSampleSplit : Form
    {
        private Dictionary<string, object> map = new Dictionary<string, object>();

        public FormSampleSplit(Guid sampleId, TreeView treeSampleTypes)
        {
            InitializeComponent();

            map["id"] = sampleId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                string query = "select * from sample s inner join sample_type st on s.sample_type_id = st.id where s.id = @id";                

                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new SqlParameter("@id", map["id"])))
                {
                    reader.Read();

                    map["number"] = reader["number"];
                    map["laboratory_id"] = reader["laboratory_id"];
                    map["sample_type_id"] = reader["sample_type_id"];                    
                    map["project_sub_id"] = reader["project_sub_id"];
                    map["station_id"] = reader["station_id"];
                    map["sampler_id"] = reader["sampler_id"];
                    map["sampling_method_id"] = reader["sampling_method_id"];
                    map["municipality_id"] = reader["municipality_id"];
                    map["location_type"] = reader["location_type"];
                    map["location"] = reader["location"];
                    map["latitude"] = reader["latitude"];
                    map["longitude"] = reader["longitude"];
                    map["altitude"] = reader["altitude"];
                    map["sampling_date_from"] = reader["sampling_date_from"];
                    map["use_sampling_date_to"] = reader["use_sampling_date_to"];
                    map["sampling_date_to"] = reader["sampling_date_to"];
                    map["reference_date"] = reader["reference_date"];
                    map["external_id"] = reader["external_id"];
                    map["confidential"] = reader["confidential"];
                }

                tbSampleNumber.Text = map["number"].ToString();

                string sampleTypeName = DB.GetScalar(conn, "select name from sample_type where id = @id", CommandType.Text, new SqlParameter("@id", map["sample_type_id"])).ToString();
                tbSampleType.Text = sampleTypeName;
                
                TreeNode[] tnodes = treeSampleTypes.Nodes.Find(map["sample_type_id"].ToString(), true);
                if (tnodes.Length < 1)
                {
                    throw new Exception("Unable to find sample type id " + map["sample_type_id"].ToString());
                }

                UI.PopulateSampleComponentsAscending(conn, Guid.Parse(map["sample_type_id"].ToString()), tnodes[0], cboxComponents);
            }
        }

        private void FormSampleSplit_Load(object sender, EventArgs e)
        {
            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxComponents.SelectedValue))
            {
                MessageBox.Show("Sample component is required");
                return;
            }

            if (String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Count is required");
                return;
            }

            Guid compId = Guid.Parse(cboxComponents.SelectedValue.ToString());
            int count = Convert.ToInt32(tbCount.Text.Trim());

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_sample", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < count; i++)
                {
                    int nextSampleCount = DB.GetNextSampleCount(conn, trans);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@number", nextSampleCount);
                    cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), map["laboratory_id"]));
                    cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), map["sample_type_id"]));
                    cmd.Parameters.AddWithValue("@sample_storage_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sample_component_id", DB.MakeParam(typeof(Guid), compId));
                    cmd.Parameters.AddWithValue("@project_sub_id", DB.MakeParam(typeof(Guid), map["project_sub_id"]));
                    cmd.Parameters.AddWithValue("@station_id", DB.MakeParam(typeof(Guid), map["station_id"]));
                    cmd.Parameters.AddWithValue("@sampler_id", DB.MakeParam(typeof(Guid), map["sampler_id"]));
                    cmd.Parameters.AddWithValue("@sampling_method_id", DB.MakeParam(typeof(Guid), map["sampling_method_id"]));
                    cmd.Parameters.AddWithValue("@transform_from_id", DB.MakeParam(typeof(Guid), map["id"]));
                    cmd.Parameters.AddWithValue("@transform_to_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@imported_from", DBNull.Value);
                    cmd.Parameters.AddWithValue("@imported_from_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@municipality_id", DB.MakeParam(typeof(Guid), map["municipality_id"]));
                    cmd.Parameters.AddWithValue("@location_type", map["location_type"]);
                    cmd.Parameters.AddWithValue("@location", map["location"]);
                    cmd.Parameters.AddWithValue("@latitude", map["latitude"]);
                    cmd.Parameters.AddWithValue("@longitude", map["longitude"]);
                    cmd.Parameters.AddWithValue("@altitude", map["altitude"]);
                    cmd.Parameters.AddWithValue("@sampling_date_from", map["sampling_date_from"]);
                    cmd.Parameters.AddWithValue("@use_sampling_date_to", map["use_sampling_date_to"]);
                    cmd.Parameters.AddWithValue("@sampling_date_to", map["sampling_date_to"]);
                    cmd.Parameters.AddWithValue("@reference_date", map["reference_date"]);
                    cmd.Parameters.AddWithValue("@external_id", map["external_id"]);
                    cmd.Parameters.AddWithValue("@wet_weight_g", DBNull.Value);
                    cmd.Parameters.AddWithValue("@dry_weight_g", DBNull.Value);
                    cmd.Parameters.AddWithValue("@volume_l", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_weight_start", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_weight_end", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_temperature", DBNull.Value);
                    cmd.Parameters.AddWithValue("@confidential", map["confidential"]);
                    cmd.Parameters.AddWithValue("@parameters", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@locked_by", DBNull.Value);
                    cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();                
                }

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
