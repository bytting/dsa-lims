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
using log4net;

namespace DSA_lims
{
    public partial class FormEnergyLine : Form
    {
        private ILog mLog = null;
        EnergyLineModel EnergyLine = new EnergyLineModel();        

        public FormEnergyLine(ILog log, Guid nid, string nname)
        {            
            InitializeComponent();

            // Creating a new line     
            mLog = log;
            EnergyLine.NuclideId = nid;
            EnergyLine.Id = Guid.Empty;

            Text = "Create energy line";
            tbNuclide.Text = nname;
            cbInUse.Checked = true;
        }

        public FormEnergyLine(ILog log, Guid nid, Guid eid, string nname)
        {            
            InitializeComponent();

            // Update existing line            
            mLog = log;
            EnergyLine.NuclideId = nid;
            EnergyLine.Id = eid;

            Text = "Update energy line";
            tbNuclide.Text = nname;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_nuclide_transmission", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", EnergyLine.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Energy line with ID " + EnergyLine.Id.ToString() + " was not found");

                    reader.Read();
                    tbTransFrom.Text = reader["transmission_from"].ToString();
                    tbTransTo.Text = reader["transmission_to"].ToString();
                    tbEnergy.Text = reader["energy"].ToString();
                    tbEnergyUnc.Text = reader["energy_uncertainty"].ToString();
                    tbIntensity.Text = reader["intensity"].ToString();
                    tbIntensityUnc.Text = reader["intensity_uncertainty"].ToString();
                    tbProbOfDecay.Text = reader["probability_of_decay"].ToString();
                    tbProbOfDecayUnc.Text = reader["probability_of_decay_uncertainty"].ToString();
                    tbTotInternalConv.Text = reader["total_internal_conversion"].ToString();
                    tbKShellConv.Text = reader["kshell_conversion"].ToString();
                    cbInUse.Checked = InstanceStatus.IsActive(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    EnergyLine.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    EnergyLine.CreatedBy = reader["created_by"].ToString();
                    EnergyLine.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    EnergyLine.UpdatedBy = reader["updated_by"].ToString();
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
            if(String.IsNullOrEmpty(tbTransFrom.Text.Trim()))
            {
                MessageBox.Show("Transmission from is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbTransTo.Text.Trim()))
            {
                MessageBox.Show("Transmission to is mandatory");
                return;
            }
                        
            EnergyLine.TransmissionFrom = Convert.ToDouble(tbTransFrom.Text.Trim());
            EnergyLine.TransmissionTo = Convert.ToDouble(tbTransTo.Text.Trim());
            EnergyLine.Energy = Convert.ToDouble(tbEnergy.Text.Trim());
            EnergyLine.EnergyUncertainty = Convert.ToDouble(tbEnergyUnc.Text.Trim());
            EnergyLine.Intensity = Convert.ToDouble(tbIntensity.Text.Trim());
            EnergyLine.IntensityUncertainty = Convert.ToDouble(tbIntensityUnc.Text.Trim());
            EnergyLine.ProbabilityOfDecay = Convert.ToDouble(tbProbOfDecay.Text.Trim());
            EnergyLine.ProbabilityOfDecayUncertainty = Convert.ToDouble(tbProbOfDecayUnc.Text.Trim());
            EnergyLine.TotalInternalConversion = Convert.ToDouble(tbTotInternalConv.Text.Trim());
            EnergyLine.KShellConversion = Convert.ToDouble(tbKShellConv.Text.Trim());
            EnergyLine.InstanceStatusId = cbInUse.Checked == true ? 1 : 2;
            EnergyLine.Comment = tbComment.Text.Trim();

            bool success;
            if (EnergyLine.Id == Guid.Empty)
                success = InsertEnergyLine();
            else
                success = UpdateEnergyLine();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertEnergyLine()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                EnergyLine.CreateDate = DateTime.Now;
                EnergyLine.CreatedBy = Common.Username;
                EnergyLine.UpdateDate = DateTime.Now;
                EnergyLine.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_nuclide_transmission", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                EnergyLine.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", EnergyLine.Id);
                cmd.Parameters.AddWithValue("@nuclide_id", EnergyLine.NuclideId);
                cmd.Parameters.AddWithValue("@transmission_from", EnergyLine.TransmissionFrom);
                cmd.Parameters.AddWithValue("@transmission_to", EnergyLine.TransmissionTo);
                cmd.Parameters.AddWithValue("@energy", EnergyLine.Energy);
                cmd.Parameters.AddWithValue("@energy_uncertainty", EnergyLine.EnergyUncertainty);
                cmd.Parameters.AddWithValue("@intensity", EnergyLine.Intensity);
                cmd.Parameters.AddWithValue("@intensity_uncertainty", EnergyLine.IntensityUncertainty);
                cmd.Parameters.AddWithValue("@probability_of_decay", EnergyLine.ProbabilityOfDecay);
                cmd.Parameters.AddWithValue("@probability_of_decay_uncertainty", EnergyLine.ProbabilityOfDecayUncertainty);
                cmd.Parameters.AddWithValue("@total_internal_conversion", EnergyLine.TotalInternalConversion);
                cmd.Parameters.AddWithValue("@kshell_conversion", EnergyLine.KShellConversion);
                cmd.Parameters.AddWithValue("@instance_status_id", EnergyLine.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", EnergyLine.Comment);
                cmd.Parameters.AddWithValue("@create_date", EnergyLine.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", EnergyLine.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", EnergyLine.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", EnergyLine.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide_transmission", EnergyLine.Id, AuditOperation.Insert, JsonConvert.SerializeObject(EnergyLine));

                transaction.Commit();
            }
            catch(Exception ex)
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

        private bool UpdateEnergyLine()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                EnergyLine.UpdateDate = DateTime.Now;
                EnergyLine.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();            
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_nuclide_transmission", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", EnergyLine.Id);
                cmd.Parameters.AddWithValue("@transmission_from", EnergyLine.TransmissionFrom);
                cmd.Parameters.AddWithValue("@transmission_to", EnergyLine.TransmissionTo);
                cmd.Parameters.AddWithValue("@energy", EnergyLine.Energy);
                cmd.Parameters.AddWithValue("@energy_uncertainty", EnergyLine.EnergyUncertainty);
                cmd.Parameters.AddWithValue("@intensity", EnergyLine.Intensity);
                cmd.Parameters.AddWithValue("@intensity_uncertainty", EnergyLine.IntensityUncertainty);
                cmd.Parameters.AddWithValue("@probability_of_decay", EnergyLine.ProbabilityOfDecay);
                cmd.Parameters.AddWithValue("@probability_of_decay_uncertainty", EnergyLine.ProbabilityOfDecayUncertainty);
                cmd.Parameters.AddWithValue("@total_internal_conversion", EnergyLine.TotalInternalConversion);
                cmd.Parameters.AddWithValue("@kshell_conversion", EnergyLine.KShellConversion);
                cmd.Parameters.AddWithValue("@instance_status_id", EnergyLine.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", EnergyLine.Comment);
                cmd.Parameters.AddWithValue("@update_date", EnergyLine.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", EnergyLine.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "nuclide_transmission", EnergyLine.Id, AuditOperation.Update, JsonConvert.SerializeObject(EnergyLine));

                transaction.Commit();
            }
            catch(Exception ex)
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
