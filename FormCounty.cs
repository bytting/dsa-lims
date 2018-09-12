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
    public partial class FormCounty : Form
    {
        private ILog mLog = null;

        public CountyType County = new CountyType();

        public FormCounty(ILog log)
        {
            InitializeComponent();
            mLog = log;
            Text = "Create county";
            cbInUse.Checked = true;
        }

        public FormCounty(ILog log, Guid cid)
        {
            InitializeComponent();
            mLog = log;
            County.Id = cid;
            Text = "Update county";

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_county", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", County.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("County with ID " + cid.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbNumber.Text = reader["county_number"].ToString();
                    cbInUse.Checked = Convert.ToBoolean(reader["in_use"]);                    
                    County.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    County.CreatedBy = reader["created_by"].ToString();
                    County.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    County.UpdatedBy = reader["updated_by"].ToString();
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
                MessageBox.Show("Name is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbNumber.Text.Trim()))
            {
                MessageBox.Show("Number is mandatory");
                return;
            }

            County.Name = tbName.Text.Trim();
            County.Number = Convert.ToInt32(tbNumber.Text.Trim());
            County.InUse = cbInUse.Checked;

            bool success;
            if (County.Id == Guid.Empty)
                success = InsertCounty();
            else
                success = UpdateCounty();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertCounty()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                County.CreateDate = DateTime.Now;
                County.CreatedBy = Common.Username;
                County.UpdateDate = DateTime.Now;
                County.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_county", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                County.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", County.Id);
                cmd.Parameters.AddWithValue("@name", County.Name);
                cmd.Parameters.AddWithValue("@county_number", County.Number);
                cmd.Parameters.AddWithValue("@in_use", County.InUse);
                cmd.Parameters.AddWithValue("@create_date", County.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", County.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", County.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", County.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "county", County.Id, AuditOperation.Insert, JsonConvert.SerializeObject(County));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateCounty()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                County.UpdateDate = DateTime.Now;
                County.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_county", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", County.Id);
                cmd.Parameters.AddWithValue("@name", County.Name);
                cmd.Parameters.AddWithValue("@county_number", County.Number);                
                cmd.Parameters.AddWithValue("@in_use", County.InUse);                
                cmd.Parameters.AddWithValue("@update_date", County.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", County.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "county", County.Id, AuditOperation.Update, JsonConvert.SerializeObject(County));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
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
