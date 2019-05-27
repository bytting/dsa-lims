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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrintPrepLabel : Form
    {
        PrivateFontCollection privateFonts = new PrivateFontCollection();
        Font fontBarcode = null;
        Font fontLabel = null;

        DSASettings mSettings = null;
        List<Guid> mPrepIds = new List<Guid>();

        string sampleNumber, prepNumber, sampleType, projectMain, projectSub, refDate, laboratory, prepWeightUnit;
        string prepQuantUnit, samplingTimeFrom, samplingTimeTo, station;
        double fillHeight = 0d, prepWeight = 0d, prepQuant = 0d;

        PrintDocument printDocument = new PrintDocument();

        public FormPrintPrepLabel(DSASettings s, List<Guid> prepIds)
        {
            InitializeComponent();

            mSettings = s;
            mPrepIds = prepIds;

            cboxPaperSizes.DisplayMember = "PaperName";
        }

        private void FormPrintPrepLabel_Load(object sender, EventArgs e)
        {
            try
            {
                fontLabel = new Font("Arial", 8);

                string InstallationDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                string fontFileName = InstallationDirectory + Path.DirectorySeparatorChar + "free3of9.ttf";
                if (File.Exists(fontFileName))
                {
                    privateFonts.AddFontFile(InstallationDirectory + Path.DirectorySeparatorChar + "free3of9.ttf");
                    fontBarcode = new Font(privateFonts.Families[0], 32, FontStyle.Regular);
                }
                else fontBarcode = fontLabel;

                cboxPrinters.SelectedIndexChanged -= cboxPrinters_SelectedIndexChanged;

                foreach (string p in PrinterSettings.InstalledPrinters)
                    cboxPrinters.Items.Add(p.ToString());

                cboxPrinters.SelectedIndexChanged += cboxPrinters_SelectedIndexChanged;

                if (cboxPrinters.FindString(mSettings.LabelPrinterName) > -1)
                    cboxPrinters.Text = mSettings.LabelPrinterName;

                cbLandscape.Checked = mSettings.LabelPrinterLandscape;
            }
            catch(Exception ex)
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

        private void btnOk_Click(object sender, EventArgs e)
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

            PaperSize paperSize = cboxPaperSizes.SelectedItem as PaperSize;
            printDocument.DefaultPageSettings.Landscape = cbLandscape.Checked;
            printDocument.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintPage += PrintDocument_PrintPage;

            string query = @"
select 
    s.number as 'sample_number', 
    s.sampling_date_from as 'sampling_date_from',
    s.sampling_date_to as 'sampling_date_to',
    p.number as 'preparation_number',
    st.name as 'sample_type_name',
    pm.name as 'project_main_name',
    ps.name as 'project_sub_name',
    s.reference_date as 'reference_date',
    l.name as 'laboratory_name',
    p.fill_height_mm as 'fill_height',
    p.amount as 'preparation_amount',
    pu.name_short as 'preparation_unit_name',
    p.quantity as 'preparation_quantity',
    qu.name as 'preparation_quantity_unit',
    sta.name as 'station_name'
from preparation p
    left outer join sample s on p.sample_id = s.id
    left outer join sample_type st on s.sample_type_id = st.id    
    left outer join project_sub ps on s.project_sub_id = ps.id
    left outer join project_main pm on ps.project_main_id = pm.id
    left outer join laboratory l on s.laboratory_id = l.id
    left outer join preparation_unit pu on pu.id = p.prep_unit_id
    left outer join quantity_unit qu on qu.id = p.quantity_unit_id
    left outer join station sta on sta.id = s.station_id
where p.id = @pid
";

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();                
                foreach (Guid pid in mPrepIds)
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, new SqlParameter("@pid", pid)))
                    {
                        if (!reader.HasRows)
                            continue;

                        reader.Read();

                        sampleNumber = reader.GetString("sample_number");
                        if (DB.IsValidField(reader["sampling_date_from"]))
                            samplingTimeFrom = reader.GetDateTime("sampling_date_from").ToString(Utils.DateTimeFormatNorwegian);
                        else samplingTimeFrom = "";
                        if (DB.IsValidField(reader["sampling_date_to"]))
                            samplingTimeTo = reader.GetDateTime("sampling_date_to").ToString(Utils.DateTimeFormatNorwegian);
                        else samplingTimeTo = "";
                        prepNumber = reader.GetString("preparation_number");
                        sampleType = reader.GetString("sample_type_name");
                        projectMain = reader.GetString("project_main_name");
                        projectSub = reader.GetString("project_sub_name");
                        refDate = reader.GetDateTime("reference_date").ToString(Utils.DateTimeFormatNorwegian);
                        laboratory = reader.GetString("laboratory_name");
                        if (DB.IsValidField(reader["fill_height"]))
                            fillHeight = reader.GetDouble("fill_height");
                        if (DB.IsValidField(reader["preparation_amount"]))
                            prepWeight = reader.GetDouble("preparation_amount");
                        if (DB.IsValidField(reader["preparation_unit_name"]))
                            prepWeightUnit = reader.GetString("preparation_unit_name");
                        else prepWeightUnit = "";
                        if (DB.IsValidField(reader["preparation_quantity"]))
                            prepQuant = reader.GetDouble("preparation_quantity");
                        if (DB.IsValidField(reader["preparation_quantity_unit"]))
                            prepQuantUnit = reader.GetString("preparation_quantity_unit");
                        else prepQuantUnit = "";
                        if (DB.IsValidField(reader["station_name"]))
                            station = reader.GetString("station_name");
                        else station = "";

                        printDocument.Print();
                    }
                }                
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

            mSettings.LabelPrinterName = cboxPrinters.Text;
            mSettings.LabelPrinterPaperName = paperSize.PaperName;
            mSettings.LabelPrinterLandscape = cbLandscape.Checked;

            DialogResult = DialogResult.Cancel;
            Close();
        }        

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString("ID: " + sampleNumber + "/" + prepNumber, fontLabel, Brushes.Black, 2, 1);
            e.Graphics.DrawString("Lab: " + laboratory, fontLabel, Brushes.Black, 120, 1);
            e.Graphics.DrawString("Project: " + projectMain + " - " + projectSub, fontLabel, Brushes.Black, 2, 12);
            e.Graphics.DrawString("Sample type: " + sampleType, fontLabel, Brushes.Black, 2, 23);            
            e.Graphics.DrawString("Ref.date: " + refDate, fontLabel, Brushes.Black, 2, 34);
            e.Graphics.DrawString("Station: " + station, fontLabel, Brushes.Black, 160, 34);
            string sampTime = samplingTimeFrom;
            sampTime += String.IsNullOrEmpty(samplingTimeTo) ? "" : ", " + samplingTimeTo;
            e.Graphics.DrawString("Sampling time: " + sampTime, fontLabel, Brushes.Black, 2, 45);                        
            e.Graphics.DrawString("Fill height(mm): " + (fillHeight > 0d ? fillHeight.ToString() : ""), fontLabel, Brushes.Black, 2, 56);
            e.Graphics.DrawString("Amount: " + (prepWeight > 0d ? prepWeight.ToString() + " " + prepWeightUnit : ""), fontLabel, Brushes.Black, 2, 67);
            e.Graphics.DrawString("Quantity: " + (prepQuant > 0d ? prepQuant.ToString() + " " + prepQuantUnit : ""), fontLabel, Brushes.Black, 2, 78);
            e.Graphics.DrawString("*" + sampleNumber + "*", fontBarcode, Brushes.Black, 130, 60);
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
    }
}
