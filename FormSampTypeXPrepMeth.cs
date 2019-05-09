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
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSampTypeXPrepMeth : Form
    {
        private Guid SampleTypeId = Guid.Empty;
        private string SampleTypeName = String.Empty;
        private List<Guid> MethodsAbove = null;
        private List<Guid> MethodsBelow = null;

        public FormSampTypeXPrepMeth(string sampleTypeName, Guid sampleTypeId, List<Guid> methodsAbove, List<Guid> methodsBelow)
        {
            InitializeComponent();

            SampleTypeId = sampleTypeId;
            SampleTypeName = sampleTypeName;
            MethodsAbove = methodsAbove;
            MethodsBelow = methodsBelow;

            tbSampleType.Text = SampleTypeName;                                        
        }

        private void FormSampTypeXPrepMeth_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                var methArr = from item in MethodsAbove select "'" + item + "'";
                string smeth = string.Join(",", methArr);

                string query;
                if (String.IsNullOrEmpty(smeth))
                    query = "select id, name from preparation_method where instance_status_id < 2 order by name";
                else query = "select id, name from preparation_method where instance_status_id < 2 and id not in(" + smeth + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))
                {
                    lbPrepMeth.Items.Clear();

                    while (reader.Read())
                    {
                        var pm = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        lbPrepMeth.Items.Add(pm);
                    }
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
            if (lbPrepMeth.SelectedItems.Count > 0)
            {
                foreach (object item in lbPrepMeth.SelectedItems)
                {
                    var selItem = item as Lemma<Guid, string>;
                    if(MethodsBelow.Contains(selItem.Id))
                    {
                        MessageBox.Show("Preparation method " + selItem.Name + " already exist for one or more sample types below this one. You must remove these first");
                        return;
                    }
                }

                SqlConnection conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    SqlCommand cmd = new SqlCommand("insert into sample_type_x_preparation_method values(@sample_type_id, @preparation_method_id)", conn);

                    foreach (object item in lbPrepMeth.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@preparation_method_id", selItem.Id, Guid.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.Error(ex);
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    conn?.Close();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
