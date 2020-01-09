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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormReportPrepSummary : Form
    {
        Guid mAssignmentId = Guid.Empty;

        public FormReportPrepSummary(Guid assignmentId)
        {
            InitializeComponent();

            mAssignmentId = assignmentId;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void FormReportPrepSummary_Load(object sender, EventArgs e)
        {            
            DataTable1TableAdapter.Fill(this.DSPrepSummary.DataTable1, mAssignmentId);

            reportViewerPrepSummary.ZoomMode = Microsoft.Reporting.WinForms.ZoomMode.PageWidth;
            reportViewerPrepSummary.RefreshReport();
        }
    }
}
