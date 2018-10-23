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
        public Guid SelectedOrder = Guid.Empty;
        public Guid SelectedOrderLine = Guid.Empty;

        public FormSelectOrder(TreeView treeSampleTypes)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;

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
    }
}
