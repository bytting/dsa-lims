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
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormOrderAddSampleType : Form
    {
        private Assignment mAssignment = null;
        private TreeView TreeSampleTypes = null;

        public FormOrderAddSampleType(Assignment assignment, TreeView treeSampleTypes)
        {
            InitializeComponent();

            tbNumSamples.KeyPress += CustomEvents.Integer_KeyPress;

            mAssignment = assignment;
            TreeSampleTypes = treeSampleTypes;                            
        }

        private void FormOrderAddSampleType_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            UI.PopulateSampleTypes(TreeSampleTypes, cboxSampleType);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_activity_units_short", new SqlParameter[] { }, cboxRequestedUnit);
                UI.PopulateComboBoxes(conn, "csp_select_activity_unit_types", new SqlParameter[] { }, cboxRequestedUnitType);
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
            if (!CheckExistingSampleType())
                return;

            if(!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbNumSamples.Text))
            {
                MessageBox.Show("Number of samples is mandatory");
                return;
            }

            if(!Utils.IsValidInteger(tbNumSamples.Text))
            {
                MessageBox.Show("Number of samples must be a number");
                return;
            }

            int nsamples = Convert.ToInt32(tbNumSamples.Text);
            if(nsamples < 1 || nsamples > 10000)
            {
                MessageBox.Show("Number of samples must be between 1 and 10000");
                return;
            }

            AssignmentSampleType ast = new AssignmentSampleType();
            ast.AssignmentId = mAssignment.Id;
            ast.SampleTypeId = Utils.MakeGuid(cboxSampleType.SelectedValue);
            ast.SampleComponentId = Utils.MakeGuid(cboxSampleComponent.SelectedValue);
            ast.SampleCount = nsamples;
            ast.RequestedActivityUnitId = Utils.MakeGuid(cboxRequestedUnit.SelectedValue);
            ast.RequestedActivityUnitTypeId = Utils.MakeGuid(cboxRequestedUnitType.SelectedValue);
            ast.ReturnToSender = cbReturnToSender.Checked;
            ast.Comment = tbComment.Text.Trim();
            ast.CreateDate = DateTime.Now;
            ast.CreateId = Common.UserId;
            ast.UpdateDate = DateTime.Now;
            ast.UpdateId = Common.UserId;
            ast.Dirty = true;
            mAssignment.SampleTypes.Add(ast);            

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnSelectSampleType_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleType.SelectedValue = form.SelectedSampleTypeId;
        }

        private void cboxSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Utils.MakeGuid(cboxSampleType.SelectedValue);
            TreeNode[] tnodes = TreeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleComponent);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }        

        private bool CheckExistingSampleType()
        {
            if(!Utils.IsValidGuid(cboxSampleType.SelectedValue))
            {
                cboxSampleType.SelectedItem = Guid.Empty;
                cboxSampleComponent.DataSource = null;

                lblStatus.Text = Utils.makeStatusMessage("You must select an existing sample type");
                return false;
            }

            return true;
        }

        private void cboxSampleType_Leave(object sender, EventArgs e)
        {
            lblStatus.Text = "";

            if (String.IsNullOrEmpty(cboxSampleType.Text.Trim()))
            {                
                cboxSampleType.SelectedValue = Guid.Empty;
                cboxSampleComponent.DataSource = null;
                return;
            }

            CheckExistingSampleType();
        }
    }
}
