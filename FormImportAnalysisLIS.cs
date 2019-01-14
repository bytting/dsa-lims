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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DSA_lims
{
    public partial class FormImportAnalysisLIS : Form
    {
        private AnalysisParameters parameters = null;
        private AnalysisResult result = null;

        public FormImportAnalysisLIS(AnalysisParameters analysisParameters, AnalysisResult analysisResult)
        {
            InitializeComponent();

            parameters = analysisParameters;
            result = analysisResult;
        }

        private void FormImportAnalysisLIS_Load(object sender, EventArgs e)
        {
            try
            {
                tbFilename.Text = parameters.FileName;
                tbLIMSSampleName.Text = parameters.SampleName;
                tbLIMSPrepGeom.Text = parameters.PreparationGeometry;
                tbLIMSGeomFillHeight.Text = parameters.PreparationFillHeight.ToString();
                tbLIMSGeomAmount.Text = parameters.PreparationAmount.ToString();
                tbLIMSGeomQuantity.Text = parameters.PreparationQuantity.ToString();

                LoadLIS(parameters.FileName);
                foreach(AnalysisResult.Isotop isotop in result.Isotopes)
                {
                    if (isotop.MDA == 0.0)
                        isotop.ApprovedMDA = false;
                }
                
                PopulateUI();
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ClearUI()
        {
            tbNuclideLibrary.Text = "";
            tbDetLimLib.Text = "";

            grid.Rows.Clear();
        }

        private void PopulateUI()
        {
            ClearUI();
            
            tbNuclideLibrary.Text = result.NuclideLibrary;
            tbDetLimLib.Text = result.DetLimLib;

            grid.DataSource = result.Isotopes;

            grid.Columns["NuclideName"].ReadOnly = true;
            grid.Columns["ConfidenceValue"].ReadOnly = true;
            grid.Columns["Activity"].ReadOnly = true;
            grid.Columns["Uncertainty"].ReadOnly = true;
            grid.Columns["MDA"].ReadOnly = true;

            grid.Columns["Uncertainty"].HeaderText = "Uncertainty (2σ)";
            grid.Columns["MDA"].HeaderText = "MDA (95%)";

            foreach(DataGridViewRow row in grid.Rows)
            {
                string nuclName = row.Cells["NuclideName"].Value.ToString().ToUpper();
                if (!parameters.AllNuclides.Contains(nuclName))
                {
                    row.DefaultCellStyle.BackColor = Color.Tomato;
                    foreach (DataGridViewCell cell in row.Cells)
                        cell.ReadOnly = true;
                }
                else if (!parameters.AnalMethNuclides.Contains(nuclName))
                {
                    row.DefaultCellStyle.BackColor = Color.Yellow;
                    foreach (DataGridViewCell cell in row.Cells)
                        cell.ReadOnly = true;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
            }
        }

        private int GetFirstPositiveNumber(string line)
        {
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(line);
            if (match.Success)
                return Convert.ToInt32(match.Value);
            return -1;
        }

        private void LoadLIS(string filename)
        {
            char[] splitColon = { ':' };
            char[] splitBlank = { ' ' };
            
            TextReader reader = File.OpenText(filename);
            string line;            
            result.Clear();
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Contains("SPECTRUM NO"))
                {
                    string[] items = line.Split(splitColon);
                    result.SpectrumName = items[1].Replace("\t", "").Trim();
                    result.SpectrumName = items[1].Replace(" ", "");
                }

                if (line.Contains("SAMPLE IDENTITY"))
                {
                    string[] items = line.Split(splitColon);
                    result.SampleName = items[1].Trim();                    
                }

                if (line.Contains("SAMPLE PLACE"))
                {
                    string[] items = line.Split(splitColon);
                    result.SamplePlace = items[1].Trim();
                }

                if (line.Contains("SAMPLE QUANTITY"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);
                    result.SampleQuantity = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
                    result.Unit = unitpart[1];
                }

                if (line.Contains("BEAKER NO"))
                {
                    string[] items = line.Split(splitColon);
                    result.Geometry = items[1].Trim();
                }
                if (line.Contains("SAMPLE HEIGHT"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);
                    result.Height = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
                }
                if (line.Contains("SAMPLE WEIGHT"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);
                    result.Weight = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
                }

                if (line.Contains("REFERENCE TIME"))
                {
                    string[] items = line.Split(splitColon);
                    string rf = items[1].Trim();
                    string[] timeparts = rf.Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
                    int year = Convert.ToInt32(timeparts[0].Trim());
                    int month = Convert.ToInt32(timeparts[1].Trim());
                    int day = Convert.ToInt32(timeparts[2].Trim());
                    int hour = Convert.ToInt32(timeparts[3].Trim());
                    int min = Convert.ToInt32(timeparts[4].Trim());

                    DateTime refdate = new DateTime(year, month, day, hour, min, 00);
                    result.ReferenceTime = refdate.ToString();
                }
                if (line.Contains("NUCLIDE LIBRARY"))
                {
                    string[] items = line.Split(splitColon);
                    result.NuclideLibrary = items[1].Trim();
                }
                if (line.Contains("DETECTION LIMIT LIB"))
                {
                    string[] items = line.Split(splitColon);
                    result.DetLimLib = items[1].Trim();
                }
                if (line.Contains("THE FOLLOWING ISOTOPES WERE IDENTIFIED"))
                {
                    ParseResults(reader);
                }

                if (line.Contains("Detection limits"))
                {
                    ParseMDA(reader);
                }
            }
            reader.Close();
        }

        private void ParseResults(TextReader reader)
        {
            string line;
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("THE FOLLOWING PEAKS WERE NOT IDENTIFIED"))
                    return;

                if (line.StartsWith("NR"))
                {
                    result.SigmaAct = GetFirstPositiveNumber(line);
                }

                string[] items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length == 5)
                {
                    AnalysisResult.Isotop isotop = new AnalysisResult.Isotop();
                    isotop.ApprovedMDA = true;
                    isotop.NuclideName = items[1].Trim().ToUpper();
                    isotop.ConfidenceValue = Convert.ToDouble(items[2].Trim(), CultureInfo.InvariantCulture);
                    isotop.Activity = Convert.ToDouble(items[3].Trim(), CultureInfo.InvariantCulture);
                    isotop.Uncertainty = Convert.ToDouble(items[4].Trim(), CultureInfo.InvariantCulture) * 2.0;
                    isotop.Uncertainty /= 100d;
                    isotop.Uncertainty *= isotop.Activity;
                    result.Isotopes.Add(isotop);
                }
            }
        }

        private void ParseMDA(TextReader reader)
        {
            string line;
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items[0].Trim().StartsWith("NUCLIDE"))
                {
                    // Plukk ut MDA%
                    result.MDAFactor = GetFirstPositiveNumber(line);
                    continue;
                }

                if (items[0].Trim().StartsWith("Bq"))
                    continue;

                AnalysisResult.Isotop isotop = result.Isotopes.Find(i => i.NuclideName == items[0].Trim());
                if (isotop != null)
                {
                    isotop.MDA = Convert.ToDouble(items[1].Trim(), CultureInfo.InvariantCulture);
                }
                else
                {
                    isotop = new AnalysisResult.Isotop();
                    isotop.ApprovedMDA = true;
                    isotop.NuclideName = items[0].Trim().ToUpper();
                    isotop.Activity = 0.0;
                    isotop.ConfidenceValue = 0.0;
                    isotop.Uncertainty = 0.0;
                    string s = items[1].Trim();
                    if (s != "Infinity" && s != "Nuclide")
                    {
                        isotop.MDA = Convert.ToDouble(items[1].Trim(), CultureInfo.InvariantCulture);
                        result.Isotopes.Add(isotop);
                    }
                }
            }
        }

        private void btnShowLIS_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", parameters.FileName);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            foreach(AnalysisResult.Isotop iso in result.Isotopes)
            {
                bool approved = iso.ApprovedRES || iso.ApprovedMDA;
                bool prop = iso.Accredited || iso.Reportable;
                if(prop && !approved)
                {
                    MessageBox.Show("Nuclide " + iso.NuclideName + ": Can not set accredited or reportable on nuclide that is not approved");
                    return;
                }
            }

            result.Isotopes.RemoveAll(x => parameters.AllNuclides.Contains(x.NuclideName) == false);
            result.Isotopes.RemoveAll(x => parameters.AnalMethNuclides.Contains(x.NuclideName) == false);

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
