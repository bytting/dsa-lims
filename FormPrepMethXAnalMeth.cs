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
    public partial class FormPrepMethXAnalMeth : Form
    {
        private Guid PreparationMethodId = Guid.Empty;
        private string PreparationMethodName = String.Empty;
        private List<Guid> ExistingAnalysisMethods = null;

        public FormPrepMethXAnalMeth(string preparationMethodName, Guid preparationMethodId, List<Guid> existingAnalysisMethods)
        {
            InitializeComponent();

            PreparationMethodId = preparationMethodId;
            PreparationMethodName = preparationMethodName;
            ExistingAnalysisMethods = existingAnalysisMethods;

            tbPreparationMethod.Text = PreparationMethodName;

            using (SqlConnection conn = DB.OpenConnection())
            {
                var analMethArr = from item in ExistingAnalysisMethods select "'" + item + "'";
                string sanalmeth = string.Join(",", analMethArr);

                string query;
                if (String.IsNullOrEmpty(sanalmeth))
                    query = "select id, name from analysis_method order by name";
                else query = "select id, name from analysis_method where id not in(" + sanalmeth + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))
                {
                    lbAnalysisMethods.Items.Clear();

                    while (reader.Read())
                    {
                        var am = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                        lbAnalysisMethods.Items.Add(am);
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
            if (lbAnalysisMethods.SelectedItems.Count > 0)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into preparation_method_x_analysis_method values(@preparation_method_id, @analysis_method_id)", conn);

                    foreach (object item in lbAnalysisMethods.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@preparation_method_id", PreparationMethodId);
                        cmd.Parameters.AddWithValue("@analysis_method_id", selItem.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
