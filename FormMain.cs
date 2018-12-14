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
using System.Diagnostics;

namespace DSA_lims
{
    public partial class FormMain : Form
    {
        private ResourceManager r = null;

        private Guid selectedOrderId = Guid.Empty;
        private Guid selectedSampleId = Guid.Empty;
        private Guid selectedAnalysisMethodId = Guid.Empty;

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
                if (!Directory.Exists(DSAEnvironment.SettingsPath))
                    Directory.CreateDirectory(DSAEnvironment.SettingsPath);

                Common.Log = DSALogger.CreateLogger(DSAEnvironment.SettingsPath + Path.DirectorySeparatorChar + "dsa-lims.log");

                Common.Log.Info("Loading settings file " + DSAEnvironment.SettingsFilename);
                LoadSettings(DSAEnvironment.SettingsFilename);

                DB.ConnectionString = Common.Settings.ConnectionString;

                ShowLogin();

                DB.ConnectionString = Common.Settings.ConnectionString;

                tabs.Appearance = TabAppearance.FlatButtons;
                tabs.ItemSize = new Size(0, 1);
                tabs.SizeMode = TabSizeMode.Fixed;
                tabs.SelectedTab = tabMenu;                

                tabsPrepAnal.Appearance = TabAppearance.FlatButtons;
                tabsPrepAnal.ItemSize = new Size(0, 1);
                tabsPrepAnal.SizeMode = TabSizeMode.Fixed;
                tabsPrepAnal.SelectedTab = tabMenu;

                lblCurrentTab.Text = tabs.SelectedTab.Text;
                lblStatus.Text = "";                
                lblSampleToolId.Text = "";
                lblSampleToolExId.Text = "";
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                lblSampleToolLaboratory.Text = "";

                cbMachineSettingsUseAD.Checked = Common.Settings.UseActiveDirectoryCredentials;

                tbMenuLookup.KeyPress += CustomEvents.Integer_KeyPress;
                tbSamplesLookup.KeyPress += CustomEvents.Integer_KeyPress;
                tbSampleInfoLatitude.KeyPress += CustomEvents.Numeric_KeyPress;
                tbSampleInfoLongitude.KeyPress += CustomEvents.Numeric_KeyPress;
                tbSampleInfoAltitude.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalWetWeight.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalDryWeight.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalVolume.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalLODStartWeight.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalLODEndWeight.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalLODWater.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalPrepFillHeight.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalPrepAmount.KeyPress += CustomEvents.Numeric_KeyPress;
                tbPrepAnalPrepQuantity.KeyPress += CustomEvents.Numeric_KeyPress;

                tbMenuLookup.Text = "";
                ActiveControl = tbMenuLookup;

                panelSampleLatLonAlt_Resize(sender, e);

                r = new ResourceManager("DSA_lims.lang_" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                Common.Log.Info("Setting language " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                SetLanguageLabels(r);

                using (SqlConnection conn = DB.OpenConnection())
                {
                    DB.LoadSampleTypes(conn);
                    
                    cboxSamplesStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                    cboxSampleInstanceStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                    cboxPrepAnalPrepAmountUnit.DataSource = DB.GetIntLemmata(conn, "csp_select_preparation_units", true);

                    cboxPrepAnalPrepQuantityUnit.DataSource = DB.GetIntLemmata(conn, "csp_select_quantity_units", true);

                    cboxPrepAnalAnalWorkflowStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_workflow_status");

                    cboxPrepAnalPrepWorkflowStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_workflow_status");

                    cboxSampleInfoLocationTypes.DataSource = DB.GetIntLemmata(conn, "csp_select_location_types", true);

                    cboxPrepAnalAnalSigmaAct.DataSource = DB.GetSigmaValues();
                    cboxPrepAnalAnalSigmaMDA.DataSource = DB.GetSigmaMDAValues();

                    cboxOrderRequestedSigma.DataSource = DB.GetSigmaValues();
                    cboxOrderRequestedSigmaMDA.DataSource = DB.GetSigmaMDAValues();

                    UI.PopulatePersons(conn, gridSysPers);

                    UI.PopulateCompanies(conn, gridMetaCompanies);

                    UI.PopulatePreparationUnits(conn, gridMetaUnitPrepUnits);

                    UI.PopulateActivityUnits(conn, gridMetaUnitActivityUnits);

                    UI.PopulateQuantityUnits(conn, gridMetaUnitQuantUnits);

                    UI.PopulateActivityUnitTypes(conn, gridMetaUnitActivityUnitTypes);

                    UI.PopulateComboBoxes(conn, "csp_select_activity_units_short", new SqlParameter[] { }, cboxPrepAnalAnalUnit);
                    
                    UI.PopulateComboBoxes(conn, "csp_select_activity_unit_types", new SqlParameter[] { }, cboxPrepAnalAnalUnitType);

                    UI.PopulateProjectsMain(conn, gridProjectMain);

                    UI.PopulateComboBoxes(conn, "csp_select_projects_main_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleProject, cboxSamplesProjects);

                    UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridMetaLab);

                    UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Active)
                    }, cboxSampleLaboratory, cboxOrderLaboratory);

                    UI.PopulateUsers(conn, InstanceStatus.Deleted, gridMetaUsers);                    

                    UI.PopulateNuclides(conn, gridSysNuclides);

                    UI.PopulateGeometries(conn, gridSysGeom);                    

                    UI.PopulateComboBoxes(conn, "csp_select_preparation_geometries_short", new[] {                    
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxPrepAnalPrepGeom);

                    UI.PopulateCounties(conn, gridSysCounty);                    

                    UI.PopulateComboBoxes(conn, "csp_select_counties_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleCounties);

                    UI.PopulateStations(conn, gridMetaStation);                    

                    UI.PopulateComboBoxes(conn, "csp_select_stations_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleInfoStations);

                    UI.PopulateSampleStorage(conn, gridMetaSampleStorage);                    

                    UI.PopulateComboBoxes(conn, "csp_select_sample_storages_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleSampleStorage);

                    UI.PopulateSamplers(conn, gridMetaSamplers);                    

                    UI.PopulateComboBoxes(conn, "csp_select_samplers_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleInfoSampler);

                    UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);                    

                    UI.PopulateComboBoxes(conn, "csp_select_sampling_methods_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleInfoSamplingMeth);

                    UI.PopulatePreparationMethods(conn, gridTypeRelPrepMeth);

                    UI.PopulateAnalysisMethods(conn, gridTypeRelAnalMeth);

                    UI.PopulateSampleTypes(conn, treeSampleTypes);

                    UI.PopulateSampleTypes(treeSampleTypes, cboxSampleSampleType);                    

                    UI.PopulateCustomers(conn, InstanceStatus.Deleted, gridCustomers);
                }
                
                HideMenuItems();

                Common.Log.Info("Application loaded successfully");
            }
            catch(Exception ex)
            {
                Common.Log.Fatal(ex);
                MessageBox.Show(ex.Message);
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

            using (SqlConnection conn = DB.OpenConnection())
            {
                DB.UnlockSamples(conn);
                DB.UnlockOrders(conn);
            }

            SaveSettings(DSAEnvironment.SettingsFilename);
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
                    lblStatus.Text = Utils.makeStatusMessage(msg);
                    lblStatus.ForeColor = SystemColors.ControlText;
                    break;
                case StatusMessageType.Warning:
                    lblStatus.Text = Utils.makeStatusMessage(msg);
                    lblStatus.ForeColor = Color.OrangeRed;
                    break;
                case StatusMessageType.Error:
                    lblStatus.Text = Utils.makeErrorMessage(msg);
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
                = miAuditLog.Visible = miSys.Visible = false;
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
            Common.UserId = Guid.Empty;
            Common.Username = String.Empty;                        
            Common.LabId = Guid.Empty;

            FormLogin formLogin = new FormLogin(Common.Settings);
            if (formLogin.ShowDialog() != DialogResult.OK)            
                Close();                            

            Common.UserId = formLogin.UserId;
            Common.Username = formLogin.UserName;
            Common.LabId = formLogin.LabId;
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

        private void treeSampleTypes_AfterSelect(object sender, TreeViewEventArgs e)
        {            
            TreeNode tnode = e.Node;

            lblTypeRelSampCompSel.Text = lblTypeRelSampParSel.Text = lblTypeRelSampPrepSel.Text = tnode.Text;

            try
            {
                lbSampleTypesComponents.Items.Clear();
                lbSampleTypesInheritedComponents.Items.Clear();

                Guid sampleTypeId = Guid.Parse(tnode.Name);

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
                    lb.Items.Add(new Lemma<Guid, string>(Guid.Parse(reader["id"].ToString()), reader["name"].ToString()));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = Guid.Parse(tnode.Parent.Name);
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

        private void miNewLaboratory_Click(object sender, EventArgs e)
        {            
            FormLaboratory form = new FormLaboratory();
            switch(form.ShowDialog())
            {                
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.LaboratoryName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridMetaLab);

                        UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxSampleLaboratory, cboxOrderLaboratory);
                    }
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
            FormUser form = new FormUser();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridMetaUsers);
            }
        }

        private void miEditUser_Click(object sender, EventArgs e)
        {
            // edit user
            if (gridMetaUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid uid = Guid.Parse(gridMetaUsers.SelectedRows[0].Cells["id"].Value.ToString());

            FormUser form = new FormUser(uid);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridMetaUsers);
            }
        }        

        private void miDeleteUser_Click(object sender, EventArgs e)
        {
            // Delete user
        }

        private void miResetPass_Click(object sender, EventArgs e)
        {
            // reset password
        }

        private void miEditLaboratory_Click(object sender, EventArgs e)
        {
            // edit lab
            if (gridMetaLab.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaLab.SelectedRows[0];
            Guid lid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormLaboratory form = new FormLaboratory(lid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.LaboratoryName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {                        
                        UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridMetaLab);
                        UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxSampleLaboratory, cboxOrderLaboratory);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update laboratory failed", StatusMessageType.Error);                    
                    break;
            }                        
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

                        UI.PopulateComboBoxes(conn, "csp_select_projects_main_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleProject, cboxSamplesProjects);
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

            Guid pmid = Guid.Parse(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());

            FormProject form = new FormProject(pmid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Project " + form.ProjectName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsMain(conn, gridProjectMain);

                        UI.PopulateComboBoxes(conn, "csp_select_projects_main_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleProject, cboxSamplesProjects);
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

            Guid pmid = Guid.Parse(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());
            string pmname = gridProjectMain.SelectedRows[0].Cells["name"].Value.ToString();
            
            FormProjectSub form = new FormProjectSub(pmname, pmid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sub project " + form.ProjectSubName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsSub(conn, pmid, gridProjectSub);

                        UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                            new SqlParameter("@project_main_id", pmid),
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleSubProject);
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

            Guid pmid = Guid.Parse(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());
            string pmname = gridProjectMain.SelectedRows[0].Cells["name"].Value.ToString();
            Guid psid = Guid.Parse(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());

            FormProjectSub form = new FormProjectSub(pmname, pmid, psid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Project " + form.ProjectSubName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateProjectsSub(conn, pmid, gridProjectSub);

                        UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                            new SqlParameter("@project_main_id", pmid),
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleSubProject);
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

            FormSampleType form = new FormSampleType(treeSampleTypes.SelectedNode, false);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        DB.LoadSampleTypes(conn);
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
            {
                MessageBox.Show("You must select a parent sample type");
                return;
            }

            FormSampleType form = new FormSampleType(treeSampleTypes.SelectedNode, true);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        DB.LoadSampleTypes(conn);
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
            string tempFileName = Path.GetTempFileName() + ".txt";
            File.Copy(DSALogger.LogFile, tempFileName, true);
            Process.Start(tempFileName);
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
            tabs.SelectedTab = tabMetadata;
            tabsMeta.SelectedTab = tabCustomers;
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
            Guid nid = Guid.Parse(row.Cells["id"].Value.ToString());

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
                        UI.PopulateComboBoxes(conn, "csp_select_preparation_geometries_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxPrepAnalPrepGeom);
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
            Guid gid = Guid.Parse(row.Cells["id"].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());            

            row = gridSysMunicipality.SelectedRows[0];
            Guid mid = Guid.Parse(row.Cells["id"].Value.ToString());            

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
                        UI.PopulateComboBoxes(conn, "csp_select_stations_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoStations);
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
            Guid sid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormStation form = new FormStation(sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Station " + form.StationName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateStations(conn, gridMetaStation);                        
                        UI.PopulateComboBoxes(conn, "csp_select_stations_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoStations);
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
            Guid ssid = Guid.Parse(row.Cells["id"].Value.ToString());

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

                        UI.PopulateComboBoxes(conn, "csp_select_samplers", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoSampler);
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
            {
                MessageBox.Show("You must select a sampler first");
                return;
            }

            DataGridViewRow row = gridMetaSamplers.SelectedRows[0];
            Guid sid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormSampler form = new FormSampler(sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.SamplerName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplers(conn, gridMetaSamplers);                        
                        UI.PopulateComboBoxes(conn, "csp_select_samplers", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoSampler);
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

            if (!Utils.IsValidGuid(cboxSampleSampleType.SelectedValue))
            {
                cboxSampleSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Guid.Parse(cboxSampleSampleType.SelectedValue.ToString());
            TreeNode[] tnodes = treeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleSampleComponent);
            }
        }        

        private void cboxSampleSampleType_Leave(object sender, EventArgs e)
        {
            ClearStatusMessage();

            if (String.IsNullOrEmpty(cboxSampleSampleType.Text.Trim()))
            {                
                cboxSampleSampleType.SelectedItem = Guid.Empty;
                cboxSampleSampleComponent.DataSource = null;
                return;
            }

            if(!Utils.IsValidGuid(cboxSampleSampleType.SelectedValue))
            {
                cboxSampleSampleType.SelectedValue = Guid.Empty;
                cboxSampleSampleComponent.DataSource = null;
                SetStatusMessage("You must select an existing sample type", StatusMessageType.Warning);
            }
        }

        private void btnSampleSelectSampleType_Click(object sender, EventArgs e)
        {
            FormSelectSampleType form = new FormSelectSampleType();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            cboxSampleSampleType.SelectedValue = form.SelectedSampleTypeId;
        }

        private void cboxSampleProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleProject.SelectedValue))
            {
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                return;
            }

            Guid projectId = Guid.Parse(cboxSampleProject.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSampleSubProject);
            }

            lblSampleToolProject.Text = "[Project] " + cboxSampleProject.Text;
            lblSampleToolSubProject.Text = "";
        }

        private void cboxSampleSubProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleSubProject.SelectedValue))
            {
                lblSampleToolSubProject.Text = "";
                return;
            }

            lblSampleToolSubProject.Text = cboxSampleSubProject.Text;
        }

        private void cboxSampleInfoStations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxSampleInfoStations.SelectedValue))
            {
                tbSampleInfoLatitude.Text = "";
                tbSampleInfoLongitude.Text = "";
                tbSampleInfoAltitude.Text = "";
                return;
            }

            Guid stationId = Guid.Parse(cboxSampleInfoStations.SelectedValue.ToString());

            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_station", CommandType.StoredProcedure, 
                    new SqlParameter("@id", stationId)))
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
            if(!Utils.IsValidGuid(cboxSampleCounties.SelectedValue))
            {
                cboxSampleMunicipalities.DataSource = null;
                return;
            }

            Guid countyId = Guid.Parse(cboxSampleCounties.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_municipalities_for_county_short", new[] {
                    new SqlParameter("@county_id", countyId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxSampleMunicipalities);
            }
        }        

        private void miSamplesNew_Click(object sender, EventArgs e)
        {
            // new sample            
            ClearSample();
            selectedSampleId = Guid.Empty;

            FormSampleNew form = new FormSampleNew(treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            selectedSampleId = form.SampleId;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateSample(conn, selectedSampleId);
            }

            tabs.SelectedTab = tabSample;

            lblStatus.Text = Utils.makeStatusMessage("Sample " + form.SampleNumber + " created successfully");
        }

        private void miSamplesImportExcel_Click(object sender, EventArgs e)
        {
            // Import sample from excel
        }

        private void ClearPrepAnalSample()
        {
            tbPrepAnalInfoComment.Text = "";
            tbPrepAnalDryWeight.Text = "";
            tbPrepAnalWetWeight.Text = "";
            tbPrepAnalVolume.Text = "";
            tbPrepAnalLODStartWeight.Text = "";
            tbPrepAnalLODEndWeight.Text = "";
            tbPrepAnalLODWater.Text = "";
            tbPrepAnalLODTemp.Text = "";
        }

        private void ClearPrepAnalPreparation()
        {
            cboxPrepAnalPrepGeom.SelectedValue = Guid.Empty;
            tbPrepAnalPrepFillHeight.Text = "";
            tbPrepAnalPrepAmount.Text = "";
            cboxPrepAnalPrepAmountUnit.SelectedValue = 0;
            cboxPrepAnalPrepWorkflowStatus.SelectedValue = WorkflowStatus.Construction;
            tbPrepAnalPrepReqUnit.Text = "";
            tbPrepAnalPrepComment.Text = "";
            lblPrepAnalPrepRange.Text = "";
        }

        private void ClearPrepAnalAnalysis()
        {
            cboxPrepAnalAnalUnit.SelectedValue = Guid.Empty;
            cboxPrepAnalAnalUnitType.SelectedValue = Guid.Empty;
            cboxPrepAnalAnalSigmaAct.SelectedValue = 0;
            tbPrepAnalAnalSpecRef.Text = "";
            tbPrepAnalAnalNuclLib.Text = "";
            tbPrepAnalAnalMDALib.Text = "";
            cboxPrepAnalAnalWorkflowStatus.SelectedValue = WorkflowStatus.Construction;
            tbPrepAnalAnalComment.Text = "";
        }

        private void ClearSample()
        {
            cboxSampleSampleType.SelectedValue = Guid.Empty;
            cboxSampleSampleComponent.SelectedValue = Guid.Empty;
            cboxSampleInfoSampler.SelectedValue = Guid.Empty;
            cboxSampleInfoSamplingMeth.SelectedValue = Guid.Empty;
            cboxSampleProject.SelectedValue = Guid.Empty;
            cboxSampleSubProject.SelectedValue = Guid.Empty;
            cboxSampleInfoStations.SelectedValue = Guid.Empty;
            tbSampleInfoLatitude.Text = "";
            tbSampleInfoLongitude.Text = "";
            tbSampleInfoAltitude.Text = "";
            cboxSampleCounties.SelectedValue = Guid.Empty;
            cboxSampleMunicipalities.SelectedValue = Guid.Empty;
            cboxSampleInfoLocationTypes.SelectedValue = -1;
            tbSampleLocation.Text = "";
            cboxSampleLaboratory.SelectedValue = Guid.Empty;
            DateTime now = DateTime.Now;
            tbSampleSamplingDateFrom.Text = "";
            tbSampleSamplingDateTo.Text = "";
            tbSampleReferenceDate.Text = "";
            tbSampleExId.Text = "";
            cbSampleConfidential.Checked = false;
            cboxSampleSampleStorage.SelectedValue = Guid.Empty;
            tbSampleComment.Text = "";
            gridSampleAttachments.DataSource = null;
        }

        private void miSamplesEdit_Click(object sender, EventArgs e)
        {
            // edit sample

            if(gridSamples.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a sample first");
                return;
            }

            selectedSampleId = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());

            ClearSample();

            using (SqlConnection conn = DB.OpenConnection())
            {                
                PopulateSample(conn, selectedSampleId);
            }

            tabs.SelectedTab = tabSample;
        }

        private void PopulateSample(SqlConnection conn, Guid sampleId)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample", CommandType.StoredProcedure,
                new SqlParameter("@id", sampleId)))
            {
                if (!reader.HasRows)
                {
                    Common.Log.Error("Sample with ID " + sampleId.ToString() + " was not found");
                    MessageBox.Show("Sample with ID " + sampleId.ToString() + " was not found");
                    return;
                }

                reader.Read();

                map["id"] = reader["id"];
                map["number"] = reader["number"];
                map["laboratory_id"] = reader["laboratory_id"];
                map["sample_type_id"] = reader["sample_type_id"];
                map["sample_storage_id"] = reader["sample_storage_id"];
                map["sample_component_id"] = reader["sample_component_id"];
                map["project_sub_id"] = reader["project_sub_id"];
                map["station_id"] = reader["station_id"];
                map["sampler_id"] = reader["sampler_id"];
                map["sampling_method_id"] = reader["sampling_method_id"];
                map["transform_from_id"] = reader["transform_from_id"];
                map["transform_to_id"] = reader["transform_to_id"];
                map["imported_from"] = reader["imported_from"];
                map["imported_from_id"] = reader["imported_from_id"];
                map["municipality_id"] = reader["municipality_id"];
                map["location_type"] = reader["location_type"];
                map["location"] = reader["location"];
                map["latitude"] = reader["latitude"];
                map["longitude"] = reader["longitude"];
                map["altitude"] = reader["altitude"];
                map["sampling_date_from"] = reader["sampling_date_from"];
                map["use_sampling_date_to"] = reader["use_sampling_date_to"];
                map["sampling_date_to"] = reader["sampling_date_to"];
                map["reference_date"] = reader["reference_date"];
                map["external_id"] = reader["external_id"];
                map["wet_weight_g"] = reader["wet_weight_g"];
                map["dry_weight_g"] = reader["dry_weight_g"];
                map["volume_l"] = reader["volume_l"];
                map["lod_weight_start"] = reader["lod_weight_start"];
                map["lod_weight_end"] = reader["lod_weight_end"];
                map["lod_temperature"] = reader["lod_temperature"];
                map["confidential"] = reader["confidential"];
                map["parameters"] = reader["parameters"];
                map["instance_status_id"] = reader["instance_status_id"];
                map["comment"] = reader["comment"];
                map["create_date"] = reader["create_date"];
                map["created_by"] = reader["created_by"];
                map["update_date"] = reader["update_date"];
                map["updated_by"] = reader["updated_by"];
            }
            
            cboxSampleSampleType.SelectedValue = map["sample_type_id"];
            cboxSampleSampleComponent.SelectedValue = map["sample_component_id"];
            cboxSampleInfoSampler.SelectedValue = map["sampler_id"];
            cboxSampleInfoSamplingMeth.SelectedValue = map["sampling_method_id"];

            if (map["project_sub_id"] != DBNull.Value)
            {
                object mpid = DB.GetScalar(conn, "select project_main_id from project_sub where id = @id", CommandType.Text, new SqlParameter("@id", map["project_sub_id"]));
                cboxSampleProject.SelectedValue = mpid;
                cboxSampleSubProject.SelectedValue = map["project_sub_id"];
            }
            
            cboxSampleInfoStations.SelectedValue = map["station_id"];

            if (map["municipality_id"] != DBNull.Value)
            {
                object cid = DB.GetScalar(conn, "select county_id from municipality where id = @id", CommandType.Text, new SqlParameter("@id", map["municipality_id"]));
                cboxSampleCounties.SelectedValue = cid;
                cboxSampleMunicipalities.SelectedValue = map["municipality_id"];
            }

            cboxSampleInfoLocationTypes.Text = map["location_type"].ToString();
            tbSampleLocation.Text = map["location"].ToString();

            cboxSampleLaboratory.SelectedValue = map["laboratory_id"];

            if(map["sampling_date_from"] == DBNull.Value)
            {
                tbSampleSamplingDateFrom.Text = "";
                tbSampleSamplingDateFrom.Tag = null;
            }   
            else
            {
                DateTime samplingDateFrom = Convert.ToDateTime(map["sampling_date_from"]);
                tbSampleSamplingDateFrom.Text = samplingDateFrom.ToString(Utils.DateTimeFormatNorwegian);
                tbSampleSamplingDateFrom.Tag = samplingDateFrom;
            }

            if (map["sampling_date_to"] == DBNull.Value)
            {
                tbSampleSamplingDateTo.Text = "";
                tbSampleSamplingDateTo.Tag = null;
            }
            else
            {
                DateTime samplingDateTo = Convert.ToDateTime(map["sampling_date_to"]);
                tbSampleSamplingDateTo.Text = samplingDateTo.ToString(Utils.DateTimeFormatNorwegian);
                tbSampleSamplingDateTo.Tag = samplingDateTo;
            }

            if (map["reference_date"] == DBNull.Value)
            {
                tbSampleReferenceDate.Text = "";
                tbSampleReferenceDate.Tag = null;
            }
            else
            {
                DateTime referenceDate = Convert.ToDateTime(map["reference_date"]);
                tbSampleReferenceDate.Text = referenceDate.ToString(Utils.DateTimeFormatNorwegian);
                tbSampleReferenceDate.Tag = referenceDate;
            }

            tbSampleExId.Text = map["external_id"].ToString();
            cbSampleConfidential.Checked = Convert.ToBoolean(map["confidential"]);

            cboxSampleSampleStorage.SelectedValue = map["sample_storage_id"];

            cboxSampleInstanceStatus.SelectedValue = map["instance_status_id"];

            tbSampleComment.Text = map["comment"].ToString();
            lblSampleToolId.Text = "[Sample] " + map["number"].ToString();
            lblSampleToolLaboratory.Text = String.IsNullOrEmpty(cboxSampleLaboratory.Text) ? "" : "[Laboratory] " + cboxSampleLaboratory.Text;
        }

        private void miSamplesDelete_Click(object sender, EventArgs e)
        {
            // delete sample
        }

        private void miSamplesSplit_Click(object sender, EventArgs e)
        {
            // split sample
            if(gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample to split");
                return;
            }

            Guid sid = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());
            string comp = gridSamples.SelectedRows[0].Cells["sample_component_name"].Value.ToString();
            if(!String.IsNullOrEmpty(comp))
            {
                MessageBox.Show("Cannot split a sample which is already a component");
                return;
            }

            using (SqlConnection conn = DB.OpenConnection())
            {
                int mergeTest = Convert.ToInt32(DB.GetScalar(conn, "select count(transform_to_id) from sample where id = '" + sid.ToString() + "'", CommandType.Text));
                if (mergeTest > 0)
                {
                    MessageBox.Show("Cannot split, sample has already been merged");
                    return;
                }
            }   
                                     
            FormSampleSplit form = new FormSampleSplit(sid, treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSamples(conn, gridSamples);
            }

            lblStatus.Text = Utils.makeStatusMessage("Splitting sample successful");
        }

        private void miSamplesMerge_Click(object sender, EventArgs e)
        {
            // merge sample
            if (gridSamples.SelectedRows.Count < 2)
            {
                MessageBox.Show("You must select two or more samples to merge");
                return;
            }

            List<Guid> sampleIds = new List<Guid>();
            foreach(DataGridViewRow row in gridSamples.SelectedRows)            
                sampleIds.Add(Guid.Parse(row.Cells["id"].Value.ToString()));

            var sampleIdsArr = from item in sampleIds select "'" + item + "'";
            string sampleIdsCsv = string.Join(",", sampleIdsArr);

            using (SqlConnection conn = DB.OpenConnection())
            {
                int mergeTest = Convert.ToInt32(DB.GetScalar(conn, "select count(transform_to_id) from sample where id in (" + sampleIdsCsv + ")", CommandType.Text));
                if(mergeTest > 0)
                {
                    MessageBox.Show("Cannot merge, one or more of these samples has already been merged");
                    return;
                }

                Func<string, int> nCheck = field => Convert.ToInt32(DB.GetScalar(conn, "select count(distinct(" + field + ")) from sample where id in(" + sampleIdsCsv + ")", CommandType.Text));

                // FIXME: Must select new sample type
                if ((nCheck("laboratory_id") & nCheck("project_sub_id")) != 1)
                {
                    MessageBox.Show("All samples to be merged must have the same laboratory and project");
                    return;
                }
            }                

            FormSampleMerge form = new FormSampleMerge(sampleIdsCsv);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateSamples(conn, gridSamples);
            }

            lblStatus.Text = Utils.makeStatusMessage("Merging samples successful");
        }

        private void miSamplesSetOrder_Click(object sender, EventArgs e)
        {
            // add sample to order
            if (gridSamples.SelectedRows.Count < 1)
                return;

            Guid sampleId = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());
            string sampleName = gridSamples.SelectedRows[0].Cells["number"].Value.ToString();

            bool hasLock = false;
            using (SqlConnection conn = DB.OpenConnection())
            {
                hasLock = DB.LockSample(conn, sampleId);
            }

            if (!hasLock)
            {
                MessageBox.Show("Sample " + sampleName + " is already locked by someone else");
                return;
            }

            FormSelectOrder form = new FormSelectOrder(treeSampleTypes, sampleId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                lblStatus.Text = Utils.makeStatusMessage("Successfully added sample " + sampleName + " to order " + form.SelectedOrderName);
            }

            using (SqlConnection conn = DB.OpenConnection())
            {
                DB.UnlockSamples(conn);
            }
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
            if(gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample first");
                return;
            }

            selectedSampleId = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulatePrepAnal(conn, selectedSampleId);
            }

            ClearPrepAnalSample();
            ClearPrepAnalPreparation();
            ClearPrepAnalAnalysis();

            tabs.SelectedTab = tabPrepAnal;
        }

        private void PopulatePrepAnal(SqlConnection conn, Guid sampleId)
        {
            treePrepAnal.Nodes.Clear();
            
            TreeNode sampleNode = null;

            string query = @"
select s.id as 'sample_id', s.number as 'sample_number', st.name as 'sample_type_name', l.name as 'laboratory_name'
from sample s
inner join sample_type st on st.id = s.sample_type_id
inner join laboratory l on l.id = s.laboratory_id
where s.id = @id
";

            using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, 
                new SqlParameter("@id", sampleId)))
            {
                reader.Read();
                string txt = reader["sample_number"].ToString() + " - " + reader["sample_type_name"].ToString() + ", " + reader["laboratory_name"].ToString();
                sampleNode = treePrepAnal.Nodes.Add(sampleId.ToString(), txt);
            }

            query = @"
select p.id as 'preparation_id', p.number as 'preparation_number', a.name as 'assignment_name', pm.name as 'preparation_method_name'
from preparation p 
inner join preparation_method pm on pm.id = p.preparation_method_id
left outer join assignment a on a.id = p.assignment_id
where sample_id = @sample_id
order by p.number
";
            using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text,
                new SqlParameter("@sample_id", sampleId)))
            {
                while (reader.Read())
                {
                    string txt = reader["preparation_number"].ToString() + " - " + reader["preparation_method_name"].ToString();
                    if(!String.IsNullOrEmpty(reader["assignment_name"].ToString()))
                        txt += ", " + reader["assignment_name"].ToString();
                    TreeNode prepNode = sampleNode.Nodes.Add(reader["preparation_id"].ToString(), txt);
                }
            }
                
            foreach (TreeNode prepNode in sampleNode.Nodes)
            {
                Guid prepId = Guid.Parse(prepNode.Name);
                query = @"
select a.id as 'analysis_id', a.number as 'analysis_number', am.name as 'analysis_method_name', ass.name as 'assignment_name'
from analysis a 
inner join analysis_method am on am.id = a.analysis_method_id
left outer join assignment ass on ass.id = a.assignment_id
where preparation_id = @preparation_id
order by a.number
";
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text,
                    new SqlParameter("@preparation_id", prepId)))
                {
                    while (reader.Read())
                    {
                        string txt = reader["analysis_number"].ToString() + " - " + reader["analysis_method_name"].ToString();
                        if(!String.IsNullOrEmpty(reader["assignment_name"].ToString()))
                            txt += ", " + reader["assignment_name"].ToString();
                        TreeNode analNode = prepNode.Nodes.Add(reader["analysis_id"].ToString(), txt);
                    }
                }
            }

            treePrepAnal.ExpandAll();
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
                        UI.PopulateComboBoxes(conn, "csp_select_sampling_methods_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoSamplingMeth);
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
            Guid smid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormSamplingMeth form = new FormSamplingMeth(smid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampling method " + form.SamplingMethodName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);                        
                        UI.PopulateComboBoxes(conn, "csp_select_sampling_methods_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleInfoSamplingMeth);
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
            if (!Utils.IsValidGuid(cboxSamplesProjects.SelectedValue))
            {
                cboxSamplesProjectsSub.DataSource = null;
                return;
            }

            Guid projectId = Guid.Parse(cboxSamplesProjects.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {                
                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSamplesProjectsSub);
            }
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
            Guid pmid = Guid.Parse(row.Cells["id"].Value.ToString());

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
            Guid amid = Guid.Parse(row.Cells["id"].Value.ToString());

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
                Guid.Parse(treeSampleTypes.SelectedNode.Name), 
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
            Guid sampleTypeId = Guid.Parse(tnode.Name);
            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand(@"
select pm.id, pm.name from preparation_method pm	
    inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
    inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
order by name
", conn);
                cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), sampleTypeId));
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())                    
                        existingMethods.Add(Guid.Parse(reader["id"].ToString()));
                }

                if (ascend)
                {
                    while (tnode.Parent != null)
                    {
                        tnode = tnode.Parent;
                        sampleTypeId = Guid.Parse(tnode.Name);

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), sampleTypeId));
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                existingMethods.Add(Guid.Parse(reader["id"].ToString()));
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
            Guid sampleTypeId = Guid.Parse(treeSampleTypes.SelectedNode.Name);

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

            Guid sampleTypeId = Guid.Parse(treeSampleTypes.SelectedNode.Name);

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

            Guid amid = Guid.Parse(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value.ToString());
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
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_nuclides_for_analysis_method", CommandType.StoredProcedure,
                    new SqlParameter("@analysis_method_id", amid)))
                {
                    while (reader.Read())
                        existingNuclides.Add(Guid.Parse(reader["id"].ToString()));
                }
            }

            return existingNuclides;
        }

        private void miAnalysisMethodsRemNuclide_Click(object sender, EventArgs e)
        {
            // remove nuclides from analysis method
        }        

        private void miTypeRelPrepMethAddAnalMeth_Click(object sender, EventArgs e)
        {
            // add analysis methods to preparation method
            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }

            Guid pmid = Guid.Parse(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value.ToString());
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
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_analysis_methods_for_preparation_method", CommandType.StoredProcedure,
                    new SqlParameter("@preparation_method_id", pmid)))
                {
                    while (reader.Read())
                        existingAnalysisMethods.Add(Guid.Parse(reader["id"].ToString()));
                }
            }

            return existingAnalysisMethods;
        }

        private void miTypeRelPrepMethRemAnalMeth_Click(object sender, EventArgs e)
        {
            // remove analysis methods to preparation method
        }        

        private void miOrdersClearAllFilters_Click(object sender, EventArgs e)
        {
            // clear all order filters
        }        

        private void miOrdersNew_Click(object sender, EventArgs e)
        {
            // create new order
            ClearOrderInfo();
            selectedOrderId = Guid.Empty;

            FormOrderNew form = new FormOrderNew();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            selectedOrderId = form.OrderId;
            tbOrderName.Text = form.OrderName;
            PopulateOrder(selectedOrderId);

            tabs.SelectedTab = tabOrder;
        }

        private void miOrdersEdit_Click(object sender, EventArgs e)
        {
            // edit existing order
            if(gridOrders.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an order first");
                return;
            }

            ClearOrderInfo();
            selectedOrderId = Guid.Parse(gridOrders.SelectedRows[0].Cells["id"].Value.ToString());
            PopulateOrder(selectedOrderId);
            
            tabs.SelectedTab = tabOrder;
        }

        private void miOrdersDelete_Click(object sender, EventArgs e)
        {
            // delete order
        }

        private void ClearOrderInfo()
        {
            tbOrderName.Text = "";
            cboxOrderLaboratory.SelectedValue = Guid.Empty;
            cboxOrderResponsible.SelectedValue = Guid.Empty;
            tbOrderDeadline.Text = "";
            cboxOrderRequestedSigma.SelectedValue = -1;
            tbOrderContentComment.Text = "";
            tbOrderCustomerName.Text = "";                        
            tbOrderCustomerEmail.Text = "";
            tbOrderCustomerPhone.Text = "";
            tbOrderCustomerAddress.Text = "";
            tbOrderReportComment.Text = "";
            // TODO
        }

        private void PopulateOrder(Guid id)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_assignment", CommandType.StoredProcedure, 
                    new SqlParameter("@id", id)))
                {
                    if (!reader.HasRows)
                    {
                        Common.Log.Error("Order with ID " + id.ToString() + " was not found");
                        MessageBox.Show("Order with ID " + id.ToString() + " was not found");
                        return;
                    }

                    reader.Read();

                    map["id"] = reader["id"];
                    map["name"] = reader["name"];
                    map["laboratory_id"] = reader["laboratory_id"];
                    map["account_id"] = reader["account_id"];
                    map["deadline"] = reader["deadline"];
                    map["requested_sigma_act"] = reader["requested_sigma_act"];
                    map["requested_sigma_mda"] = reader["requested_sigma_mda"];
                    map["customer_name"] = reader["customer_name"];                                        
                    map["customer_email"] = reader["customer_email"];
                    map["customer_phone"] = reader["customer_phone"];
                    map["customer_address"] = reader["customer_address"];
                    map["approved_customer"] = reader["approved_customer"];
                    map["approved_laboratory"] = reader["approved_laboratory"];
                    map["content_comment"] = reader["content_comment"];
                    map["report_comment"] = reader["report_comment"];
                    map["closed_date"] = reader["closed_date"];
                    map["closed_by"] = reader["closed_by"];
                    map["instance_status_id"] = reader["instance_status_id"];
                    map["locked_by"] = reader["locked_by"];
                    map["create_date"] = reader["create_date"];
                    map["created_by"] = reader["created_by"];
                    map["update_date"] = reader["update_date"];
                    map["updated_by"] = reader["updated_by"];    
                }

                tbOrderName.Text = map["name"].ToString();
                cboxOrderLaboratory.SelectedValue = map["laboratory_id"];
                Guid accId = Guid.Parse(map["account_id"].ToString());
                cboxOrderResponsible.SelectedValue = accId;                
                DateTime deadline = Convert.ToDateTime(map["deadline"]);
                tbOrderDeadline.Text = deadline.ToString(Utils.DateFormatNorwegian);
                tbOrderDeadline.Tag = deadline;
                cboxOrderRequestedSigma.SelectedValue = map["requested_sigma_act"];
                cboxOrderRequestedSigmaMDA.SelectedValue = map["requested_sigma_mda"];
                CustomerModel cust = new CustomerModel();
                cust.Name = map["customer_name"].ToString();                
                cust.Email = map["customer_email"].ToString();
                cust.Phone = map["customer_phone"].ToString();
                cust.Address = map["customer_address"].ToString();
                tbOrderCustomer.Text = cust.Name;
                tbOrderCustomer.Tag = cust;
                tbOrderCustomerName.Text = cust.Name;                                
                tbOrderCustomerEmail.Text = cust.Email;
                tbOrderCustomerPhone.Text = cust.Phone;
                tbOrderCustomerAddress.Text = cust.Address;
                tbOrderContentComment.Text = map["content_comment"].ToString();
                tbOrderReportComment.Text = map["report_comment"].ToString();                

                UI.PopulateOrderContent(conn, selectedOrderId, treeOrderContent, Guid.Empty, treeSampleTypes, true);

                gridOrderConnectedItems.DataSource = null;
            }
        }

        private void miOrderAddSampleType_Click(object sender, EventArgs e)
        {
            // add sample type to order
            FormOrderAddSampleType form = new FormOrderAddSampleType(selectedOrderId, treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrderContent(conn, selectedOrderId, treeOrderContent, Guid.Empty, treeSampleTypes, true);
            }
        }

        private void miOrderRemSampleType_Click(object sender, EventArgs e)
        {
            // remove sample type from order
        }

        private void miOrderAddPrepMeth_Click(object sender, EventArgs e)
        {
            // add preparation method to order
            if (treeOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            TreeNode tnode = treeOrderContent.SelectedNode;
            if(tnode.Level != 0)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            Guid orderSampleTypeId = Guid.Parse(tnode.Name);
            FormOrderAddPrepMeth form = new FormOrderAddPrepMeth(orderSampleTypeId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrderContent(conn, selectedOrderId, treeOrderContent, Guid.Empty, treeSampleTypes, true);
            }
        }

        private void miOrderRemPrepMeth_Click(object sender, EventArgs e)
        {
            // remove preparation method from order
        }

        private void miOrderAddAnalMeth_Click(object sender, EventArgs e)
        {
            // add analysis method to order
            if (treeOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }

            TreeNode tnode = treeOrderContent.SelectedNode;

            if (tnode.Level != 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }
            
            Guid orderPrepMethId = Guid.Parse(tnode.Name);
            FormOrderAddAnalMeth form = new FormOrderAddAnalMeth(orderPrepMethId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateOrderContent(conn, selectedOrderId, treeOrderContent, Guid.Empty, treeSampleTypes, true);
            }
        }

        private void miOrderRemAnalMeth_Click(object sender, EventArgs e)
        {
            // remove analysis method from order
        }

        private void miOrderSave_Click(object sender, EventArgs e)
        {
            // save order
            if(!Utils.IsValidGuid(cboxOrderLaboratory.SelectedValue))
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (cboxOrderResponsible.SelectedValue == null)
            {
                MessageBox.Show("Responsible is mandatory");
                return;
            }

            if (tbOrderDeadline.Tag == null)
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }

            DateTime dl = (DateTime)tbOrderDeadline.Tag;
            if (dl < DateTime.Now)
            {
                MessageBox.Show("Deadline can not be in the past");
                return;
            }

            if (tbOrderCustomer.Tag == null)
            {
                MessageBox.Show("Customer is mandatory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();                

                Guid labId = Guid.Parse(cboxOrderLaboratory.SelectedValue.ToString());
                CustomerModel cust = (CustomerModel)tbOrderCustomer.Tag;

                SqlCommand cmd = new SqlCommand("csp_update_assignment_details", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;                
                cmd.Parameters.AddWithValue("@id", selectedOrderId);                
                cmd.Parameters.AddWithValue("@laboratory_id", labId);
                cmd.Parameters.AddWithValue("@account_id", DB.MakeParam(typeof(String), cboxOrderResponsible.SelectedValue));
                cmd.Parameters.AddWithValue("@deadline", DB.MakeParam(typeof(DateTime), tbOrderDeadline.Tag));
                cmd.Parameters.AddWithValue("@requested_sigma_act", DB.MakeParam(typeof(double), cboxOrderRequestedSigma.SelectedValue));
                cmd.Parameters.AddWithValue("@requested_sigma_mda", DB.MakeParam(typeof(double), cboxOrderRequestedSigmaMDA.SelectedValue));
                cmd.Parameters.AddWithValue("@customer_name", DB.MakeParam(typeof(string), cust.Name));                                
                cmd.Parameters.AddWithValue("@customer_email", DB.MakeParam(typeof(string), cust.Email));
                cmd.Parameters.AddWithValue("@customer_phone", DB.MakeParam(typeof(string), cust.Phone));
                cmd.Parameters.AddWithValue("@customer_address", DB.MakeParam(typeof(string), cust.Address));
                cmd.Parameters.AddWithValue("@content_comment", DB.MakeParam(typeof(string), tbOrderContentComment.Text));
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active); // FIXME
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                trans.Commit();
                
                lblStatus.Text = Utils.makeStatusMessage("Order " + tbOrderName.Text + " updated");

                UI.PopulateOrders(conn, InstanceStatus.Deleted, gridOrders);
                tabs.SelectedTab = tabOrders;
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void btnOrderSelectDeadline_Click(object sender, EventArgs e)
        {
            FormSelectDate form = new FormSelectDate();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            DateTime selectedDate = form.SelectedDate;
            tbOrderDeadline.Tag = selectedDate;
            tbOrderDeadline.Text = selectedDate.ToString(Utils.DateFormatNorwegian);
        }

        private void miCustomersNew_Click(object sender, EventArgs e)
        {
            // create new customer            
            FormCustomer form = new FormCustomer();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer " + form.CustomerName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCustomers(conn, InstanceStatus.Deleted, gridCustomers);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create customer failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCustomersEdit_Click(object sender, EventArgs e)
        {
            // edit customer
            if (gridCustomers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a customer first");
                return;
            }

            DataGridViewRow row = gridCustomers.SelectedRows[0];
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormCustomer form = new FormCustomer(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer " + form.CustomerName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCustomers(conn, InstanceStatus.Deleted, gridCustomers);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update customer failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCustomersDelete_Click(object sender, EventArgs e)
        {
            // delete customer
        }        

        private void gridSysCounty_SelectionChanged(object sender, EventArgs e)
        {
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            Guid cid = Guid.Parse(gridSysCounty.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
                UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
        }

        private void gridProjectMain_SelectionChanged(object sender, EventArgs e)
        {
            if (gridProjectMain.SelectedRows.Count < 1)
                return;

            Guid pmid = Guid.Parse(gridProjectMain.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectsSub(conn, pmid, gridProjectSub);
            }
        }

        private void gridTypeRelAnalMeth_SelectionChanged(object sender, EventArgs e)
        {
            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
                return;

            Guid amid = Guid.Parse(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalMethNuclides(conn, amid, lbTypRelAnalMethNuclides);
            }
        }

        private void gridTypeRelPrepMeth_SelectionChanged(object sender, EventArgs e)
        {
            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
                return;

            Guid pmid = Guid.Parse(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulatePrepMethAnalMeths(conn, pmid, lbTypRelPrepMethAnalMeth);
            }
        }

        private void cboxOrderLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxOrderLaboratory.SelectedValue))
            {
                cboxOrderResponsible.SelectedValue = Guid.Empty;
                return;
            }

            Guid labId = Guid.Parse(cboxOrderLaboratory.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_accounts_for_laboratory", new[] {
                    new SqlParameter("@laboratory_id", labId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxOrderResponsible);
            }
        }

        private void treePrepAnal_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnPrepAnalAddAnal.Enabled = false;

            switch (e.Node.Level)
            {
                case 0:
                    ClearPrepAnalSample();
                    Guid sid = Guid.Parse(e.Node.Name);
                    PopulateSampleInfo(sid, e.Node);
                    tabsPrepAnal.SelectedTab = tabPrepAnalSample;
                    break;
                case 1:
                    ClearPrepAnalPreparation();
                    Guid pid = Guid.Parse(e.Node.Name);
                    PopulatePreparation(pid);
                    btnPrepAnalAddAnal.Enabled = true;
                    tabsPrepAnal.SelectedTab = tabPrepAnalPreps;
                    break;
                case 2:
                    ClearPrepAnalAnalysis();
                    Guid aid = Guid.Parse(e.Node.Name);
                    PopulateAnalysis(aid);
                    tabsPrepAnal.SelectedTab = tabPrepAnalAnalysis;
                    break;
            }
        }

        private void TreeDrawNode(DrawTreeNodeEventArgs e)
        {
            if (e.Node == null)
                return;
            
            var selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var unfocused = !e.Node.TreeView.Focused;
            
            if (selected && unfocused)
            {
                var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, SystemColors.HighlightText, TextFormatFlags.GlyphOverhangPadding);
            }
            else e.DrawDefault = true;            
        }

        private void treePrepAnal_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            TreeDrawNode(e);
        }

        private void treeSampleTypes_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            TreeDrawNode(e);
        }

        private void tabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if ((e.TabPage == tabSample || e.TabPage == tabPrepAnal) && selectedSampleId != Guid.Empty)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    if (!DB.LockSample(conn, selectedSampleId))
                    {
                        MessageBox.Show("Unable to lock sample");
                        e.Cancel = true;
                    }
                }
            }
            else if (e.TabPage == tabOrder && selectedOrderId != Guid.Empty)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    if (!DB.LockOrder(conn, selectedOrderId))
                    {
                        MessageBox.Show("Unable to lock order");
                        e.Cancel = true;
                    }
                }
            }
            else if (e.TabPage == tabOrders)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    UI.PopulateOrders(conn, InstanceStatus.Active, gridOrders);
                }
            }
            else if (e.TabPage == tabSamples)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    UI.PopulateSamples(conn, gridSamples);
                }
            }
        }

        private void tabs_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if(e.TabPage == tabSample || e.TabPage == tabPrepAnal)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    DB.UnlockSamples(conn);
                }                    
            }
            else if (e.TabPage == tabOrder)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    DB.UnlockOrders(conn);
                }
            }
        }

        private void miBack_Click(object sender, EventArgs e)
        {
            if (tabs.SelectedTab == tabSample || tabs.SelectedTab == tabPrepAnal)
                tabs.SelectedTab = tabSamples;
            else if (tabs.SelectedTab == tabOrder)
                tabs.SelectedTab = tabOrders;
            else tabs.SelectedTab = tabMenu;
        }

        private void btnSampleUpdate_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSampleLaboratory.SelectedValue))
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxSampleSampleType.SelectedValue))
            {
                MessageBox.Show("Sample type is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxSampleProject.SelectedValue))
            {
                MessageBox.Show("Main project is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxSampleSubProject.SelectedValue))
            {
                MessageBox.Show("Sub project is mandatory");
                return;
            }

            SqlConnection conn = null;            

            try
            {
                conn = DB.OpenConnection();

                // update selected sample
                SqlCommand cmd = new SqlCommand("select number from sample where id = @id", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", selectedSampleId);
                object oNumber = cmd.ExecuteScalar();

                cmd.CommandText = "csp_update_sample";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", selectedSampleId);
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), cboxSampleLaboratory.SelectedValue));
                cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), cboxSampleSampleType.SelectedValue));
                cmd.Parameters.AddWithValue("@sample_storage_id", DB.MakeParam(typeof(Guid), cboxSampleSampleStorage.SelectedValue));
                cmd.Parameters.AddWithValue("@sample_component_id", DB.MakeParam(typeof(Guid), cboxSampleSampleComponent.SelectedValue));
                cmd.Parameters.AddWithValue("@project_sub_id", DB.MakeParam(typeof(Guid), cboxSampleSubProject.SelectedValue));
                cmd.Parameters.AddWithValue("@station_id", DB.MakeParam(typeof(Guid), cboxSampleInfoStations.SelectedValue));
                cmd.Parameters.AddWithValue("@sampler_id", DB.MakeParam(typeof(Guid), cboxSampleInfoSampler.SelectedValue));
                cmd.Parameters.AddWithValue("@sampling_method_id", DB.MakeParam(typeof(Guid), cboxSampleInfoSamplingMeth.SelectedValue));
                cmd.Parameters.AddWithValue("@municipality_id", DB.MakeParam(typeof(Guid), cboxSampleMunicipalities.SelectedValue));
                cmd.Parameters.AddWithValue("@location_type", DB.MakeParam(typeof(string), cboxSampleInfoLocationTypes.Text));
                cmd.Parameters.AddWithValue("@location", DB.MakeParam(typeof(string), tbSampleLocation.Text.Trim()));
                cmd.Parameters.AddWithValue("@latitude", DB.MakeParam(typeof(double), tbSampleInfoLatitude.Text.Trim()));
                cmd.Parameters.AddWithValue("@longitude", DB.MakeParam(typeof(double), tbSampleInfoLongitude.Text.Trim()));
                cmd.Parameters.AddWithValue("@altitude", DB.MakeParam(typeof(double), tbSampleInfoAltitude.Text.Trim()));
                cmd.Parameters.AddWithValue("@sampling_date_from", DB.MakeParam(typeof(DateTime), tbSampleSamplingDateFrom.Tag));
                cmd.Parameters.AddWithValue("@use_sampling_date_to", 0); // FIXME
                cmd.Parameters.AddWithValue("@sampling_date_to", DB.MakeParam(typeof(DateTime), tbSampleSamplingDateTo.Tag));
                cmd.Parameters.AddWithValue("@reference_date", DB.MakeParam(typeof(DateTime), tbSampleReferenceDate.Tag));
                cmd.Parameters.AddWithValue("@external_id", DB.MakeParam(typeof(string), tbSampleExId.Text.Trim()));
                cmd.Parameters.AddWithValue("@confidential", cbSampleConfidential.Checked ? 1 : 0);
                cmd.Parameters.AddWithValue("@instance_status_id", DB.MakeParam(typeof(int), cboxSampleInstanceStatus.SelectedValue));
                cmd.Parameters.AddWithValue("@comment", tbSampleComment.Text.Trim());
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                lblStatus.Text = Utils.makeStatusMessage("Sample " + oNumber.ToString() + " updated");
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void panelSampleLatLonAlt_Resize(object sender, EventArgs e)
        {
            int w = panelSampleLatLonAlt.Width;
            tbSampleInfoLatitude.Width = w / 3;
            tbSampleInfoAltitude.Width = w / 3;
        }

        private void btnPrepAnalPrepUpdate_Click(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(treePrepAnal.SelectedNode.Name))
            {
                MessageBox.Show("No valid preparation ID found");
                return;
            }

            Guid pid = Guid.Parse(treePrepAnal.SelectedNode.Name);
            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_update_preparation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", pid);
                cmd.Parameters.AddWithValue("@preparation_geometry_id", DB.MakeParam(typeof(Guid), cboxPrepAnalPrepGeom.SelectedValue));
                cmd.Parameters.AddWithValue("@workflow_status_id", cboxPrepAnalPrepWorkflowStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@amount", DB.MakeParam(typeof(double), tbPrepAnalPrepAmount.Text));
                cmd.Parameters.AddWithValue("@prep_unit_id", DB.MakeParam(typeof(Guid), cboxPrepAnalPrepAmountUnit.SelectedValue));
                cmd.Parameters.AddWithValue("@quantity", DB.MakeParam(typeof(double), tbPrepAnalPrepQuantity.Text));
                cmd.Parameters.AddWithValue("@quantity_unit_id", DB.MakeParam(typeof(Guid), cboxPrepAnalPrepQuantityUnit.SelectedValue));
                cmd.Parameters.AddWithValue("@fill_height_mm", DB.MakeParam(typeof(double), tbPrepAnalPrepFillHeight.Text));
                cmd.Parameters.AddWithValue("@comment", tbPrepAnalPrepComment.Text);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                lblStatus.Text = Utils.makeStatusMessage("Preparation updated successfully");
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void PopulatePreparation(Guid pid)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_preparation", CommandType.StoredProcedure, 
                    new SqlParameter("@id", pid)))
                {
                    reader.Read();

                    cboxPrepAnalPrepGeom.SelectedValue = reader["preparation_geometry_id"];
                    tbPrepAnalPrepFillHeight.Text = reader["fill_height_mm"].ToString();
                    tbPrepAnalPrepAmount.Text = reader["amount"].ToString();
                    cboxPrepAnalPrepAmountUnit.SelectedValue = reader["prep_unit_id"];
                    tbPrepAnalPrepQuantity.Text = reader["quantity"].ToString();
                    cboxPrepAnalPrepQuantityUnit.SelectedValue = reader["quantity_unit_id"];
                    cboxPrepAnalPrepWorkflowStatus.SelectedValue = reader["workflow_status_id"];
                    tbPrepAnalPrepComment.Text = reader["comment"].ToString();
                }

                string query = @"
select au.name + ', ' + aut.name from preparation p
    inner join sample s on p.sample_id = s.id
    inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
    inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id
    inner join assignment a on a.id = p.assignment_id
    inner join activity_unit au on au.id = ast.requested_activity_unit_id
    inner join activity_unit_type aut on aut.id = ast.requested_activity_unit_type_id
where p.id = @pid
";
                object o = DB.GetScalar(conn, query, CommandType.Text, new SqlParameter("@pid", pid));
                if(o != null && o != DBNull.Value)
                    tbPrepAnalPrepReqUnit.Text = o.ToString();
            }                
        }        

        private void btnPrepAnalAnalUpdate_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(treePrepAnal.SelectedNode.Name))
            {
                MessageBox.Show("No valid analysis ID found");
                return;
            }

            Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);
            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_update_analysis", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", aid);
                cmd.Parameters.AddWithValue("@workflow_status_id", cboxPrepAnalAnalWorkflowStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@specter_reference", tbPrepAnalAnalSpecRef.Text);
                cmd.Parameters.AddWithValue("@activity_unit_id", DB.MakeParam(typeof(Guid), cboxPrepAnalAnalUnit.SelectedValue));
                cmd.Parameters.AddWithValue("@activity_unit_type_id", DB.MakeParam(typeof(Guid), cboxPrepAnalAnalUnitType.SelectedValue));
                cmd.Parameters.AddWithValue("@sigma_act", DB.MakeParam(typeof(int), cboxPrepAnalAnalSigmaAct.SelectedValue));
                cmd.Parameters.AddWithValue("@sigma_mda", DB.MakeParam(typeof(int), cboxPrepAnalAnalSigmaMDA.SelectedValue));
                cmd.Parameters.AddWithValue("@nuclide_library", tbPrepAnalAnalNuclLib.Text);
                cmd.Parameters.AddWithValue("@mda_library", tbPrepAnalAnalMDALib.Text);
                cmd.Parameters.AddWithValue("@comment", tbPrepAnalAnalComment.Text);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                lblStatus.Text = Utils.makeStatusMessage("Analysis updated successfully");
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void PopulateAnalysis(Guid aid)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_analysis", CommandType.StoredProcedure,
                    new SqlParameter("@id", aid)))
                {
                    reader.Read();

                    cboxPrepAnalAnalUnit.SelectedValue = reader["activity_unit_id"];
                    cboxPrepAnalAnalUnitType.SelectedValue = reader["activity_unit_type_id"];
                    cboxPrepAnalAnalSigmaAct.SelectedValue = reader["sigma_act"];
                    cboxPrepAnalAnalSigmaMDA.SelectedValue = reader["sigma_mda"];
                    tbPrepAnalAnalSpecRef.Text = reader["specter_reference"].ToString();
                    tbPrepAnalAnalNuclLib.Text = reader["nuclide_library"].ToString();
                    tbPrepAnalAnalMDALib.Text = reader["mda_library"].ToString();
                    cboxPrepAnalAnalWorkflowStatus.SelectedValue = reader["workflow_status_id"];
                    tbPrepAnalAnalComment.Text = reader["comment"].ToString();
                    selectedAnalysisMethodId = Guid.Parse(reader["analysis_method_id"].ToString());
                }

                UI.PopulateAnalysisResults(conn, aid, gridPrepAnalResults);
            }
        }

        private void cboxPrepAnalPrepGeom_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblPrepAnalPrepRange.Text = "";

            if (Utils.IsValidGuid(cboxPrepAnalPrepGeom.SelectedValue))
            {
                string fhInfo = "[";
                Guid geomId = Guid.Parse(cboxPrepAnalPrepGeom.SelectedValue.ToString());

                using (SqlConnection conn = DB.OpenConnection())
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, "select min_fill_height_mm, max_fill_height_mm from preparation_geometry where id = @id", CommandType.Text,
                        new SqlParameter("@id", geomId)))
                    {
                        reader.Read();
                        fhInfo += reader["min_fill_height_mm"] + ", " + reader["max_fill_height_mm"] + "]";
                    }
                    lblPrepAnalPrepRange.Text = fhInfo;
                }
            }
        }

        private void btnPrepAnalSampleUpdate_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(treePrepAnal.SelectedNode.Name))
            {
                MessageBox.Show("No valid sample ID found");
                return;
            }

            Guid sid = Guid.Parse(treePrepAnal.SelectedNode.Name);
            SqlConnection conn = null;

            if(!String.IsNullOrEmpty(tbPrepAnalLODStartWeight.Text) && !String.IsNullOrEmpty(tbPrepAnalLODEndWeight.Text))
            {
                double lodStart = Convert.ToDouble(tbPrepAnalLODStartWeight.Text);
                double lodEnd = Convert.ToDouble(tbPrepAnalLODEndWeight.Text);
                if (lodStart < lodEnd)
                {
                    MessageBox.Show("LOD start weight cannot be smaller than end weight");
                    return;
                }
            }

            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_update_sample_info", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", sid);
                cmd.Parameters.AddWithValue("@wet_weight_g", DB.MakeParam(typeof(double), tbPrepAnalWetWeight.Text));
                cmd.Parameters.AddWithValue("@dry_weight_g", DB.MakeParam(typeof(double), tbPrepAnalDryWeight.Text));
                cmd.Parameters.AddWithValue("@volume_l", DB.MakeParam(typeof(double), tbPrepAnalVolume.Text));
                cmd.Parameters.AddWithValue("@lod_weight_start", DB.MakeParam(typeof(double), tbPrepAnalLODStartWeight.Text));
                cmd.Parameters.AddWithValue("@lod_weight_end", DB.MakeParam(typeof(double), tbPrepAnalLODEndWeight.Text));
                cmd.Parameters.AddWithValue("@lod_temperature", DB.MakeParam(typeof(double), tbPrepAnalLODTemp.Text));
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                lblStatus.Text = Utils.makeStatusMessage("Sample data updated successfully");
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void PopulateSampleInfo(Guid sid, TreeNode tnode)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_info", CommandType.StoredProcedure,
                    new SqlParameter("@id", sid)))
                {
                    reader.Read();

                    DateTime refDate = Convert.ToDateTime(reader["reference_date"]);

                    tnode.ToolTipText = "Component: " + reader["sample_component_name"].ToString() + Environment.NewLine
                        + "External Id: " + reader["external_id"].ToString() + Environment.NewLine
                        + "Project: " + reader["project_name"].ToString() + Environment.NewLine
                        + "Reference date: " + refDate.ToString(Utils.DateFormatNorwegian);

                    tbPrepAnalInfoComment.Text = reader["comment"].ToString();                    
                    tbPrepAnalWetWeight.Text = reader["wet_weight_g"].ToString();
                    tbPrepAnalDryWeight.Text = reader["dry_weight_g"].ToString();
                    tbPrepAnalVolume.Text = reader["volume_l"].ToString();
                    tbPrepAnalLODStartWeight.Text = reader["lod_weight_start"].ToString();
                    tbPrepAnalLODEndWeight.Text = reader["lod_weight_end"].ToString();
                    tbPrepAnalLODTemp.Text = reader["lod_temperature"].ToString();
                }
            }
        }

        private void miImportLISFile_Click(object sender, EventArgs e)
        {
            Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);
            bool doClearAnalysis = false;
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                object oCount = DB.GetScalar(connection, transaction, "select count(*) from analysis_result where analysis_id = @aid", CommandType.Text, new SqlParameter("@aid", aid));
                if (oCount != null && oCount != DBNull.Value)
                {
                    int cnt = Convert.ToInt32(oCount);
                    if (cnt > 0)
                    {
                        if (MessageBox.Show("There are already analysis results on this analysis, are you sure you want to replace them?", "Question", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        doClearAnalysis = true;
                    }
                }

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "LIS files (*.lis)|*.lis";
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                AnalysisParameters analysisParameters = new AnalysisParameters();
                analysisParameters.FileName = dialog.FileName;
                analysisParameters.PreparationGeometry = cboxPrepAnalPrepGeom.Text;
                analysisParameters.SampleName = treePrepAnal.Nodes.Count > 0 ? treePrepAnal.Nodes[0].Text : "";
                analysisParameters.SpectrumReferenceRegEx = ""; // FIXME
                analysisParameters.AllNuclides = DB.GetNuclideNames(connection, transaction);
                analysisParameters.AnalMethNuclides = DB.GetNuclideNamesForAnalysisMethod(connection, transaction, selectedAnalysisMethodId);

                AnalysisResult analysisResult = new AnalysisResult();
                FormImportAnalysisLIS form = new FormImportAnalysisLIS(analysisParameters, analysisResult);
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                if(doClearAnalysis)
                    ClearAnalysisResults(connection, transaction, aid);

                SqlCommand cmd = new SqlCommand(@"
update analysis set 
    specter_reference = @specter_reference,
    nuclide_library = @nuclide_library,
    mda_library = @mda_library,
    sigma_act = @sigma_act,
    sigma_mda = @sigma_mda,
    update_date = @update_date,
    updated_by = @updated_by
where id = @id
", connection, transaction);
                cmd.Parameters.AddWithValue("@id", aid);
                cmd.Parameters.AddWithValue("@specter_reference", analysisResult.SpectrumName);
                cmd.Parameters.AddWithValue("@nuclide_library", analysisResult.NuclideLibrary);
                cmd.Parameters.AddWithValue("@mda_library", analysisResult.DetLimLib);
                cmd.Parameters.AddWithValue("@sigma_act", analysisResult.SigmaAct);
                cmd.Parameters.AddWithValue("@sigma_mda", analysisResult.SigmaMDA);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
insert into analysis_result values(
    @id,
    @analysis_id,
    @nuclide_id,
    @activity,
    @activity_uncertainty_abs,
    @activity_approved,
    @uniform_activity,
    @uniform_activity_unit_id,
    @detection_limit, 
    @detection_limit_approved,
    @accredited,
    @reportable,
    @instance_status_id,
    @create_date,
    @created_by,
    @update_date,
    @updated_by)";

                foreach (AnalysisResult.Isotop iso in analysisResult.Isotopes)
                {                    
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@analysis_id", aid);
                    object oNuclId = DB.GetScalar(connection, transaction, "select id from nuclide where name = '" + iso.NuclideName + "'", CommandType.Text);
                    if(oNuclId == null || oNuclId == DBNull.Value)
                    {
                        Common.Log.Warn("Unregistered nuclide: " + iso.NuclideName);
                        continue;
                    }                    
                    cmd.Parameters.AddWithValue("@nuclide_id", DB.MakeParam(typeof(Guid), oNuclId));
                    cmd.Parameters.AddWithValue("@activity", iso.Activity);
                    cmd.Parameters.AddWithValue("@activity_uncertainty_abs", iso.Uncertainty);
                    cmd.Parameters.AddWithValue("@activity_approved", iso.ApprovedRES);                    
                    double uAct = -1.0;
                    int uActUnitId = -1;
                    if(Utils.IsValidGuid(cboxPrepAnalAnalUnit.SelectedValue))
                    {
                        Guid analUnitId = Guid.Parse(cboxPrepAnalAnalUnit.SelectedValue.ToString());
                        DB.GetUniformActivity(connection, transaction, iso.Activity, analUnitId, out uAct, out uActUnitId);
                    }                    
                    cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                    cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                    cmd.Parameters.AddWithValue("@detection_limit", iso.MDA);
                    cmd.Parameters.AddWithValue("@detection_limit_approved", iso.ApprovedMDA);
                    cmd.Parameters.AddWithValue("@accredited", iso.Accredited);
                    cmd.Parameters.AddWithValue("@reportable", iso.Reportable);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(Utils.makeErrorMessage(ex.Message));
            }
            finally
            {
                connection?.Close();
            }

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalysisResults(conn, aid, gridPrepAnalResults);
            }
        }

        private void tbPrepAnalLODStartWeight_TextChanged(object sender, EventArgs e)
        {
            CalculateLODPercent();
        }

        private void CalculateLODPercent()
        {
            string startWeight = tbPrepAnalLODStartWeight.Text.Trim();
            string endWeight = tbPrepAnalLODEndWeight.Text.Trim();

            if (String.IsNullOrEmpty(startWeight) || String.IsNullOrEmpty(endWeight))
            {
                tbPrepAnalLODWater.Text = "";
                return;
            }

            double sw = Convert.ToDouble(startWeight);
            double ew = Convert.ToDouble(endWeight);

            if(sw < ew)
            {
                tbPrepAnalLODWater.Text = "";
                return;
            }

            double delta = sw - ew;
            double precent = (delta / sw) * 100.0;
            tbPrepAnalLODWater.Text = precent.ToString("0.0#");
        }

        private void btnPrepAnalAddPrep_Click(object sender, EventArgs e)
        {
            FormPrepAnalAddPrep form = new FormPrepAnalAddPrep(selectedSampleId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulatePrepAnal(conn, selectedSampleId);
            }
        }

        private void btnPrepAnalAddAnal_Click(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(treePrepAnal.SelectedNode.Name))
                return;

            Guid prepId = Guid.Parse(treePrepAnal.SelectedNode.Name);

            FormPrepAnalAddAnal form = new FormPrepAnalAddAnal(prepId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulatePrepAnal(conn, selectedSampleId);
            }
        }

        private void btnSampleAddSampleToOrder_Click(object sender, EventArgs e)
        {            
            FormSelectOrder form = new FormSelectOrder(treeSampleTypes, selectedSampleId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            lblStatus.Text = Utils.makeStatusMessage("Successfully added sample " + form.SelectedSampleNumber.ToString() + " to order " + form.SelectedOrderName);
        }

        private void btnSampleSamplingDateFrom_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleSamplingDateFrom.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);
            tbSampleSamplingDateFrom.Tag = form.SelectedDateTime;
        }

        private void btnSampleSamplingDateFromClear_Click(object sender, EventArgs e)
        {
            tbSampleSamplingDateFrom.Text = "";
            tbSampleSamplingDateFrom.Tag = null;
        }

        private void btnSampleSamplingDateToClear_Click(object sender, EventArgs e)
        {
            tbSampleSamplingDateTo.Text = "";
            tbSampleSamplingDateTo.Tag = null;
        }

        private void btnSampleSamplingDateTo_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleSamplingDateTo.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);
            tbSampleSamplingDateTo.Tag = form.SelectedDateTime;
        }

        private void btnSampleReferenceDateClear_Click(object sender, EventArgs e)
        {
            tbSampleReferenceDate.Text = "";
            tbSampleReferenceDate.Tag = null;
        }

        private void btnSampleReferenceDate_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleReferenceDate.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);
            tbSampleReferenceDate.Tag = form.SelectedDateTime;
        }

        private void cboxSampleLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxSampleLaboratory.SelectedValue))            
                lblSampleToolLaboratory.Text = "";
            else            
                lblSampleToolLaboratory.Text = "[Laboratory] " + cboxSampleLaboratory.Text;
        }

        private void btnOrderClearDeadline_Click(object sender, EventArgs e)
        {
            tbOrderDeadline.Text = "";
            tbOrderDeadline.Tag = null;
        }

        private void btnPrepAnalClearAnal_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all results from this analysis?", "Question", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);            

            using (SqlConnection conn = DB.OpenConnection())
            {
                ClearAnalysisResults(conn, null, aid);
                UI.PopulateAnalysisResults(conn, aid, gridPrepAnalResults);
            }
        }

        private void ClearAnalysisResults(SqlConnection conn, SqlTransaction trans, Guid analysisId)
        {                        
            SqlCommand cmd = new SqlCommand("delete from analysis_result where analysis_id = @aid", conn);
            if (trans != null)
                cmd.Transaction = trans;

            cmd.Parameters.AddWithValue("@aid", analysisId);
            cmd.ExecuteNonQuery();
        }

        private void btnOrderSelectCustomer_Click(object sender, EventArgs e)
        {
            FormSelectCustomer form = new FormSelectCustomer();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            CustomerModel c = form.SelectedCustomer;
            tbOrderCustomer.Text = c.Name;
            tbOrderCustomer.Tag = c;

            tbOrderCustomerName.Text = c.Name;                        
            tbOrderCustomerEmail.Text = c.Email;
            tbOrderCustomerPhone.Text = c.Phone;
            tbOrderCustomerAddress.Text = c.Address;
        }

        private void miPersonNew_Click(object sender, EventArgs e)
        {
            // new person
            FormPerson form = new FormPerson();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Person " + form.PersonName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulatePersons(conn, gridSysPers);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create person failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miPersonEdit_Click(object sender, EventArgs e)
        {
            // edit person
            if(gridSysPers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a person first");
                return;
            }

            Guid pid = Guid.Parse(gridSysPers.SelectedRows[0].Cells["id"].Value.ToString());
            FormPerson form = new FormPerson(pid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Person " + form.PersonName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulatePersons(conn, gridSysPers);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update person failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miPersonDelete_Click(object sender, EventArgs e)
        {
            // delete person
        }

        private void miCompanyNew_Click(object sender, EventArgs e)
        {
            // new company
            FormCompany form = new FormCompany();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Company " + form.CompName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCompanies(conn, gridMetaCompanies);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create company failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCompanyEdit_Click(object sender, EventArgs e)
        {
            // edit company
            if (gridMetaCompanies.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a company first");
                return;
            }

            Guid cid = Guid.Parse(gridMetaCompanies.SelectedRows[0].Cells["id"].Value.ToString());
            FormCompany form = new FormCompany(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Company " + form.CompName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCompanies(conn, gridMetaCompanies);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update company failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCompanyDelete_Click(object sender, EventArgs e)
        {
            // delete company
        }

        private void treeOrderContent_AfterSelect(object sender, TreeViewEventArgs e)
        {
            gridOrderConnectedItems.DataSource = null;
            string query = "";
            Guid orderSampleTypeId = Guid.Empty;
            Guid orderPrepMethId = Guid.Empty;
            Guid orderAnalMethId = Guid.Empty;

            switch (e.Node.Level)
            {
                case 0:
                    query = @"
select s.id, s.number as 'Connected samples', st.name as 'Type', sc.name as 'Comp.', sa.name as 'Sampler'
from sample s
	inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
	inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id and ast.id = @astid
	inner join assignment a on a.id = ast.assignment_id
    inner join sample_type st on st.id = s.sample_type_id
    left outer join sample_component sc on sc.id = s.sample_component_id
    left outer join cv_sampler sa on sa.id = s.sampler_id
where a.id = @aid
order by s.number
";
                    orderSampleTypeId = Guid.Parse(e.Node.Name);
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        gridOrderConnectedItems.DataSource = DB.GetDataTable(conn, query, CommandType.Text, new[] {
                            new SqlParameter("@aid", selectedOrderId),
                            new SqlParameter("@astid", orderSampleTypeId)
                        });
                        gridOrderConnectedItems.Columns["id"].Visible = false;
                    }
                    break;

                case 1:
                    query = @"
select 
    p.id, 
    convert(nvarchar(50), s.number) + '/' + convert(nvarchar(50), p.number) as 'Preparations',
    pm.name as 'Method',
    pg.name as 'Geometry',
    ws.name as 'Status'
from preparation p
	inner join sample s on p.sample_id = s.id
	inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
	inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id and ast.id = @astid
	inner join assignment_preparation_method apm on apm.assignment_sample_type_id = ast.id and apm.id = @apmid
	inner join assignment a on a.id = ast.assignment_id and a.id = @aid
    inner join preparation_method pm on pm.id = p.preparation_method_id
    left outer join preparation_geometry pg on pg.id = p.preparation_geometry_id
    inner join workflow_status ws on ws.id = p.workflow_status_id
where p.assignment_id = @aid
order by s.number, p.number
";
                    orderSampleTypeId = Guid.Parse(e.Node.Parent.Name);
                    orderPrepMethId = Guid.Parse(e.Node.Name);
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        gridOrderConnectedItems.DataSource = DB.GetDataTable(conn, query, CommandType.Text, new[] {
                            new SqlParameter("@aid", selectedOrderId),
                            new SqlParameter("@astid", orderSampleTypeId),
                            new SqlParameter("@apmid", orderPrepMethId)
                        });
                        gridOrderConnectedItems.Columns["id"].Visible = false;
                    }
                    break;

                case 2:
                    query = @"
select
	convert(nvarchar(50), s.number) + '/' + convert(nvarchar(50), p.number) + '/' + convert(nvarchar(50), anal.number) as 'Analyses',
    am.name as 'Method',
    anal.specter_reference as 'Spec.ref.',
    ws.name as 'Status'
from analysis anal
	inner join preparation p on anal.preparation_id = p.id and p.assignment_id = @aid
	inner join sample s on p.sample_id = s.id
	inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
	inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id and ast.id = @astid
	inner join assignment_preparation_method apm on apm.assignment_sample_type_id = ast.id and apm.id = @apmid
	inner join assignment_analysis_method aam on aam.assignment_preparation_method_id = apm.id and aam.id = @aamid
	inner join assignment a on a.id = ast.assignment_id and a.id = @aid
    left outer join analysis_method am on am.id = anal.analysis_method_id
    inner join workflow_status ws on ws.id = anal.workflow_status_id
where anal.assignment_id = @aid
order by s.number, p.number
";
                    orderSampleTypeId = Guid.Parse(e.Node.Parent.Parent.Name);
                    orderPrepMethId = Guid.Parse(e.Node.Parent.Name);
                    orderAnalMethId = Guid.Parse(e.Node.Name);
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        gridOrderConnectedItems.DataSource = DB.GetDataTable(conn, query, CommandType.Text, new[] {
                            new SqlParameter("@aid", selectedOrderId),
                            new SqlParameter("@astid", orderSampleTypeId),
                            new SqlParameter("@apmid", orderPrepMethId),
                            new SqlParameter("@aamid", orderAnalMethId),
                        });
                    }
                    break;
            }
        }        

        private void miTypeRelSampleTypesExportSampTypeList_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();            
            dialog.Filter = "TXT files (*.txt)|*.txt";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            List<string> stList = new List<string>();
            AddSampleTypesToList(treeSampleTypes.Nodes, stList);

            stList.Sort(delegate (string s1, string s2) { return s1.CompareTo(s2); });

            using (TextWriter writer = File.CreateText(dialog.FileName))
            {
                foreach (string s in stList)
                    writer.WriteLine(s);
            }
        }

        private void AddSampleTypesToList(TreeNodeCollection nodes, List<string> list)
        {
            foreach (TreeNode tn in nodes)
            {
                list.Add(tn.Text + " -> " + tn.Tag.ToString());
                AddSampleTypesToList(tn.Nodes, list);
            }
        }

        private void btnPrepAnalEditResult_Click(object sender, EventArgs e)
        {
            if(gridPrepAnalResults.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a result first");
                return;
            }

            if (!Utils.IsValidGuid(cboxPrepAnalAnalUnit.SelectedValue))
            {
                MessageBox.Show("You must save a unit first");
                return;
            }

            Guid unitId = Guid.Parse(cboxPrepAnalAnalUnit.SelectedValue.ToString());
            Guid resultId = Guid.Parse(gridPrepAnalResults.SelectedRows[0].Cells["id"].Value.ToString());
            string nuclName = gridPrepAnalResults.SelectedRows[0].Cells["nuclide_name"].Value.ToString();            
            FormPrepAnalResult form = new FormPrepAnalResult(resultId, unitId, nuclName);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            Guid analId = Guid.Parse(treePrepAnal.SelectedNode.Name);
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalysisResults(conn, analId, gridPrepAnalResults);
            }
        }

        private void btnPrepAnalAddResult_Click(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxPrepAnalAnalUnit.SelectedValue))
            {
                MessageBox.Show("You must save a unit first");
                return;
            }

            Guid unitId = Guid.Parse(cboxPrepAnalAnalUnit.SelectedValue.ToString());
            Guid analId = Guid.Parse(treePrepAnal.SelectedNode.Name);
            List<string> nuclides = null;
            using (SqlConnection conn = DB.OpenConnection())
            {
                nuclides = DB.GetNuclideNamesForAnalysisMethod(conn, null, selectedAnalysisMethodId);
            }
            List<string> existingNuclides = new List<string>();
            foreach (DataGridViewRow row in gridPrepAnalResults.Rows)
            {
                string nucl = row.Cells["nuclide_name"].Value.ToString();
                existingNuclides.Add(nucl);
            }
            nuclides.RemoveAll(x => existingNuclides.Contains(x));
            FormPrepAnalResult form = new FormPrepAnalResult(analId, unitId, nuclides);
            if (form.ShowDialog() != DialogResult.OK)
                return;
            
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateAnalysisResults(conn, analId, gridPrepAnalResults);
            }
        }
    }    
}
