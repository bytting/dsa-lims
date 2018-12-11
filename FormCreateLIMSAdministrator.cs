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
using System.Drawing;
using System.Linq;
using System.Text;
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
