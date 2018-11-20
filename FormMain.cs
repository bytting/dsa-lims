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

        private Guid selectedOrderId = Guid.Empty;
        private Guid selectedSampleId = Guid.Empty;        

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

                tbMenuLookup.Text = "";
                ActiveControl = tbMenuLookup;

                panelSampleLatLonAlt_Resize(sender, e);

                r = new ResourceManager("DSA_lims.lang_" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                Common.Log.Info("Setting language " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                SetLanguageLabels(r);                                

                Common.Log.Info("Loading settings file " + DSAEnvironment.SettingsFilename);
                LoadSettings(DSAEnvironment.SettingsFilename);
                cbMachineSettingsUseAD.Checked = Common.Settings.UseActiveDirectoryCredentials;

                if (!Directory.Exists(DSAEnvironment.SettingsPath))
                    Directory.CreateDirectory(DSAEnvironment.SettingsPath);

                if (!Directory.Exists(DSAEnvironment.AnalysisPluginDirectory))
                    Directory.CreateDirectory(DSAEnvironment.AnalysisPluginDirectory);

                using (SqlConnection conn = DB.OpenConnection())
                {
                    Common.Username = "Admin"; // FIXME

                    DB.LoadSampleTypes(conn);
                    
                    cboxSamplesStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                    cboxSampleInstanceStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_instance_status");

                    cboxPrepAnalPrepAmountUnit.DataSource = DB.GetIntLemmata(conn, "csp_select_preparation_units", true);

                    cboxPrepAnalPrepQuantityUnit.DataSource = DB.GetIntLemmata(conn, "csp_select_quantity_units", true);

                    cboxPrepAnalAnalWorkflowStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_workflow_status");

                    cboxPrepAnalPrepWorkflowStatus.DataSource = DB.GetIntLemmata(conn, "csp_select_workflow_status");

                    cboxSampleInfoLocationTypes.DataSource = DB.GetIntLemmata(conn, "csp_select_location_types", true);

                    cboxPrepAnalAnalSigma.DataSource = DB.GetSigmaValues();

                    cboxOrderRequestedSigma.DataSource = DB.GetSigmaValues();

                    UI.PopulateActivityUnits(conn, gridMetaUnitsActivity);
                    
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

                    UI.PopulateComboBoxes(conn, "csp_select_customers", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Active)
                    }, cboxOrderCustomerName);                    
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

            using (SqlConnection conn = DB.OpenConnection())
            {
                DB.UnlockSamples(conn);
                DB.UnlockOrders(conn);
            }
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
            Guid lid = Guid.Parse(row.Cells[0].Value.ToString());

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

        private void miEditUser_Click(object sender, EventArgs e)
        {
            // edit user
            if(gridMetaUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }
            
            string uname = gridMetaUsers.SelectedRows[0].Cells["username"].Value.ToString();

            FormUser form = new FormUser(uname);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridMetaUsers);
            }
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
            Guid nid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid gid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid cid = Guid.Parse(row.Cells[0].Value.ToString());            

            row = gridSysMunicipality.SelectedRows[0];
            Guid mid = Guid.Parse(row.Cells[0].Value.ToString());            

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
            Guid sid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid ssid = Guid.Parse(row.Cells[0].Value.ToString());

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
                        UI.PopulateComboBoxes(conn, "csp_select_samplers_short", new[] {
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
                return;

            DataGridViewRow row = gridMetaSamplers.SelectedRows[0];
            Guid sid = Guid.Parse(row.Cells[0].Value.ToString());

            FormSampler form = new FormSampler(sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.SamplerName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSamplers(conn, gridMetaSamplers);                        
                        UI.PopulateComboBoxes(conn, "csp_select_samplers_short", new[] {
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
            tbPrepAnalPrepComment.Text = "";
            lblPrepAnalPrepRange.Text = "";
        }

        private void ClearPrepAnalAnalysis()
        {
            cboxPrepAnalAnalUnit.SelectedValue = Guid.Empty;
            cboxPrepAnalAnalUnitType.SelectedValue = Guid.Empty;
            cboxPrepAnalAnalSigma.SelectedValue = 0;
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
            dtSampleSamplingDateFrom.Value = now;
            dtSampleSamplingTimeFrom.Value = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
            cbSampleUseSamplingTimeTo.Checked = false;
            dtSampleSamplingDateTo.Value = now;
            dtSampleSamplingTimeTo.Value = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
            cbSampleUseSamplingTimeTo.Checked = false;
            dtSampleSamplingDateTo.Enabled = false;
            dtSampleSamplingTimeTo.Enabled = false;
            dtSampleReferenceDate.Value = now;
            dtSampleReferenceTime.Value = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
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

            DateTime samplingDateFrom = Convert.ToDateTime(map["sampling_date_from"]);
            DateTime samplingDateTo = Convert.ToDateTime(map["sampling_date_to"]);
            DateTime referenceDate = Convert.ToDateTime(map["reference_date"]);
            dtSampleSamplingDateFrom.Value = dtSampleSamplingTimeFrom.Value = samplingDateFrom;
            cbSampleUseSamplingTimeTo.Checked = Convert.ToBoolean(map["use_sampling_date_to"]);
            dtSampleSamplingDateTo.Value = dtSampleSamplingTimeTo.Value = samplingDateTo;
            dtSampleReferenceDate.Value = dtSampleReferenceTime.Value = referenceDate;

            tbSampleExId.Text = map["external_id"].ToString();
            cbSampleConfidential.Checked = Convert.ToBoolean(map["confidential"]);

            cboxSampleSampleStorage.SelectedValue = map["sample_storage_id"];

            cboxSampleInstanceStatus.SelectedValue = map["instance_status_id"];

            tbSampleComment.Text = map["comment"].ToString();
            lblSampleToolId.Text = "[Sample] " + map["number"].ToString();
            lblSampleToolLaboratory.Text = String.IsNullOrEmpty(cboxSampleLaboratory.Text) ? "" : "[Laboratory] " + cboxSampleLaboratory.Text;
        }

        private void cbSampleUseSamplingTimeTo_CheckedChanged(object sender, EventArgs e)
        {
            dtSampleSamplingDateTo.Enabled = cbSampleUseSamplingTimeTo.Checked;
            dtSampleSamplingTimeTo.Enabled = cbSampleUseSamplingTimeTo.Checked;
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
            string sampleNumber = gridSamples.SelectedRows[0].Cells["number"].Value.ToString();

            FormSelectOrder form = new FormSelectOrder(treeSampleTypes, sampleId, sampleNumber);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            string sampleName = gridSamples.SelectedRows[0].Cells["number"].Value.ToString();

            lblStatus.Text = Utils.makeStatusMessage("Successfully added sample " + sampleName + " to order " + form.SelectedOrderName);
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
";
            using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text,
                new SqlParameter("@sample_id", sampleId)))
            {
                while (reader.Read())
                {
                    string txt = reader["preparation_number"].ToString() 
                        + " - " + reader["preparation_method_name"].ToString() + ", " 
                        + reader["assignment_name"].ToString();
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
";
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text,
                    new SqlParameter("@preparation_id", prepId)))
                {
                    while (reader.Read())
                    {
                        string txt = reader["analysis_number"].ToString() + " - " 
                            + reader["analysis_method_name"].ToString() + ", " 
                            + reader["assignment_name"].ToString();
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
            Guid smid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid pmid = Guid.Parse(row.Cells[0].Value.ToString());

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
            Guid amid = Guid.Parse(row.Cells[0].Value.ToString());

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
                MessageBox.Show("You must select an order forst");
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
            tbOrderCustomerAddress.Text = "";
            tbOrderCustomerEmail.Text = "";
            tbOrderCustomerPhone.Text = "";
            cboxOrderCustomerName.SelectedValue = Guid.Empty;
            tbOrderContactEmail.Text = "";
            tbOrderContactPhone.Text = "";
            cboxOrderContact.SelectedValue = Guid.Empty;
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
                    map["requested_sigma"] = reader["requested_sigma"];
                    map["customer_name"] = reader["customer_name"];
                    map["customer_address"] = reader["customer_address"];
                    map["customer_email"] = reader["customer_email"];
                    map["customer_phone"] = reader["customer_phone"];
                    map["customer_contact_name"] = reader["customer_contact_name"];
                    map["customer_contact_email"] = reader["customer_contact_email"];
                    map["customer_contact_phone"] = reader["customer_contact_phone"];
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
                cboxOrderResponsible.SelectedValue = map["account_id"];
                DateTime deadline = Convert.ToDateTime(map["deadline"]);
                tbOrderDeadline.Text = deadline.ToString(Utils.DateFormatNorwegian);
                cboxOrderRequestedSigma.SelectedValue = map["requested_sigma"];
                tbOrderContentComment.Text = map["content_comment"].ToString();
                cboxOrderCustomerName.Text = map["customer_name"].ToString();
                tbOrderCustomerAddress.Text = map["customer_address"].ToString();
                tbOrderCustomerEmail.Text = map["customer_email"].ToString();
                tbOrderCustomerPhone.Text = map["customer_phone"].ToString();
                cboxOrderContact.Text = map["customer_contact_name"].ToString();
                tbOrderContactEmail.Text = map["customer_contact_email"].ToString();
                tbOrderContactPhone.Text = map["customer_contact_phone"].ToString();

                tbOrderReportComment.Text = map["report_comment"].ToString();
                // TODO

                UI.PopulateOrderContent(conn, selectedOrderId, treeOrderContent, Guid.Empty, treeSampleTypes, true);

                gridOrderSamples.DataSource = DB.GetDataTable(
                    conn, 
                    "csp_select_samples_for_assignment_flat", 
                    CommandType.StoredProcedure, 
                    new SqlParameter("@assignment_id", id));

                gridOrderSamples.Columns["id"].Visible = false;

                gridOrderSamples.Columns["number"].HeaderText = "Sample";
                gridOrderSamples.Columns["external_id"].HeaderText = "Ex.Id";
                gridOrderSamples.Columns["laboratory_name"].HeaderText = "Laboratory";
                gridOrderSamples.Columns["sample_type_name"].HeaderText = "Sample type";
                gridOrderSamples.Columns["sample_component_name"].HeaderText = "Sample component";
                gridOrderSamples.Columns["project_name"].HeaderText = "Project";
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
            /*if(cboxOrderLaboratory.SelectedItem == null)
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (cboxOrderResponsible.SelectedItem == null)
            {
                MessageBox.Show("Responsible is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbOrderDeadline.Text))
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }

            if (cboxOrderRequestedSigma.SelectedIndex < 0)
            {
                MessageBox.Show("Requested sigma is mandatory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();                

                Lemma<Guid, string> lab = cboxOrderLaboratory.SelectedItem as Lemma<Guid, string>;
                string labPrefix = DB.GetOrderPrefix(conn, trans, lab.Id);
                int orderCount = DB.GetNextOrderCount(conn, trans, lab.Id);
                string orderName = labPrefix + "-" + DateTime.Now.ToString("yyyy") + "-" + orderCount;

                SqlCommand cmd = new SqlCommand("csp_insert_assignment", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                Guid newGuid = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", newGuid);
                cmd.Parameters.AddWithValue("@name", orderName);
                cmd.Parameters.AddWithValue("@laboratory_id", lab.Id);
                Lemma<string, string> account = cboxOrderResponsible.SelectedItem as Lemma<string, string>;
                cmd.Parameters.AddWithValue("@account_id", account.Id);
                cmd.Parameters.AddWithValue("@deadline", (DateTime)tbOrderDeadline.Tag);
                cmd.Parameters.AddWithValue("@requested_sigma", Convert.ToDouble(cboxOrderRequestedSigma.Text));
                cmd.Parameters.AddWithValue("@customer_name", cboxOrderCustomerName.Text);
                cmd.Parameters.AddWithValue("@customer_address", tbOrderCustomerAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@customer_email", tbOrderCustomerEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@customer_phone", tbOrderCustomerPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@customer_contact_name", cboxOrderContact.Text);                    
                cmd.Parameters.AddWithValue("@customer_contact_email", tbOrderContactEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@customer_contact_phone", tbOrderContactPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@approved_customer", 0);
                cmd.Parameters.AddWithValue("@approved_laboratory", 0);
                cmd.Parameters.AddWithValue("@content_comment", tbOrderContentComment.Text);
                cmd.Parameters.AddWithValue("@report_comment", tbOrderReportComment.Text);
                cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
                cmd.Parameters.AddWithValue("@closed_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                cmd.Parameters.AddWithValue("@locked_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                trans.Commit();

                selectedOrder = newGuid;                
                lblStatus.Text = StrUtils.makeStatusMessage("Order " + orderName + " created");
                tbOrderName.Text = orderName;

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
            }*/
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
                        UI.PopulateComboBoxes(conn, "csp_select_customers", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxOrderCustomerName);
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
                return;

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
                        UI.PopulateComboBoxes(conn, "csp_select_customers", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxOrderCustomerName);
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

        private void miCustomerContactNew_Click(object sender, EventArgs e)
        {
            // new customer contact
            if (gridCustomers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a customer first");
                return;
            }                

            DataGridViewRow row = gridCustomers.SelectedRows[0];
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());
            string cname = row.Cells["name"].Value.ToString();

            FormCustomerContact form = new FormCustomerContact(cid, cname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer contact " + form.ContactName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCustomerContacts(conn, cid, InstanceStatus.Deleted, gridCustomerContacts);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create customer contact failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCustomerContactEdit_Click(object sender, EventArgs e)
        {
            // edit customer contact            
            if (gridCustomers.SelectedRows.Count < 1 || gridCustomerContacts.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridCustomers.SelectedRows[0];
            Guid cid = Guid.Parse(row.Cells["id"].Value.ToString());
            string cname = row.Cells["name"].Value.ToString();

            row = gridCustomerContacts.SelectedRows[0];
            Guid ccid = Guid.Parse(row.Cells["id"].Value.ToString());

            FormCustomerContact form = new FormCustomerContact(cid, cname, ccid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer contact " + form.ContactName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCustomerContacts(conn, cid, InstanceStatus.Deleted, gridCustomerContacts);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update customer contact failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miCustomerContactDelete_Click(object sender, EventArgs e)
        {
            // delete customer contact
        }        

        private void cboxOrderCustomerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxOrderCustomerName.SelectedValue))
            {
                cboxOrderContact.DataSource = null;
                return;
            }

            Guid custId = Guid.Parse(cboxOrderCustomerName.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateComboBoxes(conn, "csp_select_customer_contacts_for_customer", new[] {
                    new SqlParameter("@customer_id", custId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxOrderContact);
            }
        }

        private void gridCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (gridCustomers.SelectedRows.Count < 1)
                return;

            Guid cid = Guid.Parse(gridCustomers.SelectedRows[0].Cells["id"].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateCustomerContacts(conn, cid, InstanceStatus.Deleted, gridCustomerContacts);
            }
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
            if (cboxOrderLaboratory.SelectedItem == null)
                return;

            Guid labId = Guid.Parse(cboxOrderLaboratory.SelectedValue.ToString());
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, labId, InstanceStatus.Active, cboxOrderResponsible);
            }
        }

        private void treePrepAnal_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch(e.Node.Level)
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
                cmd.Parameters.AddWithValue("@sampling_date_from", new DateTime(dtSampleSamplingDateFrom.Value.Year, dtSampleSamplingDateFrom.Value.Month, dtSampleSamplingDateFrom.Value.Day, dtSampleSamplingTimeFrom.Value.Hour, dtSampleSamplingTimeFrom.Value.Minute, dtSampleSamplingTimeFrom.Value.Second));
                cmd.Parameters.AddWithValue("@use_sampling_date_to", cbSampleUseSamplingTimeTo.Checked ? 1 : 0);
                cmd.Parameters.AddWithValue("@sampling_date_to", new DateTime(dtSampleSamplingDateTo.Value.Year, dtSampleSamplingDateTo.Value.Month, dtSampleSamplingDateTo.Value.Day, dtSampleSamplingTimeTo.Value.Hour, dtSampleSamplingTimeTo.Value.Minute, dtSampleSamplingTimeTo.Value.Second));
                cmd.Parameters.AddWithValue("@reference_date", new DateTime(dtSampleReferenceDate.Value.Year, dtSampleReferenceDate.Value.Month, dtSampleReferenceDate.Value.Day, dtSampleReferenceTime.Value.Hour, dtSampleReferenceTime.Value.Minute, dtSampleReferenceTime.Value.Second));
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

            double amount = Convert.ToDouble(tbPrepAnalPrepAmount.Text);
            double quantity = Convert.ToDouble(tbPrepAnalPrepQuantity.Text);
            double fillHeight = Convert.ToDouble(tbPrepAnalPrepFillHeight.Text);

            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_update_preparation", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", pid);
                cmd.Parameters.AddWithValue("@preparation_geometry_id", DB.MakeParam(typeof(Guid), cboxPrepAnalPrepGeom.SelectedValue));
                cmd.Parameters.AddWithValue("@workflow_status_id", cboxPrepAnalPrepWorkflowStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@prep_unit_id", cboxPrepAnalPrepAmountUnit.SelectedValue);
                cmd.Parameters.AddWithValue("@quantity", quantity);
                cmd.Parameters.AddWithValue("@quantity_unit_id", cboxPrepAnalPrepQuantityUnit.SelectedValue);
                cmd.Parameters.AddWithValue("@fill_height_mm", fillHeight);
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
                cmd.Parameters.AddWithValue("@sigma", DB.MakeParam(typeof(int), cboxPrepAnalAnalSigma.SelectedValue));
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
                    cboxPrepAnalAnalSigma.SelectedValue = reader["sigma"];
                    tbPrepAnalAnalSpecRef.Text = reader["specter_reference"].ToString();
                    tbPrepAnalAnalNuclLib.Text = reader["nuclide_library"].ToString();
                    tbPrepAnalAnalMDALib.Text = reader["mda_library"].ToString();
                    cboxPrepAnalAnalWorkflowStatus.SelectedValue = reader["workflow_status_id"];
                    tbPrepAnalAnalComment.Text = reader["comment"].ToString();
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

            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_update_sample_info", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", sid);
                cmd.Parameters.AddWithValue("@wet_weight_g", tbPrepAnalWetWeight.Text);
                cmd.Parameters.AddWithValue("@dry_weight_g", tbPrepAnalDryWeight.Text);
                cmd.Parameters.AddWithValue("@volume_l", tbPrepAnalVolume.Text);
                cmd.Parameters.AddWithValue("@lod_weight_start", tbPrepAnalLODStartWeight.Text);
                cmd.Parameters.AddWithValue("@lod_weight_end", tbPrepAnalLODEndWeight.Text);
                cmd.Parameters.AddWithValue("@lod_temperature", tbPrepAnalLODTemp.Text);
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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "LIS files (*.lis)|*.lis";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            AnalysisParameters analysisParameters = new AnalysisParameters();
            analysisParameters.FileName = dialog.FileName;
            analysisParameters.PreparationGeometry = cboxPrepAnalPrepGeom.Text;
            analysisParameters.SampleName = treePrepAnal.Nodes.Count > 0 ? treePrepAnal.Nodes[0].Text : "";
            analysisParameters.SpectrumReferenceRegEx = ""; // FIXME
            AnalysisResult analysisResult = new AnalysisResult();
            FormImportAnalysisLIS form = new FormImportAnalysisLIS(analysisParameters, analysisResult);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();                

                SqlCommand cmd = new SqlCommand(@"
update analysis set 
    specter_reference = @specter_reference,
    nuclide_library = @nuclide_library,
    mda_library = @mda_library,
    sigma = @sigma,    
    update_date = @update_date,
    updated_by = @updated_by
where id = @id
", connection, transaction);
                cmd.Parameters.AddWithValue("@id", aid);
                cmd.Parameters.AddWithValue("@specter_reference", analysisResult.SpectrumName);
                cmd.Parameters.AddWithValue("@nuclide_library", analysisResult.NuclideLibrary);
                cmd.Parameters.AddWithValue("@mda_library", analysisResult.DetLimLib);
                cmd.Parameters.AddWithValue("@sigma", analysisResult.Sigma);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
insert into analysis_result values(
    @id,
    @analysis_id,
    @nuclide_id,
    @activity,
    @activity_uncertainty,
    @activity_uncertainty_abs,
    @activity_approved,
    @uniform_activity,
    @uniform_activity_unit_id,
    @detection_limit, 
    @detection_limit_approved,
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
                    object o = DB.GetScalar(connection, transaction, "select id from nuclide where name = '" + iso.NuclideName + "'", CommandType.Text);
                    if(o == null || o == DBNull.Value)
                    {
                        Common.Log.Warn("Unregistered nuclide: " + iso.NuclideName);
                        continue;
                    }                    
                    cmd.Parameters.AddWithValue("@nuclide_id", Guid.Parse(o.ToString()));
                    cmd.Parameters.AddWithValue("@activity", iso.Activity);
                    cmd.Parameters.AddWithValue("@activity_uncertainty", iso.Uncertainty * 2.0);
                    cmd.Parameters.AddWithValue("@activity_uncertainty_abs", 0); // FIXME
                    cmd.Parameters.AddWithValue("@activity_approved", iso.ApprovedRES);                    
                    double uAct;
                    int uActUnitId;
                    Guid analUnitId = Guid.Parse(cboxPrepAnalAnalUnit.SelectedValue.ToString());
                    DB.GetUniformActivity(connection, transaction, iso.Activity, analUnitId, out uAct, out uActUnitId);
                    cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                    cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                    cmd.Parameters.AddWithValue("@detection_limit", iso.MDA);
                    cmd.Parameters.AddWithValue("@detection_limit_approved", iso.ApprovedMDA);
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
    }    
}
