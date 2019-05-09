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
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSelectSampleType : Form
    {
        public Guid SelectedSampleTypeId = Guid.Empty;
        public string SelectedSampleTypeName = String.Empty;
        public string SelectedSampleTypePath = String.Empty;

        public FormSelectSampleType()
        {
            InitializeComponent();
        }

        private void FormSelectSampleType_Load(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleTypes(conn, treeSampleTypes);
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
            if(treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            SelectedSampleTypeId = Guid.Parse(treeSampleTypes.SelectedNode.Name);
            SelectedSampleTypeName = treeSampleTypes.SelectedNode.Text;
            SelectedSampleTypePath = treeSampleTypes.SelectedNode.ToolTipText;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
