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
    public partial class FormSampleParameter : Form
    {
        Sample mSample = null;

        public FormSampleParameter(Sample s)
        {
            InitializeComponent();

            mSample = s;
        }

        private void FormSampleParameter_Load(object sender, EventArgs e)
        {
            List<SampleParameterName> spnList = new List<SampleParameterName>();
            SampleParameterName spn = new SampleParameterName();
            spn.Id = Guid.Empty;
            spn.Name = "";
            spn.Type = "";
            spnList.Add(spn);

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlDataReader reader = DB.GetDataReader(conn, null, "select id, name, type from sample_parameter_name order by name", CommandType.Text);
                while(reader.Read())
                {
                    spn = new SampleParameterName();
                    spn.Id = reader.GetGuid("id");
                    spn.Name = reader.GetString("name");
                    spn.Type = reader.GetString("type");
                    spnList.Add(spn);
                }
            }

            cboxSampleParameterNames.DisplayMember = "Name";
            cboxSampleParameterNames.ValueMember = "Id";
            cboxSampleParameterNames.DataSource = spnList;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxSampleParameterNames.SelectedValue))
            {
                MessageBox.Show("You must select a valid parameter type");
                return;
            }

            if(string.IsNullOrEmpty(tbSampleParameterValue.Text.Trim()))
            {
                MessageBox.Show("You must provide a value for this parameter");
                return;
            }

            Guid spnId = Utils.MakeGuid(cboxSampleParameterNames.SelectedValue);

            if(mSample.Parameters.Exists(x => x.SampleId == mSample.Id && x.SampleParameterNameId == spnId))
            {
                MessageBox.Show("This sample already have this parameter");
                return;
            }

            SampleParameter p = new SampleParameter();
            p.SampleId = mSample.Id;
            p.SampleParameterNameId = spnId;
            p.Value = tbSampleParameterValue.Text.ToString();
            mSample.Parameters.Add(p);
            mSample.Dirty = true;            

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
