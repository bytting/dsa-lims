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
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Data;

namespace DSA_lims
{
    public static partial class UI
    {
        public static void LoadInstanceStatus(SqlConnection conn)
        {
            Common.Log.Info("Loading instance status");

            try
            {
                Common.InstanceStatusList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_instance_status", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.InstanceStatusList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void LoadDecayTypes(SqlConnection conn)
        {
            Common.Log.Info("Loading decay types");

            try
            {
                Common.DecayTypeList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_decay_types", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.DecayTypeList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void LoadPreparationUnits(SqlConnection conn)
        {
            Common.Log.Info("Loading preparation units");

            try
            {
                Common.PreparationUnitList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_preparation_units", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.PreparationUnitList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void LoadUniformActivityUnits(SqlConnection conn)
        {
            Common.Log.Info("Loading uniform activity units");

            try
            {
                Common.UniformActivityUnitList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_uniform_activity_units", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.UniformActivityUnitList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void LoadWorkflowStatus(SqlConnection conn)
        {
            Common.Log.Info("Loading workflow status");

            try
            {
                Common.WorkflowStatusList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_workflow_status", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.WorkflowStatusList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static void LoadLocationTypes(SqlConnection conn)
        {
            Common.Log.Info("Loading location types");

            try
            {
                Common.LocationTypeList.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_location_types", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        Common.LocationTypeList.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        private static void AddSampleTypeComponents(SqlConnection conn, SampleTypeModel st)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_components_for_sample_type", CommandType.StoredProcedure,
                new SqlParameter("@sample_type_id", st.Id)))
            {
                while (reader.Read())
                    st.SampleComponents.Add(new SampleComponentModel(new Guid(reader["id"].ToString()), reader["name"].ToString()));
            }

            foreach (SampleTypeModel s in st.SampleTypes)
                AddSampleTypeComponents(conn, s);
        }

        private static void AddSampleTypeParameters(SqlConnection conn, SampleTypeModel st)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_parameters_for_sample_type", CommandType.StoredProcedure,
                new SqlParameter("@sample_type_id", st.Id)))
            {
                while (reader.Read())
                {
                    SampleParameterModel sampleParameter = new SampleParameterModel(new Guid(reader["id"].ToString()), reader["name"].ToString());
                    sampleParameter.Type = reader["type"].ToString();
                    st.SampleParameters.Add(sampleParameter);
                }
            }

            foreach (SampleTypeModel s in st.SampleTypes)
                AddSampleTypeParameters(conn, s);
        }

        public static void LoadSampleTypes(SqlConnection conn)
        {
            Common.Log.Info("Loading sample types");

            try
            {
                Common.SampleTypes.Clear();

                using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_types_short", CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        SampleTypeModel sampleType = new SampleTypeModel(new Guid(reader["id"].ToString()), reader["name"].ToString());

                        string[] items = sampleType.Name.Substring(1).Split(new char[] { '/' });
                        List<SampleTypeModel> current = Common.SampleTypes;
                        foreach (string item in items)
                        {
                            SampleTypeModel found = current.Find(x => x.ShortName == item);
                            if (found != null)
                            {
                                current = found.SampleTypes;
                                continue;
                            }
                            else
                            {
                                sampleType.ShortName = item;
                                current.Add(sampleType);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }

            try
            {
                foreach (SampleTypeModel st in Common.SampleTypes)
                {
                    AddSampleTypeComponents(conn, st);
                    AddSampleTypeParameters(conn, st);
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }
    }
}
