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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLaboratory : Form
    {
        private bool isEdit;

        public string Label;
        public string Prefix;
        public string Address;
        public string Email;
        public string Phone;
        public bool InUse;
        public string Comment;

        public FormLaboratory(bool edit)
        {
            InitializeComponent();
            isEdit = edit;
        }

        public void SetValues(string label, string prefix, string address, string email, string phone, bool inUse, string comment)
        {
            tbName.Text = label;
            tbPrefix.Text = prefix;
            tbAddress.Text = address;
            tbEmail.Text = email;
            tbPhone.Text = phone;
            cbInUse.Checked = inUse;
            tbComment.Text = comment;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("The name field is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbPrefix.Text.Trim()))
            {
                MessageBox.Show("The prefix field is mandatory");
                return;
            }

            Label = tbName.Text.Trim();
            Prefix = tbPrefix.Text.Trim();
            Address = tbAddress.Text.Trim();
            Email = tbEmail.Text.Trim();
            Phone = tbPhone.Text.Trim();
            InUse = cbInUse.Checked;
            Comment = tbComment.Text.Trim();
            Close();
        }

        private void FormLaboratory_Load(object sender, EventArgs e)
        {            
        }
    }
}
