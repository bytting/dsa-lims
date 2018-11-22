using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrepAnalAddPrep : Form
    {
        Guid SampleId = Guid.Empty;

        public FormPrepAnalAddPrep(Guid sampleId)
        {
            InitializeComponent();

            SampleId = sampleId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_preparation_methods_short", new[] {
                    new SqlParameter("instance_status_level", InstanceStatus.Active)
                }, cboxPrepMethods);
            }                    
        }

        private void FormPrepAnalAddPrep_Load(object sender, EventArgs e)
        {
            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Lemma<Guid, string> l = cboxPrepMethods.SelectedValue as Lemma<Guid, string>;

            if (!Utils.IsValidGuid(l.Id))
            {
                MessageBox.Show("Preparation method is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Count is mandatory");
                return;
            }

            int count = Convert.ToInt32(tbCount.Text.Trim());

            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                int nextPrepNumber = DB.GetNextPreparationNumber(connection, transaction, SampleId);

                SqlCommand cmd = new SqlCommand("csp_insert_preparation", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                while (count > 0)
                {
                    Guid newPrepId = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", newPrepId);
                    cmd.Parameters.AddWithValue("@sample_id", DB.MakeParam(typeof(Guid), SampleId));
                    cmd.Parameters.AddWithValue("@number", ++nextPrepNumber);
                    cmd.Parameters.AddWithValue("@assignment_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@laboratory_id", Common.LabId);
                    cmd.Parameters.AddWithValue("@preparation_geometry_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@preparation_method_id", DB.MakeParam(typeof(Guid), l.Id));
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
                    count--;
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
