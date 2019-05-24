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
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.IO;
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
    public static class UtilsPdf
    {
        public static void CropImageToHeight(PdfImage img, float height)
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

        private static PdfFont GetHeaderFont()
        {
            return new PdfFont(PdfFont.FontFamily.HELVETICA, 5f, PdfFont.BOLD, iTextSharp.text.BaseColor.BLACK);
        }

        private static PdfFont GetCellFont()
        {
            return new PdfFont(PdfFont.FontFamily.HELVETICA, 5f, PdfFont.NORMAL, iTextSharp.text.BaseColor.BLACK);
        }

        private static PdfPhrase GetHeaderPhrase(string text)
        {
            PdfPhrase p = new PdfPhrase(text, GetHeaderFont());
            return p;
        }

        private static PdfPhrase GetCellPhrase(string text)
        {
            PdfPhrase p = new PdfPhrase(text, GetCellFont());
            return p;
        }

        public static byte[] CreatePdfDataFromAssignment(SqlConnection conn, SqlTransaction trans, Guid assignmentId)
        {            
            string OrderName = "", LaboratoryName = "", ResponsibleName = "", CustomerName = "", CustomerCompany = "", CustomerAddress = "";
            PdfImage labLogo = null;
            
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_flat", CommandType.StoredProcedure, new SqlParameter("@id", assignmentId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    OrderName = reader.GetString("name");
                    LaboratoryName = reader.GetString("laboratory_name");
                    ResponsibleName = reader.GetString("account_name");
                    CustomerName = reader.GetString("customer_contact_name");
                    CustomerCompany = reader.GetString("customer_company_name");
                    CustomerAddress = reader.GetString("customer_contact_address");
                }
            }

            Guid labId = (Guid)DB.GetScalar(conn, trans, "select laboratory_id from assignment where id = @id", CommandType.Text, new SqlParameter("@id", assignmentId));

            if (Utils.IsValidGuid(labId))
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select laboratory_logo from laboratory where id = @id", CommandType.Text, new SqlParameter("@id", labId)))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (DB.IsValidField(reader["laboratory_logo"]))
                            labLogo = PdfImage.GetInstance((byte[])reader["laboratory_logo"]);
                    }
                }
            }

            byte[] pdfData = null;
            PdfDocument document = null;
            MemoryStream ms = null;

            try
            {
                ms = new MemoryStream();
                document = new PdfDocument();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                PdfContentByte cb = writer.DirectContent;

                cb.BeginText();

                PdfBaseFont baseFont = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ROMAN, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
                PdfBaseFont baseFontBold = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_BOLD, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
                PdfBaseFont baseFontItalic = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ITALIC, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
                float fontSize = 10, fontSizeHeader = 14;
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

                if (hasLogos)
                    topCursor -= labLogo.ScaledHeight;

                cb.SetFontAndSize(baseFontBold, fontSizeHeader);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "OPPDRAGSOVERSIKT", leftCursor, topCursor, 0);
                cb.SetFontAndSize(baseFont, fontSize);
                topCursor -= lineSpace * 2;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdrag: " + OrderName, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                string cust = CustomerName;
                if (!String.IsNullOrEmpty(CustomerCompany)) cust += ", " + CustomerCompany;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdragsgiver: " + cust, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, CustomerAddress, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Laboratorium/Kontaktperson: " + LaboratoryName + " / " + ResponsibleName, leftCursor, topCursor, 0);
                topCursor -= lineSpace * 4;
                cb.SetFontAndSize(baseFontBold, fontSizeHeader);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Måleresultater", leftCursor, topCursor, 0);
                cb.EndText();
                
                topCursor -= lineSpace;                

                PdfPTable table = new PdfPTable(12);
                table.TotalWidth = document.GetRight(margin) - document.GetLeft(margin);

                PdfPCell cell = new PdfPCell(GetHeaderPhrase("Analysis"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("P.Status"));                
                table.AddCell(cell);                
                cell = new PdfPCell(GetHeaderPhrase("A.Status"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Method"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Nuclide"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Activity"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Act.Unc."));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Act.Appr."));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("MDA"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("MDA Appr."));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Reportable"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Accredited"));                
                table.AddCell(cell);

                string query = @"
select 
    s.number as 'sample', 
	p.number as 'preparation', 
    p.workflow_status_id as 'preparation_wfstatus', 
	a.number as 'analysis', 	
    a.workflow_status_id as 'analysis_wfstatus', 
	am.name_short as 'analysis_method', 
	n.name as 'nuclide_name', 
	ar.activity as 'act', 
	ar.activity_uncertainty_abs as 'act.unc', 
	ar.activity_approved as 'act.appr', 
	ar.detection_limit as 'det.lim', 
	ar.detection_limit_approved as 'det.lim.appr', 
	ar.reportable, 
	ar.accredited
from sample s
    inner join preparation p on p.sample_id = s.id and p.instance_status_id <= 1
    inner join analysis a on a.preparation_id = p.id and a.instance_status_id <= 1 and a.assignment_id = @assignment_id
    inner join analysis_result ar on ar.analysis_id = a.id
    inner join analysis_method am on am.id = a.analysis_method_id
    inner join nuclide n on n.id = ar.nuclide_id
order by s.number, p.number, a.number
";
                int nRows = 1;                
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new[] {
                    new SqlParameter("@assignment_id", assignmentId)
                }))
                {
                    while (reader.Read())
                    {
                        int pwfstat = reader.GetInt32("preparation_wfstatus");
                        string spwfstat = "Unknown";
                        switch (pwfstat)
                        {
                            case WorkflowStatus.Construction:
                                spwfstat = "Construction";
                                break;
                            case WorkflowStatus.Complete:
                                spwfstat = "Complete";
                                break;
                            case WorkflowStatus.Rejected:
                                spwfstat = "Rejected";
                                break;
                        }

                        int awfstat = reader.GetInt32("analysis_wfstatus");
                        string sawfstat = "Unknown";
                        switch (awfstat)
                        {
                            case WorkflowStatus.Construction:
                                sawfstat = "Construction";
                                break;
                            case WorkflowStatus.Complete:
                                sawfstat = "Complete";
                                break;
                            case WorkflowStatus.Rejected:
                                sawfstat = "Rejected";
                                break;
                        }

                        string sact = "";
                        if(DB.IsValidField(reader["act"]))
                            sact = reader.GetDouble("act").ToString(Utils.ScientificFormat);

                        string sactunc = "";
                        if (DB.IsValidField(reader["act.unc"]))
                            sactunc = reader.GetDouble("act.unc").ToString(Utils.ScientificFormat);

                        string sdetlim = "";
                        if (DB.IsValidField(reader["det.lim"]))
                            sdetlim = reader.GetDouble("det.lim").ToString(Utils.ScientificFormat);

                        cell = new PdfPCell(GetCellPhrase(reader.GetString("sample") + "/" + reader.GetString("preparation") + "/" + reader.GetString("analysis")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(spwfstat));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(sawfstat));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("analysis_method")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("nuclide_name")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(sact));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(sactunc));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("act.appr")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(sdetlim));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("det.lim.appr")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("reportable")));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("accredited")));                        
                        table.AddCell(cell);
                        nRows++;
                    }
                }

                float currHeight = topCursor;
                int currRow = 0, pageRows = 0;

                for (int i = 0; i < nRows; i++)
                {
                    currHeight -= table.GetRowHeight(i);
                    pageRows++;

                    if (currHeight <= document.GetBottom(10f))
                    {
                        table.WriteSelectedRows(currRow, currRow + pageRows, document.GetLeft(10), topCursor, cb);
                        document.NewPage();

                        currRow += pageRows;
                        currHeight = topCursor = document.GetTop(10f);
                        pageRows = 0;
                    }
                }

                if (pageRows > 0)
                {
                    table.WriteSelectedRows(currRow, currRow + pageRows, document.GetLeft(10), topCursor, cb);
                    document.NewPage();
                }                
            }
            finally
            {
                document?.Close();
                if(ms != null)                
                    pdfData = ms.GetBuffer();
            }

            return pdfData;
        }        

        public static byte[] CreatePdfDataFromDataGridView(DataGridView grid, int startColumn, int endColumn, string title)
        {
            byte[] pdfData = null;
            PdfDocument document = null;
            MemoryStream ms = null;

            try
            {
                ms = new MemoryStream();
                document = new PdfDocument();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                PdfContentByte cb = writer.DirectContent;

                cb.BeginText();
                
                PdfBaseFont baseFontBold = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_BOLD, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
                float fontSizeHeader = 14f;
                float topCursor = document.GetTop(10f);

                cb.SetFontAndSize(baseFontBold, fontSizeHeader);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, title, document.GetLeft(10f), topCursor, 0);
                topCursor -= 20f;
                cb.EndText();

                int nCols = endColumn - startColumn;

                PdfPTable table = new PdfPTable(nCols);
                table.TotalWidth = document.GetRight(10) - document.GetLeft(10);

                for (int i = startColumn; i < endColumn; i++)
                {                    
                    PdfPCell cell = new PdfPCell(GetHeaderPhrase(grid.Columns[i].HeaderText));                    
                    cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                    cell.VerticalAlignment = PdfPCell.ALIGN_LEFT;
                    table.AddCell(cell);
                }

                for (int i = 0; i < grid.Rows.Count; i++)
                {
                    for (int j = startColumn; j < endColumn; j++)
                    {
                        PdfPCell cell = new PdfPCell(GetCellPhrase(grid.Rows[i].Cells[j].FormattedValue.ToString()));                    
                        cell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                        cell.VerticalAlignment = PdfPCell.ALIGN_LEFT;
                        table.AddCell(cell);
                    }
                }                

                float currHeight = topCursor;
                int currRow = 0, pageRows = 0, nRows = grid.Rows.Count + 1;

                for(int i=0; i<nRows; i++)
                {
                    currHeight -= table.GetRowHeight(i);                    
                    pageRows++;

                    if(currHeight <= document.GetBottom(10f))
                    {
                        table.WriteSelectedRows(currRow, currRow + pageRows, document.GetLeft(10), topCursor, cb);
                        document.NewPage();

                        currRow += pageRows;
                        currHeight = topCursor = document.GetTop(10f);
                        pageRows = 0;
                    }
                }

                if(pageRows > 0)
                {
                    table.WriteSelectedRows(currRow, currRow + pageRows, document.GetLeft(10), topCursor, cb);
                    document.NewPage();
                }
            }
            finally
            {
                document?.Close();
                if (ms != null)
                    pdfData = ms.GetBuffer();
            }

            return pdfData;
        }
    }
}
