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
    public partial class FormCreateLIMSAdministrator : Form
    {
        public string SelectedPassword = String.Empty;

        public FormCreateLIMSAdministrator()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string username = "LIMSAdministrator";
            string password1 = tbPassword1.Text;
            string password2 = tbPassword2.Text;

            if(password1.Length < Utils.MIN_PASSWORD_LENGTH)
            {
                MessageBox.Show("Password must be at least 8 charcters long");
                return;
            }

            if (password1 != password2)
            {
                MessageBox.Show("Passwords must be identical");
                return;
            }

            SelectedPassword = password1;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
