/*	
	DSA Lims - Laboratory Information Management System
    Copyright (C) 2018  Norwegian Radiation Protection Authority

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
// Authors: Dag Robole,

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
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

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("update account set password_hash = @password_hash where id = @user_id", conn);
                cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                cmd.Parameters.AddWithValue("@user_id", mUserId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
