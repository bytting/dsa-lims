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
        public NuclideModel Nuclide = new NuclideModel();

        public FormNuclide()
        {
            InitializeComponent();

            // Create new nuclide            
            PopulateDecayTypes();
            Text = "New nuclide";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;            
        }

        public FormNuclide(Guid nid)
        {
            InitializeComponent();

            // Edit existing nuclide            
            PopulateDecayTypes();            
            Nuclide.Id = nid;
            Text = "Edit nuclide";
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_nuclide", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", nid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if(!reader.HasRows)                    
                        throw new Exception("Nuclide with ID " + Nuclide.Id.ToString() + " was not found");                    

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    tbNumberOfProtons.Text = reader["proton_count"].ToString();
                    tbNumberOfNeutrons.Text = reader["neutron_count"].ToString();
                    tbHalflife.Text = reader["half_life_year"].ToString();
                    tbHalflifeUncertainty.Text = reader["half_life_uncertainty"].ToString();
                    cboxDecayTypes.SelectedValue = Convert.ToInt32(reader["decay_type_id"]);
                    tbKXrayEnergy.Text = reader["kxray_energy"].ToString();
                    tbFluorescenceYield.Text = reader["fluorescence_yield"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    Nuclide.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    Nuclide.CreatedBy = reader["created_by"].ToString();
                    Nuclide.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    Nuclide.UpdatedBy = reader["updated_by"].ToString();
                }
            }            
        }

        private void FormNuclide_Load(object sender, EventArgs e)
        {            
        }

        private void PopulateDecayTypes()
        {
            cboxDecayTypes.DataSource = Common.DecayTypeList;
            cboxDecayTypes.SelectedIndex = -1;
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

            if (cboxDecayTypes.SelectedIndex < 0)
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

            Nuclide.Name = tbName.Text.Trim();
            Nuclide.ProtonCount = Convert.ToInt32(tbNumberOfProtons.Text.Trim());
            Nuclide.NeutronCount = Convert.ToInt32(tbNumberOfNeutrons.Text.Trim());
            Nuclide.HalfLife = Convert.ToDouble(tbHalflife.Text.Trim());
            Nuclide.HalfLifeUncertainty = Convert.ToDouble(tbHalflifeUncertainty.Text.Trim());
            Nuclide.DecayTypeId = Convert.ToInt32(cboxDecayTypes.SelectedValue);
            Nuclide.XRayEnergy = Convert.ToDouble(tbKXrayEnergy.Text.Trim());
            Nuclide.FluorescenceYield = Convert.ToDouble(tbFluorescenceYield.Text.Trim());
            Nuclide.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            Nuclide.Comment = tbComment.Text.Trim();

            bool success;
            if (Nuclide.Id == Guid.Empty)
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
                Nuclide.CreateDate = DateTime.Now;
                Nuclide.CreatedBy = Common.Username;
                Nuclide.UpdateDate = DateTime.Now;
                Nuclide.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_nuclide", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                Nuclide.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", Nuclide.Id);
                cmd.Parameters.AddWithValue("@name", Nuclide.Name);
                cmd.Parameters.AddWithValue("@proton_count", Nuclide.ProtonCount);
                cmd.Parameters.AddWithValue("@neutron_count", Nuclide.NeutronCount);
                cmd.Parameters.AddWithValue("@half_life_year", Nuclide.HalfLife);
                cmd.Parameters.AddWithValue("@half_life_uncertainty", Nuclide.HalfLifeUncertainty);
                cmd.Parameters.AddWithValue("@decay_type_id", Nuclide.DecayTypeId);
                cmd.Parameters.AddWithValue("@kxray_energy", Nuclide.XRayEnergy);
                cmd.Parameters.AddWithValue("@fluorescence_yield", Nuclide.FluorescenceYield);
                cmd.Parameters.AddWithValue("@instance_status_id", Nuclide.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Nuclide.Comment);
                cmd.Parameters.AddWithValue("@create_date", Nuclide.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", Nuclide.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", Nuclide.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Nuclide.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide", Nuclide.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(Nuclide));

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
                Nuclide.UpdateDate = DateTime.Now;
                Nuclide.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_nuclide", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Nuclide.Id);
                cmd.Parameters.AddWithValue("@name", Nuclide.Name);
                cmd.Parameters.AddWithValue("@proton_count", Nuclide.ProtonCount);
                cmd.Parameters.AddWithValue("@neutron_count", Nuclide.NeutronCount);
                cmd.Parameters.AddWithValue("@half_life_year", Nuclide.HalfLife);
                cmd.Parameters.AddWithValue("@half_life_uncertainty", Nuclide.HalfLifeUncertainty);
                cmd.Parameters.AddWithValue("@decay_type_id", Nuclide.DecayTypeId);
                cmd.Parameters.AddWithValue("@kxray_energy", Nuclide.XRayEnergy);
                cmd.Parameters.AddWithValue("@fluorescence_yield", Nuclide.FluorescenceYield);
                cmd.Parameters.AddWithValue("@instance_status_id", Nuclide.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Nuclide.Comment);
                cmd.Parameters.AddWithValue("@update_date", Nuclide.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", Nuclide.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide", Nuclide.Id, AuditOperationType.Update, JsonConvert.SerializeObject(Nuclide));

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
