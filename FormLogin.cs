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
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLogin : Form
    {
        private DSASettings settings = null;
        private Guid mUserId = Guid.Empty;
        private string mUserName = String.Empty;
        private Guid mLabId = Guid.Empty;

        public Guid UserId { get { return mUserId; } }
        public string UserName { get { return mUserName; } }
        public Guid LabId { get { return mLabId; } }

        public FormLogin(DSASettings s)
        {
            InitializeComponent();
            settings = s;
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            cbUseAD.Checked = settings.UseActiveDirectoryCredentials;
            ActiveControl = tbUsername;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            miExit_Click(sender, e);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {            
            string username = tbUsername.Text.Trim();
            string password = tbPassword.Text.Trim();

            bool valid = false;

            try
            {

                if (cbUseAD.Checked)
                {
                    valid = ValidateADUser(username, password);
                }
                else
                {
                    if (username.Length < 3)
                    {
                        MessageBox.Show("Username must be at least 3 characters long");
                        return;
                    }

                    if (password.Length < Utils.MIN_PASSWORD_LENGTH)
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }

                    valid = ValidateLimsUser(username, password);
                }

                if (!valid)
                {
                    MessageBox.Show("Authentication failed");
                    return;
                }

                settings.UseActiveDirectoryCredentials = cbUseAD.Checked;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message);
            }
        }

        private bool ValidateADUser(string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, null))
            {
                if(!pc.ValidateCredentials(username, password))
                    return false;

                using (SqlConnection conn = new SqlConnection(settings.ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("select id, laboratory_id from account where username = @username", conn);
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return false;

                        reader.Read();

                        mUserId = Guid.Parse(reader["id"].ToString());
                        mLabId = Guid.Parse(reader["laboratory_id"].ToString());
                    }
                }
            }

            return true;
        }

        private bool ValidateLimsUser(string username, string password)
        {
            byte[] hash1 = Utils.MakePasswordHash(password, username);
            byte[] hash2 = null;
            Guid userId = Guid.Empty, labId = Guid.Empty;

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(settings.ConnectionString);
                conn.Open();

                SqlCommand cmd = new SqlCommand("select id, laboratory_id, password_hash from account where username = @username", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@username", username);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageBox.Show("Authentication failed");
                        return false;
                    }

                    reader.Read();

                    userId = Guid.Parse(reader["id"].ToString());
                    labId = Guid.Parse(reader["laboratory_id"].ToString());
                    hash2 = reader.GetSqlBinary(2).Value;
                }

                if (!Utils.PasswordHashEqual(hash1, hash2))
                {
                    MessageBox.Show("Authentication failed");
                    return false;
                }

                mUserId = userId;
                mUserName = username;
                mLabId = labId;

                return true;
            }            
            finally
            {
                conn?.Close();
            }
        }

    private void miExit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void miSetConnString_Click(object sender, EventArgs e)
        {
            FormConnectionString form = new FormConnectionString(settings.ConnectionString);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            settings.ConnectionString = form.ConnectionString;
        }

        private void tbUsername_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                ActiveControl = tbPassword;
            }
        }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                e.Handled = true;
                btnOk_Click(sender, e);
            }
        }
    }
}
