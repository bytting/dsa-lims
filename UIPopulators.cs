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
        public static void PopulatePreparationUnits(SqlConnection conn, ComboBox cb)
        {
            Common.Log.Info("Populating preparation units");

            cb.DataSource = Common.PreparationUnitList;
            cb.SelectedIndex = -1;
        }

        public static void PopulateUniformActivityUnits(SqlConnection conn)
        {
            Common.Log.Info("Populating uniform activity units");

            // populate uniform activity units
        }

        public static void PopulateWorkflowStatus(SqlConnection conn, ComboBox cb)
        {
            Common.Log.Info("Populating workflow status");

            cb.DataSource = Common.WorkflowStatusList;
            cb.SelectedIndex = -1;
        }

        public static void PopulateLocationTypes(SqlConnection conn, ComboBox cb)
        {
            Common.Log.Info("Populating location types");

            cb.DataSource = Common.LocationTypeList;
            cb.SelectedIndex = -1;
        }

        public static void PopulateActivityUnits(SqlConnection conn, DataGridView grid, ComboBox cb)
        {
            Common.Log.Info("Populating activity units");

            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_activity_units_flat", CommandType.StoredProcedure);

            grid.DataSource = dt;

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Unit name";
            grid.Columns["convert_factor"].HeaderText = "Conv. fact.";
            grid.Columns["uniform_activity_unit_name"].HeaderText = "Uniform unit";

            cb.Items.Clear();
            foreach (DataRow row in dt.Rows)
                cb.Items.Add(new Lemma<Guid, string>(new Guid(row["id"].ToString()), row["name"].ToString()));
        }

        private static void AddSampleTypeNodes(TreeNodeCollection nodes, SampleTypeModel st, ComboBox cb)
        {
            TreeNode node = nodes.Add(st.Name, st.ShortName);
            node.ToolTipText = st.Name.Substring(1);
            node.Tag = st;

            cb?.Items.Add(st);

            foreach (SampleTypeModel s in st.SampleTypes)
                AddSampleTypeNodes(node.Nodes, s, cb);
        }

        public static void PopulateSampleTypes(SqlConnection conn, TreeView tree, ComboBox cb)
        {
            Common.Log.Info("Populating sample types");

            try
            {
                tree.Nodes.Clear();
                cb?.Items.Clear();

                foreach (SampleTypeModel st in Common.SampleTypes)
                    AddSampleTypeNodes(tree.Nodes, st, cb);

                if(cb != null)
                    cb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void PopulateProjects(SqlConnection conn, TreeView tree, ComboBox cb)
        {
            Common.Log.Info("Populating projects");

            try
            {
                tree.Nodes.Clear();
                cb.Items.Clear();

                TreeNode root = tree.Nodes.Add("Projects", "Projects");

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_main_projects_short", CommandType.StoredProcedure,
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)))
                {
                    while (reader.Read())
                    {
                        Guid id = new Guid(reader["id"].ToString());
                        string name = reader["name"].ToString();

                        TreeNode node = root.Nodes.Add(name, name);
                        node.Tag = id;

                        cb.Items.Add(new Lemma<Guid, string>(id, name));
                    }
                }

                foreach (TreeNode node in root.Nodes)
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
                Common.Log.Error(ex);
            }
        }

        public static void PopulateSubProjects(SqlConnection conn, Guid pid, ComboBox cb)
        {
            Common.Log.Info("Populating sub-projects");

            cb.Items.Clear();

            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sub_projects_short", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@parent_id", pid),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                }))
            {
                while (reader.Read())
                {
                    Guid id = new Guid(reader["id"].ToString());
                    string name = reader["name"].ToString();

                    cb.Items.Add(new Lemma<Guid, string>(id, name));
                }
            }
        }

        public static void PopulateLaboratories(SqlConnection conn, DataGridView grid)
        {
            Common.Log.Info("Populating laboratories");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_laboratories_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
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

        public static void PopulateUsers(SqlConnection conn, DataGridView grid)
        {
            Common.Log.Info("Populating users");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_accounts_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["password_hash"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            // Set UI state
            grid.Columns["username"].HeaderText = "Username";
            grid.Columns["fullname"].HeaderText = "Name";
            grid.Columns["laboratory_name"].HeaderText = "Laboratory";
            grid.Columns["language_code"].HeaderText = "Language";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateNuclides(SqlConnection conn, DataGridView grid)
        {
            Common.Log.Info("Populating nuclides");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_nuclides_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
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
            Common.Log.Info("Populating energy lines");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_nuclide_transmissions_for_nuclide_flat", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@nuclide_id", nid),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                });

            // Set UI state
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
            Common.Log.Info("Populating geometries");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_preparation_geometries_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
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

        public static void PopulateCounties(SqlConnection conn, DataGridView grid)
        {
            Common.Log.Info("Populating counties");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_counties_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
            grid.Columns["id"].Visible = false;
            grid.Columns["created_by"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["updated_by"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["county_number"].HeaderText = "Number";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateMunicipalities(SqlConnection conn, Guid cid, DataGridView grid)
        {
            Common.Log.Info("Populating municipalities");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_municipalities_for_county_flat", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@county_id", cid),
                    new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
                });

            // Set UI state
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

        public static void PopulateStations(SqlConnection conn, DataGridView grid, ComboBox cb)
        {
            Common.Log.Info("Populating stations");

            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_stations_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
            grid.DataSource = dt;

            // Set UI state
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

            cb.Items.Clear();
            foreach (DataRow row in dt.Rows)
            {
                StationModel station = new StationModel(new Guid(row["id"].ToString()), row["name"].ToString());
                station.Latitude = Convert.ToDouble(row["latitude"]);
                station.Longitude = Convert.ToDouble(row["longitude"]);
                station.Altitude = Convert.ToDouble(row["altitude"]);
                cb.Items.Add(station);
            }
            cb.SelectedIndex = -1;
        }

        public static void PopulateSampleStorage(SqlConnection conn, DataGridView grid)
        {
            Common.Log.Info("Populating sample storage");

            // Set data source
            grid.DataSource = DB.GetDataTable(conn, "csp_select_sample_storages_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            // Set UI state
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

        public static void PopulateSamplers(SqlConnection conn, DataGridView grid, ComboBox cb)
        {
            Common.Log.Info("Populating samplers");

            // Set data source
            DataTable dt = DB.GetDataTable(conn, "csp_select_samplers_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
            grid.DataSource = dt;

            // Set UI state
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

            cb.Items.Clear();
            foreach (DataRow row in dt.Rows)
                cb.Items.Add(new Lemma<Guid, string>(new Guid(row["id"].ToString()), row["name"].ToString()));
            cb.SelectedIndex = -1;
        }
    }
}
