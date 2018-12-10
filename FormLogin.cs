﻿/*	
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLogin : Form
    {
        private DSASettings settings = null;        

        public FormLogin(DSASettings s)
        {
            InitializeComponent();
            settings = s;
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            cbUseAD.Checked = settings.UseActiveDirectoryCredentials;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            miExit_Click(sender, e);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;
            Close();
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
    }
}
