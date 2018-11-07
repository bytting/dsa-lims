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
        private Guid OrderId = Guid.Empty;
        private TreeView TreeSampleTypes = null;

        public FormOrderAddSampleType(Guid orderId, TreeView treeSampleTypes)
        {
            InitializeComponent();

            OrderId = orderId;
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

            if(!StrUtils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbNumSamples.Text.Trim()))
            {
                MessageBox.Show("Number of samples is mandatory");
                return;
            }

            try
            {
                Guid sampleTypeId = Guid.Parse(cboxSampleType.SelectedValue.ToString());
                int count = Convert.ToInt32(tbNumSamples.Text);

                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("csp_insert_assignment_sample_type", conn);
                    cmd.CommandType = CommandType.StoredProcedure;                    
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@assignment_id", OrderId);
                    cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), sampleTypeId));                    
                    cmd.Parameters.AddWithValue("@sample_component_id", DB.MakeParam(typeof(Guid), cboxSampleComponent.SelectedValue));
                    cmd.Parameters.AddWithValue("@sample_count", count);                    
                    cmd.Parameters.AddWithValue("@requested_activity_unit_id", DB.MakeParam(typeof(Guid), cboxRequestedUnit.SelectedValue));                    
                    cmd.Parameters.AddWithValue("@requested_activity_unit_type_id", DB.MakeParam(typeof(Guid), cboxRequestedUnitType.SelectedValue));
                    cmd.Parameters.AddWithValue("@return_to_sender", cbReturnToSender.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@comment", tbComment.Text);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }

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

            if (!StrUtils.IsValidGuid(cboxSampleType.SelectedValue))
                return;            

            Guid sampleTypeId = Guid.Parse(cboxSampleType.SelectedValue.ToString());
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                AddSampleTypeComponents(conn, sampleTypeId, tnodes[0]);
            }
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
            if(!StrUtils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleType.SelectedItem = Guid.Empty;
                cboxSampleComponent.DataSource = null;

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
