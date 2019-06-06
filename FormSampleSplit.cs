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
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormSampleSplit : Form
    {        
        private TreeView mTreeSampleTypes = null;
        private Guid mOldSampleId = Guid.Empty;
        private Sample mNewSample = null;

        public FormSampleSplit(Guid sampleId, TreeView treeSampleTypes)
        {
            InitializeComponent();

            mTreeSampleTypes = treeSampleTypes;
            mOldSampleId = sampleId;
        }

        private void FormSampleSplit_Load(object sender, EventArgs e)
        {
            tbCount.KeyPress += CustomEvents.Integer_KeyPress;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                mNewSample = new Sample();
                mNewSample.LoadFromDB(conn, null, mOldSampleId);                
                foreach (Preparation p in mNewSample.Preparations)                
                    p.Analyses.Clear();
                mNewSample.Preparations.Clear();
                mNewSample.Parameters.Clear();
                mNewSample.LodWeightStart = null;
                mNewSample.LodWeightEnd = null;
                mNewSample.LodTemperature = null;
                mNewSample.LodWaterPercent = null;
                mNewSample.LodFactor = null;
                mNewSample.LodWeightStartAsh = null;
                mNewSample.LodWeightEndAsh = null;
                mNewSample.LodTemperatureAsh = null;
                mNewSample.LodWaterPercentAsh = null;
                mNewSample.LodFactorAsh = null;
                mNewSample.LodWeightStartAsh2 = null;
                mNewSample.LodWeightEndAsh2 = null;
                mNewSample.LodTemperatureAsh2 = null;
                mNewSample.LodWaterPercentAsh2 = null;
                mNewSample.LodFactorAsh2 = null;
                mNewSample.TransformToId = Guid.Empty;
                mNewSample.InstanceStatusId = InstanceStatus.Active;

                tbSampleNumber.Text = mNewSample.Number.ToString();                
                tbSampleType.Text = mNewSample.GetSampleTypeName(conn, null);

                TreeNode[] tnodes = mTreeSampleTypes.Nodes.Find(mNewSample.SampleTypeId.ToString(), true);
                if (tnodes.Length < 1)                
                    throw new Exception("Unable to find sample type id " + mNewSample.SampleTypeId);

                UI.PopulateSampleComponentsAscending(conn, mNewSample.SampleTypeId, tnodes[0], cboxComponents);
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

            Guid compId = Utils.MakeGuid(cboxComponents.SelectedValue);
            int count = 0;
            try
            {
                count = Convert.ToInt32(tbCount.Text.Trim());
            }
            catch
            {
                MessageBox.Show("Invalid number format");
                return;
            }

            if (count < 1 || count > 10000)
            {
                MessageBox.Show("Split count must be between 1 and 10000");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                DateTime currDate = DateTime.Now;

                for (int i = 0; i < count; i++)
                {
                    mNewSample.Id = Guid.NewGuid();
                    mNewSample.Number = DB.GetNextSampleCount(conn, trans);
                    mNewSample.SampleComponentId = compId;
                    mNewSample.TransformFromId = mOldSampleId;                    
                    mNewSample.CreateDate = currDate;
                    mNewSample.CreateId = Common.UserId;
                    mNewSample.UpdateDate = currDate;
                    mNewSample.UpdateId = Common.UserId;                    

                    mNewSample.StoreToDB(conn, trans);

                    string json = JsonConvert.SerializeObject(mNewSample);
                    DB.AddAuditMessage(conn, trans, "sample", mNewSample.Id, AuditOperationType.Insert, json, "");
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
