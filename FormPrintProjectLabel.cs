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
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrintProjectLabel : Form
    {
        DSASettings mSettings = null;
        Font fontLabel = null, fontTitle = null, fontHeader = null;
        PrintDocument printDocument = new PrintDocument();
        Guid mProjectId = Guid.Empty;

        string ProjectMainName, ProjectSubName;

        public FormPrintProjectLabel(DSASettings s, Guid projectId)
        {
            InitializeComponent();

            mProjectId = projectId;

            tbCopies.Text = "1";
            tbCopies.KeyPress += CustomEvents.Integer_KeyPress;

            mSettings = s;

            cboxPaperSizes.DisplayMember = "PaperName";
        }

        private void FormPrintProjectLabel_Load(object sender, EventArgs e)
        {
            try
            {
                fontTitle = new Font("Arial", 16);
                fontHeader = new Font("Arial", 8, FontStyle.Bold);
                fontLabel = new Font("Arial", 12);

                cboxPrinters.SelectedIndexChanged -= cboxPrinters_SelectedIndexChanged;

                foreach (string p in PrinterSettings.InstalledPrinters)
                    cboxPrinters.Items.Add(p.ToString());

                cboxPrinters.SelectedIndexChanged += cboxPrinters_SelectedIndexChanged;

                if (cboxPrinters.FindString(mSettings.LabelPrinterName) > -1)
                    cboxPrinters.Text = mSettings.LabelPrinterName;

                cbLandscape.Checked = mSettings.LabelPrinterLandscape;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }        

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Pen pen = new Pen(Color.Black);
            e.Graphics.DrawString("DSA Project", fontTitle, Brushes.Black, 2, 1);
            e.Graphics.DrawLine(pen, 2, 28, 132, 28);

            e.Graphics.DrawString("Main project:", fontHeader, Brushes.Black, 2, 38);
            e.Graphics.DrawString(ProjectMainName, fontLabel, Brushes.Black, 2, 54);

            e.Graphics.DrawString("Sub project:", fontHeader, Brushes.Black, 2, 82);
            e.Graphics.DrawString(ProjectSubName, fontLabel, Brushes.Black, 2, 98);
        }

        private void cboxPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cboxPrinters.Text))
                return;

            cboxPaperSizes.Items.Clear();

            printDocument.PrinterSettings.PrinterName = cboxPrinters.Text;

            foreach (PaperSize ps in printDocument.PrinterSettings.PaperSizes)
                cboxPaperSizes.Items.Add(ps);

            foreach (PaperSize ps in cboxPaperSizes.Items)
            {
                if (ps.PaperName == mSettings.LabelPrinterPaperName)
                {
                    cboxPaperSizes.SelectedItem = ps;
                    break;
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(cboxPrinters.Text))
                {
                    MessageBox.Show("You must select a printer");
                    return;
                }

                if (cboxPaperSizes.SelectedItem == null)
                {
                    MessageBox.Show("You must select a printer size");
                    return;
                }

                if (String.IsNullOrEmpty(tbCopies.Text))
                {
                    MessageBox.Show("Number of copies must be a positive number");
                    return;
                }

                int copies = Convert.ToInt32(tbCopies.Text);
                if (copies < 1)
                {
                    MessageBox.Show("Number of copies must be a positive number");
                    return;
                }

                PaperSize paperSize = cboxPaperSizes.SelectedItem as PaperSize;
                printDocument.DefaultPageSettings.Landscape = cbLandscape.Checked;
                printDocument.DefaultPageSettings.PaperSize = paperSize;

                printDocument.PrintPage += PrintDocument_PrintPage;

                string query = @"
select 
    ps.name as 'project_sub_name', 
    pm.name as 'project_main_name'    
from project_sub ps
    inner join project_main pm on pm.id = ps.project_main_id and ps.id = @psid
";

                SqlConnection conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, new SqlParameter("@psid", mProjectId)))
                    {
                        if (!reader.HasRows)
                        {
                            MessageBox.Show("No id found for sub project");
                            return;
                        }

                        reader.Read();

                        ProjectMainName = reader.GetString("project_main_name");
                        ProjectSubName = reader.GetString("project_sub_name");

                        for (int c = 0; c < copies; c++)
                            printDocument.Print();
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.Error(ex);
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    conn?.Close();
                }

                mSettings.LabelPrinterName = cboxPrinters.Text;
                mSettings.LabelPrinterPaperName = paperSize.PaperName;
                mSettings.LabelPrinterLandscape = cbLandscape.Checked;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }
    }
}
