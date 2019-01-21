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
using System.Threading.Tasks;
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

        string sampleNumber, prepNumber, sampleType, projectMain, projectSub, refDate, laboratory, fillHeight, prepWeight, prepWeightUnit;
        string prepQuant, prepQuantUnit, samplingTimeFrom, samplingTimeTo;

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
    qu.name as 'preparation_quantity_unit'
from preparation p
    inner join sample s on p.sample_id = s.id
    inner join sample_type st on s.sample_type_id = st.id    
    inner join project_sub ps on s.project_sub_id = ps.id
    inner join project_main pm on ps.project_main_id = pm.id
    inner join laboratory l on s.laboratory_id = l.id
    inner join preparation_unit pu on pu.id = p.prep_unit_id
    inner join quantity_unit qu on qu.id = p.quantity_unit_id
where p.id = @pid
";

            using (SqlConnection conn = DB.OpenConnection())
            {
                foreach (Guid pid in mPrepIds)
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, new SqlParameter("@pid", pid)))
                    {
                        if (!reader.HasRows)
                            continue;

                        reader.Read();

                        sampleNumber = reader["sample_number"].ToString();
                        if (DB.IsValidField(reader["sampling_date_from"]))
                            samplingTimeFrom = Convert.ToDateTime(reader["sampling_date_from"]).ToString(Utils.DateTimeFormatNorwegian);
                        else samplingTimeFrom = "";
                        if (DB.IsValidField(reader["sampling_date_to"]))
                            samplingTimeTo = Convert.ToDateTime(reader["sampling_date_to"]).ToString(Utils.DateTimeFormatNorwegian);
                        else samplingTimeTo = "";
                        prepNumber = reader["preparation_number"].ToString();                        
                        sampleType = reader["sample_type_name"].ToString();
                        projectMain = reader["project_main_name"].ToString();
                        projectSub = reader["project_sub_name"].ToString();
                        refDate = Convert.ToDateTime(reader["reference_date"]).ToString(Utils.DateTimeFormatNorwegian);
                        laboratory = reader["laboratory_name"].ToString();
                        fillHeight = reader["fill_height"].ToString();
                        prepWeight = reader["preparation_amount"].ToString();
                        prepWeightUnit = reader["preparation_unit_name"].ToString();
                        prepQuant = reader["preparation_quantity"].ToString();
                        prepQuantUnit = reader["preparation_quantity_unit"].ToString();

                        printDocument.Print();                            
                    }
                }
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
            e.Graphics.DrawString("Lab: " + laboratory, fontLabel, Brushes.Black, 140, 1);
            e.Graphics.DrawString("Project: " + projectMain + " - " + projectSub, fontLabel, Brushes.Black, 2, 12);
            e.Graphics.DrawString("Sample type: " + sampleType, fontLabel, Brushes.Black, 2, 23);            
            e.Graphics.DrawString("Ref.date: " + refDate, fontLabel, Brushes.Black, 2, 34);
            string sampTime = samplingTimeFrom;
            sampTime += String.IsNullOrEmpty(samplingTimeTo) ? "" : ", " + samplingTimeTo;
            e.Graphics.DrawString("Sampling time: " + sampTime, fontLabel, Brushes.Black, 2, 45);                        
            e.Graphics.DrawString("Fill height(mm): " + fillHeight, fontLabel, Brushes.Black, 2, 56);
            e.Graphics.DrawString("Amount: " + prepWeight + " " + prepWeightUnit, fontLabel, Brushes.Black, 2, 67);
            e.Graphics.DrawString("Quantity: " + prepQuant + " " + prepQuantUnit, fontLabel, Brushes.Black, 2, 78);
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
