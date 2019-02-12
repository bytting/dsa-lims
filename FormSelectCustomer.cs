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
    public partial class FormSelectCustomer : Form
    {
        private int mInstanceStatusLevel;

        public Customer SelectedCustomer = new Customer();        

        public FormSelectCustomer(int instanceStatusLevel)
        {
            InitializeComponent();
            
            mInstanceStatusLevel = instanceStatusLevel;
        }

        private void FormSelectCustomer_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateCustomers(conn, mInstanceStatusLevel, gridCustomers);

                gridCustomers.Columns["company_email"].Visible = false;
                gridCustomers.Columns["company_phone"].Visible = false;
                gridCustomers.Columns["company_address"].Visible = false;
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

            SelectedCustomer.Id = Guid.Parse(gridCustomers.SelectedRows[0].Cells["id"].Value.ToString());
            SelectedCustomer.CompanyName = gridCustomers.SelectedRows[0].Cells["company_name"].Value.ToString();
            SelectedCustomer.CompanyEmail = gridCustomers.SelectedRows[0].Cells["company_email"].Value.ToString();
            SelectedCustomer.CompanyPhone = gridCustomers.SelectedRows[0].Cells["company_phone"].Value.ToString();
            SelectedCustomer.CompanyAddress = gridCustomers.SelectedRows[0].Cells["company_address"].Value.ToString();
            SelectedCustomer.ContactName = gridCustomers.SelectedRows[0].Cells["person_name"].Value.ToString();
            SelectedCustomer.ContactEmail = gridCustomers.SelectedRows[0].Cells["person_email"].Value.ToString();
            SelectedCustomer.ContactPhone = gridCustomers.SelectedRows[0].Cells["person_phone"].Value.ToString();
            SelectedCustomer.ContactAddress = gridCustomers.SelectedRows[0].Cells["person_address"].Value.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
