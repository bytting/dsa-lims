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
using System.IO;
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
    public partial class FormLaboratory : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid LaboratoryId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string LaboratoryName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormLaboratory()
        {
            InitializeComponent(); 
                       
            Text = "Create laboratory";
            lblLaboratoryLogoSize.Text = "";
            lblAccreditedLogoSize.Text = "";

            p["laboratory_logo"] = null;
            p["accredited_logo"] = null;

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);
            }            
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormLaboratory(Guid lid)
        {
            InitializeComponent();            
            p["id"] = lid;
            Text = "Update laboratory";
            lblLaboratoryLogoSize.Text = "";
            lblAccreditedLogoSize.Text = "";

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                SqlCommand cmd = new SqlCommand("csp_select_laboratory", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Laboratory with ID " + p["id"] + " was not found");

                    reader.Read();

                    tbName.Text = reader.GetString("name");
                    tbPrefix.Text = reader.GetString("name_prefix");
                    tbAddress.Text = reader.GetString("address");
                    tbEmail.Text = reader.GetString("email");
                    tbPhone.Text = reader.GetString("phone");
                    cboxInstanceStatus.SelectedValue = reader.GetInt32("instance_status_id");
                    tbComment.Text = reader.GetString("comment");

                    if (DB.IsValidField(reader["laboratory_logo"]))
                    {
                        p["laboratory_logo"] = (byte[])reader["laboratory_logo"];
                        picLaboratoryLogo.Image = Image.FromStream(new MemoryStream((byte[])p["laboratory_logo"]));
                        lblLaboratoryLogoSize.Text = picLaboratoryLogo.Image.Width.ToString() + " x " + picLaboratoryLogo.Image.Height.ToString();
                    }
                    else
                    {
                        p["laboratory_logo"] = picLaboratoryLogo.Image = null;
                    }

                    if (DB.IsValidField(reader["accredited_logo"]))
                    {
                        p["accredited_logo"] = (byte[])reader["accredited_logo"];
                        picAccreditedLogo.Image = Image.FromStream(new MemoryStream((byte[])p["accredited_logo"]));
                        lblAccreditedLogoSize.Text = picAccreditedLogo.Image.Width.ToString() + " x " + picAccreditedLogo.Image.Height.ToString();
                    }
                    else
                    {
                        p["accredited_logo"] = picAccreditedLogo.Image = null;
                    }

                    p["assignment_counter"] = reader.GetInt32("assignment_counter");
                    p["create_date"] = reader.GetDateTime("create_date");
                    p["create_id"] = reader.GetGuid("create_id");
                    p["update_date"] = reader.GetDateTime("update_date");
                    p["update_id"] = reader.GetGuid("update_id");
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

            p["name"] = tbName.Text.Trim();
            p["name_prefix"] = tbPrefix.Text.Trim().ToUpper();
            p["address"] = tbAddress.Text.Trim();
            p["email"] = tbEmail.Text.Trim();
            p["phone"] = tbPhone.Text.Trim();
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            SqlConnection connection = null;
            SqlTransaction transaction = null;
            bool success = true;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                if (DB.NameExists(connection, transaction, "laboratory", p["name"].ToString(), LaboratoryId))
                {
                    MessageBox.Show("The laboratory '" + p["name"] + "' already exists");
                    return;
                }

                if (!p.ContainsKey("id"))
                    InsertLaboratory(connection, transaction);
                else
                    UpdateLaboratory(connection, transaction);

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

        private void InsertLaboratory(SqlConnection conn, SqlTransaction trans)
        {            
            p["last_assignment_counter_year"] = DateTime.Now.Year;
            p["assignment_counter"] = 1;                
            p["create_date"] = DateTime.Now;
            p["create_id"] = Common.UserId;
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;        

            SqlCommand cmd = new SqlCommand("csp_insert_laboratory", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            p["id"] = Guid.NewGuid();
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_prefix", p["name_prefix"]);
            cmd.Parameters.AddWithValue("@address", p["address"], String.Empty);
            cmd.Parameters.AddWithValue("@email", p["email"], String.Empty);
            cmd.Parameters.AddWithValue("@phone", p["phone"], String.Empty);
            cmd.Parameters.AddWithValue("@last_assignment_counter_year", p["last_assignment_counter_year"]);
            cmd.Parameters.AddWithValue("@assignment_counter", p["assignment_counter"]);                
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);

            if(p["laboratory_logo"] == null)
                cmd.Parameters.Add("@laboratory_logo", SqlDbType.VarBinary, -1).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@laboratory_logo", SqlDbType.VarBinary, -1).Value = p["laboratory_logo"];

            if (p["accredited_logo"] == null)
                cmd.Parameters.Add("@accredited_logo", SqlDbType.VarBinary, -1).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@accredited_logo", SqlDbType.VarBinary, -1).Value = p["accredited_logo"];

            cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
            cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();        
        }

        private void UpdateLaboratory(SqlConnection conn, SqlTransaction trans)
        {            
            p["update_date"] = DateTime.Now;
            p["update_id"] = Common.UserId;                

            SqlCommand cmd = new SqlCommand("csp_update_laboratory", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", p["id"]);
            cmd.Parameters.AddWithValue("@name", p["name"]);
            cmd.Parameters.AddWithValue("@name_prefix", p["name_prefix"]);                
            cmd.Parameters.AddWithValue("@address", p["address"], String.Empty);
            cmd.Parameters.AddWithValue("@email", p["email"], String.Empty);
            cmd.Parameters.AddWithValue("@phone", p["phone"], String.Empty);
            cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
            cmd.Parameters.AddWithValue("@comment", p["comment"], String.Empty);

            if (p["laboratory_logo"] == null)
                cmd.Parameters.Add("@laboratory_logo", SqlDbType.VarBinary, -1).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@laboratory_logo", SqlDbType.VarBinary, -1).Value = p["laboratory_logo"];

            if (p["accredited_logo"] == null)
                cmd.Parameters.Add("@accredited_logo", SqlDbType.VarBinary, -1).Value = DBNull.Value;
            else
                cmd.Parameters.Add("@accredited_logo", SqlDbType.VarBinary, -1).Value = p["accredited_logo"];

            cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
            cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
            cmd.ExecuteNonQuery();                
        }

        private void picLaboratoryLogo_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;
            
            p["laboratory_logo"] = File.ReadAllBytes(dialog.FileName);
            picLaboratoryLogo.Image = Image.FromStream(new MemoryStream((byte[])p["laboratory_logo"]));
            lblLaboratoryLogoSize.Text = picLaboratoryLogo.Image.Width.ToString() + " x " + picLaboratoryLogo.Image.Height.ToString();
        }

        private void picAccreditedLogo_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG)|*.BMP;*.JPG;*.JPEG;*.GIF;*.PNG";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            p["accredited_logo"] = File.ReadAllBytes(dialog.FileName);
            picAccreditedLogo.Image = Image.FromStream(new MemoryStream((byte[])p["accredited_logo"]));
            lblAccreditedLogoSize.Text = picAccreditedLogo.Image.Width.ToString() + " x " + picAccreditedLogo.Image.Height.ToString();
        }
    }
}
