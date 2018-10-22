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
    public partial class FormOrderAddAnalMeth : Form
    {
        private Guid OrderPrepMethId = Guid.Empty;

        public FormOrderAddAnalMeth(Guid orderPrepMethId)
        {
            InitializeComponent();

            OrderPrepMethId = orderPrepMethId;
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalysisMethods(conn, cboxAnalysisMethods);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cboxAnalysisMethods.SelectedItem == null)
            {
                MessageBox.Show("Analysis method is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Analysis method count is mandatory");
                return;
            }

            try
            {
                int cnt = Convert.ToInt32(tbCount.Text.Trim());

                using (SqlConnection conn = DB.OpenConnection())
                {
                    Lemma<Guid, string> analMeth = cboxAnalysisMethods.SelectedItem as Lemma<Guid, string>;

                    SqlCommand cmd = new SqlCommand("csp_insert_assignment_analysis_method", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@assignment_preparation_method_id", OrderPrepMethId);
                    cmd.Parameters.AddWithValue("@analysis_method_id", analMeth.Id);
                    cmd.Parameters.AddWithValue("@analysis_method_count", cnt);
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
    }
}
