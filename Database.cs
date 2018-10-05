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
using System.Text;

namespace DSA_lims
{
    public static class DB
    {    
        public static string GetConnectionString()
        {
            return Properties.Settings.Default.DSADB;
        }

        public static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(GetConnectionString());
            connection.Open();
            return connection;
        }

        public static DataTable GetDataTable(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            adapter.SelectCommand.CommandType = queryType;
            adapter.SelectCommand.Parameters.AddRange(parameters);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static SqlDataReader GetDataReader(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        public static object GetScalar(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static void AddAuditMessage(SqlConnection conn, SqlTransaction trans, string tbl, Guid id, AuditOperationType op, string msg)
        {
            SqlCommand cmd = new SqlCommand("csp_insert_audit_message", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@source_table", tbl);
            cmd.Parameters.AddWithValue("@source_id", id);
            cmd.Parameters.AddWithValue("@operation", op.ToString());
            cmd.Parameters.AddWithValue("@value", msg);
            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

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

        public static object MakeParam(Type type, object o)
        {
            if (type == typeof(double))
                return Convert.ToDouble(o);
            else if(type == typeof(int))
                return Convert.ToInt32(o);
            else
                return o.ToString();
        }
    }    
}
