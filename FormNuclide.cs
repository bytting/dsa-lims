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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormNuclide : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();

        public Guid NuclideId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string NuclideName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }        

        public FormNuclide()
        {
            InitializeComponent();

            // Create new nuclide                                    
            Text = "New nuclide";            
            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.LoadIntList(conn, "csp_select_instance_status");
                cboxDecayTypes.DataSource = DB.LoadIntList(conn, "csp_select_decay_types", true);
            }
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
            cboxDecayTypes.SelectedValue = -1;
        }

        public FormNuclide(Guid nid)
        {
            InitializeComponent();

            // Edit existing nuclide
            p["id"] = nid;
            Text = "Edit nuclide";

            using (SqlConnection conn = DB.OpenConnection())
            {
                cboxInstanceStatus.DataSource = DB.LoadIntList(conn, "csp_select_instance_status");
                cboxDecayTypes.DataSource = DB.LoadIntList(conn, "csp_select_decay_types", true);

                SqlCommand cmd = new SqlCommand("csp_select_nuclide", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", nid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if(!reader.HasRows)                    
                        throw new Exception("Nuclide with ID " + p["id"] + " was not found");                    

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbNumberOfProtons.Text = reader["proton_count"].ToString();
                    tbNumberOfNeutrons.Text = reader["neutron_count"].ToString();
                    tbHalflife.Text = reader["half_life_year"].ToString();
                    tbHalflifeUncertainty.Text = reader["half_life_uncertainty"].ToString();
                    cboxDecayTypes.SelectedValue = reader["decay_type_id"];
                    tbKXrayEnergy.Text = reader["kxray_energy"].ToString();
                    tbFluorescenceYield.Text = reader["fluorescence_yield"].ToString();
                    cboxInstanceStatus.SelectedValue = reader["instance_status_id"];
                    tbComment.Text = reader["comment"].ToString();
                    p["create_date"] = reader["create_date"];
                    p["created_by"] = reader["created_by"];
                    p["update_date"] = reader["update_date"];
                    p["updated_by"] = reader["updated_by"];
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
                MessageBox.Show("Nuclide name is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbNumberOfProtons.Text.Trim()))
            {
                MessageBox.Show("Number of protons is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbNumberOfNeutrons.Text.Trim()))
            {
                MessageBox.Show("Number of neutrons is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbHalflife.Text.Trim()))
            {
                MessageBox.Show("Halflife is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbHalflifeUncertainty.Text.Trim()))
            {
                MessageBox.Show("Halflife uncertainty is mandatory");
                return;
            }                

            if ((int)cboxDecayTypes.SelectedValue == -1)
            {
                MessageBox.Show("Decay type is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbKXrayEnergy.Text.Trim()))
            {
                MessageBox.Show("KXray energy is mandatory");
                return;
            }                

            if (String.IsNullOrEmpty(tbFluorescenceYield.Text.Trim()))
            {
                MessageBox.Show("Fluorescence yield is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();
            p["proton_count"] = Convert.ToInt32(tbNumberOfProtons.Text.Trim());
            p["neutron_count"] = Convert.ToInt32(tbNumberOfNeutrons.Text.Trim());
            p["halflife"] = Convert.ToDouble(tbHalflife.Text.Trim());
            p["halflife_uncertainty"] = Convert.ToDouble(tbHalflifeUncertainty.Text.Trim());
            p["decay_type_id"] = Convert.ToInt32(cboxDecayTypes.SelectedValue);
            p["xray_energy"] = Convert.ToDouble(tbKXrayEnergy.Text.Trim());
            p["fluorescence_yield"] = Convert.ToDouble(tbFluorescenceYield.Text.Trim());
            p["instance_status_id"] = cboxInstanceStatus.SelectedValue;
            p["comment"] = tbComment.Text.Trim();

            bool success;
            if (!p.ContainsKey("id"))
                success = InsertNuclide();
            else
                success = UpdateNuclide();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();            
        }

        private bool InsertNuclide()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["create_date"] = DateTime.Now;
                p["created_by"] = Common.Username;
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_nuclide", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@proton_count", p["proton_count"]);
                cmd.Parameters.AddWithValue("@neutron_count", p["neutron_count"]);
                cmd.Parameters.AddWithValue("@half_life_year", p["halflife"]);
                cmd.Parameters.AddWithValue("@half_life_uncertainty", p["halflife_uncertainty"]);
                cmd.Parameters.AddWithValue("@decay_type_id", p["decay_type_id"]);
                cmd.Parameters.AddWithValue("@kxray_energy", p["xray_energy"]);
                cmd.Parameters.AddWithValue("@fluorescence_yield", p["fluorescence_yield"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@comment", p["comment"]);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
                cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide", (Guid)p["id"], AuditOperationType.Insert, JsonConvert.SerializeObject(p));

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

        private bool UpdateNuclide()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_nuclide", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@proton_count", p["proton_count"]);
                cmd.Parameters.AddWithValue("@neutron_count", p["neutron_count"]);
                cmd.Parameters.AddWithValue("@half_life_year", p["halflife"]);
                cmd.Parameters.AddWithValue("@half_life_uncertainty", p["halflife_uncertainty"]);
                cmd.Parameters.AddWithValue("@decay_type_id", p["decay_type_id"]);
                cmd.Parameters.AddWithValue("@kxray_energy", p["xray_energy"]);
                cmd.Parameters.AddWithValue("@fluorescence_yield", p["fluorescence_yield"]);
                cmd.Parameters.AddWithValue("@instance_status_id", p["instance_status_id"]);
                cmd.Parameters.AddWithValue("@comment", p["comment"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide", (Guid)p["id"], AuditOperationType.Update, JsonConvert.SerializeObject(p));

                transaction.Commit();
            }
            catch(Exception ex)
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
