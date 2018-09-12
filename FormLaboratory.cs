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
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormLaboratory : Form
    {
        private ILog mLog = null;

        public LaboratoryType Laboratory = new LaboratoryType();

        public FormLaboratory(ILog log)
        {
            InitializeComponent();
            mLog = log;
            Text = "Create laboratory";
            cbInUse.Checked = true;
        }

        public FormLaboratory(ILog log, Guid lid)
        {
            InitializeComponent();
            mLog = log;
            Laboratory.Id = lid;
            Text = "Update laboratory";

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_laboratory", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Laboratory.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Laboratory with ID " + Laboratory.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbPrefix.Text = reader["name_prefix"].ToString();
                    tbAddress.Text = reader["address"].ToString();
                    tbEmail.Text = reader["email"].ToString();
                    tbPhone.Text = reader["phone"].ToString();
                    cbInUse.Checked = Convert.ToBoolean(reader["in_use"]);
                    tbComment.Text = reader["comment"].ToString();

                    Laboratory.AssignmentCounter = Convert.ToInt32(reader["assignment_counter"]);
                    Laboratory.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Laboratory.CreatedBy = reader["created_by"].ToString();
                    Laboratory.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Laboratory.UpdatedBy = reader["updated_by"].ToString();
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
                MessageBox.Show("The name field is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbPrefix.Text.Trim()))
            {
                MessageBox.Show("The prefix field is mandatory");
                return;
            }

            Laboratory.Name = tbName.Text.Trim();
            Laboratory.NamePrefix = tbPrefix.Text.Trim();
            Laboratory.Address = tbAddress.Text.Trim();
            Laboratory.Email = tbEmail.Text.Trim();
            Laboratory.Phone = tbPhone.Text.Trim();
            Laboratory.InUse = cbInUse.Checked;
            Laboratory.Comment = tbComment.Text.Trim();

            bool success;
            if (Laboratory.Id == Guid.Empty)
                success = InsertLaboratory();
            else
                success = UpdateLaboratory();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertLaboratory()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Laboratory.AssignmentCounter = 1;
                Laboratory.CreateDate = DateTime.Now;
                Laboratory.CreatedBy = Common.Username;
                Laboratory.UpdateDate = DateTime.Now;
                Laboratory.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_laboratory", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Laboratory.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Laboratory.Id);
                cmd.Parameters.AddWithValue("@name", Laboratory.Name);
                cmd.Parameters.AddWithValue("@name_prefix", Laboratory.NamePrefix);
                cmd.Parameters.AddWithValue("@address", Laboratory.Address);
                cmd.Parameters.AddWithValue("@email", Laboratory.Email);
                cmd.Parameters.AddWithValue("@phone", Laboratory.Phone);
                cmd.Parameters.AddWithValue("@assignment_counter", Laboratory.AssignmentCounter);
                cmd.Parameters.AddWithValue("@in_use", Laboratory.InUse);
                cmd.Parameters.AddWithValue("@comment", Laboratory.Comment);
                cmd.Parameters.AddWithValue("@create_date", Laboratory.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Laboratory.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Laboratory.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Laboratory.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "laboratory", Laboratory.Id, AuditOperation.Insert, JsonConvert.SerializeObject(Laboratory));

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

        private bool UpdateLaboratory()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                Laboratory.UpdateDate = DateTime.Now;
                Laboratory.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_laboratory", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Laboratory.Id);
                cmd.Parameters.AddWithValue("@name", Laboratory.Name);
                cmd.Parameters.AddWithValue("@name_prefix", Laboratory.NamePrefix);                
                cmd.Parameters.AddWithValue("@address", Laboratory.Address);
                cmd.Parameters.AddWithValue("@email", Laboratory.Email);
                cmd.Parameters.AddWithValue("@phone", Laboratory.Phone);
                cmd.Parameters.AddWithValue("@in_use", Laboratory.InUse);
                cmd.Parameters.AddWithValue("@comment", Laboratory.Comment);
                cmd.Parameters.AddWithValue("@update_date", Laboratory.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Laboratory.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "laboratory", Laboratory.Id, AuditOperation.Update, JsonConvert.SerializeObject(Laboratory));

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
