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
    public partial class FormReportAssignedWork : Form
    {
        Guid mLabId = Guid.Empty;

        public FormReportAssignedWork(Guid labId)
        {
            InitializeComponent();

            mLabId = labId;
        }

        private void FormReportAssignedWork_Load(object sender, EventArgs e)
        {            
            assignmentTableAdapter.Fill(this.DSAssignedWork.assignment, mLabId);
            
            //reportViewerAssignedWork.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.FullPage;

            reportViewerAssignedWork.RefreshReport();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
