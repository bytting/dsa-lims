using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrintSampleLabel : Form
    {
        DSASettings mSettings = null;
        string mSampleNumber;
        string mExternalSampleId;
        string mProjectMain;
        string mProjectSub;
        string mLaboratory;
        string mSampleType;

        string samplePart;

        PrintDocument printDocument = new PrintDocument();

        public FormPrintSampleLabel(
            DSASettings s, 
            string sampleNumber, 
            string externalSampleId, 
            string projectMain, 
            string projectSub, 
            string laboratory, 
            string sampleType)
        {
            InitializeComponent();

            tbCopies.Text = "1";            
            tbCopies.KeyPress += CustomEvents.Integer_KeyPress;

            tbReplications.Text = "1";
            tbReplications.KeyPress += CustomEvents.Integer_KeyPress;

            mSettings = s;
            mSampleNumber = sampleNumber;
            mExternalSampleId = externalSampleId;
            mProjectMain = projectMain;
            mProjectSub = projectSub;
            mLaboratory = laboratory;
            mSampleType = sampleType;

            cboxPaperSizes.DisplayMember = "PaperName";
        }

        private void FormPrintSampleLabel_Load(object sender, EventArgs e)
        {
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
            for (int c = 0; c < copies; c++)
            {
                for (int r = 1; r <= reps; r++)
                {                    
                    samplePart = r.ToString() + "/" + reps.ToString();
                    printDocument.Print();
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
            Font font = new Font("Free 3 of 9", 38);
            Font font2 = new Font("Arial", 11);
            
            e.Graphics.DrawString("ID: " + mSampleNumber, font2, Brushes.Black, 5, 1);
            e.Graphics.DrawString(mExternalSampleId, font2, Brushes.Black, 90, 1);            
            e.Graphics.DrawString("Sample type: " + mSampleType, font2, Brushes.Black, 5, 15);
            e.Graphics.DrawString("Main project: " + mProjectMain, font2, Brushes.Black, 5, 30);
            e.Graphics.DrawString("Sub project: " + mProjectSub, font2, Brushes.Black, 5, 45);
            e.Graphics.DrawString("Laboratory: " + mLaboratory, font2, Brushes.Black, 5, 60);
            e.Graphics.DrawString("Batch: " + samplePart, font2, Brushes.Black, 220, 60);
            e.Graphics.DrawString("*" + mSampleNumber + "*", font, Brushes.Black, 5, 80);

            Image img = Properties.Resources.dsa_logo_64;            
            e.Graphics.DrawImage(img, 150f, 80f, (float)img.Width / 1.2f, (float)img.Height/1.2f);
            //int u = 230, t = 5, width1 = 52, height1 = 46;
            //e.Graphics.DrawImage(Properties.Resources.Symbol, u, t, width1, height1);
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
