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
    public partial class FormSampTypeXPrepMeth : Form
    {
        private Guid SampleTypeId = Guid.Empty;
        private string SampleTypeName = String.Empty;
        private List<Guid> MethodsBelow = null;

        public FormSampTypeXPrepMeth(string sampleTypeName, Guid sampleTypeId, List<Guid> methodsAbove, List<Guid> methodsBelow)
        {
            InitializeComponent();

            SampleTypeId = sampleTypeId;
            SampleTypeName = sampleTypeName;
            MethodsBelow = methodsBelow;

            tbSampleType.Text = SampleTypeName;            

            using (SqlConnection conn = DB.OpenConnection())
            {
                var methArr = from item in methodsAbove select "'" + item + "'";
                string smeth = string.Join(",", methArr);

                string query;
                if (String.IsNullOrEmpty(smeth))
                    query = "select id, name from preparation_method order by name";
                else query = "select id, name from preparation_method where id not in(" + smeth + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))
                {
                    lbPrepMeth.Items.Clear();

                    while (reader.Read())
                    {
                        var pm = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                        lbPrepMeth.Items.Add(pm);
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
            if (lbPrepMeth.SelectedItems.Count > 0)
            {
                foreach (object item in lbPrepMeth.SelectedItems)
                {
                    var selItem = item as Lemma<Guid, string>;
                    if(MethodsBelow.Contains(selItem.Id))
                    {
                        MessageBox.Show("Preparation method " + selItem.Name + " already exist for one or more sample types below this one. You must remove these first");
                        return;
                    }
                }

                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into sample_type_x_preparation_method values(@sample_type_id, @preparation_method_id)", conn);

                    foreach (object item in lbPrepMeth.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId);
                        cmd.Parameters.AddWithValue("@preparation_method_id", selItem.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
