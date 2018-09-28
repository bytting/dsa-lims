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
        private ILog log = null;
        private DSASettings settings = new DSASettings();        

        private ResourceManager r = null;
        private TabPage returnFromSample = null;        

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
                Common.Log = log = DSALogger.CreateLogger(tbLog);

                tabs.Appearance = TabAppearance.FlatButtons;
                tabs.ItemSize = new Size(0, 1);
                tabs.SizeMode = TabSizeMode.Fixed;
                tabs.SelectedTab = tabMenu;
                lblCurrentTab.Text = tabs.SelectedTab.Text;
                lblStatus.Text = "";
                btnSamplesPrintSampleLabel.Visible = true;
                btnSamplesPrintPrepLabel.Visible = false;
                lblSampleToolId.Text = "";
                lblSampleToolExId.Text = "";
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                lblSampleToolLaboratory.Text = "";

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

                    DB.LoadInstanceStatus(conn);
                    DB.LoadDecayTypes(conn);
                    DB.LoadPreparationUnits(conn);
                    DB.LoadUniformActivityUnits(conn);
                    DB.LoadWorkflowStatus(conn);
                    DB.LoadLocationTypes(conn);
                    DB.LoadSampleTypes(conn);
                    
                    UI.PopulatePreparationUnits(cboxSamplePrepUnit);
                    UI.PopulateWorkflowStatus(cboxSampleAnalWorkflowStatus);
                    UI.PopulateLocationTypes(cboxSampleInfoLocationTypes);
                    UI.PopulateActivityUnits(conn, gridMetaUnitsActivity, cboxSampleAnalUnit);
                    UI.PopulateSampleTypes(treeSampleTypes);
                    UI.PopulateSampleTypes(cboxSampleSampleType);
                    UI.PopulateProjects(conn, treeProjects, cboxSampleProject);
                    UI.PopulateLaboratories(conn, gridMetaLab);
                    UI.PopulateLaboratories(conn, cboxSampleLaboratory);
                    UI.PopulateUsers(conn, gridMetaUsers);
                    UI.PopulateNuclides(conn, gridSysNuclides);
                    UI.PopulateGeometries(conn, gridSysGeom);                    
                    UI.PopulateCounties(conn, gridSysCounty);
                    UI.PopulateCounties(conn, cboxSampleCounties);
                    UI.PopulateStations(conn, gridMetaStation);
                    UI.PopulateStations(conn, cboxSampleInfoStations);
                    UI.PopulateSampleStorage(conn, gridMetaSampleStorage);
                    UI.PopulateSamplers(conn, gridMetaSamplers);
                    UI.PopulateSamplers(conn, cboxSampleInfoSampler);
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
            ClearStatusMessage();

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

            TreeNode tnode = e.Node;

            try
            {
                lbSampleTypesComponents.Items.Clear();
                lbSampleTypesInheritedComponents.Items.Clear();

                SampleTypeModel st = tnode.Tag as SampleTypeModel;
                lbSampleTypesComponents.Items.AddRange(st.SampleComponents.ToArray());

                while (tnode.Parent != null)
                {
                    tnode = tnode.Parent;
                    if (tnode.Tag == null)
                        throw new Exception("Missing ID tag in treeSampleTypes_AfterSelect");

                    SampleTypeModel s = tnode.Tag as SampleTypeModel;
                    lbSampleTypesInheritedComponents.Items.AddRange(s.SampleComponents.ToArray());
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
            SetStatusMessage("Settings saved");
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
            FormLaboratory form = new FormLaboratory(log);
            switch(form.ShowDialog())
            {                
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.Laboratory.Name + " created");
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

            FormLaboratory form = new FormLaboratory(log, lid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.Laboratory.Name + " updated");
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
            FormProject form = new FormProject(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Main project " + form.MainProject.Name + " inserted");                    
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateProjects(conn, treeProjects, cboxSampleProject);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create main project failed", StatusMessageType.Error);
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
                    SetStatusMessage("Project " + form.MainProject.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateProjects(conn, treeProjects, cboxSampleProject);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create project failed", StatusMessageType.Error);
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
                    SetStatusMessage("Sub project " + form.SubProject.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateProjects(conn, treeProjects, cboxSampleProject);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sub project failed", StatusMessageType.Error);
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
                    SetStatusMessage("Sub project " + form.SubProject.Name + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateProjects(conn, treeProjects, cboxSampleProject);
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sub project failed", StatusMessageType.Error);
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
            FormNuclide form = new FormNuclide(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Nuclide " + form.Nuclide.Name + " inserted");
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

            FormNuclide form = new FormNuclide(log, nid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Nuclide " + form.Nuclide.Name + " updated");
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

            FormEnergyLine form = new FormEnergyLine(log, nid, nname);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Energy line for " + nname + " inserted");
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

            FormEnergyLine form = new FormEnergyLine(log, nid, eid, nname);
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
            FormGeometry form = new FormGeometry(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Geometry " + form.Geometry.Name + " inserted");
                    using (SqlConnection conn = DB.OpenConnection())
                        UI.PopulateGeometries(conn, gridSysGeom);
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

            FormGeometry form = new FormGeometry(log, gid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Geometry " + form.Geometry.Name + " updated");
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
            FormCounty form = new FormCounty(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.County.Name + " inserted");
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

            FormCounty form = new FormCounty(log, cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.County.Name + " updated");
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

            FormMunicipality form = new FormMunicipality(log, cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Municipality " + form.Municipality.Name + " inserted");
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

            FormMunicipality form = new FormMunicipality(log, cid, mid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Municipality " + form.Municipality.Name + " updated");
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

            Guid cid = new Guid(e.Row.Cells[0].Value.ToString());
            using (SqlConnection conn = DB.OpenConnection())            
                UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
        }

        private void miNewStation_Click(object sender, EventArgs e)
        {        
            FormStation form = new FormStation(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Station " + form.Station.Name + " inserted");
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

            FormStation form = new FormStation(log, sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Station " + form.Station.Name + " updated");
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
            FormSampleStorage form = new FormSampleStorage(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorage.Name + " inserted");
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

            FormSampleStorage form = new FormSampleStorage(log, ssid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorage.Name + " updated");
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
            FormSampler form = new FormSampler(log);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.Sampler.Name + " inserted");
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

            FormSampler form = new FormSampler(log, sid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sampler " + form.Sampler.Name + " updated");
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

            if (cboxSampleSampleType.SelectedItem == null)
            {
                cboxSampleSampleComponent.Items.Clear();
                return;
            }

            var st = cboxSampleSampleType.SelectedItem as SampleTypeModel;
            cboxSampleSampleComponent.Items.Clear();
            cboxSampleSampleComponent.Items.AddRange(st.SampleComponents.ToArray());

            TreeNode[] tnode = treeSampleTypes.Nodes.Find(st.Name, true);
            if(tnode != null && tnode.Length != 0)
            {
                TreeNode n = tnode[0];
                while (n.Parent != null)
                {
                    n = n.Parent;
                    SampleTypeModel s = n.Tag as SampleTypeModel;
                    cboxSampleSampleComponent.Items.AddRange(s.SampleComponents.ToArray());
                }
            }

            cboxSampleSampleComponent.SelectedIndex = -1;
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

            SampleTypeModel st = cboxSampleSampleType.SelectedItem as SampleTypeModel;
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

            cboxSampleSampleType.SelectedItem = form.SelectedSampleType;
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
                UI.PopulateSubProjects(conn, project.Id, cboxSampleSubProject);

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
    }    
}
