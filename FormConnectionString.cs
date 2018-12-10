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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormConnectionString : Form
    {
        private string mConnectionString;
        public string ConnectionString { get { return mConnectionString; } }

        public FormConnectionString(string currentConnString)
        {
            InitializeComponent();

            tbConnectionString.Text = currentConnString;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbConnectionString.Text.Trim()))
            {
                MessageBox.Show("Database connection string is mandatory");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = new SqlConnection(tbConnectionString.Text.Trim());
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection string does not appear to be valid: " + ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            mConnectionString = tbConnectionString.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
