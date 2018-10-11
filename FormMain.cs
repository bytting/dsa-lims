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
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.Reflection;
using System.Data.SqlClient;
using log4net;
using System.IO;
using System.Xml.Serialization;

namespace DSA_lims
{
    public partial class FormMain : Form
    {
        private ResourceManager r = null;

        public FormMain()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        }        

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                Common.Log = DSALogger.CreateLogger(tbLog);

                tabs.Appearance = TabAppearance.FlatButtons;
                tabs.ItemSize = new Size(0, 1);
                tabs.SizeMode = TabSizeMode.Fixed;
                tabs.SelectedTab = tabMenu;
                lblCurrentTab.Text = tabs.SelectedTab.Text;
                lblStatus.Text = "";                
                lblSampleToolId.Text = "";
                lblSampleToolExId.Text = "";
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                lblSampleToolLaboratory.Text = "";

                tbMenuLookup.Text = "";
                ActiveControl = tbMenuLookup;

                r = new ResourceManager("DSA_lims.lang_" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                Common.Log.Info("Setting language " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                SetLanguageLabels(r);

                Common.Log.Info("Loading settings file " + DSAEnvironment.SettingsFilename);
                LoadSettings(DSAEnvironment.SettingsFilename);
                cbMachineSettingsUseAD.Checked = Common.Settings.UseActiveDirectoryCredentials;

                using (SqlConnection conn = DB.OpenConnection())
                {
                    Common.Username = "Admin"; // FIXME

                    DB.LoadInstanceStatus(conn);
                    DB.LoadDecayTypes(conn);
                    DB.LoadPreparationUnits(conn);
                    DB.LoadUniformActivityUnits(conn);
                    DB.LoadWorkflowStatus(conn);
                    DB.LoadLocationTypes(conn);                    

                    UI.PopulateInstanceStatus(cboxSamplesStatus);
                    UI.PopulatePreparationUnits(cboxSamplePrepUnit);
                    UI.PopulateWorkflowStatus(cboxSampleAnalWorkflowStatus, cboxSamplePrepWorkflowStatus);
                    UI.PopulateLocationTypes(cboxSampleInfoLocationTypes);
                    UI.PopulateActivityUnits(conn, gridMetaUnitsActivity);
                    UI.PopulateActivityUnits(conn, cboxSampleAnalUnit);
                    UI.PopulateActivityUnitTypes(conn, cboxSampleAnalUnitType);
                    UI.PopulateProjectsMain(conn, gridProjectMain);
                    UI.PopulateProjectsMain(conn, cboxSampleProject, cboxSamplesProjects);
                    UI.PopulateLaboratories(conn, gridMetaLab);
                    UI.PopulateLaboratories(conn, cboxSampleLaboratory);
                    UI.PopulateUsers(conn, gridMetaUsers);
                    UI.PopulateNuclides(conn, gridSysNuclides);
                    UI.PopulateGeometries(conn, gridSysGeom);
                    UI.PopulateGeometries(conn, cboxSamplePrepGeom);
                    UI.PopulateCounties(conn, gridSysCounty);
                    UI.PopulateCounties(conn, cboxSampleCounties);
                    UI.PopulateStations(conn, gridMetaStation);
                    UI.PopulateStations(conn, cboxSampleInfoStations);
                    UI.PopulateSampleStorage(conn, gridMetaSampleStorage);
                    UI.PopulateSampleStorage(conn, cboxSampleSampleStorage);
                    UI.PopulateSamplers(conn, gridMetaSamplers);
                    UI.PopulateSamplers(conn, cboxSampleInfoSampler);
                    UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);
                    UI.PopulateSamplingMethods(conn, cboxSampleInfoSamplingMeth);
                    UI.PopulateSamples(conn, gridSamples);
                    UI.PopulatePreparationMethods(conn, gridTypeRelPrepMeth);
                    UI.PopulateAnalysisMethods(conn, gridTypeRelAnalMeth);
                    UI.PopulateSampleTypes(conn, treeSampleTypes);
                    UI.PopulateSampleTypes(treeSampleTypes, cboxSampleSampleType);
                }
                
                HideMenuItems();

                Common.Log.Info("Application loaded successfully");
            }
            catch(Exception ex)
            {
                Common.Log.Fatal(ex.Message, ex);
                Environment.Exit(1);
            }
        }        

        private void FormMain_Shown(object sender, EventArgs e)
        {
            //ShowLogin();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.Log.Info("Application closing down");            
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Common.Log.Info("Application closed successfully");
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            //
        }

        private void SetStatusMessage(string msg, StatusMessageType msgLevel = StatusMessageType.Success)
        {
            switch(msgLevel)
            {
                case StatusMessageType.Success:
                    lblStatus.Text = StrUtils.makeStatusMessage(msg);
                    lblStatus.ForeColor = SystemColors.ControlText;
                    break;
                case StatusMessageType.Warning:
                    lblStatus.Text = StrUtils.makeStatusMessage(msg);
                    lblStatus.ForeColor = Color.OrangeRed;
                    break;
                case StatusMessageType.Error:
                    lblStatus.Text = StrUtils.makeErrorMessage(msg);
                    lblStatus.ForeColor = Color.Red;
                    break;
            }            
        }

        private void ClearStatusMessage()
        {
            lblStatus.Text = "";
            lblStatus.ForeColor = SystemColors.ControlText;
        }

        private void HideMenuItems()
        {
            miSample.Visible = miOrder.Visible = miSearch.Visible = miMeta.Visible = miOrders.Visible 
                = miSamples.Visible = miProjects.Visible = miCustomers.Visible = miTypeRelations.Visible 
                = miAuditLog.Visible = miLog.Visible = miSys.Visible = false;
        }

        private void HideMetaMenuItems()
        {
            miStations.Visible = false;
            miSampleStorage.Visible = false;
            miUnits.Visible = false;
            miSamplers.Visible = false;
            miSamplingMethods.Visible = false;            
        }

        private void HideSysMenuItems()
        {
            miLaboratories.Visible = false;
            miUsers.Visible = false;
            miNuclides.Visible = false;
            miMunicipalities.Visible = false;
            miAccreditationRules.Visible = false;
            miGeometries.Visible = false;
        }        

        private void miLogout_Click(object sender, EventArgs e)
        {
            ShowLogin();
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowLogin()
        {
            FormLogin formLogin = new FormLogin(Common.Settings);
            if (formLogin.ShowDialog() != DialogResult.OK)
                Close();
        }

        public void SaveSettings(string settingsFilename)
        {
            try
            {
                // Serialize settings to file
                using (StreamWriter sw = new StreamWriter(settingsFilename))
                {
                    XmlSerializer x = new XmlSerializer(Common.Settings.GetType());
                    x.Serialize(sw, Common.Settings);
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex.Message, ex);
            }
        }

        public void LoadSettings(string settingsFilename)
        {
            try
            {
                if (!File.Exists(settingsFilename))
                    SaveSettings(settingsFilename);

                // Deserialize settings from file
                using (StreamReader sr = new StreamReader(settingsFilename))
                {
                    XmlSerializer x = new XmlSerializer(Common.Settings.GetType());
                    Common.Settings = x.Deserialize(sr) as DSASettings;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex.Message, ex);
            }
        }        

        private void SetLanguageLabels(ResourceManager r)
        {
            lblMenuSamples.Text = r.GetString(lblMenuSamples.Name);
            lblMenuOrders.Text = r.GetString(lblMenuOrders.Name);
            btnMenuNewSample.Text = r.GetString(btnMenuNewSample.Name);
            miFile.Text = r.GetString(miFile.Name);
        }        

        private void tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblCurrentTab.Text = tabs.SelectedTab.Text;

            HideMenuItems();

            if (tabs.SelectedTab == tabMenu)
            {
                tbMenuLookup.Text = "";
                ActiveControl = tbMenuLookup;
            }
            else if (tabs.SelectedTab == tabSample)
            {
                miSample.Visible = true;
            }
            else if (tabs.SelectedTab == tabOrder)
            {
                miOrder.Visible = true;
            }
            else if (tabs.SelectedTab == tabSearch)
            {
                miSearch.Visible = true;
            }            
            else if(tabs.SelectedTab == tabMetadata)
            {                
                miMeta.Visible = true;
                tabsMeta_SelectedIndexChanged(sender, e);
            }
            else if (tabs.SelectedTab == tabOrders)
            {
                miOrders.Visible = true;
            }
            else if (tabs.SelectedTab == tabSamples)
            {
                miSamples.Visible = true;
                tbSamplesLookup.Text = "";
                ActiveControl = tbSamplesLookup;
            }            
            else if (tabs.SelectedTab == tabProjects)
            {
                miProjects.Visible = true;
            }
            else if (tabs.SelectedTab == tabCustomers)
            {
                miCustomers.Visible = true;
            }
            else if (tabs.SelectedTab == tabTypeRel)
            {
                miTypeRelations.Visible = true;
            }
            else if (tabs.SelectedTab == tabAuditLog)
            {
                miAuditLog.Visible = true;
            }
            else if (tabs.SelectedTab == tabLog)
            {
                miLog.Visible = true;
            }
            else if (tabs.SelectedTab == tabSysdata)
            {
                miSys.Visible = true;
                tabsSys_SelectedIndexChanged(sender, e);
            }                        
        }

        private void btnSamples_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSamples;
        }                

        private void btnOrders_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrders;
        }                                                        

        private void tabsSample_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
        }

        private void btnMenuNewOrder_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrder;
        }

        private void treeSampleTypes_AfterSelect(object sender, TreeViewEventArgs e)
        {            
            TreeNode tnode = e.Node;

            lblTypeRelSampCompSel.Text = lblTypeRelSampParSel.Text = lblTypeRelSampPrepSel.Text = tnode.Text;

            try
            {
                lbSampleTypesComponents.Items.Clear();
                lbSampleTypesInheritedComponents.Items.Clear();

                Guid sampleTypeId = new Guid(tnode.Name);

                using (SqlConnection conn = DB.OpenConnection())
                {
                    // add sample components
                    AddSampleTypeComponents(conn, sampleTypeId, false, tnode);

                    // add preparation methods
                    UI.PopulateSampleTypePrepMeth(conn, tnode, lbTypeRelSampTypePrepMeth, lbTypeRelSampTypeInheritedPrepMeth);
                }
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
            }            
        }        

        private void AddSampleTypeComponents(SqlConnection conn, Guid sampleTypeId, bool inherited, TreeNode tnode)
        {
            ListBox lb = inherited ? lbSampleTypesInheritedComponents : lbSampleTypesComponents;

            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    lb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = new Guid(tnode.Parent.Name);
                AddSampleTypeComponents(conn, parentId, true, tnode.Parent);
            }
        }

        private void miUserSettings_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabUserSettings;
        }

        private void miMachineSettings_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabMachineSettings;
        }

        private void btnMachineSettingsSave_Click(object sender, EventArgs e)
        {
            Common.Settings.UseActiveDirectoryCredentials = cbMachineSettingsUseAD.Checked;

            SaveSettings(DSAEnvironment.SettingsFilename);
            SetStatusMessage("Settings saved");
        }

        private void btnUserSettingsSave_Click(object sender, EventArgs e)
        {
            //
        }        

        private void btnSampleSave_Click(object sender, EventArgs e)
        {
            if(cboxSampleLaboratory.SelectedItem == null)
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (cboxSampleSampleType.SelectedItem == null)
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if (cboxSampleProject.SelectedItem == null)
            {
                MessageBox.Show("Main project is mandatory");
                return;
            }

            if (cboxSampleSubProject.SelectedItem == null)
            {
                MessageBox.Show("Sub project is mandatory");
                return;
            }

            try
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlParameter sampleNumber = new SqlParameter("@number", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    SqlCommand cmd = new SqlCommand("csp_insert_sample", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@laboratory_id", Lemma<Guid, string>.IdParam(cboxSampleLaboratory.SelectedItem));
                    cmd.Parameters.AddWithValue("@sample_type_id", Lemma<Guid, string>.IdParam(cboxSampleSampleType.SelectedItem));
                    cmd.Parameters.AddWithValue("@sample_storage_id", Lemma<Guid, string>.IdParam(cboxSampleSampleStorage.SelectedItem));
                    cmd.Parameters.AddWithValue("@sample_component_id", Lemma<Guid, string>.IdParam(cboxSampleSampleComponent.SelectedItem));
                    cmd.Parameters.AddWithValue("@project_sub_id", Lemma<Guid, string>.IdParam(cboxSampleSubProject.SelectedItem));
                    cmd.Parameters.AddWithValue("@station_id", Lemma<Guid, string>.IdParam(cboxSampleInfoStations.SelectedItem));
                    cmd.Parameters.AddWithValue("@sampler_id", Lemma<Guid, string>.IdParam(cboxSampleInfoSampler.SelectedItem));
                    cmd.Parameters.AddWithValue("@transform_from_id", Guid.Empty);
                    cmd.Parameters.AddWithValue("@transform_to_id", Guid.Empty);
                    cmd.Parameters.AddWithValue("@current_order_id", Guid.Empty);
                    cmd.Parameters.AddWithValue("@imported_from", DBNull.Value);
                    cmd.Parameters.AddWithValue("@imported_from_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@municipality_id", Lemma<Guid, string>.IdParam(cboxSampleMunicipalities.SelectedItem));
                    cmd.Parameters.AddWithValue("@location_type", DB.MakeParam(typeof(string), cboxSampleInfoLocationTypes.Text));
                    cmd.Parameters.AddWithValue("@location", DB.MakeParam(typeof(string), tbSampleLocation.Text.Trim()));
                    cmd.Parameters.AddWithValue("@latitude", DB.MakeParam(typeof(double), tbSampleInfoLatitude.Text.Trim()));
                    cmd.Parameters.AddWithValue("@longitude", DB.MakeParam(typeof(double), tbSampleInfoLongitude.Text.Trim()));
                    cmd.Parameters.AddWithValue("@altitude", DB.MakeParam(typeof(double), tbSampleInfoAltitude.Text.Trim()));
                    cmd.Parameters.AddWithValue("@sampling_date_from", new DateTime(dtSampleSamplingDateFrom.Value.Year, dtSampleSamplingDateFrom.Value.Month, dtSampleSamplingDateFrom.Value.Day, dtSampleSamplingTimeFrom.Value.Hour, dtSampleSamplingTimeFrom.Value.Minute, dtSampleSamplingTimeFrom.Value.Second));
                    cmd.Parameters.AddWithValue("@sampling_date_to", new DateTime(dtSampleSamplingDateTo.Value.Year, dtSampleSamplingDateTo.Value.Month, dtSampleSamplingDateTo.Value.Day, dtSampleSamplingTimeTo.Value.Hour, dtSampleSamplingTimeTo.Value.Minute, dtSampleSamplingTimeTo.Value.Second));
                    cmd.Parameters.AddWithValue("@reference_date", new DateTime(dtSampleReferenceDate.Value.Year, dtSampleReferenceDate.Value.Month, dtSampleReferenceDate.Value.Day, dtSampleReferenceTime.Value.Hour, dtSampleReferenceTime.Value.Minute, dtSampleReferenceTime.Value.Second));
                    cmd.Parameters.AddWithValue("@external_id", DB.MakeParam(typeof(string), tbSampleExId.Text.Trim()));
                    cmd.Parameters.AddWithValue("@wet_weight_g", DBNull.Value);
                    cmd.Parameters.AddWithValue("@dry_weight_g", DBNull.Value);
                    cmd.Parameters.AddWithValue("@volume_l", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_weight_start", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_weight_end", DBNull.Value);
                    cmd.Parameters.AddWithValue("@lod_temperature", DBNull.Value);
                    cmd.Parameters.AddWithValue("@confidential", cbSampleConfidential.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@parameters", DBNull.Value);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@comment", tbSampleComment.Text.Trim());
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);
                    cmd.Parameters.Add(sampleNumber);

                    cmd.ExecuteNonQuery();
                    
                    lblStatus.Text = StrUtils.makeStatusMessage("Sample " + sampleNumber.Value.ToString() + " created");

                    UI.PopulateSamples(conn, gridSamples);

                    tabs.SelectedTab = tabSamples;

                    tbSamplesLookup.Text = sampleNumber.Value.ToString();                    
                }
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
            }            
        }

        private void btnSamplesOpen_Click(object sender, EventArgs e)
        {            
            tabs.SelectedTab = tabSample;
        }

        private void btnOrdersOpen_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrder;
        }

        private void miNewLaboratory_Click(object sender, EventArgs e)
        {            
            FormLaboratory form = new FormLaboratory();
            switch(form.ShowDialog())
            {                
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.LaboratoryName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateLaboratories(conn, gridMetaLab);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create laboratory failed", StatusMessageType.Error);
                    break;
            }                            
        }

        private void miDeleteLaboratory_Click(object sender, EventArgs e)
        {
            // delete laboratory
        }

        private void tabsMeta_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideMetaMenuItems();

            if(tabsMeta.SelectedTab == tabSysLaboratories)
            {
                miLaboratories.Visible = true;
            }
            else if (tabsMeta.SelectedTab == tabSysUsers)
            {
                miUsers.Visible = true;
            }
        }

        private void tabsSys_SelectedIndexChanged(object sender, EventArgs e)
        {            
            HideSysMenuItems();

            if (tabsSys.SelectedTab == tabSysLaboratories)
            {
                miLaboratories.Visible = true;
            }
            else if (tabsSys.SelectedTab == tabSysUsers)
            {
                miUsers.Visible = true;
            }
        }

        private void miNewUser_Click(object sender, EventArgs e)
        {
            // New user
        }

        private void miDeleteUser_Click(object sender, EventArgs e)
        {
            // Delete user
        }

        private void miEditLaboratory_Click(object sender, EventArgs e)
        {
            // edit lab
            if (gridMetaLab.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaLab.SelectedRows[0];
            Guid lid = new Guid(row.Cells[0].Value.ToString());

            FormLaboratory form = new FormLaboratory(lid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.LaboratoryName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateLaboratories(conn, gridMetaLab);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update laboratory failed", StatusMessageType.Error);                    
                    break;
            }                        
        }

        private void miEditUser_Click(object sender, EventArgs e)
        {
            // edit user
        }

        private void miResetPass_Click(object sender, EventArgs e)
        {
            // reset password
        }

        private void miProjectsNew_Click(object sender, EventArgs e)
        {
            // new main project
            FormProject form = new FormProject();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Main project " + form.ProjectName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsMain(conn, gridProjectMain);
                        UI.PopulateProjectsMain(conn, cboxSampleProject, cboxSamplesProjects);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create main project failed", StatusMessageType.Error);
                    break;
            }                        
        }        

        private void miProjectsEdit_Click(object sender, EventArgs e)
        {
            if (gridProjectMain.SelectedRows.Count < 1)
                return;

            Guid pmid = new Guid(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());

            FormProject form = new FormProject(pmid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Project " + form.ProjectName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsMain(conn, gridProjectMain);
                        UI.PopulateProjectsMain(conn, cboxSampleProject, cboxSamplesProjects);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Edit main project failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miProjectsDelete_Click(object sender, EventArgs e)
        {
            // delete project
        }

        private void miProjectsSubNew_Click(object sender, EventArgs e)
        {
            // new sub project
            if (gridProjectMain.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a main project first");
                return;
            }

            Guid pmid = new Guid(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());
            string pmname = gridProjectMain.SelectedRows[0].Cells["name"].Value.ToString();
            
            FormProjectSub form = new FormProjectSub(pmname, pmid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sub project " + form.ProjectSubName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsSub(conn, gridProjectSub, pmid);
                        UI.PopulateProjectsSub(conn, pmid, cboxSampleSubProject);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create main project failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miProjectsSubEdit_Click(object sender, EventArgs e)
        {
            // edit sub project
            if (gridProjectMain.SelectedRows.Count < 1)
                return;

            if (gridProjectSub.SelectedRows.Count < 1)
                return;

            Guid pmid = new Guid(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());
            string pmname = gridProjectMain.SelectedRows[0].Cells["name"].Value.ToString();
            Guid psid = new Guid(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());

            FormProjectSub form = new FormProjectSub(pmname, pmid, psid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Project " + form.ProjectSubName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsSub(conn, gridProjectSub, pmid);
                        UI.PopulateProjectsSub(conn, pmid, cboxSampleSubProject);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Edit sub project failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miProjectsSubDelete_Click(object sender, EventArgs e)
        {
            // delete sub project
        }

        private void miNuclidesNew_Click(object sender, EventArgs e)
        {            
            FormNuclide form = new FormNuclide();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Nuclide " + form.NuclideName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateNuclides(conn, gridSysNuclides);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create nuclide failed", StatusMessageType.Error);
                    break;
            }            
        }        

        private void miSampleTypesNew_Click(object sender, EventArgs e)
        {
            // New sample type
            if (treeSampleTypes.SelectedNode == null)
                return;
            
            FormSampleType form = new FormSampleType(
                new Guid(treeSampleTypes.SelectedNode.Name), 
                treeSampleTypes.SelectedNode.Text, 
                treeSampleTypes.SelectedNode.ToolTipText, 
                false);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSampleTypes(conn, treeSampleTypes);
                        UI.PopulateSampleTypes(treeSampleTypes, cboxSampleSampleType);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sample type failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSampleTypesEdit_Click(object sender, EventArgs e)
        {
            // Edit sample type
            if (treeSampleTypes.SelectedNode == null)
                return;

            FormSampleType form = new FormSampleType(
                new Guid(treeSampleTypes.SelectedNode.Name),
                treeSampleTypes.SelectedNode.Text,
                treeSampleTypes.SelectedNode.ToolTipText,
                true);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSampleTypes(conn, treeSampleTypes);
                        UI.PopulateSampleTypes(treeSampleTypes, cboxSampleSampleType);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update sample type failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSampleTypesDelete_Click(object sender, EventArgs e)
        {
            // Delete sample type
        }

        private void miSystemDataView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSysdata;
        }

        private void miLogView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabLog;
        }

        private void miAuditLogView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabAuditLog;
        }

        private void miSearchView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSearch;
        }

        private void miTypeRelationsView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabTypeRel;
        }

        private void miMetadataView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabMetadata;
        }

        private void miCustomersView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabCustomers;
        }

        private void miProjectsView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabProjects;
        }

        private void miMainMenuView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabMenu;
        }

        private void miNuclidesEdit_Click(object sender, EventArgs e)
        {
            if (gridSysNuclides.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysNuclides.SelectedRows[0];            
            Guid nid = new Guid(row.Cells[0].Value.ToString());

            FormNuclide form = new FormNuclide(nid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Nuclide " + form.NuclideName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateNuclides(conn, gridSysNuclides);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update nuclide failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEnergyLineNew_Click(object sender, EventArgs e)
        {
            // new energy line
            if (gridSysNuclides.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysNuclides.SelectedRows[0];
            Guid nid = new Guid(row.Cells[0].Value.ToString());
            string nname = row.Cells[1].Value.ToString();

            FormEnergyLine form = new FormEnergyLine(nid, nname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Energy line for " + nname + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateEnergyLines(conn, nid, gridSysNuclideTrans);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create energy line failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEnergyLineEdit_Click(object sender, EventArgs e)
        {
            // edit energy line
            if (gridSysNuclides.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysNuclides.SelectedRows[0];
            Guid nid = new Guid(row.Cells[0].Value.ToString());
            string nname = row.Cells[1].Value.ToString();

            DataGridViewRow row2 = gridSysNuclideTrans.SelectedRows[0];
            Guid eid = new Guid(row2.Cells[0].Value.ToString());

            FormEnergyLine form = new FormEnergyLine(nid, eid, nname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Energy line for " + nname + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateEnergyLines(conn, nid, gridSysNuclideTrans);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update energy line failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEnergyLineDelete_Click(object sender, EventArgs e)
        {
            // delete energy line
        }

        private void gridSysNuclides_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
                return;
            
            Guid nid = new Guid(e.Row.Cells[0].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())            
                UI.PopulateEnergyLines(conn, nid, gridSysNuclideTrans);
        }

        private void miNewGeometry_Click(object sender, EventArgs e)
        {         
            FormGeometry form = new FormGeometry();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Geometry " + form.GeometryName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateGeometries(conn, gridSysGeom);
                        UI.PopulateGeometries(conn, cboxSamplePrepGeom);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create geometry failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditGeometry_Click(object sender, EventArgs e)
        {            
            if (gridSysGeom.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysGeom.SelectedRows[0];
            Guid gid = new Guid(row.Cells[0].Value.ToString());

            FormGeometry form = new FormGeometry(gid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Geometry " + form.GeometryName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateGeometries(conn, gridSysGeom);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update geometry failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteGeometry_Click(object sender, EventArgs e)
        {
            // delete geom
        }

        private void miNewCounty_Click(object sender, EventArgs e)
        {            
            FormCounty form = new FormCounty();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.CountyName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateCounties(conn, gridSysCounty);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create county failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditCounty_Click(object sender, EventArgs e)
        {        
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());

            FormCounty form = new FormCounty(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.CountyName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateCounties(conn, gridSysCounty);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create county failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteCounty_Click(object sender, EventArgs e)
        {
            // delete county
        }

        private void miNewMunicipality_Click(object sender, EventArgs e)
        {        
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());

            FormMunicipality form = new FormMunicipality(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Municipality " + form.MunicipalityName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create municipality failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditMunicipality_Click(object sender, EventArgs e)
        {        
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            if (gridSysMunicipality.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());            

            row = gridSysMunicipality.SelectedRows[0];
            Guid mid = new Guid(row.Cells[0].Value.ToString());            

            FormMunicipality form = new FormMunicipality(cid, mid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Municipality " + form.MunicipalityName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update municipality failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteMunicipality_Click(object sender, EventArgs e)
        {
            // delete municipality
        }

        private void gridSysCounty_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
                return;

            Guid cid = new Guid(e.Row.Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())            
                UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
        }

        private void miNewStation_Click(object sender, EventArgs e)
        {        
            FormStation form = new FormStation();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Station " + form.StationName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateStations(conn, gridMetaStation);
                        UI.PopulateStations(conn, cboxSampleInfoStations);
                    }                        
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create station failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditStation_Click(object sender, EventArgs e)
        {            
            if (gridMetaStation.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaStation.SelectedRows[0];
            Guid sid = new Guid(row.Cells[0].Value.ToString());

            FormStation form = new FormStation(sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Station " + form.StationName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateStations(conn, gridMetaStation);
                        UI.PopulateStations(conn, cboxSampleInfoStations);
                    }                        
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update station failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteStation_Click(object sender, EventArgs e)
        {
            // delete station
        }

        private void miNewSampleStorage_Click(object sender, EventArgs e)
        {        
            FormSampleStorage form = new FormSampleStorage();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorageName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateSampleStorage(conn, gridMetaSampleStorage);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sample storage failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditSampleStorage_Click(object sender, EventArgs e)
        {        
            if (gridMetaSampleStorage.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSampleStorage.SelectedRows[0];
            Guid ssid = new Guid(row.Cells[0].Value.ToString());

            FormSampleStorage form = new FormSampleStorage(ssid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorageName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateSampleStorage(conn, gridMetaSampleStorage);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update sample storage failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteSampleStorage_Click(object sender, EventArgs e)
        {
            // delete sample storage
        }

        private void miSamplerNew_Click(object sender, EventArgs e)
        {        
            FormSampler form = new FormSampler();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.SamplerName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplers(conn, gridMetaSamplers);
                        UI.PopulateSamplers(conn, cboxSampleInfoSampler);
                    }                        
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sampler failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSamplerEdit_Click(object sender, EventArgs e)
        {        
            if (gridMetaSamplers.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSamplers.SelectedRows[0];
            Guid sid = new Guid(row.Cells[0].Value.ToString());

            FormSampler form = new FormSampler(sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.SamplerName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplers(conn, gridMetaSamplers);
                        UI.PopulateSamplers(conn, cboxSampleInfoSampler);
                    }                        
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update sampler failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSamplerDelete_Click(object sender, EventArgs e)
        {
            // delete sampler
        }

        private void cboxSampleSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearStatusMessage();
            cboxSampleSampleComponent.Items.Clear();

            if (cboxSampleSampleType.SelectedItem == null)            
                return;

            var sampleType = cboxSampleSampleType.SelectedItem as Lemma<Guid, string>;
            TreeNode[] tnodes = treeSampleTypes.Nodes.Find(sampleType.Id.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                AddSampleTypeComponents(conn, sampleType.Id, tnodes[0]);
            }

            cboxSampleSampleComponent.SelectedIndex = -1;
        }

        private void AddSampleTypeComponents(SqlConnection conn, Guid sampleTypeId, TreeNode tnode)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    cboxSampleSampleComponent.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = new Guid(tnode.Parent.Name);
                AddSampleTypeComponents(conn, parentId, tnode.Parent);
            }
        }

        private void cboxSampleSampleType_Leave(object sender, EventArgs e)
        {
            ClearStatusMessage();

            if (String.IsNullOrEmpty(cboxSampleSampleType.Text.Trim()))
            {
                cboxSampleSampleType.Text = "";
                cboxSampleSampleType.SelectedItem = null;
                cboxSampleSampleComponent.Items.Clear();
                return;
            }

            Lemma<Guid, string> st = cboxSampleSampleType.SelectedItem as Lemma<Guid, string>;
            if (st == null)
            {
                cboxSampleSampleType.Text = "";
                cboxSampleSampleType.SelectedItem = null;
                cboxSampleSampleComponent.Items.Clear();
                SetStatusMessage("You must select an existing sample type", StatusMessageType.Warning);
            }
        }

        private void btnSampleSelectSampleType_Click(object sender, EventArgs e)
        {
            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleSampleType.SelectedIndex = cboxSampleSampleType.FindStringExact(
                form.SelectedSampleTypeName + " -> " + form.SelectedSampleTypePath);
        }

        private void cboxSampleProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxSampleProject.SelectedItem == null)
            {
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                return;
            }

            Lemma<Guid, string> project = cboxSampleProject.SelectedItem as Lemma<Guid, string>;
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectsSub(conn, project.Id, cboxSampleSubProject);
            }

            lblSampleToolProject.Text = "[Project] " + project.Name;
            lblSampleToolSubProject.Text = "";
        }

        private void cboxSampleSubProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxSampleSubProject.SelectedItem == null)
            {
                lblSampleToolSubProject.Text = "";
                return;
            }                

            Lemma<Guid, string> subProject = cboxSampleSubProject.SelectedItem as Lemma<Guid, string>;

            lblSampleToolSubProject.Text = subProject.Name;
        }

        private void cboxSampleInfoStations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cboxSampleInfoStations.SelectedItem == null)
            {
                tbSampleInfoLatitude.Text = "";
                tbSampleInfoLongitude.Text = "";
                tbSampleInfoAltitude.Text = "";
                return;
            }

            Lemma<Guid, string> station = cboxSampleInfoStations.SelectedItem as Lemma<Guid, string>;

            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_station", CommandType.StoredProcedure, 
                    new SqlParameter("@id", station.Id)))
                {
                    reader.Read();
                    tbSampleInfoLatitude.Text = reader["latitude"].ToString();
                    tbSampleInfoLongitude.Text = reader["longitude"].ToString();
                    tbSampleInfoAltitude.Text = reader["altitude"].ToString();
                }
            }                
        }

        private void tbSampleExId_TextChanged(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(tbSampleExId.Text.Trim()))
            {
                lblSampleToolExId.Text = "";
                return;
            }

            lblSampleToolExId.Text = "[Ex.Id] " + tbSampleExId.Text.Trim();
        }

        private void cboxSampleCounties_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cboxSampleCounties.SelectedItem == null)
            {
                cboxSampleMunicipalities.DataSource = null;
                return;
            }

            Lemma<Guid, string> county = cboxSampleCounties.SelectedItem as Lemma<Guid, string>;
            using (SqlConnection conn = DB.OpenConnection())            
                UI.PopulateMunicipalities(conn, county.Id, cboxSampleMunicipalities);
        }

        private void gridProjectMain_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
                return;

            Guid pmid = new Guid(e.Row.Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
                UI.PopulateProjectsSub(conn, gridProjectSub, pmid);
        }

        private void miSamplesNew_Click(object sender, EventArgs e)
        {
            // new sample
            tabs.SelectedTab = tabSample;
        }

        private void miSamplesImportExcel_Click(object sender, EventArgs e)
        {
            // Import sample from excel
        }

        private void miSamplesEdit_Click(object sender, EventArgs e)
        {
            // edit sample
        }

        private void miSamplesDelete_Click(object sender, EventArgs e)
        {
            // delete sample
        }

        private void miSamplesSplit_Click(object sender, EventArgs e)
        {
            // split sample
        }

        private void miSamplesMerge_Click(object sender, EventArgs e)
        {
            // merge sample
        }

        private void miSamplesSetOrder_Click(object sender, EventArgs e)
        {
            // sample, set order
        }

        private void miSamplesSetProject_Click(object sender, EventArgs e)
        {
            // sample, set project
        }

        private void miSamplesSetCustomer_Click(object sender, EventArgs e)
        {
            // sample, set customer
        }

        private void miSamplesSetSampler_Click(object sender, EventArgs e)
        {
            // sample, set sampler
        }

        private void miSamplesSetSamplingMethod_Click(object sender, EventArgs e)
        {
            // sample, set sampling method
        }

        private void miSamplesPrepAnal_Click(object sender, EventArgs e)
        {
            // go to sample prep/anal
            tabs.SelectedTab = tabPrepAnal;
        }

        private void btnSampleClose_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSamples;
        }

        private void btnPrepAnalClose_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSamples;
        }

        private void btnPrepAnalSave_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSamples;
        }

        private void miSamplesSetExcempt_Click(object sender, EventArgs e)
        {
            // set sample excempt from public
        }

        private void miSamplingMethodNew_Click(object sender, EventArgs e)
        {
            // new sampling method
            FormSamplingMeth form = new FormSamplingMeth();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampling method " + form.SamplingMethodName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);
                        UI.PopulateSamplingMethods(conn, cboxSampleInfoSamplingMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sampler failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSamplingMethodEdit_Click(object sender, EventArgs e)
        {
            // edit sampling method
            if (gridMetaSamplingMeth.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSamplingMeth.SelectedRows[0];
            Guid smid = new Guid(row.Cells[0].Value.ToString());

            FormSamplingMeth form = new FormSamplingMeth(smid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampling method " + form.SamplingMethodName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);
                        UI.PopulateSamplingMethods(conn, cboxSampleInfoSamplingMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update sampling method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSamplingMethodDelete_Click(object sender, EventArgs e)
        {
            // delete sampling method
        }

        private void cboxSamplesProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxSamplesProjects.SelectedItem == null)
            {
                cboxSamplesProjectsSub.Items.Clear();
                return;
            }

            Lemma<Guid, string> project = cboxSamplesProjects.SelectedItem as Lemma<Guid, string>;
            using (SqlConnection conn = DB.OpenConnection())
                UI.PopulateProjectsSub(conn, project.Id, cboxSamplesProjectsSub);
        }

        private void miPreparationMethodsNew_Click(object sender, EventArgs e)
        {
            // new preparation method
            FormPreparationMethod form = new FormPreparationMethod();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Preparation method " + form.PreparationMethodName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulatePreparationMethods(conn, gridTypeRelPrepMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create preparation method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miPreparationMethodEdit_Click(object sender, EventArgs e)
        {
            // edit preparation method
            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridTypeRelPrepMeth.SelectedRows[0];
            Guid pmid = new Guid(row.Cells[0].Value.ToString());

            FormPreparationMethod form = new FormPreparationMethod(pmid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Preparation method " + form.PreparationMethodName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulatePreparationMethods(conn, gridTypeRelPrepMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update preparation method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miPreparationMethodDelete_Click(object sender, EventArgs e)
        {
            // delete preparation method
        }

        private void miAnalysisMethodsNew_Click(object sender, EventArgs e)
        {
            // new analysis method
            FormAnalysisMethods form = new FormAnalysisMethods();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Analysis method " + form.AnalysisMethodName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateAnalysisMethods(conn, gridTypeRelAnalMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create preparation method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miAnalysisMethodsEdit_Click(object sender, EventArgs e)
        {
            // edit analysis method
            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridTypeRelAnalMeth.SelectedRows[0];
            Guid amid = new Guid(row.Cells[0].Value.ToString());

            FormAnalysisMethods form = new FormAnalysisMethods(amid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Analysis method " + form.AnalysisMethodName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateAnalysisMethods(conn, gridTypeRelAnalMeth);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update analysis method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miAnalysisMethodsDelete_Click(object sender, EventArgs e)
        {
            // delete analysis method
        }

        private void miAddPrepMethToSampType_Click(object sender, EventArgs e)
        {
            if(treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }
            
            List<Guid> methodsAbove = GetPreparationMethodsForSampleType(treeSampleTypes.SelectedNode, true);

            List<Guid> methodsBelow = new List<Guid>();
            GetPreparationMethodsBelowSampleType(treeSampleTypes.SelectedNode, methodsBelow);

            FormSampTypeXPrepMeth form = new FormSampTypeXPrepMeth(
                treeSampleTypes.SelectedNode.Text, 
                new Guid(treeSampleTypes.SelectedNode.Name), 
                methodsAbove, 
                methodsBelow);

            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSampleTypePrepMeth(conn, treeSampleTypes.SelectedNode, lbTypeRelSampTypePrepMeth, lbTypeRelSampTypeInheritedPrepMeth);
            }
        }        

        private List<Guid> GetPreparationMethodsForSampleType(TreeNode tnode, bool ascend)
        {
            List<Guid> existingMethods = new List<Guid>();
            Guid sampleTypeId = new Guid(tnode.Name);
            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(@"
select pm.id, pm.name from preparation_method pm	
    inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
    inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
order by name
", conn);
                cmd.Parameters.AddWithValue("@sample_type_id", sampleTypeId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())                    
                        existingMethods.Add(new Guid(reader["id"].ToString()));
                }

                if (ascend)
                {
                    while (tnode.Parent != null)
                    {
                        tnode = tnode.Parent;
                        sampleTypeId = new Guid(tnode.Name);

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@sample_type_id", sampleTypeId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                existingMethods.Add(new Guid(reader["id"].ToString()));
                        }
                    }
                }
            }            

            return existingMethods;
        }

        private void GetPreparationMethodsBelowSampleType(TreeNode tnode, List<Guid> methods)
        {
            foreach (TreeNode tn in tnode.Nodes)
            {
                methods.AddRange(GetPreparationMethodsForSampleType(tn, false));
                GetPreparationMethodsBelowSampleType(tn, methods);
            }
        }

        private void miTypeRelSampleTypesCompNew_Click(object sender, EventArgs e)
        {
            if (treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            // new sample component
            Guid sampleTypeId = new Guid(treeSampleTypes.SelectedNode.Name);

            FormSampleComponent form = new FormSampleComponent(sampleTypeId);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample component " + form.SampleComponentName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {                        
                        UI.PopulateSampleComponents(conn, sampleTypeId, lbSampleTypesComponents);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create preparation method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miTypeRelSampleTypesCompEdit_Click(object sender, EventArgs e)
        {
            // edit sample component
            if (treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            if (lbSampleTypesComponents.SelectedItems.Count < 1)
            {
                MessageBox.Show("You must select a sample component first");
                return;
            }

            Guid sampleTypeId = new Guid(treeSampleTypes.SelectedNode.Name);

            Lemma<Guid, string> sampleComponent = lbSampleTypesComponents.SelectedItems[0] as Lemma<Guid, string>;

            FormSampleComponent form = new FormSampleComponent(sampleTypeId, sampleComponent.Id);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample component " + form.SampleComponentName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSampleComponents(conn, sampleTypeId, lbSampleTypesComponents);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update preparation method failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miAnalysisMethodsAddNuclide_Click(object sender, EventArgs e)
        {
            // add nuclide to analysis method
            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an analysis type first");
                return;
            }

            Guid amid = new Guid(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value.ToString());
            string amname = gridTypeRelAnalMeth.SelectedRows[0].Cells["name"].Value.ToString();

            List<Guid> existingNuclides = GetNuclidesForAnalysisType(amid);

            FormAnalMethXNuclide form = new FormAnalMethXNuclide(amname, amid, existingNuclides);

            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalMethNuclides(conn, amid, lbTypRelAnalMethNuclides);
            }
        }

        public List<Guid> GetNuclidesForAnalysisType(Guid amid)
        {
            List<Guid> existingNuclides = new List<Guid>();            
            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(@"
select n.id, n.name from nuclide n
    inner join analysis_method_x_nuclide amn on amn.nuclide_id = n.id
    inner join analysis_method am on amn.analysis_method_id = am.id and am.id = @analysis_method_id
order by name
", conn);
                cmd.Parameters.AddWithValue("@analysis_method_id", amid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        existingNuclides.Add(new Guid(reader["id"].ToString()));
                }
            }

            return existingNuclides;
        }

        private void miAnalysisMethodsRemNuclide_Click(object sender, EventArgs e)
        {
            // remove nuclides from analysis method
        }

        private void gridTypeRelAnalMeth_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
                return;

            Guid amid = new Guid(e.Row.Cells["id"].Value.ToString());

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalMethNuclides(conn, amid, lbTypRelAnalMethNuclides);
            }
        }

        private void miTypeRelPrepMethAddAnalMeth_Click(object sender, EventArgs e)
        {
            // add analysis methods to preparation method
            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }

            Guid pmid = new Guid(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value.ToString());
            string pmname = gridTypeRelPrepMeth.SelectedRows[0].Cells["name"].Value.ToString();

            List<Guid> existingAnalysisMethods = GetAnalysisMethodsForPreparationMethod(pmid);

            FormPrepMethXAnalMeth form = new FormPrepMethXAnalMeth(pmname, pmid, existingAnalysisMethods);

            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulatePrepMethAnalMeths(conn, pmid, lbTypRelPrepMethAnalMeth);
            }
        }

        public List<Guid> GetAnalysisMethodsForPreparationMethod(Guid pmid)
        {
            List<Guid> existingAnalysisMethods = new List<Guid>();
            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(@"
select am.id, am.name from analysis_method am
    inner join preparation_method_x_analysis_method pmam on pmam.analysis_method_id = am.id
    inner join preparation_method pm on pmam.preparation_method_id = pm.id and pm.id = @preparation_method_id
order by name
", conn);
                cmd.Parameters.AddWithValue("@preparation_method_id", pmid);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        existingAnalysisMethods.Add(new Guid(reader["id"].ToString()));
                }
            }

            return existingAnalysisMethods;
        }

        private void miTypeRelPrepMethRemAnalMeth_Click(object sender, EventArgs e)
        {
            // remove analysis methods to preparation method
        }

        private void gridTypeRelPrepMeth_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
                return;

            Guid pmid = new Guid(e.Row.Cells["id"].Value.ToString());

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulatePrepMethAnalMeths(conn, pmid, lbTypRelPrepMethAnalMeth);
            }
        }
    }    
}
