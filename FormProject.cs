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
    public partial class FormProject : Form
    {
        private ILog mLog = null;

        public MainProjectType MainProject = new MainProjectType();

        public FormProject(ILog log)
        {
            InitializeComponent();
            mLog = log;
            Text = "Create new main project";
            cbInUse.Checked = true;
        }

        public FormProject(ILog log, Guid pid)
        {
            InitializeComponent();
            mLog = log;
            MainProject.Id = pid;
            Text = "Update main project";

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_project", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", MainProject.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Project with ID " + MainProject.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    cbInUse.Checked = Convert.ToBoolean(reader["in_use"]);
                    tbComment.Text = reader["comment"].ToString();

                    MainProject.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    MainProject.CreatedBy = reader["created_by"].ToString();
                    MainProject.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    MainProject.UpdatedBy = reader["updated_by"].ToString();
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
                MessageBox.Show("Project name is mandatory");
                return;
            }

            MainProject.Name = tbName.Text.Trim();
            MainProject.InUse = cbInUse.Checked;
            MainProject.Comment = tbComment.Text.Trim();

            bool success;
            if (MainProject.Id == Guid.Empty)
                success = InsertMainProject();
            else
                success = UpdateMainProject();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertMainProject()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                MainProject.CreateDate = DateTime.Now;
                MainProject.CreatedBy = Common.Username;
                MainProject.UpdateDate = DateTime.Now;
                MainProject.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_project", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                MainProject.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", MainProject.Id);
                cmd.Parameters.AddWithValue("@name", MainProject.Name);                
                cmd.Parameters.AddWithValue("@in_use", MainProject.InUse);
                cmd.Parameters.AddWithValue("@comment", MainProject.Comment);
                cmd.Parameters.AddWithValue("@create_date", MainProject.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", MainProject.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", MainProject.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", MainProject.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "project", MainProject.Id, AuditOperation.Insert, JsonConvert.SerializeObject(MainProject));

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

        private bool UpdateMainProject()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                MainProject.UpdateDate = DateTime.Now;
                MainProject.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_project", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", MainProject.Id);
                cmd.Parameters.AddWithValue("@name", MainProject.Name);                
                cmd.Parameters.AddWithValue("@in_use", MainProject.InUse);
                cmd.Parameters.AddWithValue("@comment", MainProject.Comment);
                cmd.Parameters.AddWithValue("@update_date", MainProject.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", MainProject.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "project", MainProject.Id, AuditOperation.Update, JsonConvert.SerializeObject(MainProject));

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
