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
    public partial class FormOrderAddSampleType : Form
    {
        private TreeView TreeSampleTypes = null;

        public FormOrderAddSampleType(TreeView treeSampleTypes)
        {
            InitializeComponent();

            TreeSampleTypes = treeSampleTypes;
            UI.PopulateSampleTypes(TreeSampleTypes, cboxSampleType);
        }

        private void FormOrderAddSampleType_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!CheckExistingSampleType())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSelectSampleType_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleType.SelectedIndex = cboxSampleType.FindStringExact(
                form.SelectedSampleTypeName + " -> " + form.SelectedSampleTypePath);
        }

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "";            
            cboxSampleComponent.Items.Clear();

            if (cboxSampleType.SelectedItem == null)
                return;

            var sampleType = cboxSampleType.SelectedItem as Lemma<Guid, string>;
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleType.Id.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                AddSampleTypeComponents(conn, sampleType.Id, tnodes[0]);
            }

            cboxSampleComponent.SelectedIndex = -1;
        }

        private void AddSampleTypeComponents(SqlConnection conn, Guid sampleTypeId, TreeNode tnode)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    cboxSampleComponent.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = new Guid(tnode.Parent.Name);
                AddSampleTypeComponents(conn, parentId, tnode.Parent);
            }
        }

        private bool CheckExistingSampleType()
        {
            Lemma<Guid, string> st = cboxSampleType.SelectedItem as Lemma<Guid, string>;
            if (st == null)
            {
                cboxSampleType.Text = "";
                cboxSampleType.SelectedItem = null;
                cboxSampleComponent.Items.Clear();

                lblStatus.Text = StrUtils.makeStatusMessage("You must select an existing sample type");
                return false;
            }

            return true;
        }

        private void cboxSampleType_Leave(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (String.IsNullOrEmpty(cboxSampleType.Text.Trim()))
            {                
                cboxSampleType.SelectedItem = null;
                cboxSampleComponent.Items.Clear();
                return;
            }

            CheckExistingSampleType();
        }        
    }
}
