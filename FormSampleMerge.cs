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
    public partial class FormSampleMerge : Form
    {
        private string SampleIdsCsv = String.Empty;

        public FormSampleMerge(string sampleIdsCsv)
        {
            InitializeComponent();

            SampleIdsCsv = sampleIdsCsv;            
        }

        private void FormSampleMerge_Load(object sender, EventArgs e)
        {            
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                string query = String.Format(@"
select 
    s.id,     
    s.number as 'sample_number', 
    s.external_id,
    st.name as 'sample_type_name',
    sc.name as 'sample_component_name'
from sample s
    inner join sample_type st on st.id = s.sample_type_id
    left outer join sample_component sc on sc.id = s.sample_component_id
where s.id in({0}) 
order by s.number desc", 
SampleIdsCsv);
                gridSamples.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text);                
                gridSamples.Columns["id"].Visible = false;
                gridSamples.Columns["sample_number"].HeaderText = "Sample number";
                gridSamples.Columns["external_id"].HeaderText = "External Id";
                gridSamples.Columns["sample_type_name"].HeaderText = "Sample type";
                gridSamples.Columns["sample_component_name"].HeaderText = "Sample component";
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
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                DateTime currDate = DateTime.Now;

                // FIXME: Using first sample in list as a template
                Guid oldSampleId = Utils.MakeGuid(gridSamples.Rows[0].Cells["id"].Value);
                Sample newSample = new Sample();                
                newSample.LoadFromDB(conn, trans, oldSampleId);
                newSample.Id = Guid.NewGuid();
                foreach (Preparation p in newSample.Preparations)
                    p.Analyses.Clear();
                newSample.Preparations.Clear();
                newSample.Parameters.Clear();
                newSample.Number = DB.GetNextSampleCount(conn, trans);
                newSample.ExternalId = String.Empty;
                newSample.SampleComponentId = Guid.Empty;
                newSample.TransformFromId = Guid.Empty;
                newSample.InstanceStatusId = InstanceStatus.Active;
                newSample.WetWeight_g = null;
                newSample.DryWeight_g = null;
                newSample.LodWeightStart = null;
                newSample.LodWeightEnd = null;
                newSample.LodTemperature = null;
                newSample.LodWaterPercent = null;
                newSample.LodFactor = null;
                newSample.LodWeightStartAsh = null;
                newSample.LodWeightEndAsh = null;
                newSample.LodTemperatureAsh = null;
                newSample.LodWaterPercentAsh = null;
                newSample.LodFactorAsh = null;
                newSample.LodWeightStartAsh2 = null;
                newSample.LodWeightEndAsh2 = null;
                newSample.LodTemperatureAsh2 = null;
                newSample.LodWaterPercentAsh2 = null;
                newSample.LodFactorAsh2 = null;
                newSample.Comment = String.Empty;
                newSample.CreateDate = currDate;
                newSample.CreateId = Common.UserId;
                newSample.UpdateDate = currDate;
                newSample.UpdateId = Common.UserId;

                newSample.StoreToDB(conn, trans);

                string json = JsonConvert.SerializeObject(newSample);
                DB.AddAuditMessage(conn, trans, "sample", newSample.Id, AuditOperationType.Insert, json, "");

                SqlCommand cmd = new SqlCommand("update sample set transform_to_id = @transform_to_id where id in("+ SampleIdsCsv + ")", conn, trans);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@transform_to_id", newSample.Id);
                cmd.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
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
