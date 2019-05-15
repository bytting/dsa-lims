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
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DSA_lims
{
    public partial class FormMain : Form
    {
        private bool initialized = false;
        private ResourceManager r = null;
        
        int statusMessageTimeout = 16000;
        System.Timers.Timer statusMessageTimer = null;

        private Sample sample = new Sample();
        private Assignment assignment = new Assignment();
        private Analysis analysis = new Analysis();
        private Preparation preparation = new Preparation();

        ToolTip ttCoords = new ToolTip();

        bool searchIsDirty = false;

        public FormMain()
        {
            InitializeComponent();
            
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

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
            lblCurrentUser.Text = "";
            lblSampleToolId.Text = "";
            lblSampleToolExId.Text = "";
            lblSampleToolProject.Text = "";
            lblSampleToolSubProject.Text = "";
            lblSampleToolLaboratory.Text = "";
            tbMenuLookup.Text = "";

            tbMenuLookup.KeyPress += CustomEvents.Integer_KeyPress;
            tbSamplesLookup.KeyPress += CustomEvents.Integer_KeyPress;
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
            tbSearchActMin.KeyPress += CustomEvents.Numeric_KeyPress;
            tbSearchActMax.KeyPress += CustomEvents.Numeric_KeyPress;
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

                Common.InstallationDirectory = Path.GetDirectoryName(Application.ExecutablePath);

                r = new ResourceManager("DSA_lims.lang_" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                Common.Log.Info("Setting language " + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                SetLanguageLabels(r);

                statusMessageTimer = new System.Timers.Timer(statusMessageTimeout);
                statusMessageTimer.SynchronizingObject = this;
                statusMessageTimer.Elapsed += StatusMessageTimer_Elapsed;
                statusMessageTimer.AutoReset = false;
                statusMessageTimer.Enabled = false;

                string NL = Environment.NewLine;
                ttCoords.SetToolTip(lblSampleCoords, "Latitude, Longitude" + NL + NL
                + "Formats: " + NL + "61° 34' 12\" N   11° 67' 20\" E" + NL + "61° 34" + Utils.NumberSeparator + "23' N   11° 67" + Utils.NumberSeparator + "33' E"
                + NL + "61" + Utils.NumberSeparator + "543478 N   11" + Utils.NumberSeparator + "776344 E" + NL + "61" + Utils.NumberSeparator + "543478   -11" + Utils.NumberSeparator + "776344" + NL + NL + "° can be replaced with *");

                // delete old temp files
                string[] oldFiles = Directory.GetFiles(Path.GetTempPath(), "*-dsalims.pdf", SearchOption.AllDirectories);
                foreach (string oldFile in oldFiles)
                {
                    if (File.Exists(oldFile))
                    {
                        try { File.Delete(oldFile); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                if(Common.Log != null)
                    Common.Log.Fatal(ex);
                MessageBox.Show(ex.Message);
                Environment.Exit(1);
            }                
        }

        private void StatusMessageTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lblStatus.Text = "";
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            try
            {
                HideMenuItems();
                ShowLogin();
                initialized = InitializeUI();
                Application.Idle += Application_Idle;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            if (tabs.SelectedTab == tabPrepAnal)
            {
                if (tabsPrepAnal.SelectedTab == tabPrepAnalAnalysis)
                {
                    if (analysis.IsDirty())
                    {
                        btnPrepAnalAnalUpdate.ForeColor = Color.Green;
                        btnPrepAnalAnalDiscard.ForeColor = Color.Red;
                    }
                    else
                    {
                        btnPrepAnalAnalUpdate.ForeColor = SystemColors.ControlText;
                        btnPrepAnalAnalDiscard.ForeColor = SystemColors.ControlText;
                    }
                }
                else if (tabsPrepAnal.SelectedTab == tabPrepAnalPreps)
                {
                    if (preparation.IsDirty())
                    {
                        btnPrepAnalPrepUpdate.ForeColor = Color.Green;
                        btnPrepAnalPrepDiscard.ForeColor = Color.Red;
                    }
                    else
                    {
                        btnPrepAnalPrepUpdate.ForeColor = SystemColors.ControlText;
                        btnPrepAnalPrepDiscard.ForeColor = SystemColors.ControlText;
                    }
                }
                else if (tabsPrepAnal.SelectedTab == tabPrepAnalSample)
                {
                    if(sample.IsDirty())
                    {
                        btnPrepAnalSampleUpdate.ForeColor = Color.Green;
                        btnPrepAnalSampleDiscard.ForeColor = Color.Red;
                    }
                    else
                    {
                        btnPrepAnalSampleUpdate.ForeColor = SystemColors.ControlText;
                        btnPrepAnalSampleDiscard.ForeColor = SystemColors.ControlText;
                    }
                }
            }
            else if (tabs.SelectedTab == tabOrder)
            {
                if (assignment.IsDirty())
                {
                    btnOrderSave.ForeColor = Color.Green;
                    btnOrderDiscard.ForeColor = Color.Red;
                }
                else
                {
                    btnOrderSave.ForeColor = SystemColors.ControlText;
                    btnOrderDiscard.ForeColor = SystemColors.ControlText;
                }
            }
            else if (tabs.SelectedTab == tabSample)
            {
                if (sample.IsDirty())
                {
                    btnSampleUpdate.ForeColor = Color.Green;
                    btnSampleDiscard.ForeColor = Color.Red;
                }
                else
                {
                    btnSampleUpdate.ForeColor = SystemColors.ControlText;
                    btnSampleDiscard.ForeColor = SystemColors.ControlText;
                }
            }
            else if (tabs.SelectedTab == tabSearch && tabsSearch.SelectedTab == tabSearchSearch)
            {
                if (searchIsDirty)                
                    btnSearchSearch.ForeColor = Color.Green;                
                else                
                    btnSearchSearch.ForeColor = SystemColors.ControlText;                
            }
        }

        private bool InitializeUI()
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.LoadSampleTypes(conn, null);

                cboxSamplesStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                cboxSampleInstanceStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_instance_status", false);

                cboxPrepAnalPrepAmountUnit.DataSource = DB.GetIntLemmata(conn, null, "csp_select_preparation_units", true);

                cboxPrepAnalPrepQuantityUnit.DataSource = DB.GetIntLemmata(conn, null, "csp_select_quantity_units", true);

                cboxPrepAnalAnalWorkflowStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_workflow_status", false);

                cboxPrepAnalPrepWorkflowStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_workflow_status", false);

                cboxOrderStatus.DataSource = DB.GetIntLemmata(conn, null, "csp_select_workflow_status", false);

                cboxSampleInfoLocationTypes.DataSource = DB.GetIntLemmata(conn, null, "csp_select_location_types", true);

                cboxOrderRequestedSigma.DataSource = DB.GetSigmaValues(conn, null, false);
                cboxOrderRequestedSigmaMDA.DataSource = DB.GetSigmaValues(conn, null, true);

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
                    }, cboxSamplesProjects, cboxSampleProject);

                UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridSysLab);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSampleLaboratory, cboxOrderLaboratory);

                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridSysUsers);

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

                UI.PopulateOrderYears(conn, cboxOrdersYear);

                UI.PopulateOrderWorkflowStatus(conn, cboxOrdersWorkflowStatus);

                UI.PopulateComboBoxes(conn, "csp_select_assignments_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSamplesOrders);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                    }, cboxSamplesLaboratory, cboxOrdersLaboratory);

                UI.PopulateSampleParameterNames(conn, gridSysSampParamNames);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                conn?.Close();
            }

            try
            {
                cboxOrdersTop.DataSource = DB.GetTopValues();
                cboxOrdersTop.SelectedValue = 50;

                cboxSamplesTop.DataSource = DB.GetTopValues();
                cboxSamplesTop.SelectedValue = 50;

                if (Common.LabId != Guid.Empty)
                {
                    cboxOrdersLaboratory.SelectedValue = Common.LabId;
                    cboxSamplesLaboratory.SelectedValue = Common.LabId;
                }                    
                
                cboxSamplesStatus.SelectedValue = InstanceStatus.Active;
                cboxOrdersWorkflowStatus.SelectedValue = WorkflowStatus.Construction;
                
                PopulateOrders();
                btnOrdersSearch.ForeColor = SystemColors.ControlText;

                PopulateSamples();
                btnSamplesSearch.ForeColor = SystemColors.ControlText;

                panelSampleLatLonAlt_Resize(null, null);

                HideMenuItems();
                SetMenuItemVisibilities();

                ActiveControl = tbMenuLookup;
                
                Common.Log.Info("Application initialized successfully");
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }

        private bool DiscardUnsavedChanges()
        {
            if (tabs.SelectedTab == tabPrepAnal)
            {
                if (sample.IsDirty())
                {
                    DialogResult r = MessageBox.Show("Changes to the current sample will be discarded. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (r == DialogResult.No)
                        return false;
                }

                if (preparation.IsDirty())
                {
                    DialogResult r = MessageBox.Show("Changes to the current preparation will be discarded. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (r == DialogResult.No)
                        return false;
                }

                if (analysis.IsDirty())
                {
                    DialogResult r = MessageBox.Show("Changes to the current analysis will be discarded. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (r == DialogResult.No)
                        return false;
                }                
                
                sample.ClearDirty();
                preparation.ClearDirty();
                analysis.ClearDirty();
            }
            else if (tabs.SelectedTab == tabOrder)
            {
                if (assignment.IsDirty())
                {
                    DialogResult r = MessageBox.Show("Changes to the current assignment will be discarded. Do you want to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (r == DialogResult.No)
                        return false;
                }

                assignment.ClearDirty();
            }

            return true;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {            
            try
            {
                if(!DiscardUnsavedChanges())
                {
                    e.Cancel = true;
                    return;
                }

                Common.Log.Info("Application closing down");

                if (initialized)
                {
                    SqlConnection conn = null;
                    try
                    {
                        conn = DB.OpenConnection();
                        DB.UnlockSamples(conn);
                        DB.UnlockOrders(conn);
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

                SaveSettings(DSAEnvironment.SettingsFilename);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Common.Log.Error(ex);
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
            statusMessageTimer.Stop();

            switch (msgLevel)
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
            
            statusMessageTimer.Start();
        }

        private void HideMenuItems()
        {
            miSample.Visible = miOrder.Visible = miSearch.Visible = miMeta.Visible = miOrders.Visible 
                = miSamples.Visible = miProjects.Visible = miCustomers.Visible = miTypeRelations.Visible = miSys.Visible = false;
        }

        private void HideMetaMenuItems()
        {
            miStations.Visible = false;
            miSampleStorage.Visible = false;
            miUnits.Visible = false;
            miSamplers.Visible = false;
            miSamplingMethods.Visible = false;
            miCompanies.Visible = false;
            miCustomers.Visible = false;           
        }

        private void HideSysMenuItems()
        {
            miLaboratories.Visible = false;
            miUsers.Visible = false;
            miNuclides.Visible = false;
            miMunicipalities.Visible = false;
            miAccreditationRules.Visible = false;
            miGeometries.Visible = false;
            miPersonalia.Visible = false;
        }        

        private void miLogout_Click(object sender, EventArgs e)
        {
            if (!DiscardUnsavedChanges())            
                return;

            try
            {
                HideMenuItems();
                ShowLogin();
                initialized = InitializeUI();
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ShowLogin()
        {
            if (tabs.SelectedTab != tabMenu)
                tabs.SelectedTab = tabMenu;

            Common.UserId = Guid.Empty;
            Common.Username = String.Empty;
            Common.LabId = Guid.Empty;
            Common.LabLogo = null;
            Common.LabAccredLogo = null;
            Roles.UserRoles.Clear();

            lblCurrentUser.Text = "";

            FormLogin formLogin = new FormLogin(Common.Settings);
            if (formLogin.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
            }

            Common.UserId = formLogin.UserId;
            Common.Username = formLogin.UserName;
            Common.LabId = formLogin.LabId;

            lblCurrentUser.Text = Common.Username;

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                DB.LoadUserRoles(conn, trans, Common.UserId, ref Roles.UserRoles);

                if (Common.LabId != Guid.Empty)
                {
                    using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select laboratory_logo, accredited_logo from laboratory where id = @id",
                        CommandType.Text, new SqlParameter("@id", Common.LabId)))
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            if (!reader.IsDBNull(0))
                                Common.LabLogo = Image.FromStream(new MemoryStream((byte[])reader["laboratory_logo"]));
                            if (!reader.IsDBNull(1))
                                Common.LabAccredLogo = Image.FromStream(new MemoryStream((byte[])reader["accredited_logo"]));
                        }
                    }
                }

                trans.Commit();     
            }
            catch(Exception ex)
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
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
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
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }        

        private void SetLanguageLabels(ResourceManager r)
        {
            lblMenuSamples.Text = r.GetString(lblMenuSamples.Name);
            lblMenuOrders.Text = r.GetString(lblMenuOrders.Name);
            btnMenuNewSample.Text = r.GetString(btnMenuNewSample.Name);
            miFile.Text = r.GetString(miFile.Name);
        }        

        private void SetMenuItemVisibilities()
        {
            bool isAdmin = Roles.IsAdmin();
            miSearchView.Enabled = miProjectsView.Enabled = miCustomersView.Enabled = miTypeRelationsView.Enabled = miMetadataView.Enabled = miSystemDataView.Enabled = miAuditLogView.Enabled = isAdmin;
            btnMenuNewSample.Enabled = btnMenuSamples.Enabled = btnMenuNewOrder.Enabled = btnOrders.Enabled = btnMenuCustomer.Enabled = btnMenuProjects.Enabled = btnMenuMetadata.Enabled = btnMenuSearch.Enabled = isAdmin;
            tbMenuLookup.Enabled = isAdmin;

            miTypeRelSampleTypesNewRoot.Enabled = btnTypeRelSampleTypesNewRoot.Enabled = miTypeRelSampleTypesNew.Enabled = btnTypeRelSampleTypesNew.Enabled = miTypeRelSampleTypesEdit.Enabled = btnTypeRelSampleTypesEdit.Enabled = miTypeRelSampleTypesDelete.Enabled = btnTypeRelSampleTypesDelete.Enabled = isAdmin;
            miNewLaboratory.Enabled = miEditLaboratory.Enabled = miDeleteLaboratory.Enabled = btnSysLabNew.Enabled = btnSysLabEdit.Enabled = btnSysLabDelete.Enabled = isAdmin;
            miNewUser.Enabled = miEditUser.Enabled = miDeleteUser.Enabled = btnMetaUsersNew.Enabled = btnMetaUsersEdit.Enabled = btnMetaUsersDelete.Enabled = isAdmin;
            miNewCounty.Enabled = miEditCounty.Enabled = miDeleteCounty.Enabled = miNewMunicipality.Enabled = miEditMunicipality.Enabled = miDeleteMunicipality.Enabled = isAdmin;
            btnNewCounty.Enabled = btnEditCounty.Enabled = btnDeleteCounty.Enabled = btnNewMunicipality.Enabled = btnEditMunicipality.Enabled = btnDeleteMunicipality.Enabled = isAdmin;
            miTypeRelSampleTypesCompNew.Enabled = miTypeRelSampleTypesCompEdit.Enabled = btnTypeRelSampTypeCompAdd.Enabled = btnTypeRelSampTypeCompEdit.Enabled = btnTypeRelSampTypeCompDelete.Enabled = isAdmin;
            miPreparationMethodsNew.Enabled = miPreparationMethodEdit.Enabled = miPreparationMethodDelete.Enabled = btnTypeRelSampTypePrepMethAdd.Enabled = isAdmin;
            miSamplesSetOrder.Enabled = btnSamplesSetOrder.Enabled = btnSampleAddSampleToOrder.Enabled = isAdmin;
            miSamplesPrepAnal.Enabled = btnSamplesPrepAnal.Enabled = btnSampleGoToPrepAnal.Enabled = isAdmin;
            miSamplesUnlock.Enabled = btnSamplesUnlock.Visible = isAdmin;
            miOrdersUnlock.Enabled = btnOrdersUnlock.Visible = isAdmin;
            btnSysLabPrepMethAdd.Enabled = btnSysLabPrepMethRemove.Enabled = btnSysLabAnalMethAdd.Enabled = btnSysLabAnalMethRemove.Enabled = false;
            miNewGeometry.Enabled = btnNewGeometry.Enabled = miEditGeometry.Enabled = btnEditGeometry.Enabled = miDeleteGeometry.Enabled = btnDeleteGeometry.Enabled = isAdmin;
            miNuclidesNew.Enabled = btnSysNuclideNew.Enabled = miNuclidesEdit.Enabled = btnSysNuclideEdit.Enabled = miNuclidesDelete.Enabled = btnSysNuclideDelete.Enabled = isAdmin;

            btnSysUsersAddRoles.Enabled = btnSysUsersRemRoles.Enabled = btnSysUsersAnalMethAdd.Enabled = btnSysUsersAnalMethRemove.Enabled = isAdmin;

            cboxOrderStatus.Enabled = cbOrderApprovedLaboratory.Enabled = Roles.HasAccess(Role.LaboratoryAdministrator);

            btnSysSampParamNameNew.Enabled = btnSysSampParamNameEdit.Enabled = btnSysSampParamNameDelete.Enabled = isAdmin;

            // FIXME: Accreditation rules

            if (Roles.HasAccess(Role.LaboratoryAdministrator) && Common.LabId != Guid.Empty)
            {                
                miSearchView.Enabled = miProjectsView.Enabled = miCustomersView.Enabled = miTypeRelationsView.Enabled = miMetadataView.Enabled = miSystemDataView.Enabled = miAuditLogView.Enabled = true;
                btnMenuNewSample.Enabled = btnMenuSamples.Enabled = btnMenuNewOrder.Enabled = btnOrders.Enabled = btnMenuCustomer.Enabled = btnMenuProjects.Enabled = btnMenuMetadata.Enabled = btnMenuSearch.Enabled = true;
                tbMenuLookup.Enabled = true;
                miPreparationMethodsNew.Enabled = miPreparationMethodEdit.Enabled = miPreparationMethodDelete.Enabled = btnTypeRelSampTypePrepMethAdd.Enabled = true;
                miSamplesSetOrder.Enabled = btnSamplesSetOrder.Enabled = btnSampleAddSampleToOrder.Enabled = true;
                miSamplesPrepAnal.Enabled = btnSamplesPrepAnal.Enabled = btnSampleGoToPrepAnal.Enabled = true;
                btnSysLabPrepMethAdd.Enabled = btnSysLabPrepMethRemove.Enabled = btnSysLabAnalMethAdd.Enabled = btnSysLabAnalMethRemove.Enabled = true;
            }

            if (Roles.HasAccess(Role.LaboratoryOperator) && Common.LabId != Guid.Empty)
            {
                miMetadataView.Visible = true;
                btnMenuSamples.Enabled = btnMenuNewSample.Enabled = true;
                tbMenuLookup.Enabled = true;
                miSearchView.Enabled = btnMenuSearch.Enabled = true;
                miSamplesSetOrder.Enabled = btnSamplesSetOrder.Enabled = btnSampleAddSampleToOrder.Enabled = true;
                miSamplesPrepAnal.Enabled = btnSamplesPrepAnal.Enabled = btnSampleGoToPrepAnal.Enabled = true;
            }

            if (Roles.HasAccess(Role.SampleRegistrator))
            {
                btnMenuSamples.Enabled = btnMenuNewSample.Enabled = true;
                tbMenuLookup.Enabled = true;
            }

            if (Roles.HasAccess(Role.OrderAdministrator))
            {
                btnMenuNewOrder.Enabled = btnOrders.Enabled = true;
            }

            if (Roles.HasAccess(Role.OrderOperator))
            {
                btnMenuNewOrder.Enabled = btnOrders.Enabled = true;
            }

            if (Roles.HasAccess(Role.Spectator))
            {
                miSearch.Enabled = true;
                btnMenuSearch.Enabled = true;
            }
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

            lblTypeRelSampCompSel.Text = lblTypeRelSampPrepSel.Text = tnode.Text;
            
            lbSampleTypesComponents.Items.Clear();
            lbSampleTypesInheritedComponents.Items.Clear();

            SqlConnection conn = null;            
            try
            {
                conn = DB.OpenConnection();            

                Guid sampleTypeId = Guid.Parse(tnode.Name);

                // add sample components
                AddSampleTypeComponents(conn, null, sampleTypeId, false, tnode);

                // add preparation methods
                UI.PopulateSampleTypePrepMeth(conn, null, tnode, lbTypeRelSampTypePrepMeth, lbTypeRelSampTypeInheritedPrepMeth);
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

        private void AddSampleTypeComponents(SqlConnection conn, SqlTransaction trans, Guid sampleTypeId, bool inherited, TreeNode tnode)
        {
            ListBox lb = inherited ? lbSampleTypesInheritedComponents : lbSampleTypesComponents;

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = Guid.Parse(tnode.Parent.Name);
                AddSampleTypeComponents(conn, trans, parentId, true, tnode.Parent);
            }
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
                        UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridSysLab);

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

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                
                if (tabsMeta.SelectedTab == tabMetaStations)
                {
                    miStations.Visible = true;
                    UI.PopulateStations(conn, gridMetaStation);
                }
                else if (tabsMeta.SelectedTab == tabMetaSampleStorage)
                {
                    miSampleStorage.Visible = true;
                    UI.PopulateSampleStorage(conn, gridMetaSampleStorage);
                }
                else if (tabsMeta.SelectedTab == tabMetaUnits)
                {
                    // FIXME                
                }
                else if (tabsMeta.SelectedTab == tabMetaSamplers)
                {
                    miSamplers.Visible = true;
                    UI.PopulateSamplers(conn, gridMetaSamplers);
                }
                else if (tabsMeta.SelectedTab == tabMetaSamplingMeth)
                {
                    miSamplingMethods.Visible = true;
                    UI.PopulateSamplingMethods(conn, gridMetaSamplingMeth);
                }
                else if (tabsMeta.SelectedTab == tabMetaCompanies)
                {
                    miCompanies.Visible = true;
                    UI.PopulateCompanies(conn, gridMetaCompanies);
                }
                else if (tabsMeta.SelectedTab == tabCustomers)
                {
                    miCustomers.Visible = true;
                    UI.PopulateCustomers(conn, InstanceStatus.Deleted, gridCustomers);
                }                
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

        private void tabsSys_SelectedIndexChanged(object sender, EventArgs e)
        {            
            HideSysMenuItems();

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                
                if (tabsSys.SelectedTab == tabSysLaboratories)
                {
                    miLaboratories.Visible = true;
                    UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridSysLab);
                }
                else if (tabsSys.SelectedTab == tabSysUsers)
                {
                    miUsers.Visible = true;
                    UI.PopulateUsers(conn, InstanceStatus.Deleted, gridSysUsers);
                }
                else if (tabsSys.SelectedTab == tabSysMunicipalities)
                {
                    miMunicipalities.Visible = true;
                    UI.PopulateCounties(conn, gridSysCounty);
                }
                else if (tabsSys.SelectedTab == tabSysNuclides)
                {
                    miNuclides.Visible = true;
                    UI.PopulateNuclides(conn, gridSysNuclides);
                }
                else if (tabsSys.SelectedTab == tabSysGeometries)
                {
                    miGeometries.Visible = true;
                    UI.PopulateGeometries(conn, gridSysGeom);
                }
                else if (tabsSys.SelectedTab == tabSysPers)
                {
                    miPersonalia.Visible = false;
                    UI.PopulatePersons(conn, gridSysPers);
                }             
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

        private void miNewUser_Click(object sender, EventArgs e)
        {
            // New user

            if(!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create laboratory users");
                return;
            }

            FormUser form = new FormUser();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridSysUsers);
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

            SetStatusMessage("Added username " + form.UserName);
        }

        private void miEditUser_Click(object sender, EventArgs e)
        {
            // edit user

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit laboratory users");
                return;
            }

            if (gridSysUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid uid = Guid.Parse(gridSysUsers.SelectedRows[0].Cells["id"].Value.ToString());

            FormUser form = new FormUser(uid);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateUsers(conn, InstanceStatus.Deleted, gridSysUsers);
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

            SetStatusMessage("Edited username " + form.UserName);
        }        

        private void miDeleteUser_Click(object sender, EventArgs e)
        {
            // Delete user
        }

        private void miResetPass_Click(object sender, EventArgs e)
        {
            if(gridSysUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid userId = Utils.MakeGuid(gridSysUsers.SelectedRows[0].Cells["id"].Value);
            string username = gridSysUsers.SelectedRows[0].Cells["username"].Value.ToString();
            FormResetPassword form = new FormResetPassword(userId, username);
            if(form.ShowDialog() == DialogResult.OK)
            {
                SetStatusMessage("Password updated for user " + username, StatusMessageType.Success);
            }
        }

        private void miEditLaboratory_Click(object sender, EventArgs e)
        {
            // edit lab
            if (gridSysLab.SelectedRows.Count < 1)
                return;
            
            Guid lid = Utils.MakeGuid(gridSysLab.SelectedRows[0].Cells["id"].Value);

            FormLaboratory form = new FormLaboratory(lid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Laboratory " + form.LaboratoryName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {                        
                        UI.PopulateLaboratories(conn, InstanceStatus.Deleted, gridSysLab);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create projects");
                return;
            }

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
                        }, cboxSamplesProjects, cboxSampleProject);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create main project failed", StatusMessageType.Error);
                    break;
            }                        
        }        

        private void miProjectsEdit_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit projects");
                return;
            }

            if (gridProjectMain.SelectedRows.Count < 1)
                return;

            Guid pmid = Utils.MakeGuid(gridProjectMain.SelectedRows[0].Cells["id"].Value);

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
                        }, cboxSamplesProjects, cboxSampleProject);
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to delete projects");
                return;
            }
        }

        private void miProjectsSubNew_Click(object sender, EventArgs e)
        {
            // new sub project

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create projects");
                return;
            }

            if (gridProjectMain.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a main project first");
                return;
            }

            Guid pmid = Utils.MakeGuid(gridProjectMain.SelectedRows[0].Cells["id"].Value);
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit projects");
                return;
            }

            if (gridProjectMain.SelectedRows.Count < 1)
                return;

            if (gridProjectSub.SelectedRows.Count < 1)
                return;

            Guid pmid = Utils.MakeGuid(gridProjectMain.SelectedRows[0].Cells["id"].Value);
            string pmname = gridProjectMain.SelectedRows[0].Cells["name"].Value.ToString();
            Guid psid = Utils.MakeGuid(gridProjectSub.SelectedRows[0].Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to delete projects");
                return;
            }
        }

        private void miNuclidesNew_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create nuclides");
                return;
            }

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

            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

            FormSampleType form = new FormSampleType(treeSampleTypes, treeSampleTypes.SelectedNode, false);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        DB.LoadSampleTypes(conn, null);
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

            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

            if (treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a parent sample type");
                return;
            }

            FormSampleType form = new FormSampleType(treeSampleTypes, treeSampleTypes.SelectedNode, true);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        DB.LoadSampleTypes(conn, null);
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

            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }
        }

        private void miSystemDataView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabSysdata;
        }

        private void miLogView_Click(object sender, EventArgs e)
        {
            try
            {
                string tempFileName = Path.GetTempFileName() + ".txt";
                File.Copy(DSALogger.LogFile, tempFileName, true);
                Process.Start(tempFileName);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);                
            }
        }

        private void miAuditLogView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabAuditLog;
        }

        private void miSearchView_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleTypes(treeSampleTypes, cboxSearchSampleType);
                UI.PopulateComboBoxes(conn, "csp_select_projects_main_short", new[] {
                        new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSearchProject);
                UI.PopulateComboBoxes(conn, "csp_select_stations_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSearchStations);
                UI.PopulateNuclides(conn, cboxSearchNuclides);
                List<Lemma<int, string>> maxShown = new List<Lemma<int, string>>();
                maxShown.Add(new Lemma<int, string>(0, ""));
                maxShown.Add(new Lemma<int, string>(100, "100"));
                maxShown.Add(new Lemma<int, string>(1000, "1000"));
                maxShown.Add(new Lemma<int, string>(10000, "10000"));
                cboxSearchMaxShown.DataSource = maxShown;
                cboxSearchMaxShown.DisplayMember = "Name";
                cboxSearchMaxShown.ValueMember = "Id";
                cboxSearchMaxShown.SelectedValue = 1000;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

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
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateProjectsMain(conn, gridProjectMain);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabProjects;
        }

        private void miMainMenuView_Click(object sender, EventArgs e)
        {
            tabs.SelectedTab = tabMenu;
        }

        private void miNuclidesEdit_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit nuclides");
                return;
            }

            if (gridSysNuclides.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysNuclides.SelectedRows[0];            
            Guid nid = Utils.MakeGuid(row.Cells["id"].Value);

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create preparation geometries");
                return;
            }

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit preparation geometries");
                return;
            }

            if (gridSysGeom.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridSysGeom.SelectedRows[0];
            Guid gid = Utils.MakeGuid(row.Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to delete preparation geometries");
                return;
            }
        }

        private void miNewCounty_Click(object sender, EventArgs e)
        {            
            FormCounty form = new FormCounty();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.CountyName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCounties(conn, gridSysCounty);

                        UI.PopulateComboBoxes(conn, "csp_select_counties_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                        }, cboxSampleCounties);
                    }
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
            Guid cid = Utils.MakeGuid(row.Cells["id"].Value);

            FormCounty form = new FormCounty(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("County " + form.CountyName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateCounties(conn, gridSysCounty);

                        UI.PopulateComboBoxes(conn, "csp_select_counties_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxSampleCounties);
                    }
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
            Guid cid = Utils.MakeGuid(row.Cells["id"].Value);
            string cName = row.Cells["name"].Value.ToString();

            FormMunicipality form = new FormMunicipality(cid, cName);
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
            
            Guid cid = Utils.MakeGuid(gridSysCounty.SelectedRows[0].Cells["id"].Value);
            string cName = gridSysCounty.SelectedRows[0].Cells["name"].Value.ToString();
            Guid mid = Utils.MakeGuid(gridSysMunicipality.SelectedRows[0].Cells["id"].Value);            

            FormMunicipality form = new FormMunicipality(cid, mid, cName);
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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to create stations");
                return;
            }

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to edit stations");
                return;
            }

            if (gridMetaStation.SelectedRows.Count < 1)
                return;
            
            Guid sid = Utils.MakeGuid(gridMetaStation.SelectedRows[0].Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to delete stations");
                return;
            }
        }

        private void miNewSampleStorage_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create sample storage");
                return;
            }

            FormSampleStorage form = new FormSampleStorage();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorageName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSampleStorage(conn, gridMetaSampleStorage);

                        UI.PopulateComboBoxes(conn, "csp_select_sample_storages_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxSampleSampleStorage);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sample storage failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miEditSampleStorage_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit sample storage");
                return;
            }

            if (gridMetaSampleStorage.SelectedRows.Count < 1)
                return;
            
            Guid ssid = Utils.MakeGuid(gridMetaSampleStorage.SelectedRows[0].Cells["id"].Value);

            FormSampleStorage form = new FormSampleStorage(ssid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample storage " + form.SampleStorageName + " updated");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        UI.PopulateSampleStorage(conn, gridMetaSampleStorage);

                        UI.PopulateComboBoxes(conn, "csp_select_sample_storages_short", new[] {
                            new SqlParameter("@instance_status_level", InstanceStatus.Active)
                        }, cboxSampleSampleStorage);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Update sample storage failed", StatusMessageType.Error);
                    break;
            }                        
        }

        private void miDeleteSampleStorage_Click(object sender, EventArgs e)
        {
            // delete sample storage

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create delete storage");
                return;
            }
        }

        private void miSamplerNew_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to create samplers");
                return;
            }

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to edit samplers");
                return;
            }

            if (gridMetaSamplers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a sampler first");
                return;
            }
            
            Guid sid = Utils.MakeGuid(gridMetaSamplers.SelectedRows[0].Cells["id"].Value);

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to delete samplers");
                return;
            }
        }

        private void cboxSampleSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleSampleType.SelectedValue))
            {
                cboxSampleSampleComponent.DataSource = null;
                return;
            }

            Guid sampleTypeId = Utils.MakeGuid(cboxSampleSampleType.SelectedValue);
            TreeNode[] tnodes = treeSampleTypes.Nodes.Find(sampleTypeId.ToString(), true);
            if (tnodes.Length < 1)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleComponentsAscending(conn, sampleTypeId, tnodes[0], cboxSampleSampleComponent);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
        }        

        private void cboxSampleSampleType_Leave(object sender, EventArgs e)
        {
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
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleProject.SelectedValue))
            {
                lblSampleToolProject.Text = "";
                lblSampleToolSubProject.Text = "";
                cboxSampleSubProject.DataSource = null;
                return;
            }

            Guid projectId = Utils.MakeGuid(cboxSampleProject.SelectedValue);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSampleSubProject);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            lblSampleToolProject.Text = "[Project] " + cboxSampleProject.Text;
            lblSampleToolSubProject.Text = "";
        }

        private void cboxSampleSubProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleSubProject.SelectedValue))
            {
                lblSampleToolSubProject.Text = "";
                return;
            }

            lblSampleToolSubProject.Text = cboxSampleSubProject.Text;
        }

        private void cboxSampleInfoStations_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleInfoStations.SelectedValue))
            {
                tbSampleInfoLatitude.Enabled = true;
                tbSampleInfoLatitude.Text = "";
                tbSampleInfoLongitude.Enabled = true;
                tbSampleInfoLongitude.Text = "";
                tbSampleInfoAltitude.Enabled = true;
                tbSampleInfoAltitude.Text = "";
                btnSampleSelectCoords.Enabled = true;
                return;
            }

            tbSampleInfoLatitude.Enabled = false;
            tbSampleInfoLongitude.Enabled = false;
            tbSampleInfoAltitude.Enabled = false;
            btnSampleSelectCoords.Enabled = false;

            Guid stationId = Utils.MakeGuid(cboxSampleInfoStations.SelectedValue);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_station", CommandType.StoredProcedure, 
                    new SqlParameter("@id", stationId)))
                {
                    reader.Read();

                    tbSampleInfoLatitude.Text = reader.GetString("latitude");
                    tbSampleInfoLongitude.Text = reader.GetString("longitude");
                    tbSampleInfoAltitude.Text = reader.GetString("altitude");
                }
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

        private void tbSampleExId_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (String.IsNullOrEmpty(tbSampleExId.Text.Trim()))
            {
                lblSampleToolExId.Text = "";
                return;
            }

            lblSampleToolExId.Text = "[Ex.Id] " + tbSampleExId.Text.Trim();
        }

        private void cboxSampleCounties_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleCounties.SelectedValue))
            {
                cboxSampleMunicipalities.DataSource = null;
                return;
            }

            Guid countyId = Utils.MakeGuid(cboxSampleCounties.SelectedValue);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_municipalities_for_county_short", new[] {
                    new SqlParameter("@county_id", countyId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxSampleMunicipalities);
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

        private void miSamplesNew_Click(object sender, EventArgs e)
        {
            // new sample                        
            FormSampleNew form = new FormSampleNew(treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                sample.LoadFromDB(conn, null, form.SampleId);
                PopulateSample(conn, null, sample, true);
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

            tabs.SelectedTab = tabSample;
            tabsSample.SelectedTab = tabSamplesInfo;

            btnSamplesSearch.ForeColor = Color.Red;

            SetStatusMessage("Sample " + form.SampleNumber + " created successfully");
        }

        private void miSamplesImportExcel_Click(object sender, EventArgs e)
        {
            // Import sample from excel
        }

        private void miSamplesEdit_Click(object sender, EventArgs e)
        {
            // edit sample

            if(gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample first");
                return;
            }

            SqlConnection conn = null;

            try
            {
                Guid sid = Utils.MakeGuid(gridSamples.SelectedRows[0].Cells["id"].Value);

                conn = DB.OpenConnection();

                sample.LoadFromDB(conn, null, sid);

                if (Common.LabId == Guid.Empty)
                {
                    if (Common.UserId != sample.CreateId)
                    {
                        MessageBox.Show("Can not edit this sample. Sample does not belong to your user");
                        return;
                    }
                }
                else
                {
                    bool allow = false;

                    if (Common.LabId == sample.LaboratoryId)
                    {
                        allow = true;
                    }
                    else
                    {
                        if (Common.UserId == sample.CreateId)
                            allow = true;
                    }

                    if (!allow)
                    {
                        MessageBox.Show("Can not edit this sample. Sample does not belong to you or your laboratory");
                        return;
                    }
                }

                PopulateSample(conn, null, sample, true);
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
            
            tabs.SelectedTab = tabSample;
            tabsSample.SelectedTab = tabSamplesInfo;
        }

        private void PopulateSample(SqlConnection conn, SqlTransaction trans, Sample s, bool clearDirty)
        {            
            cboxSampleSampleType.SelectedValue = s.SampleTypeId;
            cboxSampleSampleComponent.SelectedValue = s.SampleComponentId;
            cboxSampleInfoSampler.SelectedValue = s.SamplerId;
            cboxSampleInfoSamplingMeth.SelectedValue = s.SamplingMethodId;

            if (Utils.IsValidGuid(s.ProjectSubId))
            {
                object mpid = DB.GetScalar(conn, null, "select project_main_id from project_sub where id = @id", CommandType.Text,
                    new SqlParameter("@id", s.ProjectSubId));
                cboxSampleProject.SelectedValue = mpid;
                cboxSampleSubProject.SelectedValue = s.ProjectSubId;
            }

            if (!Utils.IsValidGuid(Common.LabId))
            {
                cboxSampleProject.Enabled = false;
                cboxSampleSubProject.Enabled = false;
            }
            else
            {
                cboxSampleProject.Enabled = true;
                cboxSampleSubProject.Enabled = true;
            }

            if (!Utils.IsValidGuid(s.StationId))
            {
                cboxSampleInfoStations.SelectedValue = Guid.Empty;
                tbSampleInfoLatitude.Text = s.Latitude.ToString();
                tbSampleInfoLongitude.Text = s.Longitude.ToString();
                tbSampleInfoAltitude.Text = s.Altitude.ToString();
            }
            else cboxSampleInfoStations.SelectedValue = s.StationId;

            if (Utils.IsValidGuid(s.MunicipalityId))
            {
                object cid = DB.GetScalar(conn, null, "select county_id from municipality where id = @id", CommandType.Text,
                    new SqlParameter("@id", s.MunicipalityId));
                cboxSampleCounties.SelectedValue = cid;
                cboxSampleMunicipalities.SelectedValue = s.MunicipalityId;
            }
            else
            {
                cboxSampleCounties.SelectedValue = Guid.Empty;
            }

            cboxSampleInfoLocationTypes.Text = s.LocationType;
            tbSampleLocation.Text = s.Location;

            cboxSampleLaboratory.SelectedValue = s.LaboratoryId;

            tbSampleSamplingDateFrom.TextChanged -= tbSampleSamplingDateFrom_TextChanged;
            tbSampleSamplingDateTo.TextChanged -= tbSampleSamplingDateTo_TextChanged;
            tbSampleReferenceDate.TextChanged -= tbSampleReferenceDate_TextChanged;

            if (!s.SamplingDateFrom.HasValue)
            {
                tbSampleSamplingDateFrom.Tag = null;
                tbSampleSamplingDateFrom.Text = "";
                tbSampleSamplingDateTo.Enabled = false;
                btnSampleSamplingDateTo.Enabled = false;
                btnSampleSamplingDateToClear.Enabled = false;
            }
            else
            {
                tbSampleSamplingDateFrom.Tag = s.SamplingDateFrom.Value;
                tbSampleSamplingDateFrom.Text = s.SamplingDateFrom.Value.ToString(Utils.DateTimeFormatNorwegian);
                tbSampleSamplingDateTo.Enabled = true;
                btnSampleSamplingDateTo.Enabled = true;
                btnSampleSamplingDateToClear.Enabled = true;
            }

            if (!s.SamplingDateTo.HasValue)
            {
                tbSampleSamplingDateTo.Tag = null;
                tbSampleSamplingDateTo.Text = "";
            }
            else
            {
                tbSampleSamplingDateTo.Tag = s.SamplingDateTo.Value;
                tbSampleSamplingDateTo.Text = s.SamplingDateTo.Value.ToString(Utils.DateTimeFormatNorwegian);
            }

            if (!s.ReferenceDate.HasValue)
            {
                tbSampleReferenceDate.Tag = null;
                tbSampleReferenceDate.Text = "";
            }
            else
            {
                tbSampleReferenceDate.Tag = s.ReferenceDate.Value;
                tbSampleReferenceDate.Text = s.ReferenceDate.Value.ToString(Utils.DateTimeFormatNorwegian);
            }

            tbSampleSamplingDateFrom.TextChanged += tbSampleSamplingDateFrom_TextChanged;
            tbSampleSamplingDateTo.TextChanged += tbSampleSamplingDateTo_TextChanged;
            tbSampleReferenceDate.TextChanged += tbSampleReferenceDate_TextChanged;

            tbSampleExId.Text = s.ExternalId;
            cbSampleConfidential.Checked = s.Confidential;

            cboxSampleSampleStorage.SelectedValue = s.SampleStorageId;

            cboxSampleInstanceStatus.SelectedValue = s.InstanceStatusId;

            tbSampleComment.Text = s.Comment;
            lblSampleToolId.Text = "[Sample] " + s.Number.ToString();
            lblSampleToolLaboratory.Text = String.IsNullOrEmpty(cboxSampleLaboratory.Text) ? "" : "[Laboratory] " + cboxSampleLaboratory.Text;

            PopulateSampleParameters(s, clearDirty);

            // Show attachments
            UI.PopulateAttachments(conn, trans, "sample", s.Id, gridSampleAttachments);

            if (clearDirty)
            {
                sample.ClearDirty();
            }                        
        }

        private void PopulateSampleParameters(Sample s, bool clearDirty)
        {
            gridSampleParameters.Columns.Clear();
            gridSampleParameters.Rows.Clear();

            gridSampleParameters.Columns.Add("Id", "Id");
            gridSampleParameters.Columns.Add("Name", "Name");            
            gridSampleParameters.Columns.Add("Value", "Value");
            gridSampleParameters.Columns.Add("Type", "Type");

            foreach (SampleParameter p in s.Parameters)
            {
                gridSampleParameters.Rows.Add(new object[] {
                    p.Id,
                    p.Name,                    
                    p.Value,
                    p.Type
                });
            }

            gridSampleParameters.Columns["Id"].Visible = false;

            if (clearDirty)
                foreach (SampleParameter p in s.Parameters)
                    p.Dirty = false;
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

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                int mergeTest = Convert.ToInt32(DB.GetScalar(conn, null, "select count(transform_to_id) from sample where id = '" + sid.ToString() + "'", CommandType.Text));
                if (mergeTest > 0)
                {
                    MessageBox.Show("Cannot split, sample has already been merged");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormSampleSplit form = new FormSampleSplit(sid, treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;
            
            PopulateSamples();

            SetStatusMessage("Splitting sample successful");
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
                sampleIds.Add(Utils.MakeGuid(row.Cells["id"].Value));

            var sampleIdsArr = from item in sampleIds select "'" + item + "'";
            string sampleIdsCsv = string.Join(",", sampleIdsArr);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                int mergeTest = Convert.ToInt32(DB.GetScalar(conn, null, "select count(transform_to_id) from sample where id in (" + sampleIdsCsv + ")", CommandType.Text));
                if(mergeTest > 0)
                {
                    MessageBox.Show("Cannot merge, one or more of these samples has already been merged");
                    return;
                }

                Func<string, int> nCheck = field => Convert.ToInt32(DB.GetScalar(conn, null, "select count(distinct(" + field + ")) from sample where id in(" + sampleIdsCsv + ")", CommandType.Text));

                // FIXME: Must select new sample type
                if ((nCheck("laboratory_id") & nCheck("project_sub_id")) != 1)
                {
                    MessageBox.Show("All samples to be merged must have the same laboratory and project");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormSampleMerge form = new FormSampleMerge(sampleIdsCsv);
            if (form.ShowDialog() != DialogResult.OK)
                return;
            
            PopulateSamples();

            SetStatusMessage("Merging samples successful");
        }

        private void miSamplesSetOrder_Click(object sender, EventArgs e)
        {
            // add sample to order

            if (gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample first");
                return;
            }                

            if (!Utils.IsValidGuid(Common.LabId) || !Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to add samples to orders");
                return;
            }            

            Guid sampleId = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());
            string sampleName = gridSamples.SelectedRows[0].Cells["number"].Value.ToString();

            bool hasLock = false;
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                hasLock = DB.LockSample(conn, sampleId);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            if (!hasLock)
            {
                MessageBox.Show("Sample " + sampleName + " is already locked by someone else");
                return;
            }

            FormSelectOrder form = new FormSelectOrder(treeSampleTypes, sampleId);
            if (form.ShowDialog() == DialogResult.OK)
            {
                SetStatusMessage("Successfully added sample " + sampleName + " to order " + form.SelectedOrderName);
            }

            conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.UnlockSamples(conn);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to preparations and analyses");
                return;
            }

            if(!Utils.IsValidGuid(Common.LabId))
            {
                MessageBox.Show("You must be a member of a laboratory in order to access preparations and analyses");
                return;
            }
            
            if(gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample first");
                return;
            }

            Guid sid = Guid.Parse(gridSamples.SelectedRows[0].Cells["id"].Value.ToString());

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                sample.LoadFromDB(conn, null, sid);

                if (!PopulatePrepAnal(conn, sample))
                {
                    MessageBox.Show("Unable to populate sample");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabPrepAnal;
            tabsPrepAnal.SelectedTab = tabPrepAnalSample;
        }

        private bool PopulatePrepAnal(SqlConnection conn, Sample s)
        {
            if(!s.HasRequiredFields())
            {
                MessageBox.Show("This sample is not complete yet");
                return false;
            }

            treePrepAnal.Nodes.Clear();

            TreeNode sampleNode = null;
            Font fontSample = new Font(treePrepAnal.Font, FontStyle.Bold);            

            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_sample_header", CommandType.StoredProcedure, 
                new SqlParameter("@id", s.Id)))
            {
                reader.Read();

                string txt = reader.GetString("sample_number") + " - " + reader.GetString("sample_type_name") + ", " + reader.GetString("laboratory_name");
                sampleNode = treePrepAnal.Nodes.Add(sample.Id.ToString(), txt);
                sampleNode.NodeFont = fontSample;
            }
            
            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_preparation_headers_for_sample", CommandType.StoredProcedure,
                new SqlParameter("@sample_id", s.Id)))
            {
                while (reader.Read())
                {
                    int status = reader.GetInt32("workflow_status_id");
                    string txt = reader.GetString("preparation_number") + " - " + reader.GetString("preparation_method_name");
                    if(DB.IsValidField(reader["assignment_name"]))
                        txt += ", " + reader.GetString("assignment_name");
                    TreeNode prepNode = sampleNode.Nodes.Add(reader.GetString("preparation_id"), txt);
                    prepNode.ToolTipText = reader.GetString("preparation_method_name_full");
                    prepNode.ForeColor = WorkflowStatus.GetStatusColor(status);
                }
            }
                
            foreach (TreeNode prepNode in sampleNode.Nodes)
            {
                Guid prepId = Guid.Parse(prepNode.Name);
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_analysis_headers_for_preparation", CommandType.StoredProcedure,
                    new SqlParameter("@preparation_id", prepId)))
                {
                    while (reader.Read())
                    {
                        int status = reader.GetInt32("workflow_status_id");
                        string txt = reader.GetString("analysis_number") + " - " + reader.GetString("analysis_method_name");
                        if(DB.IsValidField(reader["assignment_name"]))
                            txt += ", " + reader.GetString("assignment_name");
                        TreeNode analNode = prepNode.Nodes.Add(reader.GetString("analysis_id"), txt);
                        analNode.ToolTipText = reader.GetString("analysis_method_name_full");
                        analNode.ForeColor = WorkflowStatus.GetStatusColor(status);
                    }
                }
            }

            treePrepAnal.ExpandAll();
            
            tbPrepAnalInfoComment.Text = s.Comment;
            tbPrepAnalWetWeight.Text = s.WetWeight_g.ToString();
            tbPrepAnalDryWeight.Text = s.DryWeight_g.ToString();
            tbPrepAnalVolume.Text = s.Volume_l.ToString();
            tbPrepAnalLODStartWeight.Text = s.LodWeightStart.ToString();
            tbPrepAnalLODEndWeight.Text = s.LodWeightEnd.ToString();
            tbPrepAnalLODTemp.Text = s.LodTemperature.ToString();

            sample.ClearDirty();
            preparation.ClearDirty();
            analysis.ClearDirty();

            tabsPrepAnal.SelectedTab = tabPrepAnalSample;
            return true;
        }

        private void miSamplesSetExcempt_Click(object sender, EventArgs e)
        {
            // set sample excempt from public
        }

        private void miSamplingMethodNew_Click(object sender, EventArgs e)
        {
            // new sampling method

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to create sampling methods");
                return;
            }

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to edit sampling methods");
                return;
            }

            if (gridMetaSamplingMeth.SelectedRows.Count < 1)
                return;

            DataGridViewRow row = gridMetaSamplingMeth.SelectedRows[0];
            Guid smid = Utils.MakeGuid(row.Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to delete sampling methods");
                return;
            }
        }

        private void cboxSamplesProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSamplesProjects.SelectedValue))
            {
                cboxSamplesProjectsSub.DataSource = null;
                return;
            }

            Guid projectId = Utils.MakeGuid(cboxSamplesProjects.SelectedValue);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSamplesProjectsSub);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void miPreparationMethodsNew_Click(object sender, EventArgs e)
        {
            // new preparation method

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods");
                return;
            }

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods");
                return;
            }

            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;                
            }
            
            Guid pmid = Utils.MakeGuid(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods");
                return;
            }

            // FIXME: Not implemented
            MessageBox.Show("Not implemented");
        }

        private void miAnalysisMethodsNew_Click(object sender, EventArgs e)
        {
            // new analysis method
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage analysis methods");
                return;
            }

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
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage analysis methods");
                return;
            }

            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a analysis method first");
                return;
            }
            
            Guid amid = Utils.MakeGuid(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value);

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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage analysis methods");
                return;
            }

            // FIXME: Not implemented
            MessageBox.Show("Not implemented");
        }

        private void miAddPrepMethToSampType_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods for sample types");
                return;
            }

            if (treeSampleTypes.SelectedNode == null)
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

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleTypePrepMeth(conn, null, treeSampleTypes.SelectedNode, lbTypeRelSampTypePrepMeth, lbTypeRelSampTypeInheritedPrepMeth);
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

        private List<Guid> GetPreparationMethodsForSampleType(TreeNode tnode, bool ascend)
        {
            List<Guid> existingMethods = new List<Guid>();
            Guid sampleTypeId = Guid.Parse(tnode.Name);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("csp_select_preparation_methods_for_sample_type_short", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@sample_type_id", sampleTypeId, Guid.Empty);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())                    
                        existingMethods.Add(reader.GetGuid("id"));
                }

                if (ascend)
                {
                    while (tnode.Parent != null)
                    {
                        tnode = tnode.Parent;
                        sampleTypeId = Guid.Parse(tnode.Name);

                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@sample_type_id", sampleTypeId, Guid.Empty);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                existingMethods.Add(reader.GetGuid("id"));
                        }
                    }
                }
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
            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

            if (treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            // new sample component
            Guid sampleTypeId = Guid.Parse(treeSampleTypes.SelectedNode.Name);

            FormSampleComponent form = new FormSampleComponent(sampleTypeId, treeSampleTypes.SelectedNode.Text);
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

            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

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

            FormSampleComponent form = new FormSampleComponent(sampleTypeId, treeSampleTypes.SelectedNode.Text, sampleComponent.Id);
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage analysis methods");
                return;
            }

            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an analysis type first");
                return;
            }

            Guid amid = Utils.MakeGuid(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value);
            string amname = gridTypeRelAnalMeth.SelectedRows[0].Cells["name"].Value.ToString();

            List<Guid> existingNuclides = GetNuclidesForAnalysisType(amid);

            FormAnalMethXNuclide form = new FormAnalMethXNuclide(amname, amid, existingNuclides);

            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateAnalMethNuclides(conn, null, amid, lbTypRelAnalMethNuclides);
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

        public List<Guid> GetNuclidesForAnalysisType(Guid amid)
        {
            List<Guid> existingNuclides = new List<Guid>();
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_nuclides_for_analysis_method", CommandType.StoredProcedure,
                    new SqlParameter("@analysis_method_id", amid)))
                {
                    while (reader.Read())
                        existingNuclides.Add(reader.GetGuid("id"));
                }
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

            return existingNuclides;
        }

        private void miAnalysisMethodsRemNuclide_Click(object sender, EventArgs e)
        {
            // remove nuclides from analysis method

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage analysis methods");
                return;
            }

            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an analysis method first");
                return;
            }

            if (lbTypRelAnalMethNuclides.SelectedItems.Count < 1)
            {
                MessageBox.Show("You must select one or more nuclides first");
                return;
            }

            Guid amid = Utils.MakeGuid(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value);

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("delete from analysis_method_x_nuclide where analysis_method_id = @amid and nuclide_id = @nid", conn, trans);

                foreach (Lemma<Guid, string> l in lbTypRelAnalMethNuclides.SelectedItems)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@amid", amid);
                    cmd.Parameters.AddWithValue("@nid", l.Id);
                    cmd.ExecuteNonQuery();
                }                

                UI.PopulateAnalMethNuclides(conn, trans, amid, lbTypRelAnalMethNuclides);

                trans.Commit();
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

        private void miTypeRelPrepMethAddAnalMeth_Click(object sender, EventArgs e)
        {
            // add analysis methods to preparation method
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods");
                return;
            }

            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }

            Guid pmid = Utils.MakeGuid(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value);
            string pmname = gridTypeRelPrepMeth.SelectedRows[0].Cells["name"].Value.ToString();

            List<Guid> existingAnalysisMethods = GetAnalysisMethodsForPreparationMethod(pmid);

            FormPrepMethXAnalMeth form = new FormPrepMethXAnalMeth(pmname, pmid, existingAnalysisMethods);

            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulatePrepMethAnalMeths(conn, null, pmid, lbTypRelPrepMethAnalMeth);
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

        public List<Guid> GetAnalysisMethodsForPreparationMethod(Guid pmid)
        {
            List<Guid> existingAnalysisMethods = new List<Guid>();

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_analysis_methods_for_preparation_method", CommandType.StoredProcedure,
                    new SqlParameter("@preparation_method_id", pmid)))
                {
                    while (reader.Read())
                        existingAnalysisMethods.Add(reader.GetGuid("id"));
                }
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

            return existingAnalysisMethods;
        }

        private void miTypeRelPrepMethRemAnalMeth_Click(object sender, EventArgs e)
        {
            // remove analysis methods from preparation method
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage preparation methods");
                return;
            }

            if(gridTypeRelPrepMeth.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a preparation method first");
                return;
            }

            if(lbTypRelPrepMethAnalMeth.SelectedItems.Count < 1)
            {
                MessageBox.Show("You must select one or more analysis methods first");
                return;
            }

            Guid pmid = Utils.MakeGuid(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value);

            SqlConnection conn = null;
            SqlTransaction trans = null;
            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("delete from preparation_method_x_analysis_method where preparation_method_id = @pmid and analysis_method_id = @amid", conn, trans);

                foreach (Lemma<Guid, string> l in lbTypRelPrepMethAnalMeth.SelectedItems)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@pmid", pmid);
                    cmd.Parameters.AddWithValue("@amid", l.Id);
                    cmd.ExecuteNonQuery();
                }

                UI.PopulatePrepMethAnalMeths(conn, trans, pmid, lbTypRelPrepMethAnalMeth);

                trans.Commit();
            }
            catch(Exception ex)
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

        private void miOrdersNew_Click(object sender, EventArgs e)
        {
            // create new order

            FormOrderNew form = new FormOrderNew();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            assignment = new Assignment();

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                assignment.LoadFromDB(conn, null, form.OrderId);
                tbOrderName.Text = form.OrderName;
                PopulateOrder(conn, null, assignment, true);

                UI.PopulateComboBoxes(conn, "csp_select_assignments_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSamplesOrders);

                UI.PopulateOrderYears(conn, cboxOrdersYear);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabOrder;
            tabsOrder.SelectedTab = tabOrderInfo;
        }

        private void miOrdersEdit_Click(object sender, EventArgs e)
        {
            // edit existing order
            if(gridOrders.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select an order first");
                return;
            }

            Guid orderId = Utils.MakeGuid(gridOrders.SelectedRows[0].Cells["id"].Value);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (!DB.HasAccessToOrder(conn, null, orderId))
                {
                    MessageBox.Show("You don't have permission to edit this order");
                    return;
                }

                assignment.LoadFromDB(conn, null, orderId);

                PopulateOrder(conn, null, assignment, true);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabOrder;
            tabsOrder.SelectedTab = tabOrderInfo;
        }

        private void miOrdersDelete_Click(object sender, EventArgs e)
        {
            // delete order
        }        

        private void PopulateOrder(SqlConnection conn, SqlTransaction trans, Assignment a, bool clearDirty)
        {
            tbOrderName.Text = a.Name;
            cboxOrderLaboratory.SelectedValue = a.LaboratoryId;
            cboxOrderResponsible.SelectedValue = a.AccountId;
            if (a.Deadline.HasValue)
            {
                tbOrderDeadline.Tag = a.Deadline.Value;
                tbOrderDeadline.Text = a.Deadline.Value.ToString(Utils.DateFormatNorwegian);
            }
            else
            {
                tbOrderDeadline.Tag = null;
                tbOrderDeadline.Text = "";
            }
            cboxOrderRequestedSigma.SelectedValue = a.RequestedSigmaAct;
            cboxOrderRequestedSigmaMDA.SelectedValue = a.RequestedSigmaMDA;
            tbOrderCustomer.Text = a.CustomerContactName;
            tbOrderCustomerInfo.Text =
                a.CustomerContactName + Environment.NewLine +
                a.CustomerCompanyName + Environment.NewLine + Environment.NewLine +
                a.CustomerContactEmail + Environment.NewLine +
                a.CustomerContactPhone + Environment.NewLine + Environment.NewLine +
                a.CustomerContactAddress;
            tbOrderContentComment.Text = a.ContentComment;
            tbOrderReportComment.Text = a.ReportComment;
            cbOrderApprovedCustomer.Checked = a.ApprovedCustomer;
            tbOrderApprovedCustomerBy.Text = DB.GetNameFromUsername(conn, trans, a.ApprovedCustomerBy);
            cbOrderApprovedLaboratory.Checked = a.ApprovedLaboratory;
            tbOrderApprovedLaboratoryBy.Text = DB.GetNameFromUsername(conn, trans, a.ApprovedLaboratoryBy);
            cboxOrderStatus.SelectedValue = a.WorkflowStatusId;
            tbOrderLastWorkflowStatusBy.Text = DB.GetNameFromUsername(conn, trans, a.LastWorkflowStatusBy);
            if (a.LastWorkflowStatusDate.HasValue)
                tbOrderLastWorkflowStatusBy.Text += " at " + a.LastWorkflowStatusDate.Value.ToString(Utils.DateTimeFormatNorwegian);

            PopulateOrderContent(conn, trans, a);

            // Show attachments
            UI.PopulateAttachments(conn, null, "assignment", a.Id, gridOrderAttachments);

            PopulateOrderOverview(conn, trans, a);

            if(clearDirty)
            {
                assignment.Dirty = false;
                foreach (AssignmentSampleType ast in a.SampleTypes)
                {
                    ast.Dirty = false;
                    foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                    {
                        apm.Dirty = false;
                        foreach (AssignmentAnalysisMethod aam in apm.AnalysisMethods)                 
                            aam.Dirty = false;
                    }
                }
            }

            if (a.WorkflowStatusId == WorkflowStatus.Complete)
            {
                cbOrderApprovedCustomer.Enabled = false;
                cbOrderApprovedLaboratory.Enabled = false;
                tbOrderReportComment.Enabled = false;
            }
        }

        private void PopulateOrderContent(SqlConnection conn, SqlTransaction trans, Assignment a)
        {
            treeOrderContent.Nodes.Clear();

            foreach (AssignmentSampleType ast in a.SampleTypes)
            {
                string txt = ast.SampleCount.ToString() + ", " + ast.SampleTypeName(conn, trans);
                if (Utils.IsValidGuid(ast.SampleComponentId))
                    txt += ", " + ast.SampleComponentName(conn, trans);
                TreeNode[] nodes = treeSampleTypes.Nodes.Find(ast.SampleTypeId.ToString(), true);
                if (nodes.Length > 0)
                    txt += " -> " + nodes[0].FullPath;
                if (ast.ReturnToSender)
                    txt += ", Return to customer";

                TreeNode tnode = treeOrderContent.Nodes.Add(ast.Id.ToString(), txt);
                tnode.Tag = ast;
                tnode.ToolTipText = ast.Comment;
                tnode.NodeFont = new Font(treeOrderContent.Font.FontFamily, treeOrderContent.Font.Size, FontStyle.Bold);

                foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                {
                    txt = apm.PreparationMethodCount.ToString() + ", " + apm.PreparationMethodName(conn, trans);
                    if (Utils.IsValidGuid(apm.PreparationLaboratoryId))
                        txt += " (" + apm.PreparationLaboratoryName(conn, trans) + ")";

                    TreeNode tn = tnode.Nodes.Add(apm.Id.ToString(), txt);
                    tn.Tag = apm;
                    tn.ToolTipText = apm.PreparationMethodNameFull(conn, trans) + Environment.NewLine + Environment.NewLine + apm.Comment;

                    foreach (AssignmentAnalysisMethod aam in apm.AnalysisMethods)
                    {
                        txt = aam.AnalysisMethodCount.ToString() + ", " + aam.AnalysisMethodName(conn, trans);
                        if (Utils.IsValidGuid(assignment.LaboratoryId))
                            txt += " (" + assignment.LaboratoryName(conn, trans) + ")";

                        TreeNode tn2 = tn.Nodes.Add(aam.Id.ToString(), txt);
                        tn2.Tag = aam;
                        tn2.ToolTipText = aam.AnalysisMethodNameFull(conn, trans) + Environment.NewLine + Environment.NewLine + aam.Comment;
                    }
                }
            }

            treeOrderContent.ExpandAll();
        }        

        private void PopulateOrderOverview(SqlConnection conn, SqlTransaction trans, Assignment a)
        {
            // Populate order overview            

            Font fontSample = new Font(tvOrderContent.Font, FontStyle.Bold);
            
            tvOrderContent.Nodes.Clear();

            List<SampleHeader> sampHeaders = new List<SampleHeader>();

            TreeNode root = tvOrderContent.Nodes.Add(a.Name);
            root.NodeFont = fontSample;
            
            string query = @"
select s.id as 'sample_id', s.number as 'sample_number', p.id as 'preparation_id', p.number as 'preparation_number', a.id as 'analysis_id', a.number as 'analysis_number'
from analysis a
            inner join preparation p on a.preparation_id = p.id and p.instance_status_id = 1
            inner join sample s on s.id = p.sample_id
where a.instance_status_id = 1 and a.assignment_id = @assid
order by s.number, p.number, a.number
";
            SampleHeader sh = null;
            PreparationHeader ph = null;
            AnalysisHeader ah = null;
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@assid", a.Id)))
            {
                while (reader.Read())
                {
                    Guid sId = reader.GetGuid("sample_id");
                    int sNumber = reader.GetInt32("sample_number");
                    sh = sampHeaders.Find(x => x.Id == sId);
                    if (sh == null)
                    {
                        sh = new SampleHeader();
                        sampHeaders.Add(sh);
                    }
                    sh.Id = sId;
                    sh.Number = sNumber;

                    Guid pId = reader.GetGuid("preparation_id");
                    int pNumber = reader.GetInt32("preparation_number");
                    ph = sh.Preparations.Find(x => x.Id == pId);
                    if (ph == null)
                    {
                        ph = new PreparationHeader();
                        sh.Preparations.Add(ph);
                    }
                    ph.Id = pId;
                    ph.Number = pNumber;

                    Guid aId = reader.GetGuid("analysis_id");
                    int aNumber = reader.GetInt32("analysis_number");
                    ah = ph.Analyses.Find(x => x.Id == aId);
                    if (ah == null)
                    {
                        ah = new AnalysisHeader();
                        ph.Analyses.Add(ah);
                    }
                    ah.Id = aId;
                    ah.Number = aNumber;
                }
            }

            query = @"
select s.id as 'sample_id', s.number as 'sample_number', p.id as 'preparation_id', p.number as 'preparation_number'
from preparation p    
            inner join sample s on s.id = p.sample_id
where p.instance_status_id = 1 and p.assignment_id = @assid
order by s.number, p.number
";            
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@assid", a.Id)))
            {
                while (reader.Read())
                {
                    Guid sId = reader.GetGuid("sample_id");
                    int sNumber = reader.GetInt32("sample_number");
                    sh = sampHeaders.Find(x => x.Id == sId);
                    if (sh == null)
                    {
                        sh = new SampleHeader();
                        sampHeaders.Add(sh);
                    }
                    sh.Id = sId;
                    sh.Number = sNumber;

                    Guid pId = reader.GetGuid("preparation_id");
                    int pNumber = reader.GetInt32("preparation_number");
                    ph = sh.Preparations.Find(x => x.Id == pId);
                    if (ph == null)
                    {
                        ph = new PreparationHeader();
                        sh.Preparations.Add(ph);
                    }
                    ph.Id = pId;
                    ph.Number = pNumber;
                }
            }

            sampHeaders.Sort((s1, s2) => s1.Number.CompareTo(s2.Number));

            foreach (SampleHeader sHeader in sampHeaders)
            {
                sHeader.Populate(conn, trans);
                string label = "Sample " + sHeader.Number + ", " + sHeader.SampleTypeName + " " + sHeader.SampleComponentName;
                TreeNode sNode = root.Nodes.Add(sHeader.Id.ToString(), label);
                sNode.NodeFont = fontSample;

                foreach (PreparationHeader pHeader in sHeader.Preparations)
                {
                    pHeader.Populate(conn, trans);
                    label = "Preparation " + pHeader.Number + ", " + pHeader.PreparationMethodName + ", " + pHeader.LaboratoryName + ", " + pHeader.WorkflowStatusName;
                    TreeNode pNode = sNode.Nodes.Add(pHeader.Id.ToString(), label);
                    pNode.ForeColor = WorkflowStatus.GetStatusColor(pHeader.WorkflowStatusId);

                    foreach (AnalysisHeader aHeader in pHeader.Analyses)
                    {
                        aHeader.Populate(conn, trans);
                        label = "Analysis " + aHeader.Number + ", " + aHeader.AnalysisMethodName + ", " + aHeader.LaboratoryName + ", " + aHeader.WorkflowStatusName;
                        TreeNode aNode = pNode.Nodes.Add(aHeader.Id.ToString(), label);
                        aNode.ForeColor = WorkflowStatus.GetStatusColor(aHeader.WorkflowStatusId);
                    }
                }
            }

            btnOrderRemoveSampleFromOrder.Enabled = btnOrderGoToPrepAnal.Enabled = false;

            tvOrderContent.ExpandAll();
        }

        private void miOrderAddSampleType_Click(object sender, EventArgs e)
        {
            // add sample type to order
            if (assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("This order has been closed and can not be updated");
                return;
            }
                            
            if (assignment.ApprovedLaboratory || assignment.ApprovedCustomer)
            {
                MessageBox.Show("This order has been approved and can not be updated");
                return;
            }

            FormOrderAddSampleType form = new FormOrderAddSampleType(assignment, treeSampleTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateOrderContent(conn, null, assignment);
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

        private void miOrderRemSampleType_Click(object sender, EventArgs e)
        {
            // remove sample type from order            

            if (!Roles.HasAccess(Role.OrderOperator, Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have permission to delete sample types from orders");
                return;
            }

            if (treeOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            if (assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed order");
                return;
            }

            if (assignment.ApprovedCustomer || assignment.ApprovedLaboratory)
            {
                MessageBox.Show("You can not edit an approved order");
                return;
            }

            TreeNode tnode = treeOrderContent.SelectedNode;
            if(tnode.Level != 0)
            {
                MessageBox.Show("You must select a top level sample type");
                return;
            }

            Guid astId = Guid.Parse(tnode.Name);
            string query = @"
select count(*) from sample s
    inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
    inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id and ast.id = @astid    
";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            int n;
            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();
                                
                n = (int)DB.GetScalar(conn, trans, query, CommandType.Text, new SqlParameter("@astid", astId));                
                if (n > 0)
                {
                    MessageBox.Show("Can not delete this sample type, it has " + n + " samples connected to it");
                    return;
                }

                DialogResult r = MessageBox.Show("Are you sure you want to delete this sample type?", "Warning", MessageBoxButtons.YesNo);
                if (r == DialogResult.No)
                    return;

                List<Guid> delApmIds = new List<Guid>();
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_preparation_method where assignment_sample_type_id = @astid", CommandType.Text, new SqlParameter("@astid", astId)))
                {
                    while(reader.Read())                    
                        delApmIds.Add(reader.GetGuid("id"));
                }

                SqlCommand cmd = new SqlCommand("", conn, trans);
                foreach (Guid pid in delApmIds)
                {
                    cmd.CommandText = "delete from assignment_analysis_method where assignment_preparation_method_id = @pid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from assignment_preparation_method where id = @pid";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "delete from assignment_sample_type where id = @astid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@astid", astId);
                cmd.ExecuteNonQuery();

                trans.Commit();                
            }
            catch(Exception ex)
            {                
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            if (assignment.SampleTypes.Exists(x => x.Id == astId))
            {
                var ast = assignment.SampleTypes.Find(x => x.Id == astId);
                assignment.SampleTypes.Remove(ast);
            }            

            tnode.Remove();
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

            AssignmentSampleType ast = tnode.Tag as AssignmentSampleType;
            
            if (assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("This order has been closed and can not be updated");
                return;
            }

            if (assignment.ApprovedLaboratory || assignment.ApprovedCustomer)
            {
                MessageBox.Show("This order has been approved and can not be updated");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                int nSamples;
                DB.GetSampleCountForAST(conn, null, ast.Id, out nSamples);
                if(nSamples > 0)
                {
                    MessageBox.Show("Can not add preparation methods to this sample type because it has " + nSamples + " samples connected to it");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormOrderAddPrepMeth form = new FormOrderAddPrepMeth(assignment, ast);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateOrderContent(conn, null, assignment);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
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

            AssignmentPreparationMethod apm = tnode.Tag as AssignmentPreparationMethod;
            AssignmentSampleType ast = tnode.Parent.Tag as AssignmentSampleType;

            if (assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("This order has been closed and can not be updated");
                return;
            }

            if (assignment.ApprovedLaboratory || assignment.ApprovedCustomer)
            {
                MessageBox.Show("This order has been approved and can not be updated");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                int nSamples;
                DB.GetSampleCountForAST(conn, null, ast.Id, out nSamples);
                if (nSamples > 0)
                {
                    MessageBox.Show("Can not add analysis methods to this sample type because it has " + nSamples + " samples connected to it");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormOrderAddAnalMeth form = new FormOrderAddAnalMeth(assignment, apm);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateOrderContent(conn, null, assignment);
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

        private void miOrderSave_Click(object sender, EventArgs e)
        {
            // save order            
            if(!assignment.IsDirty())
            {
                SetStatusMessage("Nothing to save for order " + assignment.Name);
                return;
            }            

            int wfStatus = (int)cboxOrderStatus.SelectedValue;

            if (wfStatus == WorkflowStatus.Complete && (!cbOrderApprovedCustomer.Checked || !cbOrderApprovedLaboratory.Checked))
            {
                MessageBox.Show("You can not complete an order that is not approved");
                return;
            }

            if (wfStatus == WorkflowStatus.Complete && wfStatus == assignment.WorkflowStatusId)
            {
                MessageBox.Show("Can not save a completed order");
                return;
            }

            if (!Utils.IsValidGuid(cboxOrderResponsible.SelectedValue))
            {
                MessageBox.Show("Responsible is mandatory");
                return;
            }

            if (tbOrderDeadline.Tag == null)
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }

            DateTime deadline = (DateTime)tbOrderDeadline.Tag;
            if (deadline.Date < DateTime.Now.Date)
            {
                if(assignment.WorkflowStatusId == WorkflowStatus.Complete && wfStatus == WorkflowStatus.Construction)
                {                    
                    FormSelectDate form = new FormSelectDate("New deadline required");
                    if (form.ShowDialog() != DialogResult.OK)
                        return;

                    if(form.SelectedDate.Date < DateTime.Now.Date)
                    {
                        MessageBox.Show("Deadline can not be in the past");
                        return;
                    }

                    deadline = form.SelectedDate;
                    assignment.Deadline = deadline;
                    tbOrderDeadline.Tag = assignment.Deadline.Value;
                    tbOrderDeadline.Text = assignment.Deadline.Value.ToString(Utils.DateFormatNorwegian);

                    cbOrderApprovedCustomer.Checked = false;
                    cbOrderApprovedLaboratory.Checked = false;
                }
                else if(wfStatus == WorkflowStatus.Rejected)
                {

                }
                else
                {
                    MessageBox.Show("Deadline can not be in the past");
                    return;
                }
            }

            if (String.IsNullOrEmpty(tbOrderCustomer.Text))
            {
                MessageBox.Show("Customer is mandatory");
                return;
            }

            if (!Utils.IsValidGuid(cboxOrderLaboratory.SelectedValue))
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (!cbOrderApprovedLaboratory.Checked)
            {
                if (assignment.ApprovedLaboratory)
                {
                    if (!Utils.IsValidGuid(Common.LabId))
                    {
                        MessageBox.Show("You can not approve orders for laboratory");
                        return;
                    }

                    if (!Roles.HasAccess(Role.LaboratoryAdministrator))
                    {
                        MessageBox.Show("You don't have permission to approve orders");
                        return;
                    }

                    if (Common.LabId != assignment.LaboratoryId)
                    {
                        MessageBox.Show("You can not approve orders for this laboratory");
                        return;
                    }
                }
            }

            if (cbOrderApprovedLaboratory.Checked)
            {
                if(!assignment.ApprovedLaboratory)
                {
                    if (!Utils.IsValidGuid(Common.LabId))
                    {
                        MessageBox.Show("You can not approve orders for laboratory");
                        return;
                    }

                    if (!Roles.HasAccess(Role.LaboratoryAdministrator))
                    {
                        MessageBox.Show("You don't have permission to approve orders");
                        return;
                    }

                    if (Common.LabId != assignment.LaboratoryId)
                    {
                        MessageBox.Show("You can not approve orders for this laboratory");
                        return;
                    }
                }

                if (assignment.SampleTypes.Count == 0)
                {
                    MessageBox.Show("Can not approve an empty order");
                    return;
                }
                else
                {
                    bool found = false;
                    foreach (AssignmentSampleType ast in assignment.SampleTypes)
                    {
                        if (ast.PreparationMethods.Count > 0)
                            found = true;

                        foreach (AssignmentPreparationMethod apm in ast.PreparationMethods)
                        {
                            if (apm.AnalysisMethods.Count > 0)
                                found = true;
                        }
                    }

                    if (!found)
                    {
                        MessageBox.Show("Can not approve an order without any preparation or analysis methods");
                        return;
                    }
                }
            }

            if (cbOrderApprovedCustomer.Checked && !cbOrderApprovedLaboratory.Checked)
            {
                MessageBox.Show("Laboratory must approve this order before customer");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();                

                if (wfStatus == WorkflowStatus.Complete)
                {
                    if (!Roles.HasAccess(Role.LaboratoryAdministrator))
                    {
                        MessageBox.Show("You are not allowed to complete orders");
                        return;
                    }

                    // Check that the order is full
                    int nCurrSamples, nCurrPreparations, nCurrAnalyses;
                    DB.GetOrderCurrentInventory(conn, trans, assignment.Id, out nCurrSamples, out nCurrPreparations, out nCurrAnalyses);

                    int nReqSamples, nReqPreparations, nReqAnalyses;
                    DB.GetOrderRequiredInventory(conn, trans, assignment.Id, out nReqSamples, out nReqPreparations, out nReqAnalyses);

                    if(nCurrSamples < nReqSamples)
                    {
                        MessageBox.Show("Can not set this order to complete. Connected samples " + nCurrSamples + " of " + nReqSamples);
                        return;
                    }

                    if (nCurrPreparations < nReqPreparations)
                    {
                        MessageBox.Show("Can not set this order to complete. Connected preparations " + nCurrPreparations + " of " + nReqPreparations);
                        return;
                    }

                    if (nCurrAnalyses < nReqAnalyses)
                    {
                        MessageBox.Show("Can not set this order to complete. Connected analyses " + nCurrAnalyses + " of " + nReqAnalyses);
                        return;
                    }

                    // Check that everything is complete
                    string query = "select count(*) from preparation where assignment_id = @assid and workflow_status_id > 1 and instance_status_id < 2";
                    int np = (int)DB.GetScalar(conn, trans, query, CommandType.Text, new SqlParameter("@assid", assignment.Id));
                    if(np < nReqPreparations)
                    {
                        MessageBox.Show("Can not set this order to complete. One or more preparations are not completed or rejected");
                        return;
                    }

                    query = "select count(*) from analysis where assignment_id = @assid and workflow_status_id > 1 and instance_status_id < 2";
                    np = (int)DB.GetScalar(conn, trans, query, CommandType.Text, new SqlParameter("@assid", assignment.Id));
                    if (np < nReqAnalyses)
                    {
                        MessageBox.Show("Can not set this order to complete. One or more analyses are not completed or rejected");
                        return;
                    }
                }

                if (assignment.ApprovedLaboratory != cbOrderApprovedLaboratory.Checked)
                {
                    assignment.ApprovedLaboratoryBy = Common.Username;
                }

                if (assignment.ApprovedCustomer != cbOrderApprovedCustomer.Checked)
                {
                    assignment.ApprovedCustomerBy = Common.Username;
                }

                if (assignment.WorkflowStatusId != wfStatus)
                {
                    assignment.LastWorkflowStatusBy = Common.Username;
                    assignment.LastWorkflowStatusDate = DateTime.Now;
                }

                assignment.LaboratoryId = Utils.MakeGuid(cboxOrderLaboratory.SelectedValue);
                assignment.AccountId = Utils.MakeGuid(cboxOrderResponsible.SelectedValue);
                assignment.Deadline = deadline;
                assignment.RequestedSigmaAct = (double)cboxOrderRequestedSigma.SelectedValue;
                assignment.RequestedSigmaMDA = (double)cboxOrderRequestedSigmaMDA.SelectedValue;
                assignment.ContentComment = tbOrderContentComment.Text.Trim();
                assignment.ApprovedLaboratory = cbOrderApprovedLaboratory.Checked;
                assignment.ApprovedCustomer = cbOrderApprovedCustomer.Checked;
                assignment.ReportComment = tbOrderReportComment.Text.Trim();
                assignment.WorkflowStatusId = wfStatus;                           

                assignment.StoreToDB(conn, trans);

                trans.Commit();

                PopulateOrder(conn, trans, assignment, true);

                btnOrdersSearch_Click(sender, e);

                SetStatusMessage("Order " + assignment.Name + " updated");

                tbOrderReportComment.Enabled = cbOrderApprovedLaboratory.Enabled = cbOrderApprovedCustomer.Enabled = 
                    !(assignment.WorkflowStatusId == WorkflowStatus.Complete);
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

            assignment.Dirty = true;
        }

        private void miCustomersNew_Click(object sender, EventArgs e)
        {
            // create new customer            

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to create customers");
                return;
            }

            FormCustomer form = new FormCustomer();
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer created");
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to edit customers");
                return;
            }

            if (gridCustomers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a customer first");
                return;
            }
            
            Guid cid = Utils.MakeGuid(gridCustomers.SelectedRows[0].Cells["id"].Value);

            FormCustomer form = new FormCustomer(cid);
            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Customer updated");
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

            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to delete customers");
                return;
            }
        }        

        private void gridSysCounty_SelectionChanged(object sender, EventArgs e)
        {
            if (gridSysCounty.SelectedRows.Count < 1)
                return;

            Guid cid = Utils.MakeGuid(gridSysCounty.SelectedRows[0].Cells["id"].Value);
            using (SqlConnection conn = DB.OpenConnection())
                UI.PopulateMunicipalities(conn, cid, gridSysMunicipality);
        }

        private void gridProjectMain_SelectionChanged(object sender, EventArgs e)
        {
            if (gridProjectMain.SelectedRows.Count < 1)
            {
                gridProjectSub.DataSource = null;
                gridProjectsUsers.DataSource = null;
                return;
            }

            Guid pmid = Utils.MakeGuid(gridProjectMain.SelectedRows[0].Cells["id"].Value);
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateProjectsSub(conn, pmid, gridProjectSub);
            }
        }

        private void gridTypeRelAnalMeth_SelectionChanged(object sender, EventArgs e)
        {
            if (gridTypeRelAnalMeth.SelectedRows.Count < 1)
                return;

            Guid amid = Utils.MakeGuid(gridTypeRelAnalMeth.SelectedRows[0].Cells["id"].Value);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateAnalMethNuclides(conn, null, amid, lbTypRelAnalMethNuclides);
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

        private void gridTypeRelPrepMeth_SelectionChanged(object sender, EventArgs e)
        {
            if (gridTypeRelPrepMeth.SelectedRows.Count < 1)
                return;

            Guid pmid = Utils.MakeGuid(gridTypeRelPrepMeth.SelectedRows[0].Cells["id"].Value);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulatePrepMethAnalMeths(conn, null, pmid, lbTypRelPrepMethAnalMeth);
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

        private void cboxOrderLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;

            if (!Utils.IsValidGuid(cboxOrderLaboratory.SelectedValue))
            {            
                cboxOrderResponsible.SelectedValue = Guid.Empty;
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_accounts_for_laboratory_short", new[] {
                    new SqlParameter("@laboratory_id", assignment.LaboratoryId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxOrderResponsible);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
        }

        private void treePrepAnal_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnPrepAnalAddPrep.Enabled = false;
            btnPrepAnalDelPrep.Enabled = false;
            btnPrepAnalAddAnal.Enabled = false;
            btnPrepAnalDelAnal.Enabled = false;

            sample.ClearDirty();
            preparation.ClearDirty();
            analysis.ClearDirty();

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                Guid sid, pid;
                switch (e.Node.Level)
                {
                    case 0:                        
                        sid = Guid.Parse(e.Node.Name);
                        sample.LoadFromDB(conn, null, sid);
                        PopulateSampleInfo(conn, null, sample, e.Node, true);
                        btnPrepAnalAddPrep.Enabled = true;
                        tabsPrepAnal.SelectedTab = tabPrepAnalSample;
                        break;
                    case 1:
                        sid = Guid.Parse(e.Node.Parent.Name);
                        sample.LoadFromDB(conn, null, sid);
                        pid = Guid.Parse(e.Node.Name);
                        preparation.LoadFromDB(conn, null, pid);
                        PopulatePreparation(conn, null, preparation, true);
                        btnPrepAnalDelPrep.Enabled = true;
                        btnPrepAnalAddAnal.Enabled = true;
                        tabsPrepAnal.SelectedTab = tabPrepAnalPreps;
                        break;
                    case 2:
                        sid = Guid.Parse(e.Node.Parent.Parent.Name);
                        sample.LoadFromDB(conn, null, sid);
                        pid = Guid.Parse(e.Node.Parent.Name);
                        preparation.LoadFromDB(conn, null, pid);
                        Guid aid = Guid.Parse(e.Node.Name);
                        analysis.LoadFromDB(conn, null, aid);
                        PopulateAnalysis(conn, null, analysis, true);
                        btnPrepAnalDelAnal.Enabled = true;
                        tabsPrepAnal.SelectedTab = tabPrepAnalAnalysis;
                        break;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
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
            if (e.TabPage == tabSample || e.TabPage == tabPrepAnal)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    if (!DB.LockSample(conn, sample.Id))
                    {
                        MessageBox.Show("Unable to lock sample");
                        e.Cancel = true;
                    }
                }
            }
            else if (e.TabPage == tabOrder)
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    if (!DB.LockOrder(conn, assignment.Id))
                    {
                        MessageBox.Show("Unable to lock order");
                        e.Cancel = true;
                    }
                }
            }            
        }

        private void tabs_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (!DiscardUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }
            
            if (e.TabPage == tabSample || e.TabPage == tabPrepAnal)
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
            if (!sample.IsDirty())
            {
                SetStatusMessage("Nothing to save for sample " + sample.Number);
                return;
            }

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

            if((int)cboxSampleInstanceStatus.SelectedValue == 0)
            {
                MessageBox.Show("Status is mandatory");
                return;
            }

            if(!String.IsNullOrEmpty(tbSampleInfoAltitude.Text) && !Utils.IsValidDecimal(tbSampleInfoAltitude.Text))
            {
                MessageBox.Show("Altitude must be a number");
                return;
            }

            double? lat = null, lon = null, alt = null;

            try
            {
                if (!String.IsNullOrEmpty(tbSampleInfoLatitude.Text.Trim()))
                    lat = UtilsGeo.GetLatitude(tbSampleInfoLatitude.Text.Trim());

                if (!String.IsNullOrEmpty(tbSampleInfoLongitude.Text.Trim()))
                    lon = UtilsGeo.GetLongitude(tbSampleInfoLongitude.Text.Trim());                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (!String.IsNullOrEmpty(tbSampleInfoAltitude.Text))
                alt = Convert.ToDouble(tbSampleInfoAltitude.Text);

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();

                Guid newSampleTypeId = Utils.MakeGuid(cboxSampleSampleType.SelectedValue);
                if (sample.SampleTypeId != newSampleTypeId && sample.HasOrders(conn, null))
                {
                    MessageBox.Show("Can not change sample type. This sample belongs to one or more orders");
                    return;
                }

                if (tbSampleReferenceDate.Tag != null)
                {
                    DateTime newRefDate = (DateTime)tbSampleReferenceDate.Tag;
                    if (sample.ReferenceDate != newRefDate)
                    {
                        if (sample.HasCompletedAnalysisResults(conn, null))
                        {
                            MessageBox.Show("Can not change reference date. This sample has one or more completed analyses");
                            return;
                        }
                    }
                }

                sample.LaboratoryId = Utils.MakeGuid(cboxSampleLaboratory.SelectedValue);
                sample.SampleTypeId = Utils.MakeGuid(cboxSampleSampleType.SelectedValue);
                sample.SampleStorageId = Utils.MakeGuid(cboxSampleSampleStorage.SelectedValue);
                sample.SampleComponentId = Utils.MakeGuid(cboxSampleSampleComponent.SelectedValue);
                sample.ProjectSubId = Utils.MakeGuid(cboxSampleSubProject.SelectedValue);
                sample.StationId = Utils.MakeGuid(cboxSampleInfoStations.SelectedValue);
                sample.SamplerId = Utils.MakeGuid(cboxSampleInfoSampler.SelectedValue);
                sample.SamplingMethodId = Utils.MakeGuid(cboxSampleInfoSamplingMeth.SelectedValue);
                sample.MunicipalityId = Utils.MakeGuid(cboxSampleMunicipalities.SelectedValue);
                sample.LocationType = cboxSampleInfoLocationTypes.Text;
                sample.Location = tbSampleLocation.Text;
                sample.Latitude = lat;
                sample.Longitude = lon;
                sample.Altitude = alt;
                if (tbSampleSamplingDateFrom.Tag != null)
                    sample.SamplingDateFrom = (DateTime)tbSampleSamplingDateFrom.Tag;
                else sample.SamplingDateFrom = null;
                if (tbSampleSamplingDateTo.Tag != null)
                    sample.SamplingDateTo = (DateTime)tbSampleSamplingDateTo.Tag;
                else sample.SamplingDateTo = null;
                if (tbSampleReferenceDate.Tag != null)
                    sample.ReferenceDate = (DateTime)tbSampleReferenceDate.Tag;
                else sample.ReferenceDate = null;
                sample.ExternalId = tbSampleExId.Text;
                sample.Confidential = cbSampleConfidential.Checked;
                sample.InstanceStatusId = (int)cboxSampleInstanceStatus.SelectedValue;
                sample.Comment = tbSampleComment.Text.Trim();

                sample.StoreToDB(conn, null);

                SetStatusMessage("Sample " + sample.Number + " updated");
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
            if (!preparation.IsDirty())
            {
                SetStatusMessage("Nothing to save for preparation " + preparation.Number);
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                if(preparation.IsClosed(conn, trans))
                {
                    MessageBox.Show("This preparation belongs to a closed order and can not be updated");
                    return;
                }

                if(!Utils.IsValidGuid(cboxPrepAnalPrepGeom.SelectedValue))
                {
                    MessageBox.Show("Geometry is required");
                    return;
                }

                if (String.IsNullOrEmpty(tbPrepAnalPrepAmount.Text))
                {
                    MessageBox.Show("Preparation amount is required");
                    return;
                }
                
                if ((int)cboxPrepAnalPrepAmountUnit.SelectedValue == 0)
                {
                    MessageBox.Show("Preparation amount unit is required");
                    return;
                }

                if ((int)cboxPrepAnalPrepWorkflowStatus.SelectedValue == 0)
                {
                    MessageBox.Show("Preparation status is required");
                    return;
                }


                Guid pgid = Utils.MakeGuid(cboxPrepAnalPrepGeom.SelectedValue);
                if (!String.IsNullOrEmpty(tbPrepAnalPrepFillHeight.Text) && Utils.IsValidGuid(pgid))
                {
                    double fh = Convert.ToDouble(tbPrepAnalPrepFillHeight.Text);
                    PreparationGeometry pg = new PreparationGeometry(conn, trans, pgid);
                    if(fh < pg.MinFillHeightMM || fh > pg.MaxFillHeightMM)
                    {
                        MessageBox.Show("Fill height " + fh + " is outside valid range. Valid range is [" + pg.MinFillHeightMM + ", " + pg.MaxFillHeightMM + "]");
                        return;
                    }
                }

                if(preparation.WorkflowStatusId == WorkflowStatus.Complete && (int)cboxPrepAnalPrepWorkflowStatus.SelectedValue == WorkflowStatus.Complete)
                {
                    MessageBox.Show("Can not save a completed preparation");
                    return;
                }
                
                if (preparation.WorkflowStatusId != WorkflowStatus.Rejected && (int)cboxPrepAnalPrepWorkflowStatus.SelectedValue == WorkflowStatus.Rejected)
                {
                    SqlCommand cmd = new SqlCommand("select count(*) from analysis where preparation_id = @pid and workflow_status_id <> 3", conn, trans);
                    cmd.Parameters.AddWithValue("@pid", preparation.Id);
                    int n = (int)cmd.ExecuteScalar();
                    if(n > 0)
                    {
                        MessageBox.Show("Can not reject this preparation because one or more analyses are not rejected");
                        return;
                    }
                }

                preparation.PreparationGeometryId = pgid;
                preparation.FillHeightMM = Utils.ToDouble(tbPrepAnalPrepFillHeight.Text);
                preparation.Amount = Utils.ToDouble(tbPrepAnalPrepAmount.Text);
                preparation.PrepUnitId = (int)cboxPrepAnalPrepAmountUnit.SelectedValue;
                preparation.Quantity = Utils.ToDouble(tbPrepAnalPrepQuantity.Text);
                preparation.QuantityUnitId = (int)cboxPrepAnalPrepQuantityUnit.SelectedValue;
                preparation.Comment = tbPrepAnalPrepComment.Text.Trim();
                preparation.WorkflowStatusId = (int)cboxPrepAnalPrepWorkflowStatus.SelectedValue;

                preparation.StoreToDB(conn, trans);

                trans.Commit();

                treePrepAnal.SelectedNode.ForeColor = WorkflowStatus.GetStatusColor(preparation.WorkflowStatusId);
                SetStatusMessage("Preparation updated successfully");
            }
            catch(Exception ex)
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

        private void PopulatePreparation(SqlConnection conn, SqlTransaction trans, Preparation p, bool clearDirty)
        {
            lblPrepAnalPrepRange.Text = "";
            cboxPrepAnalPrepGeom.SelectedValue = p.PreparationGeometryId;
            tbPrepAnalPrepFillHeight.Text = p.FillHeightMM.ToString();
            tbPrepAnalPrepAmount.Text = p.Amount.ToString();            
            cboxPrepAnalPrepAmountUnit.SelectedValue = p.PrepUnitId;
            tbPrepAnalPrepQuantity.Text = p.Quantity.ToString();            
            cboxPrepAnalPrepQuantityUnit.SelectedValue = p.QuantityUnitId;
            tbPrepAnalPrepComment.Text = p.Comment;
            cboxPrepAnalPrepWorkflowStatus.SelectedValue = p.WorkflowStatusId;
            tbPrepAnalPrepReqUnit.Text = p.GetRequestedActivityUnitName(conn, trans);

            if (Utils.IsValidGuid(p.PreparationGeometryId))
            {
                PreparationGeometry pg = new PreparationGeometry(conn, trans, p.PreparationGeometryId);
                lblPrepAnalPrepRange.Text = "[" + pg.MinFillHeightMM + ", " + pg.MaxFillHeightMM + "]";
            }

            UI.PopulateAttachments(conn, trans, "preparation", p.Id, gridPrepAnalPrepAttachments);

            btnPrepAnalPrepUpdate.Enabled = true;
            btnPrepAnalPrepDiscard.Enabled = true;

            if (Utils.IsValidGuid(p.AssignmentId))
            {
                Guid labId = DB.GetLaboratoryIdFromOrderId(conn, trans, p.AssignmentId);
                if(Utils.IsValidGuid(labId))
                {
                    if(labId != Common.LabId)
                    {
                        btnPrepAnalPrepUpdate.Enabled = false;
                        btnPrepAnalPrepDiscard.Enabled = false;
                    }
                }
            }

            if (clearDirty)
                p.ClearDirty();
        }

        private void btnPrepAnalAnalUpdate_Click(object sender, EventArgs e)
        {
            if(!analysis.IsDirty())
            {                
                SetStatusMessage("Nothing to save for analysis " + preparation.Number + "/" + analysis.Number);
                return;
            }
            
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                if (analysis.IsClosed(conn, trans))
                {
                    MessageBox.Show("This analysis belongs to a closed order and can not be updated");
                    return;
                }                

                if(!Utils.IsValidGuid(cboxPrepAnalAnalUnit.SelectedValue))
                {
                    MessageBox.Show("Analysis unit is required");
                    return;
                }

                if((int)cboxPrepAnalAnalWorkflowStatus.SelectedValue == 0)
                {
                    MessageBox.Show("Analysis status is required");
                    return;
                }

                if (!String.IsNullOrEmpty(tbPrepAnalAnalSpecRef.Text.Trim()))
                {
                    SqlCommand cmd = new SqlCommand("select count(*) from analysis where specter_reference = @specref and id not in(@exId)", conn, trans);
                    cmd.Parameters.AddWithValue("@specref", tbPrepAnalAnalSpecRef.Text.Trim());
                    cmd.Parameters.AddWithValue("@exId", analysis.Id);

                    int cnt = (int)cmd.ExecuteScalar();
                    if (cnt > 0)
                    {
                        MessageBox.Show("The spectrum reference is already used");
                        return;
                    }
                }

                if (analysis.WorkflowStatusId == WorkflowStatus.Complete && (int)cboxPrepAnalAnalWorkflowStatus.SelectedValue == WorkflowStatus.Complete)
                {
                    MessageBox.Show("Can not save a completed analysis");
                    return;
                }

                if (preparation.WorkflowStatusId == WorkflowStatus.Rejected && (int)cboxPrepAnalAnalWorkflowStatus.SelectedValue != WorkflowStatus.Rejected)
                {
                    MessageBox.Show("Can not activate this analysis because the preparation is rejected");
                    return;
                }

                if (analysis.WorkflowStatusId != WorkflowStatus.Rejected && (int)cboxPrepAnalAnalWorkflowStatus.SelectedValue == WorkflowStatus.Rejected)
                {
                    SqlCommand cmd = new SqlCommand("select COUNT(*) from analysis_result where analysis_id = @aid and instance_status_id = 1 and (activity_approved = 1 or accredited = 1 or detection_limit_approved = 1)", conn, trans);
                    cmd.Parameters.AddWithValue("@aid", analysis.Id);
                    int n = (int)cmd.ExecuteScalar();
                    if (n > 0)
                    {
                        MessageBox.Show("Can not reject this analysis because one or more results are approved");
                        return;
                    }
                }                

                analysis.ActivityUnitId = Utils.MakeGuid(cboxPrepAnalAnalUnit.SelectedValue);
                analysis.ActivityUnitTypeId = Utils.MakeGuid(cboxPrepAnalAnalUnitType.SelectedValue);
                analysis.SpecterReference = tbPrepAnalAnalSpecRef.Text;
                analysis.Comment = tbPrepAnalAnalComment.Text;
                analysis.WorkflowStatusId = (int)cboxPrepAnalAnalWorkflowStatus.SelectedValue;

                analysis.StoreToDB(conn, trans);

                if (!String.IsNullOrEmpty(analysis.ImportFile) && File.Exists(analysis.ImportFile))
                {
                    try
                    {
                        DB.AddAttachment(conn, trans, "analysis", analysis.Id, Path.GetFileNameWithoutExtension(analysis.ImportFile), ".lis", File.ReadAllBytes(analysis.ImportFile));
                        analysis.ImportFile = String.Empty;
                        UI.PopulateAttachments(conn, trans, "analysis", analysis.Id, gridPrepAnalAnalAttachments);
                    }
                    catch { }
                }

                trans.Commit();

                treePrepAnal.SelectedNode.ForeColor = WorkflowStatus.GetStatusColor(analysis.WorkflowStatusId);
                SetStatusMessage("Analysis updated successfully");
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

        private void cboxPrepAnalPrepGeom_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblPrepAnalPrepRange.Text = "";            

            if (Utils.IsValidGuid(cboxPrepAnalPrepGeom.SelectedValue))
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    PreparationGeometry pg = new PreparationGeometry(conn, null, Utils.MakeGuid(cboxPrepAnalPrepGeom.SelectedValue));
                    lblPrepAnalPrepRange.Text = "[" + pg.MinFillHeightMM + ", " + pg.MaxFillHeightMM + "]";
                }
            }

            preparation.Dirty = true;
        }

        private void btnPrepAnalSampleUpdate_Click(object sender, EventArgs e)
        {            
            SqlConnection conn = null;

            try
            {
                double? lodStart = Utils.ToDouble(tbPrepAnalLODStartWeight.Text);
                double? lodEnd = Utils.ToDouble(tbPrepAnalLODEndWeight.Text);

                if (lodStart != null && lodEnd != null && lodStart < lodEnd)
                {
                    MessageBox.Show("LOD start weight cannot be smaller than end weight");
                    return;
                }

                conn = DB.OpenConnection();

                if (sample.IsClosed(conn, null))
                {
                    MessageBox.Show("This sample belongs to a closed order and can not be updated");
                    return;
                }

                sample.WetWeight_g = Utils.ToDouble(tbPrepAnalWetWeight.Text);
                sample.DryWeight_g = Utils.ToDouble(tbPrepAnalDryWeight.Text);
                sample.Volume_l = Utils.ToDouble(tbPrepAnalVolume.Text);
                sample.LodTemperature = Utils.ToDouble(tbPrepAnalLODTemp.Text);
                sample.LodWeightStart = lodStart;
                sample.LodWeightEnd = lodEnd;                

                sample.StoreLabInfoToDB(conn, null);

                SetStatusMessage("Lab data updated for sample " + sample.Number);
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

        private void PopulateSampleInfo(SqlConnection conn, SqlTransaction trans, Sample s, TreeNode tnode, bool clearDirty)
        {                        
            tnode.ToolTipText = "Component: " + sample.GetSampleComponentName(conn, trans) + Environment.NewLine
                + "External Id: " + sample.ExternalId + Environment.NewLine
                + "Project: " + sample.GetProjectName(conn, trans) + Environment.NewLine
                + "Reference date: " + sample.ReferenceDate.Value.ToString(Utils.DateTimeFormatNorwegian);
            
            tbPrepAnalInfoComment.Text = s.Comment;
            tbPrepAnalWetWeight.Text = s.WetWeight_g.ToString();
            tbPrepAnalDryWeight.Text = s.DryWeight_g.ToString();
            tbPrepAnalVolume.Text = s.Volume_l.ToString();
            tbPrepAnalLODStartWeight.Text = s.LodWeightStart.ToString();
            tbPrepAnalLODEndWeight.Text = s.LodWeightEnd.ToString();
            tbPrepAnalLODTemp.Text = s.LodTemperature.ToString();

            btnPrepAnalSampleUpdate.Enabled = true;
            btnPrepAnalSampleDiscard.Enabled = true;
            if (s.LaboratoryId != Common.LabId)
            {
                btnPrepAnalSampleUpdate.Enabled = false;
                btnPrepAnalSampleDiscard.Enabled = false;
            }

            if (clearDirty)
                s.ClearDirty();
        }

        private void miImportLISFile_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (analysis.IsClosed(conn, null))
                {
                    MessageBox.Show("This analysis belongs to a closed order and can not be updated");
                    return;
                }

                if (!DB.CanUserApproveAnalysis(conn, null, Common.UserId, analysis.AnalysisMethodId))
                {
                    MessageBox.Show("You are not allowed to approve results for this analysis method");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed analysis");
                return;
            }            

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "LIS files (*.lis)|*.lis";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            Analysis a = analysis.Clone();
            a.ImportFile = dialog.FileName;

            FormImportAnalysisLIS form = new FormImportAnalysisLIS(preparation, a);
            if (form.ShowDialog() != DialogResult.OK)            
                return;

            analysis = a.Clone();

            conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateAnalysis(conn, null, analysis, false);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Imported LIS file " + analysis.ImportFile + " for analysis");
        }

        private void PopulateAnalysis(SqlConnection conn, SqlTransaction trans, Analysis a, bool clearDirty)
        {
            cboxPrepAnalAnalUnit.SelectedValue = a.ActivityUnitId;
            cboxPrepAnalAnalUnitType.SelectedValue = a.ActivityUnitTypeId;
            cboxPrepAnalAnalWorkflowStatus.SelectedValue = a.WorkflowStatusId;
            tbPrepAnalAnalSpecRef.Text = a.SpecterReference;
            tbPrepAnalAnalNuclLib.Text = a.NuclideLibrary;
            tbPrepAnalAnalMDALib.Text = a.MDALibrary;
            tbPrepAnalAnalComment.Text = a.Comment;            

            PopulateAnalysisResults(a, clearDirty);

            UI.PopulateAttachments(conn, trans, "analysis", a.Id, gridPrepAnalAnalAttachments);

            btnPrepAnalAnalUpdate.Enabled = true;
            btnPrepAnalAnalDiscard.Enabled = true;

            if (Utils.IsValidGuid(a.AssignmentId))
            {
                Guid labId = DB.GetLaboratoryIdFromOrderId(conn, trans, a.AssignmentId);
                if (Utils.IsValidGuid(labId))
                {
                    if (labId != Common.LabId)
                    {
                        btnPrepAnalAnalUpdate.Enabled = false;
                        btnPrepAnalAnalDiscard.Enabled = false;
                    }
                }
            }

            if (clearDirty)
                a.ClearDirty();
        }

        private void PopulateAnalysisResults(Analysis a, bool clearDirty)
        {
            a.Results.Sort((r1, r2) => r1.NuclideName.CompareTo(r2.NuclideName));

            gridPrepAnalResults.Columns.Clear();
            gridPrepAnalResults.Rows.Clear();

            gridPrepAnalResults.Columns.Add("Id", "Id");
            gridPrepAnalResults.Columns.Add("NuclideName", "Nuclide");
            gridPrepAnalResults.Columns.Add("Activity", "Activity");
            gridPrepAnalResults.Columns.Add("ActivityUncertaintyABS", "Act.Unc.");
            DataGridViewCheckBoxColumn actApprCol = new DataGridViewCheckBoxColumn();
            actApprCol.Name = "ActivityApproved";
            actApprCol.HeaderText = "Act.Appr.";
            actApprCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridPrepAnalResults.Columns.Add(actApprCol);
            gridPrepAnalResults.Columns.Add("DetectionLimit", "MDA.");            
            DataGridViewCheckBoxColumn mdaApprCol = new DataGridViewCheckBoxColumn();
            mdaApprCol.Name = "DetectionLimitApproved";
            mdaApprCol.HeaderText = "MDA.Appr.";
            mdaApprCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridPrepAnalResults.Columns.Add(mdaApprCol);
            DataGridViewCheckBoxColumn accApprCol = new DataGridViewCheckBoxColumn();
            accApprCol.Name = "Accredited";
            accApprCol.HeaderText = "Accredited";
            accApprCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridPrepAnalResults.Columns.Add(accApprCol);            
            DataGridViewCheckBoxColumn repApprCol = new DataGridViewCheckBoxColumn();
            repApprCol.Name = "Reportable";
            repApprCol.HeaderText = "Reportable";
            repApprCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridPrepAnalResults.Columns.Add(repApprCol);

            foreach (AnalysisResult ar in a.Results)
            {
                gridPrepAnalResults.Rows.Add(new object[] {
                    ar.Id,
                    ar.NuclideName,
                    ar.Activity.ToString(Utils.ScientificFormat),
                    ar.ActivityUncertaintyABS.ToString(Utils.ScientificFormat),
                    ar.ActivityApproved,
                    ar.DetectionLimit.ToString(Utils.ScientificFormat),
                    ar.DetectionLimitApproved,
                    ar.Accredited,
                    ar.Reportable
                });
            }            

            gridPrepAnalResults.Columns["Id"].Visible = false;

            if (clearDirty)
                foreach (AnalysisResult ar in a.Results)
                    ar.Dirty = false;
        }

        public void CalculateLODPercent()
        {
            double lws, lwe;
            try
            {
                lws = Convert.ToDouble(tbPrepAnalLODStartWeight.Text);
                lwe = Convert.ToDouble(tbPrepAnalLODEndWeight.Text);
            }
            catch
            {
                tbPrepAnalLODWater.Text = "";
                return;
            }

            if (lws < lwe)
            {
                tbPrepAnalLODWater.Text = "";
                return;
            }

            double delta = lws - lwe;
            double percent = (delta / lws) * 100.0;
            tbPrepAnalLODWater.Text = percent.ToString("0.0#");
        }

        private void tbPrepAnalLODStartWeight_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
            CalculateLODPercent();
        }

        private void btnPrepAnalAddPrep_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("You must select the sample first");
                return;
            }

            FormPrepAnalAddPrep form = new FormPrepAnalAddPrep(sample);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulatePrepAnal(conn, sample);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
        }

        private void btnPrepAnalAddAnal_Click(object sender, EventArgs e)
        {
            if(treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("You must select a preparation first");
                return;
            }            

            FormPrepAnalAddAnal form = new FormPrepAnalAddAnal(preparation);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulatePrepAnal(conn, sample);
            }
        }

        private void btnSampleAddSampleToOrder_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to add samples to orders");
                return;
            }

            FormSelectOrder form = new FormSelectOrder(treeSampleTypes, sample.Id);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SetStatusMessage("Successfully added sample " + form.SelectedSampleNumber.ToString() + " to order " + form.SelectedOrderName);
        }

        private void btnSampleSamplingDateFrom_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleSamplingDateFrom.Tag = form.SelectedDateTime;
            tbSampleSamplingDateFrom.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);
        }

        private void btnSampleSamplingDateFromClear_Click(object sender, EventArgs e)
        {
            tbSampleSamplingDateTo.Tag = null;
            tbSampleSamplingDateTo.Text = "";

            tbSampleSamplingDateFrom.Tag = null;
            tbSampleSamplingDateFrom.Text = "";            
        }

        private void btnSampleSamplingDateToClear_Click(object sender, EventArgs e)
        {            
            tbSampleSamplingDateTo.Tag = null;
            tbSampleSamplingDateTo.Text = "";
        }

        private void btnSampleSamplingDateTo_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleSamplingDateTo.Tag = form.SelectedDateTime;
            tbSampleSamplingDateTo.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);            
        }

        private void btnSampleReferenceDateClear_Click(object sender, EventArgs e)
        {            
            tbSampleReferenceDate.Tag = null;
            tbSampleReferenceDate.Text = "";
        }

        private void btnSampleReferenceDate_Click(object sender, EventArgs e)
        {
            FormSelectDateTime form = new FormSelectDateTime();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleReferenceDate.Tag = form.SelectedDateTime;
            tbSampleReferenceDate.Text = form.SelectedDateTime.ToString(Utils.DateTimeFormatNorwegian);            
        }

        private void cboxSampleLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (!Utils.IsValidGuid(cboxSampleLaboratory.SelectedValue))            
                lblSampleToolLaboratory.Text = "";
            else            
                lblSampleToolLaboratory.Text = "[Laboratory] " + cboxSampleLaboratory.Text;
        }

        private void btnOrderClearDeadline_Click(object sender, EventArgs e)
        {
            tbOrderDeadline.Text = "";
            tbOrderDeadline.Tag = null;

            assignment.Dirty = true;
        }

        private void btnOrderSelectCustomer_Click(object sender, EventArgs e)
        {
            FormSelectCustomer form = new FormSelectCustomer(InstanceStatus.Deleted);
            if (form.ShowDialog() != DialogResult.OK)
                return;
            
            assignment.CustomerContactName = form.SelectedCustomer.ContactName;
            assignment.CustomerContactEmail = form.SelectedCustomer.ContactEmail;
            assignment.CustomerContactPhone = form.SelectedCustomer.ContactPhone;
            assignment.CustomerContactAddress = form.SelectedCustomer.ContactAddress;
            assignment.CustomerCompanyName = form.SelectedCustomer.CompanyName;
            assignment.CustomerCompanyEmail = form.SelectedCustomer.CompanyEmail;
            assignment.CustomerCompanyPhone = form.SelectedCustomer.CompanyPhone;
            assignment.CustomerCompanyAddress = form.SelectedCustomer.CompanyAddress;

            tbOrderCustomer.Text = assignment.CustomerContactName;
            tbOrderCustomerInfo.Text =
                assignment.CustomerContactName + Environment.NewLine +
                assignment.CustomerCompanyName + Environment.NewLine + Environment.NewLine +
                assignment.CustomerContactEmail + Environment.NewLine +
                assignment.CustomerContactPhone + Environment.NewLine + Environment.NewLine +
                assignment.CustomerContactAddress;

            assignment.Dirty = true;
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
            Guid orderSampleTypeId = Guid.Empty;
            
            miOrderAddPrepMeth.Enabled = false;
            btnOrderAddPrepMeth.Enabled = false;
            miOrderAddAnalMeth.Enabled = false;
            btnOrderAddAnalMeth.Enabled = false;

            switch (e.Node.Level)
            {
                case 0:
                    miOrderAddPrepMeth.Enabled = true;
                    btnOrderAddPrepMeth.Enabled = true;

                    orderSampleTypeId = Guid.Parse(e.Node.Name);
                    break;

                case 1:                    
                    miOrderAddAnalMeth.Enabled = true;
                    btnOrderAddAnalMeth.Enabled = true;

                    orderSampleTypeId = Guid.Parse(e.Node.Parent.Name);
                    break;

                case 2:
                    orderSampleTypeId = Guid.Parse(e.Node.Parent.Parent.Name);
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
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (analysis.IsClosed(conn, null))
                {
                    MessageBox.Show("This analysis belongs to a closed order and can not be updated");
                    return;
                }
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed analysis");
                return;
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Rejected)
            {
                MessageBox.Show("You can not edit a rejected analysis");
                return;
            }

            if (!Utils.IsValidGuid(analysis.ActivityUnitId))
            {
                MessageBox.Show("You must save a unit first");
                return;
            }

            if (gridPrepAnalResults.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single result first");
                return;
            }

            DataGridViewCell cell = null;
            try
            {
                cell = gridPrepAnalResults.SelectedRows[0].Cells["Id"];
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }

            if(cell.Value == null)
            {
                Common.Log.Error("Analysis result cell is null");
                MessageBox.Show("Analysis result cell is null");
                return;
            }

            Guid resultId = Guid.Parse(cell.Value.ToString());            

            FormPrepAnalResult form = new FormPrepAnalResult(analysis, resultId);
            if (form.ShowDialog() != DialogResult.OK)            
                return;            

            PopulateAnalysisResults(analysis, false);
        }

        private void btnPrepAnalAddResult_Click(object sender, EventArgs e)
        {
            if (analysis.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed analysis");
                return;
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Rejected)
            {
                MessageBox.Show("You can not edit a rejected analysis");
                return;
            }

            if (!Utils.IsValidGuid(analysis.ActivityUnitId))
            {
                MessageBox.Show("You must save a unit first");
                return;
            }            
            
            Dictionary<string, Guid> nuclides = null;
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (analysis.IsClosed(conn, null))
                {
                    MessageBox.Show("This analysis belongs to a closed order and can not be updated");
                    return;
                }

                nuclides = DB.GetNuclideNamesForAnalysisMethod(conn, null, analysis.AnalysisMethodId);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }
            
            foreach (AnalysisResult ar in analysis.Results)            
                nuclides.Remove(ar.NuclideName.ToUpper());            

            FormPrepAnalResult form = new FormPrepAnalResult(analysis, nuclides);
            if (form.ShowDialog() != DialogResult.OK)            
                return;

            conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateAnalysis(conn, null, analysis, false);
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

        private void cboxSampleInfoLocationTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if ((int)cboxSampleInfoLocationTypes.SelectedValue == 0)
            {
                tbSampleLocation.Text = "";
                tbSampleLocation.Enabled = false;
            }
            else
            {
                tbSampleLocation.Enabled = true;
            }
        }

        private void btnSampleGoToPrepAnal_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to preparations and analyses");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (!PopulatePrepAnal(conn, sample))
                    return;
            }   
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }   
            finally
            {
                conn?.Close();
            }      

            tabs.SelectedTab = tabPrepAnal;
        }

        private void tbSamplesLookup_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (String.IsNullOrEmpty(tbSamplesLookup.Text))
                return;

            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                if (!Utils.IsValidInteger(tbSamplesLookup.Text))
                {
                    tbSamplesLookup.Text = "";
                    e.Handled = true;
                    MessageBox.Show("Sample lookup must be a number");
                    return;
                }

                int selNum = -1;
                int snum = Convert.ToInt32(tbSamplesLookup.Text);

                btnSamplesClearFilters_Click(sender, e);

                if (gridSamples.SelectedRows.Count == 1)                
                    selNum = Convert.ToInt32(gridSamples.SelectedRows[0].Cells["number"].Value);

                if (snum == selNum)
                    miSamplesPrepAnal_Click(sender, e);
                else
                    PopulateSamplesSingle(snum);
                
                e.Handled = true;
                btnSamplesSearch.ForeColor = Color.Red;
            }
        }

        private void PopulateSamples()
        {
            string query = "select distinct ";
            if (cboxSamplesTop.SelectedValue != null)
                query += "top " + cboxSamplesTop.SelectedValue + " ";

            query += @"
		s.id,	
		s.number,
		s.external_id,
		l.name as 'laboratory_name',
		st.name as 'sample_type_name',	
		sc.name as 'sample_component_name',        
		pm.name + ' - ' + ps.name as 'project_name',
		ss.name as 'sample_storage_name',
		s.reference_date,
		insta.name as 'instance_status_name',
        (select name from cv_account where id = s.locked_id) as 'locked_name',
		(select number from sample where id = s.transform_from_id) as 'split_from',
		(select number from sample where id = s.transform_to_id) as 'merge_to',
		(select convert(varchar(80), number) + ', ' as 'data()' from sample where transform_to_id = s.id for XML PATH('')) as 'merge_from'
	from sample s 
		left outer join laboratory l on s.laboratory_id = l.id
		left outer join sample_type st on s.sample_type_id = st.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
		left outer join sample_component sc on s.sample_component_id = sc.id
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		inner join instance_status insta on s.instance_status_id = insta.id
        left outer join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
        left outer join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id
        left outer join assignment a on a.id = ast.assignment_id
	where 1=1
";
            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();

                SqlDataAdapter adapter = new SqlDataAdapter("", conn);

                if (Utils.IsValidGuid(cboxSamplesProjects.SelectedValue))
                {
                    query += " and pm.id = @project_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@project_id", cboxSamplesProjects.SelectedValue, Guid.Empty);
                }

                if (Utils.IsValidGuid(cboxSamplesProjectsSub.SelectedValue))
                {
                    query += " and ps.id = @project_sub_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@project_sub_id", cboxSamplesProjectsSub.SelectedValue, Guid.Empty);
                }

                if (Utils.IsValidGuid(cboxSamplesOrders.SelectedValue))
                {
                    query += " and a.id = @assignment_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@assignment_id", cboxSamplesOrders.SelectedValue, Guid.Empty);
                }

                if (Utils.IsValidGuid(cboxSamplesLaboratory.SelectedValue))
                {
                    query += " and l.id = @lab_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@lab_id", cboxSamplesLaboratory.SelectedValue, Guid.Empty);
                }

                if ((int)cboxSamplesStatus.SelectedValue != 0)
                {
                    query += " and s.instance_status_id = @instlev";
                    adapter.SelectCommand.Parameters.AddWithValue("@instlev", (int)cboxSamplesStatus.SelectedValue);
                }

                query += " order by s.number desc";
                
                adapter.SelectCommand.CommandText = query;
                adapter.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                gridSamples.DataSource = dt;

                gridSamples.Columns["id"].Visible = false;
                gridSamples.Columns["merge_to"].Visible = false;

                gridSamples.Columns["number"].HeaderText = "Sample number";
                gridSamples.Columns["external_id"].HeaderText = "Ex.Id";
                gridSamples.Columns["laboratory_name"].HeaderText = "Laboratory";
                gridSamples.Columns["sample_type_name"].HeaderText = "Type";
                gridSamples.Columns["sample_component_name"].HeaderText = "Component";
                gridSamples.Columns["project_name"].HeaderText = "Project";
                gridSamples.Columns["sample_storage_name"].HeaderText = "Storage";
                gridSamples.Columns["reference_date"].HeaderText = "Ref.date";
                gridSamples.Columns["instance_status_name"].HeaderText = "Status";
                gridSamples.Columns["locked_name"].HeaderText = "Locked by";
                gridSamples.Columns["split_from"].HeaderText = "Split from";
                gridSamples.Columns["merge_from"].HeaderText = "Merge from";

                gridSamples.Columns["reference_date"].DefaultCellStyle.Format = Utils.DateFormatNorwegian;
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Showing " + gridSamples.RowCount + " samples");

            if (Utils.IsValidGuid(sample.Id))
            {
                gridSamples.ClearSelection();
                foreach (DataGridViewRow row in gridSamples.Rows)
                {
                    Guid sid = Utils.MakeGuid(row.Cells["id"].Value);
                    if (sample.Id == sid)
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }

            ActiveControl = tbSamplesLookup;
        }

        private void PopulateSamplesSingle(int sampleNumber)
        {
            cboxSamplesProjects.SelectedValue = Guid.Empty;
            cboxSamplesProjectsSub.SelectedValue = Guid.Empty;
            cboxSamplesOrders.SelectedValue = Guid.Empty;
            cboxSamplesLaboratory.SelectedValue = Guid.Empty;            

            string query = @"
select distinct
		s.id,	
		s.number,
		s.external_id,
		l.name as 'laboratory_name',
		st.name as 'sample_type_name',	
		sc.name as 'sample_component_name',        
		pm.name + ' - ' + ps.name as 'project_name',
		ss.name as 'sample_storage_name',
		s.reference_date,
		insta.name as 'instance_status_name',
        (select name from cv_account where id = s.locked_id) as 'locked_name',
		(select number from sample where id = s.transform_from_id) as 'split_from',
		(select number from sample where id = s.transform_to_id) as 'merge_to',
		(select convert(varchar(80), number) + ', ' as 'data()' from sample where transform_to_id = s.id for XML PATH('')) as 'merge_from'
	from sample s 
		left outer join laboratory l on s.laboratory_id = l.id
		left outer join sample_type st on s.sample_type_id = st.id
		left outer join sample_storage ss on s.sample_storage_id = ss.id
		left outer join sample_component sc on s.sample_component_id = sc.id
		inner join project_sub ps on s.project_sub_id = ps.id
		inner join project_main pm on pm.id = ps.project_main_id
		inner join instance_status insta on s.instance_status_id = insta.id
        left outer join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
        left outer join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id
        left outer join assignment a on a.id = ast.assignment_id	
where s.number = @sample_number
";
            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);                

                adapter.SelectCommand.CommandText = query;
                adapter.SelectCommand.CommandType = CommandType.Text;
                adapter.SelectCommand.Parameters.AddWithValue("@sample_number", sampleNumber);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                gridSamples.DataSource = dt;

                gridSamples.Columns["id"].Visible = false;
                gridSamples.Columns["merge_to"].Visible = false;

                gridSamples.Columns["number"].HeaderText = "Sample number";
                gridSamples.Columns["external_id"].HeaderText = "Ex.Id";
                gridSamples.Columns["laboratory_name"].HeaderText = "Laboratory";
                gridSamples.Columns["sample_type_name"].HeaderText = "Type";
                gridSamples.Columns["sample_component_name"].HeaderText = "Component";
                gridSamples.Columns["project_name"].HeaderText = "Project";
                gridSamples.Columns["sample_storage_name"].HeaderText = "Storage";
                gridSamples.Columns["reference_date"].HeaderText = "Ref.date";
                gridSamples.Columns["instance_status_name"].HeaderText = "Status";
                gridSamples.Columns["locked_name"].HeaderText = "Locked by";
                gridSamples.Columns["split_from"].HeaderText = "Split from";
                gridSamples.Columns["merge_from"].HeaderText = "Merge from";

                gridSamples.Columns["reference_date"].DefaultCellStyle.Format = Utils.DateFormatNorwegian;
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            ActiveControl = tbSamplesLookup;
        }

        private void btnSamplesClearFilters_Click(object sender, EventArgs e)
        {            
            tbSamplesLookup.Text = "";
            cboxSamplesProjects.SelectedValue = Guid.Empty;
            cboxSamplesProjectsSub.SelectedValue = Guid.Empty;
            cboxSamplesOrders.SelectedValue = Guid.Empty;
            cboxSamplesStatus.SelectedValue = InstanceStatus.Active;
            cboxSamplesLaboratory.SelectedValue = Guid.Empty;
            cboxSamplesTop.SelectedValue = 50;
        }

        private void PopulateOrders()
        {
            string query = "select ";
            if(cboxOrdersTop.SelectedValue != null)            
                query += "top " + cboxOrdersTop.SelectedValue + " ";

            query += @"
		a.id,
		a.name,		
		l.name as 'laboratory_name',
		va.name as 'account_name',
		a.deadline,
		a.requested_sigma_act,
		a.requested_sigma_mda,        
		a.customer_contact_name,
        a.customer_company_name,
		a.approved_customer,
		a.approved_laboratory,			
        wf.name as 'workflow_status',
        (select name from cv_account where id = a.locked_id) as 'locked_name'
	from assignment a 		
		left outer join laboratory l on a.laboratory_id = l.id
		left outer join cv_account va on a.account_id = va.id
        inner join workflow_status wf on a.workflow_status_id = wf.id
	where 1 = 1
";
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlDataAdapter adapter = new SqlDataAdapter("", conn);

                if (Utils.IsValidGuid(cboxOrdersLaboratory.SelectedValue))
                {
                    query += " and l.id = @laboratory_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@laboratory_id", cboxOrdersLaboratory.SelectedValue, Guid.Empty);
                }

                if (!String.IsNullOrEmpty(cboxOrdersYear.Text))
                {
                    query += " and year(a.create_date) = @year";
                    adapter.SelectCommand.Parameters.AddWithValue("@year", Convert.ToInt32(cboxOrdersYear.Text));
                }

                int wstat = Convert.ToInt32(cboxOrdersWorkflowStatus.SelectedValue);
                if (wstat != 0)
                {
                    query += " and a.workflow_status_id = @workflow_status_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@workflow_status_id", wstat);
                }

                query += " order by a.create_date desc";

                adapter.SelectCommand.CommandText = query;
                adapter.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                gridOrders.DataSource = dt;

                gridOrders.Columns["id"].Visible = false;

                gridOrders.Columns["name"].HeaderText = "Name";
                gridOrders.Columns["laboratory_name"].HeaderText = "Laboratory";
                gridOrders.Columns["account_name"].HeaderText = "Responsible";
                gridOrders.Columns["requested_sigma_act"].HeaderText = "Req.Sig.Act.";
                gridOrders.Columns["requested_sigma_mda"].HeaderText = "Req.Sig.MDA";
                gridOrders.Columns["deadline"].HeaderText = "Deadline";                
                gridOrders.Columns["customer_contact_name"].HeaderText = "Contact";
                gridOrders.Columns["customer_company_name"].HeaderText = "Company";
                gridOrders.Columns["approved_customer"].HeaderText = "Appr.Cust";
                gridOrders.Columns["approved_laboratory"].HeaderText = "Appr.Lab";
                gridOrders.Columns["workflow_status"].HeaderText = "Status";                
                gridOrders.Columns["locked_name"].HeaderText = "Locked by";

                gridOrders.Columns["deadline"].DefaultCellStyle.Format = Utils.DateFormatNorwegian;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Showing " + gridOrders.RowCount + " orders");

            if (Utils.IsValidGuid(assignment.Id))
            {
                gridOrders.ClearSelection();
                foreach (DataGridViewRow row in gridOrders.Rows)
                {
                    Guid oid = Utils.MakeGuid(row.Cells["id"].Value);
                    if (assignment.Id == oid)
                    {
                        row.Selected = true;
                        break;
                    }
                }
            }
        }

        private void miOrdersClearAllFilters_Click(object sender, EventArgs e)
        {            
            cboxOrdersLaboratory.SelectedValue = Guid.Empty;
            cboxOrdersYear.Text = "";
            cboxOrdersWorkflowStatus.SelectedValue = 0;
            cboxOrdersTop.SelectedValue = 50;
        }

        private void btnOrderCreateReport_Click(object sender, EventArgs e)
        {            
            if(assignment.IsDirty())
            {
                MessageBox.Show("You must save changes first");
                return;
            }

            if(assignment.WorkflowStatusId != WorkflowStatus.Complete)
            {
                MessageBox.Show("Order must be saved as complete first");
                return;
            }

            if(!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to generate order reports");
                return;
            }

            if (assignment.LaboratoryId != Common.LabId)
            {
                MessageBox.Show("You don't have access to generate order reports for this laboratory");
                return;
            }

            FormReportAnalysisReport form = new FormReportAnalysisReport(assignment);
            form.ShowDialog();

            if (form.HasNewVersion)
            {
                SqlConnection conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    string attachmentName = "Report-" + assignment.Name + "-" + assignment.AnalysisReportVersion;
                    DB.AddAttachment(conn, null, "assignment", assignment.Id, attachmentName, ".pdf", form.ReportData);

                    UI.PopulateAttachments(conn, null, "assignment", assignment.Id, gridOrderAttachments);

                    SetStatusMessage("Added order attachment: " + attachmentName);
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
        }

        private void layoutPrepAnalAnal_Resize(object sender, EventArgs e)
        {
            cboxPrepAnalAnalUnitType.Width = panelPrepAnalAnalUnit.Width / 2;
        }                                

        private void btnSysUsersAddRoles_Click(object sender, EventArgs e)
        {
            if(!Roles.IsAdmin())
            {
                MessageBox.Show("You must log in as LIMSAdministrator to manage roles");
                return;
            }

            if(gridSysUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid userId = Guid.Parse(gridSysUsers.SelectedRows[0].Cells["id"].Value.ToString());
            List<Guid> existingRoles = new List<Guid>();
            foreach (Lemma<Guid, string> l in lbSysUsersRoles.Items)
                existingRoles.Add(l.Id);

            FormAccountXRoles form = new FormAccountXRoles(userId, existingRoles);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateRoles(conn, userId, lbSysUsersRoles);
            }

            string user = gridSysUsers.SelectedRows[0].Cells["name"].Value.ToString();
            SetStatusMessage("Added roles for user " + user);
        }

        private void gridMetaUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (gridSysUsers.SelectedRows.Count < 1)
                return;

            Guid uid = Guid.Parse(gridSysUsers.SelectedRows[0].Cells["id"].Value.ToString());

            if(Roles.IsAdmin() || uid == Common.UserId)
            {
                miResetPass.Enabled = true;
                btnMetaUsersResetPass.Enabled = true;
            }
            else
            {
                miResetPass.Enabled = false;
                btnMetaUsersResetPass.Enabled = false;
            }

            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateRoles(conn, uid, lbSysUsersRoles);
                UI.PopulateUserAnalMeths(conn, uid, gridSysUsersAnalMeth);
            }
        }

        private void btnSysUsersRemRoles_Click(object sender, EventArgs e)
        {
            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You must log in as LIMSAdministrator to manage roles");
                return;
            }

            if (gridSysUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid userId = Guid.Parse(gridSysUsers.SelectedRows[0].Cells["id"].Value.ToString());

            if (lbSysUsersRoles.SelectedItems.Count < 1)
            {
                MessageBox.Show("You must select one or more roles first");
                return;
            }

            string query = "delete from account_x_role where account_id = @account_id and role_id = @role_id";

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand(query, conn);
                foreach(Lemma<Guid, string> l in lbSysUsersRoles.SelectedItems)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@account_id", userId);
                    cmd.Parameters.AddWithValue("@role_id", l.Id);
                    cmd.ExecuteNonQuery();
                }

                UI.PopulateRoles(conn, userId, lbSysUsersRoles);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            string user = gridSysUsers.SelectedRows[0].Cells["name"].Value.ToString();
            SetStatusMessage("Removed roles for user " + user);
        }        

        private void tbSampleSamplingDateFrom_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            if (String.IsNullOrEmpty(tbSampleSamplingDateFrom.Text))
            {
                tbSampleSamplingDateTo.Tag = null;
                tbSampleSamplingDateTo.Text = "";
                tbSampleSamplingDateTo.Enabled = false;
                btnSampleSamplingDateTo.Enabled = false;
                btnSampleSamplingDateToClear.Enabled = false;
                
                DateTime now = DateTime.Now;
                tbSampleReferenceDate.Tag = now;
                tbSampleReferenceDate.Text = now.ToString(Utils.DateTimeFormatNorwegian);
                return;
            }

            tbSampleSamplingDateTo.Enabled = true;
            btnSampleSamplingDateTo.Enabled = true;
            btnSampleSamplingDateToClear.Enabled = true;

            DateTime sdf = (DateTime)tbSampleSamplingDateFrom.Tag;
            if(sdf.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Sampling time from must be earlier than current time");
                tbSampleSamplingDateFrom.Tag = null;
                tbSampleSamplingDateFrom.Text = "";
                return;
            }

            if (tbSampleSamplingDateTo.Tag == null)
            {            
                tbSampleReferenceDate.Tag = sdf;
                tbSampleReferenceDate.Text = sdf.ToString(Utils.DateTimeFormatNorwegian);
            }
            else
            {                
                DateTime sdt = (DateTime)tbSampleSamplingDateTo.Tag;
                if (sdf > sdt)
                {
                    MessageBox.Show("Sampling time to must be later than sampling date from");
                    tbSampleSamplingDateFrom.Tag = null;
                    tbSampleSamplingDateFrom.Text = "";
                    return;
                }
                long addTicks = (sdt.Ticks - sdf.Ticks) / 2;
                DateTime rd = new DateTime(sdf.Ticks + addTicks);
                tbSampleReferenceDate.Tag = rd;
                tbSampleReferenceDate.Text = rd.ToString(Utils.DateTimeFormatNorwegian);
            }
        }

        private void tbSampleSamplingDateTo_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;

            DateTime sdf;
            if (String.IsNullOrEmpty(tbSampleSamplingDateTo.Text))
            {
                if (tbSampleSamplingDateFrom.Tag != null)
                {
                    sdf = (DateTime)tbSampleSamplingDateFrom.Tag;
                    tbSampleReferenceDate.Tag = sdf;
                    tbSampleReferenceDate.Text = sdf.ToString(Utils.DateTimeFormatNorwegian);
                }
                else
                {
                    DateTime now = DateTime.Now;
                    tbSampleReferenceDate.Tag = now;
                    tbSampleReferenceDate.Text = now.ToString(Utils.DateTimeFormatNorwegian);
                }
                return;
            }

            DateTime sdt = (DateTime)tbSampleSamplingDateTo.Tag;
            if (sdt.Date > DateTime.Now.Date)
            {
                MessageBox.Show("Sampling time to must be earlier than current time");
                tbSampleSamplingDateTo.Tag = null;
                tbSampleSamplingDateTo.Text = "";
                return;
            }
            
            sdf = (DateTime)tbSampleSamplingDateFrom.Tag;
            if(sdf > sdt)
            {
                MessageBox.Show("Sampling time to must be later than sampling date from");
                tbSampleSamplingDateTo.Tag = null;
                tbSampleSamplingDateTo.Text = "";
                return;
            }
            long addTicks = (sdt.Ticks - sdf.Ticks) / 2;
            DateTime rd = new DateTime(sdf.Ticks + addTicks);
            tbSampleReferenceDate.Tag = rd;
            tbSampleReferenceDate.Text = rd.ToString(Utils.DateTimeFormatNorwegian);
        }

        private void tbSampleReferenceDate_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void btnSamplePrintSampleLabel_Click(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (!sample.HasRequiredFields())
                {
                    MessageBox.Show("Can not print label. Required fields for this sample must be saved first");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            List<Guid> sampleIds = new List<Guid>();
            sampleIds.Add(sample.Id);
            FormPrintSampleLabel form = new FormPrintSampleLabel(Common.Settings, sampleIds);
            form.ShowDialog();
        }

        private void tbMenuLookup_KeyPress(object sender, KeyPressEventArgs e)
        {            
            if (String.IsNullOrEmpty(tbMenuLookup.Text))            
                return;            

            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                if (!Utils.IsValidInteger(tbMenuLookup.Text))
                {
                    tbMenuLookup.Text = "";
                    e.Handled = true;
                    MessageBox.Show("Sample lookup must be a number");
                    return;
                }

                int snum = Convert.ToInt32(tbMenuLookup.Text);                
                tabs.SelectedTab = tabSamples;
                PopulateSamplesSingle(snum);                
                tbMenuLookup.Text = "";
                e.Handled = true;
                return;
            }
        }

        private void gridSamples_SelectionChanged(object sender, EventArgs e)
        {
            ActiveControl = tbSamplesLookup;
        }

        private void gridAttachments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.ShowAttachment(conn, e.RowIndex, grid);
            }
        }

        private void btnSampleScanAttachment_Click(object sender, EventArgs e)
        {
            FormScan form = new FormScan(Common.Settings);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "sample", sample.Id, form.DocumentName, ".pdf", form.PdfData);

                UI.PopulateAttachments(conn, null, "sample", sample.Id, gridSampleAttachments);

                SetStatusMessage("Added sample attachment: " + form.DocumentName);
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

        private void btnOrderScanAttachment_Click(object sender, EventArgs e)
        {
            FormScan form = new FormScan(Common.Settings);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "assignment", assignment.Id, form.DocumentName, ".pdf", form.PdfData);

                UI.PopulateAttachments(conn, null, "assignment", assignment.Id, gridOrderAttachments);

                SetStatusMessage("Added order attachment: " + form.DocumentName);
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

        private void btnPrepAnalPrepScanAttachment_Click(object sender, EventArgs e)
        {
            Guid prepId = Guid.Parse(treePrepAnal.SelectedNode.Name);

            FormScan form = new FormScan(Common.Settings);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "preparation", prepId, form.DocumentName, ".pdf", form.PdfData);

                UI.PopulateAttachments(conn, null, "preparation", prepId, gridPrepAnalPrepAttachments);

                SetStatusMessage("Added preparation attachment: " + form.DocumentName);
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

        private void btnPrepAnalAnalScanAttachment_Click(object sender, EventArgs e)
        {
            Guid analId = Guid.Parse(treePrepAnal.SelectedNode.Name);

            FormScan form = new FormScan(Common.Settings);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "analysis", analId, form.DocumentName, ".pdf", form.PdfData);

                UI.PopulateAttachments(conn, null, "analysis", analId, gridPrepAnalAnalAttachments);

                SetStatusMessage("Added analysis attachment: " + form.DocumentName);
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

        private void btnProjectScanAttachment_Click(object sender, EventArgs e)
        {
            if(gridProjectSub.SelectedRows.Count < 1)
            {
                MessageBox.Show("No sub-project selected");
                return;
            }

            Guid psid = Guid.Parse(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());

            FormScan form = new FormScan(Common.Settings);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "project_sub", psid, form.DocumentName, ".pdf", form.PdfData);

                UI.PopulateAttachments(conn, null, "project_sub", psid, gridProjectAttachments);

                SetStatusMessage("Added project attachment: " + form.DocumentName);
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

        private void gridProjectSub_SelectionChanged(object sender, EventArgs e)
        {
            if (gridProjectSub.SelectedRows.Count < 1)
            {
                gridProjectsUsers.DataSource = null;
                return;
            }

            Guid psid = Guid.Parse(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateUsersForProjectSub(conn, null, psid, gridProjectsUsers);

                UI.PopulateAttachments(conn, null, "project_sub", psid, gridProjectAttachments);
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

        private void miTypeRelSampleTypesNewRoot_Click(object sender, EventArgs e)
        {
            // New root sample type

            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

            FormSampleType form = new FormSampleType(treeSampleTypes, null, false);

            switch (form.ShowDialog())
            {
                case DialogResult.OK:
                    SetStatusMessage("Sample type " + form.SampleTypeName + " created");
                    using (SqlConnection conn = DB.OpenConnection())
                    {
                        DB.LoadSampleTypes(conn, null);
                        UI.PopulateSampleTypes(conn, treeSampleTypes);
                        UI.PopulateSampleTypes(treeSampleTypes, cboxSampleSampleType);
                    }
                    break;
                case DialogResult.Abort:
                    SetStatusMessage("Create sample type failed", StatusMessageType.Error);
                    break;
            }
        }

        private void miSamplesPrintSampleLabels_Click(object sender, EventArgs e)
        {
            if(gridSamples.SelectedRows.Count < 1)
            {
                MessageBox.Show("No samples selected");
                return;
            }

            List<Guid> sampleIds = new List<Guid>();

            foreach (DataGridViewRow row in gridSamples.SelectedRows)
            {
                Guid sid = Utils.MakeGuid(row.Cells["id"].Value);
                sampleIds.Add(sid);
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                foreach (Guid sid in sampleIds)
                {
                    if (!Sample.HasRequiredFields(conn, null, sid))
                    {
                        MessageBox.Show("Can not print labels. Required fields missing for one or more samples");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormPrintSampleLabel form = new FormPrintSampleLabel(Common.Settings, sampleIds);
            form.ShowDialog();
        }

        private void btnSampleBrowseAttachment_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*)|*";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            string fileExt = Path.GetExtension(dialog.FileName);
            byte[] content = File.ReadAllBytes(dialog.FileName);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "sample", sample.Id, fileName, fileExt, content);

                UI.PopulateAttachments(conn, null, "sample", sample.Id, gridSampleAttachments);

                SetStatusMessage("Added sample attachment: " + fileName);
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

        private void btnOrderBrowseAttachment_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*)|*";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            string fileExt = Path.GetExtension(dialog.FileName);
            byte[] content = File.ReadAllBytes(dialog.FileName);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "assignment", assignment.Id, fileName, fileExt, content);

                UI.PopulateAttachments(conn, null, "assignment", assignment.Id, gridOrderAttachments);

                SetStatusMessage("Added order attachment: " + fileName);
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

        private void btnProjectBrowseAttachment_Click(object sender, EventArgs e)
        {
            if(gridProjectSub.SelectedRows.Count < 1)
            {
                MessageBox.Show("No project selected");
                return;
            }

            Guid pid = Guid.Parse(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*)|*";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            string fileExt = Path.GetExtension(dialog.FileName);
            byte[] content = File.ReadAllBytes(dialog.FileName);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "project_sub", pid, fileName, fileExt, content);

                UI.PopulateAttachments(conn, null, "project_sub", pid, gridProjectAttachments);

                SetStatusMessage("Added project attachment: " + fileName);
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

        private void btnPrepAnalPrepBrowseAttachment_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("No sample selected");
                return;
            }

            if(treePrepAnal.SelectedNode.Level != 1)
            {
                MessageBox.Show("No preparation selected");
                return;
            }

            Guid pid = Guid.Parse(treePrepAnal.SelectedNode.Name);

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*)|*";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            string fileExt = Path.GetExtension(dialog.FileName);
            byte[] content = File.ReadAllBytes(dialog.FileName);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "preparation", pid, fileName, fileExt, content);

                UI.PopulateAttachments(conn, null, "preparation", pid, gridPrepAnalPrepAttachments);

                SetStatusMessage("Added preparation attachment: " + fileName);
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

        private void btnPrepAnalAnalBrowseAttachment_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("No sample selected");
                return;
            }

            if (treePrepAnal.SelectedNode.Level != 2)
            {
                MessageBox.Show("No analysis selected");
                return;
            }

            Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files(*)|*";
            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
            string fileExt = Path.GetExtension(dialog.FileName);
            byte[] content = File.ReadAllBytes(dialog.FileName);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.AddAttachment(conn, null, "analysis", aid, fileName, fileExt, content);

                UI.PopulateAttachments(conn, null, "analysis", aid, gridPrepAnalAnalAttachments);

                SetStatusMessage("Added analysis attachment: " + fileName);
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

        private void btnPrepAnalAnalDeleteAttachment_Click(object sender, EventArgs e)
        {
            if(gridPrepAnalAnalAttachments.SelectedRows.Count < 1)
            {
                MessageBox.Show("No attachment selected");
                return;
            }

            if (treePrepAnal.SelectedNode == null || treePrepAnal.SelectedNode.Level != 2)
            {
                MessageBox.Show("No analysis selected");
                return;
            }

            string attName = gridPrepAnalAnalAttachments.SelectedRows[0].Cells["name"].Value.ToString();
            DialogResult dr = MessageBox.Show("Are you sure you want to delete attachment '" + attName + "' ?", "Warning", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            Guid analId = Guid.Parse(treePrepAnal.SelectedNode.Name);
            Guid attId = Guid.Parse(gridPrepAnalAnalAttachments.SelectedRows[0].Cells["id"].Value.ToString());
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.DeleteAttachment(conn, null, "analysis", attId);

                UI.PopulateAttachments(conn, null, "analysis", analId, gridPrepAnalAnalAttachments);
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

        private void btnPrepAnalPrepDeleteAttachment_Click(object sender, EventArgs e)
        {
            if (gridPrepAnalPrepAttachments.SelectedRows.Count < 1)
            {
                MessageBox.Show("No attachment selected");
                return;
            }

            if (treePrepAnal.SelectedNode == null || treePrepAnal.SelectedNode.Level != 1)
            {
                MessageBox.Show("No preparation selected");
                return;
            }

            string attName = gridPrepAnalPrepAttachments.SelectedRows[0].Cells["name"].Value.ToString();
            DialogResult dr = MessageBox.Show("Are you sure you want to delete attachment '" + attName + "' ?", "Warning", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            Guid prepId = Guid.Parse(treePrepAnal.SelectedNode.Name);
            Guid attId = Guid.Parse(gridPrepAnalPrepAttachments.SelectedRows[0].Cells["id"].Value.ToString());
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.DeleteAttachment(conn, null, "preparation", attId);

                UI.PopulateAttachments(conn, null, "preparation", prepId, gridPrepAnalPrepAttachments);
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

        private void btnProjectDeleteAttachment_Click(object sender, EventArgs e)
        {
            if (gridProjectSub.SelectedRows.Count < 1)
            {
                MessageBox.Show("No project selected");
                return;
            }

            if (gridProjectAttachments.SelectedRows.Count < 1)
            {
                MessageBox.Show("No attachment selected");
                return;
            }

            string attName = gridProjectAttachments.SelectedRows[0].Cells["name"].Value.ToString();
            DialogResult dr = MessageBox.Show("Are you sure you want to delete attachment '" + attName + "' ?", "Warning", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            Guid projId = Guid.Parse(gridProjectSub.SelectedRows[0].Cells["id"].Value.ToString());
            Guid attId = Guid.Parse(gridProjectAttachments.SelectedRows[0].Cells["id"].Value.ToString());
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.DeleteAttachment(conn, null, "project_sub", attId);

                UI.PopulateAttachments(conn, null, "project_sub", projId, gridProjectAttachments);
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

        private void btnOrderDeleteAttachment_Click(object sender, EventArgs e)
        {
            if (gridOrderAttachments.SelectedRows.Count < 1)
            {
                MessageBox.Show("No attachment selected");
                return;
            }

            string attName = gridOrderAttachments.SelectedRows[0].Cells["name"].Value.ToString();
            DialogResult dr = MessageBox.Show("Are you sure you want to delete attachment '" + attName + "' ?", "Warning", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            Guid attId = Guid.Parse(gridOrderAttachments.SelectedRows[0].Cells["id"].Value.ToString());
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.DeleteAttachment(conn, null, "assignment", attId);

                UI.PopulateAttachments(conn, null, "assignment", assignment.Id, gridOrderAttachments);
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

        private void btnSampleDeleteAttachment_Click(object sender, EventArgs e)
        {
            if (gridSampleAttachments.SelectedRows.Count < 1)
            {
                MessageBox.Show("No attachment selected");
                return;
            }

            string attName = gridSampleAttachments.SelectedRows[0].Cells["name"].Value.ToString();
            DialogResult dr = MessageBox.Show("Are you sure you want to delete attachment '" + attName + "' ?", "Warning", MessageBoxButtons.YesNo);
            if (dr != DialogResult.Yes)
                return;

            Guid attId = Guid.Parse(gridSampleAttachments.SelectedRows[0].Cells["id"].Value.ToString());
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                DB.DeleteAttachment(conn, null, "sample", attId);

                UI.PopulateAttachments(conn, null, "sample", sample.Id, gridSampleAttachments);
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

        private void btnPrepAnalPrepPrintLabel_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null || treePrepAnal.SelectedNode.Level != 1)
            {
                MessageBox.Show("You must select a preparation first");
                return;
            }

            if(preparation.WorkflowStatusId != WorkflowStatus.Complete)
            {
                MessageBox.Show("Preparation must be saved with complete status before printing a label");
                return;
            }

            List<Guid> prepIds = new List<Guid>();
            prepIds.Add(preparation.Id);

            FormPrintPrepLabel form = new FormPrintPrepLabel(Common.Settings, prepIds);
            form.ShowDialog();
        }

        private void AddSampleTypeToXML(SqlConnection conn, SampleTypeModel stm, XmlDocument doc, XmlNode xnode)
        {
            XmlNode newXNode = doc.CreateElement("sampletype");
            XmlAttribute attrId = doc.CreateAttribute("id");
            attrId.InnerText = stm.Id.ToString();
            newXNode.Attributes.Append(attrId);
            XmlAttribute attrName = doc.CreateAttribute("name");
            attrName.InnerText = stm.Name;
            newXNode.Attributes.Append(attrName);
            XmlAttribute attrCommonName = doc.CreateAttribute("name_common");
            attrCommonName.InnerText = stm.NameCommon;
            newXNode.Attributes.Append(attrCommonName);
            XmlAttribute attrLatinName = doc.CreateAttribute("name_latin");
            attrLatinName.InnerText = stm.NameLatin;
            newXNode.Attributes.Append(attrLatinName);            

            using(SqlDataReader reader = DB.GetDataReader(conn, null, "select id, name from sample_component where sample_type_id = @stid", CommandType.Text, 
                new SqlParameter("@stid", stm.Id)))
            {
                while(reader.Read())
                {
                    XmlNode newCNode = doc.CreateElement("component");
                    XmlAttribute attrCompId = doc.CreateAttribute("id");
                    attrCompId.InnerText = reader.GetString("id");
                    newCNode.Attributes.Append(attrCompId);
                    XmlAttribute attrCompName = doc.CreateAttribute("name");
                    attrCompName.InnerText = reader.GetString("name");
                    newCNode.Attributes.Append(attrCompName);
                    newXNode.AppendChild(newCNode);
                }
            }

            xnode.AppendChild(newXNode);

            List<SampleTypeModel> stmNodes = Common.SampleTypeList.FindAll(x => x.ParentId == stm.Id);

            foreach (SampleTypeModel m in stmNodes)
            {
                AddSampleTypeToXML(conn, m, doc, newXNode);
            }
        }

        private void miTypeRelSampleTypesExportSampTypeXML_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "STX files (*.stx)|*.stx";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection connection = null;            

            try
            {
                connection = DB.OpenConnection();

                XmlDocument doc = new XmlDocument();

                XmlNode xroot = doc.CreateElement("sampletypes");
                doc.AppendChild(xroot);

                List<SampleTypeModel> roots = Common.SampleTypeList.FindAll(x => x.ParentId == Guid.Empty);

                foreach (SampleTypeModel m in roots)
                {
                    AddSampleTypeToXML(connection, m, doc, xroot);
                }

                doc.Save(dialog.FileName);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        private void miNuclidesExportNuclidesXML_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "NUX files (*.nux)|*.nux";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection connection = null;

            try
            {
                connection = DB.OpenConnection();

                XmlDocument doc = new XmlDocument();

                XmlNode xroot = doc.CreateElement("nuclides");
                doc.AppendChild(xroot);

                using (SqlDataReader reader = DB.GetDataReader(connection, null, "select * from nuclide", CommandType.Text))
                {
                    while (reader.Read())
                    {
                        XmlNode newXNode = doc.CreateElement("nuclide");

                        XmlAttribute attrId = doc.CreateAttribute("id");
                        attrId.InnerText = reader.GetString("id");
                        newXNode.Attributes.Append(attrId);

                        XmlAttribute attrZAS = doc.CreateAttribute("zas");
                        attrZAS.InnerText = reader.GetString("zas");
                        newXNode.Attributes.Append(attrZAS);

                        XmlAttribute attrName = doc.CreateAttribute("name");
                        attrName.InnerText = reader.GetString("name");
                        newXNode.Attributes.Append(attrName);

                        XmlAttribute attrProtons = doc.CreateAttribute("protons");
                        attrProtons.InnerText = reader.GetString("protons");
                        newXNode.Attributes.Append(attrProtons);

                        XmlAttribute attrNeutrons = doc.CreateAttribute("neutrons");
                        attrNeutrons.InnerText = reader.GetString("neutrons");
                        newXNode.Attributes.Append(attrNeutrons);

                        XmlAttribute attrMetaStable = doc.CreateAttribute("meta_stable");
                        attrMetaStable.InnerText = reader.GetString("meta_stable");
                        newXNode.Attributes.Append(attrMetaStable);

                        XmlAttribute attrHalfLife = doc.CreateAttribute("half_life_year");
                        attrHalfLife.InnerText = reader.GetDouble("half_life_year").ToString(Utils.ScientificFormat);
                        newXNode.Attributes.Append(attrHalfLife);

                        xroot.AppendChild(newXNode);
                    }
                }

                doc.Save(dialog.FileName);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }

        private void cboxPrepAnalAnalUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;            
        }

        private void cboxPrepAnalAnalUnitType_SelectedIndexChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;            
        }

        private void tbPrepAnalAnalSpecRef_TextChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;            
        }

        private void tbPrepAnalAnalNuclLib_TextChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;            
        }

        private void tbPrepAnalAnalMDALib_TextChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;            
        }

        private void tbPrepAnalAnalComment_TextChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;
        }

        private void cboxPrepAnalAnalWorkflowStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            analysis.Dirty = true;
        }

        private void tbPrepAnalPrepFillHeight_TextChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;            
        }

        private void tbPrepAnalPrepAmount_TextChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void cboxPrepAnalPrepAmountUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void tbPrepAnalPrepQuantity_TextChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void cboxPrepAnalPrepQuantityUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void tbPrepAnalPrepComment_TextChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void cboxPrepAnalPrepWorkflowStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            preparation.Dirty = true;
        }

        private void treePrepAnal_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (!DiscardUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }
        }

        private void btnPrepAnalRemoveResult_Click(object sender, EventArgs e)
        {
            if (gridPrepAnalResults.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select one or more results first");
                return;
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed analysis");
                return;
            }

            if (analysis.WorkflowStatusId == WorkflowStatus.Rejected)
            {
                MessageBox.Show("You can not edit a rejected analysis");
                return;
            }

            DialogResult r = MessageBox.Show("Are you sure you want to delete " + gridPrepAnalResults.SelectedRows.Count + " results from this analysis?", "Warning", MessageBoxButtons.YesNo);
            if (r == DialogResult.No)                
                return;

            foreach (DataGridViewRow row in gridPrepAnalResults.SelectedRows)
            {                
                Guid id = Utils.MakeGuid(row.Cells["Id"].Value);
                analysis.Results.RemoveAll(x => x.Id == id);            
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                PopulateAnalysis(conn, null, analysis, false);
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

            analysis.Dirty = true;
        }

        private void btnPrepAnalAnalDiscard_Click(object sender, EventArgs e)
        {
            if (!analysis.IsDirty())
            {
                SetStatusMessage("Nothing to discard for analysis " + preparation.Number + "/" + analysis.Number);
                return;
            }

            DialogResult res = MessageBox.Show("Are you sure you want to discard current changes?", "Confirmation", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                analysis.LoadFromDB(conn, null, analysis.Id);
                PopulateAnalysis(conn, null, analysis, true);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Changes discarded for analysis " + preparation.Number + "/" + analysis.Number);
        }

        private void btnPrepAnalPrepDiscard_Click(object sender, EventArgs e)
        {
            if (!preparation.IsDirty())
            {
                SetStatusMessage("Nothing to discard for preparation " + preparation.Number);
                return;
            }

            DialogResult res = MessageBox.Show("Are you sure you want to discard current changes?", "Confirmation", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                preparation.LoadFromDB(conn, null, preparation.Id);
                PopulatePreparation(conn, null, preparation, true);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Changes discarded for preparation " + preparation.Number);
        }

        private void miSamplesUnlock_Click(object sender, EventArgs e)
        {
            if (gridSamples.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a sample first");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("update sample set locked_id = @locked_id where id = @id", conn);

                foreach (DataGridViewRow row in gridSamples.SelectedRows)
                {
                    Guid sampleId = Utils.MakeGuid(row.Cells["id"].Value);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", sampleId);
                    cmd.ExecuteNonQuery();
                }

                PopulateSamples();
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

        private void miOrdersUnlock_Click(object sender, EventArgs e)
        {
            if (gridOrders.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select one or more samples first");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("update assignment set locked_id = @locked_id where id = @id", conn);

                foreach (DataGridViewRow row in gridOrders.SelectedRows)
                {
                    Guid orderId = Utils.MakeGuid(row.Cells["id"].Value);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_id", DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", orderId);
                    cmd.ExecuteNonQuery();
                }

                PopulateOrders();
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

        private void btnSamplesSearch_Click(object sender, EventArgs e)
        {                                    
            PopulateSamples();
            btnSamplesSearch.ForeColor = SystemColors.ControlText;
        }

        private void btnOrdersSearch_Click(object sender, EventArgs e)
        {
            PopulateOrders();
            btnOrdersSearch.ForeColor = SystemColors.ControlText;
        }

        private void cboxOrdersLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOrdersSearch.ForeColor = Color.Red;
        }

        private void cboxOrdersYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOrdersSearch.ForeColor = Color.Red;
        }

        private void cboxOrdersWorkflowStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOrdersSearch.ForeColor = Color.Red;
        }

        private void cboxOrdersTop_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOrdersSearch.ForeColor = Color.Red;
        }

        private void cboxSamplesProjectsSub_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void cboxSamplesTop_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void cboxSamplesOrders_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void cboxSamplesStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void cboxSamplesLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSamplesSearch.ForeColor = Color.Red;
        }

        private void btnSampleSelectCoords_Click(object sender, EventArgs e)
        {
            double? lat = null, lon = null;

            if (!String.IsNullOrEmpty(tbSampleInfoLatitude.Text) && !String.IsNullOrEmpty(tbSampleInfoLongitude.Text))
            {
                try
                {
                    lat = Convert.ToDouble(tbSampleInfoLatitude.Text);
                    lon = Convert.ToDouble(tbSampleInfoLongitude.Text);
                }
                catch { }
            }

            FormGetCoords form = new FormGetCoords(lat, lon);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            tbSampleInfoLatitude.Text = form.SelectedLatitude.ToString();
            tbSampleInfoLongitude.Text = form.SelectedLongitude.ToString();
        }

        private void btnPrepAnalShowAudit_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null)
                return;

            string title;

            switch (treePrepAnal.SelectedNode.Level)
            {
                case 0:
                    break;
                case 1:
                    Guid pid = Guid.Parse(treePrepAnal.SelectedNode.Name);
                    title = treePrepAnal.SelectedNode.Parent.Text + "     " + treePrepAnal.SelectedNode.Text;
                    ShowAuditLog("preparation", pid, title);
                    break;
                case 2:
                    Guid aid = Guid.Parse(treePrepAnal.SelectedNode.Name);
                    title = treePrepAnal.SelectedNode.Parent.Parent.Text + "     " + treePrepAnal.SelectedNode.Parent.Text + "     " + treePrepAnal.SelectedNode.Text;
                    ShowAuditLog("analysis", aid, title);
                    break;
            }
        }

        private void ShowAuditLog(string table, Guid id, string title)
        {            
            lblAuditLogTitle.Text = title;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                string query = "select create_date as 'Date', operation as 'Operation', comment as 'Comment', value as 'Object' from audit_log where source_table = @table and source_id = @id order by create_date desc";

                gridAuditLog.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new[] {
                    new SqlParameter("@table", table),
                    new SqlParameter("@id", id),
                });
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabAuditLog;
        }

        private void miOrderDiscard_Click(object sender, EventArgs e)
        {
            if (!assignment.IsDirty())
            {
                SetStatusMessage("Nothing to discard for order " + assignment.Name);
                return;
            }

            DialogResult res = MessageBox.Show("Are you sure you want to discard current changes?", "Confirmation", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                assignment.LoadFromDB(conn, null, assignment.Id);
                PopulateOrder(conn, null, assignment, true);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Changes discarded for order " + assignment.Name);
        }

        private void cboxOrderResponsible_SelectedIndexChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void cboxOrderRequestedSigma_SelectedIndexChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void cboxOrderRequestedSigmaMDA_SelectedIndexChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void tbOrderContentComment_TextChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void tbOrderReportComment_TextChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void SetOrderDetailsEnabledState(bool state)
        {
            cboxOrderLaboratory.Enabled = state;
            cboxOrderResponsible.Enabled = state;
            btnOrderSelectCustomer.Enabled = state;
            btnOrderClearDeadline.Enabled = state;
            btnOrderSelectDeadline.Enabled = state;
            cboxOrderRequestedSigma.Enabled = state;
            cboxOrderRequestedSigmaMDA.Enabled = state;
            tbOrderContentComment.ReadOnly = !state;
        }

        private void cbOrderApprovedLaboratory_CheckedChanged(object sender, EventArgs e)
        {            
            assignment.Dirty = true;
            SetOrderDetailsEnabledState(!cbOrderApprovedLaboratory.Checked);
            ddbOrderAdd.Enabled = ddbOrderEdit.Enabled = ddbOrderDel.Enabled = !cbOrderApprovedLaboratory.Checked;
            if (!cbOrderApprovedLaboratory.Checked)
                cbOrderApprovedCustomer.Checked = false;
        }

        private void cbOrderApprovedCustomer_CheckedChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void cboxOrderStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            assignment.Dirty = true;
        }

        private void cboxSampleSampleComponent_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cbSampleConfidential_CheckedChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cboxSampleInfoSampler_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cboxSampleInfoSamplingMeth_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbSampleInfoLatitude_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbSampleInfoLongitude_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbSampleInfoAltitude_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cboxSampleMunicipalities_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbSampleLocation_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cboxSampleSampleStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void cboxSampleInstanceStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbSampleComment_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void btnSampleDiscard_Click(object sender, EventArgs e)
        {
            if (!sample.IsDirty())
            {
                SetStatusMessage("Nothing to discard for sample " + sample.Number);
                return;
            }

            DialogResult res = MessageBox.Show("Are you sure you want to discard current changes?", "Confirmation", MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
                return;

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();                
                sample.LoadFromDB(conn, null, sample.Id);
                PopulateSample(conn, null, sample, true);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            SetStatusMessage("Changes discarded for sample " + sample.Number);
        }

        private void miAbout_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.ShowDialog();
        }

        private void tbPrepAnalWetWeight_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbPrepAnalDryWeight_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbPrepAnalVolume_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void tbPrepAnalLODTemp_TextChanged(object sender, EventArgs e)
        {
            sample.Dirty = true;
        }

        private void btnPrepAnalSampleDiscard_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateSampleInfo(conn, null, sample, treePrepAnal.Nodes[0], true);
            }
        }

        private void btnSysLabPrepMethAdd_Click(object sender, EventArgs e)
        {
            if(gridSysLab.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single laboratory first");
                return;
            }

            Guid lid = Guid.Parse(gridSysLab.SelectedRows[0].Cells["id"].Value.ToString());
            if(lid != Common.LabId)
            {
                MessageBox.Show("You can not add preparation methods for another laboratory");
                return;
            }

            List<Guid> existingPrepMeths = new List<Guid>();
            foreach (DataGridViewRow row in gridSysLabPrepMeth.Rows)
                existingPrepMeths.Add(Utils.MakeGuid(row.Cells["id"].Value));

            FormLabXPrepMeth form = new FormLabXPrepMeth(lid, existingPrepMeths);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateLabPrepMeths(conn, lid, gridSysLabPrepMeth);
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

        private void gridSysLab_SelectionChanged(object sender, EventArgs e)
        {
            if (gridSysLab.SelectedRows.Count < 1)
            {
                gridSysLabPrepMeth.DataSource = null;
                gridSysLabAnalMeth.DataSource = null;
                return;
            }

            Guid lid = Guid.Parse(gridSysLab.SelectedRows[0].Cells["id"].Value.ToString());

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateLabPrepMeths(conn, lid, gridSysLabPrepMeth);
                UI.PopulateLabAnalMeths(conn, lid, gridSysLabAnalMeth);
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

        private void btnSysLabPrepMethRemove_Click(object sender, EventArgs e)
        {
            if (gridSysLab.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a laboratory first");
                return;
            }

            Guid lid = Utils.MakeGuid(gridSysLab.SelectedRows[0].Cells["id"].Value);
            if (lid != Common.LabId)
            {
                MessageBox.Show("You can not remove preparation methods for another laboratory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("", conn, trans);

                foreach (DataGridViewRow row in gridSysLabPrepMeth.SelectedRows)
                {
                    Guid pmid = Utils.MakeGuid(row.Cells["id"].Value);

                    cmd.CommandText = "delete from laboratory_x_preparation_method where laboratory_id = @lab_id and preparation_method_id = @pm_id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@lab_id", lid);
                    cmd.Parameters.AddWithValue("@pm_id", pmid);
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
                
                UI.PopulateLabPrepMeths(conn, lid, gridSysLabPrepMeth);                
            }
            catch(Exception ex)
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

        private void btnSysLabAnalMethAdd_Click(object sender, EventArgs e)
        {
            if (gridSysLab.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single laboratory first");
                return;
            }

            Guid lid = Utils.MakeGuid(gridSysLab.SelectedRows[0].Cells["id"].Value);
            if (lid != Common.LabId)
            {
                MessageBox.Show("You can not add analysis methods for another laboratory");
                return;
            }

            List<Guid> existingAnalMeths = new List<Guid>();
            foreach (DataGridViewRow row in gridSysLabAnalMeth.Rows)
                existingAnalMeths.Add(Utils.MakeGuid(row.Cells["id"].Value));

            FormLabXAnalMeth form = new FormLabXAnalMeth(lid, existingAnalMeths);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateLabAnalMeths(conn, lid, gridSysLabAnalMeth);
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

        private void btnSysLabAnalMethRemove_Click(object sender, EventArgs e)
        {
            if (gridSysLab.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a laboratory first");
                return;
            }

            Guid lid = Utils.MakeGuid(gridSysLab.SelectedRows[0].Cells["id"].Value);
            if (lid != Common.LabId)
            {
                MessageBox.Show("You can not remove analysis methods for another laboratory");
                return;
            }

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("delete from laboratory_x_analysis_method where laboratory_id = @lab_id and analysis_method_id = @am_id", conn);
                foreach (DataGridViewRow row in gridSysLabAnalMeth.SelectedRows)
                {
                    Guid amid = Utils.MakeGuid(row.Cells["id"].Value);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@lab_id", lid);
                    cmd.Parameters.AddWithValue("@am_id", amid);
                    cmd.ExecuteNonQuery();
                }

                UI.PopulateLabAnalMeths(conn, lid, gridSysLabAnalMeth);
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

        private void btnSysUsersAnalMethAdd_Click(object sender, EventArgs e)
        {
            if (gridSysUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a user first");
                return;
            }

            Guid uid = Utils.MakeGuid(gridSysUsers.SelectedRows[0].Cells["id"].Value);

            FormUserXAnalMeth form = new FormUserXAnalMeth(uid);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateUserAnalMeths(conn, uid, gridSysUsersAnalMeth);
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

            string user = gridSysUsers.SelectedRows[0].Cells["name"].Value.ToString();
            SetStatusMessage("Added analysis methods for user " + user);
        }

        private void btnSysUsersAnalMethRemove_Click(object sender, EventArgs e)
        {
            if (gridSysUsers.SelectedRows.Count < 1)
                return;

            Guid uid = Utils.MakeGuid(gridSysUsers.SelectedRows[0].Cells["id"].Value);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("delete from account_x_analysis_method where account_id = @acc_id and analysis_method_id = @am_id", conn);
                foreach (DataGridViewRow row in gridSysUsersAnalMeth.SelectedRows)
                {
                    Guid amid = Utils.MakeGuid(row.Cells["id"].Value);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@acc_id", uid);
                    cmd.Parameters.AddWithValue("@am_id", amid);
                    cmd.ExecuteNonQuery();
                }

                UI.PopulateUserAnalMeths(conn, uid, gridSysUsersAnalMeth);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            string user = gridSysUsers.SelectedRows[0].Cells["name"].Value.ToString();
            SetStatusMessage("Removed analysis methods for user " + user);
        }

        private void btnPrepAnalDelAnal_Click(object sender, EventArgs e)
        {
            if(treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("You must select an analysis first");
                return;
            }            

            if(Utils.IsValidGuid(analysis.AssignmentId))
            {
                MessageBox.Show("You can not delete this analysis because it belongs to an order");
                return;
            }

            DialogResult r = MessageBox.Show("Are you sure you want to delete analysis " + treePrepAnal.SelectedNode.Text + "?", "Warning", MessageBoxButtons.YesNo);
            if (r == DialogResult.No)
                return;

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                string query = "update analysis_result set instance_status_id = @status where analysis_id = @aid";
                SqlCommand cmd = new SqlCommand(query, conn, trans);
                cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                cmd.Parameters.AddWithValue("@aid", analysis.Id);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "update analysis set instance_status_id = @status where id = @aid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                cmd.Parameters.AddWithValue("@aid", analysis.Id);                
                cmd.ExecuteNonQuery();

                trans.Commit();

                treePrepAnal.Nodes.Remove(treePrepAnal.SelectedNode);
            }
            catch(Exception ex)
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

        private void btnPrepAnalDelPrep_Click(object sender, EventArgs e)
        {
            if (treePrepAnal.SelectedNode == null)
            {
                MessageBox.Show("You must select a preparation first");
                return;
            }

            if (Utils.IsValidGuid(preparation.AssignmentId))
            {
                MessageBox.Show("You can not delete this preparation because it belongs to an order");
                return;
            }

            DialogResult r = MessageBox.Show("Are you sure you want to delete preparation " + treePrepAnal.SelectedNode.Text + "?", "Warning", MessageBoxButtons.YesNo);
            if (r == DialogResult.No)
                return;

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                TreeNode tnode = treePrepAnal.SelectedNode;
                SqlCommand cmd = new SqlCommand("", conn, trans);

                foreach (TreeNode tn in tnode.Nodes)
                {
                    Guid aid = Guid.Parse(tn.Name);
                    
                    cmd.CommandText = "update analysis_result set instance_status_id = @status where analysis_id = @aid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", aid);
                    cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "update analysis set instance_status_id = @status where id = @aid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@aid", aid);
                    cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "update preparation set instance_status_id = @status where id = @pid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@pid", preparation.Id);
                cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                cmd.ExecuteNonQuery();

                trans.Commit();

                treePrepAnal.Nodes.Remove(treePrepAnal.SelectedNode);
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

        private void tvOrderContent_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnOrderRemoveSampleFromOrder.Enabled = false;

            if (e.Node.Level == 1 && Roles.HasAccess(Role.LaboratoryAdministrator))
                btnOrderRemoveSampleFromOrder.Enabled = true;

            btnOrderGoToPrepAnal.Enabled = false;

            if(e.Node.Level >= 1 && Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
                btnOrderGoToPrepAnal.Enabled = true;

        }

        private void btnOrderRemoveSampleFromOrder_Click(object sender, EventArgs e)
        {
            if(!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have permission to remove samples from orders");
                return;
            }

            if (tvOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample first");
                return;
            }

            if(assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not remove samples from a completed order");
                return;
            }

            DialogResult r = MessageBox.Show("Are you sure you want to remove sample from this order?", "Warning", MessageBoxButtons.YesNo);
            if (r == DialogResult.No)
                return;

            Guid sid = Guid.Parse(tvOrderContent.SelectedNode.Name);

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("", conn, trans);

                TreeNode tnode = tvOrderContent.SelectedNode;
                foreach(TreeNode pnode in tnode.Nodes)
                {
                    Guid pid = Guid.Parse(pnode.Name);

                    foreach (TreeNode anode in pnode.Nodes)
                    {
                        Guid aid = Guid.Parse(anode.Name);
                        cmd.CommandText = @"
update analysis_result set instance_status_id = @status where id in (
    select ar.id from analysis_result ar
        inner join analysis a on a.id = ar.analysis_id and a.assignment_id = @assid and a.preparation_id = @pid
    where a.id = @aid
)";
                        cmd.Parameters.Clear();                        
                        cmd.Parameters.AddWithValue("@aid", aid);
                        cmd.Parameters.AddWithValue("@pid", pid);
                        cmd.Parameters.AddWithValue("@assid", assignment.Id);
                        cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = @"
update analysis set instance_status_id = @status 
where id = @aid and assignment_id = @assid and preparation_id = @pid
";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@aid", aid);
                        cmd.Parameters.AddWithValue("@pid", pid);
                        cmd.Parameters.AddWithValue("@assid", assignment.Id);
                        cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                        cmd.ExecuteNonQuery();
                    }
                    
                    cmd.CommandText = "update preparation set instance_status_id = @status where id = @pid and assignment_id = @assid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@pid", pid);
                    cmd.Parameters.AddWithValue("@assid", assignment.Id);
                    cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = @"
select ast.id from assignment_sample_type ast 
    inner join assignment a on a.id = ast.assignment_id and a.id = @assid
    inner join sample_x_assignment_sample_type sxast on sxast.assignment_sample_type_id = ast.id
    inner join sample s on s.id = sxast.sample_id and s.id = @sid
";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.Parameters.AddWithValue("@assid", assignment.Id);
                object o = cmd.ExecuteScalar();
                if(!Utils.IsValidGuid(o))                
                    throw new Exception("btnOrderRemoveSampleFromOrder_Click: No valid Guid found for sample " + sid.ToString() + " in AST");
                Guid astId = Guid.Parse(o.ToString());

                cmd.CommandText = "delete from sample_x_assignment_sample_type where sample_id = @sid and assignment_sample_type_id = @astid";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@sid", sid);
                cmd.Parameters.AddWithValue("@astid", astId);
                cmd.ExecuteNonQuery();

                trans.Commit();

                assignment.LoadFromDB(conn, trans, assignment.Id);
                PopulateOrder(conn, trans, assignment, true);
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

        private void btnOrderEditSampleType_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have permission to edit sample types from orders");
                return;
            }

            if (treeOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            if (assignment.WorkflowStatusId == WorkflowStatus.Complete)
            {
                MessageBox.Show("You can not edit a completed order");
                return;
            }

            if (assignment.ApprovedCustomer || assignment.ApprovedLaboratory)
            {
                MessageBox.Show("You can not edit an approved order");
                return;
            }

            TreeNode tnode = treeOrderContent.SelectedNode;
            if (tnode.Level != 0)
            {
                MessageBox.Show("You must select a top level sample type");
                return;
            }

            Guid astId = Guid.Parse(tnode.Name);
            string query = @"
select count(*) from sample s
    inner join sample_x_assignment_sample_type sxast on s.id = sxast.sample_id
    inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id and ast.id = @astid    
";
            int n;
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                n = (int)DB.GetScalar(conn, null, query, CommandType.Text, new SqlParameter("@astid", astId));
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormSelectAstCount form = new FormSelectAstCount(tnode.Text, n);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            AssignmentSampleType ast = assignment.SampleTypes.Find(x => x.Id == astId);
            if(ast != null)
            {
                conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    ast.SampleCount = form.SelectedCount;
                    ast.Dirty = true;
                    PopulateOrderContent(conn, null, assignment);
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
        }

        private void btnProjectSubPrint_Click(object sender, EventArgs e)
        {
            if(gridProjectSub.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a sub project first");
                return;
            }

            Guid pid = Utils.MakeGuid(gridProjectSub.SelectedRows[0].Cells["id"].Value);

            FormPrintProjectLabel form = new FormPrintProjectLabel(Common.Settings, pid);
            form.ShowDialog();
        }

        private void tabsTypeRel_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                if (tabsTypeRel.SelectedTab == tabTypeRelationsSampleTypes)
                {
                    UI.PopulateSampleTypes(conn, treeSampleTypes);
                }
                else if (tabsTypeRel.SelectedTab == tabTypeRelationsPrepMeth)
                {
                    UI.PopulatePreparationMethods(conn, gridTypeRelPrepMeth);
                }
                else if (tabsTypeRel.SelectedTab == tabTypeRelationsAnalMeth)
                {
                    UI.PopulateAnalysisMethods(conn, gridTypeRelAnalMeth);
                }
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

        private void btnOrdersAssignUsers_Click(object sender, EventArgs e)
        {
            if(gridOrders.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single order first");
                return;
            }

            if(!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have permission to assign users to orders");
                return;
            }
            
            Guid aid = Utils.MakeGuid(gridOrders.SelectedRows[0].Cells["id"].Value);
            string aname = gridOrders.SelectedRows[0].Cells["name"].Value.ToString();

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                Guid aLabId = Assignment.GetLaboratoryId(conn, null, aid);
                if(aLabId != Common.LabId)
                {
                    MessageBox.Show("Can not assign users to this order. You don't belong to the same laboratory");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            FormOrdersAssignUsers form = new FormOrdersAssignUsers(aid, aname);
            if (form.ShowDialog() != DialogResult.OK)
                return;
        }

        private void btnProjectsUsersAdd_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to add users to projects");
                return;
            }

            if (gridProjectSub.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select a sub project first");
                return;
            }

            Guid pid = Utils.MakeGuid(gridProjectSub.SelectedRows[0].Cells["id"].Value);
            List<Guid> existingUsers = new List<Guid>();
            foreach (DataGridViewRow row in gridProjectsUsers.Rows)
                existingUsers.Add(Utils.MakeGuid(row.Cells["id"].Value));

            FormProjectSubXUsers form = new FormProjectSubXUsers(pid, existingUsers);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateUsersForProjectSub(conn, null, pid, gridProjectsUsers);
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

        private void btnProjectsUsersRemove_Click(object sender, EventArgs e)
        {
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to remove users from projects");
                return;
            }

            if (gridProjectSub.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a sub project first");
                return;
            }

            if (gridProjectsUsers.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select one or more users first");
                return;
            }

            Guid psid = Utils.MakeGuid(gridProjectSub.SelectedRows[0].Cells["id"].Value);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlCommand cmd = new SqlCommand("delete from project_sub_x_account where project_sub_id = @psid and account_id = @aid", conn);
                foreach(DataGridViewRow row in gridProjectsUsers.SelectedRows)
                {
                    Guid aid = Utils.MakeGuid(row.Cells["id"].Value);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@psid", psid);
                    cmd.Parameters.AddWithValue("@aid", aid);
                    cmd.ExecuteNonQuery();
                }
            
                UI.PopulateUsersForProjectSub(conn, null, psid, gridProjectsUsers);
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

        private void btnSearchSearch_Click(object sender, EventArgs e)
        {
            if(!String.IsNullOrEmpty(tbSearchActMin.Text) && !Utils.IsValidDecimal(tbSearchActMin.Text))
            {
                MessageBox.Show("Activity minimum must be a number");
                return;
            }

            if (!String.IsNullOrEmpty(tbSearchActMax.Text) && !Utils.IsValidDecimal(tbSearchActMax.Text))
            {
                MessageBox.Show("Activity maximum must be a number");
                return;
            }

            string query = "select ";
            if ((int)cboxSearchMaxShown.SelectedValue != 0)
                query += "top " + cboxSearchMaxShown.SelectedValue + " ";

            query += @"
s.number as 'Sample', st.name as 'Sample type', p.number as 'Preparation', pws.Name as 'P.status', a.number as 'Analysis', aws.Name as 'A.status', n.name as 'Nuclide', ar.activity as 'Activity', au.name as 'Unit', aut.name as 'Unit type', ar.activity_uncertainty_abs as 'Act.Unc.', ar.detection_limit as 'MDA', ar.accredited as 'Acc.'
from analysis_result ar
    inner join analysis a on a.id = ar.analysis_id
    inner join preparation p on p.id = a.preparation_id
    inner join sample s on s.id = p.sample_id
    inner join sample_type st on st.id = s.sample_type_id
    inner join project_sub ps on s.project_sub_id = ps.id
	inner join project_main pm on pm.id = ps.project_main_id
    inner join nuclide n on n.id = ar.nuclide_id 
    left outer join activity_unit au on a.activity_unit_id = au.id
    left outer join activity_unit_type aut on a.activity_unit_type_id = aut.id
    left outer join workflow_status aws on aws.id = a.workflow_status_id
    left outer join workflow_status pws on pws.id = p.workflow_status_id
where ar.instance_status_id < 2
";

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                SqlDataAdapter adapter = new SqlDataAdapter("", conn);                

                if (Utils.IsValidGuid(cboxSearchSampleType.SelectedValue))
                {
                    string st = cboxSearchSampleType.Text;
                    string[] items = st.Split(new string[] { " -> " }, StringSplitOptions.RemoveEmptyEntries);

                    query += " and st.path like '" + items[1] + "%'";
                }

                if (Utils.IsValidGuid(cboxSearchProject.SelectedValue))
                {
                    query += " and pm.id = @project_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@project_id", cboxSearchProject.SelectedValue, Guid.Empty);
                }

                if (Utils.IsValidGuid(cboxSearchProjectSub.SelectedValue))
                {
                    query += " and ps.id = @project_sub_id";
                    adapter.SelectCommand.Parameters.AddWithValue("@project_sub_id", cboxSearchProjectSub.SelectedValue, Guid.Empty);
                }

                if (Utils.IsValidGuid(cboxSearchStations.SelectedValue))
                {
                    query += " and s.station_id = @stid";
                    adapter.SelectCommand.Parameters.AddWithValue("@stid", cboxSearchStations.SelectedValue);
                }

                if (Utils.IsValidGuid(cboxSearchNuclides.SelectedValue))
                {
                    query += " and n.id = @nid";
                    adapter.SelectCommand.Parameters.AddWithValue("@nid", cboxSearchNuclides.SelectedValue);
                }

                if (!String.IsNullOrEmpty(tbSearchActMin.Text))
                {
                    double actMin = Convert.ToDouble(tbSearchActMin.Text);
                    query += " and ar.activity >= " + actMin;
                }

                if (!String.IsNullOrEmpty(tbSearchActMax.Text))
                {
                    double actMax = Convert.ToDouble(tbSearchActMax.Text);
                    query += " and ar.activity <= " + actMax;
                }

                if(cbSearchActAppr.CheckState == CheckState.Checked)                
                    query += " and ar.activity_approved = 1";
                else if(cbSearchActAppr.CheckState == CheckState.Indeterminate)
                    query += " and ar.activity_approved = 0";

                if (cbSearchMDAAppr.CheckState == CheckState.Checked)
                    query += " and ar.detection_limit_approved = 1";
                else if (cbSearchMDAAppr.CheckState == CheckState.Indeterminate)
                    query += " and ar.detection_limit_approved = 0";

                if (cbSearchAccredited.CheckState == CheckState.Checked)
                    query += " and ar.accredited = 1";
                else if (cbSearchAccredited.CheckState == CheckState.Indeterminate)
                    query += " and ar.accredited = 0";

                query += " order by s.number, p.number, a.number, n.name";

                adapter.SelectCommand.CommandText = query;
                adapter.SelectCommand.CommandType = CommandType.Text;
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                gridSearchResult.DataSource = dt;

                searchIsDirty = false;
                SetStatusMessage("Search showing " + dt.Rows.Count + " results");
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

        private void btnSearchAssignedWork_Click(object sender, EventArgs e)
        {
            FormReportAssignedWork form = new FormReportAssignedWork(Common.LabId);
            form.ShowDialog();
        }

        private void btnOrdersPrepSummary_Click(object sender, EventArgs e)
        {
            if(gridOrders.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single order first");
                return;
            }

            Guid aid = Utils.MakeGuid(gridOrders.SelectedRows[0].Cells["id"].Value);

            FormReportPrepSummary form = new FormReportPrepSummary(aid);
            form.ShowDialog();
        }

        private void btnSampleParamAdd_Click(object sender, EventArgs e)
        {
            FormSampleParameter form = new FormSampleParameter(sample);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            PopulateSampleParameters(sample, false);

            SetStatusMessage("Added sample parameter to sample " + sample.Number);
        }

        private void btnSampleParamEdit_Click(object sender, EventArgs e)
        {
            if(gridSampleParameters.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single parameter first");
                return;
            }

            Guid spId = Utils.MakeGuid(gridSampleParameters.SelectedRows[0].Cells["id"].Value);

            FormSampleParameter form = new FormSampleParameter(sample, spId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            PopulateSampleParameters(sample, false);

            SetStatusMessage("Edited sample parameter for sample " + sample.Number);
        }

        private void btnSampleParamRemove_Click(object sender, EventArgs e)
        {
            if (gridSampleParameters.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single parameter first");
                return;
            }

            Guid spId = Utils.MakeGuid(gridSampleParameters.SelectedRows[0].Cells["id"].Value);

            sample.Parameters.RemoveAll(x => x.Id == spId);
            sample.Dirty = true;

            PopulateSampleParameters(sample, false);

            SetStatusMessage("Removed sample parameter for sample " + sample.Number);
        }

        private void btnSysSampParamNameNew_Click(object sender, EventArgs e)
        {
            if(!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have permission to add sample parameter types");
                return;
            }

            FormSampParamName form = new FormSampParamName();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleParameterNames(conn, gridSysSampParamNames);
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

            SetStatusMessage("Sample parameter created");
        }

        private void btnSysSampParamNameEdit_Click(object sender, EventArgs e)
        {
            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have permission to edit sample parameter types");
                return;
            }

            if (gridSysSampParamNames.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single parameter name first");
                return;
            }

            Guid spnId = Utils.MakeGuid(gridSysSampParamNames.SelectedRows[0].Cells["id"].Value);

            FormSampParamName form = new FormSampParamName(spnId);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateSampleParameterNames(conn, gridSysSampParamNames);
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

            SetStatusMessage("Sample parameter updated");
        }

        private void btnSysSampParamNameDelete_Click(object sender, EventArgs e)
        {
            if (!Roles.IsAdmin())
            {
                MessageBox.Show("You don't have permission to delete sample parameter types");
                return;
            }

            MessageBox.Show("Not implemented");
        }

        private void cboxSearchSampleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void cboxSearchNuclides_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void tbSearchActMin_TextChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void tbSearchActMax_TextChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void cbSearchActAppr_CheckStateChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void cbSearchMDAAppr_CheckStateChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void cbSearchAccredited_CheckStateChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void cboxSearchStations_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void miSamplesCopy_Click(object sender, EventArgs e)
        {
            // copy sample

            if (gridSamples.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single sample first");
                return;
            }                        

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                Guid sid = Utils.MakeGuid(gridSamples.SelectedRows[0].Cells["id"].Value);

                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                Guid newSid = Guid.NewGuid();
                Sample s = new Sample();
                s.LoadFromDB(conn, trans, sid);

                if (Common.LabId == Guid.Empty)
                {
                    if (Common.UserId != s.CreateId)
                    {
                        MessageBox.Show("Can not copy this sample. Sample does not belong to your user");
                        return;
                    }
                }

                DialogResult r = MessageBox.Show("Are you sure you want to create a new sample by copying sample " + s.Number + "?", "Warning", MessageBoxButtons.YesNo);
                if (r == DialogResult.No)
                    return;

                int oldNumber = s.Number;
                s.Id = newSid;
                s.Number = DB.GetNextSampleCount(conn, trans);
                s.ExternalId = "";
                s.TransformFromId = Guid.Empty;
                s.TransformToId = Guid.Empty;
                s.Comment = "";
                s.ImportedFrom = "";
                s.ImportedFromId = "";
                s.WetWeight_g = null;
                s.DryWeight_g = null;
                s.Volume_l = null;
                s.LodWeightStart = null;
                s.LodWeightEnd = null;
                s.LodTemperature = null;                
                s.InstanceStatusId = 1;
                s.LockedId = Guid.Empty;
                s.CreateDate = DateTime.Now;
                s.CreateId = Common.UserId;
                s.UpdateDate = DateTime.Now;
                s.UpdateId = Common.UserId;
                s.StoreToDB(conn, trans);
                trans.Commit();
                                
                PopulateSamples();
                SetStatusMessage("Created sample " + s.Number + " from " + oldNumber);
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

        private void btnOrderGoToPrepAnal_Click(object sender, EventArgs e)
        {
            // go to sample prep/anal

            if (!Roles.HasAccess(Role.LaboratoryAdministrator, Role.LaboratoryOperator))
            {
                MessageBox.Show("You don't have access to preparations and analyses");
                return;
            }

            if (!Utils.IsValidGuid(Common.LabId))
            {
                MessageBox.Show("You must be a member of a laboratory in order to access preparations and analyses");
                return;
            }

            if(tvOrderContent.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample, preparation or analysis first");
                return;
            }

            TreeNode tnode = tvOrderContent.SelectedNode;
            if(tnode.Level == 0)
            {
                MessageBox.Show("You must select a sample, preparation or analysis first");
                return;
            }

            while (tnode.Level > 1)
                tnode = tnode.Parent;

            Guid sid = Guid.Parse(tnode.Name);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                sample.LoadFromDB(conn, null, sid);

                if (!PopulatePrepAnal(conn, sample))
                {
                    MessageBox.Show("Unable to populate sample");
                    return;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                conn?.Close();
            }

            tabs.SelectedTab = tabPrepAnal;
            tabsPrepAnal.SelectedTab = tabPrepAnalSample;
        }

        private void btnOrderShowSampleSummary_Click(object sender, EventArgs e)
        {
            FormReportSampleSummary form = new FormReportSampleSummary(assignment.Id);
            form.ShowDialog();
        }

        private void miResultsShowMap_Click(object sender, EventArgs e)
        {
            List<int> idList = new List<int>();

            foreach (DataGridViewRow row in gridSearchResult.Rows)
            {
                int num = Convert.ToInt32(row.Cells["Sample"].Value);
                if (!idList.Contains(num))
                    idList.Add(num);
            }

            FormShowResultMap form = new FormShowResultMap(idList);
            form.ShowDialog();
        }

        private void cboxSearchSampleType_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cboxSearchSampleType.Text.Trim()))
            {
                cboxSearchSampleType.SelectedItem = Guid.Empty;                
                return;
            }

            if (!Utils.IsValidGuid(cboxSearchSampleType.SelectedValue))
            {
                cboxSearchSampleType.SelectedValue = Guid.Empty;
            }
        }

        private void cboxSearchProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxSearchProject.SelectedValue))
            {
                cboxSearchProjectSub.DataSource = null;
                return;
            }

            Guid projectId = Utils.MakeGuid(cboxSearchProject.SelectedValue);
            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();
                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }, cboxSearchProjectSub);
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

            searchIsDirty = true;
        }

        private void cboxSearchProjectSub_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchIsDirty = true;
        }

        private void miManual_Click(object sender, EventArgs e)
        {
            try
            {
                string manual = Common.InstallationDirectory + Path.DirectorySeparatorChar + "DSA-Lims_MANUAL.pdf";
                if (!File.Exists(manual))
                {
                    MessageBox.Show("Unable to find manual file: " + manual);
                    return;
                }

                Process.Start(manual);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private void miTypeRelSampleTypesPrepMethRemove_Click(object sender, EventArgs e)
        {
            // remove preparation methods from sample type
            if (!Roles.HasAccess(Role.LaboratoryAdministrator))
            {
                MessageBox.Show("You don't have access to manage sample types");
                return;
            }

            if (treeSampleTypes.SelectedNode == null)
            {
                MessageBox.Show("You must select a sample type first");
                return;
            }

            if (lbTypeRelSampTypePrepMeth.SelectedItems.Count < 1)
            {
                MessageBox.Show("You must select one or more preparation methods first");
                return;
            }

            Guid stid = Utils.MakeGuid(treeSampleTypes.SelectedNode.Name);

            SqlConnection conn = null;
            SqlTransaction trans = null;
            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                SqlCommand cmd = new SqlCommand("delete from sample_type_x_preparation_method where sample_type_id = @stid and preparation_method_id = @pmid", conn, trans);

                foreach (Lemma<Guid, string> l in lbTypeRelSampTypePrepMeth.SelectedItems)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@stid", stid);
                    cmd.Parameters.AddWithValue("@pmid", l.Id);
                    cmd.ExecuteNonQuery();
                }

                UI.PopulateSampleTypePrepMeth(conn, trans, treeSampleTypes.SelectedNode, lbTypeRelSampTypePrepMeth, lbTypeRelSampTypeInheritedPrepMeth);

                trans.Commit();
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

        private void miTypeRelSampleTypesCompRemove_Click(object sender, EventArgs e)
        {
            // remove sample type component            
            MessageBox.Show("Not implemented");
        }

        private void btnOrdersOrderSummary_Click(object sender, EventArgs e)
        {
            if (gridOrders.SelectedRows.Count != 1)
            {
                MessageBox.Show("You must select a single order first");
                return;
            }

            SqlConnection conn = null;

            try
            {
                conn = DB.OpenConnection();
                Guid aid = Utils.MakeGuid(gridOrders.SelectedRows[0].Cells["id"].Value);

                byte[] pdfData = UtilsPdf.CreateAssignmentPdfData(conn, null, aid);

                string path = Path.GetTempPath();
                string fileName = Guid.NewGuid().ToString() + "-dsalims.pdf";
                string filePath = Path.Combine(path, fileName);
                File.WriteAllBytes(filePath, pdfData);

                Process.Start(filePath);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(Utils.makeErrorMessage(ex.Message));
            }
            finally
            {
                conn?.Close();
            }
        }
    }
}
