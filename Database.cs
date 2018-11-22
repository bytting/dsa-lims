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
        private static List<Guid> LockedSamples = new List<Guid>();
        private static List<Guid> LockedOrders = new List<Guid>();

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

        public static object MakeParam(Type type, object o)
        {
            if (o == null)
                return DBNull.Value;

            if(o.GetType() == typeof(string))
            {
                string s = Convert.ToString(o);
                if (String.IsNullOrEmpty(s))
                    return DBNull.Value;
            }

            if (type == typeof(double))
                return Convert.ToDouble(o);
            else if (type == typeof(float))
                return Convert.ToSingle(o);
            else if (type == typeof(int) || type == typeof(Int32))
                return Convert.ToInt32(o);
            else if (type == typeof(Int16))
                return Convert.ToInt16(o);
            else if (type == typeof(Int64))
                return Convert.ToInt64(o);
            else if (type == typeof(DateTime))
                return Convert.ToDateTime(o);
            else if (type == typeof(bool))
                return Convert.ToBoolean(o);
            else if (type == typeof(string))            
                return o.ToString();
            else if (type == typeof(Guid))
            {
                Guid g;
                if (!Guid.TryParse(o.ToString(), out g) || g == Guid.Empty)
                    return DBNull.Value;

                return g;
            }
            else return o.ToString();
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

        public static SqlDataReader GetDataReader(SqlConnection conn, SqlTransaction trans, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        public static SqlDataReader GetDataReader(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            return GetDataReader(conn, null, query, queryType, parameters);
        }

        public static object GetScalar(SqlConnection conn, SqlTransaction trans, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static object GetScalar(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            return GetScalar(conn, null, query, queryType, parameters);
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

        public static List<Lemma<int, string>> GetIntLemmata(SqlConnection conn, string proc, bool addEmptyEntry = false)
        {
            List<Lemma<int, string>> list = new List<Lemma<int, string>>();

            if(addEmptyEntry)
                list.Add(new Lemma<int, string>(0, ""));

            try
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, proc, CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        list.Add(new Lemma<int, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }

            return list;
        }

        public static List<Lemma<double?, string>> GetSigmaValues()
        {
            return new List<Lemma<double?, string>>
            {
                new Lemma<double?, string>(null, ""),
                new Lemma<double?, string>(1.0, "1"),
                new Lemma<double?, string>(2.0, "2"),
                new Lemma<double?, string>(3.0, "3")
            };
                
        }

        public static int GetNextSampleCount(SqlConnection conn, SqlTransaction trans)
        {
            SqlParameter nextNumber = new SqlParameter("@current_count", SqlDbType.Int) { Direction = ParameterDirection.Output };

            SqlCommand cmd = new SqlCommand("csp_increment_sample_counter", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(nextNumber);
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(nextNumber.Value);
        }

        public static bool LockSample(SqlConnection conn, Guid sampleId)
        {
            SqlTransaction trans = null;

            try
            {
                trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("select locked_by from sample where id = @id", conn, trans);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", sampleId);
                object o = cmd.ExecuteScalar();                
                if (o == DBNull.Value)
                {
                    cmd.CommandText = "update sample set locked_by = @locked_by where id = @id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_by", Common.Username);
                    cmd.Parameters.AddWithValue("@id", sampleId);
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    if(!LockedSamples.Contains(sampleId))
                        LockedSamples.Add(sampleId);
                    return true;
                }
                else
                {
                    trans.Commit(); 

                    if(o.ToString().ToLower() == Common.Username.ToLower())
                    {
                        if (!LockedSamples.Contains(sampleId))
                            LockedSamples.Add(sampleId);
                        return true;
                    }

                    return false;
                }                
            }
            catch(Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                return false;
            }                        
        }

        public static void UnlockSamples(SqlConnection conn)
        {            
            try
            {
                SqlCommand cmd = new SqlCommand("update sample set locked_by = NULL where id = @id", conn);
                cmd.CommandType = CommandType.Text;

                foreach (Guid id in LockedSamples)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                LockedSamples.Clear();
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);                
            }
        }

        public static string GetOrderPrefix(SqlConnection conn, SqlTransaction trans, Guid labId)
        {
            SqlCommand cmd = new SqlCommand("select name_prefix from laboratory where id = @id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@id", labId);
            return cmd.ExecuteScalar().ToString();
        }

        public static int GetNextOrderCount(SqlConnection conn, SqlTransaction trans, Guid labId)
        {
            SqlParameter nextNumber = new SqlParameter("@current_count", SqlDbType.Int) { Direction = ParameterDirection.Output };

            SqlCommand cmd = new SqlCommand("csp_increment_assignment_counter", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@laboratory_id", labId);
            cmd.Parameters.Add(nextNumber);
            cmd.ExecuteNonQuery();
            return Convert.ToInt32(nextNumber.Value);
        }

        public static bool LockOrder(SqlConnection conn, Guid orderId)
        {
            SqlTransaction trans = null;

            try
            {
                trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("select locked_by from assignment where id = @id", conn, trans);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", orderId);
                object o = cmd.ExecuteScalar();
                if (o == DBNull.Value)
                {
                    cmd.CommandText = "update assignment set locked_by = @locked_by where id = @id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_by", Common.Username);
                    cmd.Parameters.AddWithValue("@id", orderId);
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                    if (!LockedOrders.Contains(orderId))
                        LockedOrders.Add(orderId);
                    return true;
                }
                else
                {
                    trans.Commit();

                    if(o.ToString().ToLower() == Common.Username.ToLower())
                    {
                        if (!LockedOrders.Contains(orderId))
                            LockedOrders.Add(orderId);
                        return true;
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
        }

        public static void UnlockOrders(SqlConnection conn)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("update assignment set locked_by = NULL where id = @id", conn);
                cmd.CommandType = CommandType.Text;

                foreach (Guid id in LockedOrders)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                LockedOrders.Clear();
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }
        }

        public static int GetNextPreparationNumber(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            SqlCommand cmd = new SqlCommand("select max(number) from preparation where sample_id = @sample_id", conn);
            cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sample_id", sampleId);
            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value)
                return 1;

            return Convert.ToInt32(o);
        }

        public static void LoadSampleTypes(SqlConnection conn)
        {
            Common.SampleTypeList.Clear();            

            using (SqlDataReader reader = DB.GetDataReader(conn, "csp_select_sample_types", CommandType.StoredProcedure))
            {
                while (reader.Read())
                {
                    SampleTypeModel sampleType = new SampleTypeModel(
                        Guid.Parse(reader["id"].ToString()),
                        Guid.Parse(reader["parent_id"].ToString()),
                        reader["name"].ToString(),
                        reader["name_common"].ToString(),
                        reader["name_latin"].ToString());

                    Common.SampleTypeList.Add(sampleType);
                }
            }
        }

        public static bool GetUniformActivity(SqlConnection conn, SqlTransaction trans, double activity, Guid activityUnitId, out double uActivity, out int uActivityUnitId)
        {
            SqlCommand cmd = new SqlCommand("select convert_factor, uniform_activity_unit_id from activity_unit where id = @id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@id", activityUnitId);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    uActivity = 0d;
                    uActivityUnitId = 0;
                    return false;
                }

                reader.Read();
                double convFactor = Convert.ToDouble(reader["convert_factor"]);
                uActivityUnitId = Convert.ToInt32(reader["uniform_activity_unit_id"]);
                uActivity = activity / convFactor;
            }            

            return true;
        }
    }
}
