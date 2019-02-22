using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormReportAuditComment : Form
    {
        private string mSelectedComment;

        public string SelectedComment { get { return mSelectedComment; } }

        public FormReportAuditComment()
        {
            InitializeComponent();

            lblChars.Text = "";
        }

        private void tbComment_TextChanged(object sender, EventArgs e)
        {
            lblChars.Text = tbComment.Text.Length + " / 100";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbComment.Text.Trim()))
            {
                MessageBox.Show("You must provide a comment");
                return;
            }

            mSelectedComment = tbComment.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
