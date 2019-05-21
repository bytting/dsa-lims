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
    [JsonObject]
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
        public Guid CreateId { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid UpdateId { get; set; }

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
            a.CreateId = CreateId;
            a.UpdateDate = UpdateDate;
            a.UpdateId = UpdateId;
            a.ImportFile = ImportFile;
            a.Dirty = Dirty;
            a.Results.AddRange(Results);
            return a;
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

                Id = reader.GetGuid("id");
                Number = reader.GetInt32("number");
                AssignmentId = reader.GetGuid("assignment_id");
                LaboratoryId = reader.GetGuid("laboratory_id");
                PreparationId = reader.GetGuid("preparation_id");
                AnalysisMethodId = reader.GetGuid("analysis_method_id");
                WorkflowStatusId = reader.GetInt32("workflow_status_id");
                SpecterReference = reader.GetString("specter_reference");
                ActivityUnitId = reader.GetGuid("activity_unit_id");
                ActivityUnitTypeId = reader.GetGuid("activity_unit_type_id");
                SigmaActivity = reader.GetDouble("sigma_act");
                SigmaMDA = reader.GetDouble("sigma_mda");
                NuclideLibrary = reader.GetString("nuclide_library");
                MDALibrary = reader.GetString("mda_library");
                InstanceStatusId = reader.GetInt32("instance_status_id");
                Comment = reader.GetString("comment");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
            }

            Results.Clear();

            List<Guid> analResIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from analysis_result where analysis_id = @id and instance_status_id <= @instance_status_level", CommandType.Text,
                new SqlParameter("@id", analId),
                new SqlParameter("@instance_status_level", InstanceStatus.Active)))
            {
                while (reader.Read())
                    analResIds.Add(reader.GetGuid("id"));
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
                cmd.Parameters.AddWithValue("@number", Number, null);
                cmd.Parameters.AddWithValue("@assignment_id", AssignmentId, Guid.Empty);
                cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                cmd.Parameters.AddWithValue("@preparation_id", PreparationId, Guid.Empty);
                cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId, Guid.Empty);
                cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                cmd.Parameters.AddWithValue("@specter_reference", SpecterReference, String.Empty);
                cmd.Parameters.AddWithValue("@activity_unit_id", ActivityUnitId, Guid.Empty);
                cmd.Parameters.AddWithValue("@activity_unit_type_id", ActivityUnitTypeId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sigma_act", SigmaActivity);
                cmd.Parameters.AddWithValue("@sigma_mda", SigmaMDA);
                cmd.Parameters.AddWithValue("@nuclide_library", NuclideLibrary, String.Empty);
                cmd.Parameters.AddWithValue("@mda_library", MDALibrary, String.Empty);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                cmd.ExecuteNonQuery();

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
                    cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                    cmd.Parameters.AddWithValue("@specter_reference", SpecterReference, String.Empty);
                    cmd.Parameters.AddWithValue("@activity_unit_id", ActivityUnitId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@activity_unit_type_id", ActivityUnitTypeId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sigma_act", SigmaActivity);
                    cmd.Parameters.AddWithValue("@sigma_mda", SigmaMDA);
                    cmd.Parameters.AddWithValue("@nuclide_library", NuclideLibrary, String.Empty);
                    cmd.Parameters.AddWithValue("@mda_library", MDALibrary, String.Empty);
                    cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, String.Empty);

                    cmd.ExecuteNonQuery();

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
                    storedAnalResIds.Add(reader.GetGuid("id"));
            }

            cmd.CommandText = "update analysis_result set instance_status_id = @status where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid arId in storedAnalResIds)
            {
                if(Results.FindIndex(x => x.Id == arId) == -1)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
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
