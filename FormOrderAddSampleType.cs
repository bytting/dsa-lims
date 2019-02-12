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
        private Assignment mAssignment = null;
        private TreeView TreeSampleTypes = null;

        public FormOrderAddSampleType(Assignment assignment, TreeView treeSampleTypes)
        {
            InitializeComponent();

            tbNumSamples.KeyPress += CustomEvents.Integer_KeyPress;

            mAssignment = assignment;
            TreeSampleTypes = treeSampleTypes;
            UI.PopulateSampleTypes(TreeSampleTypes, cboxSampleType);
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_activity_units_short", new SqlParameter[] { }, cboxRequestedUnit);
                UI.PopulateComboBoxes(conn, "csp_select_activity_unit_types", new SqlParameter[] { }, cboxRequestedUnitType);
            }                
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

            if(!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbNumSamples.Text.Trim()))
            {
                MessageBox.Show("Number of samples is mandatory");
                return;
            }

            AssignmentSampleType ast = new AssignmentSampleType();
            ast.AssignmentId = mAssignment.Id;
            ast.SampleTypeId = Guid.Parse(cboxSampleType.SelectedValue.ToString());
            ast.SampleComponentId = Guid.Parse(cboxSampleComponent.SelectedValue.ToString());
            ast.SampleCount = Convert.ToInt32(tbNumSamples.Text);
            ast.RequestedActivityUnitId = Guid.Parse(cboxRequestedUnit.SelectedValue.ToString());
            ast.RequestedActivityUnitTypeId = Guid.Parse(cboxRequestedUnitType.SelectedValue.ToString());
            ast.ReturnToSender = cbReturnToSender.Checked;
            ast.Comment = tbComment.Text.Trim();
            ast.CreateDate = DateTime.Now;
            ast.CreatedBy = Common.Username;
            ast.UpdateDate = DateTime.Now;
            ast.UpdatedBy = Common.Username;
            ast.Dirty = true;
            mAssignment.SampleTypes.Add(ast);            

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSelectSampleType_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleType.SelectedValue = form.SelectedSampleTypeId;
        }

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Guid.Parse(cboxSampleType.SelectedValue.ToString());
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleComponent);
            }
        }        

        private bool CheckExistingSampleType()
        {
            if(!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleType.SelectedItem = Guid.Empty;
                cboxSampleComponent.DataSource = null;

                lblStatus.Text = Utils.makeStatusMessage("You must select an existing sample type");
                return false;
            }

            return true;
        }

        private void cboxSampleType_Leave(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (String.IsNullOrEmpty(cboxSampleType.Text.Trim()))
            {                
                cboxSampleType.SelectedValue = Guid.Empty;
                cboxSampleComponent.DataSource = null;
                return;
            }

            CheckExistingSampleType();
        }
    }
}
