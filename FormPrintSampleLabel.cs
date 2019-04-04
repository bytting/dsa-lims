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
    public partial class FormPrintSampleLabel : Form
    {
        PrivateFontCollection privateFonts = new PrivateFontCollection();
        Font fontBarcode = null;
        Font fontLabel = null;

        DSASettings mSettings = null;
        List<Guid> mSampleIds = new List<Guid>();

        string sampleNumber;
        string externalSampleId;
        string projectMain;
        string projectSub;
        string laboratory;
        string sampleType;
        string samplePart;

        PrintDocument printDocument = new PrintDocument();

        public FormPrintSampleLabel(DSASettings s, List<Guid> sampleIds)
        {
            InitializeComponent();

            mSampleIds = sampleIds;

            tbCopies.Text = "1";            
            tbCopies.KeyPress += CustomEvents.Integer_KeyPress;

            tbReplications.Text = "1";
            tbReplications.KeyPress += CustomEvents.Integer_KeyPress;

            mSettings = s;

            cboxPaperSizes.DisplayMember = "PaperName";
        }

        private void FormPrintSampleLabel_Load(object sender, EventArgs e)
        {
            fontLabel = new Font("Arial", 10);

            string InstallationDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            string fontFileName = InstallationDirectory + Path.DirectorySeparatorChar + "free3of9.ttf";
            if (File.Exists(fontFileName))
            {
                privateFonts.AddFontFile(InstallationDirectory + Path.DirectorySeparatorChar + "free3of9.ttf");
                fontBarcode = new Font(privateFonts.Families[0], 38, FontStyle.Regular);
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

            if (String.IsNullOrEmpty(tbReplications.Text))
            {
                MessageBox.Show("Number of repliactions must be a positive number");
                return;
            }            

            int reps = Convert.ToInt32(tbReplications.Text);
            if(reps < 1)
            {
                MessageBox.Show("Number of repliactions must be a positive number");
                return;
            }

            PaperSize paperSize = cboxPaperSizes.SelectedItem as PaperSize;            
            printDocument.DefaultPageSettings.Landscape = cbLandscape.Checked;            
            printDocument.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintPage += PrintDocument_PrintPage;

            string query = @"
select 
    s.number as 'sample_number', 
    s.external_id as 'external_id',
    st.name as 'sample_type_name',
    pm.name as 'project_main_name',
    ps.name as 'project_sub_name',
    l.name as 'laboratory_name'
from sample s
    inner join sample_type st on s.sample_type_id = st.id    
    inner join project_sub ps on s.project_sub_id = ps.id
    inner join project_main pm on ps.project_main_id = pm.id
    inner join laboratory l on s.laboratory_id = l.id
where s.id = @id
";

            using (SqlConnection conn = DB.OpenConnection())
            {
                foreach (Guid sid in mSampleIds)
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, new SqlParameter("@id", sid)))
                    {
                        if (!reader.HasRows)
                            continue;

                        reader.Read();

                        sampleNumber = reader.GetString("sample_number");
                        externalSampleId = reader.GetString("external_id");
                        sampleType = reader.GetString("sample_type_name");
                        projectMain = reader.GetString("project_main_name");
                        projectSub = reader.GetString("project_sub_name");
                        laboratory = reader.GetString("laboratory_name");

                        for (int c = 0; c < copies; c++)
                        {
                            for (int r = 1; r <= reps; r++)
                            {
                                samplePart = r.ToString() + "/" + reps.ToString();
                                printDocument.Print();
                            }
                        }
                    }
                }
            }

            mSettings.LabelPrinterName = cboxPrinters.Text;
            mSettings.LabelPrinterPaperName = paperSize.PaperName;
            mSettings.LabelPrinterLandscape = cbLandscape.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {

            e.Graphics.DrawString("ID: " + sampleNumber, fontLabel, Brushes.Black, 2, 1);
            e.Graphics.DrawString("Ex.ID: " + externalSampleId, fontLabel, Brushes.Black, 120, 1);            
            e.Graphics.DrawString("Sample type: " + sampleType, fontLabel, Brushes.Black, 2, 15);
            e.Graphics.DrawString("Main project: " + projectMain, fontLabel, Brushes.Black, 2, 30);
            e.Graphics.DrawString("Sub project: " + projectSub, fontLabel, Brushes.Black, 2, 45);
            e.Graphics.DrawString("Laboratory: " + laboratory, fontLabel, Brushes.Black, 2, 60);
            e.Graphics.DrawString("Batch: " + samplePart, fontLabel, Brushes.Black, 220, 60);
            e.Graphics.DrawString("*" + sampleNumber + "*", fontBarcode, Brushes.Black, 2, 80);

            if (Common.LabLogo != null)
            {
                Image img = UtilsMedia.CropImageToHeight(Common.LabLogo, 40);
                e.Graphics.DrawImage(img, 210f, 80f);
            }
        }

        private void cboxPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cboxPrinters.Text))
                return;

            cboxPaperSizes.Items.Clear();
            
            printDocument.PrinterSettings.PrinterName = cboxPrinters.Text;

            foreach(PaperSize ps in printDocument.PrinterSettings.PaperSizes)
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
