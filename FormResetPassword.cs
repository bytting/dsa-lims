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
    public partial class FormResetPassword : Form
    {
        private Guid mUserId = Guid.Empty;
        private string mUsername = String.Empty;

        public FormResetPassword(Guid userId, string username)
        {
            InitializeComponent();

            mUserId = userId;
            mUsername = username;
            tbUsername.Text = mUsername;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (tbPassword1.Text.Length < Utils.MIN_PASSWORD_LENGTH)
            {
                MessageBox.Show("Password must be at least 8 characters long");
                return;
            }

            if (tbPassword1.Text.CompareTo(tbPassword2.Text) != 0)
            {
                MessageBox.Show("Passwords are not equal");
                return;
            }

            byte[] passwordHash = Utils.MakePasswordHash(tbPassword1.Text.Trim(), mUsername);

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("update account set password_hash = @password_hash where id = @user_id", conn);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                cmd.Parameters.AddWithValue("@user_id", mUserId);
                cmd.ExecuteNonQuery();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
