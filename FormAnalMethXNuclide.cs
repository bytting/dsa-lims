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
    public partial class FormAnalMethXNuclide : Form
    {
        private Guid AnalysisMethodId = Guid.Empty;
        private string AnalysisMethodName = String.Empty;
        private List<Guid> ExistingNuclides = null;

        public FormAnalMethXNuclide(string analysisMethodName, Guid analysisMethodId, List<Guid> existingNuclides)
        {
            InitializeComponent();

            AnalysisMethodId = analysisMethodId;
            AnalysisMethodName = analysisMethodName;
            ExistingNuclides = existingNuclides;

            tbAnalysisMethod.Text = AnalysisMethodName;

            using (SqlConnection conn = DB.OpenConnection())
            {
                var nuclArr = from item in ExistingNuclides select "'" + item + "'";
                string snucl = string.Join(",", nuclArr);

                string query;
                if (String.IsNullOrEmpty(snucl))
                    query = "select id, name from nuclide order by name";
                else query = "select id, name from nuclide where id not in(" + snucl + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text))
                {
                    lbNuclides.Items.Clear();

                    while (reader.Read())
                    {
                        var pm = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                        lbNuclides.Items.Add(pm);
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (lbNuclides.SelectedItems.Count > 0)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into analysis_method_x_nuclide values(@analysis_method_id, @nuclide_id)", conn);

                    foreach (object item in lbNuclides.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId);
                        cmd.Parameters.AddWithValue("@nuclide_id", selItem.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
