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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using PdfDocument = iTextSharp.text.Document;
using PdfWriter = iTextSharp.text.pdf.PdfWriter;
using PdfContentByte = iTextSharp.text.pdf.PdfContentByte;
using PdfBaseFont = iTextSharp.text.pdf.BaseFont;
using PdfFont = iTextSharp.text.Font;
using PdfPhrase = iTextSharp.text.Phrase;
using PdfElement = iTextSharp.text.Element;
using PdfPTable = iTextSharp.text.pdf.PdfPTable;
using PdfColumnText = iTextSharp.text.pdf.ColumnText;
using PdfPCell = iTextSharp.text.pdf.PdfPCell;
using PdfParagraph = iTextSharp.text.Paragraph;
using PdfChunk = iTextSharp.text.Chunk;
using PdfFontFactory = iTextSharp.text.FontFactory;
using PdfImage = iTextSharp.text.Image;

namespace DSA_lims
{
    public partial class FormCreateOrderReport : Form
    {
        private Guid mAssignmentId = Guid.Empty;
        private string OrderName, LaboratoryName, ResponsibleName, CustomerName, CustomerCompany, CustomerAddress;
        private PdfImage labLogo = null, accredLogo = null;

        public FormCreateOrderReport(Guid assignmentId)
        {
            InitializeComponent();

            mAssignmentId = assignmentId;
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_assignment_informative", CommandType.StoredProcedure, new SqlParameter("@id", mAssignmentId)))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        OrderName = reader["name"].ToString();
                        LaboratoryName = reader["laboratory_name"].ToString();
                        ResponsibleName = reader["responsible_name"].ToString();
                        CustomerName = reader["customer_name"].ToString();
                        CustomerCompany = reader["customer_company"].ToString();
                        CustomerAddress = reader["customer_address"].ToString();
                    }
                }

                Guid labId = (Guid)DB.GetScalar(conn, "select laboratory_id from assignment where id = @id", CommandType.Text, new SqlParameter("@id", mAssignmentId));

                using (SqlDataReader reader = DB.GetDataReader(conn, "select laboratory_logo, accredited_logo from laboratory where id = @id", CommandType.Text, new SqlParameter("@id", labId)))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (reader["laboratory_logo"] != null && reader["laboratory_logo"] != DBNull.Value)
                            labLogo = PdfImage.GetInstance((byte[])reader["laboratory_logo"]);

                        if (reader["accredited_logo"] != null && reader["accredited_logo"] != DBNull.Value)
                            accredLogo = PdfImage.GetInstance((byte[])reader["accredited_logo"]);
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CropImageToHeight(PdfImage img, float height)
        {
            if (img.Height <= height)
                return;

            float w = img.Width;
            float h = img.Height;
            float scaleFactor = height / h;
            w = w * scaleFactor;
            h = h * scaleFactor;
            img.ScaleAbsolute(w, h);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            PdfDocument document = new PdfDocument();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            document.Open();            

            PdfContentByte cb = writer.DirectContent;

            cb.BeginText();

            PdfBaseFont baseFont = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ROMAN, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
            PdfBaseFont baseFontBold = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_BOLD, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
            PdfBaseFont baseFontItalic = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ITALIC, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
            float fontSize = 11, fontSizeSmall = 9, fontSizeTiny = 7, fontSizeHeader = 13;
            float margin = 50;
            float leftCursor = margin, topCursor = document.Top - margin, lineSpace = 13;
            bool hasLogos = false;

            if (labLogo != null)
            {
                CropImageToHeight(labLogo, 64f);
                labLogo.SetAbsolutePosition(leftCursor, topCursor);
                document.Add(labLogo);
                hasLogos = true;                
            }

            if (accredLogo != null)
            {
                CropImageToHeight(accredLogo, 64f);
                accredLogo.SetAbsolutePosition(document.PageSize.Width - accredLogo.ScaledWidth - margin, topCursor);
                document.Add(accredLogo);
                hasLogos = true;
            }

            if(hasLogos)
                topCursor -= labLogo.ScaledHeight;

            cb.SetFontAndSize(baseFont, fontSizeHeader);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "FORENKLET MÅLERAPPORT", leftCursor, topCursor, 0);
            cb.SetFontAndSize(baseFont, fontSize);
            topCursor -= lineSpace * 2;
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Henvisning til eventuell endringsrapport, Rev.nr.:", leftCursor, topCursor, 0);
            topCursor -= lineSpace * 2;
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdrag: " + OrderName, leftCursor, topCursor, 0);
            topCursor -= lineSpace;
            string cust = CustomerName;
            if (!String.IsNullOrEmpty(CustomerCompany)) cust += ", " + CustomerCompany;
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdragsgiver: " + cust, leftCursor, topCursor, 0);
            topCursor -= lineSpace;
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, CustomerAddress, leftCursor, topCursor, 0);
            topCursor -= lineSpace;
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Laboratorie/Kontaktperson: " + LaboratoryName + " / " + ResponsibleName, leftCursor, topCursor, 0);            
            topCursor -= lineSpace * 6;
            cb.SetFontAndSize(baseFontBold, fontSizeHeader);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Måleresultater", leftCursor, topCursor, 0);
            cb.EndText();

            topCursor -= lineSpace;
            
            PdfPTable table = new PdfPTable(6);
            table.TotalWidth = document.GetRight(margin) - document.GetLeft(margin);

            string query = @"
select s.number as 'sample', p.number as 'preparation', a.number as 'analysis', am.name as 'analysis_method', n.name as 'nuclide', ar.activity
from sample s
    inner join preparation p on p.sample_id = s.id
    inner join analysis a on a.preparation_id = p.id and a.assignment_id = @assignment_id
    inner join analysis_result ar on ar.analysis_id = a.id and ar.reportable = 1
    inner join analysis_method am on am.id = a.analysis_method_id
    inner join nuclide n on n.id = ar.nuclide_id
order by s.number, p.number, a.number
";
            int nRows = 0;
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new[] {
                    new SqlParameter("@assignment_id", mAssignmentId)
                }))
                {                    
                    while (reader.Read())
                    {
                        table.AddCell(reader["sample"].ToString());
                        table.AddCell(reader["preparation"].ToString());
                        table.AddCell(reader["analysis"].ToString());
                        table.AddCell(reader["analysis_method"].ToString());
                        table.AddCell(reader["nuclide"].ToString());
                        table.AddCell(reader["activity"].ToString());
                        nRows++;
                    }
                }
            }

            //PdfPCell cell = new PdfPCell(new PdfPhrase("Måleresultater"));
            //cell.Colspan = 3;
            //cell.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            //table.AddCell(cell);                
            //document.Add(table);            
            float rowHeight = table.GetRowHeight(0);
            int currRow = 0;
            while (true)
            {                
                int pageRows = (int)((topCursor - document.Bottom) / rowHeight);

                pageRows = (nRows > pageRows) ? pageRows : nRows;             
                table.WriteSelectedRows(currRow, currRow + pageRows, leftCursor, topCursor, cb);
                nRows -= pageRows;
                currRow += pageRows;
                if (nRows <= 0)
                    break;

                document.NewPage();
                topCursor = document.Top - margin;
            }

            document.Close();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "PDF files (*.pdf)|*.pdf";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(dialog.FileName, ms.GetBuffer());

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
