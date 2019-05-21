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
    public class Preparation
    {
        public Preparation()
        {
            Id = Guid.NewGuid();
            Dirty = false;

            Analyses = new List<Analysis>();
        }

        public Guid Id { get; set; }
        public Guid SampleId { get; set; }
        public int Number { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid PreparationGeometryId { get; set; }
        public Guid PreparationMethodId { get; set; }
        public int WorkflowStatusId { get; set; }
        public double? Amount { get; set; }
        public int PrepUnitId { get; set; }
        public double? Quantity { get; set; }
        public int QuantityUnitId { get; set; }
        public double? FillHeightMM { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateId { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid UpdateId { get; set; }

        public List<Analysis> Analyses { get; set; }

        public bool Dirty;

        public bool IsDirty()
        {
            if (Dirty)
                return true;

            foreach (Analysis a in Analyses)
                if (a.IsDirty())
                    return true;

            return false;
        }

        public void ClearDirty()
        {
            Dirty = false;

            foreach (Analysis a in Analyses)
                a.ClearDirty();
        }        

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid prepId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from preparation where id = @id", CommandType.Text, new SqlParameter("@id", prepId));
            return cnt > 0;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid prepId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_preparation", CommandType.StoredProcedure,
                new SqlParameter("@id", prepId)))
            {
                if (!reader.HasRows)
                    throw new Exception("Error: Preparation with id " + prepId.ToString() + " was not found");
                
                reader.Read();

                Id = reader.GetGuid("id");
                SampleId = reader.GetGuid("sample_id");
                Number = reader.GetInt32("number");
                AssignmentId = reader.GetGuid("assignment_id");
                LaboratoryId = reader.GetGuid("laboratory_id");
                PreparationGeometryId = reader.GetGuid("preparation_geometry_id");
                PreparationMethodId = reader.GetGuid("preparation_method_id");
                WorkflowStatusId = reader.GetInt32("workflow_status_id");
                Amount = reader.GetDoubleNullable("amount");
                PrepUnitId = reader.GetInt32("prep_unit_id");
                Quantity = reader.GetDoubleNullable("quantity");
                QuantityUnitId = reader.GetInt32("quantity_unit_id");
                FillHeightMM = reader.GetDoubleNullable("fill_height_mm");
                InstanceStatusId = reader.GetInt32("instance_status_id");
                Comment = reader.GetString("comment");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
            }

            // Load analyses
            Analyses.Clear();

            List<Guid> analysisIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from analysis where preparation_id = @id", CommandType.Text,
                new SqlParameter("@id", Id)))
            {
                while (reader.Read())
                    analysisIds.Add(reader.GetGuid("id"));
            }

            foreach (Guid aId in analysisIds)
            {
                Analysis a = new Analysis();
                a.LoadFromDB(conn, trans, aId);
                Analyses.Add(a);
            }
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                throw new Exception("Error: Can not store a preparation with empty id");
            
            SqlCommand cmd = new SqlCommand("", conn, trans);            

            if (!Preparation.IdExists(conn, trans, Id))
            {
                // Insert new preparation
                cmd.CommandText = "csp_insert_preparation";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@sample_id", SampleId, Guid.Empty);
                cmd.Parameters.AddWithValue("@number", Number);
                cmd.Parameters.AddWithValue("@assignment_id", AssignmentId, Guid.Empty);
                cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                cmd.Parameters.AddWithValue("@preparation_geometry_id", PreparationGeometryId, Guid.Empty);
                cmd.Parameters.AddWithValue("@preparation_method_id", PreparationMethodId, Guid.Empty);
                cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                cmd.Parameters.AddWithValue("@amount", Amount, null);
                cmd.Parameters.AddWithValue("@prep_unit_id", PrepUnitId);
                cmd.Parameters.AddWithValue("@quantity", Quantity, null);
                cmd.Parameters.AddWithValue("@quantity_unit_id", QuantityUnitId);
                cmd.Parameters.AddWithValue("@fill_height_mm", FillHeightMM, null);
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
                    // Update existing preparation
                    cmd.CommandText = "csp_update_preparation";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@preparation_geometry_id", PreparationGeometryId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                    cmd.Parameters.AddWithValue("@amount", Amount, null);
                    cmd.Parameters.AddWithValue("@prep_unit_id", PrepUnitId);
                    cmd.Parameters.AddWithValue("@quantity", Quantity, null);
                    cmd.Parameters.AddWithValue("@quantity_unit_id", QuantityUnitId);
                    cmd.Parameters.AddWithValue("@fill_height_mm", FillHeightMM, null);
                    cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                    cmd.ExecuteNonQuery();

                    Dirty = false;                    
                }
            }

            foreach (Analysis a in Analyses)
                a.StoreToDB(conn, trans);

            // Remove deleted analyses from DB
            List<Guid> storedAnalIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from analysis where preparation_id = @id", CommandType.Text,
                new SqlParameter("@id", Id)))
            {
                while (reader.Read())
                    storedAnalIds.Add(reader.GetGuid("id"));
            }

            cmd.CommandText = "update analysis set instance_status_id = @status where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid aId in storedAnalIds)
            {
                if (Analyses.FindIndex(x => x.Id == aId) == -1)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@status", InstanceStatus.Deleted);
                    cmd.Parameters.AddWithValue("@id", aId);
                    cmd.ExecuteNonQuery();
                }
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
    inner join preparation p on p.sample_id = s.id and p.id = @pid
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
    inner join preparation p on p.sample_id = s.id and p.id = @pid
where p.assignment_id = ast.assignment_id
";
            o = cmd.ExecuteScalar();
            if (DB.IsValidField(o))
                requnit += o.ToString();

            return requnit;
        }
    }
}
