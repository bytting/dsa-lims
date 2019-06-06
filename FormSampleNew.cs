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
        }

        private void FormSampleNew_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateProjectMain(conn, null, Common.UserId, InstanceStatus.Active, cboxProjectMain);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxLaboratory);

                if (Utils.IsValidGuid(Common.LabId))
                {
                    cboxLaboratory.SelectedValue = Common.LabId;
                }
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

                DateTime currDate = DateTime.Now;

                Sample sample = new Sample();
                sample.Number = DB.GetNextSampleCount(conn, trans);
                sample.LaboratoryId = Utils.MakeGuid(cboxLaboratory.SelectedValue);                
                sample.SampleTypeId = Utils.MakeGuid(cboxSampleType.SelectedValue);
                if (Utils.IsValidGuid(cboxSampleComponent.SelectedValue))
                    sample.SampleComponentId = Utils.MakeGuid(cboxSampleComponent.SelectedValue);                
                sample.ProjectSubId = Utils.MakeGuid(cboxProjectSub.SelectedValue);
                sample.InstanceStatusId = InstanceStatus.Active;
                sample.CreateDate = currDate;
                sample.CreateId = Common.UserId;
                sample.UpdateDate = currDate;
                sample.UpdateId = Common.UserId;                

                sample.StoreToDB(conn, trans);

                string json = JsonConvert.SerializeObject(sample);
                DB.AddAuditMessage(conn, trans, "sample", sample.Id, AuditOperationType.Insert, json, "");

                trans.Commit();

                SampleId = sample.Id;
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

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateProjectSub(conn, null, projectMainId, Common.UserId, InstanceStatus.Active, cboxProjectSub);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
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
            }
            finally
            {
                conn?.Close();
            }
        }        
    }
}
