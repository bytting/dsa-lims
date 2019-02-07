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
using Newtonsoft.Json;

namespace DSA_lims
{
    public static class DB
    {        
        private static List<Guid> LockedSamples = new List<Guid>();
        private static List<Guid> LockedOrders = new List<Guid>();

        public static string ConnectionString { get; set; }

        public static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
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

        public static List<Lemma<double?, string>> GetSigmaValues(SqlConnection conn, SqlTransaction trans = null)
        {
            List<Lemma<double?, string>> lst = new List<Lemma<double?, string>>();
            lst.Add(new Lemma<double?, string>(null, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sigma_values", CommandType.StoredProcedure, new SqlParameter[] { }))
            {
                while(reader.Read())
                    lst.Add(new Lemma<double?, string>(reader.GetDouble(0), reader.GetString(1)));
            }

            return lst;
        }

        public static List<Lemma<double?, string>> GetSigmaMDAValues(SqlConnection conn, SqlTransaction trans = null)
        {
            List<Lemma<double?, string>> lst = new List<Lemma<double?, string>>();
            lst.Add(new Lemma<double?, string>(null, ""));

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sigma_mda_values", CommandType.StoredProcedure, new SqlParameter[] { }))
            {
                while (reader.Read())
                    lst.Add(new Lemma<double?, string>(reader.GetDouble(0), reader.GetString(1)));
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

        public static List<Lemma<int?, string>> GetIntLemmata(SqlConnection conn, SqlTransaction trans, string proc, bool addEmptyEntry = false)
        {
            List<Lemma<int?, string>> list = new List<Lemma<int?, string>>();

            if(addEmptyEntry)
                list.Add(new Lemma<int?, string>(null, ""));

            try
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, proc, CommandType.StoredProcedure))
                {
                    while (reader.Read())
                        list.Add(new Lemma<int?, string>(Convert.ToInt32(reader["id"]), reader["name"].ToString()));
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

        public static string GetOrderPrefix(SqlConnection conn, SqlTransaction trans, Guid labId)
        {
            SqlCommand cmd = new SqlCommand("select name_prefix from laboratory where id = @id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@id", labId);
            return cmd.ExecuteScalar().ToString();
        }        

        public static bool IsOrderClosed(SqlConnection conn, SqlTransaction trans, Guid orderId)
        {
            SqlCommand cmd = new SqlCommand("select workflow_status_id from assignment where id = @id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@id", orderId);

            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value)
                return false;

            return Convert.ToInt32(o) == WorkflowStatus.Complete;
        }

        public static bool IsSampleClosed(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            string query = @"
select count(*) as 'nclosed'
from sample s
	inner join sample_x_assignment_sample_type sxast on sxast.sample_id = s.id
	inner join assignment_sample_type ast on ast.id = sxast.assignment_sample_type_id
	inner join assignment a on a.id = ast.assignment_id
where s.id = @sid and a.workflow_status_id = 2
";
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sid", sampleId);

            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value)
                return false;

            return Convert.ToInt32(o) > 0;
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

        public static Guid GetLaboratoryIdFromSampleId(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            SqlCommand cmd = new SqlCommand(@"
select a.laboratory_id 
from assignment a
    inner join assignment_sample_type ast on ast.assignment_id = a.id
    inner join sample_x_assignment_sample_type sxast on sxast.assignment_sample_type_id = ast.id
    inner join sample s on s.id = sxast.sample_id
where s.id = @sample_id
", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sample_id", sampleId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return Guid.Empty;

            return Guid.Parse(o.ToString());
        }

        public static string GetCreatedByFromSampleId(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            SqlCommand cmd = new SqlCommand("select created_by from sample where id = @sample_id", conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sample_id", sampleId);
            object o = cmd.ExecuteScalar();
            if (!IsValidField(o))
                return String.Empty;

            return o.ToString();
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
                    names.Add(reader["name"].ToString().ToUpper(), Guid.Parse(reader["id"].ToString()));
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
                    names.Add(reader["name"].ToString().ToUpper(), Guid.Parse(reader["id"].ToString()));
                }
            }
            return names;
        }

        public static string GetAccountNameFromUsername(SqlConnection conn, SqlTransaction trans, string username)
        {
            object o = DB.GetScalar(conn, trans, "select name + ' (' + email + ')' as 'name' from cv_account where username = @username", CommandType.Text, new[] {
                new SqlParameter("@username", username)
            });

            if (!IsValidField(o))
                return "";

            return o.ToString();
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
                    userRoles.Add(reader["name"].ToString().ToUpper());
                }
            }
        }

        public static bool SampleHasRequiredFields(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            string query = "select number, sample_type_id, project_sub_id, laboratory_id, reference_date from sample where id = @id";

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@id", sampleId)))
            {
                if (!reader.HasRows)
                    return false;

                reader.Read();

                if (!IsValidField(reader["number"])             
                    || !IsValidField(reader["sample_type_id"]) 
                    || !IsValidField(reader["project_sub_id"]) 
                    || !IsValidField(reader["laboratory_id"]) 
                    || !IsValidField(reader["reference_date"]))
                    return false;

                if (!Utils.IsValidGuid(reader["sample_type_id"]) 
                    || !Utils.IsValidGuid(reader["project_sub_id"]) 
                    || !Utils.IsValidGuid(reader["laboratory_id"]))
                    return false;
            }

            return true;
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
            SqlCommand cmd = new SqlCommand("insert into attachment values(@id, @source_table, @source_id, @label, @file_extension, @content, @create_date, @created_by)", conn, trans);
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@source_table", sourceTable);
            cmd.Parameters.AddWithValue("@source_id", sourceId);
            cmd.Parameters.AddWithValue("@label", docName);            
            cmd.Parameters.AddWithValue("@file_extension", docExtension);
            cmd.Parameters.AddWithValue("@content", data);
            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@created_by", Common.Username);
            cmd.ExecuteNonQuery();
        }

        public static void DeleteAttachment(SqlConnection conn, SqlTransaction trans, string sourceTable, Guid sourceId)
        {
            SqlCommand cmd = new SqlCommand("delete from attachment where source_table = '" + sourceTable + "' and id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", sourceId);
            cmd.ExecuteNonQuery();
        }

        public static bool IsOrderApproved(SqlConnection conn, SqlTransaction trans, Guid orderId)
        {
            bool apprCust = false;
            bool apprLab = false;

            using (SqlDataReader reader = GetDataReader(conn, trans, "select approved_customer, approved_laboratory from assignment where id = @id", CommandType.Text, 
                new SqlParameter("@id", orderId)))
            {
                if (!reader.HasRows)
                    return false;

                reader.Read();

                if (!IsValidField(reader["approved_customer"]))
                    return false;

                if (!IsValidField(reader["approved_laboratory"]))
                    return false;

                apprCust = Convert.ToBoolean(reader["approved_customer"]);
                apprLab = Convert.ToBoolean(reader["approved_laboratory"]);
            }

            return apprCust && apprLab;
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
	inner join assignment_preparation_method apm on apm.assignment_sample_type_id = ast.id	
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

                    if(IsValidField(reader["nsamples"]))
                        nSamples = Convert.ToInt32(reader["nsamples"]);
                    if (IsValidField(reader["npreparations"]))
                        nPreparations = Convert.ToInt32(reader["npreparations"]);
                    if (IsValidField(reader["nanalyses"]))
                        nAnalyses = Convert.ToInt32(reader["nanalyses"]);
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
		select count(*) from preparation p where p.assignment_id = @aid
	) as 'npreparations',
	(
		select count(*) from analysis a where a.assignment_id = @aid
	) as 'nanalyses'
";

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@aid", assignmentId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    if(IsValidField(reader["nsamples"]))
                        nSamples = Convert.ToInt32(reader["nsamples"]);
                    if (IsValidField(reader["npreparations"]))
                        nPreparations = Convert.ToInt32(reader["npreparations"]);
                    if (IsValidField(reader["nanalyses"]))
                        nAnalyses = Convert.ToInt32(reader["nanalyses"]);
                }
            }
        }

        public static int GetAvailableSamplesOnAssignmentSampleType(SqlConnection conn, SqlTransaction trans, Guid astId)
        {
            int n = 0;
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
                    int nAvailSamples = Convert.ToInt32(reader["available_sample_count"]);
                    int nCurrSamples = Convert.ToInt32(reader["current_sample_count"]);
                    n = nAvailSamples - nCurrSamples;
                }
            }

            return n < 0 ? 0 : n;
        }

        public static bool IsValidField(object o)
        {
            return !(o == null || o == DBNull.Value);
        }

        public static bool PreparationExists(SqlConnection conn, SqlTransaction trans, Guid pId)
        {
            SqlCommand cmd = new SqlCommand("select count(*) from preparation where id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", pId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static bool AnalysisExists(SqlConnection conn, SqlTransaction trans, Guid aId)
        {
            SqlCommand cmd = new SqlCommand("select count(*) from analysis where id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", aId);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public static bool AnalysisResultExists(SqlConnection conn, SqlTransaction trans, Guid arId)
        {
            SqlCommand cmd = new SqlCommand("select count(*) from analysis_result where id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", arId);
            return (int)cmd.ExecuteScalar() > 0;
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

        public static bool GetOrderApprovedByLab(SqlConnection conn, SqlTransaction trans, Guid orderId)
        {
            SqlCommand cmd = new SqlCommand("select approved_laboratory from assignment where id = @id", conn, trans);
            cmd.Parameters.AddWithValue("@id", orderId);
            object o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
                return false;

            return Convert.ToBoolean(o);
        }
    }

    public class Preparation
    {
        public Guid Id { get; set; }
        public Guid SampleId { get; set; }
        public int Number { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid PreparationGeometryId { get; set; }
        public Guid PreparationMethodId { get; set; }
        public int WorkflowStatusId { get; set; }
        public double Amount { get; set; }
        public int PrepUnitId { get; set; }
        public double Quantity { get; set; }
        public int QuantityUnitId { get; set; }
        public double FillHeightMM { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public bool _Dirty;

        public void Clear()
        {
            Id = Guid.Empty;
            SampleId = Guid.Empty;
            Number = 0;
            AssignmentId = Guid.Empty;
            LaboratoryId = Guid.Empty;
            PreparationGeometryId = Guid.Empty;
            PreparationMethodId = Guid.Empty;
            WorkflowStatusId = 0;
            Amount = 0d;
            PrepUnitId = 0;
            Quantity = 0d;
            QuantityUnitId = 0;
            FillHeightMM = 0d;
            InstanceStatusId = 0;
            Comment = String.Empty;
            CreateDate = DateTime.MinValue;
            CreatedBy = String.Empty;
            UpdateDate = DateTime.MinValue;
            UpdatedBy = String.Empty;
            _Dirty = false;
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid prepId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_preparation_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", prepId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    var cols = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        cols.Add(reader.GetName(i));

                    foreach (var col in cols)
                        map.Add(col, reader[col]);

                    json = JsonConvert.SerializeObject(map, Formatting.None);
                }
            }

            return json;
        }

        public bool LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid preparationId)
        {
            bool res = false;

            Clear();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_preparation", CommandType.StoredProcedure,
                new SqlParameter("@id", preparationId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    Id = Guid.Parse(reader["id"].ToString());
                    SampleId = DB.IsValidField(reader["sample_id"]) ? Guid.Parse(reader["sample_id"].ToString()) : Guid.Empty;
                    Number = Convert.ToInt32(reader["number"]);
                    AssignmentId = DB.IsValidField(reader["assignment_id"]) ? Guid.Parse(reader["assignment_id"].ToString()) : Guid.Empty;
                    LaboratoryId = DB.IsValidField(reader["laboratory_id"]) ? Guid.Parse(reader["laboratory_id"].ToString()) : Guid.Empty;
                    PreparationGeometryId = DB.IsValidField(reader["preparation_geometry_id"]) ? Guid.Parse(reader["preparation_geometry_id"].ToString()) : Guid.Empty;
                    PreparationMethodId = DB.IsValidField(reader["preparation_method_id"]) ? Guid.Parse(reader["preparation_method_id"].ToString()) : Guid.Empty;
                    WorkflowStatusId = Convert.ToInt32(reader["workflow_status_id"]);
                    Amount = DB.IsValidField(reader["amount"]) ? Convert.ToDouble(reader["amount"]) : 0d;
                    PrepUnitId = DB.IsValidField(reader["prep_unit_id"]) ? Convert.ToInt32(reader["prep_unit_id"]) : 0;
                    Quantity = DB.IsValidField(reader["quantity"]) ? Convert.ToDouble(reader["quantity"]) : 0d;
                    QuantityUnitId = DB.IsValidField(reader["quantity_unit_id"]) ? Convert.ToInt32(reader["quantity_unit_id"]) : 0;
                    FillHeightMM = DB.IsValidField(reader["fill_height_mm"]) ? Convert.ToDouble(reader["fill_height_mm"]) : 0d;
                    InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                    Comment = reader["comment"].ToString();
                    CreateDate = Convert.ToDateTime(reader["create_date"]);
                    CreatedBy = reader["created_by"].ToString();
                    UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    UpdatedBy = reader["updated_by"].ToString();
                    res = true;
                }
            }

            return res;
        }

        public bool StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            bool res = false;
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (Id == Guid.Empty || !DB.PreparationExists(conn, trans, Id))
            {
                // Insert new preparation            
                if (Id == Guid.Empty)
                    Id = Guid.NewGuid();
                cmd.CommandText = "csp_insert_preparation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@sample_id", DB.MakeParam(typeof(Guid), SampleId));
                cmd.Parameters.AddWithValue("@number", Number);
                cmd.Parameters.AddWithValue("@assignment_id", DB.MakeParam(typeof(Guid), AssignmentId));
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                cmd.Parameters.AddWithValue("@preparation_geometry_id", DB.MakeParam(typeof(Guid), PreparationGeometryId));
                cmd.Parameters.AddWithValue("@preparation_method_id", DB.MakeParam(typeof(Guid), PreparationMethodId));
                cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                cmd.Parameters.AddWithValue("@amount", DB.MakeParam(typeof(double), Amount));
                cmd.Parameters.AddWithValue("@prep_unit_id", DB.MakeParam(typeof(int), PrepUnitId));
                cmd.Parameters.AddWithValue("@quantity", DB.MakeParam(typeof(double), Quantity));
                cmd.Parameters.AddWithValue("@quantity_unit_id", DB.MakeParam(typeof(int), QuantityUnitId));
                cmd.Parameters.AddWithValue("@fill_height_mm", FillHeightMM);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Comment);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                string json = Preparation.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "preparation", Id, AuditOperationType.Insert, json, "");

                _Dirty = false;
                res = true;
            }
            else
            {
                if (_Dirty)
                {
                    // Update existing preparation
                    cmd.CommandText = "csp_update_preparation";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@preparation_geometry_id", DB.MakeParam(typeof(Guid), PreparationGeometryId));
                    cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                    cmd.Parameters.AddWithValue("@amount", DB.MakeParam(typeof(double), Amount));
                    cmd.Parameters.AddWithValue("@prep_unit_id", DB.MakeParam(typeof(int), PrepUnitId));
                    cmd.Parameters.AddWithValue("@quantity", DB.MakeParam(typeof(double), Quantity));
                    cmd.Parameters.AddWithValue("@quantity_unit_id", DB.MakeParam(typeof(int), QuantityUnitId));
                    cmd.Parameters.AddWithValue("@fill_height_mm", FillHeightMM);
                    cmd.Parameters.AddWithValue("@comment", Comment);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = Preparation.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "preparation", Id, AuditOperationType.Update, json, "");

                    _Dirty = false;
                    res = true;
                }
            }

            return res;
        }

        public bool IsDirty
        {
            get
            {
                return _Dirty;
            }
        }

        public bool IsClosed(SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
select a.workflow_status_id 
from assignment a
	inner join preparation p on p.assignment_id = a.id
where p.id = @pid
";
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@pid", Id);

            object o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
                return false;

            return Convert.ToInt32(o) == WorkflowStatus.Complete;
        }

        public string GetRequestedActivityUnitName(SqlConnection conn, SqlTransaction trans)
        {
            string requnit = "";
            string query = @"
select name
from activity_unit au
    inner join assignment_sample_type ast on au.id = ast.requested_activity_unit_id
    inner join sample_x_assignment_sample_type sxast on ast.id = sxast.assignment_sample_type_id
    inner join sample s on s.id = sxast.sample_id
    inner join preparation p on p.id = @pid
where p.assignment_id = ast.assignment_id
";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", Id);
            object o = cmd.ExecuteScalar();
            if (DB.IsValidField(o))
                requnit += o.ToString() + " ";

            cmd.CommandText = @"
select name
from activity_unit_type aut
    inner join assignment_sample_type ast on aut.id = ast.requested_activity_unit_type_id
    inner join sample_x_assignment_sample_type sxast on ast.id = sxast.assignment_sample_type_id
    inner join sample s on s.id = sxast.sample_id
    inner join preparation p on p.id = @pid
where p.assignment_id = ast.assignment_id
";
            o = cmd.ExecuteScalar();
            if (DB.IsValidField(o))
                requnit += o.ToString();

            return requnit;
        }        
    }

    public class Analysis
    {
        public Analysis()
        {
            Results = new List<AnalysisResult>();
            _Dirty = false;
        }

        public Guid Id { get; set; }
        public int Number { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid PreparationId { get; set; }
        public Guid AnalysisMethodId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string SpecterReference { get; set; }
        public Guid ActivityUnitId { get; set; }
        public Guid ActivityUnitTypeId { get; set; }
        public double SigmaActivity { get; set; }
        public double SigmaMDA { get; set; }
        public string NuclideLibrary { get; set; }
        public string MDALibrary { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public List<AnalysisResult> Results { get; set; }

        public string _ImportFile;
        public bool _Dirty;

        public void Clear()
        {
            Id = Guid.Empty;
            Number = 0;
            AssignmentId = Guid.Empty;
            LaboratoryId = Guid.Empty;
            PreparationId = Guid.Empty;
            AnalysisMethodId = Guid.Empty;
            WorkflowStatusId = 0;
            SpecterReference = String.Empty;
            ActivityUnitId = Guid.Empty;
            ActivityUnitTypeId = Guid.Empty;
            SigmaActivity = 0d;
            SigmaMDA = 0d;
            NuclideLibrary = String.Empty;
            MDALibrary = String.Empty;
            InstanceStatusId = 0;
            Comment = String.Empty;
            CreateDate = DateTime.MinValue;
            CreatedBy = String.Empty;
            UpdateDate = DateTime.MinValue;
            UpdatedBy = String.Empty;
            Results.Clear();
            _ImportFile = String.Empty;
            _Dirty = false;
        }

        public Analysis Clone()
        {
            Analysis a = new Analysis();
            a.Id = Id;
            a.Number = Number;
            a.AssignmentId = AssignmentId;
            a.LaboratoryId = LaboratoryId;
            a.PreparationId = PreparationId;
            a.AnalysisMethodId = AnalysisMethodId;
            a.WorkflowStatusId = WorkflowStatusId;
            a.SpecterReference = SpecterReference;
            a.ActivityUnitId = ActivityUnitId;
            a.ActivityUnitTypeId = ActivityUnitTypeId;
            a.SigmaActivity = SigmaActivity;
            a.SigmaMDA = SigmaMDA;
            a.NuclideLibrary = NuclideLibrary;
            a.MDALibrary = MDALibrary;
            a.InstanceStatusId = InstanceStatusId;
            a.Comment = Comment;
            a.CreateDate = CreateDate;
            a.CreatedBy = CreatedBy;
            a.UpdateDate = UpdateDate;
            a.UpdatedBy = UpdatedBy;
            a._ImportFile = _ImportFile;
            a._Dirty = _Dirty;
            a.Results.AddRange(Results);
            return a;
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid analId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", analId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    var cols = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        cols.Add(reader.GetName(i));

                    foreach (var col in cols)
                        map.Add(col, reader[col]);

                    json = JsonConvert.SerializeObject(map, Formatting.None);
                }
            }

            return json;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid analysisId)
        {
            Clear();         
            bool analysisFound = false; 

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis", CommandType.StoredProcedure,
                new SqlParameter("@id", analysisId)))
            {
                analysisFound = reader.HasRows;
                if (analysisFound)
                {
                    reader.Read();

                    Id = Guid.Parse(reader["id"].ToString());
                    Number = Convert.ToInt32(reader["number"]);
                    AssignmentId = DB.IsValidField(reader["assignment_id"]) ? Guid.Parse(reader["assignment_id"].ToString()) : Guid.Empty;
                    LaboratoryId = DB.IsValidField(reader["laboratory_id"]) ? Guid.Parse(reader["laboratory_id"].ToString()) : Guid.Empty;
                    PreparationId = DB.IsValidField(reader["preparation_id"]) ? Guid.Parse(reader["preparation_id"].ToString()) : Guid.Empty;
                    AnalysisMethodId = DB.IsValidField(reader["analysis_method_id"]) ? Guid.Parse(reader["analysis_method_id"].ToString()) : Guid.Empty;
                    WorkflowStatusId = Convert.ToInt32(reader["workflow_status_id"]);
                    SpecterReference = reader["specter_reference"].ToString();
                    ActivityUnitId = DB.IsValidField(reader["activity_unit_id"]) ? Guid.Parse(reader["activity_unit_id"].ToString()) : Guid.Empty;
                    ActivityUnitTypeId = DB.IsValidField(reader["activity_unit_type_id"]) ? Guid.Parse(reader["activity_unit_type_id"].ToString()) : Guid.Empty;
                    SigmaActivity = DB.IsValidField(reader["sigma_act"]) ? Convert.ToDouble(reader["sigma_act"]) : 0d;
                    SigmaMDA = DB.IsValidField(reader["sigma_mda"]) ? Convert.ToDouble(reader["sigma_mda"]) : 0d;
                    NuclideLibrary = reader["nuclide_library"].ToString();
                    MDALibrary = reader["mda_library"].ToString();
                    InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                    Comment = reader["comment"].ToString();
                    CreateDate = Convert.ToDateTime(reader["create_date"]);
                    CreatedBy = reader["created_by"].ToString();
                    UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    UpdatedBy = reader["updated_by"].ToString();                    
                }                
            }

            if (analysisFound)
            {
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis_results_for_analysis", CommandType.StoredProcedure,
                    new SqlParameter("@analysis_id", analysisId)))
                {
                    while (reader.Read())
                    {
                        AnalysisResult ar = new AnalysisResult();
                        ar.Id = Guid.Parse(reader["id"].ToString());
                        ar.AnalysisId = Guid.Parse(reader["analysis_id"].ToString());
                        ar.NuclideId = Guid.Parse(reader["nuclide_id"].ToString());
                        ar.NuclideName = reader["nuclide_name"].ToString();
                        ar.Activity = Convert.ToDouble(reader["activity"]);
                        ar.ActivityUncertaintyABS = Convert.ToDouble(reader["activity_uncertainty_abs"]);
                        ar.ActivityApproved = Convert.ToBoolean(reader["activity_approved"]);
                        ar.UniformActivity = Convert.ToDouble(reader["uniform_activity"]);
                        ar.UniformActivityUnitId = Convert.ToInt32(reader["uniform_activity_unit_id"]);
                        ar.DetectionLimit = Convert.ToDouble(reader["detection_limit"]);
                        ar.DetectionLimitApproved = Convert.ToBoolean(reader["detection_limit_approved"]);
                        ar.Accredited = Convert.ToBoolean(reader["accredited"]);
                        ar.Reportable = Convert.ToBoolean(reader["reportable"]);
                        ar.InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                        ar.CreateDate = Convert.ToDateTime(reader["create_date"]);
                        ar.CreatedBy = reader["created_by"].ToString();
                        ar.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                        ar.UpdatedBy = reader["updated_by"].ToString();
                
                        ar._Dirty = false;
                        Results.Add(ar);                        
                    }
                }
            }

            _ImportFile = String.Empty;
            _Dirty = false;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (Id == Guid.Empty || !DB.AnalysisExists(conn, trans, Id))
            {
                // Insert new analysis            
                if(Id == Guid.Empty)
                    Id = Guid.NewGuid();
                cmd.CommandText = "csp_insert_analysis";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@number", Number);
                cmd.Parameters.AddWithValue("@assignment_id", DB.MakeParam(typeof(Guid), AssignmentId));
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                cmd.Parameters.AddWithValue("@preparation_id", DB.MakeParam(typeof(Guid), PreparationId));
                cmd.Parameters.AddWithValue("@analysis_method_id", DB.MakeParam(typeof(Guid), AnalysisMethodId));
                cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                cmd.Parameters.AddWithValue("@specter_reference", SpecterReference);
                cmd.Parameters.AddWithValue("@activity_unit_id", DB.MakeParam(typeof(Guid), ActivityUnitId));
                cmd.Parameters.AddWithValue("@activity_unit_type_id", DB.MakeParam(typeof(Guid), ActivityUnitTypeId));
                cmd.Parameters.AddWithValue("@sigma_act", SigmaActivity);
                cmd.Parameters.AddWithValue("@sigma_mda", SigmaMDA);
                cmd.Parameters.AddWithValue("@nuclide_library", NuclideLibrary);
                cmd.Parameters.AddWithValue("@mda_library", MDALibrary);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Comment);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                string json = Analysis.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "analysis", Id, AuditOperationType.Insert, json, "");

                _Dirty = false;
            }
            else
            {
                if (_Dirty)
                {
                    // Update existing analysis
                    cmd.CommandText = "csp_update_analysis";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                    cmd.Parameters.AddWithValue("@specter_reference", SpecterReference);
                    cmd.Parameters.AddWithValue("@activity_unit_id", DB.MakeParam(typeof(Guid), ActivityUnitId));
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", DB.MakeParam(typeof(Guid), ActivityUnitTypeId));
                    cmd.Parameters.AddWithValue("@sigma_act", SigmaActivity);
                    cmd.Parameters.AddWithValue("@sigma_mda", SigmaMDA);
                    cmd.Parameters.AddWithValue("@nuclide_library", NuclideLibrary);
                    cmd.Parameters.AddWithValue("@mda_library", MDALibrary);
                    cmd.Parameters.AddWithValue("@comment", Comment);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = Analysis.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "analysis", Id, AuditOperationType.Update, json, "");

                    _Dirty = false;
                }
            }

            List<Guid> existingResultIds = new List<Guid>();
            foreach (AnalysisResult r in Results)
            {
                if(DB.AnalysisResultExists(conn, trans, r.Id))
                {
                    // analysis result already exist in the db
                    if (r._Dirty)
                    {
                        // analysis result has been modified, update it
                        cmd.CommandText = @"
update analysis_result set
    activity = @activity,
	activity_uncertainty_abs = @activity_uncertainty_abs,
	activity_approved = @activity_approved,
	uniform_activity = @uniform_activity,
	uniform_activity_unit_id = @uniform_activity_unit_id,
	detection_limit = @detection_limit,
	detection_limit_approved = @detection_limit_approved,
	accredited = @accredited,
	reportable = @reportable,
	instance_status_id = @instance_status_id,
	update_date = @update_date,
	updated_by = @updated_by
where id = @id
";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@id", r.Id);
                        cmd.Parameters.AddWithValue("@activity", r.Activity);
                        cmd.Parameters.AddWithValue("@activity_uncertainty_abs", r.ActivityUncertaintyABS);
                        cmd.Parameters.AddWithValue("@activity_approved", r.ActivityApproved);
                        double uAct = -1.0;
                        int uActUnitId = -1;
                        if (Utils.IsValidGuid(ActivityUnitId))
                        {
                            DB.GetUniformActivity(conn, trans, r.Activity, ActivityUnitId, out uAct, out uActUnitId);
                        }
                        cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                        cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                        cmd.Parameters.AddWithValue("@detection_limit", r.DetectionLimit);
                        cmd.Parameters.AddWithValue("@detection_limit_approved", r.DetectionLimitApproved);
                        cmd.Parameters.AddWithValue("@accredited", r.Accredited);
                        cmd.Parameters.AddWithValue("@reportable", r.Reportable);
                        cmd.Parameters.AddWithValue("@instance_status_id", r.InstanceStatusId);
                        cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                        cmd.ExecuteNonQuery();

                        string json = AnalysisResult.ToJSON(conn, trans, r.Id);
                        if (!String.IsNullOrEmpty(json))
                            DB.AddAuditMessage(conn, trans, "analysis_result", r.Id, AuditOperationType.Update, json, "");

                        r._Dirty = false;
                    }
                }
                else
                {
                    // analysis result does not exist, insert it
                    cmd.CommandText = @"
insert into analysis_result values(
    @id,
    @analysis_id,
    @nuclide_id,
    @activity,
    @activity_uncertainty_abs,
    @activity_approved,
    @uniform_activity,
    @uniform_activity_unit_id,
    @detection_limit, 
    @detection_limit_approved,
    @accredited,
    @reportable,
    @instance_status_id,
    @create_date,
    @created_by,
    @update_date,
    @updated_by)";
                    cmd.CommandType = CommandType.Text;
                    r.Id = Guid.NewGuid();
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", r.Id);
                    cmd.Parameters.AddWithValue("@analysis_id", Id);
                    cmd.Parameters.AddWithValue("@nuclide_id", DB.MakeParam(typeof(Guid), r.NuclideId));
                    cmd.Parameters.AddWithValue("@activity", r.Activity);
                    cmd.Parameters.AddWithValue("@activity_uncertainty_abs", r.ActivityUncertaintyABS);
                    cmd.Parameters.AddWithValue("@activity_approved", r.ActivityApproved);
                    double uAct = -1.0;
                    int uActUnitId = -1;
                    if (Utils.IsValidGuid(ActivityUnitId))
                    {
                        DB.GetUniformActivity(conn, trans, r.Activity, ActivityUnitId, out uAct, out uActUnitId);
                    }
                    cmd.Parameters.AddWithValue("@uniform_activity", uAct);
                    cmd.Parameters.AddWithValue("@uniform_activity_unit_id", uActUnitId);
                    cmd.Parameters.AddWithValue("@detection_limit", r.DetectionLimit);
                    cmd.Parameters.AddWithValue("@detection_limit_approved", r.DetectionLimitApproved);
                    cmd.Parameters.AddWithValue("@accredited", r.Accredited);
                    cmd.Parameters.AddWithValue("@reportable", r.Reportable);
                    cmd.Parameters.AddWithValue("@instance_status_id", r.InstanceStatusId);
                    cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@created_by", Common.Username);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = AnalysisResult.ToJSON(conn, trans, r.Id);
                    if(!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "analysis_result", r.Id, AuditOperationType.Insert, json, "");

                    r._Dirty = false;
                }

                existingResultIds.Add(r.Id);
            }

            // FIXME: Audit log
            string strExResIds = string.Join(",", existingResultIds.Select(x => "'" + x.ToString() + "'"));
            if (!String.IsNullOrEmpty(strExResIds))            
                cmd.CommandText = "delete from analysis_result where analysis_id = @analysis_id and id not in (" + strExResIds + ")";            
            else            
                cmd.CommandText = "delete from analysis_result where analysis_id = @analysis_id";

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@analysis_id", Id);
            cmd.ExecuteNonQuery();
        }

        public bool IsDirty
        {
            get
            {
                if (_Dirty)
                    return true;

                foreach (AnalysisResult r in Results)
                {
                    if (r._Dirty)
                        return true;
                }
                return false;
            }            
        }

        public bool IsClosed(SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
select a.workflow_status_id 
from assignment a
	inner join analysis an on an.assignment_id = a.id
where an.id = @aid
";
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@aid", Id);

            object o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
                return false;

            return Convert.ToInt32(o) == WorkflowStatus.Complete;
        }        
    }

    public class AnalysisResult
    {        
        public AnalysisResult()
        {
            Id = Guid.NewGuid();
            InstanceStatusId = InstanceStatus.Active;
        }

        public Guid Id { get; set; }
        public Guid AnalysisId { get; set; }
        public Guid NuclideId { get; set; }
        public string NuclideName { get; set; }
        public double Activity { get; set; }
        public double ActivityUncertaintyABS { get; set; }
        public bool ActivityApproved { get; set; }
        public double UniformActivity { get; set; }
        public int UniformActivityUnitId { get; set; }
        public double DetectionLimit { get; set; }
        public bool DetectionLimitApproved { get; set; }
        public bool Accredited { get; set; }
        public bool Reportable { get; set; }
        public int InstanceStatusId { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public bool _Dirty;

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid analResId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis_result_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", analResId)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    var cols = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        cols.Add(reader.GetName(i));

                    foreach (var col in cols)
                        map.Add(col, reader[col]);

                    json = JsonConvert.SerializeObject(map, Formatting.None);
                }
            }

            return json;
        }
    }

    public class PreparationGeometry
    {
        public PreparationGeometry()
        {
        }

        public PreparationGeometry(SqlConnection conn, SqlTransaction trans, Guid id)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_preparation_geometry", CommandType.StoredProcedure,
                    new SqlParameter("@id", id)))
            {
                if (reader.HasRows)
                {
                    reader.Read();

                    Id = Guid.Parse(reader["id"].ToString());
                    Name = reader["name"].ToString();
                    if (DB.IsValidField(reader["min_fill_height_mm"]))
                        MinFillHeightMM = Convert.ToDouble(reader["min_fill_height_mm"]);
                    if (DB.IsValidField(reader["max_fill_height_mm"]))
                        MaxFillHeightMM = Convert.ToDouble(reader["max_fill_height_mm"]);
                    InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                    Comment = reader["comment"].ToString();
                    CreateDate = Convert.ToDateTime(reader["create_date"]);
                    CreatedBy = reader["created_by"].ToString();
                    UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    UpdatedBy = reader["updated_by"].ToString();
                }
            }
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public double MinFillHeightMM { get; set; }
        public double MaxFillHeightMM { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
