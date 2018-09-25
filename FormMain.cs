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
using System.Configuration;
using System.Data.SqlClient;
using log4net;
using System.IO;
using System.Xml.Serialization;

namespace DSA_lims
{
    public partial class FormMain : Form
    {        
        private ILog log = null;
        private DSASettings settings = new DSASettings();        

        private ResourceManager r = null;
        private TabPage returnFromSample = null;
        private string SampleTypesRootName = "Sample types";
        private string ProjectsRootName = "Projects";

        private List<Tag<int, string>> decayTypeList = new List<Tag<int, string>>();
        private List<Tag<int, string>> preparationUnitList = new List<Tag<int, string>>();
        private List<Tag<int, string>> uniformActivityUnitList = new List<Tag<int, string>>();
        private List<Tag<int, string>> workflowStatusList = new List<Tag<int, string>>();
        private List<Tag<int, string>> locationTypeList = new List<Tag<int, string>>();

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
                log = DSALogger.CreateLogger(tbLog);

                tabs.Appearance = TabAppearance.FlatButtons;
                tabs.ItemSize = new Size(0, 1);
                tabs.SizeMode = TabSizeMode.Fixed;
                tabs.SelectedTab = tabMenu;
                lblCurrentTab.Text = tabs.SelectedTab.Text;
                lblStatus.Text = "";
                btnSamplesPrintSampleLabel.Visible = true;
                btnSamplesPrintPrepLabel.Visible = false;

                tbMenuLookup.Text = "";
                ActiveControl = tbMenuLookup;

                r = new ResourceManager("DSA_lims.lang_" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                log.Info("Setting language " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                SetLanguageLabels(r);

                log.Info("Loading settings file " + DSAEnvironment.SettingsFilename);
                LoadSettings(DSAEnvironment.SettingsFilename);
                cbMachineSettingsUseAD.Checked = settings.UseActiveDirectoryCredentials;

                using (SqlConnection conn = DB.OpenConnection())
                {
                    Common.Username = "Admin"; // FIXME

                    log.Info("Loading decay types");
                    LoadDecayTypes(conn);

                    log.Info("Loading preparation units");
                    LoadPreparationUnits(conn);

                    log.Info("Loading uniform activity units");
                    LoadUniformActivityUnits(conn);

                    log.Info("Loading workflow status");
                    LoadWorkflowStatus(conn);

                    log.Info("Loading location types");
                    LoadLocationTypes(conn);

                    log.Info("Populating preparation units");
                    PopulatePreparationUnits(conn);

                    log.Info("Populating activity units");
                    PopulateActivityUnits(conn);

                    log.Info("Populating uniform activity units");
                    PopulateUniformActivityUnits(conn);

                    log.Info("Populating workflow status");
                    PopulateWorkflowStatus(conn);

                    log.Info("Populating location types");
                    PopulateLocationTypes(conn);

                    log.Info("Populating laboratories");
                    PopulateLaboratories(conn);

                    log.Info("Populating users");
                    PopulateUsers(conn);

                    log.Info("Populating sample types");
                    PopulateSampleTypes(conn);

                    log.Info("Populating projects");
                    PopulateProjects(conn);

                    log.Info("Populating nuclides");
                    PopulateNuclides(conn);

                    log.Info("Populating geometries");
                    PopulateGeometries(conn);

                    log.Info("Populating counties");
                    PopulateCounties(conn);

                    log.Info("Populating stations");
                    PopulateStations(conn);

                    log.Info("Populating sample storage");
                    PopulateSampleStorage(conn);

                    log.Info("Populating samplers");
                    PopulateSamplers(conn);
                }
                
                HideMenuItems();

                log.Info("Application loaded successfully");
            }
            catch(Exception ex)
            {
                log.Fatal(ex.Message, ex);
                Environment.Exit(1);
            }
        }        

        private void FormMain_Shown(object sender, EventArgs e)
        {
            //ShowLogin();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Info("Application closing down");            
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {            
            log.Info("Application closed successfully");
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            //
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
            FormLogin formLogin = new FormLogin(settings);
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
                    XmlSerializer x = new XmlSerializer(settings.GetType());
                    x.Serialize(sw, settings);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
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
                    XmlSerializer x = new XmlSerializer(settings.GetType());
                    settings = x.Deserialize(sr) as DSASettings;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }
        }

        private void LoadDecayTypes(SqlConnection conn)
        {
            try
            {
                decayTypeList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_decay_types", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        decayTypeList.Add(new Tag<int, string>(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void LoadPreparationUnits(SqlConnection conn)
        {
            try
            {
                preparationUnitList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_preparation_units", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        preparationUnitList.Add(new Tag<int, string>(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void LoadUniformActivityUnits(SqlConnection conn)
        {
            try
            {
                uniformActivityUnitList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_uniform_activity_units", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        uniformActivityUnitList.Add(new Tag<int, string>(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void LoadWorkflowStatus(SqlConnection conn)
        {
            try
            {
                workflowStatusList.Clear();                       

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_workflow_status", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        workflowStatusList.Add(new Tag<int, string>(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void LoadLocationTypes(SqlConnection conn)
        {
            try
            {
                locationTypeList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_location_types", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        locationTypeList.Add(new Tag<int, string>(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void PopulatePreparationUnits(SqlConnection conn)
        {
            cboxSamplePrepUnit.DataSource = preparationUnitList;
            cboxSamplePrepUnit.SelectedIndex = -1;
        }

        private void PopulateUniformActivityUnits(SqlConnection conn)
        {
            // populate uniform activity units
        }

        private void PopulateWorkflowStatus(SqlConnection conn)
        {
            cboxSampleAnalWorkflowStatus.DataSource = workflowStatusList;
            cboxSampleAnalWorkflowStatus.SelectedIndex = -1;
        }

        private void PopulateLocationTypes(SqlConnection conn)
        {
            cboxSampleInfoLocationTypes.DataSource = locationTypeList;
            cboxSampleInfoLocationTypes.SelectedIndex = -1;
        }

        private void PopulateActivityUnits(SqlConnection conn)
        {
            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_activity_units_flat", CommandType.StoredProcedure);

            gridMetaUnitsActivity.DataSource = dt;

            gridMetaUnitsActivity.Columns["id"].Visible = false;                        

            gridMetaUnitsActivity.Columns["name"].HeaderText = "Unit name";
            gridMetaUnitsActivity.Columns["convert_factor"].HeaderText = "Conv. fact.";
            gridMetaUnitsActivity.Columns["uniform_activity_unit_name"].HeaderText = "Uniform unit";

            cboxSampleAnalUnit.Items.Clear();            
            foreach(DataRow row in dt.Rows)
            {
                Tag<Guid, string> au = new Tag<Guid, string>(new Guid(row["id"].ToString()), row["name"].ToString());
                cboxSampleAnalUnit.Items.Add(au);
            }            
        }

        private void PopulateSampleTypes(SqlConnection conn)
        {
            try
            {
                treeSampleTypes.Nodes.Clear();
                cboxSampleSampleType.Items.Clear();

                TreeNode root = treeSampleTypes.Nodes.Add(SampleTypesRootName, SampleTypesRootName);
                
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_types_short", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        string sampleType = reader["name"].ToString();
                        Guid id = new Guid(reader["id"].ToString());

                        cboxSampleSampleType.Items.Add(new Tag<Guid, string>(id, sampleType));

                        string[] items = sampleType.Substring(1).Split(new char[] { '/' });
                        TreeNode current = root;
                        foreach (string item in items)
                        {
                            if (current.Nodes.ContainsKey(item))
                            {
                                current = current.Nodes[item];
                                continue;
                            }
                            else
                            {
                                current = current.Nodes.Add(item, item);
                                current.ToolTipText = sampleType;
                                current.Tag = reader["id"].ToString();
                            }
                        }
                    }
                }

                root.Expand();

                cboxSampleSampleType.SelectedIndex = -1;
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private void PopulateProjects(SqlConnection conn)
        {
            try
            {
                treeProjects.Nodes.Clear();
                TreeNode root = treeProjects.Nodes.Add(ProjectsRootName, ProjectsRootName);

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_main_projects_short", CommandType.StoredProcedure, 
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
                {
                    while (reader.Read())
                    {
                        Guid id = new Guid(reader["id"].ToString());
                        string name = reader["name"].ToString();

                        TreeNode node = root.Nodes.Add(name, name);
                        node.Tag = id;
                    }
                }

                foreach(TreeNode node in root.Nodes)
                {
                    Guid parent_id = new Guid(node.Tag.ToString());
                    using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sub_projects_short", CommandType.StoredProcedure, 
                        new[] {
                            new SqlParameter("@parent_id", parent_id),
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }))
                    {
                        while (reader.Read())
                        {
                            Guid id = new Guid(reader["id"].ToString());
                            string name = reader["name"].ToString();

                            TreeNode n = node.Nodes.Add(name, name);
                            n.Tag = id;
                        }
                    }
                }

                root.Expand();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void PopulateLaboratories(SqlConnection conn)
        {
            // Set data source
            gridMetaLab.DataSource = DB.GetDataTable(conn, "csp_select_laboratories_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            gridMetaLab.Columns["id"].Visible = false;
            gridMetaLab.Columns["assignment_counter"].Visible = false;
            gridMetaLab.Columns["comment"].Visible = false;
            gridMetaLab.Columns["create_date"].Visible = false;
            gridMetaLab.Columns["created_by"].Visible = false;
            gridMetaLab.Columns["update_date"].Visible = false;
            gridMetaLab.Columns["updated_by"].Visible = false;

            gridMetaLab.Columns["name"].HeaderText = "Name";
            gridMetaLab.Columns["name_prefix"].HeaderText = "Prefix";
            gridMetaLab.Columns["address"].HeaderText = "Address";
            gridMetaLab.Columns["email"].HeaderText = "Email";
            gridMetaLab.Columns["phone"].HeaderText = "Phone";
            gridMetaLab.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateUsers(SqlConnection conn)
        {
            // Set data source
            gridMetaUsers.DataSource = DB.GetDataTable(conn, "csp_select_accounts_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            gridMetaUsers.Columns["password_hash"].Visible = false;
            gridMetaUsers.Columns["create_date"].Visible = false;
            gridMetaUsers.Columns["update_date"].Visible = false;

            // Set UI state
            gridMetaUsers.Columns["username"].HeaderText = "Username";
            gridMetaUsers.Columns["fullname"].HeaderText = "Name";
            gridMetaUsers.Columns["laboratory_name"].HeaderText = "Laboratory";
            gridMetaUsers.Columns["language_code"].HeaderText = "Language";
            gridMetaUsers.Columns["instance_status_name"].HeaderText = "Status";            
        }

        private void PopulateNuclides(SqlConnection conn)
        {
            // Set data source
            gridSysNuclides.DataSource = DB.GetDataTable(conn, "csp_select_nuclides_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            gridSysNuclides.Columns["id"].Visible = false;
            gridSysNuclides.Columns["comment"].Visible = false;
            gridSysNuclides.Columns["created_by"].Visible = false;
            gridSysNuclides.Columns["create_date"].Visible = false;
            gridSysNuclides.Columns["updated_by"].Visible = false;
            gridSysNuclides.Columns["update_date"].Visible = false;

            gridSysNuclides.Columns["name"].HeaderText = "Name";
            gridSysNuclides.Columns["proton_count"].HeaderText = "Protons";
            gridSysNuclides.Columns["neutron_count"].HeaderText = "Neutrons";
            gridSysNuclides.Columns["half_life_year"].HeaderText = "T 1/2 (Years)";
            gridSysNuclides.Columns["half_life_uncertainty"].HeaderText = "T 1/2 Unc. (Years)";
            gridSysNuclides.Columns["decay_type_name"].HeaderText = "Decay type";
            gridSysNuclides.Columns["kxray_energy"].HeaderText = "KXray Energy";
            gridSysNuclides.Columns["fluorescence_yield"].HeaderText = "Fluorescence Yield";
            gridSysNuclides.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateEnergyLines(SqlConnection conn, Guid nid)
        {
            // Set data source
            gridSysNuclideTrans.DataSource = DB.GetDataTable(conn, "csp_select_nuclide_transmissions_for_nuclide_flat", CommandType.StoredProcedure, 
                new [] {
                    new SqlParameter("@nuclide_id", nid),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                });

            // Set UI state
            gridSysNuclideTrans.Columns["id"].Visible = false;
            gridSysNuclideTrans.Columns["nuclide_name"].Visible = false;
            gridSysNuclideTrans.Columns["comment"].Visible = false;
            gridSysNuclideTrans.Columns["created_by"].Visible = false;
            gridSysNuclideTrans.Columns["create_date"].Visible = false;
            gridSysNuclideTrans.Columns["updated_by"].Visible = false;
            gridSysNuclideTrans.Columns["update_date"].Visible = false;
            
            gridSysNuclideTrans.Columns["transmission_from"].HeaderText = "Tr. from";
            gridSysNuclideTrans.Columns["transmission_to"].HeaderText = "Tr. to";
            gridSysNuclideTrans.Columns["energy"].HeaderText = "Energy";
            gridSysNuclideTrans.Columns["energy_uncertainty"].HeaderText = "Energy Unc.";
            gridSysNuclideTrans.Columns["intensity"].HeaderText = "Intensity";
            gridSysNuclideTrans.Columns["intensity_uncertainty"].HeaderText = "Intensity Unc.";
            gridSysNuclideTrans.Columns["probability_of_decay"].HeaderText = "POD";
            gridSysNuclideTrans.Columns["probability_of_decay_uncertainty"].HeaderText = "POD Unc.";
            gridSysNuclideTrans.Columns["total_internal_conversion"].HeaderText = "TIC conv.";
            gridSysNuclideTrans.Columns["kshell_conversion"].HeaderText = "KShell conv.";
            gridSysNuclideTrans.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateGeometries(SqlConnection conn)
        {
            // Set data source
            gridSysGeom.DataSource = DB.GetDataTable(conn, "csp_select_preparation_geometries_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            gridSysGeom.Columns["id"].Visible = false;
            gridSysGeom.Columns["comment"].Visible = false;
            gridSysGeom.Columns["created_by"].Visible = false;
            gridSysGeom.Columns["create_date"].Visible = false;
            gridSysGeom.Columns["updated_by"].Visible = false;
            gridSysGeom.Columns["update_date"].Visible = false;

            gridSysGeom.Columns["name"].HeaderText = "Name";
            gridSysGeom.Columns["instance_status_name"].HeaderText = "Status";
            // FIXME
        }

        private void PopulateCounties(SqlConnection conn)
        {
            // Set data source
            gridSysCounty.DataSource = DB.GetDataTable(conn, "csp_select_counties_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            gridSysCounty.Columns["id"].Visible = false;
            gridSysCounty.Columns["created_by"].Visible = false;
            gridSysCounty.Columns["create_date"].Visible = false;
            gridSysCounty.Columns["updated_by"].Visible = false;
            gridSysCounty.Columns["update_date"].Visible = false;

            gridSysCounty.Columns["name"].HeaderText = "Name";
            gridSysCounty.Columns["county_number"].HeaderText = "Number";
            gridSysCounty.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateMunicipalities(SqlConnection conn, Guid cid)
        {
            // Set data source
            gridSysMunicipality.DataSource = DB.GetDataTable(conn, "csp_select_municipalities_for_county_flat", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@county_id", cid),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                });

            // Set UI state
            gridSysMunicipality.Columns["id"].Visible = false;
            gridSysMunicipality.Columns["county_name"].Visible = false;
            gridSysMunicipality.Columns["created_by"].Visible = false;
            gridSysMunicipality.Columns["create_date"].Visible = false;
            gridSysMunicipality.Columns["updated_by"].Visible = false;
            gridSysMunicipality.Columns["update_date"].Visible = false;

            gridSysMunicipality.Columns["name"].HeaderText = "Name";            
            gridSysMunicipality.Columns["municipality_number"].HeaderText = "Number";
            gridSysMunicipality.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateStations(SqlConnection conn)
        {
            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_stations_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
            gridMetaStation.DataSource = dt;

            // Set UI state
            gridMetaStation.Columns["id"].Visible = false;
            gridMetaStation.Columns["comment"].Visible = false;
            gridMetaStation.Columns["created_by"].Visible = false;
            gridMetaStation.Columns["create_date"].Visible = false;
            gridMetaStation.Columns["updated_by"].Visible = false;
            gridMetaStation.Columns["update_date"].Visible = false;

            gridMetaStation.Columns["name"].HeaderText = "Name";
            gridMetaStation.Columns["latitude"].HeaderText = "Latitude";
            gridMetaStation.Columns["longitude"].HeaderText = "Longitude";
            gridMetaStation.Columns["altitude"].HeaderText = "Altitude";
            gridMetaStation.Columns["instance_status_name"].HeaderText = "Status";            

            cboxSampleInfoStations.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                Tag<Guid, string> s = new Tag<Guid, string>(new Guid(row["id"].ToString()), row["name"].ToString());
                cboxSampleInfoStations.Items.Add(s);
            }
            cboxSampleInfoStations.SelectedIndex = -1;
        }

        private void PopulateSampleStorage(SqlConnection conn)
        {
            // Set data source
            gridMetaSampleStorage.DataSource = DB.GetDataTable(conn, "csp_select_sample_storages_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            gridMetaSampleStorage.Columns["id"].Visible = false;
            gridMetaSampleStorage.Columns["comment"].Visible = false;
            gridMetaSampleStorage.Columns["created_by"].Visible = false;
            gridMetaSampleStorage.Columns["create_date"].Visible = false;
            gridMetaSampleStorage.Columns["updated_by"].Visible = false;
            gridMetaSampleStorage.Columns["update_date"].Visible = false;

            gridMetaSampleStorage.Columns["name"].HeaderText = "Name";
            gridMetaSampleStorage.Columns["address"].HeaderText = "Address";
            gridMetaSampleStorage.Columns["instance_status_name"].HeaderText = "Status";
        }

        private void PopulateSamplers(SqlConnection conn)
        {
            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_samplers_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
            gridMetaSamplers.DataSource = dt;

            // Set UI state
            gridMetaSamplers.Columns["id"].Visible = false;
            gridMetaSamplers.Columns["comment"].Visible = false;
            gridMetaSamplers.Columns["created_by"].Visible = false;
            gridMetaSamplers.Columns["create_date"].Visible = false;
            gridMetaSamplers.Columns["updated_by"].Visible = false;
            gridMetaSamplers.Columns["update_date"].Visible = false;

            gridMetaSamplers.Columns["name"].HeaderText = "Name";
            gridMetaSamplers.Columns["address"].HeaderText = "Address";
            gridMetaSamplers.Columns["email"].HeaderText = "Email";
            gridMetaSamplers.Columns["phone"].HeaderText = "Phone";
            gridMetaSamplers.Columns["instance_status_name"].HeaderText = "Status";

            cboxSampleInfoSampler.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                Tag<Guid, string> s = new Tag<Guid, string>(new Guid(row["id"].ToString()), row["name"].ToString());
                cboxSampleInfoSampler.Items.Add(s);
            }
            cboxSampleInfoSampler.SelectedIndex = -1;
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
                tbSampleListLookup.Text = "";
                ActiveControl = tbSampleListLookup;
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
            btnSamplesPrintSampleLabel.Visible = true;
            btnSamplesPrintPrepLabel.Visible = false;

            if (tabsSample.SelectedTab == tabSamplesPrep)
            {
                btnSamplesPrintPrepLabel.Visible = true;
            }
        }

        private void btnMenuNewSample_Click(object sender, EventArgs e)
        {            
            tabsSample.TabPages.Clear();
            tabsSample.TabPages.Add(tabSamplesInfo);
            returnFromSample = tabMenu;
            tabs.SelectedTab = tabSample;
        }

        private void btnMenuNewOrder_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrder;
        }

        private void treeSampleTypes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
                return;

            try
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    Guid id = new Guid(e.Node.Tag.ToString());
                    lbSampleTypesComponents.Items.Clear();

                    SqlCommand cmd = new SqlCommand("select id, name from sample_component where sample_type_id = @ID order by name", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            lbSampleTypesComponents.Items.Add(new Tag<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
                    }

                    lbSampleTypesInheritedComponents.Items.Clear();
                    TreeNode node = e.Node;
                    while (node.Parent != null && node.Parent.Level != 0)
                    {
                        node = node.Parent;
                        if (node.Tag == null)
                            throw new Exception("Missing ID tag in treeSampleTypes_AfterSelect");

                        id = new Guid(node.Tag.ToString());
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@ID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                lbSampleTypesInheritedComponents.Items.Add(new Tag<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
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
            settings.UseActiveDirectoryCredentials = cbMachineSettingsUseAD.Checked;

            SaveSettings(DSAEnvironment.SettingsFilename);
            lblStatus.Text = StrUtils.makeStatusMessage("Settings saved");
        }

        private void btnUserSettingsSave_Click(object sender, EventArgs e)
        {
            //
        }

        private void btnSampleClose_Click(object sender, EventArgs e)
        {
            if (returnFromSample != null)
                tabs.SelectedTab = returnFromSample;
            else tabs.SelectedTab = tabMenu;
        }

        private void btnSampleSave_Click(object sender, EventArgs e)
        {
            if (!tabsSample.TabPages.ContainsKey(tabSamplesParams.Name))
                tabsSample.TabPages.Add(tabSamplesParams);
            if (!tabsSample.TabPages.ContainsKey(tabSamplesPrep.Name))
                tabsSample.TabPages.Add(tabSamplesPrep);
            if (!tabsSample.TabPages.ContainsKey(tabSamplesAnalysis.Name))
                tabsSample.TabPages.Add(tabSamplesAnalysis);
        }

        private void btnSamplesOpen_Click(object sender, EventArgs e)
        {
            tabsSample.TabPages.Clear();
            tabsSample.TabPages.Add(tabSamplesInfo);
            tabsSample.TabPages.Add(tabSamplesParams);
            tabsSample.TabPages.Add(tabSamplesPrep);
            tabsSample.TabPages.Add(tabSamplesAnalysis);
            returnFromSample = tabSamples;
            tabs.SelectedTab = tabSample;
        }

        private void btnOrdersOpen_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabOrder;
        }

        private void miNewLaboratory_Click(object sender, EventArgs e)
        {
            // create laboratory
            FormLaboratory form = new FormLaboratory(log);
            switch(form.ShowDialog())
            {                
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Laboratory " + form.Laboratory.Name + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateLaboratories(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create laboratory failed");
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

            FormLaboratory form = new FormLaboratory(log, lid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Laboratory " + form.Laboratory.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateLaboratories(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update laboratory failed");
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
            FormProject form = new FormProject(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Main project " + form.MainProject.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateProjects(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create main project failed");
                    break;
            }                        
        }        

        private void miProjectsEdit_Click(object sender, EventArgs e)
        {
            // edit project
            if (treeProjects.SelectedNode == null)
                return;

            Guid pid = new Guid(treeProjects.SelectedNode.Tag.ToString());

            FormProject form = new FormProject(log, pid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Project " + form.MainProject.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateProjects(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create project failed");
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
            if(treeProjects.SelectedNode == null)
            {
                MessageBox.Show("No main project selected");
                return;
            }

            if(treeProjects.SelectedNode.Level != 1)
            {
                MessageBox.Show("Selected project is not a main project");
                return;
            }

            Guid parent_id = new Guid(treeProjects.SelectedNode.Tag.ToString());

            FormProjectSub form = new FormProjectSub(log, treeProjects.SelectedNode.Text, parent_id);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sub project " + form.SubProject.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateProjects(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create sub project failed");
                    break;
            }
        }

        private void miProjectsSubEdit_Click(object sender, EventArgs e)
        {
            // edit sub project
            if (treeProjects.SelectedNode == null)
            {
                MessageBox.Show("No main project selected");
                return;
            }

            if (treeProjects.SelectedNode.Level != 2)
            {
                MessageBox.Show("Selected project is not a sub project");
                return;
            }

            Guid parent_id = new Guid(treeProjects.SelectedNode.Parent.Tag.ToString());

            Guid pid = new Guid(treeProjects.SelectedNode.Tag.ToString());

            FormProjectSub form = new FormProjectSub(log, treeProjects.SelectedNode.Parent.Text, parent_id, pid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sub project " + form.SubProject.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateProjects(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create sub project failed");
                    break;
            }
        }

        private void miProjectsSubDelete_Click(object sender, EventArgs e)
        {
            // delete sub project
        }

        private void treeProjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tbProjectsName.Text = "";                        
            tbProjectsComment.Text = "";
            cbProjectsInUse.Checked = false;

            if (e.Node.Tag == null)
                return;

            try
            {
                SqlConnection conn = DB.OpenConnection();
                Guid id = new Guid(e.Node.Tag.ToString());

                SqlCommand cmd = new SqlCommand("csp_select_project", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return;

                    reader.Read();

                    tbProjectsName.Text = reader["name"].ToString();
                    tbProjectsComment.Text = reader["comment"].ToString();                    
                    cbProjectsInUse.Checked = InstanceStatus.IsActive(reader["instance_status_id"]);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void miNuclidesNew_Click(object sender, EventArgs e)
        {            
            FormNuclide form = new FormNuclide(log, decayTypeList);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Nuclide " + form.Nuclide.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateNuclides(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create nuclide failed");
                    break;
            }            
        }        

        private void miSampleTypesNew_Click(object sender, EventArgs e)
        {
            // New sample type
        }

        private void miSampleTypesEdit_Click(object sender, EventArgs e)
        {
            // Edit sample type

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

            FormNuclide form = new FormNuclide(log, decayTypeList, nid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Nuclide " + form.Nuclide.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateNuclides(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update nuclide failed");
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

            FormEnergyLine form = new FormEnergyLine(log, nid, nname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Energy line for " + nname + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateEnergyLines(conn, nid);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create energy line failed");
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

            FormEnergyLine form = new FormEnergyLine(log, nid, eid, nname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Energy line for " + nname + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateEnergyLines(conn, nid);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update energy line failed");
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
                PopulateEnergyLines(conn, nid);
        }

        private void miNewGeometry_Click(object sender, EventArgs e)
        {
            // new geom
            FormGeometry form = new FormGeometry(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Geometry " + form.Geometry.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateGeometries(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create geometry failed");
                    break;
            }                        
        }

        private void miEditGeometry_Click(object sender, EventArgs e)
        {
            // edit geom
            if (gridSysGeom.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysGeom.SelectedRows[0];
            Guid gid = new Guid(row.Cells[0].Value.ToString());

            FormGeometry form = new FormGeometry(log, gid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Geometry " + form.Geometry.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateGeometries(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update geometry failed");
                    break;
            }                        
        }

        private void miDeleteGeometry_Click(object sender, EventArgs e)
        {
            // delete geom
        }

        private void miNewCounty_Click(object sender, EventArgs e)
        {
            // new county
            FormCounty form = new FormCounty(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("County " + form.County.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateCounties(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create county failed");
                    break;
            }                        
        }

        private void miEditCounty_Click(object sender, EventArgs e)
        {
            // edit county
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());

            FormCounty form = new FormCounty(log, cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("County " + form.County.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateCounties(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create county failed");
                    break;
            }                        
        }

        private void miDeleteCounty_Click(object sender, EventArgs e)
        {
            // delete county
        }

        private void miNewMunicipality_Click(object sender, EventArgs e)
        {
            // new municipality            
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());

            FormMunicipality form = new FormMunicipality(log, cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Municipality " + form.Municipality.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateMunicipalities(conn, cid);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create municipality failed");
                    break;
            }                        
        }

        private void miEditMunicipality_Click(object sender, EventArgs e)
        {
            // edit municipality
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            if (gridSysMunicipality.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysCounty.SelectedRows[0];
            Guid cid = new Guid(row.Cells[0].Value.ToString());            

            row = gridSysMunicipality.SelectedRows[0];
            Guid mid = new Guid(row.Cells[0].Value.ToString());            

            FormMunicipality form = new FormMunicipality(log, cid, mid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Municipality " + form.Municipality.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateMunicipalities(conn, cid);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update municipality failed");
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

            Guid cid = new Guid(e.Row.Cells[0].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())            
                PopulateMunicipalities(conn, cid);
        }

        private void miNewStation_Click(object sender, EventArgs e)
        {
            // create station
            FormStation form = new FormStation(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Station " + form.Station.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateStations(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create station failed");
                    break;
            }                        
        }

        private void miEditStation_Click(object sender, EventArgs e)
        {
            // edit station
            if (gridMetaStation.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaStation.SelectedRows[0];
            Guid sid = new Guid(row.Cells[0].Value.ToString());

            FormStation form = new FormStation(log, sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Station " + form.Station.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateStations(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update station failed");
                    break;
            }                        
        }

        private void miDeleteStation_Click(object sender, EventArgs e)
        {
            // delete station
        }

        private void miNewSampleStorage_Click(object sender, EventArgs e)
        {
            // new sample storage
            FormSampleStorage form = new FormSampleStorage(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sample storage " + form.SampleStorage.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateSampleStorage(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create sample storage failed");
                    break;
            }                        
        }

        private void miEditSampleStorage_Click(object sender, EventArgs e)
        {
            // edit sample storage
            if (gridMetaSampleStorage.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSampleStorage.SelectedRows[0];
            Guid ssid = new Guid(row.Cells[0].Value.ToString());

            FormSampleStorage form = new FormSampleStorage(log, ssid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sample storage " + form.SampleStorage.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateSampleStorage(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update sample storage failed");
                    break;
            }                        
        }

        private void miDeleteSampleStorage_Click(object sender, EventArgs e)
        {
            // delete sample storage
        }

        private void miSamplerNew_Click(object sender, EventArgs e)
        {
            // new sampler
            FormSampler form = new FormSampler(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sampler " + form.Sampler.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateSamplers(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Create sampler failed");
                    break;
            }
        }

        private void miSamplerEdit_Click(object sender, EventArgs e)
        {
            // edit sampler
            if (gridMetaSamplers.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSamplers.SelectedRows[0];
            Guid sid = new Guid(row.Cells[0].Value.ToString());

            FormSampler form = new FormSampler(log, sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    lblStatus.Text = StrUtils.makeStatusMessage("Sampler " + form.Sampler.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        PopulateSamplers(conn);
                    break;
                case DialogResult.Abort:
                    lblStatus.Text = StrUtils.makeErrorMessage("Update sampler failed");
                    break;
            }
        }

        private void miSamplerDelete_Click(object sender, EventArgs e)
        {
            // delete sampler
        }

        private void cboxSampleSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sampleType = cboxSampleSampleType.SelectedItem as Tag<Guid, string>;            
            using (SqlConnection conn = DB.OpenConnection())
            {
                // FIXME: add inherited components
                cboxSampleSampleComponent.DataSource = DB.GetDataTable(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure, new SqlParameter("@sample_type_id", sampleType.Id));
                cboxSampleSampleComponent.SelectedIndex = -1;
            }
        }
    }    
}
