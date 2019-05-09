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
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSampParamName : Form
    {
        Guid mSampParamNameId = Guid.Empty;

        public FormSampParamName()
        {
            InitializeComponent();
        }

        public FormSampParamName(Guid sampParamNameId)
        {
            InitializeComponent();

            mSampParamNameId = sampParamNameId;
        }

        private void FormSampParamName_Load(object sender, EventArgs e)
        {            
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                cboxSampParamType.Items.Clear();
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "select * from sample_parameter_type order by name", CommandType.Text))
                {
                    while(reader.Read())                
                        cboxSampParamType.Items.Add(reader.GetString("name"));
                }

                if(mSampParamNameId != Guid.Empty)
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, null, "select * from sample_parameter_name where id = @id", CommandType.Text, 
                        new SqlParameter("@id", mSampParamNameId)))
                    {
                        if (!reader.HasRows)
                            throw new Exception("FormSampParamName_Load: No sample parameter name found with id: " + mSampParamNameId.ToString());

                        reader.Read();

                        cboxSampParamType.Text = reader.GetString("type");
                        tbParamName.Text = reader.GetString("name");
                    }
                    cboxSampParamType.Enabled = false;
                }
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
            if (String.IsNullOrEmpty(tbParamName.Text.Trim()))
            {
                MessageBox.Show("Parameter name is mandatory");
                return;
            }

            if(String.IsNullOrEmpty(cboxSampParamType.Text))
            {
                MessageBox.Show("Parameter type is mandatory");
                return;
            }

            string paramName = tbParamName.Text.Trim();
            string paramType = cboxSampParamType.Text;

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                if (DB.NameExists(connection, transaction, "sample_parameter_name", paramName, mSampParamNameId))
                {
                    MessageBox.Show("The sample parameter name '" + paramName + "' already exists");
                    return;
                }

                if (mSampParamNameId == Guid.Empty)
                {
                    // insert
                    SqlCommand cmd = new SqlCommand("insert into sample_parameter_name values(@id, @name, @type)", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@name", paramName);
                    cmd.Parameters.AddWithValue("@type", paramType);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // update
                    SqlCommand cmd = new SqlCommand("update sample_parameter_name set name = @name where id = @id", connection, transaction);
                    cmd.Parameters.AddWithValue("@id", mSampParamNameId);
                    cmd.Parameters.AddWithValue("@name", paramName);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                success = false;
                transaction?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;            
            Close();
        }        
    }
}
