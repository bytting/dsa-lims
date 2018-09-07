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

        List<DecayType> decayTypes = new List<DecayType>();

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
                decayTypes.Clear();                

                using (SqlDataReader reader = DB.GetDataReader(conn, "select id, name from decay_type order by name", CommandType.Text))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = reader["name"].ToString();
                        decayTypes.Add(new DecayType(id, name));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void PopulateSampleTypes(SqlConnection conn)
        {
            try
            {
                treeSampleTypes.Nodes.Clear();
                TreeNode root = treeSampleTypes.Nodes.Add(SampleTypesRootName, SampleTypesRootName);
                
                using (SqlDataReader reader = DB.GetDataReader(conn, "select id, name, in_use from sample_type order by name", CommandType.Text))
                {
                    while (reader.Read())
                    {
                        string sampleType = reader["name"].ToString();
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

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_main_projects_short", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        Guid id = new Guid(reader["id"].ToString());
                        string name = reader["name"].ToString();

                        TreeNode node = root.Nodes.Add(name, name);
                        node.Tag = id;
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
            gridMetaLab.DataSource = DB.GetDataTable(conn, "csp_select_laboratories", CommandType.StoredProcedure);

            // Set UI state
            gridMetaLab.Columns["id"].Visible = false;
            gridMetaLab.Columns["assignment_counter"].Visible = false;

            gridMetaLab.Columns["name"].HeaderText = "Name";
            gridMetaLab.Columns["name_prefix"].HeaderText = "Prefix";
            gridMetaLab.Columns["address"].HeaderText = "Address";
            gridMetaLab.Columns["email"].HeaderText = "Email";
            gridMetaLab.Columns["phone"].HeaderText = "Phone";
            gridMetaLab.Columns["comment"].HeaderText = "Comment";
            gridMetaLab.Columns["in_use"].HeaderText = "In use";
            gridMetaLab.Columns["create_date"].HeaderText = "Created";
            gridMetaLab.Columns["created_by"].HeaderText = "Created by";
            gridMetaLab.Columns["update_date"].HeaderText = "Updated";
            gridMetaLab.Columns["updated_by"].HeaderText = "Updated by";

            gridMetaLab.Columns["create_date"].DefaultCellStyle.Format = StrUtils.DateFormatNorwegian;
            gridMetaLab.Columns["update_date"].DefaultCellStyle.Format = StrUtils.DateFormatNorwegian;
        }

        private void PopulateUsers(SqlConnection conn)
        {
            // Set data source
            gridMetaUsers.DataSource = DB.GetDataTable(conn, "csp_select_users", CommandType.StoredProcedure);

            gridMetaUsers.Columns["password_hash"].Visible = false;

            // Set UI state
            gridMetaUsers.Columns["username"].HeaderText = "Username";
            gridMetaUsers.Columns["fullname"].HeaderText = "Name";
            gridMetaUsers.Columns["language_code"].HeaderText = "Language";
            gridMetaUsers.Columns["in_use"].HeaderText = "In use";
            gridMetaUsers.Columns["create_date"].HeaderText = "Created";            
            gridMetaUsers.Columns["update_date"].HeaderText = "Updated";

            gridMetaUsers.Columns["create_date"].DefaultCellStyle.Format = StrUtils.DateFormatNorwegian;
            gridMetaUsers.Columns["update_date"].DefaultCellStyle.Format = StrUtils.DateFormatNorwegian;
        }

        private void PopulateNuclides(SqlConnection conn)
        {
            // Set data source
            gridSysNuclides.DataSource = DB.GetDataTable(conn, "csp_select_nuclides_flat", CommandType.StoredProcedure);

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
            gridSysNuclides.Columns["decay_type"].HeaderText = "Decay type";
            gridSysNuclides.Columns["kxray_energy"].HeaderText = "KXray Energy";
            gridSysNuclides.Columns["fluorescence_yield"].HeaderText = "Fluorescence Yield";
        }

        private void PopulateEnergyLines(SqlConnection conn, Guid nid)
        {
            // Set data source
            SqlDataAdapter adapter = new SqlDataAdapter("csp_select_energy_lines_for_nuclide", conn);
            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            adapter.SelectCommand.Parameters.AddWithValue("@nuclide_id", nid);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            gridSysNuclideTrans.DataSource = dt;

            // Set UI state
            gridSysNuclideTrans.Columns["id"].Visible = false;            
            gridSysNuclideTrans.Columns["nuclide_id"].Visible = false;
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

                    SqlCommand cmd = new SqlCommand("select id, name, in_use from sample_component where sample_type_id = @ID order by name", conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            lbSampleTypesComponents.Items.Add(new SampleComponent(reader));
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
                                lbSampleTypesInheritedComponents.Items.Add(new SampleComponent(reader));
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
            if (!tabsSample.TabPages.ContainsKey(tabSamplesResult.Name))
                tabsSample.TabPages.Add(tabSamplesResult);
        }

        private void btnSamplesOpen_Click(object sender, EventArgs e)
        {
            tabsSample.TabPages.Clear();
            tabsSample.TabPages.Add(tabSamplesInfo);
            tabsSample.TabPages.Add(tabSamplesParams);
            tabsSample.TabPages.Add(tabSamplesPrep);
            tabsSample.TabPages.Add(tabSamplesResult);
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
            FormLaboratory form = new FormLaboratory(false);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into laboratory (id,name,name_prefix,address,email,phone,assignment_counter,comment,in_use,create_date,created_by,update_date,updated_by) values(@id,@name,@name_prefix,@address,@email,@phone,@assignment_counter,@comment,@in_use,@create_date,@created_by,@update_date,@updated_by)", conn);
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@name", form.Label);
                    cmd.Parameters.AddWithValue("@name_prefix", form.Prefix);
                    cmd.Parameters.AddWithValue("@address", form.Address);
                    cmd.Parameters.AddWithValue("@email", form.Email);
                    cmd.Parameters.AddWithValue("@phone", form.Phone);
                    cmd.Parameters.AddWithValue("@assignment_counter", 1);
                    cmd.Parameters.AddWithValue("@comment", form.Comment);
                    cmd.Parameters.AddWithValue("@in_use", form.InUse);
                    cmd.Parameters.Add("@create_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.Add("@update_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);
                    cmd.ExecuteNonQuery();

                    PopulateLaboratories(conn);
                }
                lblStatus.Text = StrUtils.makeStatusMessage("Laboratory " + form.Label + " created");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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

            if(gridMetaLab.SelectedRows.Count < 1)
            {
                MessageBox.Show("No laboratory selected");
                return;
            }

            // edit laboratory

            DataGridViewRow row = gridMetaLab.SelectedRows[0];

            FormLaboratory form = new FormLaboratory(true);
            form.SetValues(
                row.Cells["name"].Value.ToString(), 
                row.Cells["name_prefix"].Value.ToString(), 
                row.Cells["address"].Value.ToString(), 
                row.Cells["email"].Value.ToString(), 
                row.Cells["phone"].Value.ToString(), 
                Convert.ToBoolean(row.Cells["in_use"].Value),
                row.Cells["comment"].Value.ToString());
                        
            if (form.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("update laboratory set name=@name,name_prefix=@name_prefix,address=@address,email=@email,phone=@phone,comment=@comment,in_use=@in_use,update_date=@update_date,updated_by=@updated_by where id=@id", conn);
                    cmd.Parameters.AddWithValue("@id", row.Cells["id"].Value.ToString());
                    cmd.Parameters.AddWithValue("@name", form.Label);
                    cmd.Parameters.AddWithValue("@name_prefix", form.Prefix);
                    cmd.Parameters.AddWithValue("@address", form.Address);
                    cmd.Parameters.AddWithValue("@email", form.Email);
                    cmd.Parameters.AddWithValue("@phone", form.Phone);
                    cmd.Parameters.AddWithValue("@comment", form.Comment);
                    cmd.Parameters.AddWithValue("@in_use", form.InUse);
                    cmd.Parameters.Add("@update_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);
                    cmd.ExecuteNonQuery();

                    PopulateLaboratories(conn);
                }
                lblStatus.Text = StrUtils.makeStatusMessage("Laboratory " + form.Label + " updated");
            }
            catch (Exception ex)
            {
                log.Error(ex);
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
            if (form.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("insert into project (id,name,comment,in_use,create_date,created_by,update_date,updated_by) values(@id,@name,@comment,@in_use,@create_date,@created_by,@update_date,@updated_by)", conn);
                    cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                    cmd.Parameters.AddWithValue("@name", form.ProjectName);
                    cmd.Parameters.AddWithValue("@comment", form.Comment);
                    cmd.Parameters.AddWithValue("@in_use", form.InUse);
                    cmd.Parameters.Add("@create_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.Add("@update_date", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);
                    cmd.ExecuteNonQuery();

                    PopulateProjects(conn);
                }
                lblStatus.Text = StrUtils.makeStatusMessage("Main project " + form.ProjectName + " created");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }            
        }

        private void miProjectsSubNew_Click(object sender, EventArgs e)
        {
            // new sub project
        }

        private void miProjectsEdit_Click(object sender, EventArgs e)
        {
            // edit project
        }

        private void miProjectsDelete_Click(object sender, EventArgs e)
        {
            // delete project
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
                cmd.Parameters.AddWithValue("@ID", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return;

                    reader.Read();

                    tbProjectsName.Text = reader["name"].ToString();
                    tbProjectsComment.Text = reader["comment"].ToString();
                    cbProjectsInUse.Checked = Convert.ToBoolean(reader["in_use"]);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void miNuclidesNew_Click(object sender, EventArgs e)
        {            
            FormNuclide form = new FormNuclide(log, decayTypes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateNuclides(conn);
            }
                            
            lblStatus.Text = StrUtils.makeStatusMessage("Nuclide " + form.Nuclide.Name + " inserted");
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

            FormNuclide form = new FormNuclide(log, decayTypes, nid);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateNuclides(conn);
            }

            lblStatus.Text = StrUtils.makeStatusMessage("Nuclide " + form.Nuclide.Name + " updated");
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
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateEnergyLines(conn, nid);
            }

            lblStatus.Text = StrUtils.makeStatusMessage("Energy line for " + nname + " inserted");
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
            if (form.ShowDialog() != DialogResult.OK)
                return;

            using (SqlConnection conn = DB.OpenConnection())
            {
                PopulateEnergyLines(conn, nid);
            }

            lblStatus.Text = StrUtils.makeStatusMessage("Energy line for " + nname + " updated");
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
            {
                PopulateEnergyLines(conn, nid);
            }
        }
    }    
}
