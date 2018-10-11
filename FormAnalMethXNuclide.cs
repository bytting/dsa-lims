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
    public partial class FormAnalMethXNuclide : Form
    {
        private Guid AnalysisMethodId = Guid.Empty;
        private string AnalysisMethodName = String.Empty;
        private List<Guid> ExistingNuclides = null;

        public FormAnalMethXNuclide(string analysisMethodName, Guid analysisMethodId, List<Guid> existingNuclides)
        {
            InitializeComponent();

            AnalysisMethodId = analysisMethodId;
            AnalysisMethodName = analysisMethodName;
            ExistingNuclides = existingNuclides;

            tbAnalysisMethod.Text = AnalysisMethodName;

            using (SqlConnection conn = DB.OpenConnection())
            {
                var nuclArr = from item in ExistingNuclides select "'" + item + "'";
                string snucl = string.Join(",", nuclArr);

                string query;
                if (String.IsNullOrEmpty(snucl))
                    query = "select id, name from nuclide order by name";
                else query = "select id, name from nuclide where id not in(" + snucl + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))
                {
                    lbNuclides.Items.Clear();

                    while (reader.Read())
                    {
                        var pm = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                        lbNuclides.Items.Add(pm);
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (lbNuclides.SelectedItems.Count > 0)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into analysis_method_x_nuclide values(@analysis_method_id, @nuclide_id)", conn);

                    foreach (object item in lbNuclides.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId);
                        cmd.Parameters.AddWithValue("@nuclide_id", selItem.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
