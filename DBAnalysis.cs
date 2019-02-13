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
using Newtonsoft.Json;

namespace DSA_lims
{
    public class Analysis
    {
        public Analysis()
        {
            Id = Guid.NewGuid();
            Results = new List<AnalysisResult>();
            Dirty = false;
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

        public string ImportFile;
        public bool Dirty;

        public bool IsDirty()
        {            
            if (Dirty)
                return true;

            foreach (AnalysisResult r in Results)                
                if (r.IsDirty())
                    return true;

            return false;
        }

        public void ClearDirty()
        {
            Dirty = false;
            foreach (AnalysisResult ar in Results)
                ar.ClearDirty();
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
            a.ImportFile = ImportFile;
            a.Dirty = Dirty;
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

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid analId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from analysis where id = @id", CommandType.Text, new SqlParameter("@id", analId));
            return cnt > 0;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid analId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_analysis", CommandType.StoredProcedure,
                new SqlParameter("@id", analId)))
            {
                if (!reader.HasRows)
                    throw new Exception("Error: Analysis with id " + analId.ToString() + " was not found");
                                
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

            Results.Clear();

            List<Guid> analResIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from analysis_result where analysis_id = @id", CommandType.Text,
                new SqlParameter("@id", analId)))
            {
                while (reader.Read())
                    analResIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            foreach (Guid analResId in analResIds)
            {
                AnalysisResult ar = new AnalysisResult();
                ar.LoadFromDB(conn, trans, analResId);
                Results.Add(ar);
            }

            ImportFile = String.Empty;
            Dirty = false;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                throw new Exception("Error: Can not store an analysis with empty id");

            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!Analysis.IdExists(conn, trans, Id))
            {
                // Insert new analysis
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

                Dirty = false;
            }
            else
            {
                if (Dirty)
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

                    Dirty = false;
                }
            }
           
            foreach (AnalysisResult r in Results)            
                r.StoreToDB(conn, trans);

            // Remove deleted analysis results from DB
            List<Guid> storedAnalResIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from analysis_result where analysis_id = @id", CommandType.Text, 
                new SqlParameter("@id", Id)))
            {
                while(reader.Read())                
                    storedAnalResIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            cmd.CommandText = "delete from analysis_result where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid arId in storedAnalResIds)
            {
                if(Results.FindIndex(x => x.Id == arId) == -1)
                {
                    string json = AnalysisResult.ToJSON(conn, trans, arId);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "analysis_result", arId, AuditOperationType.Delete, json, "");

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", arId);
                    cmd.ExecuteNonQuery();
                }
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
}
