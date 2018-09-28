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
    public partial class FormStation : Form
    {
        public StationModel Station = new StationModel();

        public FormStation()
        {
            InitializeComponent();
            // create new station            
            Text = "Create station";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }
        public FormStation(Guid sid)
        {
            InitializeComponent();
            // edit existing station            
            Station.Id = sid;
            Text = "Update station";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_station", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Station.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Station with ID " + Station.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbLatitude.Text = reader["latitude"].ToString();
                    tbLongitude.Text = reader["longitude"].ToString();
                    tbAltitude.Text = reader["altitude"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    Station.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Station.CreatedBy = reader["created_by"].ToString();
                    Station.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Station.UpdatedBy = reader["updated_by"].ToString();
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
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbLatitude.Text.Trim()))
            {
                MessageBox.Show("Latitude is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbLongitude.Text.Trim()))
            {
                MessageBox.Show("Longitude is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbAltitude.Text.Trim()))
            {
                MessageBox.Show("Altitude is mandatory");
                return;
            }

            Station.Name = tbName.Text.Trim();
            Station.Latitude = Convert.ToDouble(tbLatitude.Text.Trim());
            Station.Longitude = Convert.ToDouble(tbLongitude.Text.Trim());
            Station.Altitude = Convert.ToDouble(tbAltitude.Text.Trim());
            Station.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            Station.Comment = tbComment.Text.Trim();

            bool success;
            if (Station.Id == Guid.Empty)
                success = InsertStation();
            else
                success = UpdateStation();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertStation()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Station.CreateDate = DateTime.Now;
                Station.CreatedBy = Common.Username;
                Station.UpdateDate = DateTime.Now;
                Station.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_station", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Station.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Station.Id);
                cmd.Parameters.AddWithValue("@name", Station.Name);
                cmd.Parameters.AddWithValue("@latitude", Station.Latitude);
                cmd.Parameters.AddWithValue("@longitude", Station.Longitude);
                cmd.Parameters.AddWithValue("@altitude", Station.Altitude);
                cmd.Parameters.AddWithValue("@instance_status_id", Station.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Station.Comment);
                cmd.Parameters.AddWithValue("@create_date", Station.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Station.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Station.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Station.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "station", Station.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(Station));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateStation()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Station.UpdateDate = DateTime.Now;
                Station.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_station", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Station.Id);
                cmd.Parameters.AddWithValue("@name", Station.Name);
                cmd.Parameters.AddWithValue("@latitude", Station.Latitude);
                cmd.Parameters.AddWithValue("@longitude", Station.Longitude);
                cmd.Parameters.AddWithValue("@altitude", Station.Altitude);
                cmd.Parameters.AddWithValue("@instance_status_id", Station.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Station.Comment);
                cmd.Parameters.AddWithValue("@update_date", Station.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Station.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "station", Station.Id, AuditOperationType.Update, JsonConvert.SerializeObject(Station));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }
    }
}
