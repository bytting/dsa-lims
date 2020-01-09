﻿/*	
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public static partial class UI    
    {
        public static void PopulateComboBoxes(SqlConnection conn, string procedure, SqlParameter[] sqlParams, params ComboBox[] cbn)
        {
            List<Lemma<Guid, string>> list = new List<Lemma<Guid, string>>();
            list.Add(new Lemma<Guid, string>(Guid.Empty, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, null, procedure, CommandType.StoredProcedure, sqlParams))
            {
                while (reader.Read())
                    list.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
            }

            foreach (ComboBox cb in cbn)
            {
                object o = cb.SelectedValue;
                cb.DataSource = new List<Lemma<Guid, string>>(list);
                cb.DisplayMember = "Name";
                cb.ValueMember = "Id";
                if (o != null)
                    cb.SelectedValue = o;
            }
        }

        public static void PopulateProjectMain(SqlConnection conn, SqlTransaction trans, Guid accountId, int instanceStatusLevel,  params ComboBox[] cbn)
        {
            List<Lemma<Guid, string>> list = new List<Lemma<Guid, string>>();
            list.Add(new Lemma<Guid, string>(Guid.Empty, ""));

            string query;
            if (!Utils.IsValidGuid(Common.LabId))
            {
                query = @"
select distinct pm.id, pm.name
from project_main pm
    inner join project_sub ps on ps.project_main_id = pm.id
    inner join project_sub_x_account psxa on psxa.project_sub_id = ps.id and psxa.account_id = @aid
where pm.instance_status_id <= @isl
order by pm.name
";
            }
            else
            {
                query = "select id, name from project_main where instance_status_id <= @isl order by name";
            }

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text,
                new SqlParameter("@aid", accountId), 
                new SqlParameter("@isl", instanceStatusLevel)
                ))
            {
                while (reader.Read())
                    list.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
            }

            foreach (ComboBox cb in cbn)
            {
                object o = cb.SelectedValue;
                cb.DataSource = new List<Lemma<Guid, string>>(list);
                cb.DisplayMember = "Name";
                cb.ValueMember = "Id";
                if (o != null)
                    cb.SelectedValue = o;
            }
        }

        public static void PopulateProjectSub(SqlConnection conn, SqlTransaction trans, Guid projectMainId, Guid accountId, int instanceStatusLevel, params ComboBox[] cbn)
        {
            List<Lemma<Guid, string>> list = new List<Lemma<Guid, string>>();
            list.Add(new Lemma<Guid, string>(Guid.Empty, ""));

            string query;
            if (!Utils.IsValidGuid(Common.LabId))
            {
                query = @"
select ps.id, ps.name
from project_sub ps
    inner join project_main pm on ps.project_main_id = pm.id and pm.id = @pmid
    inner join project_sub_x_account psxa on psxa.project_sub_id = ps.id and psxa.account_id = @aid
where ps.instance_status_id <= @isl
order by ps.name
";
            }
            else
            {
                query = "select id, name from project_sub where project_main_id = @pmid and instance_status_id <= @isl order by name";
            }

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, 
                new SqlParameter("@aid", accountId), 
                new SqlParameter("@pmid", projectMainId),
                new SqlParameter("@isl", instanceStatusLevel)))
            {
                while (reader.Read())
                    list.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
            }

            foreach (ComboBox cb in cbn)
            {
                object o = cb.SelectedValue;
                cb.DataSource = new List<Lemma<Guid, string>>(list);
                cb.DisplayMember = "Name";
                cb.ValueMember = "Id";
                if (o != null)
                    cb.SelectedValue = o;
            }
        }

        public static void PopulatePreparationUnits(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_preparation_units", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_short"].HeaderText = "Short name";
        }

        public static void PopulateActivityUnits(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_activity_units_flat", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Unit name";
            grid.Columns["convert_factor"].HeaderText = "Conv.fact.";
            grid.Columns["uniform_activity_unit_name"].HeaderText = "Uniform unit";
        }

        public static void PopulateQuantityUnits(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_quantity_units", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
        }

        public static void PopulateActivityUnitTypes(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_activity_unit_types", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_short"].HeaderText = "Short name";
        }

        public static void PopulateProjectsMain(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_projects_main_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";            
        }

        public static void PopulateProjectsSub(SqlConnection conn, Guid project_main_id, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_projects_sub_flat", CommandType.StoredProcedure,            
                new SqlParameter("@project_main_id", project_main_id),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted)
            );

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["project_main_name"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateLaboratories(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_laboratories_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["assignment_counter"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_prefix"].HeaderText = "Prefix";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateUsers(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_accounts_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));

            grid.Columns["id"].Visible = false;
            grid.Columns["address"].Visible = false;
            grid.Columns["language_code"].Visible = false;
            grid.Columns["create_date"].Visible = false;            
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["laboratory_name"].HeaderText = "Lab";            
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateUsersForProjectSub(SqlConnection conn, SqlTransaction trans, Guid projectSubId, DataGridView grid)
        {
            string query = @"
select a.id, a.name, a.email 
from cv_account a
    inner join project_sub_x_account psa on psa.account_id = a.id and psa.project_sub_id = @psid
where a.email is not NULL
";
            grid.DataSource = DB.GetDataTable(conn, trans, query, CommandType.Text, new SqlParameter("@psid", projectSubId));

            grid.Columns["id"].Visible = false;            

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["email"].HeaderText = "Email";
        }

        public static void PopulateNuclides(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {                        
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_nuclides_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["zas"].HeaderText = "Id (ZAS)";
            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["protons"].HeaderText = "Protons";
            grid.Columns["neutrons"].HeaderText = "Neutrons";
            grid.Columns["meta_stable"].HeaderText = "Meta stable";
            grid.Columns["half_life_year"].HeaderText = "T 1/2 (Years)";
            grid.Columns["instance_status_name"].HeaderText = "Status";

            grid.Columns["half_life_year"].DefaultCellStyle.Format = Utils.ScientificFormat;
        }

        public static void PopulateNuclides(SqlConnection conn, ComboBox cb)
        {
            List<Lemma<Guid, string>> nucls = new List<Lemma<Guid, string>>();
            nucls.Add(new Lemma<Guid, string>(Guid.Empty, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_nuclides_short", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Active)))
            {
                while (reader.Read())
                {
                    nucls.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
            cb.DisplayMember = "Name";
            cb.ValueMember = "Id";
            cb.DataSource = nucls;
        }

        public static void PopulateGeometries(SqlConnection conn, DataGridView grid)
        {            
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_preparation_geometries_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["min_fill_height_mm"].HeaderText = "Min Fillh. (mm)";
            grid.Columns["max_fill_height_mm"].HeaderText = "Max Fillh. (mm)";
            grid.Columns["volume_l"].HeaderText = "Volume (L)";
            grid.Columns["radius_mm"].HeaderText = "Radius (mm)";
            grid.Columns["instance_status_name"].HeaderText = "Status";
            
        }        

        public static void PopulateCounties(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_counties_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["county_number"].HeaderText = "Number";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        public static void PopulateMunicipalities(SqlConnection conn, Guid cid, DataGridView grid)
        {            
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_municipalities_for_county_flat", CommandType.StoredProcedure,
                new SqlParameter("@county_id", cid),
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["county_name"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["municipality_number"].HeaderText = "Number";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        public static void PopulateStations(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_stations_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["latitude"].HeaderText = "Latitude";
            grid.Columns["longitude"].HeaderText = "Longitude";
            grid.Columns["altitude"].HeaderText = "Altitude";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        public static void PopulateSampleStorage(SqlConnection conn, DataGridView grid)
        {         
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_sample_storages_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));
        
            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        public static void PopulateSamplers(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_samplers_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));

            grid.Columns["id"].Visible = false;

            grid.Columns["person_name"].HeaderText = "Sampl.Name";
            grid.Columns["person_email"].HeaderText = "Sampl.Email";
            grid.Columns["person_phone"].HeaderText = "Sampl.Phone";
            grid.Columns["person_address"].HeaderText = "Sampl.Address";
            grid.Columns["company_name"].HeaderText = "Comp.Name";
            grid.Columns["company_email"].HeaderText = "Comp.Email";
            grid.Columns["company_phone"].HeaderText = "Comp.Phone";
            grid.Columns["company_address"].HeaderText = "Comp.Address";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        public static void PopulateSamplingMethods(SqlConnection conn, int instanceStatusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_sampling_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", instanceStatusLevel));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulatePreparationMethods(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_preparation_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_short"].HeaderText = "Abbr.";
            grid.Columns["description_link"].HeaderText = "Desc.link";
            grid.Columns["destructive"].HeaderText = "Destructive";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }                

        public static void PopulateAnalysisMethods(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_analysis_methods_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["name_short"].HeaderText = "Abbr.";
            grid.Columns["description_link"].HeaderText = "Desc.link";
            grid.Columns["specter_reference_regexp"].HeaderText = "Spec.Ref RE";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }        

        private static void AddSampleTypeChildren(List<SampleTypeModel> sampleTypeList, TreeNode tnode)
        {
            List<SampleTypeModel> children = sampleTypeList.FindAll(x => x.ParentId == Guid.Parse(tnode.Name));
            foreach (SampleTypeModel st in children)
            {
                TreeNode n = tnode.Nodes.Add(st.Id.ToString(), st.Name);
                n.ToolTipText = n.FullPath + Environment.NewLine + Environment.NewLine + "English name: " + st.NameCommon + Environment.NewLine + "Latin name: " + st.NameLatin;
                n.Tag = n.FullPath;
                AddSampleTypeChildren(sampleTypeList, n);
            }
        }

        public static void PopulateSampleTypes(SqlConnection conn, TreeView tree)
        {            
            try
            {
                tree.Nodes.Clear();
                List<SampleTypeModel> roots = Common.SampleTypeList.FindAll(x => x.ParentId == Guid.Empty);
                foreach (SampleTypeModel st in roots)
                {
                    TreeNode n = tree.Nodes.Add(st.Id.ToString(), st.Name);
                    n.ToolTipText = n.FullPath + Environment.NewLine + Environment.NewLine + "English name: " + st.NameCommon + Environment.NewLine + "Latin name: " + st.NameLatin;
                    n.Tag = n.FullPath;
                }

                foreach (TreeNode tnode in tree.Nodes)
                {
                    AddSampleTypeChildren(Common.SampleTypeList, tnode);
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        private static void AddSampleTypeChildren(TreeNodeCollection tnc, List<Lemma<Guid, string>> list)
        {
            foreach (TreeNode tn in tnc)
            {
                Lemma<Guid, string> st = new Lemma<Guid, string>(Guid.Parse(tn.Name), tn.Text + " -> " + tn.Tag.ToString());
                list.Add(st);
                AddSampleTypeChildren(tn.Nodes, list);
            }
        }

        public static void PopulateSampleTypes(TreeView tree, params ComboBox[] cbn)
        {
            foreach (ComboBox cb in cbn)
            {
                List<Lemma<Guid, string>> sampleTypeList = new List<Lemma<Guid, string>>();
                sampleTypeList.Add(new Lemma<Guid, string>(Guid.Empty, ""));
                AddSampleTypeChildren(tree.Nodes, sampleTypeList);
                sampleTypeList.Sort(delegate (Lemma<Guid, string> l1, Lemma<Guid, string> l2)
                {
                    return l1.Name.CompareTo(l2.Name);
                });
                cb.DataSource = sampleTypeList;
                cb.DisplayMember = "Name";
                cb.ValueMember = "Id";
            }
        }                

        public static void PopulateSampleTypePrepMeth(SqlConnection conn, Guid sampleTypeId, Guid labId, ComboBox cbox)
        {
            List<Lemma<Guid, string>> list = new List<Lemma<Guid, string>>();

            string query = @"
select pm.id, pm.name_short as 'name' from preparation_method pm	
    inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
    inner join laboratory_x_preparation_method lxpm on lxpm.preparation_method_id = pm.id and lxpm.laboratory_id = @laboratory_id
    inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
where pm.instance_status_id <= 1
order by name";
            
            using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, 
                new SqlParameter("@sample_type_id", sampleTypeId),
                new SqlParameter("@laboratory_id", labId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> st = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                    list.Add(st);
                }
            }
            
            while (true)
            {
                sampleTypeId = DB.GetSampleTypeParentId(conn, null, sampleTypeId);

                using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, 
                    new SqlParameter("@sample_type_id", sampleTypeId), 
                    new SqlParameter("@laboratory_id", labId)))
                {
                    while (reader.Read())
                    {
                        Lemma<Guid, string> st = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        list.Add(st);
                    }
                }

                if (sampleTypeId == Guid.Empty)
                    break;
            }

            cbox.DisplayMember = "name";
            cbox.ValueMember = "id";
            cbox.DataSource = list;
        }

        public static void PopulateSampleTypePrepMeth(SqlConnection conn, SqlTransaction trans, TreeNode tnode, ListBox lb, ListBox lbInherited)
        {
            Guid sampleTypeId = Guid.Parse(tnode.Name);            

            string query = @"
select pm.id, pm.name from preparation_method pm	
    inner join sample_type_x_preparation_method stpm on stpm.preparation_method_id = pm.id
    inner join sample_type st on stpm.sample_type_id = st.id and st.id = @sample_type_id
order by name";

            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> st = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                    lb.Items.Add(st);
                }
            }            

            lbInherited.Items.Clear();
            while (tnode.Parent != null)
            {
                tnode = tnode.Parent;
                sampleTypeId = Guid.Parse(tnode.Name);
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@sample_type_id", sampleTypeId)))
                {
                    while (reader.Read())
                    {
                        Lemma<Guid, string> st = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                        lbInherited.Items.Add(st);
                    }
                }
            }
        }

        public static void PopulateSampleComponents(SqlConnection conn, Guid sampleTypeId, ListBox lb)
        {
            lb.Items.Clear();            

            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure, 
                new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> sampleComponent = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                    lb.Items.Add(sampleComponent);
                }
            }
        }

        public static void PopulateSampleComponentsAscending(SqlConnection conn, Guid sampleTypeId, TreeNode tnode, ComboBox cbox)
        {
            List<Lemma<Guid, string>> comps = new List<Lemma<Guid, string>>();
            comps.Add(new Lemma<Guid, string>(Guid.Empty, ""));
            AddSampleTypeComponentsAscending(conn, sampleTypeId, tnode, comps);
            cbox.DataSource = comps;
            cbox.DisplayMember = "Name";
            cbox.ValueMember = "Id";
        }

        private static void AddSampleTypeComponentsAscending(SqlConnection conn, Guid sampleTypeId, TreeNode tnode, List<Lemma<Guid, string>> comps)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                    new SqlParameter("@sample_type_id", sampleTypeId)))
            {
                while (reader.Read())
                    comps.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
            }

            if (tnode.Parent != null)
            {
                Guid parentId = Guid.Parse(tnode.Parent.Name);
                AddSampleTypeComponentsAscending(conn, parentId, tnode.Parent, comps);
            }
        }

        public static void PopulateAnalMethNuclides(SqlConnection conn, SqlTransaction trans, Guid analysisMethodId, ListBox lb)
        {
            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_nuclides_for_analysis_method", CommandType.StoredProcedure,
                new SqlParameter("@analysis_method_id", analysisMethodId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> n = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                    lb.Items.Add(n);
                }
            }
        }

        public static void PopulatePrepMethAnalMeths(SqlConnection conn, SqlTransaction trans, Guid preparationMethodId, ListBox lb)
        {
            lb.Items.Clear();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis_methods_for_preparation_method", CommandType.StoredProcedure,
                new SqlParameter("@preparation_method_id", preparationMethodId)))
            {
                while (reader.Read())
                {
                    Lemma<Guid, string> n = new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name"));
                    lb.Items.Add(n);
                }
            }
        }

        public static void PopulateCustomers(SqlConnection conn, int statusLevel, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_customers_flat", CommandType.StoredProcedure,
                new SqlParameter("@instance_status_level", statusLevel));

            grid.Columns["id"].Visible = false;

            grid.Columns["person_name"].HeaderText = "Cont.Name";
            grid.Columns["person_email"].HeaderText = "Cont.Email";
            grid.Columns["person_phone"].HeaderText = "Cont.Phone";
            grid.Columns["person_address"].HeaderText = "Cont.Address";
            grid.Columns["company_name"].HeaderText = "Comp.Name";
            grid.Columns["company_email"].HeaderText = "Comp.Email";
            grid.Columns["company_phone"].HeaderText = "Comp.Phone";
            grid.Columns["company_address"].HeaderText = "Comp.Address";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateOrders(SqlConnection conn, int statusLevel, Guid laboratoryId, DataGridView grid)
        {
            string query = @"
select id, name, customer_contact_name, customer_company_name
from assignment a 
where laboratory_id = @laboratory_id and instance_status_id <= @instance_status_level 
order by create_date desc";

            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text,
                new SqlParameter("@laboratory_id", laboratoryId),
                new SqlParameter("@instance_status_level", statusLevel));

            grid.Columns["id"].Visible = false;            

            grid.Columns["name"].HeaderText = "Name";            
            grid.Columns["customer_contact_name"].HeaderText = "Customer";
            grid.Columns["customer_company_name"].HeaderText = "Company";
        }

        public static void PopulateOrdersConstruction(SqlConnection conn, Guid laboratoryId, DataGridView grid)
        {
            string query = @"
select id, name, description, customer_contact_name
from assignment a 
where laboratory_id = @laboratory_id and instance_status_id = 1 and workflow_status_id = 1
order by create_date desc";

            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@laboratory_id", laboratoryId));

            grid.Columns["id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["description"].HeaderText = "Description";
            grid.Columns["customer_contact_name"].HeaderText = "Customer";            
        }        

        public static void PopulatePersons(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_persons", CommandType.StoredProcedure);

            grid.Columns["id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["update_date"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";            
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["address"].HeaderText = "Address";            
        }

        public static void PopulateCompanies(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "csp_select_companies_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", InstanceStatus.Deleted));

            grid.Columns["id"].Visible = false;
            grid.Columns["comment"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["email"].HeaderText = "Email";
            grid.Columns["phone"].HeaderText = "Phone";
            grid.Columns["address"].HeaderText = "Address";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateOrderYears(SqlConnection conn, ComboBox cbox)
        {
            List<string> years = new List<string>();
            years.Add("");

            using (SqlDataReader reader = DB.GetDataReader(conn, null, "select distinct year(create_date) as 'year' from assignment order by year desc", CommandType.Text))
            {
                while(reader.Read())
                {
                    years.Add(reader.GetString("year"));
                }
            }
            cbox.DataSource = years;
        }

        public static void PopulateOrderWorkflowStatus(SqlConnection conn, ComboBox cbox)
        {
            List<Lemma<int, string>> stats = new List<Lemma<int, string>>();
            stats.Add(new Lemma<int, string>(0, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, null, "csp_select_workflow_status", CommandType.StoredProcedure))
            {
                while (reader.Read())
                {
                    stats.Add(new Lemma<int, string>(reader.GetInt32("id"), reader.GetString("name")));
                }
            }
            cbox.DataSource = stats;
        }

        public static void PopulateRoles(SqlConnection conn, Guid userId, ListBox lb)
        {
            List<Lemma<Guid, string>> roles = new List<Lemma<Guid, string>>();

            string query = @"
select id, name 
from role r
    inner join account_x_role axr on axr.role_id = r.id and axr.account_id = @account_id
";
            using (SqlDataReader reader = DB.GetDataReader(conn, null, query, CommandType.Text, new[] {
                new SqlParameter("@account_id", userId)
            }))
            {
                while (reader.Read())
                {
                    roles.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }

            lb.DataSource = roles;
        }

        public static void PopulateAttachments(SqlConnection conn, SqlTransaction trans, string sourceTable, Guid id, DataGridView grid)
        {
            string query = "select id, name, file_extension from attachment where source_table = @source_table and source_id = @source_id order by create_date desc";
            grid.DataSource = DB.GetDataTable(conn, trans, query, CommandType.Text, new[] {
                new SqlParameter("@source_table", sourceTable),
                new SqlParameter("@source_id", id)
            });

            grid.Columns["id"].Visible = false;
            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["file_extension"].HeaderText = "Type";
        }

        public static void ShowAttachment(SqlConnection conn, int gridIndex, DataGridView grid)
        {
            if (gridIndex < 0 || gridIndex >= grid.Rows.Count)
                return;

            Guid id = Utils.MakeGuid(grid.Rows[gridIndex].Cells["id"].Value);
            string fname = grid.Rows[gridIndex].Cells["name"].Value.ToString();
            string ext = grid.Rows[gridIndex].Cells["file_extension"].Value.ToString();

            string pathname = Path.GetTempPath() + "\\" + fname + ext;
            if (File.Exists(pathname))
            {
                try
                {
                    File.Delete(pathname);
                }
                catch
                {
                    MessageBox.Show("The file " + pathname + " is already open");
                    return;
                }
            }

            byte[] data = null;
            SqlCommand cmd = new SqlCommand("select content from attachment where id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            data = (byte[])cmd.ExecuteScalar();            

            if (data != null)
            {
                try
                {
                    File.WriteAllBytes(pathname, data);
                    Process.Start(pathname);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public static void PopulateLabPrepMeths(SqlConnection conn, Guid labId, DataGridView grid)
        {
            string query = @"
select pm.id, pm.name_short, pm.name
from preparation_method pm
    inner join laboratory_x_preparation_method lxpm on lxpm.preparation_method_id = pm.id and lxpm.laboratory_id = @lab_id
order by pm.name_short
";
            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@lab_id", labId));

            grid.Columns["id"].Visible = false;

            grid.Columns["name_short"].HeaderText = "Abbr.";
            grid.Columns["name"].HeaderText = "Name";
        }

        public static void PopulateLabAnalMeths(SqlConnection conn, Guid labId, DataGridView grid)
        {
            string query = @"
select am.id, am.name_short, am.name
from analysis_method am
    inner join laboratory_x_analysis_method lxam on lxam.analysis_method_id = am.id and lxam.laboratory_id = @lab_id
order by am.name_short
";
            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@lab_id", labId));

            grid.Columns["id"].Visible = false;

            grid.Columns["name_short"].HeaderText = "Abbr.";
            grid.Columns["name"].HeaderText = "Name";
        }

        public static void PopulateUserAnalMeths(SqlConnection conn, Guid userId, DataGridView grid)
        {
            string query = @"
select am.id, am.name_short, am.name
from analysis_method am
    inner join account_x_analysis_method lxam on lxam.analysis_method_id = am.id
where lxam.account_id = @acc_id
order by am.name_short
";
            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@acc_id", userId));

            grid.Columns["id"].Visible = false;

            grid.Columns["name_short"].HeaderText = "Abbr.";
            grid.Columns["name"].HeaderText = "Name";
        }

        public static void PopulateSampleParameterNames(SqlConnection conn, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, null, "select * from sample_parameter_name order by name asc", CommandType.Text);

            grid.Columns["id"].Visible = false;
            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["type"].HeaderText = "Type";
        }

        public static void PopulateAccreditationTerms(SqlConnection conn, SqlTransaction trans, int instance_status_level, DataGridView grid)
        {
            grid.DataSource = DB.GetDataTable(conn, trans, "csp_select_accreditation_terms_flat", CommandType.StoredProcedure, 
                new SqlParameter("@instance_status_level", instance_status_level));

            grid.Columns["id"].Visible = false;
            grid.Columns["create_date"].Visible = false;
            grid.Columns["create_id"].Visible = false;
            grid.Columns["update_date"].Visible = false;
            grid.Columns["update_id"].Visible = false;

            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["fill_height_min"].HeaderText = "F.H. min";
            grid.Columns["fill_height_max"].HeaderText = "F.H. max";
            grid.Columns["weight_min"].HeaderText = "Weight min";
            grid.Columns["weight_max"].HeaderText = "Weight max";            
            grid.Columns["volume_min"].HeaderText = "Volume min";
            grid.Columns["volume_max"].HeaderText = "Volume max";
            grid.Columns["density_min"].HeaderText = "Density min";
            grid.Columns["density_max"].HeaderText = "Density max";
            grid.Columns["instance_status_name"].HeaderText = "Status";
        }

        public static void PopulateAccreditationTermNuclides(SqlConnection conn, SqlTransaction trans, Guid accredId, ListBox lb)
        {
            lb.Items.Clear();
            string query = @"
select n.id, n.name from nuclide n
	inner join accreditation_term_x_nuclide atxn on n.id = atxn.nuclide_id and atxn.accreditation_term_id = @accreditation_term_id
order by n.name";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@accreditation_term_id", accredId)))
            {
                while(reader.Read())
                {
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
        }

        public static void PopulateAccreditationTermSampleTypes(SqlConnection conn, SqlTransaction trans, Guid accredId, DataGridView grid)
        {
            string query = @"
select st.id, st.name, st.path from sample_type st
	inner join accreditation_term_x_sample_type atxst on st.id = atxst.sample_type_id and atxst.accreditation_term_id = @accreditation_term_id
order by st.name";
            grid.DataSource = DB.GetDataTable(conn, null, query, CommandType.Text, new SqlParameter("@accreditation_term_id", accredId));

            grid.Columns["id"].Visible = false;
            grid.Columns["name"].HeaderText = "Name";
            grid.Columns["path"].HeaderText = "Path";
        }

        public static void PopulateAccreditationTermLaboratories(SqlConnection conn, SqlTransaction trans, Guid accredId, ListBox lb)
        {
            lb.Items.Clear();
            string query = @"
select l.id, l.name from laboratory l
	inner join accreditation_term_x_laboratory atxl on l.id = atxl.laboratory_id and atxl.accreditation_term_id = @accreditation_term_id
order by l.name";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@accreditation_term_id", accredId)))
            {
                while (reader.Read())
                {
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
        }

        public static void PopulateAccreditationTermPreparationMethods(SqlConnection conn, SqlTransaction trans, Guid accredId, ListBox lb)
        {
            lb.Items.Clear();
            string query = @"
select pm.id, pm.name from preparation_method pm
	inner join accreditation_term_x_preparation_method atxpm on pm.id = atxpm.preparation_method_id and atxpm.accreditation_term_id = @accreditation_term_id
order by pm.name";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@accreditation_term_id", accredId)))
            {
                while (reader.Read())
                {
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
        }

        public static void PopulateAccreditationTermAnalysisMethods(SqlConnection conn, SqlTransaction trans, Guid accredId, ListBox lb)
        {
            lb.Items.Clear();
            string query = @"
select am.id, am.name from analysis_method am
	inner join accreditation_term_x_analysis_method atxam on am.id = atxam.analysis_method_id and atxam.accreditation_term_id = @accreditation_term_id
order by am.name";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@accreditation_term_id", accredId)))
            {
                while (reader.Read())
                {
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
        }

        public static void PopulateAccreditationTermSampleComponents(SqlConnection conn, SqlTransaction trans, Guid accredId, Guid sampTypeId, ListBox lb)
        {
            lb.Items.Clear();
            string query = @"
select sc.id, sc.name from sample_component sc
	inner join accreditation_term_x_sample_type_x_sample_component ass on sc.id = ass.sample_component_id and ass.sample_type_id = @sample_type_id and ass.accreditation_term_id = @accreditation_term_id
order by sc.name";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new[] {
                new SqlParameter("@sample_type_id", sampTypeId),
                new SqlParameter("@accreditation_term_id", accredId)
            }))
            {
                while (reader.Read())
                {
                    lb.Items.Add(new Lemma<Guid, string>(reader.GetGuid("id"), reader.GetString("name")));
                }
            }
        }
    }
}
