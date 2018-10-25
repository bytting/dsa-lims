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
        private Guid SampleTypeId = Guid.Empty;

        public Guid SelectedLaboratory = Guid.Empty;
        public Guid SelectedOrder = Guid.Empty;
        public Guid SelectedOrderLine = Guid.Empty;

        public FormSelectOrder(TreeView treeSampleTypes, Guid sampId)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;
            SampleId = sampId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateLaboratories(conn, InstanceStatus.Deleted, cboxLaboratory);

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
            if(cboxLaboratory.SelectedItem == null)
            {
                MessageBox.Show("You must select a laboratory first");
                return;
            }

            if(gridOrders.SelectedRows.Count < 1)
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

            if(tnode.Level != 0)
            {
                MessageBox.Show("You must select a top level order line for this sample");
                return;
            }

            Lemma<Guid, string> lab = cboxLaboratory.SelectedItem as Lemma<Guid, string>;
            SelectedLaboratory = lab.Id;
            SelectedOrder = new Guid(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            SelectedOrderLine = new Guid(tnode.Name);

            GenerateOrderPreparations(SampleId, SelectedLaboratory, SelectedOrder, SelectedOrderLine, tnode.Nodes);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void GenerateOrderPreparations(Guid sampleId, Guid labId, Guid orderId, Guid orderLineId, TreeNodeCollection tnodes)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                int nextPrepNumber = 1;

                foreach (TreeNode tnode in tnodes)
                {
                    Guid prepMethLineId = Guid.Parse(tnode.Name);

                    if (tnode.Tag != null)
                    {
                        List<Guid> prepList = tnode.Tag as List<Guid>;
                        foreach (Guid pid in prepList)
                        {
                            GenerateOrderAnalyses(conn, orderId, labId, pid, tnode.Nodes);
                        }
                    }
                    else
                    {
                        Guid prepMethId = Guid.Empty;
                        int prepCount = 0;

                        string query = "select preparation_method_id, preparation_method_count from assignment_preparation_method where id = @id";
                        using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new SqlParameter("@id", prepMethLineId)))
                        {
                            reader.Read(); // FIXME
                            prepMethId = Guid.Parse(reader["preparation_method_id"].ToString());
                            prepCount = Convert.ToInt32(reader["preparation_method_count"]);
                        }

                        SqlCommand cmd = new SqlCommand("csp_insert_preparation", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        while (prepCount > 0)
                        {
                            Guid newPrepId = Guid.NewGuid();
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", newPrepId);
                            cmd.Parameters.AddWithValue("@sample_id", sampleId);
                            cmd.Parameters.AddWithValue("@number", nextPrepNumber++);
                            cmd.Parameters.AddWithValue("@assignment_id", orderId);
                            cmd.Parameters.AddWithValue("@laboratory_id", labId);
                            cmd.Parameters.AddWithValue("@preparation_geometry_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@preparation_method_id", prepMethId);
                            cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                            cmd.Parameters.AddWithValue("@amount", DBNull.Value);
                            cmd.Parameters.AddWithValue("@prep_unit_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@fill_height_mm", DBNull.Value);
                            cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                            cmd.Parameters.AddWithValue("@comment", DBNull.Value);
                            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@created_by", Common.Username);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                            cmd.ExecuteNonQuery();

                            GenerateOrderAnalyses(conn, orderId, labId, newPrepId, tnode.Nodes);

                            prepCount--;
                        }
                    }
                }
            }
        }

        private void GenerateOrderAnalyses(SqlConnection conn, Guid orderId, Guid labId, Guid prepId, TreeNodeCollection tnodes)
        {
            Guid analMethId = Guid.Empty;
            int analCount = 0;
            int nextAnalNumber = 1;

            foreach (TreeNode tnode in tnodes)
            {
                Guid analMethLineId = Guid.Parse(tnode.Name);

                string query = "select analysis_method_id, analysis_method_count from assignment_analysis_method where id = @id";
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new SqlParameter("@id", analMethLineId)))
                {
                    reader.Read(); // FIXME
                    analMethId = Guid.Parse(reader["analysis_method_id"].ToString());
                    analCount = Convert.ToInt32(reader["analysis_method_count"]);
                }

                SqlCommand cmd = new SqlCommand("csp_insert_analysis", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                while (analCount > 0)
                {                    
                    Guid newAnalId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newAnalId);
                    cmd.Parameters.AddWithValue("@number", nextAnalNumber++);
                    cmd.Parameters.AddWithValue("@assignment_id", orderId);
                    cmd.Parameters.AddWithValue("@laboratory_id", labId);
                    cmd.Parameters.AddWithValue("@preparation_id", prepId);
                    cmd.Parameters.AddWithValue("@analysis_method_id", analMethId);
                    cmd.Parameters.AddWithValue("@workflow_status_id", 1);
                    cmd.Parameters.AddWithValue("@specter_reference", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@sigma", DBNull.Value);
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

            if (cboxLaboratory.SelectedItem == null)            
                return;

            Lemma<Guid, string> lab = cboxLaboratory.SelectedItem as Lemma<Guid, string>;
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrders(conn, InstanceStatus.Active, lab.Id, gridOrders);
            }
        }

        private void gridOrders_SelectionChanged(object sender, EventArgs e)
        {
            if (gridOrders.SelectedRows.Count < 1)
                return;

            Guid oid = new Guid(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrderContent(conn, oid, treeOrderLines, SampleTypeId, TreeSampleTypes);
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
            Guid apmId = new Guid(tnode.Name);

            using (SqlConnection conn = DB.OpenConnection())
            {
                object o = DB.GetScalar(conn, "select preparation_laboratory_id from assignment_preparation_method where id = @id", CommandType.Text, 
                    new SqlParameter("@id", apmId));
                if(o == null || o == DBNull.Value)
                {
                    MessageBox.Show("Preparation method is not marked as existing");
                    return;
                }
                prepLabId = new Guid(o.ToString());
            }

            FormSelectExistingPreps form = new FormSelectExistingPreps(prepLabId, SampleId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            if (form.SelectedPreparationIds.Count == 0)
                tnode.Tag = null;
            else
            {
                List<Guid> guidList = new List<Guid>();
                guidList.AddRange(form.SelectedPreparationIds);
                tnode.Tag = guidList;
            }            

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
        }

        private void UpdateCurrentPreparations(TreeNode tnode)
        {
            tbCurrentPreparations.Text = "";

            if (tnode.Level != 1)
                return;

            if (tnode.Tag == null)
                return;

            string query = "select number from preparation where id in (";
            List<Guid> prepList = tnode.Tag as List<Guid>;
            foreach (Guid id in prepList)
                query += "'" + id.ToString() + "',";
            query = query.Substring(0, query.Length - 1) + ")";

            string line = "Connected preparations: ";
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))
                {
                    while (reader.Read())
                        line += reader["number"].ToString() + ", ";
                }
            }

            tbCurrentPreparations.Text = line.Substring(0, line.Length - 2);
        }
    }
}
