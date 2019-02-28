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
        Guid mSPId = Guid.Empty;

        public FormSampleParameter(Sample s)
        {
            InitializeComponent();

            mSample = s;
        }

        public FormSampleParameter(Sample s, Guid spId)
        {
            InitializeComponent();

            mSample = s;
            mSPId = spId;
        }

        private void FormSampleParameter_Load(object sender, EventArgs e)
        {
            lblTypeInfo.Text = "";

            SampleParameterName spn = new SampleParameterName();
            spn.Id = Guid.Empty;
            spn.Name = "";
            spn.Type = "";
            cboxSampleParameterNames.Items.Add(spn);            

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlDataReader reader = DB.GetDataReader(conn, null, "select id, name, type from sample_parameter_name order by name", CommandType.Text);
                while(reader.Read())
                {
                    spn = new SampleParameterName();
                    spn.Id = reader.GetGuid("id");
                    spn.Name = reader.GetString("name");
                    spn.Type = reader.GetString("type");
                    cboxSampleParameterNames.Items.Add(spn);
                }
            }

            cboxSampleParameterNames.DisplayMember = "Name";
            cboxSampleParameterNames.ValueMember = "Id";

            if(mSPId != Guid.Empty)
            {
                SampleParameter p = mSample.Parameters.Find(x => x.Id == mSPId);
                cboxSampleParameterNames.Text = p.Name;
                cboxSampleParameterNames.Enabled = false;
                tbSampleParameterValue.Text = p.Value;
                lblTypeInfo.Text = "Parameter type: " + p.Type;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SampleParameterName spn = cboxSampleParameterNames.SelectedItem as SampleParameterName;

            if (spn.Id == Guid.Empty)
            {
                MessageBox.Show("You must select a valid parameter type");
                return;
            }

            if(string.IsNullOrEmpty(tbSampleParameterValue.Text.Trim()))
            {
                MessageBox.Show("You must provide a value for this parameter");
                return;
            }

            if (mSPId == Guid.Empty)
            {
                if (mSample.Parameters.Exists(x => x.SampleId == mSample.Id && x.SampleParameterNameId == spn.Id))
                {
                    MessageBox.Show("This sample already have this parameter");
                    return;
                }

                SampleParameter p = new SampleParameter();
                p.SampleId = mSample.Id;
                p.SampleParameterNameId = spn.Id;
                p.Name = spn.Name;
                p.Type = spn.Type;
                p.Value = tbSampleParameterValue.Text.ToString();
                mSample.Parameters.Add(p);
                p.Dirty = true;
            }
            else
            {
                SampleParameter p = mSample.Parameters.Find(x => x.Id == mSPId);
                p.Value = tbSampleParameterValue.Text.ToString();
                p.Dirty = true;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cboxSampleParameterNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbSampleParameterValue.Text = "";

            if (cboxSampleParameterNames.SelectedItem == null)
                return;            

            tbSampleParameterValue.KeyPress -= CustomEvents.Integer_KeyPress;
            tbSampleParameterValue.KeyPress -= CustomEvents.Numeric_KeyPress;

            SampleParameterName spn = cboxSampleParameterNames.SelectedItem as SampleParameterName;            

            switch (spn.Type)
            {
                case SampleParameterType.Integer:
                    tbSampleParameterValue.KeyPress += CustomEvents.Integer_KeyPress;
                    break;
                case SampleParameterType.Decimal:
                    tbSampleParameterValue.KeyPress += CustomEvents.Numeric_KeyPress;
                    break;
            }

            lblTypeInfo.Text = "Parameter type: " + spn.Type;
        }
    }
}
