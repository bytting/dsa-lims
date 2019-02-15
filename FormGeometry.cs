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
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormGeometry : Form
    {        
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid GeometryId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string GeometryName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormGeometry()
        {
            InitializeComponent();

            Text = "New geometry";
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");
            }
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormGeometry(Guid gid)
        {
            InitializeComponent();
            
            Text = "Edit geometry";
            p["id"] = gid;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status");

                SqlCommand cmd = new SqlCommand("csp_select_preparation_geometry", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Geometry with ID " + p["id"] + " was not found");

                    reader.Read();
                    tbName.Text = reader.GetString("name");
                    tbMinFillHeight.Text = reader.GetDouble("min_fill_height_mm").ToString();
                    tbMaxFillHeight.Text = reader.GetDouble("max_fill_height_mm").ToString();
                    cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                    tbComment.Text = reader.GetString("comment");
                    p["create_date"] = reader.GetDateTime("create_date");
                    p["created_by"] = reader.GetString("created_by");
                    p["update_date"] = reader.GetDateTime("update_date");
                    p["updated_by"] = reader.GetString("updated_by");
                }
            }
        }

        private void FormGeometry_Load(object sender, EventArgs e)
        {
            tbMinFillHeight.KeyPress += CustomEvents.Numeric_KeyPress;
            tbMaxFillHeight.KeyPress += CustomEvents.Numeric_KeyPress;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Geomery name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbMinFillHeight.Text))
            {
                MessageBox.Show("Minimum fill height is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbMaxFillHeight.Text))
            {
                MessageBox.Show("Maximum fill height is mandatory");
                return;
            }

            double minFillHeight = Convert.ToDouble(tbMinFillHeight.Text);
            double maxFillHeight = Convert.ToDouble(tbMaxFillHeight.Text);

            if(minFillHeight < 0d || maxFillHeight < 0d)
            {
                MessageBox.Show("Minimum and maximum fill height can not be less than zero");
                return;
            }

            if(minFillHeight > maxFillHeight)
            {
                MessageBox.Show("Minimum fill height can not be bigger than maximum fill height");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["min_fill_height"] = minFillHeight;
            p["max_fill_height"] = maxFillHeight;
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                if (DB.NameExists(connection, transaction, "preparation_geometry", p["name"].ToString(), GeometryId))
                {
                    MessageBox.Show("The geometry '" + p["name"] + "' already exists");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertGeometry(connection, transaction);
                else
                    UpdateGeometry(connection, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                transaction?.Rollback();
                Common.Log.Error(ex);                
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private void InsertGeometry(SqlConnection conn, SqlTransaction trans)
        {            
            p["create_date"] = DateTime.Now;
            p["created_by"] = Common.Username;
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;

            SqlCommand cmd = new SqlCommand("csp_insert_preparation_geometry", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@min_fill_height", p["min_fill_height"]);
            cmd.Parameters.AddWithValue("@max_fill_height", p["max_fill_height"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                        
        }

        private void UpdateGeometry(SqlConnection conn, SqlTransaction trans)
        {        
            p["update_date"] = DateTime.Now;
            p["updated_by"] = Common.Username;

            SqlCommand cmd = new SqlCommand("csp_update_preparation_geometry", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@min_fill_height", p["min_fill_height"]);
            cmd.Parameters.AddWithValue("@max_fill_height", p["max_fill_height"]);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
            cmd.ExecuteNonQuery();                
        }        
    }
}
