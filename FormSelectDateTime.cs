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

namespace DSA_lims
{
    public partial class FormSelectDateTime : Form
    {
        private DateTime mDate = new DateTime();
        private DateTime mTime = new DateTime();

        public DateTime SelectedDateTime
        {
            get { return new DateTime(mDate.Year, mDate.Month, mDate.Day, mTime.Hour, mTime.Minute, mTime.Second); }
        }

        public FormSelectDateTime(string label = "")
        {
            InitializeComponent();

            lblInfo.Text = label;
        }

        private void FormSelectDateTime_Load(object sender, EventArgs e)
        {
            dtDate.Format = DateTimePickerFormat.Custom;
            dtDate.CustomFormat = Utils.DateFormatNorwegian;
            dtTime.Format = DateTimePickerFormat.Custom;
            dtTime.CustomFormat = Utils.TimeFormatNorwegian;

            DateTime now = DateTime.Now;
            dtDate.Value = now;
            dtTime.Value = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            mDate = dtDate.Value;
            mTime = dtTime.Value;
            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
