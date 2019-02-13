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
    public class Preparation
    {
        public Preparation()
        {
            Id = Guid.NewGuid();
            Dirty = false;
        }

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

        public bool Dirty;

        public bool IsDirty()
        {
            return Dirty;
        }

        public void ClearDirty()
        {
            Dirty = false;
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

                    Dirty = false;                    
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
}
