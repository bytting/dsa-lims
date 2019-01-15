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
            string username = tbUsername.Text.ToLower().Trim();
            string password = tbPassword.Text.Trim();

            try
            {
                if (cbUseAD.Checked)
                {
                    if(!username.StartsWith(Environment.UserDomainName.ToLower()))
                        username = Environment.UserDomainName.ToLower() + "\\" + username;

                    if (!ValidateADUser(username, password))
                    {
                        if(username != Environment.UserDomainName.ToLower() + "\\administrator")
                        {
                            MessageBox.Show("Authentication failed");                            
                        }
                        else
                        {
                            tbUsername.Text = "";
                            tbPassword.Text = "";
                        }
                        return;
                    }
                }
                else
                {
                    if (username.Length < Utils.MIN_USERNAME_LENGTH)
                    {
                        MessageBox.Show("Username must be at least 3 characters long");
                        return;
                    }

                    if (password.Length < Utils.MIN_PASSWORD_LENGTH)
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }

                    if (!ValidateLimsUser(username, password))
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }
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

                if (username == Environment.UserDomainName.ToLower() + "\\administrator")
                {
                    FormCreateLIMSAdministrator form = new FormCreateLIMSAdministrator();
                    if (form.ShowDialog() != DialogResult.OK)
                        return false;

                    using (SqlConnection conn = new SqlConnection(settings.ConnectionString))
                    {
                        conn.Open();

                        Guid personId = Guid.Empty;
                        Guid adminId = Guid.Empty;
                        SqlCommand cmd = new SqlCommand("select id from person where name = 'LIMSAdministrator'", conn);
                        cmd.CommandType = System.Data.CommandType.Text;
                        object o = cmd.ExecuteScalar();
                        if (!DB.IsValidField(o))
                        {
                            personId = Guid.NewGuid();
                            cmd.CommandText = "insert into person values(@id, @name, @email, @phone, @address, @create_date, @update_date)";
                            cmd.Parameters.AddWithValue("@id", personId);
                            cmd.Parameters.AddWithValue("@name", "LIMSAdministrator");
                            cmd.Parameters.AddWithValue("@email", DBNull.Value);
                            cmd.Parameters.AddWithValue("@phone", DBNull.Value);
                            cmd.Parameters.AddWithValue("@address", DBNull.Value);
                            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            personId = Guid.Parse(o.ToString());
                        }

                        byte[] passwordHash = Utils.MakePasswordHash(form.SelectedPassword, "LIMSAdministrator");

                        cmd = new SqlCommand("select id from account where person_id = @pid", conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@pid", personId);
                        o = cmd.ExecuteScalar();
                        if (!DB.IsValidField(o))
                        {
                            adminId = Guid.NewGuid();
                            cmd.CommandText = "insert into account values(@id, @username, @person_id, @laboratory_id, @language_code, @instance_status_id, @password_hash, @create_date, @update_date)";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", adminId);
                            cmd.Parameters.AddWithValue("@username", "LIMSAdministrator");
                            cmd.Parameters.AddWithValue("@person_id", personId);
                            cmd.Parameters.AddWithValue("@laboratory_id", DBNull.Value);
                            cmd.Parameters.AddWithValue("@language_code", "en");
                            cmd.Parameters.AddWithValue("@instance_status_id", 1);
                            cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        }
                        else
                        {
                            adminId = Guid.Parse(o.ToString());
                            cmd.CommandText = "update account set password_hash = @password_hash, update_date = @update_date where id = @id";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@id", adminId);                            
                            cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);                            
                        }
                        cmd.ExecuteNonQuery();
                    }

                    return false;
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(settings.ConnectionString))
                    {
                        string shortUsername = username;
                        if(shortUsername.StartsWith(Environment.UserDomainName.ToLower()))
                        {
                            string[] items = shortUsername.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            shortUsername = items[1];
                        }

                        conn.Open();

                        SqlCommand cmd = new SqlCommand("select id, laboratory_id from account where username = @username", conn);
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Parameters.AddWithValue("@username", shortUsername);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return false;

                            reader.Read();

                            mUserId = Guid.Parse(reader["id"].ToString());
                            mUserName = shortUsername;
                            mLabId = Utils.IsValidGuid(reader["laboratory_id"]) ? Guid.Parse(reader["laboratory_id"].ToString()) : Guid.Empty;
                        }
                    }

                    return true;
                }
            }            
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

                SqlCommand cmd = new SqlCommand("select id, laboratory_id, password_hash from account where upper(username) = @username", conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.AddWithValue("@username", username.ToUpper());

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)                    
                        return false;

                    reader.Read();

                    userId = Guid.Parse(reader["id"].ToString());
                    labId = Utils.IsValidGuid(reader["laboratory_id"]) ? Guid.Parse(reader["laboratory_id"].ToString()) : Guid.Empty;
                    hash2 = reader.GetSqlBinary(2).Value;
                }

                if (!Utils.PasswordHashEqual(hash1, hash2))                
                    return false;

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
