using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NTwain;
using NTwain.Data;
using System.Reflection;
using System.Drawing.Imaging;
using tsText = iTextSharp.text;
using tsPdf = iTextSharp.text.pdf;

namespace DSA_lims
{
    public partial class FormScan : Form
    {
        DSASettings mSettings = null;
        TwainSession session = null;
        DataSource dataSource = null;
        List<Image> images = new List<Image>();

        public byte[] PdfData = null;
        public string DocumentName = String.Empty;

        public FormScan(DSASettings s)
        {
            InitializeComponent();

            mSettings = s;
        }        

        private void FormScan_Load(object sender, EventArgs e)
        {            
            var appId = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly());
            session = new TwainSession(appId);
            session.TransferReady += Session_TransferReady;
            session.DataTransferred += Session_DataTransferred;
            session.TransferError += Session_TransferError;
            session.SourceDisabled += Session_SourceDisabled;
            session.Open();

            foreach (DataSource ds in session)
                cboxScanner.Items.Add(ds.Name);

            cboxScanner.Text = mSettings.ScannerName;
            cbDuplex.Checked = mSettings.ScannerDuplex;
            cboxFlipType.Text = mSettings.ScannerFlipType;
            cboxPixelType.Text = mSettings.ScannerPixelType;        
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cboxScanner.Text))
            {
                MessageBox.Show("You must set a scanner");
                return;
            }

            if (String.IsNullOrEmpty(cboxFlipType.Text))
            {
                MessageBox.Show("You must set a orientation");
                return;
            }

            if (String.IsNullOrEmpty(cboxPixelType.Text))
            {
                MessageBox.Show("You must set a color");
                return;
            }

            if (String.IsNullOrEmpty(tbFileName.Text.Trim()))
            {
                MessageBox.Show("You must set a document name");
                return;
            }

            try
            {                
                DocumentName = tbFileName.Text.Trim();
                btnPreview.Enabled = false;
                btnStore.Enabled = false;
                PdfData = null;
                images.Clear();

                if (dataSource != null && dataSource.IsOpen)
                    dataSource.Close();
                dataSource = session.FirstOrDefault(x => x.Name == cboxScanner.Text);
                dataSource.Open();
                dataSource.Capabilities.CapFeederEnabled.SetValue(BoolType.True);
                dataSource.Capabilities.CapDuplexEnabled.SetValue(cbDuplex.Checked ? BoolType.True : BoolType.False);
                dataSource.Capabilities.ICapSupportedSizes.SetValue(SupportedSize.None);
                dataSource.Capabilities.ICapFlipRotation.SetValue(cboxFlipType.Text == "Fanfold" ? FlipRotation.Fanfold : FlipRotation.Book);
                dataSource.Capabilities.ICapPixelType.SetValue(cboxPixelType.Text == "Black and White" ? PixelType.BlackWhite : PixelType.RGB);
                dataSource.Capabilities.ICapImageFileFormat.SetValue(FileFormat.Png);

                dataSource.Enable(SourceEnableMode.NoUI, true, this.Handle);

                mSettings.ScannerName = cboxScanner.Text;
                mSettings.ScannerDuplex = cbDuplex.Checked;
                mSettings.ScannerFlipType = cboxFlipType.Text;
                mSettings.ScannerPixelType = cboxPixelType.Text;
            }
            catch
            {
                Common.Log.Error("Scanning of document failed on scanner " + cboxScanner.Text);
            }
        }

        private void Session_SourceDisabled(object sender, EventArgs e)
        {
            try
            {
                var document = new tsText.Document(tsText.PageSize.A4);
                var output = new MemoryStream();
                var writer = tsPdf.PdfWriter.GetInstance(document, output);
                document.Open();
                foreach (Image img in images)
                {
                    var pdfImg = tsText.Image.GetInstance(img, ImageFormat.Png);
                    pdfImg.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
                    pdfImg.SpacingBefore = 0f;
                    pdfImg.SpacingAfter = 0f;
                    pdfImg.Alignment = tsText.Element.ALIGN_LEFT;
                    pdfImg.SetAbsolutePosition(0, 0);
                    document.Add(pdfImg);
                    document.NewPage();
                }

                document.Close();

                PdfData = output.ToArray();

                btnPreview.Invoke((MethodInvoker)delegate
                {
                    btnPreview.Enabled = true;
                });

                btnStore.Invoke((MethodInvoker)delegate
                {
                    btnStore.Enabled = true;
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Session_TransferError(object sender, TransferErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void Session_DataTransferred(object sender, DataTransferredEventArgs e)
        {
            Image img = Image.FromStream(e.GetNativeImageStream());

            images.Add(img);
        }

        private void Session_TransferReady(object sender, TransferReadyEventArgs e)
        {            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormScan_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (dataSource != null && dataSource.IsOpen)            
                dataSource.Close();            

            if (session != null)
                session.Close();            
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (PdfData != null)
            {
                try
                {
                    string p = Path.GetTempFileName() + ".pdf";
                    File.WriteAllBytes(p, PdfData);
                    System.Diagnostics.Process.Start(p);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnStore_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
