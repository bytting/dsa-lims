﻿/*	
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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormPrepAnalResult : Form
    {        
        private Analysis mAnalysis = null;
        private AnalysisResult mResult = null;
        private Dictionary<string, Guid> mNuclides = null;
        bool editing = false;

        string invalidChars = "abcdfghijklmnopqrstuvwxyzæøåABCDFGHIJKLMNOPQRSTUVWXYZÆØÅ|*&/\\=<>(){},%[]";

        public FormPrepAnalResult(Analysis analysis, Dictionary<string, Guid> nuclides)
        {
            InitializeComponent();

            Text = "DSA-Lims - Add analysis result";
            cboxNuclides.DataSource = nuclides.Keys.ToArray();
            cboxNuclides.Text = "";
            mAnalysis = analysis;
            mNuclides = nuclides;
            mResult = new AnalysisResult();            
        }

        public FormPrepAnalResult(Analysis analysis, Guid resultId)
        {
            InitializeComponent();

            Text = "DSA-Lims - Edit analysis result";
            editing = true;
            mAnalysis = analysis;
            mResult = mAnalysis.Results.Find(x => x.Id == resultId);
            if(mResult == null)
            {
                MessageBox.Show("Unable to find analysis result with id: " + resultId);
                Close();
            }
                        
            cboxNuclides.Items.Add(mResult.NuclideName);
            cboxNuclides.Text = mResult.NuclideName;            
            cboxNuclides.Enabled = false;                        
        }

        private void FormPrepAnalResult_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                if (editing)
                {                    
                    cboxSigmaActivity.DataSource = DB.GetSigmaValues(conn, null, false);
                    cboxSigmaMDA.DataSource = DB.GetSigmaValues(conn, null, true);                

                    cboxSigmaActivity.SelectedValue = 2d;
                    cboxSigmaActivity.Enabled = false;
                    cboxSigmaMDA.SelectedValue = 1.645d;
                    cboxSigmaMDA.Enabled = false;

                    tbActivity.Text = mResult.Activity.ToString(Utils.ScientificFormat);
                    tbUncertainty.Text = mResult.ActivityUncertaintyABS.ToString(Utils.ScientificFormat);
                    cbActivityApproved.Checked = mResult.ActivityApproved;
                    tbDetectionLimit.Text = mResult.DetectionLimit.ToString(Utils.ScientificFormat);
                    cbDetectionLimitApproved.Checked = mResult.DetectionLimitApproved;
                    cbAccredited.Checked = mResult.Accredited;
                    cbReportable.Checked = mResult.Reportable;
                }
                else
                {                    
                    cboxSigmaActivity.DataSource = DB.GetSigmaValues(conn, null, false);
                    cboxSigmaMDA.DataSource = DB.GetSigmaValues(conn, null, true);                

                    cboxSigmaActivity.SelectedValue = 2d;
                    cboxSigmaMDA.SelectedValue = 1.645d;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
                Close();
            }
            finally
            {
                conn?.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!editing)
            {
                Guid nuclideId = mNuclides[cboxNuclides.Text];
                if (!Utils.IsValidGuid(nuclideId))
                {
                    MessageBox.Show("The nuclide field is mandatory");
                    return;
                }
            }

            if(String.IsNullOrEmpty(tbActivity.Text.Trim()))
            {
                if(!String.IsNullOrEmpty(tbUncertainty.Text.Trim()))
                {
                    MessageBox.Show("Activity uncertainty must be empty when activity is empty");
                    return;
                }
            }

            if((double)cboxSigmaActivity.SelectedValue == 0d)
            {
                MessageBox.Show("Sigma uncertainty is mandatory");
                return;
            }

            if ((double)cboxSigmaMDA.SelectedValue == 0d)
            {
                MessageBox.Show("Sigma MDA is mandatory");
                return;
            }

            bool approved = cbActivityApproved.Checked || cbDetectionLimitApproved.Checked;

            if(approved)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    if (!DB.CanUserApproveAnalysis(conn, null, Common.UserId, mAnalysis.AnalysisMethodId))
                    {
                        MessageBox.Show("You are not allowed to approve results for this analysis method");
                        return;
                    }
                }
            }

            if (!approved && (cbAccredited.Checked || cbReportable.Checked))
            {                
                MessageBox.Show("Activity or MDA must be approved before setting accredited and reportable");
                return;
            }

            double? act = null;
            if (Double.TryParse(tbActivity.Text.Trim(), out double a))            
                act = a;

            if (act < 0d)
            {
                MessageBox.Show("Activity can not be negative");
                return;
            }

            if((act == null || act == 0) && cbActivityApproved.Checked)
            {
                MessageBox.Show("Can not approve an activity of zero");
                return;
            }

            double? unc = null;
            if (Double.TryParse(tbUncertainty.Text.Trim(), out double u))
                unc = u;

            if (unc < 0d)
            {
                MessageBox.Show("Uncertainty can not be negative");
                return;
            }

            if(act != null && unc != null && !cbUncertaintyAbs.Checked)
            {
                if(unc < 0d || unc > 100d)
                {
                    MessageBox.Show("Activity uncertainty is out of percent range [0, 100]");
                    return;
                }

                unc = act * (unc / 100d);
            }

            if (unc != null)
            {
                double sigmaAct = Convert.ToDouble(cboxSigmaActivity.SelectedValue);
                unc /= sigmaAct;
                unc *= 2d;
            }

            if(act != null && unc == null)
            {
                MessageBox.Show("Activity uncertainty is required");
                return;
            }

            double? detlim = null;
            if (Double.TryParse(tbDetectionLimit.Text.Trim(), out double dl))            
                detlim = dl;

            if (detlim < 0d)
            {
                MessageBox.Show("MDA can not be negative");
                return;
            }

            if ((detlim == null || detlim == 0) && cbDetectionLimitApproved.Checked)
            {
                MessageBox.Show("Can not approve a MDA of zero");
                return;
            }

            if (detlim != null)
            {
                double sigmaMDA = Convert.ToDouble(cboxSigmaMDA.SelectedValue);
                detlim /= sigmaMDA;
                detlim *= 1.645d;
            }

            mResult.Activity = act;
            mResult.ActivityUncertaintyABS = unc;
            mResult.DetectionLimit = detlim;
            mResult.ActivityApproved = cbActivityApproved.Checked;
            mResult.DetectionLimitApproved = cbDetectionLimitApproved.Checked;
            mResult.Accredited = cbAccredited.Checked;
            mResult.Reportable = cbReportable.Checked;

            if (!editing)
            {
                mResult.AnalysisId = mAnalysis.Id;
                mResult.NuclideName = cboxNuclides.Text;
                mResult.NuclideId = mNuclides[mResult.NuclideName];
                mAnalysis.Results.Add(mResult);
            }

            mResult.Dirty = true;

            DialogResult = DialogResult.OK;
            Close();
        }        

        private void tbActivity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (invalidChars.Contains(e.KeyChar))
                e.Handled = true;
        }        
    }
}
