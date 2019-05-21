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
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormSelectOrder : Form
    {
        private TreeView TreeSampleTypes = null;
        private Sample mSample = null;

        public Guid SelectedLaboratoryId = Guid.Empty;
        public Guid SelectedOrderId = Guid.Empty;
        public string SelectedOrderName = String.Empty;
        public Guid SelectedOrderLineId = Guid.Empty;

        public FormSelectOrder(TreeView treeSampleTypes, Sample s)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;
            mSample = s;
        }

        private void FormSelectOrder_Load(object sender, EventArgs e)
        {
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
                SelectedLaboratoryId = Utils.MakeGuid(cboxLaboratory.SelectedValue);
                SelectedOrderId = Utils.MakeGuid(gridOrders.SelectedRows[0].Cells["id"].Value);
                SelectedOrderName = gridOrders.SelectedRows[0].Cells["name"].Value.ToString();
                SelectedOrderLineId = Guid.Parse(tnode.Name);                

                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                Assignment ass = new Assignment();
                ass.LoadFromDB(conn, trans, SelectedOrderId);

                if (!ass.ApprovedLaboratory || !ass.ApprovedCustomer)
                {
                    MessageBox.Show("You can not add samples to this order. The order is not approved");
                    return;
                }

                int nAvail = DB.GetAvailableSamplesOnAssignmentSampleType(conn, trans, SelectedOrderLineId);
                if (nAvail == 0)
                {
                    MessageBox.Show("This order line if already full");
                    return;
                }

                string query = @"
select count(*) 
from sample_x_assignment_sample_type sxast 
    inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id
    inner join assignment a on a.id = ast.assignment_id and a.id = @aid
where sxast.sample_id = @sid";
                object o = DB.GetScalar(conn, trans, query, CommandType.Text, new[] {
                    new SqlParameter("@sid", mSample.Id),
                    new SqlParameter("@aid", SelectedOrderId)
                });

                if (o != null && o != DBNull.Value)
                {
                    int cnt = Convert.ToInt32(o);
                    if (cnt > 0)
                    {
                        MessageBox.Show("This sample is already added to this order");
                        return;
                    }
                }

                foreach (TreeNode tn in tnode.Nodes)
                {
                    Guid prepMethLineId = Guid.Parse(tn.Name);

                    o = DB.GetScalar(conn, trans, "select preparation_laboratory_id from assignment_preparation_method where id = @id", CommandType.Text,
                        new SqlParameter("@id", prepMethLineId));
                    if (!Utils.IsValidGuid(o))
                    {
                        Common.Log.Error("Invalid or non existing Guid on assignment preparation method");
                        MessageBox.Show("Invalid or non existing Guid on assignment preparation method");
                        return;
                    }

                    Guid prepLabId = Guid.Parse(o.ToString());
                    if (prepLabId != SelectedLaboratoryId)
                    {
                        if (tn.Tag == null)
                        {
                            MessageBox.Show("You must select external preparations");
                            return;
                        }
                        else
                        {
                            o = DB.GetScalar(conn, trans, "select preparation_method_count from assignment_preparation_method where id = @id", CommandType.Text,
                                new SqlParameter("@id", prepMethLineId));
                            int cnt = Convert.ToInt32(o);
                            List<Guid> prepList = tn.Tag as List<Guid>;
                            if (prepList.Count != cnt)
                            {
                                MessageBox.Show("Wrong number of external preparations");
                                return;
                            }
                        }
                    }
                }

                GenerateOrderPreparations(conn, trans, mSample.Id, SelectedLaboratoryId, SelectedOrderId, SelectedOrderLineId, tnode.Nodes);

                mSample.LoadFromDB(conn, trans, mSample.Id);

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

        private void GenerateOrderPreparations(SqlConnection conn, SqlTransaction trans, Guid sampleId, Guid labId, Guid orderId, Guid orderLineId, TreeNodeCollection tnodes)
        {                                        
            int nextPrepNumber = DB.GetNextPreparationNumber(conn, trans, sampleId);

            foreach (TreeNode tnode in tnodes)
            {
                Guid prepMethLineId = Guid.Parse(tnode.Name);

                if (tnode.Tag != null)
                {
                    List<Guid> prepList = tnode.Tag as List<Guid>;
                    foreach (Guid pid in prepList)                        
                        GenerateOrderAnalyses(conn, trans, orderId, labId, pid, tnode.Nodes);
                }
                else
                {
                    Guid prepMethId = Guid.Empty;
                    int prepCount = 0;

                    string query = "select preparation_method_id, preparation_method_count from assignment_preparation_method where id = @id";
                    using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@id", prepMethLineId)))
                    {
                        reader.Read(); // FIXME

                        prepMethId = reader.GetGuid("preparation_method_id");
                        prepCount = reader.GetInt32("preparation_method_count");
                    }

                    SqlCommand cmd = new SqlCommand("csp_insert_preparation", conn, trans);
                    cmd.CommandType = CommandType.StoredProcedure;
                    while (prepCount > 0)
                    {
                        Guid newPrepId = Guid.NewGuid();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", newPrepId);
                        cmd.Parameters.AddWithValue("@sample_id", sampleId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@number", nextPrepNumber++);
                        cmd.Parameters.AddWithValue("@assignment_id", orderId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@laboratory_id", labId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@preparation_geometry_id", DBNull.Value);
                        cmd.Parameters.AddWithValue("@preparation_method_id", prepMethId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatus.Construction);
                        cmd.Parameters.AddWithValue("@amount", DBNull.Value);
                        cmd.Parameters.AddWithValue("@prep_unit_id", 0);
                        cmd.Parameters.AddWithValue("@quantity", DBNull.Value);
                        cmd.Parameters.AddWithValue("@quantity_unit_id", 0);
                        cmd.Parameters.AddWithValue("@fill_height_mm", DBNull.Value);
                        cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                        cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                        cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                        cmd.ExecuteNonQuery();

                        GenerateOrderAnalyses(conn, trans, orderId, labId, newPrepId, tnode.Nodes);

                        prepCount--;
                    }                        
                }
            }

            SqlCommand cmd2 = new SqlCommand("insert into sample_x_assignment_sample_type values(@sample_id, @assignment_sample_type_id)", conn, trans);
            cmd2.Parameters.AddWithValue("@sample_id", sampleId, Guid.Empty);
            cmd2.Parameters.AddWithValue("@assignment_sample_type_id", orderLineId, Guid.Empty);
            cmd2.ExecuteNonQuery();            
        }

        private void GenerateOrderAnalyses(SqlConnection conn, SqlTransaction trans, Guid orderId, Guid labId, Guid prepId, TreeNodeCollection tnodes)
        {
            Guid analMethId = Guid.Empty;
            int analCount = 0;
            int nextAnalNumber = DB.GetNextAnalysisNumber(conn, trans, prepId);

            foreach (TreeNode tnode in tnodes)
            {
                Guid analMethLineId = Guid.Parse(tnode.Name);

                string query = "select analysis_method_id, analysis_method_count from assignment_analysis_method where id = @id";
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@id", analMethLineId)))
                {
                    reader.Read(); // FIXME

                    analMethId = reader.GetGuid("analysis_method_id");
                    analCount = reader.GetInt32("analysis_method_count");
                }

                SqlCommand cmd = new SqlCommand("csp_insert_analysis", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;

                while (analCount > 0)
                {                    
                    Guid newAnalId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newAnalId);
                    cmd.Parameters.AddWithValue("@number", nextAnalNumber++);
                    cmd.Parameters.AddWithValue("@assignment_id", orderId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@laboratory_id", labId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@preparation_id", prepId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@analysis_method_id", analMethId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatus.Construction);
                    cmd.Parameters.AddWithValue("@specter_reference", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sigma_act", 0);
                    cmd.Parameters.AddWithValue("@sigma_mda", 0);
                    cmd.Parameters.AddWithValue("@nuclide_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@mda_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@create_id", Common.UserId);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId);
                                        
                    cmd.ExecuteNonQuery();

                    analCount--;
                }
            }
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
                UI.PopulateOrderContentForSampleTypeName(conn, oid, treeOrderLines, mSample.SampleTypeId, TreeSampleTypes, true);
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

        private void btnExistingPreps_Click(object sender, EventArgs e)
        {
            // select existing preparations
            if (treeOrderLines.SelectedNode == null)
                return;

            TreeNode tnode = treeOrderLines.SelectedNode;

            if (tnode.Level != 1)
                return;

            Guid prepLabId = Guid.Empty;
            Guid apmId = Guid.Parse(tnode.Name);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                object o = DB.GetScalar(conn, null, "select preparation_laboratory_id from assignment_preparation_method where id = @id", CommandType.Text, 
                    new SqlParameter("@id", apmId));
                if(!DB.IsValidField(o))
                {
                    MessageBox.Show("These preparations are not marked as external");
                    return;
                }
                prepLabId = Guid.Parse(o.ToString());
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

            FormSelectExistingPreps form = new FormSelectExistingPreps(prepLabId, mSample.Id);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (form.SelectedPreparationIds.Count == 0)
                tnode.Tag = null;
            else            
                tnode.Tag = new List<Guid>(form.SelectedPreparationIds);

            if(tnode.Tag == null)
            {
                if (tnode.Text.EndsWith(" ..."))
                    tnode.Text = tnode.Text.Substring(0, tnode.Text.Length - 4);
            }
            else
            {
                if (!tnode.Text.EndsWith(" ..."))
                    tnode.Text = tnode.Text + " ...";
            }

            UpdateCurrentPreparations(tnode);
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

            List<Guid> prepList = tnode.Tag as List<Guid>;
            string query = "select number from preparation where id in (" 
                + String.Join(",", prepList.Select(x => "'" + x.ToString() + "'").ToArray()) + ")";

            List<object> prepNums = new List<object>();
            using (SqlConnection conn = DB.OpenConnection())            
                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))            
                    while (reader.Read())
                        prepNums.Add(mSample.Number + "/" + reader.GetString("number"));

            tnode.ToolTipText = "Connected preparations: " + String.Join(", ", prepNums);
        }        
    }
}
