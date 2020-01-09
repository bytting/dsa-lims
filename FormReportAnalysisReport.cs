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

using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormReportAnalysisReport : Form
    {        
        byte[] mContent = null;
        bool mHasNewVersion;
        Assignment mAssignment = null;

        public byte[] ReportData { get { return mContent; } }

        public bool HasNewVersion { get { return mHasNewVersion; } }

        public FormReportAnalysisReport(Assignment assignment)
        {
            InitializeComponent();

            mAssignment = assignment;
            mHasNewVersion = false;
        }

        private void FormReportViewer_Load(object sender, EventArgs e)
        {
            DataTable1TableAdapter.Fill(DSOrderReport.DataTable1, mAssignment.Name);
            DataTable2TableAdapter.Fill(DSOrderReport.DataTable2, mAssignment.Name);

            reportViewer.LocalReport.DataSources.Clear();
            ReportDataSource rd1 = new ReportDataSource("DataSet1", DSOrderReport.Tables[0]);
            ReportDataSource rd2 = new ReportDataSource("DataSet2", DSOrderReport.Tables[1]);
            reportViewer.LocalReport.DataSources.Add(rd1);
            reportViewer.LocalReport.DataSources.Add(rd2);
            reportViewer.LocalReport.Refresh();
            reportViewer.RefreshReport();
        }        

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCreateVersion_Click(object sender, EventArgs e)
        {
            if (mAssignment.WorkflowStatusId != WorkflowStatus.Complete)
            {
                MessageBox.Show("Order must be saved as complete first");
                return;
            }
            
            int newVersion = mAssignment.AnalysisReportVersion + 1;

            if (mAssignment.AnalysisReportVersion > 0)
            {
                FormReportAuditComment form = new FormReportAuditComment();
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                mAssignment.AuditComment += "v." + newVersion + ": " + form.SelectedComment + Environment.NewLine;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();                
                mAssignment.AnalysisReportVersion = newVersion;
                mAssignment.Dirty = true;
                mAssignment.StoreToDB(conn, null);

                DataTable1TableAdapter.Fill(DSOrderReport.DataTable1, mAssignment.Name);
                DataTable2TableAdapter.Fill(DSOrderReport.DataTable2, mAssignment.Name);

                reportViewer.LocalReport.DataSources.Clear();
                ReportDataSource rd1 = new ReportDataSource("DataSet1", DSOrderReport.Tables[0]);
                ReportDataSource rd2 = new ReportDataSource("DataSet2", DSOrderReport.Tables[1]);
                reportViewer.LocalReport.DataSources.Add(rd1);
                reportViewer.LocalReport.DataSources.Add(rd2);
                reportViewer.LocalReport.Refresh();
                reportViewer.RefreshReport();

                mContent = reportViewer.LocalReport.Render("PDF", "");

                mHasNewVersion = true;
                btnCreateVersion.Enabled = false;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }            
        }
    }
}
