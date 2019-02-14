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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormOrderAddAnalMeth : Form
    {
        private Assignment mAssignment = null;
        private AssignmentPreparationMethod mApm = null;

        public FormOrderAddAnalMeth(Assignment ass, AssignmentPreparationMethod apm)
        {
            InitializeComponent();

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;

            mAssignment = ass;
            mApm = apm;

            using (SqlConnection conn = DB.OpenConnection())
            {                
                UI.PopulateComboBoxes(conn, "csp_select_analysis_methods_for_laboratory_and_preparation_method_short", new[] {
                    new SqlParameter("@laboratory_id", mApm.PreparationLaboratoryId),
                    new SqlParameter("@preparation_method_id", mApm.PreparationMethodId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxAnalysisMethods);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxAnalysisMethods.SelectedValue))
            {
                MessageBox.Show("Analysis method is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(tbCount.Text.Trim()))
            {
                MessageBox.Show("Analysis method count is mandatory");
                return;
            }

            AssignmentAnalysisMethod aam = new AssignmentAnalysisMethod();
            aam.AssignmentPreparationMethodId = mApm.Id;
            aam.AnalysisMethodId = Guid.Parse(cboxAnalysisMethods.SelectedValue.ToString());
            aam.AnalysisMethodCount = Convert.ToInt32(tbCount.Text);
            aam.Comment = tbComment.Text.Trim();
            aam.CreateDate = DateTime.Now;
            aam.CreatedBy = Common.Username;
            aam.UpdateDate = DateTime.Now;
            aam.UpdatedBy = Common.Username;
            aam.Dirty = true;
            mApm.AnalysisMethods.Add(aam);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
