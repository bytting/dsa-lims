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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DSA_lims
{
    public class AssignmentPreparationMethod
    {
        public AssignmentPreparationMethod()
        {
            Id = Guid.NewGuid();
            Dirty = false;
            AnalysisMethods = new List<AssignmentAnalysisMethod>();
        }

        public Guid Id { get; set; }
        public Guid AssignmentSampleTypeId { get; set; }
        public Guid PreparationMethodId { get; set; }
        public int PreparationMethodCount { get; set; }
        public Guid PreparationLaboratoryId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public List<AssignmentAnalysisMethod> AnalysisMethods { get; set; }

        public bool Dirty;

        public bool IsDirty
        {
            get
            {
                if (Dirty)
                    return true;

                foreach (AssignmentAnalysisMethod aam in AnalysisMethods)
                    if (aam.IsDirty)
                        return true;

                return false;
            }
        }

        public void ClearDirty()
        {
            Dirty = false;
            foreach (AssignmentAnalysisMethod aam in AnalysisMethods)
                aam.ClearDirty();
        }

        public string PreparationMethodName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name_short from preparation_method where id = @pmid", CommandType.Text, new SqlParameter("@pmid", PreparationMethodId));
            return o == null || o == DBNull.Value ? "" : o.ToString();
        }

        public string PreparationMethodNameFull(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from preparation_method where id = @pmid", CommandType.Text, new SqlParameter("@pmid", PreparationMethodId));
            return o == null || o == DBNull.Value ? "" : o.ToString();
        }

        public string PreparationLaboratoryName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from laboratory where id = @lid", CommandType.Text, new SqlParameter("@lid", PreparationLaboratoryId));
            return o == null || o == DBNull.Value ? "" : o.ToString();
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid apmId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_preparation_method_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", apmId)))
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

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid apmId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from assignment_preparation_method where id = @id", CommandType.Text, new SqlParameter("@id", apmId));
            return cnt > 0;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!AssignmentPreparationMethod.IdExists(conn, trans, Id))
            {
                // Insert new apm
                cmd.CommandText = "csp_insert_assignment_preparation_method";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@assignment_sample_type_id", DB.MakeParam(typeof(Guid), AssignmentSampleTypeId));
                cmd.Parameters.AddWithValue("@preparation_method_id", DB.MakeParam(typeof(Guid), PreparationMethodId));
                cmd.Parameters.AddWithValue("@preparation_method_count", DB.MakeParam(typeof(int), PreparationMethodCount));
                cmd.Parameters.AddWithValue("@preparation_laboratory_id", DB.MakeParam(typeof(Guid), PreparationLaboratoryId));                
                cmd.Parameters.AddWithValue("@comment", DB.MakeParam(typeof(String), Comment));
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                string json = Assignment.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "assignment_preparation_method", Id, AuditOperationType.Insert, json, "");

                Dirty = false;
            }
            else
            {
                if (Dirty)
                {
                    // Update existing apm
                    cmd.CommandText = "csp_update_assignment_preparation_method";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@assignment_sample_type_id", DB.MakeParam(typeof(Guid), AssignmentSampleTypeId));
                    cmd.Parameters.AddWithValue("@preparation_method_id", DB.MakeParam(typeof(Guid), PreparationMethodId));
                    cmd.Parameters.AddWithValue("@preparation_method_count", DB.MakeParam(typeof(int), PreparationMethodCount));
                    cmd.Parameters.AddWithValue("@preparation_laboratory_id", DB.MakeParam(typeof(Guid), PreparationLaboratoryId));
                    cmd.Parameters.AddWithValue("@comment", DB.MakeParam(typeof(String), Comment));                    
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = Assignment.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "assignment_preparation_method", Id, AuditOperationType.Update, json, "");

                    Dirty = false;
                }
            }

            foreach (AssignmentAnalysisMethod aam in AnalysisMethods)
                aam.StoreToDB(conn, trans);

            // Remove deleted prep methods from DB
            List<Guid> storedAnalMethIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_analysis_method where assignment_preparation_method_id = @id", CommandType.Text,
                new SqlParameter("@id", Id)))
            {
                while (reader.Read())
                    storedAnalMethIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            cmd.CommandText = "delete from assignment_analysis_method where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid aamId in storedAnalMethIds)
            {
                if (AnalysisMethods.FindIndex(x => x.Id == aamId) == -1)
                {
                    string json = AssignmentAnalysisMethod.ToJSON(conn, trans, aamId);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "assignment_analysis_method", aamId, AuditOperationType.Delete, json, "");

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", aamId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid apmId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_preparation_method", CommandType.StoredProcedure,
                    new SqlParameter("@id", apmId)))
            {
                if(!reader.HasRows)
                    throw new Exception("Error: Assignment preparation method with id " + apmId.ToString() + " was not found");

                reader.Read();

                Id = Guid.Parse(reader["id"].ToString());
                AssignmentSampleTypeId = Guid.Parse(reader["assignment_sample_type_id"].ToString());
                PreparationMethodId = Guid.Parse(reader["preparation_method_id"].ToString());
                PreparationMethodCount = Convert.ToInt32(reader["preparation_method_count"]);
                if (DB.IsValidField(reader["preparation_laboratory_id"]))
                    PreparationLaboratoryId = Guid.Parse(reader["preparation_laboratory_id"].ToString());
                else PreparationLaboratoryId = Guid.Empty;
                Comment = reader["comment"].ToString();
                CreateDate = Convert.ToDateTime(reader["create_date"]);
                CreatedBy = reader["created_by"].ToString();
                UpdateDate = Convert.ToDateTime(reader["update_date"]);
                UpdatedBy = reader["updated_by"].ToString();
                Dirty = false;
            }

            List<Guid> analMethIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_analysis_method where assignment_preparation_method_id = @apmid", CommandType.Text,
                new SqlParameter("@apmid", apmId)))
            {
                while (reader.Read())
                    analMethIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            foreach (Guid aamId in analMethIds)
            {
                AssignmentAnalysisMethod aam = new AssignmentAnalysisMethod();
                aam.LoadFromDB(conn, trans, aamId);
                AnalysisMethods.Add(aam);
            }
        }        
    }
}
