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
    public partial class FormSelectSampleType : Form
    {
        public SampleTypeModel SelectedSampleType = null;

        public FormSelectSampleType()
        {
            InitializeComponent();
        }

        private void FormSelectSampleType_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())            
                UI.PopulateSampleTypes(treeSampleTypes);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            SelectedSampleType = treeSampleTypes.SelectedNode.Tag as SampleTypeModel;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
