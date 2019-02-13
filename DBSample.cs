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
    public class Sample
    {
        public Sample()
        {
            Id = Guid.NewGuid();
            Dirty = false;
            SamplingDateFrom = DateTime.MinValue;
            SamplingDateTo = DateTime.MinValue;
            ReferenceDate = DateTime.MinValue;
            InstanceStatusId = InstanceStatus.Active;
        }

        public Guid Id { get; set; }
        public int Number { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid SampleTypeId { get; set; }
        public Guid SampleStorageId { get; set; }
        public Guid SampleComponentId { get; set; }
        public Guid ProjectSubId { get; set; }
        public Guid StationId { get; set; }
        public Guid SamplerId { get; set; }
        public Guid SamplingMethodId { get; set; }
        public Guid TransformFromId { get; set; }
        public Guid TransformToId { get; set; }
        public string ImportedFrom { get; set; }
        public string ImportedFromId { get; set; }
        public Guid MunicipalityId { get; set; }
        public string LocationType { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public DateTime SamplingDateFrom { get; set; }
        public DateTime SamplingDateTo { get; set; }
        public DateTime ReferenceDate { get; set; }
        public string ExternalId { get; set; }
        public double WetWeight_g { get; set; }
        public double DryWeight_g { get; set; }
        public double Volume_l { get; set; }
        public double LodWeightStart { get; set; }
        public double LodWeightEnd { get; set; }
        public double LodTemperature { get; set; }
        public bool Confidential { get; set; }
        public string Parameters { get; set; }
        public int InstanceStatusId { get; set; }
        public string LockedBy { get; set; }
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

        public string GetSampleComponentName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from sample_component where id = @id", CommandType.Text, new SqlParameter("@id", SampleComponentId));
            if (!DB.IsValidField(o))
                return "";
            return o.ToString();
        }

        public string GetProjectName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, @"
select pm.name + ' - ' + ps.name
from project_sub ps
    inner join project_main pm on pm.id = ps.project_main_id
where ps.id = @psid
", CommandType.Text, new SqlParameter("@psid", ProjectSubId));

            if (!DB.IsValidField(o))
                return "";
            return o.ToString();
        }

        public Guid GetAssignmentLaboratory(SqlConnection conn, SqlTransaction trans)
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
            cmd.Parameters.AddWithValue("@sample_id", Id);
            object o = cmd.ExecuteScalar();
            if (!DB.IsValidField(o))
                return Guid.Empty;

            return Guid.Parse(o.ToString());
        }

        public bool HasRequiredFields()
        {
            if (Number <= 0
                || !Utils.IsValidGuid(SampleTypeId)
                || !Utils.IsValidGuid(ProjectSubId)
                || !Utils.IsValidGuid(LaboratoryId)
                || ReferenceDate == DateTime.MinValue)
                return false;

            return true;
        }

        public static bool HasRequiredFields(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            string query = "select number, sample_type_id, project_sub_id, laboratory_id, reference_date from sample where id = @id";

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, query, CommandType.Text, new SqlParameter("@id", sampleId)))
            {
                if (!reader.HasRows)
                    return false;

                reader.Read();

                if (!DB.IsValidField(reader["number"])
                    || !DB.IsValidField(reader["sample_type_id"])
                    || !DB.IsValidField(reader["project_sub_id"])
                    || !DB.IsValidField(reader["laboratory_id"])
                    || !DB.IsValidField(reader["reference_date"]))
                    return false;

                if (!Utils.IsValidGuid(reader["sample_type_id"])
                    || !Utils.IsValidGuid(reader["project_sub_id"])
                    || !Utils.IsValidGuid(reader["laboratory_id"]))
                    return false;
            }

            return true;
        }

        public double CalculateLODPercent()
        {
            if (LodWeightStart == 0d && LodWeightEnd == 0d)
                return -1;            

            if (LodWeightStart < LodWeightEnd)            
                return -1;

            double delta = LodWeightStart - LodWeightEnd;
            return (delta / LodWeightStart) * 100.0;
        }

        public string GetLODPercentString()
        {
            double percent = CalculateLODPercent();
            return percent == -1 ? "" : percent.ToString("0.0#");
        }

        public bool HasOrders(SqlConnection conn, SqlTransaction trans)
        {
            string query = "select count(*) from sample_x_assignment_sample_type where sample_id = @sid";
            SqlCommand cmd = new SqlCommand(query, conn, trans);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@sid", Id);

            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value)
                return false;

            return Convert.ToInt32(o) > 0;
        }

        public bool IsClosed(SqlConnection conn, SqlTransaction trans)
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
            cmd.Parameters.AddWithValue("@sid", Id);

            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value)
                return false;

            return Convert.ToInt32(o) > 0;
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", sampleId)))
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

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from sample where id = @id", CommandType.Text, 
                new SqlParameter("@id", sampleId));
            return cnt > 0;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid sampleId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample", CommandType.StoredProcedure,
                new SqlParameter("@id", sampleId)))
            {
                if (!reader.HasRows)
                    throw new Exception("Error: Sample with id " + sampleId.ToString() + " was not found");

                reader.Read();

                Id = Guid.Parse(reader["id"].ToString());
                Number = Convert.ToInt32(reader["number"]);
                LaboratoryId = Guid.Parse(reader["laboratory_id"].ToString());
                SampleTypeId = Guid.Parse(reader["sample_type_id"].ToString());
                SampleStorageId = Utils.MakeGuid(reader["sample_storage_id"]);
                SampleComponentId = Utils.MakeGuid(reader["sample_component_id"]);
                ProjectSubId = Utils.MakeGuid(reader["project_sub_id"]);
                StationId = Utils.MakeGuid(reader["station_id"]);
                SamplerId = Utils.MakeGuid(reader["sampler_id"]);
                SamplingMethodId = Utils.MakeGuid(reader["sampling_method_id"]);
                TransformFromId = Utils.MakeGuid(reader["transform_from_id"]);
                TransformToId = Utils.MakeGuid(reader["transform_to_id"]);
                ImportedFrom = reader["imported_from"].ToString();
                ImportedFromId = reader["imported_from_id"].ToString();
                MunicipalityId = Utils.MakeGuid(reader["municipality_id"]);
                LocationType = reader["location_type"].ToString();
                Location = reader["location"].ToString();
                if(DB.IsValidField(reader["latitude"]))
                    Latitude = Convert.ToDouble(reader["latitude"]);
                if (DB.IsValidField(reader["longitude"]))
                    Longitude = Convert.ToDouble(reader["longitude"]);
                if (DB.IsValidField(reader["altitude"]))
                    Altitude = Convert.ToDouble(reader["altitude"]);
                if (DB.IsValidField(reader["sampling_date_from"]))
                    SamplingDateFrom = Convert.ToDateTime(reader["sampling_date_from"]);
                if (DB.IsValidField(reader["sampling_date_to"]))
                    SamplingDateTo = Convert.ToDateTime(reader["sampling_date_to"]);
                if (DB.IsValidField(reader["reference_date"]))
                    ReferenceDate = Convert.ToDateTime(reader["reference_date"]);
                ExternalId = reader["external_id"].ToString();
                WetWeight_g = Convert.ToDouble(reader["wet_weight_g"]);
                DryWeight_g = Convert.ToDouble(reader["dry_weight_g"]);
                Volume_l = Convert.ToDouble(reader["volume_l"]);
                LodWeightStart = Convert.ToDouble(reader["lod_weight_start"]);
                LodWeightEnd = Convert.ToDouble(reader["lod_weight_end"]);
                LodTemperature = Convert.ToDouble(reader["lod_temperature"]);
                Confidential = Convert.ToBoolean(reader["confidential"]);
                Parameters = reader["parameters"].ToString();
                InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                LockedBy = reader["locked_by"].ToString();
                Comment = reader["comment"].ToString();
                CreateDate = Convert.ToDateTime(reader["create_date"]);
                CreatedBy = reader["created_by"].ToString();
                UpdateDate = Convert.ToDateTime(reader["update_date"]);
                UpdatedBy = reader["updated_by"].ToString();
            }

            Dirty = false;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                throw new Exception("Error: Can not store a sample with empty id");

            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!Sample.IdExists(conn, trans, Id))
            {
                // insert new analysis result
                cmd.CommandText = "csp_insert_sample";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@number", DB.MakeParam(typeof(int), Number));
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), SampleTypeId));
                cmd.Parameters.AddWithValue("@sample_storage_id", DB.MakeParam(typeof(Guid), SampleStorageId));
                cmd.Parameters.AddWithValue("@sample_component_id", DB.MakeParam(typeof(Guid), SampleComponentId));
                cmd.Parameters.AddWithValue("@project_sub_id", DB.MakeParam(typeof(Guid), ProjectSubId));
                cmd.Parameters.AddWithValue("@station_id", DB.MakeParam(typeof(Guid), StationId));
                cmd.Parameters.AddWithValue("@sampler_id", DB.MakeParam(typeof(Guid), SamplerId));
                cmd.Parameters.AddWithValue("@sampling_method_id", DB.MakeParam(typeof(Guid), SamplingMethodId));
                cmd.Parameters.AddWithValue("@transform_from_id", DB.MakeParam(typeof(Guid), TransformFromId));
                cmd.Parameters.AddWithValue("@transform_to_id", DB.MakeParam(typeof(Guid), TransformToId));
                cmd.Parameters.AddWithValue("@imported_from", DB.MakeParam(typeof(String), ImportedFrom));
                cmd.Parameters.AddWithValue("@imported_from_id", DB.MakeParam(typeof(String), ImportedFromId));
                cmd.Parameters.AddWithValue("@municipality_id", DB.MakeParam(typeof(Guid), MunicipalityId));
                cmd.Parameters.AddWithValue("@location_type", DB.MakeParam(typeof(String), LocationType));
                cmd.Parameters.AddWithValue("@location", DB.MakeParam(typeof(String), Location));
                cmd.Parameters.AddWithValue("@latitude", DB.MakeParam(typeof(double), Latitude));
                cmd.Parameters.AddWithValue("@longitude", DB.MakeParam(typeof(double), Longitude));
                cmd.Parameters.AddWithValue("@altitude", DB.MakeParam(typeof(double), Altitude));
                cmd.Parameters.AddWithValue("@sampling_date_from", DB.MakeParam(typeof(DateTime), SamplingDateFrom));
                cmd.Parameters.AddWithValue("@sampling_date_to", DB.MakeParam(typeof(DateTime), SamplingDateTo));
                cmd.Parameters.AddWithValue("@reference_date", DB.MakeParam(typeof(DateTime), ReferenceDate));
                cmd.Parameters.AddWithValue("@external_id", DB.MakeParam(typeof(String), ExternalId));
                cmd.Parameters.AddWithValue("@wet_weight_g", DB.MakeParam(typeof(double), WetWeight_g));
                cmd.Parameters.AddWithValue("@dry_weight_g", DB.MakeParam(typeof(double), DryWeight_g));
                cmd.Parameters.AddWithValue("@volume_l", DB.MakeParam(typeof(double), Volume_l));
                cmd.Parameters.AddWithValue("@lod_weight_start", DB.MakeParam(typeof(double), LodWeightStart));
                cmd.Parameters.AddWithValue("@lod_weight_end", DB.MakeParam(typeof(double), LodWeightEnd));
                cmd.Parameters.AddWithValue("@lod_temperature", DB.MakeParam(typeof(double), LodTemperature));
                cmd.Parameters.AddWithValue("@confidential", DB.MakeParam(typeof(bool), Confidential));
                cmd.Parameters.AddWithValue("@parameters", DB.MakeParam(typeof(String), Parameters));
                cmd.Parameters.AddWithValue("@instance_status_id", DB.MakeParam(typeof(int), InstanceStatusId));
                cmd.Parameters.AddWithValue("@locked_by", DB.MakeParam(typeof(String), LockedBy));
                cmd.Parameters.AddWithValue("@comment", DB.MakeParam(typeof(String), Comment));
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                string json = Sample.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "sample", Id, AuditOperationType.Insert, json, "");

                Dirty = false;
            }
            else
            {
                if (Dirty)
                {
                    // update existing analysis result
                    cmd.CommandText = "csp_update_sample";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", Id);                    
                    cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                    cmd.Parameters.AddWithValue("@sample_type_id", DB.MakeParam(typeof(Guid), SampleTypeId));
                    cmd.Parameters.AddWithValue("@sample_storage_id", DB.MakeParam(typeof(Guid), SampleStorageId));
                    cmd.Parameters.AddWithValue("@sample_component_id", DB.MakeParam(typeof(Guid), SampleComponentId));
                    cmd.Parameters.AddWithValue("@project_sub_id", DB.MakeParam(typeof(Guid), ProjectSubId));
                    cmd.Parameters.AddWithValue("@station_id", DB.MakeParam(typeof(Guid), StationId));
                    cmd.Parameters.AddWithValue("@sampler_id", DB.MakeParam(typeof(Guid), SamplerId));
                    cmd.Parameters.AddWithValue("@sampling_method_id", DB.MakeParam(typeof(Guid), SamplingMethodId));                    
                    cmd.Parameters.AddWithValue("@municipality_id", DB.MakeParam(typeof(Guid), MunicipalityId));
                    cmd.Parameters.AddWithValue("@location_type", DB.MakeParam(typeof(String), LocationType));
                    cmd.Parameters.AddWithValue("@location", DB.MakeParam(typeof(String), Location));
                    cmd.Parameters.AddWithValue("@latitude", DB.MakeParam(typeof(double), Latitude));
                    cmd.Parameters.AddWithValue("@longitude", DB.MakeParam(typeof(double), Longitude));
                    cmd.Parameters.AddWithValue("@altitude", DB.MakeParam(typeof(double), Altitude));
                    cmd.Parameters.AddWithValue("@sampling_date_from", DB.MakeParam(typeof(DateTime), SamplingDateFrom));
                    cmd.Parameters.AddWithValue("@sampling_date_to", DB.MakeParam(typeof(DateTime), SamplingDateTo));
                    cmd.Parameters.AddWithValue("@reference_date", DB.MakeParam(typeof(DateTime), ReferenceDate));
                    cmd.Parameters.AddWithValue("@external_id", DB.MakeParam(typeof(String), ExternalId));                    
                    cmd.Parameters.AddWithValue("@confidential", DB.MakeParam(typeof(bool), Confidential));                    
                    cmd.Parameters.AddWithValue("@instance_status_id", DB.MakeParam(typeof(int), InstanceStatusId));                    
                    cmd.Parameters.AddWithValue("@comment", DB.MakeParam(typeof(String), Comment));
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = Sample.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "sample", Id, AuditOperationType.Update, json, "");

                    Dirty = false;
                }
            }
        }

        public void StoreLabInfoToDB(SqlConnection conn, SqlTransaction trans)
        {
            if (Id == Guid.Empty)
                throw new Exception("Error: Can not store sample lab info with empty id");

            SqlCommand cmd = new SqlCommand("csp_update_sample_info", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", Id);
            cmd.Parameters.AddWithValue("@wet_weight_g", DB.MakeParam(typeof(double), WetWeight_g));
            cmd.Parameters.AddWithValue("@dry_weight_g", DB.MakeParam(typeof(double), DryWeight_g));
            cmd.Parameters.AddWithValue("@volume_l", DB.MakeParam(typeof(double), Volume_l));
            cmd.Parameters.AddWithValue("@lod_weight_start", DB.MakeParam(typeof(double), LodWeightStart));
            cmd.Parameters.AddWithValue("@lod_weight_end", DB.MakeParam(typeof(double), LodWeightEnd));
            cmd.Parameters.AddWithValue("@lod_temperature", DB.MakeParam(typeof(double), LodTemperature));
            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@updated_by", Common.Username);

            cmd.ExecuteNonQuery();
        }
    }
}
