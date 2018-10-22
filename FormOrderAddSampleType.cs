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
                UI.PopulateActivityUnits(conn, cboxRequestedUnit);
                UI.PopulateActivityUnitTypes(conn, cboxRequestedUnitType);
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

            if(cboxSampleType.SelectedItem == null)
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
                using (SqlConnection conn = DB.OpenConnection())
                {
                    Lemma<Guid, string> sampleType = cboxSampleType.SelectedItem as Lemma<Guid, string>;
                    Lemma<Guid, string> sampleComponent = cboxSampleComponent.SelectedItem as Lemma<Guid, string>;
                    int count = Convert.ToInt32(tbNumSamples.Text);
                    Lemma<Guid, string> activityUnit = cboxRequestedUnit.SelectedItem as Lemma<Guid, string>;
                    Lemma<int, string> activityUnitType = cboxRequestedUnitType.SelectedItem as Lemma<int, string>;                    

                    SqlCommand cmd = new SqlCommand("csp_insert_assignment_sample_type", conn);
                    cmd.CommandType = CommandType.StoredProcedure;                    
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@assignment_id", OrderId);
                    cmd.Parameters.AddWithValue("@sample_type_id", sampleType.Id);

                    if (sampleComponent == null)
                        cmd.Parameters.AddWithValue("@sample_component_id", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@sample_component_id", sampleComponent.Id);

                    cmd.Parameters.AddWithValue("@sample_count", count);

                    if(activityUnit == null)
                        cmd.Parameters.AddWithValue("@requested_activity_unit_id", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@requested_activity_unit_id", activityUnit.Id);

                    if (activityUnitType == null)
                        cmd.Parameters.AddWithValue("@requested_activity_unit_type_id", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@requested_activity_unit_type_id", activityUnitType.Id);

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

            if (cboxSampleType.SelectedItem == null)
            {
                cboxSampleComponent.Items.Clear();
                return;
            }

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
