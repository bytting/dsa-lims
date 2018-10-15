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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public static partial class UI    
    {
        public static void PopulateInstanceStatus(params ComboBox[] cbn)
        {
            foreach (ComboBox cb in cbn)
            {
                cb.DataSource = Common.InstanceStatusList;
                cb.SelectedIndex = -1;
            }
        }

        public static void PopulatePreparationUnits(params ComboBox[] cbn)
        {
            foreach (ComboBox cb in cbn)
            {
                cb.DataSource = Common.PreparationUnitList;
                cb.SelectedIndex = -1;
            }
        }        

        public static void PopulateWorkflowStatus(params ComboBox[] cbn)
        {
            foreach(ComboBox cb in cbn)
            {
                cb.DataSource = Common.WorkflowStatusList;
                cb.SelectedIndex = -1;
            }            
        }

        public static void PopulateLocationTypes(params ComboBox[] cbn)
        {
            foreach (ComboBox cb in cbn)
            {
                cb.DataSource = Common.LocationTypeList;
                cb.SelectedIndex = -1;
            }
        }

        public static void PopulateActivityUnits(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_activity_units_flat", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Unit name";
            grid.Columns["convert_factor"].HeaderText = "Conv. fact.";
            grid.Columns["uniform_activity_unit_name"].HeaderText = "Uniform unit";
        }

        public static void PopulateActivityUnits(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_activity_units_short", CommandType.StoredProcedure))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateActivityUnitTypes(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_activity_unit_types", CommandType.StoredProcedure))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateProjectsMain(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_projects_main_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";            
        }

        public static void PopulateProjectsMain(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_projects_main_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach(ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())                
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateProjectsSub(SqlConnection conn, DataGridView grid, Guid project_main_id)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_projects_sub_flat", CommandType.StoredProcedure,            
                new SqlParameter("@project_main_id", project_main_id),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
            );

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["project_main_name"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateProjectsSub(SqlConnection conn, Guid project_main_id, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_projects_sub_short", CommandType.StoredProcedure,
                new SqlParameter("@project_main_id", project_main_id),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateLaboratories(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, "csp_select_laboratories_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["assignment_counter"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_prefix"].HeaderText = "Prefix";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateLaboratories(SqlConnection conn, int instanceStatusLevel, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_laboratories_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel)))
            {
                foreach(ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateUsers(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, "csp_select_accounts_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));

            grid.Columns["password_hash"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_date"].Visible = false;
        
            grid.Columns["username"].HeaderText = "Username";
            grid.Columns["fullname"].HeaderText = "Name";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["laboratory_name"].HeaderText = "Laboratory";
            grid.Columns["language_code"].HeaderText = "Language";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateUsers(SqlConnection conn, Guid laboratoryId, int instanceStatusLevel, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_accounts_for_laboratory", CommandType.StoredProcedure,
                new SqlParameter("@laboratory_id", laboratoryId),
                new SqlParameter("@instance_status_level", instanceStatusLevel)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<string, string>(reader["username"].ToString(), reader["fullname"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateNuclides(SqlConnection conn, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, "csp_select_nuclides_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["proton_count"].HeaderText = "Protons";
            grid.Columns["neutron_count"].HeaderText = "Neutrons";
            grid.Columns["half_life_year"].HeaderText = "T 1/2 (Years)";
            grid.Columns["half_life_uncertainty"].HeaderText = "T 1/2 Unc. (Years)";
            grid.Columns["decay_type_name"].HeaderText = "Decay type";
            grid.Columns["kxray_energy"].HeaderText = "KXray Energy";
            grid.Columns["fluorescence_yield"].HeaderText = "Fluorescence Yield";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateEnergyLines(SqlConnection conn, Guid nid, DataGridView grid)
        {            
            grid.DataSource = DB.GetDataTable(conn, "csp_select_nuclide_transmissions_for_nuclide_flat", CommandType.StoredProcedure,
                new SqlParameter("@nuclide_id", nid),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["nuclide_name"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["transmission_from"].HeaderText = "Tr. from";
            grid.Columns["transmission_to"].HeaderText = "Tr. to";
            grid.Columns["energy"].HeaderText = "Energy";
            grid.Columns["energy_uncertainty"].HeaderText = "Energy Unc.";
            grid.Columns["intensity"].HeaderText = "Intensity";
            grid.Columns["intensity_uncertainty"].HeaderText = "Intensity Unc.";
            grid.Columns["probability_of_decay"].HeaderText = "POD";
            grid.Columns["probability_of_decay_uncertainty"].HeaderText = "POD Unc.";
            grid.Columns["total_internal_conversion"].HeaderText = "TIC conv.";
            grid.Columns["kshell_conversion"].HeaderText = "KShell conv.";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateGeometries(SqlConnection conn, DataGridView grid)
        {            
            grid.DataSource = DB.GetDataTable(conn, "csp_select_preparation_geometries_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
            // FIXME
        }

        public static void PopulateGeometries(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_preparation_geometries_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateCounties(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_counties_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["county_number"].HeaderText = "Number";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateCounties(SqlConnection conn, params ComboBox[] cbn)
        {            
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_counties_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }                
        }

        public static void PopulateMunicipalities(SqlConnection conn, Guid cid, DataGridView grid)
        {            
            grid.DataSource = DB.GetDataTable(conn, "csp_select_municipalities_for_county_flat", CommandType.StoredProcedure,
                new SqlParameter("@county_id", cid),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["county_name"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["municipality_number"].HeaderText = "Number";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateMunicipalities(SqlConnection conn, Guid cid, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_municipalities_for_county_short", CommandType.StoredProcedure,
                new SqlParameter("@county_id", cid),
                new SqlParameter("@instance_status_level", InstanceStatus.Active)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateStations(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_stations_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["latitude"].HeaderText = "Latitude";
            grid.Columns["longitude"].HeaderText = "Longitude";
            grid.Columns["altitude"].HeaderText = "Altitude";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateStations(SqlConnection conn, params ComboBox[] cbn)
        {            
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_stations_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateSampleStorage(SqlConnection conn, DataGridView grid)
        {         
            grid.DataSource = DB.GetDataTable(conn, "csp_select_sample_storages_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateSampleStorage(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_storages_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateSamplers(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_samplers_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateSamplers(SqlConnection conn, params ComboBox[] cbn)
        {            
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_samplers_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }                
        }

        public static void PopulateSamplingMethods(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_sampling_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateSamplingMethods(SqlConnection conn, params ComboBox[] cbn)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sampling_methods_short", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
            {
                foreach (ComboBox cb in cbn)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbn)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbn)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateSamples(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_samples_informative_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;

            grid.Columns["number"].HeaderText = "Sample id";
            grid.Columns["external_id"].HeaderText = "Ex.Id";
            grid.Columns["laboratory_name"].HeaderText = "Laboratory";
            grid.Columns["sample_type_name"].HeaderText = "Type";
            grid.Columns["sample_component_name"].HeaderText = "Component";
            grid.Columns["project_name"].HeaderText = "Project";
            grid.Columns["sample_storage_name"].HeaderText = "Storage";
            grid.Columns["current_order_name"].HeaderText = "Order";
            grid.Columns["reference_date"].HeaderText = "Ref.date";
            grid.Columns["instance_status_name"].HeaderText = "Status";

            grid.Columns["reference_date"].DefaultCellStyle.Format = StrUtils.DateTimeFormatNorwegian;
        }

        public static void PopulatePreparationMethods(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_preparation_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["description_link"].HeaderText = "Desc. link";
            grid.Columns["destructive"].HeaderText = "Destructive";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateAnalysisMethods(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_analysis_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["description_link"].HeaderText = "Desc. link";
            grid.Columns["specter_reference_regexp"].HeaderText = "Spec.Ref RegExp";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        private static void AddSampleTypeChildren(List<Lemma<Guid, Guid, string>> sampleTypeList, TreeNode tnode)
        {
            List<Lemma<Guid, Guid, string>> children = sampleTypeList.FindAll(x => x.ParentId == new Guid(tnode.Name));
            foreach (Lemma<Guid, Guid, string> st in children)
            {
                TreeNode n = tnode.Nodes.Add(st.Id.ToString(), st.Name);
                n.ToolTipText = n.FullPath;
                AddSampleTypeChildren(sampleTypeList, n);
            }
        }

        public static void PopulateSampleTypes(SqlConnection conn, TreeView tree)
        {            
            try
            {
                tree.Nodes.Clear();
                List<Lemma<Guid, Guid, string>> sampleTypeList = new List<Lemma<Guid, Guid, string>>();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_types", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        Lemma<Guid, Guid, string> sampleType = new Lemma<Guid, Guid, string>(
                            new Guid(reader["id"].ToString()), 
                            new Guid(reader["parent_id"].ToString()), 
                            reader["name"].ToString());

                        sampleTypeList.Add(sampleType);
                    }
                }

                List<Lemma<Guid, Guid, string>> roots = sampleTypeList.FindAll(x => x.ParentId == Guid.Empty);
                foreach (Lemma<Guid, Guid, string> st in roots)
                {
                    TreeNode n = tree.Nodes.Add(st.Id.ToString(), st.Name);
                    n.ToolTipText = n.FullPath;
                }

                foreach(TreeNode tnode in tree.Nodes)                
                    AddSampleTypeChildren(sampleTypeList, tnode);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        private static void AddSampleTypeChildrenCB(TreeNodeCollection tnc, ComboBox cb)
        {
            foreach (TreeNode tn in tnc)
            {
                Lemma<Guid, string> st = new Lemma<Guid, string>(new Guid(tn.Name), tn.Text + " -> " + tn.ToolTipText);
                cb.Items.Add(st);
                AddSampleTypeChildrenCB(tn.Nodes, cb);
            }
        }

        public static void PopulateSampleTypes(TreeView tree, params ComboBox[] cbn)
        {
            foreach (ComboBox cb in cbn)
                cb.Items.Clear();

            foreach (ComboBox cb in cbn)
                AddSampleTypeChildrenCB(tree.Nodes, cb);

            foreach (ComboBox cb in cbn)
                cb.SelectedIndex = -1;            
        }

        public static void PopulateSampleTypePrepMeth(SqlConnection conn, TreeNode tnode, ListBox lb, ListBox lbInherited)
        {
            Guid sampleTypeId = new Guid(tnode.Name);
            string sampleTypeName = tnode.Text;

            string query = @"
select pm.id, pm.name from preparation_method pm	
    inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
    inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
order by name";

            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> st = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                    lb.Items.Add(st);
                }
            }            

            lbInherited.Items.Clear();
            while (tnode.Parent != null)
            {
                tnode = tnode.Parent;
                sampleTypeId = new Guid(tnode.Name);
                using (SqlDataReader reader = DB.GetDataReader(conn, query, CommandType.Text, new SqlParameter("@sample_type_id", sampleTypeId)))
                {
                    while (reader.Read())
                    {
                        Lemma<Guid, string> st = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                        lbInherited.Items.Add(st);
                    }
                }
            }
        }

        public static void PopulateSampleComponents(SqlConnection conn, Guid sampleTypeId, ListBox lb)
        {
            lb.Items.Clear();            

            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure, 
                new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> sampleComponent = new Lemma<Guid, string>(
                        new Guid(reader["id"].ToString()),
                        reader["name"].ToString());

                    lb.Items.Add(sampleComponent);
                }
            }
        }

        public static void PopulateAnalMethNuclides(SqlConnection conn, Guid analysisMethodId, ListBox lb)
        {
            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_nuclides_for_analysis_method", CommandType.StoredProcedure,
                new SqlParameter("@analysis_method_id", analysisMethodId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> n = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                    lb.Items.Add(n);
                }
            }
        }

        public static void PopulatePrepMethAnalMeths(SqlConnection conn, Guid preparationMethodId, ListBox lb)
        {
            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_analysis_methods_for_preparation_method", CommandType.StoredProcedure,
                new SqlParameter("@preparation_method_id", preparationMethodId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> n = new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString());
                    lb.Items.Add(n);
                }
            }
        }

        public static void PopulateSigma(params ComboBox[] cbs)
        {
            foreach (ComboBox cb in cbs)
            {
                cb.Items.Clear();
                cb.Items.AddRange(new object[] { 1.0, 2.0, 3.0 });
                cb.SelectedIndex = -1;
            }
        }

        public static void PopulateCustomers(SqlConnection conn, int statusLevel, params ComboBox[] cbs)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_customers", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", statusLevel)))
            {
                foreach (ComboBox cb in cbs)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbs)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbs)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateCustomers(SqlConnection conn, int statusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_customers_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", statusLevel));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateCustomerContacts(SqlConnection conn, Guid customerId, int statusLevel, params ComboBox[] cbs)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_customer_contacts_for_customer", CommandType.StoredProcedure,
                new SqlParameter("@customer_id", customerId),
                new SqlParameter("@instance_status_level", statusLevel)))
            {
                foreach (ComboBox cb in cbs)
                    cb.Items.Clear();

                while (reader.Read())
                    foreach (ComboBox cb in cbs)
                        cb.Items.Add(new Lemma<Guid, string>(new Guid(reader["id"].ToString()), reader["name"].ToString()));

                foreach (ComboBox cb in cbs)
                    cb.SelectedIndex = -1;
            }
        }

        public static void PopulateCustomerContacts(SqlConnection conn, Guid customerId, int statusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_customer_contacts_for_customer_flat", CommandType.StoredProcedure,
                new SqlParameter("@customer_id", customerId), 
                new SqlParameter("@instance_status_level", statusLevel));

            grid.Columns["id"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";            
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateOrders(SqlConnection conn, int statusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, "csp_select_assignments_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", statusLevel));

            grid.Columns["id"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }
    }
}
