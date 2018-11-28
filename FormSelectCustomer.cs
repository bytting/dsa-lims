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
    public partial class FormSelectCustomer : Form
    {
        public CustomerModel SelectedCustomer = new CustomerModel();

        public FormSelectCustomer()
        {
            InitializeComponent();
        }

        private void FormSelectCustomer_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateCustomers(conn, InstanceStatus.Active, gridCustomers);
                gridCustomers.Columns["instance_status_name"].Visible = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(gridCustomers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a customer first");
                return;
            }
            
            SelectedCustomer.Name = gridCustomers.SelectedRows[0].Cells["name"].Value.ToString();
            SelectedCustomer.Contact = gridCustomers.SelectedRows[0].Cells["contact"].Value.ToString();
            SelectedCustomer.Address = gridCustomers.SelectedRows[0].Cells["address"].Value.ToString();
            SelectedCustomer.Email = gridCustomers.SelectedRows[0].Cells["email"].Value.ToString();            
            SelectedCustomer.Phone = gridCustomers.SelectedRows[0].Cells["phone"].Value.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
