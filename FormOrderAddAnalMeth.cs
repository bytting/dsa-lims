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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormOrderAddAnalMeth : Form
    {
        private Guid OrderPrepMethId = Guid.Empty;

        public FormOrderAddAnalMeth(Guid orderPrepMethId)
        {
            InitializeComponent();

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;

            OrderPrepMethId = orderPrepMethId;
            using (SqlConnection conn = DB.OpenConnection())
            {                
                UI.PopulateComboBoxes(conn, "csp_select_analysis_methods_short", new[] {
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

            try
            {
                int cnt = Convert.ToInt32(tbCount.Text.Trim());
                Guid analMethId = Guid.Parse(cboxAnalysisMethods.SelectedValue.ToString());

                using (SqlConnection conn = DB.OpenConnection())
                {                    
                    SqlCommand cmd = new SqlCommand("csp_insert_assignment_analysis_method", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@assignment_preparation_method_id", DB.MakeParam(typeof(Guid), OrderPrepMethId));
                    cmd.Parameters.AddWithValue("@analysis_method_id", DB.MakeParam(typeof(Guid), analMethId));
                    cmd.Parameters.AddWithValue("@analysis_method_count", cnt);
                    cmd.Parameters.AddWithValue("@comment", tbComment.Text);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
