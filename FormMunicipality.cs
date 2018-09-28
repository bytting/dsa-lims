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
    public partial class FormMunicipality : Form
    {
        public MunicipalityModel Municipality = new MunicipalityModel();

        public FormMunicipality(Guid cid)
        {
            InitializeComponent();            
            Municipality.CountyId = cid;
            Text = "Create municipality";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormMunicipality(Guid cid, Guid mid)
        {
            InitializeComponent();            
            Municipality.CountyId = cid;
            Municipality.Id = mid;
            Text = "Update municipality";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("select name from county where id = @id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", Municipality.CountyId);
                tbCounty.Text = cmd.ExecuteScalar().ToString();

                cmd.CommandText = "csp_select_municipality";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", Municipality.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Municipality with ID " + Municipality.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbNumber.Text = reader["municipality_number"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    Municipality.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Municipality.CreatedBy = reader["created_by"].ToString();
                    Municipality.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Municipality.UpdatedBy = reader["updated_by"].ToString();
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

            if (String.IsNullOrEmpty(tbNumber.Text.Trim()))
            {
                MessageBox.Show("Number is mandatory");
                return;
            }

            Municipality.Name = tbName.Text.Trim();
            Municipality.Number = Convert.ToInt32(tbNumber.Text.Trim());
            Municipality.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);

            bool success;
            if (Municipality.Id == Guid.Empty)
                success = InsertMunicipality();
            else
                success = UpdateMunicipality();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertMunicipality()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Municipality.CreateDate = DateTime.Now;
                Municipality.CreatedBy = Common.Username;
                Municipality.UpdateDate = DateTime.Now;
                Municipality.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_municipality", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Municipality.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Municipality.Id);
                cmd.Parameters.AddWithValue("@county_id", Municipality.CountyId);
                cmd.Parameters.AddWithValue("@name", Municipality.Name);
                cmd.Parameters.AddWithValue("@municipality_number", Municipality.Number);
                cmd.Parameters.AddWithValue("@instance_status_id", Municipality.InstanceStatusId);
                cmd.Parameters.AddWithValue("@create_date", Municipality.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Municipality.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Municipality.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Municipality.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "municipality", Municipality.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(Municipality));

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

        private bool UpdateMunicipality()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Municipality.UpdateDate = DateTime.Now;
                Municipality.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_municipality", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Municipality.Id);
                cmd.Parameters.AddWithValue("@name", Municipality.Name);
                cmd.Parameters.AddWithValue("@municipality_number", Municipality.Number);
                cmd.Parameters.AddWithValue("@instance_status_id", Municipality.InstanceStatusId);
                cmd.Parameters.AddWithValue("@update_date", Municipality.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Municipality.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "municipality", Municipality.Id, AuditOperationType.Update, JsonConvert.SerializeObject(Municipality));

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
