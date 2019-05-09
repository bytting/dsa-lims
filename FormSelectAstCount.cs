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
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSelectAstCount : Form
    {
        private string mAstInfo;
        private int mConnectedSamples, mSelectedCount;

        public int SelectedCount { get { return mSelectedCount; } }

        public FormSelectAstCount(string astInfo, int connectedSamples)
        {
            InitializeComponent();

            mAstInfo = astInfo;            
            mConnectedSamples = connectedSamples;

            tbCount.KeyPress += CustomEvents.Integer_KeyPress;
        }

        private void FormSelectAstCount_Load(object sender, EventArgs e)
        {
            lblInfo.Text = mAstInfo;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbCount.Text))
            {
                MessageBox.Show("You must enter a new count for this sample type");
                return;
            }

            if(!Utils.IsValidInteger(tbCount.Text))
            {
                MessageBox.Show("Count must be a number");
                return;
            }

            mSelectedCount = Convert.ToInt32(tbCount.Text);
            if(mSelectedCount < 1 || mSelectedCount > 10000)
            {
                MessageBox.Show("Count must be a number between 1 and 10000");
                return;
            }

            if(mSelectedCount < mConnectedSamples)
            {
                MessageBox.Show("Can not assign " + mSelectedCount + " samples. There is currently " + mConnectedSamples + " samples connected to this sample type");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }        
    }
}
