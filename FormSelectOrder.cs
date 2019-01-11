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
    public partial class FormSelectOrder : Form
    {
        private TreeView TreeSampleTypes = null;
        private Guid SampleId = Guid.Empty;
        private int SampleNumber = 0;
        public int SelectedSampleNumber { get { return SampleNumber; } }
        private Guid SampleTypeId = Guid.Empty;

        public Guid SelectedLaboratoryId = Guid.Empty;
        public Guid SelectedOrderId = Guid.Empty;
        public string SelectedOrderName = String.Empty;
        public Guid SelectedOrderLineId = Guid.Empty;

        public FormSelectOrder(TreeView treeSampleTypes, Guid sampId)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;
            SampleId = sampId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SampleNumber = DB.GetSampleNumber(conn, SampleId);     
                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxLaboratory);

                if (Utils.IsValidGuid(Common.LabId))
                {
                    cboxLaboratory.SelectedValue = Common.LabId;
                }

                object o = DB.GetScalar(conn, "select sample_type_id from sample where id = @id", CommandType.Text, new SqlParameter("@id", SampleId));
                if (o != null && o != DBNull.Value)
                    SampleTypeId = Guid.Parse(o.ToString());
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
                MessageBox.Show("You must select a order first");
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

            SelectedLaboratoryId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());
            SelectedOrderId = Guid.Parse(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            SelectedOrderName = gridOrders.SelectedRows[0].Cells["name"].Value.ToString();
            SelectedOrderLineId = Guid.Parse(tnode.Name);

            string query = @"
select count(*) 
from sample_x_assignment_sample_type sxast 
    inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id
    inner join assignment a on a.id = ast.assignment_id and a.id = @aid
where sxast.sample_id = @sid";
            object o = null;
            using (SqlConnection conn = DB.OpenConnection())
            {
                int nAvail = DB.GetAvailableSamplesOnAssignmentSampleType(conn, SelectedOrderLineId);
                if(nAvail == 0)
                {
                    MessageBox.Show("This order line if already full");
                    return;
                }

                o = DB.GetScalar(conn, query, CommandType.Text, new[] {
                    new SqlParameter("@sid", SampleId),
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

                    o = DB.GetScalar(conn, "select preparation_laboratory_id from assignment_preparation_method where id = @id", CommandType.Text,
                        new SqlParameter("@id", prepMethLineId));
                    if (o != null && o != DBNull.Value)
                    {
                        if (tn.Tag == null)
                        {
                            MessageBox.Show("You must select external preparations");
                            return;
                        }
                        else
                        {
                            o = DB.GetScalar(conn, "select preparation_method_count from assignment_preparation_method where id = @id", CommandType.Text,
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
            }
         
            GenerateOrderPreparations(SampleId, SelectedLaboratoryId, SelectedOrderId, SelectedOrderLineId, tnode.Nodes);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void GenerateOrderPreparations(Guid sampleId, Guid labId, Guid orderId, Guid orderLineId, TreeNodeCollection tnodes)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                int nextPrepNumber = DB.GetNextPreparationNumber(connection, transaction, sampleId);

                foreach (TreeNode tnode in tnodes)
                {
                    Guid prepMethLineId = Guid.Parse(tnode.Name);

                    if (tnode.Tag != null)
                    {
                        List<Guid> prepList = tnode.Tag as List<Guid>;
                        foreach (Guid pid in prepList)                        
                            GenerateOrderAnalyses(connection, transaction, orderId, labId, pid, tnode.Nodes);
                    }
                    else
                    {
                        Guid prepMethId = Guid.Empty;
                        int prepCount = 0;

                        string query = "select preparation_method_id, preparation_method_count from assignment_preparation_method where id = @id";
                        using (SqlDataReader reader = DB.GetDataReader(connection, transaction, query, CommandType.Text, new SqlParameter("@id", prepMethLineId)))
                        {
                            reader.Read(); // FIXME
                            prepMethId = Guid.Parse(reader["preparation_method_id"].ToString());
                            prepCount = Convert.ToInt32(reader["preparation_method_count"]);
                        }

                        SqlCommand cmd = new SqlCommand("csp_insert_preparation", connection, transaction);
                        cmd.CommandType = CommandType.StoredProcedure;
                        while (prepCount > 0)
                        {
                            Guid newPrepId = Guid.NewGuid();
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", newPrepId);
                            cmd.Parameters.AddWithValue("@sample_id", DB.MakeParam(typeof(Guid), sampleId));
                            cmd.Parameters.AddWithValue("@number", nextPrepNumber++);
                            cmd.Parameters.AddWithValue("@assignment_id", DB.MakeParam(typeof(Guid), orderId));
                            cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), labId));
                            cmd.Parameters.AddWithValue("@preparation_geometry_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@preparation_method_id", DB.MakeParam(typeof(Guid), prepMethId));
                            cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                            cmd.Parameters.AddWithValue("@amount", DBNull.Value);
                            cmd.Parameters.AddWithValue("@prep_unit_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@quantity", DBNull.Value);
                            cmd.Parameters.AddWithValue("@quantity_unit_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@fill_height_mm", DBNull.Value);
                            cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                            cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@created_by", Common.Username);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                            cmd.ExecuteNonQuery();

                            GenerateOrderAnalyses(connection, transaction, orderId, labId, newPrepId, tnode.Nodes);

                            prepCount--;
                        }                        
                    }
                }

                SqlCommand cmd2 = new SqlCommand("insert into sample_x_assignment_sample_type values(@sample_id, @assignment_sample_type_id)", connection, transaction);
                cmd2.Parameters.AddWithValue("@sample_id", DB.MakeParam(typeof(Guid), sampleId));
                cmd2.Parameters.AddWithValue("@assignment_sample_type_id", DB.MakeParam(typeof(Guid), orderLineId));
                cmd2.ExecuteNonQuery();

                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
            }
            finally
            {
                connection?.Close();
            }
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
                    analMethId = Guid.Parse(reader["analysis_method_id"].ToString());
                    analCount = Convert.ToInt32(reader["analysis_method_count"]);
                }

                SqlCommand cmd = new SqlCommand("csp_insert_analysis", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;

                while (analCount > 0)
                {                    
                    Guid newAnalId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newAnalId);
                    cmd.Parameters.AddWithValue("@number", nextAnalNumber++);
                    cmd.Parameters.AddWithValue("@assignment_id", DB.MakeParam(typeof(Guid), orderId));
                    cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), labId));
                    cmd.Parameters.AddWithValue("@preparation_id", DB.MakeParam(typeof(Guid), prepId));
                    cmd.Parameters.AddWithValue("@analysis_method_id", DB.MakeParam(typeof(Guid), analMethId));
                    cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                    cmd.Parameters.AddWithValue("@specter_reference", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sigma_act", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sigma_mda", DBNull.Value);
                    cmd.Parameters.AddWithValue("@nuclide_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@mda_library", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);
                                        
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

            Guid labId = Guid.Parse(cboxLaboratory.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrders(conn, InstanceStatus.Active, labId, gridOrders);
            }
        }

        private void gridOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (gridOrders.SelectedRows.Count < 1)
                return;

            Guid oid = Guid.Parse(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrderContentForSampleTypeName(conn, oid, treeOrderLines, SampleTypeId, TreeSampleTypes, false);
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

            using (SqlConnection conn = DB.OpenConnection())
            {
                object o = DB.GetScalar(conn, "select preparation_laboratory_id from assignment_preparation_method where id = @id", CommandType.Text, 
                    new SqlParameter("@id", apmId));
                if(!DB.IsValidField(o))
                {
                    MessageBox.Show("These preparations are not marked as external");
                    return;
                }
                prepLabId = Guid.Parse(o.ToString());
            }

            FormSelectExistingPreps form = new FormSelectExistingPreps(prepLabId, SampleId);
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
            tnode.ToolTipText = "";

            if (tnode.Level != 1 || tnode.Tag == null)
                return;

            List<Guid> prepList = tnode.Tag as List<Guid>;
            string query = "select number from preparation where id in (" 
                + String.Join(",", prepList.Select(x => "'" + x.ToString() + "'").ToArray()) + ")";

            List<object> prepNums = new List<object>();
            using (SqlConnection conn = DB.OpenConnection())            
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))            
                    while (reader.Read()) prepNums.Add(SampleNumber + "/" + reader["number"]);

            tnode.ToolTipText = "Connected preparations: " + String.Join(", ", prepNums);
        }
    }
}
