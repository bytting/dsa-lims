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
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public DateTime? SamplingDateFrom { get; set; }
        public DateTime? SamplingDateTo { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public string ExternalId { get; set; }
        public double? WetWeight_g { get; set; }
        public double? DryWeight_g { get; set; }
        public double? Volume_l { get; set; }
        public double? LodWeightStart { get; set; }
        public double? LodWeightEnd { get; set; }
        public double? LodTemperature { get; set; }
        public bool Confidential { get; set; }
        public string Parameters { get; set; }
        public int InstanceStatusId { get; set; }
        public Guid LockedId { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateId { get; set; }
        public DateTime UpdateDate { get; set; }
        public Guid UpdateId { get; set; }

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

        public bool HasRequiredFields()
        {
            if (Number <= 0 || !Utils.IsValidGuid(SampleTypeId) || !Utils.IsValidGuid(ProjectSubId) || !Utils.IsValidGuid(LaboratoryId) 
                || ReferenceDate == null || ReferenceDate.Value == DateTime.MinValue)
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

                Id = reader.GetGuid("id");
                Number = reader.GetInt32("number");
                LaboratoryId = reader.GetGuid("laboratory_id");
                SampleTypeId = reader.GetGuid("sample_type_id");
                SampleStorageId = reader.GetGuid("sample_storage_id");
                SampleComponentId = reader.GetGuid("sample_component_id");
                ProjectSubId = reader.GetGuid("project_sub_id");
                StationId = reader.GetGuid("station_id");
                SamplerId = reader.GetGuid("sampler_id");
                SamplingMethodId = reader.GetGuid("sampling_method_id");
                TransformFromId = reader.GetGuid("transform_from_id");
                TransformToId = reader.GetGuid("transform_to_id");
                ImportedFrom = reader.GetString("imported_from");
                ImportedFromId = reader.GetString("imported_from_id");
                MunicipalityId = reader.GetGuid("municipality_id");
                LocationType = reader.GetString("location_type");
                Location = reader.GetString("location");                
                Latitude = reader.GetDoubleNullable("latitude");                
                Longitude = reader.GetDoubleNullable("longitude");                
                Altitude = reader.GetDoubleNullable("altitude");                
                SamplingDateFrom = reader.GetDateTimeNullable("sampling_date_from");                
                SamplingDateTo = reader.GetDateTimeNullable("sampling_date_to");                
                ReferenceDate = reader.GetDateTimeNullable("reference_date");
                ExternalId = reader.GetString("external_id");
                WetWeight_g = reader.GetDoubleNullable("wet_weight_g");
                DryWeight_g = reader.GetDoubleNullable("dry_weight_g");
                Volume_l = reader.GetDoubleNullable("volume_l");
                LodWeightStart = reader.GetDoubleNullable("lod_weight_start");
                LodWeightEnd = reader.GetDoubleNullable("lod_weight_end");
                LodTemperature = reader.GetDoubleNullable("lod_temperature");
                Confidential = reader.GetBoolean("confidential");
                Parameters = reader.GetString("parameters");
                InstanceStatusId = reader.GetInt32("instance_status_id");
                LockedId = reader.GetGuid("locked_id");
                Comment = reader.GetString("comment");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
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
                // insert new sample
                cmd.CommandText = "csp_insert_sample";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@number", Number, null);
                cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_storage_id", SampleStorageId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_component_id", SampleComponentId, Guid.Empty);
                cmd.Parameters.AddWithValue("@project_sub_id", ProjectSubId, Guid.Empty);
                cmd.Parameters.AddWithValue("@station_id", StationId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sampler_id", SamplerId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sampling_method_id", SamplingMethodId, Guid.Empty);
                cmd.Parameters.AddWithValue("@transform_from_id", TransformFromId, Guid.Empty);
                cmd.Parameters.AddWithValue("@transform_to_id", TransformToId, Guid.Empty);
                cmd.Parameters.AddWithValue("@imported_from", ImportedFrom, String.Empty);
                cmd.Parameters.AddWithValue("@imported_from_id", ImportedFromId, String.Empty);
                cmd.Parameters.AddWithValue("@municipality_id", MunicipalityId, Guid.Empty);
                cmd.Parameters.AddWithValue("@location_type", LocationType, String.Empty);
                cmd.Parameters.AddWithValue("@location", Location, String.Empty);
                cmd.Parameters.AddWithValue("@latitude", Latitude, null);
                cmd.Parameters.AddWithValue("@longitude", Longitude, null);
                cmd.Parameters.AddWithValue("@altitude", Altitude, null);
                cmd.Parameters.AddWithValue("@sampling_date_from", SamplingDateFrom, null);
                cmd.Parameters.AddWithValue("@sampling_date_to", SamplingDateTo, null);
                cmd.Parameters.AddWithValue("@reference_date", ReferenceDate, null);
                cmd.Parameters.AddWithValue("@external_id", ExternalId, String.Empty);
                cmd.Parameters.AddWithValue("@wet_weight_g", WetWeight_g, null);
                cmd.Parameters.AddWithValue("@dry_weight_g", DryWeight_g, null);
                cmd.Parameters.AddWithValue("@volume_l", Volume_l, null);
                cmd.Parameters.AddWithValue("@lod_weight_start", LodWeightStart, null);
                cmd.Parameters.AddWithValue("@lod_weight_end", LodWeightEnd, null);
                cmd.Parameters.AddWithValue("@lod_temperature", LodTemperature, null);
                cmd.Parameters.AddWithValue("@confidential", Confidential, null);
                cmd.Parameters.AddWithValue("@parameters", Parameters, String.Empty);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId, null);
                cmd.Parameters.AddWithValue("@locked_id", LockedId, Guid.Empty);
                cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

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
                    // update existing sample
                    cmd.CommandText = "csp_update_sample";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", Id);                    
                    cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_type_id", SampleTypeId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_storage_id", SampleStorageId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_component_id", SampleComponentId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@project_sub_id", ProjectSubId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@station_id", StationId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sampler_id", SamplerId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sampling_method_id", SamplingMethodId, Guid.Empty);                    
                    cmd.Parameters.AddWithValue("@municipality_id", MunicipalityId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@location_type", LocationType, String.Empty);
                    cmd.Parameters.AddWithValue("@location", Location, String.Empty);
                    cmd.Parameters.AddWithValue("@latitude", Latitude, null);
                    cmd.Parameters.AddWithValue("@longitude", Longitude, null);
                    cmd.Parameters.AddWithValue("@altitude", Altitude, null);
                    cmd.Parameters.AddWithValue("@sampling_date_from", SamplingDateFrom, DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@sampling_date_to", SamplingDateTo, DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@reference_date", ReferenceDate, DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@external_id", ExternalId, String.Empty);                    
                    cmd.Parameters.AddWithValue("@confidential", Confidential, null);                    
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId, null);                    
                    cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

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
            cmd.Parameters.AddWithValue("@wet_weight_g", WetWeight_g, null);
            cmd.Parameters.AddWithValue("@dry_weight_g", DryWeight_g, null);
            cmd.Parameters.AddWithValue("@volume_l", Volume_l, null);
            cmd.Parameters.AddWithValue("@lod_weight_start", LodWeightStart, null);
            cmd.Parameters.AddWithValue("@lod_weight_end", LodWeightEnd, null);
            cmd.Parameters.AddWithValue("@lod_temperature", LodTemperature, null);
            cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
            cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

            cmd.ExecuteNonQuery();

            Dirty = false;
        }
    }
}
