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
using System.Linq;

namespace DSA_lims
{
    public partial class FormSampleMerge : Form
    {        
        private List<Guid> mSampleIds = null;
        private string mSampleIdsCsv = String.Empty;
        private List<Sample> mSamples = new List<Sample>();
        private TreeView mSampleTypeTree = null;

        public FormSampleMerge(List<Guid> sampleIds, TreeView sampleTypeTree)
        {
            InitializeComponent();

            mSampleIds = sampleIds;
            mSampleTypeTree = sampleTypeTree;
            var sampleIdsArr = from item in mSampleIds select "'" + item + "'";
            string sampleIdsCsv = string.Join(",", sampleIdsArr);
            mSampleIdsCsv = sampleIdsCsv;            
        }

        private void FormSampleMerge_Load(object sender, EventArgs e)
        {
            HashSet<string> samplePathSet = new HashSet<string>();

            SqlConnection conn = null;
            try
            {
                UI.PopulateSampleTypes(mSampleTypeTree, cboxSampleType);

                conn = DB.OpenConnection();

                foreach(Guid sid in mSampleIds)
                {
                    Sample s = new Sample();
                    s.LoadFromDB(conn, null, sid);
                    s.Parameters.Clear();
                    foreach (Preparation p in s.Preparations)
                        p.Analyses.Clear();
                    s.Preparations.Clear();

                    mSamples.Add(s);

                    samplePathSet.Add(s.GetSampleTypePath(conn, null));
                }

                if(samplePathSet.Count == 0)
                {
                    cboxSampleType.SelectedValue = Guid.Empty;
                    cboxSampleType.Enabled = true;
                }
                else if(samplePathSet.Count == 1)
                {                    
                    SqlCommand cmd = new SqlCommand("select id from sample_type where path = @path", conn);
                    cmd.Parameters.AddWithValue("@path", samplePathSet.ElementAt(0));
                    Guid stId = (Guid)cmd.ExecuteScalar();
                    cboxSampleType.SelectedValue = stId;
                    cboxSampleType.Enabled = false;
                }
                else
                {                    
                    string[] firstItems = samplePathSet.ElementAt(0).Split(new char[] { '/' });
                    int finalLevel = firstItems.Length;

                    for (int i = 1;  i < samplePathSet.Count; i++)
                    {
                        int level = 0;
                        string[] items = samplePathSet.ElementAt(i).Split(new char[] { '/' });
                        int max = Math.Min(firstItems.Length, items.Length);
                        for(int j=0; j<items.Length && j<max; j++)
                        {
                            if (items[j] == firstItems[j])
                                level++;
                            else break;
                        }

                        if (level < finalLevel)
                            finalLevel = level;
                    }
                    
                    string finalPath = String.Join("/", firstItems, 0, finalLevel);

                    if(finalPath == String.Empty)
                    {
                        cboxSampleType.SelectedValue = Guid.Empty;
                        cboxSampleType.Enabled = true;
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("select id from sample_type where path = @path", conn);
                        cmd.Parameters.AddWithValue("@path", finalPath);
                        Guid stId = (Guid)cmd.ExecuteScalar();
                        cboxSampleType.SelectedValue = stId;
                        cboxSampleType.Enabled = false;
                    }
                }

                List<Sample> mSamplesSortedBySamplingDate = mSamples.OrderBy(x => x.SamplingDateFrom).ToList();
                mSamplesSortedBySamplingDate.RemoveAll(x => x.SamplingDateFrom == null);
                if (mSamplesSortedBySamplingDate.Count == 0)
                {
                    DateTime now = DateTime.Now;
                    dtSamplingDate.Value = now;
                    dtSamplingTime.Value = now;
                }
                else
                {
                    int mid = mSamplesSortedBySamplingDate.Count / 2;
                    DateTime dtstime = mSamplesSortedBySamplingDate[mid].SamplingDateFrom.Value;
                    dtSamplingDate.Value = dtstime;
                    dtSamplingTime.Value = dtstime;
                }

                List<Sample> mSamplesSortedByRefDate = mSamples.OrderBy(x => x.ReferenceDate).ToList();
                mSamplesSortedByRefDate.RemoveAll(x => x.ReferenceDate == null);
                if (mSamplesSortedByRefDate.Count == 0)
                {
                    DateTime now = DateTime.Now;
                    dtReferenceDate.Value = now;
                    dtReferenceTime.Value = now;
                }
                else
                {
                    int mid = mSamplesSortedByRefDate.Count / 2;
                    DateTime dtrtime = mSamplesSortedByRefDate[mid].ReferenceDate.Value;
                    dtReferenceDate.Value = dtrtime;
                    dtReferenceTime.Value = dtrtime;
                }

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
order by s.number desc", mSampleIdsCsv);

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
            if(!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                MessageBox.Show("You must select a sample type");
                return;
            }

            Dictionary<string, object> map = new Dictionary<string, object>();

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                DateTime currDate = DateTime.Now;
                
                Guid oldSampleId = Utils.MakeGuid(gridSamples.Rows[0].Cells["id"].Value);
                Sample newSample = new Sample();                
                newSample.LoadFromDB(conn, trans, oldSampleId);
                newSample.Id = Guid.NewGuid();
                foreach (Preparation p in newSample.Preparations)
                    p.Analyses.Clear();
                newSample.Preparations.Clear();
                newSample.Parameters.Clear();
                newSample.Number = DB.GetNextSampleCount(conn, trans);
                newSample.ExternalId = String.IsNullOrEmpty(tbExternalId.Text.Trim()) ? String.Empty : tbExternalId.Text.Trim();
                newSample.SampleTypeId =  Utils.MakeGuid(cboxSampleType.SelectedValue);
                if (Utils.IsValidGuid(cboxSampleComponent.SelectedValue))
                    newSample.SampleComponentId = Utils.MakeGuid(cboxSampleComponent.SelectedValue);
                else newSample.SampleComponentId = Guid.Empty;
                newSample.TransformFromId = Guid.Empty;
                newSample.MunicipalityId = Guid.Empty;
                newSample.LocationType = String.Empty;
                newSample.Location = String.Empty;
                newSample.StationId = Guid.Empty;
                newSample.SampleStorageId = Guid.Empty;
                newSample.SamplerId = Guid.Empty;
                newSample.SamplingMethodId = Guid.Empty;
                newSample.Latitude = newSample.Longitude = newSample.Altitude = null;
                newSample.InstanceStatusId = InstanceStatus.Active;
                newSample.SamplingDateFrom = newSample.SamplingDateTo = new DateTime(dtSamplingDate.Value.Year, dtSamplingDate.Value.Month, dtSamplingDate.Value.Day, dtSamplingTime.Value.Hour, dtSamplingTime.Value.Minute, dtSamplingTime.Value.Second);
                newSample.ReferenceDate = new DateTime(dtReferenceDate.Value.Year, dtReferenceDate.Value.Month, dtReferenceDate.Value.Day, dtReferenceTime.Value.Hour, dtReferenceTime.Value.Minute, dtReferenceTime.Value.Second);
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
                newSample.Comment = tbComment.Text.Trim();
                newSample.LockedId = Guid.Empty;
                newSample.CreateDate = currDate;
                newSample.CreateId = Common.UserId;
                newSample.UpdateDate = currDate;
                newSample.UpdateId = Common.UserId;

                newSample.StoreToDB(conn, trans);

                string json = JsonConvert.SerializeObject(newSample);
                DB.AddAuditMessage(conn, trans, "sample", newSample.Id, AuditOperationType.Insert, json, "");                

                SqlCommand cmd = new SqlCommand("update sample set transform_to_id = @transform_to_id where id in("+ mSampleIdsCsv + ")", conn, trans);
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

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Utils.MakeGuid(cboxSampleType.SelectedValue);
            TreeNode[] tnodes = mSampleTypeTree.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
            {
                Common.Log.Error("Unable to find sample type with id: " + sampleTypeId);
                MessageBox.Show("Error: Unable to find sample type with id: " + sampleTypeId);
                return;                
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleComponent);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
        }
    }
}
