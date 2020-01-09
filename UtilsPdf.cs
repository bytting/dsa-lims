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
using PdfImage = iTextSharp.text.Image;

namespace DSA_lims
{
    public static class UtilsPdf
    {
        private static PdfBaseFont baseFontTimes = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ROMAN, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
        private static PdfBaseFont baseFontTimesBold = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_BOLD, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
        private static PdfBaseFont baseFontTimesItalic = PdfBaseFont.CreateFont(PdfBaseFont.TIMES_ITALIC, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
        private static PdfBaseFont baseFontCourier = PdfBaseFont.CreateFont(PdfBaseFont.COURIER, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);
        private static PdfBaseFont baseFontCourierBold = PdfBaseFont.CreateFont(PdfBaseFont.COURIER_BOLD, PdfBaseFont.CP1252, PdfBaseFont.NOT_EMBEDDED);

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

        private static PdfImage GetLaboratoryLogo(SqlConnection conn, SqlTransaction trans, Guid laboratoryId)
        {
            PdfImage labLogo = null;

            if (Utils.IsValidGuid(laboratoryId))
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select laboratory_logo from laboratory where id = @id", CommandType.Text, new SqlParameter("@id", laboratoryId)))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();

                        if (DB.IsValidField(reader["laboratory_logo"]))
                            labLogo = PdfImage.GetInstance((byte[])reader["laboratory_logo"]);
                    }
                }
            }

            return labLogo;
        }

        public static byte[] CreatePdfDataFromAssignment(SqlConnection conn, SqlTransaction trans, Assignment assignment)
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
                
                float fontSize = 10, fontSizeHeader = 16;
                float margin = 50;
                float leftCursor = margin, topCursor = document.Top - 10f, lineSpace = 13;

                PdfImage labLogo = GetLaboratoryLogo(conn, trans, assignment.LaboratoryId);
                if (labLogo != null)
                {
                    CropImageToHeight(labLogo, 64f);                
                    labLogo.SetAbsolutePosition(document.GetRight(10f) - labLogo.PlainWidth, document.GetTop(10f) - 40f);
                    document.Add(labLogo);                    
                }

                cb.SetFontAndSize(baseFontTimesBold, fontSizeHeader);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "OPPDRAGSOVERSIKT", leftCursor, topCursor, 0);                
                cb.SetFontAndSize(baseFontTimes, fontSize);
                topCursor -= lineSpace * 2;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdragsnummer: " + assignment.Name, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdrag opprettet: " + assignment.CreateDate.ToString(Utils.DateFormatNorwegian), leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Utskriftsdato: " + DateTime.Now.ToString(Utils.DateFormatNorwegian), leftCursor, topCursor, 0);
                topCursor -= lineSpace * 2;
                cb.SetFontAndSize(baseFontTimesBold, fontSize);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdragsgiver, Firma/Avd.", leftCursor, topCursor, 0);
                cb.SetFontAndSize(baseFontTimes, fontSize);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Navn: " + assignment.CustomerCompanyName, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Addresse: " + assignment.CustomerCompanyAddress, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Epost: " + assignment.CustomerCompanyEmail, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Telefon: " + assignment.CustomerCompanyPhone, leftCursor, topCursor, 0);
                topCursor -= lineSpace * 2;
                cb.SetFontAndSize(baseFontTimesBold, fontSize);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Oppdragsgiver, Kontakt", leftCursor, topCursor, 0);
                cb.SetFontAndSize(baseFontTimes, fontSize);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Navn: " + assignment.CustomerContactName, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Addresse: " + assignment.CustomerContactAddress, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Epost: " + assignment.CustomerContactEmail, leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Telefon: " + assignment.CustomerContactPhone, leftCursor, topCursor, 0);
                topCursor -= lineSpace * 2;
                cb.SetFontAndSize(baseFontTimesBold, fontSize);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Laboratorium", leftCursor, topCursor, 0);
                cb.SetFontAndSize(baseFontTimes, fontSize);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Navn: " + assignment.LaboratoryName(conn, trans), leftCursor, topCursor, 0);
                topCursor -= lineSpace;
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Kontakt: " + assignment.ResponsibleName(conn, trans), leftCursor, topCursor, 0);
                topCursor -= lineSpace * 2;
                cb.SetFontAndSize(baseFontTimesBold, fontSize);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Måleresultater", leftCursor, topCursor, 0);
                cb.EndText();
                
                topCursor -= lineSpace;                

                PdfPTable table = new PdfPTable(13);
                table.TotalWidth = document.GetRight(margin) - document.GetLeft(margin);

                PdfPCell cell = new PdfPCell(GetHeaderPhrase("Analysis"));                
                table.AddCell(cell);
                cell = new PdfPCell(GetHeaderPhrase("Sample Type"));
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
    st.name as 'sample_type_name',
    sc.name as 'sample_component_name',
	p.number as 'preparation', 
    pm.name_short as 'preparation_method', 
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
    inner join sample_type st on st.id = s.sample_type_id
    left outer join sample_component sc on sc.id = s.sample_component_id
    inner join preparation p on p.sample_id = s.id and p.instance_status_id = 1
    inner join preparation_method pm on pm.id = p.preparation_method_id
    inner join analysis a on a.preparation_id = p.id and a.instance_status_id = 1 and a.assignment_id = @assignment_id
    inner join analysis_result ar on ar.analysis_id = a.id and ar.instance_status_id = 1 and ar.reportable = 1
    inner join analysis_method am on am.id = a.analysis_method_id
    inner join nuclide n on n.id = ar.nuclide_id
order by s.number, p.number, a.number
";
                int nRows = 1;                
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new[] {
                    new SqlParameter("@assignment_id", assignment.Id)
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

                        cell = new PdfPCell(GetCellPhrase(reader.GetString("sample") + " / " + reader.GetString("preparation") + " / " + reader.GetString("analysis")));                        
                        table.AddCell(cell);
                        string sampleType = reader.GetString("sample_type_name");
                        if(DB.IsValidField(reader["sample_component_name"]))
                            sampleType += " / " + reader.GetString("sample_component_name");
                        cell = new PdfPCell(GetCellPhrase(sampleType));
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(spwfstat));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(sawfstat));                        
                        table.AddCell(cell);
                        cell = new PdfPCell(GetCellPhrase(reader.GetString("preparation_method") + " / " + reader.GetString("analysis_method")));                        
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
                
                float fontSizeHeader = 14f;
                float topCursor = document.GetTop(10f);

                cb.SetFontAndSize(baseFontTimesBold, fontSizeHeader);
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

        private static void AddTextToParagraph(PdfParagraph p, string t, PdfFont f)
        {
            p.Add(new PdfChunk(t.Trim(new char[] { '\r', '\n' }), f));
            p.Add(PdfChunk.NEWLINE);
        }

        public static byte[] CreatePdfDefinitionFromAssignment(SqlConnection conn, SqlTransaction trans, Assignment a, TreeView t, string title)
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

                PdfImage labLogo = GetLaboratoryLogo(conn, trans, a.LaboratoryId);
                if (labLogo != null)
                {                                        
                    CropImageToHeight(labLogo, 64f);
                    labLogo.SetAbsolutePosition(document.GetRight(10f) - labLogo.PlainWidth, document.GetTop(10f) - 40f);
                    document.Add(labLogo);
                }
                
                PdfFont fontTimes10 = new PdfFont(baseFontTimes, 10f);
                PdfFont fontTimes12 = new PdfFont(baseFontTimes, 12f);
                PdfFont fontTimesBold12 = new PdfFont(baseFontTimesBold, 12f);
                PdfFont fontTimesBold20 = new PdfFont(baseFontTimesBold, 20f);
                PdfFont fontTimesItalic8 = new PdfFont(baseFontTimesItalic, 8f);
                PdfFont fontTimesItalic10 = new PdfFont(baseFontTimesItalic, 10f);
                PdfFont fontCourier10 = new PdfFont(baseFontCourier, 10f);
                PdfFont fontCourierBold10 = new PdfFont(baseFontCourierBold, 10f);

                PdfParagraph p = new PdfParagraph();
                p.SetLeading(16f, 0f);                
                AddTextToParagraph(p, title, fontTimesBold20);
                AddTextToParagraph(p, "Oversikt generert: " + DateTime.Now.ToString(Utils.DateTimeFormatNorwegian), fontTimes10);
                AddTextToParagraph(p, "", fontTimes10);
                AddTextToParagraph(p, "Ordre: " + a.Name, fontTimes12);
                AddTextToParagraph(p, "Beskrivelse: " + a.Description, fontTimes12);
                AddTextToParagraph(p, "Laboratorium: " + a.LaboratoryName(conn, trans), fontTimes12);
                AddTextToParagraph(p, "Ansvarlig: " + a.ResponsibleName(conn, trans), fontTimes12);
                AddTextToParagraph(p, "Oppdragsgiver: " + a.CustomerContactName, fontTimes12);
                AddTextToParagraph(p, "Tidsfrist: " + a.Deadline.Value.ToString(Utils.DateFormatNorwegian), fontTimes12);
                if(a.RequestedSigmaAct == 0)
                    AddTextToParagraph(p, "Ønsket sigma aktivitet: ", fontTimes12);
                else
                    AddTextToParagraph(p, "Ønsket sigma aktivitet: " + a.RequestedSigmaAct, fontTimes12);
                if(a.RequestedSigmaMDA == 0)
                    AddTextToParagraph(p, "Ønsket sigma usikkerhet: ", fontTimes12);
                else
                    AddTextToParagraph(p, "Ønsket sigma usikkerhet: " + a.RequestedSigmaMDA, fontTimes12);
                AddTextToParagraph(p, "", fontTimes10);
                document.Add(p);

                if (!String.IsNullOrEmpty(a.ContentComment))
                {
                    p = new PdfParagraph();
                    p.SetLeading(10f, 0f);

                    AddTextToParagraph(p, "Kommentar:", fontTimesBold12);
                    AddTextToParagraph(p, "", fontTimes10);
                    
                    string[] lines = a.ContentComment.Split(new char[] { '\n' });
                    foreach (string line in lines)
                        AddTextToParagraph(p, line, fontTimesItalic10);
                    AddTextToParagraph(p, "", fontTimesItalic10);
                    document.Add(p);
                }

                p = new PdfParagraph();
                p.SetLeading(14f, 0f);

                AddTextToParagraph(p, "Ordre enheter:", fontTimesBold12);
                AddTextToParagraph(p, "", fontTimes10);

                foreach (TreeNode n in t.Nodes)
                {
                    AddTextToParagraph(p, n.Text, fontCourierBold10);
                    foreach (TreeNode n2 in n.Nodes)
                    {
                        AddTextToParagraph(p, "      " + n2.Text, fontCourier10);
                        foreach (TreeNode n3 in n2.Nodes)
                        {
                            AddTextToParagraph(p, "            " + n3.Text, fontCourier10);
                        }
                    }
                }
                document.Add(p);

                p = new PdfParagraph();
                AddTextToParagraph(p, "", fontTimes10);
                AddTextToParagraph(p, "Merknad: Analysemetoder merket med A har en prøvetype, prepararingsmetode og analysemetode som kan resultere i akkrediterte resultater", fontTimesItalic8);
                document.Add(p);
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
