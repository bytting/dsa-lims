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

using log4net;
using Newtonsoft.Json;
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
    public partial class FormGeometry : Form
    {
        private ILog mLog = null;

        public GeometryType Geometry = new GeometryType();

        public FormGeometry(ILog log)
        {
            InitializeComponent();

            mLog = log;
            Text = "New geometry";
            cbInUse.Checked = true;
        }

        public FormGeometry(ILog log, Guid gid)
        {
            InitializeComponent();

            mLog = log;
            Text = "Edit geometry";
            Geometry.Id = gid;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_geometry", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", gid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Geometry " + Geometry.Name + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbMinFillHeight.Text = reader["min_fill_height"].ToString();
                    tbMaxFillHeight.Text = reader["max_fill_height"].ToString();
                    cbInUse.Checked = Convert.ToBoolean(reader["in_use"]);
                    tbComment.Text = reader["comment"].ToString();
                    Geometry.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Geometry.CreatedBy = reader["created_by"].ToString();
                    Geometry.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Geometry.UpdatedBy = reader["updated_by"].ToString();
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
            if(String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Geomery name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbMinFillHeight.Text.Trim()))
            {
                MessageBox.Show("Minimum fill height is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbMaxFillHeight.Text.Trim()))
            {
                MessageBox.Show("Maximum fill height is mandatory");
                return;
            }

            Geometry.Name = tbName.Text.Trim();
            Geometry.MinFillHeight = Convert.ToDouble(tbMinFillHeight.Text.Trim());
            Geometry.MaxFillHeight = Convert.ToDouble(tbMaxFillHeight.Text.Trim());
            Geometry.InUse = cbInUse.Checked;
            Geometry.Comment = tbComment.Text.Trim();

            if (Geometry.Id == Guid.Empty)
                InsertGeometry();
            else
                UpdateGeometry();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void InsertGeometry()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Geometry.CreateDate = DateTime.Now;
                Geometry.CreatedBy = Common.Username;
                Geometry.UpdateDate = DateTime.Now;
                Geometry.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();                

                SqlCommand cmd = new SqlCommand("csp_insert_geometry", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Geometry.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Geometry.Id);
                cmd.Parameters.AddWithValue("@name", Geometry.Name);
                cmd.Parameters.AddWithValue("@min_fill_height", Geometry.MinFillHeight);
                cmd.Parameters.AddWithValue("@max_fill_height", Geometry.MaxFillHeight);
                cmd.Parameters.AddWithValue("@in_use", Geometry.InUse);
                cmd.Parameters.AddWithValue("@comment", Geometry.Comment);
                cmd.Parameters.AddWithValue("@create_date", Geometry.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Geometry.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Geometry.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Geometry.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "geometry", Geometry.Id, AuditOperation.Insert, JsonConvert.SerializeObject(Geometry));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
            }
            finally
            {
                connection?.Close();
            }
        }

        private void UpdateGeometry()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Geometry.UpdateDate = DateTime.Now;
                Geometry.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_geometry", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Geometry.Id);
                cmd.Parameters.AddWithValue("@name", Geometry.Name);
                cmd.Parameters.AddWithValue("@min_fill_height", Geometry.MinFillHeight);
                cmd.Parameters.AddWithValue("@max_fill_height", Geometry.MaxFillHeight);
                cmd.Parameters.AddWithValue("@in_use", Geometry.InUse);
                cmd.Parameters.AddWithValue("@comment", Geometry.Comment);                
                cmd.Parameters.AddWithValue("@update_date", Geometry.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Geometry.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "geometry", Geometry.Id, AuditOperation.Update, JsonConvert.SerializeObject(Geometry));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
            }
            finally
            {
                connection?.Close();
            }
        }
    }
}
