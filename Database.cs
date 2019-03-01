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
using System.Linq;

namespace DSA_lims
{
    public static class DB
    {        
        private static List<Guid> LockedSamples = new List<Guid>();
        private static List<Guid> LockedOrders = new List<Guid>();

        public static string ConnectionString { get { return Properties.Settings.Default.dsa_limsConnectionString; } }        

        public static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }        

        public static DataTable GetDataTable(SqlConnection conn, SqlTransaction trans, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            adapter.SelectCommand.CommandType = queryType;            
            adapter.SelectCommand.Transaction = trans;
            adapter.SelectCommand.Parameters.AddRange(parameters);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static SqlDataReader GetDataReader(SqlConnection conn, SqlTransaction trans, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        public static object GetScalar(SqlConnection conn, SqlTransaction trans, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static List<Lemma<double, string>> GetSigmaValues(SqlConnection conn, SqlTransaction trans, bool MDA)
        {
            List<Lemma<double, string>> lst = new List<Lemma<double, string>>();
            lst.Add(new Lemma<double, string>(0d, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, MDA ? "csp_select_sigma_mda_values" : "csp_select_sigma_values", CommandType.StoredProcedure))
            {
                while(reader.Read())
                    lst.Add(new Lemma<double, string>(reader.GetDouble(0), reader.GetString(1)));
            }

            return lst;
        }

        public static List<Lemma<int?, string>> GetTopValues()
        {
            List<Lemma<int?, string>> lst = new List<Lemma<int?, string>>();
            lst.Add(new Lemma<int?, string>(null, ""));
            lst.Add(new Lemma<int?, string>(50, "50"));
            lst.Add(new Lemma<int?, string>(500, "500"));

            return lst;
        }

        public static void AddAuditMessage(SqlConnection conn, SqlTransaction trans, string tbl, Guid id, AuditOperationType op, string msg, string comment = "")
        {
            SqlCommand cmd = new SqlCommand("csp_insert_audit_message", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@source_table", tbl.Trim());
            cmd.Parameters.AddWithValue("@source_id", id);
            cmd.Parameters.AddWithValue("@operation", op.ToString());
            cmd.Parameters.AddWithValue("@comment", comment.Trim());
            cmd.Parameters.AddWithValue("@value", msg.Trim());
            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        public static List<Lemma<int, string>> GetIntLemmata(SqlConnection conn, SqlTransaction trans, string proc)
        {
            List<Lemma<int, string>> list = new List<Lemma<int, string>>();
            list.Add(new Lemma<int, string>(0, ""));

            try
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, proc, CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        list.Add(new Lemma<int, string>(reader.GetInt32("id"), reader.GetString("name")));
                }
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
            }

            return list;
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
                SqlCommand cmd = new SqlCommand("select locked_id from sample where id = @id", conn, trans);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", sampleId);
                object o = cmd.ExecuteScalar();                
                if (!IsValidField(o))
                {
                    cmd.CommandText = "update sample set locked_id = @locked_id where id = @id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_id", Common.UserId);
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

                    Guid locked_id = Guid.Parse(o.ToString());
                    if(locked_id == Common.UserId)
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
                SqlCommand cmd = new SqlCommand("update sample set locked_id = NULL where id = @id", conn);
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

        public static bool LockOrder(SqlConnection conn, Guid orderId)
        {
            SqlTransaction trans = null;

            try
            {
                trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand("select locked_id from assignment where id = @id", conn, trans);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@id", orderId);
                object o = cmd.ExecuteScalar();
                if (!IsValidField(o))
                {
                    cmd.CommandText = "update assignment set locked_id = @locked_id where id = @id";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@locked_id", Common.UserId);
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

                    Guid locked_id = Guid.Parse(o.ToString());
                    if(locked_id == Common.UserId)
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
                SqlCommand cmd = new SqlCommand("update assignment set locked_id = NULL where id = @id", conn);
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

        public static string GetOrderPrefix(SqlConnection conn, SqlTransaction trans, Guid labId)
        {
            SqlCommand cmd = new SqlCommand("select name_prefix from laboratory where id = @id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@id", labId);
            return cmd.ExecuteScalar().ToString();
        }        

        public static int GetNextOrderCount(SqlConnection conn, SqlTransaction trans, Guid labId)
        {
            object o = GetScalar(conn, trans, "select last_assignment_counter_year from laboratory where id = @id", CommandType.Text, new[] {
                new SqlParameter("@id", labId)
            });

            if (o == null || o == DBNull.Value)
                throw new Exception("GetNextOrderCount: Unable to get last assignment counter year from database for lab id: " + labId.ToString());

            int storedYear = Convert.ToInt32(o);
            int currentYear = DateTime.Now.Year;

            SqlCommand cmd = new SqlCommand("", conn, trans);
            if (storedYear < currentYear)
            {
                cmd.CommandText = "update laboratory set last_assignment_counter_year = @year, assignment_counter = 1 where id = @id";
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@year", currentYear);
                cmd.Parameters.AddWithValue("@id", labId);
                cmd.ExecuteNonQuery();
            }

            SqlParameter nextNumber = new SqlParameter("@current_count", SqlDbType.Int) { Direction = ParameterDirection.Output };

            cmd.CommandText = "csp_increment_assignment_counter";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@laboratory_id", labId);
            cmd.Parameters.Add(nextNumber);
            cmd.ExecuteNonQuery();

            return Convert.ToInt32(nextNumber.Value);
        }

        public static int GetSampleNumber(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            SqlCommand cmd = new SqlCommand("select number from sample where id = @sample_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sample_id", sampleId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                throw new Exception("Requested sample number not found for id: " + sampleId.ToString());

            return Convert.ToInt32(o);
        }

        public static string GetGeometryName(SqlConnection conn, SqlTransaction trans, Guid geomId)
        {
            SqlCommand cmd = new SqlCommand("select name from preparation_geometry where id = @geometry_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@geometry_id", geomId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return "";

            return o.ToString();
        }

        public static Guid GetLaboratoryIdFromOrderId(SqlConnection conn, SqlTransaction trans, Guid orderId)
        {
            SqlCommand cmd = new SqlCommand("select laboratory_id from assignment where id = @assignment_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@assignment_id", orderId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return Guid.Empty;

            return Guid.Parse(o.ToString());
        }

        public static int GetNextPreparationNumber(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            SqlCommand cmd = new SqlCommand("select max(number) from preparation where sample_id = @sample_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sample_id", sampleId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return 1;

            return Convert.ToInt32(o) + 1;
        }

        public static int GetNextAnalysisNumber(SqlConnection conn, SqlTransaction trans, Guid prepId)
        {
            SqlCommand cmd = new SqlCommand("select max(number) from analysis where preparation_id = @prep_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@prep_id", prepId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return 1;

            return Convert.ToInt32(o) + 1;
        }

        public static void LoadSampleTypes(SqlConnection conn, SqlTransaction trans)
        {
            Common.SampleTypeList.Clear();            

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample_types", CommandType.StoredProcedure))
            {
                while (reader.Read())
                {
                    SampleTypeModel sampleType = new SampleTypeModel(
                        reader.GetGuid("id"),
                        reader.GetGuid("parent_id"),
                        reader.GetString("name"),
                        reader.GetString("name_common"),
                        reader.GetString("name_latin"));

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
                double convFactor = reader.GetDouble("convert_factor");
                uActivityUnitId = reader.GetInt32("uniform_activity_unit_id");
                uActivity = activity / convFactor;
            }            

            return true;
        }

        public static Dictionary<string, Guid> GetNuclideNames(SqlConnection conn, SqlTransaction trans)
        {
            Dictionary<string, Guid> names = new Dictionary<string, Guid>();
            names.Add("", Guid.Empty);
            SqlCommand cmd = new SqlCommand("select id, name from nuclide order by name", conn, trans);
            cmd.CommandType = CommandType.Text;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    names.Add(reader.GetString("name").ToUpper(), reader.GetGuid("id"));
                }
            }

            return names;
        }

        public static Dictionary<string, Guid> GetNuclideNamesForAnalysisMethod(SqlConnection conn, SqlTransaction trans, Guid analysisMethodId)
        {
            Dictionary<string, Guid> names = new Dictionary<string, Guid>();
            names.Add("", Guid.Empty);
            SqlCommand cmd = new SqlCommand(@"
select n.id, n.name 
from nuclide n, analysis_method_x_nuclide amxn
where amxn.nuclide_id = n.id and amxn.analysis_method_id = @aid
order by n.name
", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@aid", analysisMethodId);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    names.Add(reader.GetString("name").ToUpper(), reader.GetGuid("id"));
                }
            }
            return names;
        }

        public static void LoadUserRoles(SqlConnection conn, SqlTransaction trans, Guid userId, ref List<string> userRoles)
        {
            userRoles.Clear();

            string query = @"
select r.name 
from role r
    inner join account_x_role axr on axr.role_id = r.id and axr.account_id = @account_id
";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new[] {
                new SqlParameter("@account_id", userId)
            }))
            {
                while(reader.Read())
                {
                    userRoles.Add(reader.GetString("name").ToUpper());
                }
            }
        }        

        public static bool SpecRefExists(SqlConnection conn, SqlTransaction trans, string specref, Guid exceptId)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);
            string query = "select count(*) from analysis where specter_reference = @specref";
            cmd.Parameters.AddWithValue("@specref", specref);
            if (exceptId != Guid.Empty)
            {
                query += " and id not in(@exId)";
                cmd.Parameters.AddWithValue("@exId", exceptId);
            }
            cmd.CommandText = query;

            int cnt = (int)cmd.ExecuteScalar();
            if (cnt > 0)            
                return true;

            return false;
        }

        public static void AddAttachment(SqlConnection conn, SqlTransaction trans, string sourceTable, Guid sourceId, string docName, string docExtension, byte[] data)
        {            
            SqlCommand cmd = new SqlCommand("insert into attachment values(@id, @source_table, @source_id, @label, @file_extension, @content, @create_date, @create_id)", conn, trans);
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@source_table", sourceTable);
            cmd.Parameters.AddWithValue("@source_id", sourceId);
            cmd.Parameters.AddWithValue("@label", docName);            
            cmd.Parameters.AddWithValue("@file_extension", docExtension);
            cmd.Parameters.AddWithValue("@content", data);
            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@create_id", Common.UserId);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteAttachment(SqlConnection conn, SqlTransaction trans, string sourceTable, Guid sourceId)
        {
            SqlCommand cmd = new SqlCommand("delete from attachment where source_table = '" + sourceTable + "' and id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", sourceId);
            cmd.ExecuteNonQuery();
        }

        public static void GetOrderRequiredInventory(SqlConnection conn, SqlTransaction trans, Guid assignmentId, out int nSamples, out int nPreparations, out int nAnalyses)
        {
            nSamples = nPreparations = nAnalyses = 0;

            string query = @"
select
(
select sum(ast.sample_count)
from assignment a
	inner join assignment_sample_type ast on ast.assignment_id = a.id	
where a.id = @aid
) as 'nsamples',
(
select 
	sum(apm.preparation_method_count * ast.sample_count) as 'npreparations'	
from assignment a
	inner join assignment_sample_type ast on ast.assignment_id = a.id
	inner join assignment_preparation_method apm on apm.assignment_sample_type_id = ast.id and apm.preparation_laboratory_id = a.laboratory_id
where a.id = @aid
) as 'npreparations',
(
select 
	sum(aam.analysis_method_count * apm.preparation_method_count * ast.sample_count) as 'nanalyses'
from assignment a
	inner join assignment_sample_type ast on ast.assignment_id = a.id
	inner join assignment_preparation_method apm on apm.assignment_sample_type_id = ast.id
	left outer join assignment_analysis_method aam on aam.assignment_preparation_method_id = apm.id
where a.id = @aid
) as 'nanalyses'
";           
             
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@aid", assignmentId)))
            {
                if(reader.HasRows)
                {
                    reader.Read();

                    if (IsValidField(reader["nsamples"]))
                        nSamples = reader.GetInt32("nsamples");
                    if (IsValidField(reader["npreparations"]))
                        nPreparations = reader.GetInt32("npreparations");
                    if (IsValidField(reader["nanalyses"]))
                        nAnalyses = reader.GetInt32("nanalyses");
                }
            }
        }

        public static void GetOrderCurrentInventory(SqlConnection conn, SqlTransaction trans, Guid assignmentId, out int nSamples, out int nPreparations, out int nAnalyses)
        {
            nSamples = nPreparations = nAnalyses = 0;

            string query = @"
select
	(
		select count(*)
		from sample s
			inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
			inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id
			inner join assignment a on ast.assignment_id = a.id
		where a.id = @aid
	) as 'nsamples',
	(
		select count(*) from preparation p where p.assignment_id = @aid and p.instance_status_id < 2
	) as 'npreparations',
	(
		select count(*) from analysis a where a.assignment_id = @aid and a.instance_status_id < 2
	) as 'nanalyses'
";

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@aid", assignmentId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    
                    if(IsValidField(reader["nsamples"]))
                        nSamples = reader.GetInt32("nsamples");
                    if (IsValidField(reader["npreparations"]))
                        nPreparations = reader.GetInt32("npreparations");
                    if (IsValidField(reader["nanalyses"]))
                        nAnalyses = reader.GetInt32("nanalyses");
                }
            }
        }

        public static void GetSampleCountForAST(SqlConnection conn, SqlTransaction trans, Guid astId, out int nSamples)
        {
            nSamples = 0;

            string query = @"
select count(*) as 'nsamples'
from sample s
	inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
	inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id and sxast.assignment_sample_type_id = @astid
where s.instance_status_id < 2";

            nSamples = (int)DB.GetScalar(conn, trans, query, CommandType.Text, new SqlParameter("@astid", astId));
        }

        public static int GetAvailableSamplesOnAssignmentSampleType(SqlConnection conn, SqlTransaction trans, Guid astId)
        {
            int nAvailSamples = 0, nCurrSamples = 0, n = 0;

            string query = @"
select
(
	select ast.sample_count from assignment_sample_type ast where ast.id = @ast_id
) as 'available_sample_count',
(
	select count(*) from sample s
		inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
		inner join assignment_sample_type ast on sxast.assignment_sample_type_id = ast.id and ast.id = @ast_id
) as 'current_sample_count'
";
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@ast_id", astId)))
            {
                if(reader.HasRows)
                {
                    reader.Read();

                    if (IsValidField(reader["available_sample_count"]))
                        nAvailSamples = reader.GetInt32("available_sample_count");
                    if (IsValidField(reader["current_sample_count"]))
                        nCurrSamples = reader.GetInt32("current_sample_count");
                    n = nAvailSamples - nCurrSamples;
                }
            }

            return n < 0 ? 0 : n;
        }

        public static bool IsValidField(object o)
        {
            return !(o == null || o == DBNull.Value);
        }

        public static bool NameExists(SqlConnection conn, SqlTransaction trans, string table, string name, Guid exceptId)
        {
            string query;
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (exceptId == Guid.Empty)
            {
                query = "select count(*) from " + table + " where name = @name";
                cmd.Parameters.AddWithValue("@name", name);
            }
            else
            {
                query = "select count(*) from " + table + " where name = @name and id not in(@exId)";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@exId", exceptId);
            }

            cmd.CommandText = query;

            return (int)cmd.ExecuteScalar() > 0;
        }

        public static bool CanUserApproveAnalysis(SqlConnection conn, SqlTransaction trans, Guid userId, Guid analMethId)
        {            
            SqlCommand cmd = new SqlCommand("select count(*) from account_x_analysis_method where account_id = @acc_id and analysis_method_id = @am_id", conn, trans);
            cmd.Parameters.AddWithValue("@acc_id", userId);
            cmd.Parameters.AddWithValue("@am_id", analMethId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static double GetDouble(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetDouble: Invalid DB field: " + key);
            return Convert.ToDouble(reader[key]);
        }

        public static double? GetDoubleNullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToDouble(reader[key]);
        }

        public static float GetSingle(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetSingle: Invalid DB field: " + key);
            return Convert.ToSingle(reader[key]);
        }

        public static float? GetSingleNullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToSingle(reader[key]);
        }

        public static Int16 GetInt16(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetInt16: Invalid DB field: " + key);
            return Convert.ToInt16(reader[key]);
        }

        public static Int16? GetInt16Nullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToInt16(reader[key]);
        }

        public static Int32 GetInt32(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetInt32: Invalid DB field: " + key);
            return Convert.ToInt32(reader[key]);
        }

        public static Int32? GetInt32Nullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToInt32(reader[key]);
        }

        public static Int64 GetInt64(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetInt64: Invalid DB field: " + key);
            return Convert.ToInt64(reader[key]);
        }

        public static Int64? GetInt64Nullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToInt64(reader[key]);
        }

        public static DateTime GetDateTime(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetDateTime: Invalid DB field: " + key);
            return Convert.ToDateTime(reader[key]);
        }

        public static DateTime? GetDateTimeNullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToDateTime(reader[key]);
        }

        public static bool GetBoolean(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                throw new Exception("GetBoolean: Invalid DB field: " + key);
            return Convert.ToBoolean(reader[key]);
        }

        public static bool? GetBooleanNullable(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return null;
            return Convert.ToBoolean(reader[key]);
        }

        public static string GetString(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return String.Empty;
            return reader[key].ToString();
        }

        public static Guid GetGuid(this SqlDataReader reader, string key)
        {
            if (!IsValidField(reader[key]))
                return Guid.Empty;
            return Guid.Parse(reader[key].ToString());
        }

        public static string GetNameFromUsername(SqlConnection conn, SqlTransaction trans, string username)
        {
            object o = GetScalar(conn, trans, "select name + ' (' + email + ')' from cv_account where username = @username", CommandType.Text, 
                new SqlParameter("@username", username));

            if (!IsValidField(o))
                return String.Empty;

            return o.ToString();
        }

        public static Guid GetSampleTypeParentId(SqlConnection conn, SqlTransaction trans, Guid sampleTypeId)
        {
            object o = GetScalar(conn, trans, "select parent_id from sample_type where id = @id", CommandType.Text,
                new SqlParameter("@id", sampleTypeId));

            if (!IsValidField(o))
                return Guid.Empty;

            return Guid.Parse(o.ToString());
        }

        public static bool HasAccessToOrder(SqlConnection conn, SqlTransaction trans, Guid orderId)
        {
            Guid aLabId = Assignment.GetLaboratoryId(conn, trans, orderId);
            if (aLabId == Common.LabId)
                return true;

            Guid creatorId = Assignment.GetCreatorId(conn, trans, orderId);
            if (creatorId == Common.UserId)
                return true;

            int n = (int)GetScalar(conn, trans, "select count(*) from assignment_x_account where assignment_id = @aid and account_id = @accid", CommandType.Text,
                new SqlParameter("@aid", orderId), 
                new SqlParameter("@accid", Common.UserId));

            if (n > 0)
                return true;

            return false;
        }
    }

    public class SampleHeader
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string SampleTypeName { get; set; }
        public string SampleComponentName { get; set; }
        public string LaboratoryName { get; set; }
        public List<PreparationHeader> Preparations = new List<PreparationHeader>();

        public void Populate(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                return;

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, @"
select s.number, st.name as 'sample_type_name', sc.name as 'sample_component_name', l.name as 'laboratory_name'
from sample s
    inner join sample_type st on st.id = s.sample_type_id
    left outer join sample_component sc on sc.id = s.sample_component_id
    inner join laboratory l on l.id = s.laboratory_id
where s.id = @sid
", CommandType.Text, 
                new SqlParameter("@sid", Id)))
            {
                reader.Read();

                Number = reader.GetInt32("number");
                SampleTypeName = reader.GetString("sample_type_name");
                SampleComponentName = reader.GetString("sample_component_name");
                LaboratoryName = reader.GetString("laboratory_name");
            }
        }
    }

    public class PreparationHeader
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string PreparationMethodName { get; set; }        
        public string LaboratoryName { get; set; }
        public int WorkflowStatusId { get; set; }
        public string WorkflowStatusName { get; set; }
        public List<AnalysisHeader> Analyses = new List<AnalysisHeader>();

        public void Populate(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                return;

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, @"
select p.number, pm.name as 'preparation_method_name', l.name as 'laboratory_name', ws.id as 'workflow_status_id', ws.name as 'workflow_status_name'
from preparation p
    inner join preparation_method pm on pm.id = p.preparation_method_id    
    inner join laboratory l on l.id = p.laboratory_id
    inner join workflow_status ws on ws.id = p.workflow_status_id
where p.id = @pid
", CommandType.Text, new SqlParameter("@pid", Id)))
            {
                reader.Read();

                Number = reader.GetInt32("number");
                PreparationMethodName = reader.GetString("preparation_method_name");
                LaboratoryName = reader.GetString("laboratory_name");
                WorkflowStatusId = reader.GetInt32("workflow_status_id");
                WorkflowStatusName = reader.GetString("workflow_status_name");
            }
        }
    }

    public class AnalysisHeader
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string AnalysisMethodName { get; set; }
        public Guid PreparationId { get; set; }
        public string LaboratoryName { get; set; }
        public int WorkflowStatusId { get; set; }
        public string WorkflowStatusName { get; set; }

        public void Populate(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                return;

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, @"
select a.number, am.name as 'analysis_method_name', l.name as 'laboratory_name', ws.id as 'workflow_status_id', ws.name as 'workflow_status_name'
from analysis a
    inner join analysis_method am on am.id = a.analysis_method_id    
    inner join laboratory l on l.id = a.laboratory_id
    inner join workflow_status ws on ws.id = a.workflow_status_id
where a.id = @aid
", CommandType.Text, new SqlParameter("@aid", Id)))
            {
                reader.Read();

                Number = reader.GetInt32("number");
                AnalysisMethodName = reader.GetString("analysis_method_name");
                LaboratoryName = reader.GetString("laboratory_name");
                WorkflowStatusId = reader.GetInt32("workflow_status_id");
                WorkflowStatusName = reader.GetString("workflow_status_name");
            }
        }
    }

    public static class SqlParameterCollectionExtensions
    {
        public static SqlParameter AddWithValue(this SqlParameterCollection paramCollection, string paramName, object value, object nullValue)
        {
            if(nullValue != null && nullValue.GetType() == typeof(Guid))
            {
                Guid a = Guid.Parse(value.ToString());
                Guid b = Guid.Parse(nullValue.ToString());
                if (a == b)
                    return paramCollection.AddWithValue(paramName, DBNull.Value);
            }
            else
            {
                if (value == null || value == nullValue)
                    return paramCollection.AddWithValue(paramName, DBNull.Value);
            }            
            
            return paramCollection.AddWithValue(paramName, value);
        }
    }
}
