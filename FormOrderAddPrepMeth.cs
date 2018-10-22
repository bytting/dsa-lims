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
    public partial class FormOrderAddPrepMeth : Form
    {
        private Guid OrderSampleTypeId = Guid.Empty;

        public FormOrderAddPrepMeth(Guid orderSampleTypeId)
        {
            InitializeComponent();

            OrderSampleTypeId = orderSampleTypeId;
            cbPrepsAlreadyExists.Checked = false;
            cboxPrepMethLaboratory.Enabled = false;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulatePreparationMethods(conn, cboxPreparationMethod);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cboxPreparationMethod.SelectedItem == null)
            {
                MessageBox.Show("Preparation method is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Preparation method count is mandatory");
                return;
            }

            try
            {
                int cnt = Convert.ToInt32(tbCount.Text.Trim());

                using (SqlConnection conn = DB.OpenConnection())
                {
                    Lemma<Guid, string> prepMeth = cboxPreparationMethod.SelectedItem as Lemma<Guid, string>;
                    Lemma<Guid, string> prepLab = cboxPrepMethLaboratory.SelectedItem as Lemma<Guid, string>;

                    SqlCommand cmd = new SqlCommand("csp_insert_assignment_preparation_method", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@assignment_sample_type_id", OrderSampleTypeId);
                    cmd.Parameters.AddWithValue("@preparation_method_id", prepMeth.Id);
                    cmd.Parameters.AddWithValue("@preparation_method_count", cnt);

                    if (prepLab == null)
                        cmd.Parameters.AddWithValue("@preparation_laboratory_id", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@preparation_laboratory_id", prepLab.Id);

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

        private void cbPrepsAlreadyExists_CheckedChanged(object sender, EventArgs e)
        {
            cboxPrepMethLaboratory.Enabled = true;
        }
    }
}
