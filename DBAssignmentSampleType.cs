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
    public class AssignmentSampleType
    {
        public AssignmentSampleType()
        {
            Id = Guid.NewGuid();
            Dirty = false;
            PreparationMethods = new List<AssignmentPreparationMethod>();
        }
        
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }        
        public Guid SampleTypeId { get; set; }        
        public Guid SampleComponentId { get; set; }        
        public int SampleCount { get; set; }        
        public Guid RequestedActivityUnitId { get; set; }        
        public Guid RequestedActivityUnitTypeId { get; set; }        
        public bool ReturnToSender { get; set; }        
        public string Comment { get; set; }        
        public DateTime CreateDate { get; set; }        
        public Guid CreateId { get; set; }        
        public DateTime UpdateDate { get; set; }        
        public Guid UpdateId { get; set; }
        
        public List<AssignmentPreparationMethod> PreparationMethods { get; set; }

        public bool Dirty;

        public bool IsDirty()
        {            
            if (Dirty)
                return true;

            foreach (AssignmentPreparationMethod apm in PreparationMethods)
                if (apm.IsDirty())
                    return true;

            return false;
        }

        public void ClearDirty()
        {
            Dirty = false;
            foreach (AssignmentPreparationMethod apm in PreparationMethods)
                apm.ClearDirty();
        }

        public string SampleTypeName(SqlConnection conn, SqlTransaction trans)
        {
            if (SampleTypeId == null)
                return "";

            object o = DB.GetScalar(conn, trans, "select name from sample_type where id = @sid", CommandType.Text, new SqlParameter("@sid", SampleTypeId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public string SampleTypePath(SqlConnection conn, SqlTransaction trans)
        {
            if (SampleTypeId == null)
                return "";

            object o = DB.GetScalar(conn, trans, "select path from sample_type where id = @sid", CommandType.Text, new SqlParameter("@sid", SampleTypeId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public string SampleComponentName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from sample_component where id = @scid", CommandType.Text, new SqlParameter("@scid", SampleComponentId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }        

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid astId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from assignment_sample_type where id = @id", CommandType.Text, new SqlParameter("@id", astId));
            return cnt > 0;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!AssignmentSampleType.IdExists(conn, trans, Id))
            {
                // Insert new ast
                cmd.CommandText = "csp_insert_assignment_sample_type";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);                
                cmd.Parameters.AddWithValue("@assignment_id", AssignmentId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_component_id", SampleComponentId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_count", SampleCount);
                cmd.Parameters.AddWithValue("@requested_activity_unit_id", RequestedActivityUnitId, Guid.Empty);
                cmd.Parameters.AddWithValue("@requested_activity_unit_type_id", RequestedActivityUnitTypeId, Guid.Empty);
                cmd.Parameters.AddWithValue("@return_to_sender", ReturnToSender);
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
                    // Update existing ast
                    cmd.CommandText = "csp_update_assignment_sample_type";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@assignment_id", AssignmentId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_component_id", SampleComponentId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_count", SampleCount);
                    cmd.Parameters.AddWithValue("@requested_activity_unit_id", RequestedActivityUnitId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@requested_activity_unit_type_id", RequestedActivityUnitTypeId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@return_to_sender", ReturnToSender);
                    cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                    cmd.ExecuteNonQuery();

                    Dirty = false;
                }
            }

            foreach (AssignmentPreparationMethod apm in PreparationMethods)
                apm.StoreToDB(conn, trans);

            // Remove deleted prep methods from DB
            List<Guid> storedPrepMethIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_preparation_method where assignment_sample_type_id = @id", CommandType.Text,
                new SqlParameter("@id", Id)))
            {
                while (reader.Read())
                    storedPrepMethIds.Add(reader.GetGuid("id"));
            }

            cmd.CommandText = "delete from assignment_preparation_method where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid apmId in storedPrepMethIds)
            {
                if (PreparationMethods.FindIndex(x => x.Id == apmId) == -1)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", apmId);
                    cmd.ExecuteNonQuery();
                }
            }            
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid astId)
        {
            if(!AssignmentSampleType.IdExists(conn, trans, astId))            
                throw new Exception("Assignment sample type with id " + astId.ToString() + " was not found");

            PreparationMethods.Clear();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_sample_type", CommandType.StoredProcedure,
                new SqlParameter("@id", astId)))
            {
                if(!reader.HasRows)                
                    throw new Exception("Assignment sample type with id " + astId.ToString() + " was not found");
                
                reader.Read();
                                     
                Id = reader.GetGuid("id");
                AssignmentId = reader.GetGuid("assignment_id");
                SampleTypeId = reader.GetGuid("sample_type_id");                
                SampleComponentId = reader.GetGuid("sample_component_id");                
                SampleCount = reader.GetInt32("sample_count");                
                RequestedActivityUnitId = reader.GetGuid("requested_activity_unit_id");                                
                RequestedActivityUnitTypeId = reader.GetGuid("requested_activity_unit_type_id");                
                ReturnToSender = reader.GetBoolean("return_to_sender");
                Comment = reader.GetString("comment");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
                Dirty = false;
            }

            List<Guid> prepMethIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_preparation_method where assignment_sample_type_id = @astId", CommandType.Text,
                    new SqlParameter("@astId", astId)))
            {
                while (reader.Read())                
                    prepMethIds.Add(reader.GetGuid("id"));
            }

            foreach (Guid apmId in prepMethIds)
            {
                AssignmentPreparationMethod apm = new AssignmentPreparationMethod();
                apm.LoadFromDB(conn, trans, apmId);
                PreparationMethods.Add(apm);
            }
        }        
    }
}