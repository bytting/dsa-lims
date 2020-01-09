﻿/*	
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
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormSelectOrder : Form
    {
        private TreeView mTreeSampleTypes = null;
        private Sample mSample = null;
        private Assignment mAssignment = new Assignment();
        
        public Guid SelectedOrderId = Guid.Empty;
        public string SelectedOrderName = String.Empty;

        public FormSelectOrder(TreeView treeSampleTypes, Sample sample)
        {
            InitializeComponent();

            mTreeSampleTypes = treeSampleTypes;
            mSample = sample;
        }

        private void FormSelectOrder_Load(object sender, EventArgs e)
        {
            lblSampleInfo.Text = "Sample: " + mSample.Number + ", External Id: " + mSample.ExternalId;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                                                
                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxLaboratory);

                cboxLaboratory.SelectedValue = Common.LabId;
                cboxLaboratory.Enabled = false;
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
            if (!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
            {
                MessageBox.Show("You must select a laboratory first");
                return;
            }

            if (gridOrders.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an order first");
                return;
            }

            if(!mAssignment.ApprovedLaboratory)
            {
                MessageBox.Show("Can not add samples to orders that is not approved by laboratory");
                return;
            }

            if (!mAssignment.ApprovedCustomer)
            {
                MessageBox.Show("Can not add samples to orders that is not approved by customer");
                return;
            }

            if (mAssignment.WorkflowStatusId != WorkflowStatus.Construction)
            {
                MessageBox.Show("Can not add samples to orders that is not under construction");
                return;
            }            

            if (treeOrderLines.SelectedNode == null)
            {
                MessageBox.Show("You must select a order line first");
                return;
            }

            TreeNode tnode = treeOrderLines.SelectedNode;

            if (tnode.Level != 0)
            {
                MessageBox.Show("You must select a top level order line for this sample");
                return;
            }            

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                Guid astId = Guid.Parse(tnode.Name);
                AssignmentSampleType ast = mAssignment.SampleTypes.Find(x => x.Id == astId);
                if(ast == null)
                    throw new Exception("No assignment sample type found with id " + ast.Id.ToString());

                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                AstInfo astInfo = tnode.Tag as AstInfo;
                int nSamplesFree = DB.GetAvailableSamplesOnAssignmentSampleType(conn, trans, astId);
                if(nSamplesFree <= 0)
                {
                    MessageBox.Show("This order line is full");
                    return;
                }

                string[] items = tnode.Text.Split(new[] { " -> " }, StringSplitOptions.RemoveEmptyEntries);
                if(items.Length < 2)                
                    throw new Exception("Invalid sample type found in assignment: " + tnode.Text);
                
                string st1 = items[1];
                string st2 = mSample.GetSampleTypePath(conn, trans);
                if(!st2.StartsWith(st1))
                {
                    MessageBox.Show("Wrong sample type for sample " + mSample.Number);
                    return;
                }                

                if (astInfo.SampleComponentId != Guid.Empty && astInfo.SampleComponentId != mSample.SampleComponentId)
                {
                    MessageBox.Show("Wrong sample component for sample " + mSample.Number);
                    return;
                }

                Guid labId = Utils.MakeGuid(cboxLaboratory.SelectedValue);

                SelectedOrderId = mAssignment.Id;
                SelectedOrderName = mAssignment.Name;

                foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                {
                    if (apm.PreparationLaboratoryId != mAssignment.LaboratoryId)
                    {
                        // Check that external preparations exists
                        TreeNode[] tn = treeOrderLines.Nodes.Find(apm.Id.ToString(), true);
                        if (tn.Length < 1)
                            throw new Exception("No assignment preparation methods found with id " + apm.Id.ToString());

                        if (tn[0].Tag == null)
                        {
                            MessageBox.Show("You must specify external preparations for this order line");
                            return;
                        }
                    }
                }                

                if (mSample.HasOrder(conn, trans, SelectedOrderId))
                {
                    MessageBox.Show("Sample " + mSample.Number + " is already added to order " + mAssignment.Name);
                    return;
                }

                int nextPrepNum = DB.GetNextPreparationNumber(conn, trans, mSample.Id);

                foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                {
                    List<Guid> exIds = null;
                    if (apm.PreparationLaboratoryId != mAssignment.LaboratoryId)
                    {
                        // External preparations
                        TreeNode[] tn = treeOrderLines.Nodes.Find(apm.Id.ToString(), true);
                        if (tn.Length < 1)
                            throw new Exception("No assignment preparation method node found in tree with id " + apm.Id.ToString());

                        exIds = tn[0].Tag as List<Guid>;
                    }

                    for (int i = 0; i < apm.PreparationMethodCount; i++)
                    {
                        Preparation p = GetNextPreparation(apm, labId, ref exIds, ref nextPrepNum);

                        int nextAnalNum = DB.GetNextAnalysisNumber(conn, trans, p.Id);
                        foreach (AssignmentAnalysisMethod aam in apm.AnalysisMethods)
                        {
                            for (int j = 0; j < aam.AnalysisMethodCount; j++)
                            {
                                Analysis a = new Analysis();
                                a.PreparationId = p.Id;
                                a.AnalysisMethodId = aam.AnalysisMethodId;
                                a.InstanceStatusId = InstanceStatus.Active;
                                a.WorkflowStatusId = WorkflowStatus.Construction;
                                a.Number = nextAnalNum++;
                                a.AssignmentId = SelectedOrderId;
                                a.LaboratoryId = labId;
                                p.Analyses.Add(a);
                            }
                        }
                    }
                }

                mSample.ConnectToOrderLine(conn, trans, ast.Id);

                mSample.StoreToDB(conn, trans);

                string json = JsonConvert.SerializeObject(mSample);
                DB.AddAuditMessage(conn, trans, "sample", mSample.Id, AuditOperationType.Update, json, "");

                trans.Commit();
                DialogResult = DialogResult.OK;
            }
            catch(Exception ex)
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

        private Preparation GetNextPreparation(AssignmentPreparationMethod apm, Guid labId, ref List<Guid> exIds, ref int nextPrepNum)
        {
            Preparation p = null;

            if (exIds == null)
            {
                p = new Preparation();
                p.AssignmentId = SelectedOrderId;
                p.InstanceStatusId = InstanceStatus.Active;
                p.LaboratoryId = labId;
                p.Number = nextPrepNum++;
                p.PreparationMethodId = apm.PreparationMethodId;
                p.SampleId = mSample.Id;
                p.WorkflowStatusId = WorkflowStatus.Construction;
                mSample.Preparations.Add(p);
            }
            else
            {
                if (exIds.Count < 1)
                    throw new Exception("Missing external preparation ids in list");
                Guid pid = exIds[0];
                exIds.RemoveAt(0);
                p = mSample.Preparations.Find(x => x.Id == pid);
                if(p == null)
                    throw new Exception("External preparation with id " + pid + " was not found on sample " + mSample.Number);
            }

            return p;
        }

        private void cboxLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridOrders.DataSource = null;
            treeOrderLines.Nodes.Clear();

            if (!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
                return;
            
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                Guid labId = Utils.MakeGuid(cboxLaboratory.SelectedValue);
                UI.PopulateOrdersConstruction(conn, labId, gridOrders);
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

        private void gridOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (gridOrders.SelectedRows.Count < 1)
                return;
            
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                Guid oid = Guid.Parse(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
                mAssignment.LoadFromDB(conn, null, oid);

                List<Guid> stIds = mAssignment.GetSampleTypeIdsForSampleType(conn, null, mSample.GetSampleTypePath(conn, null));

                treeOrderLines.Nodes.Clear();
                foreach(AssignmentSampleType ast in mAssignment.SampleTypes)
                {
                    if (!stIds.Exists(x => x == ast.SampleTypeId))
                        continue;

                    string txt = ast.SampleCount + ", " + ast.SampleTypeName(conn, null);
                    if (ast.SampleComponentId != Guid.Empty)
                        txt += ", " + ast.SampleComponentName(conn, null);
                    txt += " -> " + ast.SampleTypePath(conn, null);
                    TreeNode astNode = treeOrderLines.Nodes.Add(ast.Id.ToString(), txt);
                    astNode.ToolTipText = ast.Comment;
                    astNode.Tag = new AstInfo(ast.SampleTypeId, ast.SampleComponentId);
                    foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                    {
                        TreeNode apmNode = astNode.Nodes.Add(apm.Id.ToString(), apm.PreparationMethodCount + ", " + apm.PreparationMethodName(conn, null));
                        apmNode.ToolTipText = apm.Comment;
                        foreach (AssignmentAnalysisMethod aam in apm.AnalysisMethods)
                        {
                            TreeNode aamNode = apmNode.Nodes.Add(aam.Id.ToString(), aam.AnalysisMethodCount + ", " + aam.AnalysisMethodName(conn, null));
                            aamNode.ToolTipText = aam.Comment;
                        }
                    }
                }

                treeOrderLines.ExpandAll();
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

        private void btnExistingPreps_Click(object sender, EventArgs e)
        {
            // select existing preparations
            if (treeOrderLines.SelectedNode == null)
                return;

            TreeNode apmNode = treeOrderLines.SelectedNode;

            if (apmNode.Level != 1)
                return;

            Guid astId = Guid.Parse(apmNode.Parent.Name);
            Guid apmId = Guid.Parse(apmNode.Name);

            // FIXME: Sanity checks
            AssignmentSampleType ast = mAssignment.SampleTypes.Find(x => x.Id == astId);            
            AssignmentPreparationMethod apm = ast.PreparationMethods.Find(x => x.Id == apmId);

            if(apm.PreparationLaboratoryId == mAssignment.LaboratoryId)
            {
                MessageBox.Show("These preparation methods are not registered as external");
                return;
            }

            FormSelectExistingPreps form = new FormSelectExistingPreps(apm.PreparationLaboratoryId, mSample);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (form.SelectedPreparationIds.Count == 0)
            {
                apmNode.Tag = null;
                if (apmNode.Text.EndsWith(" ..."))
                    apmNode.Text = apmNode.Text.Substring(0, apmNode.Text.Length - 4);
                return;
            }
            
            List<Guid> exIds = new List<Guid>(form.SelectedPreparationIds);
            if(apm.PreparationMethodCount != exIds.Count)
            {
                MessageBox.Show("Wrong number of external preparations");
                return;
            }

            apmNode.Tag = exIds;
            
            if (!apmNode.Text.EndsWith(" ..."))
                apmNode.Text = apmNode.Text + " ...";

            UpdateCurrentPreparations(apmNode);
        }

        private void treeOrderLines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateCurrentPreparations(e.Node);

            btnExistingPreps.Enabled = e.Node.Level == 1;
        }

        private void UpdateCurrentPreparations(TreeNode tnode)
        {            
            if (tnode.Level != 1 || tnode.Tag == null)
                return;

            tnode.ToolTipText = "";

            try
            {
                List<Guid> prepList = tnode.Tag as List<Guid>;
                string query = "select number from preparation where id in ("
                    + String.Join(",", prepList.Select(x => "'" + x.ToString() + "'").ToArray()) + ")";

                List<object> prepNums = new List<object>();
                using (SqlConnection conn = DB.OpenConnection())
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))
                        while (reader.Read())
                            prepNums.Add(mSample.Number + "/" + reader.GetString("number"));
                }

                tnode.ToolTipText = "Connected preparations: " + String.Join(", ", prepNums);
            }
            catch {}
        }        
    }

    public class AstInfo
    {
        public AstInfo() {}

        public AstInfo(Guid sampleTypeId, Guid sampleComponentId)
        {
            SampleTypeId = sampleTypeId;
            SampleComponentId = sampleComponentId;
        }

        public Guid SampleTypeId;
        public Guid SampleComponentId;
    }
}
