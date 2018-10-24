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
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
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

            SelectedOrder = new Guid(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            SelectedOrderLine = new Guid(treeOrderLines.SelectedNode.Name);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cboxLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
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
                UI.PopulateOrderContent(conn, oid, treeOrderLines, TreeSampleTypes);
            }
        }

        private void treeOrderLines_AfterSelect(object sender, TreeViewEventArgs e)
        {
            /*TreeNode tnode = treeOrderLines.SelectedNode;
            while (tnode.Level != 0)
                tnode = tnode.Parent;
            treeOrderLines.SelectedNode = tnode;*/
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

        }
    }
}
