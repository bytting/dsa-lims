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

            PaperSize paperSize = cboxPaperSizes.SelectedItem as PaperSize;            
            printDocument.DefaultPageSettings.Landscape = cbLandscape.Checked;            
            printDocument.DefaultPageSettings.PaperSize = paperSize;

            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.Print();

            mSettings.LabelPrinterName = cboxPrinters.Text;
            mSettings.LabelPrinterPaperName = paperSize.PaperName;
            mSettings.LabelPrinterLandscape = cbLandscape.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("Free 3 of 9", 36);
            Font font2 = new Font("Calibri", 11);
            
            e.Graphics.DrawString("ID: " + mSampleNumber, font2, Brushes.Black, 5, 1);
            e.Graphics.DrawString(mExternalSampleId, font2, Brushes.Black, 110, 1);
            e.Graphics.DrawString("Sample type: " + mSampleType, font2, Brushes.Black, 5, 15);
            e.Graphics.DrawString("Main project: " + mProjectMain, font2, Brushes.Black, 5, 30);
            e.Graphics.DrawString("Sub project: " + mProjectSub, font2, Brushes.Black, 5, 45);
            e.Graphics.DrawString("Laboratory: " + mLaboratory, font2, Brushes.Black, 5, 60);            
            e.Graphics.DrawString("*" + mSampleNumber + "*", font, Brushes.Black, 5, 80);
            /*if (samplePart != "1/1")
                e.Graphics.DrawString("Reps: " + samplePart, font2, Brushes.Black, 150, 1);

            int x = 140, y = 75, width = 150, height = 44;
            e.Graphics.DrawImage(Properties.Resources.logo, x, y, width, height);
            int u = 230, t = 5, width1 = 52, height1 = 46;
            e.Graphics.DrawImage(Properties.Resources.Symbol, u, t, width1, height1);*/
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
