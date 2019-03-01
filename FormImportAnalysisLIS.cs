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
using System.Data.SqlClient;

namespace DSA_lims
{
    public partial class FormImportAnalysisLIS : Form
    {        
        private Preparation mPreparation = null;
        private Analysis mAnalysis = null;
        private Dictionary<string, Guid> AllNuclides = null;
        private Dictionary<string, Guid> AnalMethNuclides = null;

        string SpectrumNo, SampleIdentity, SamplePlace, SampleQuantityUnit, Geometry, NuclideLibrary, DetLimLib;
        double SampleQuantity, SampleHeight, SampleWeight, SigmaActivity;
        DateTime ReferenceTime;

        public FormImportAnalysisLIS(Preparation preparation, Analysis analysis)
        {
            InitializeComponent();

            mPreparation = preparation;
            mAnalysis = analysis;
        }

        private void FormImportAnalysisLIS_Load(object sender, EventArgs e)
        {
            SqlConnection connection = null;

            try
            {
                connection = DB.OpenConnection();

                AllNuclides = DB.GetNuclideNames(connection, null);
                AnalMethNuclides = DB.GetNuclideNamesForAnalysisMethod(connection, null, mAnalysis.AnalysisMethodId);
                int sampNum = DB.GetSampleNumber(connection, null, mPreparation.SampleId);
                string geomName = DB.GetGeometryName(connection, null, mPreparation.PreparationGeometryId);

                tbFilename.Text = mAnalysis.ImportFile;
                tbLIMSSampleName.Text = sampNum.ToString();
                tbLIMSPrepGeom.Text = geomName;
                tbLIMSGeomFillHeight.Text = mPreparation.FillHeightMM.ToString();
                tbLIMSGeomAmount.Text = mPreparation.Amount.ToString();
                tbLIMSGeomQuantity.Text = mPreparation.Quantity.ToString();

                LoadLIS(mAnalysis.ImportFile);
                foreach(AnalysisResult r in mAnalysis.Results)
                {
                    if (r.DetectionLimit == 0.0)
                        r.DetectionLimitApproved = false;
                }
                
                PopulateUI();
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection?.Close();
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
            
            tbNuclideLibrary.Text = mAnalysis.NuclideLibrary;
            tbDetLimLib.Text = mAnalysis.MDALibrary;

            grid.DataSource = mAnalysis.Results;

            grid.Columns["Id"].Visible = false;
            grid.Columns["AnalysisId"].Visible = false;
            grid.Columns["NuclideId"].Visible = false;
            grid.Columns["UniformActivity"].Visible = false;
            grid.Columns["UniformActivityUnitId"].Visible = false;
            grid.Columns["InstanceStatusId"].Visible = false;
            grid.Columns["CreateDate"].Visible = false;
            grid.Columns["CreateId"].Visible = false;
            grid.Columns["UpdateDate"].Visible = false;
            grid.Columns["UpdateId"].Visible = false;

            grid.Columns["NuclideName"].ReadOnly = true;            
            grid.Columns["Activity"].ReadOnly = true;
            grid.Columns["ActivityUncertaintyABS"].ReadOnly = true;
            grid.Columns["DetectionLimit"].ReadOnly = true;

            grid.Columns["NuclideName"].HeaderText = "Nuclide";
            grid.Columns["ActivityUncertaintyABS"].HeaderText = "Uncertainty (2σ)";
            grid.Columns["DetectionLimit"].HeaderText = "MDA (95%)";
            grid.Columns["DetectionLimitApproved"].HeaderText = "MDA.Appr";
            grid.Columns["ActivityApproved"].HeaderText = "Act.Appr";

            grid.Columns["Activity"].DefaultCellStyle.Format = Utils.ScientificFormat;
            grid.Columns["ActivityUncertaintyABS"].DefaultCellStyle.Format = Utils.ScientificFormat;

            foreach (DataGridViewRow row in grid.Rows)
            {
                Guid nuclId = Utils.MakeGuid(row.Cells["NuclideId"].Value);
                if (nuclId == Guid.Empty)
                {
                    row.DefaultCellStyle.BackColor = Color.Tomato;
                    foreach (DataGridViewCell cell in row.Cells)                    
                        cell.ReadOnly = true;
                }
                else if (!AnalMethNuclides.ContainsKey(row.Cells["NuclideName"].Value.ToString().ToUpper()))
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
            mAnalysis.Results.Clear();
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Contains("SPECTRUM NO"))
                {
                    string[] items = line.Split(splitColon);
                    SpectrumNo = items[1].Replace("\t", "").Trim();
                    SpectrumNo = items[1].Replace(" ", "");
                }

                if (line.Contains("SAMPLE IDENTITY"))
                {
                    string[] items = line.Split(splitColon);                    
                    SampleIdentity = items[1].Trim();                    
                }

                if (line.Contains("SAMPLE PLACE"))
                {
                    string[] items = line.Split(splitColon);
                    SamplePlace = items[1].Trim();
                }

                if (line.Contains("SAMPLE QUANTITY"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);                    
                    SampleQuantity = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
                    SampleQuantityUnit = unitpart[1];
                }

                if (line.Contains("BEAKER NO"))
                {
                    string[] items = line.Split(splitColon);
                    Geometry = items[1].Trim();
                }
                if (line.Contains("SAMPLE HEIGHT"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);
                    SampleHeight = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
                }
                if (line.Contains("SAMPLE WEIGHT"))
                {
                    string[] items = line.Split(splitColon);
                    string res = items[1].Trim();

                    string[] unitpart = res.Split(splitBlank, StringSplitOptions.RemoveEmptyEntries);
                    SampleWeight = Convert.ToDouble(unitpart[0], CultureInfo.InvariantCulture);
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

                    ReferenceTime = new DateTime(year, month, day, hour, min, 00);
                }
                if (line.Contains("NUCLIDE LIBRARY"))
                {
                    string[] items = line.Split(splitColon);
                    NuclideLibrary = items[1].Trim();
                }
                if (line.Contains("DETECTION LIMIT LIB"))
                {
                    string[] items = line.Split(splitColon);
                    DetLimLib = items[1].Trim();
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

            mAnalysis.SpecterReference = SpectrumNo;
            mAnalysis.NuclideLibrary = NuclideLibrary;
            mAnalysis.MDALibrary = DetLimLib;
            mAnalysis.SigmaActivity = 2.0;
            mAnalysis.SigmaMDA = 1.645;
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
                    SigmaActivity = GetFirstPositiveNumber(line);
                }

                string[] items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length == 5)
                {
                    AnalysisResult r = new AnalysisResult();
                    r.AnalysisId = mAnalysis.Id;
                    r.DetectionLimitApproved = true;
                    r.NuclideName = items[1].Trim().ToUpper();
                    if (!AllNuclides.ContainsKey(r.NuclideName))
                        r.NuclideId = Guid.Empty;
                    else r.NuclideId = AllNuclides[r.NuclideName];                    
                    r.Activity = Convert.ToDouble(items[3].Trim(), CultureInfo.InvariantCulture);
                    r.ActivityUncertaintyABS = Convert.ToDouble(items[4].Trim(), CultureInfo.InvariantCulture) * 2.0;
                    r.ActivityUncertaintyABS /= 100d;
                    r.ActivityUncertaintyABS *= r.Activity;
                    r.DetectionLimit = 0d;
                    r.Dirty = true;
                    mAnalysis.Results.Add(r);
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
                    //mAnalysis.MDA = GetFirstPositiveNumber(line);
                    continue;
                }

                if (items[0].Trim().StartsWith("Bq"))
                    continue;

                string nuclName = items[0].Trim().ToUpper();
                AnalysisResult r = mAnalysis.Results.Find(x => x.NuclideId == AllNuclides[nuclName]);
                if (r != null)                                
                {                    
                    r.DetectionLimit = Convert.ToDouble(items[1].Trim(), CultureInfo.InvariantCulture);
                }
                else
                {
                    r = new AnalysisResult();
                    r.AnalysisId = mAnalysis.Id;
                    r.DetectionLimitApproved = true;
                    r.NuclideId = AllNuclides[nuclName];
                    r.NuclideName = nuclName;
                    r.Activity = 0.0;                    
                    r.ActivityUncertaintyABS = 0.0;
                    string s = items[1].Trim();
                    if (s != "Infinity" && s != "Nuclide")
                    {
                        r.DetectionLimit = Convert.ToDouble(items[1].Trim(), CultureInfo.InvariantCulture);
                        mAnalysis.Results.Add(r);
                    }
                }
                r.Dirty = true;
            }
        }

        private void btnShowLIS_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", mAnalysis.ImportFile);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                if (!String.IsNullOrEmpty(mAnalysis.SpecterReference) && DB.SpecRefExists(conn, null, mAnalysis.SpecterReference, mAnalysis.Id))
                {
                    MessageBox.Show("Can not import file. Specter reference has already been used");
                    return;
                }
            }

            foreach (AnalysisResult r in mAnalysis.Results)
            {
                bool approved = r.ActivityApproved || r.DetectionLimitApproved;
                bool prop = r.Accredited || r.Reportable;
                if (prop && !approved)
                {
                    MessageBox.Show("Nuclide " + r.NuclideName + ": Can not set accredited or reportable on nuclide that is not approved");
                    return;
                }                

                if(r.Activity == 0d && r.ActivityApproved)
                {
                    MessageBox.Show("Can not approve a result with no activity");
                    return;
                }

                if (r.DetectionLimit == 0d && r.DetectionLimitApproved)
                {
                    MessageBox.Show("Can not approve a MDA with no limit");
                    return;
                }
            }

            mAnalysis.Results.RemoveAll(x => x.NuclideId == Guid.Empty);
            mAnalysis.Results.RemoveAll(x => AnalMethNuclides.ContainsValue(x.NuclideId) == false);

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
