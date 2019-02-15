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
    public partial class FormAccountXRoles : Form
    {
        private Guid mUserId = Guid.Empty;
        private List<Guid> mExistingRoleIds = null;

        public FormAccountXRoles(Guid userId, List<Guid> existingRoleIds)
        {
            InitializeComponent();

            mUserId = userId;
            mExistingRoleIds = existingRoleIds;

            using (SqlConnection conn = DB.OpenConnection())
            {
                var roleArr = from item in mExistingRoleIds select "'" + item + "'";
                string sroles = string.Join(",", roleArr);

                string query;
                if (String.IsNullOrEmpty(sroles))
                    query = "select id, name from role order by name";
                else query = "select id, name from role where id not in(" + sroles + ") order by name";

                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text))
                {
                    lbRoles.Items.Clear();

                    while (reader.Read())
                    {
                        var r = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        lbRoles.Items.Add(r);
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
            if (lbRoles.SelectedItems.Count > 0)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into account_x_role values(@account_id, @role_id)", conn);

                    foreach (object item in lbRoles.SelectedItems)
                    {
                        var selItem = item as Lemma<Guid, string>;

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@account_id", mUserId, Guid.Empty);
                        cmd.Parameters.AddWithValue("@role_id", selItem.Id, Guid.Empty);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
