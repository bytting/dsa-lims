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
    public class Assignment
    {
        public Assignment()
        {
            Id = Guid.NewGuid();

            SampleTypes = new List<AssignmentSampleType>();

            Dirty = false;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LaboratoryId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime Deadline { get; set; }
        public double RequestedSigmaAct { get; set; }
        public double RequestedSigmaMDA { get; set; }
        public string CustomerCompanyName { get; set; }
        public string CustomerCompanyEmail { get; set; }
        public string CustomerCompanyPhone { get; set; }
        public string CustomerCompanyAddress { get; set; }
        public string CustomerContactName { get; set; }
        public string CustomerContactEmail { get; set; }
        public string CustomerContactPhone { get; set; }
        public string CustomerContactAddress { get; set; }
        public bool ApprovedCustomer { get; set; }
        public string ApprovedCustomerBy { get; set; }
        public bool ApprovedLaboratory { get; set; }
        public string ApprovedLaboratoryBy { get; set; }
        public string ContentComment { get; set; }
        public string ReportComment { get; set; }
        public string AuditComment { get; set; }
        public int WorkflowStatusId { get; set; }
        public DateTime LastWorkflowStatusDate { get; set; }
        public string LastWorkflowStatusBy { get; set; }
        public int AnalysisReportVersion { get; set; }
        public int InstanceStatusId { get; set; }
        public string LockedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public List<AssignmentSampleType> SampleTypes { get; set; }                

        public bool Dirty { get; set; }

        public bool IsDirty
        {
            get
            {
                if (Dirty)
                    return true;

                foreach (AssignmentSampleType ast in SampleTypes)
                    if (ast.IsDirty)
                        return true;

                return false;
            }
        }

        public void Clear()
        {
            Id = Guid.Empty;
            Name = String.Empty;
            LaboratoryId = Guid.Empty;
            AccountId = Guid.Empty;
            Deadline = DateTime.MinValue;
            RequestedSigmaAct = 0d;
            RequestedSigmaMDA = 0d;
            CustomerCompanyName = String.Empty;
            CustomerCompanyEmail = String.Empty;
            CustomerCompanyPhone = String.Empty;
            CustomerCompanyAddress = String.Empty;
            CustomerContactName = String.Empty;
            CustomerContactEmail = String.Empty;
            CustomerContactPhone = String.Empty;
            CustomerContactAddress = String.Empty;
            ApprovedCustomer = false;
            ApprovedCustomerBy = String.Empty;
            ApprovedLaboratory = false;
            ApprovedLaboratoryBy = String.Empty;
            ContentComment = String.Empty;
            ReportComment = String.Empty;
            AuditComment = String.Empty;
            WorkflowStatusId = WorkflowStatus.Construction;
            LastWorkflowStatusDate = DateTime.MinValue;
            LastWorkflowStatusBy = String.Empty;
            AnalysisReportVersion = 0;
            InstanceStatusId = InstanceStatus.Active;
            LockedBy = String.Empty;
            CreateDate = DateTime.MinValue;
            CreatedBy = String.Empty;
            UpdateDate = DateTime.MinValue;
            UpdatedBy = String.Empty;

            SampleTypes.Clear();
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid assignmentId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", assignmentId)))
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

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid assId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from assignment where id = @id", CommandType.Text, new SqlParameter("@id", assId));
            return cnt > 0;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid assId)
        {
            Clear();            

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment", CommandType.StoredProcedure,
                new SqlParameter("@id", assId)))
            {
                if(!reader.HasRows)
                    throw new Exception("Error: Assignment with id " + assId.ToString() + " was not found");
                
                reader.Read();

                Id = Guid.Parse(reader["id"].ToString());
                Name = reader["name"].ToString();
                LaboratoryId = Guid.Parse(reader["laboratory_id"].ToString());
                AccountId = Guid.Parse(reader["account_id"].ToString());
                Deadline = Convert.ToDateTime(reader["deadline"]);
                RequestedSigmaAct = Convert.ToDouble(reader["requested_sigma_act"]);
                RequestedSigmaMDA = Convert.ToDouble(reader["requested_sigma_mda"]);
                CustomerCompanyName = reader["customer_company_name"].ToString();
                CustomerCompanyEmail = reader["customer_company_email"].ToString();
                CustomerCompanyPhone = reader["customer_company_phone"].ToString();
                CustomerCompanyAddress = reader["customer_company_address"].ToString();
                CustomerContactName = reader["customer_contact_name"].ToString();
                CustomerContactEmail = reader["customer_contact_email"].ToString();
                CustomerContactPhone = reader["customer_contact_phone"].ToString();
                CustomerContactAddress = reader["customer_contact_address"].ToString();
                ApprovedCustomer = Convert.ToBoolean(reader["approved_customer"]);
                ApprovedCustomerBy = reader["approved_customer_by"].ToString();
                ApprovedLaboratory = Convert.ToBoolean(reader["approved_laboratory"]);
                ApprovedLaboratoryBy = reader["approved_laboratory_by"].ToString();
                ContentComment = reader["content_comment"].ToString();
                ReportComment = reader["report_comment"].ToString();
                AuditComment = reader["audit_comment"].ToString();
                WorkflowStatusId = Convert.ToInt32(reader["workflow_status_id"]);
                if (DB.IsValidField(reader["last_workflow_status_date"]))
                    LastWorkflowStatusDate = Convert.ToDateTime(reader["last_workflow_status_date"]);
                else LastWorkflowStatusDate = Convert.ToDateTime(reader["create_date"]);
                LastWorkflowStatusBy = reader["last_workflow_status_by"].ToString();
                AnalysisReportVersion = Convert.ToInt32(reader["analysis_report_version"]);
                InstanceStatusId = Convert.ToInt32(reader["instance_status_id"]);
                LockedBy = reader["locked_by"].ToString();
                CreateDate = Convert.ToDateTime(reader["create_date"]);
                CreatedBy = reader["created_by"].ToString();
                UpdateDate = Convert.ToDateTime(reader["update_date"]);
                UpdatedBy = reader["updated_by"].ToString();
            }
            
            List<Guid> sampleTypeIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_sample_type where assignment_id = @id", CommandType.Text,
                new SqlParameter("@id", assId)))
            {
                while (reader.Read())                    
                    sampleTypeIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            foreach (Guid astId in sampleTypeIds)
            {
                AssignmentSampleType ast = new AssignmentSampleType();
                ast.LoadFromDB(conn, trans, astId);
                SampleTypes.Add(ast);
            }

            Dirty = false;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!Assignment.IdExists(conn, trans, Id))
            {
                // Insert new assignment
                cmd.CommandText = "csp_insert_assignment";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@name", Name);
                cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                cmd.Parameters.AddWithValue("@account_id", DB.MakeParam(typeof(Guid), AccountId));
                cmd.Parameters.AddWithValue("@deadline", DB.MakeParam(typeof(DateTime), Deadline));
                cmd.Parameters.AddWithValue("@requested_sigma_act", DB.MakeParam(typeof(double), RequestedSigmaAct));
                cmd.Parameters.AddWithValue("@requested_sigma_mda", DB.MakeParam(typeof(double), RequestedSigmaMDA));
                cmd.Parameters.AddWithValue("@customer_company_name", DB.MakeParam(typeof(String), CustomerCompanyName));
                cmd.Parameters.AddWithValue("@customer_company_email", DB.MakeParam(typeof(String), CustomerCompanyEmail));
                cmd.Parameters.AddWithValue("@customer_company_phone", DB.MakeParam(typeof(String), CustomerCompanyPhone));
                cmd.Parameters.AddWithValue("@customer_company_address", DB.MakeParam(typeof(String), CustomerCompanyAddress));
                cmd.Parameters.AddWithValue("@customer_contact_name", DB.MakeParam(typeof(String), CustomerContactName));
                cmd.Parameters.AddWithValue("@customer_contact_email", DB.MakeParam(typeof(String), CustomerContactEmail));
                cmd.Parameters.AddWithValue("@customer_contact_phone", DB.MakeParam(typeof(String), CustomerContactPhone));
                cmd.Parameters.AddWithValue("@customer_contact_address", DB.MakeParam(typeof(String), CustomerContactAddress));
                cmd.Parameters.AddWithValue("@approved_customer", DB.MakeParam(typeof(bool), ApprovedCustomer));
                cmd.Parameters.AddWithValue("@approved_customer_by", DB.MakeParam(typeof(String), ApprovedCustomerBy));
                cmd.Parameters.AddWithValue("@approved_laboratory", DB.MakeParam(typeof(bool), ApprovedLaboratory));
                cmd.Parameters.AddWithValue("@approved_laboratory_by", DB.MakeParam(typeof(String), ApprovedLaboratoryBy));
                cmd.Parameters.AddWithValue("@content_comment", DB.MakeParam(typeof(String), ContentComment));
                cmd.Parameters.AddWithValue("@report_comment", DB.MakeParam(typeof(String), ReportComment));
                cmd.Parameters.AddWithValue("@audit_comment", DB.MakeParam(typeof(String), AuditComment));
                cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                cmd.Parameters.AddWithValue("@last_workflow_status_date", DB.MakeParam(typeof(DateTime), LastWorkflowStatusDate));
                cmd.Parameters.AddWithValue("@last_workflow_status_by", DB.MakeParam(typeof(String), LastWorkflowStatusBy));
                cmd.Parameters.AddWithValue("@analysis_report_version", DB.MakeParam(typeof(int), AnalysisReportVersion));
                cmd.Parameters.AddWithValue("@instance_status_id", DB.MakeParam(typeof(int), InstanceStatusId));
                cmd.Parameters.AddWithValue("@locked_by", DB.MakeParam(typeof(String), LockedBy));
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();

                string json = Assignment.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "assignment", Id, AuditOperationType.Insert, json, "");

                Dirty = false;
            }
            else
            {
                if (Dirty)
                {
                    // Update existing assignment
                    cmd.CommandText = "csp_update_assignment";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@laboratory_id", DB.MakeParam(typeof(Guid), LaboratoryId));
                    cmd.Parameters.AddWithValue("@account_id", DB.MakeParam(typeof(Guid), AccountId));
                    cmd.Parameters.AddWithValue("@deadline", DB.MakeParam(typeof(DateTime), Deadline));
                    cmd.Parameters.AddWithValue("@requested_sigma_act", DB.MakeParam(typeof(double), RequestedSigmaAct));
                    cmd.Parameters.AddWithValue("@requested_sigma_mda", DB.MakeParam(typeof(double), RequestedSigmaMDA));
                    cmd.Parameters.AddWithValue("@customer_company_name", DB.MakeParam(typeof(String), CustomerCompanyName));
                    cmd.Parameters.AddWithValue("@customer_company_email", DB.MakeParam(typeof(String), CustomerCompanyEmail));
                    cmd.Parameters.AddWithValue("@customer_company_phone", DB.MakeParam(typeof(String), CustomerCompanyPhone));
                    cmd.Parameters.AddWithValue("@customer_company_address", DB.MakeParam(typeof(String), CustomerCompanyAddress));
                    cmd.Parameters.AddWithValue("@customer_contact_name", DB.MakeParam(typeof(String), CustomerContactName));
                    cmd.Parameters.AddWithValue("@customer_contact_email", DB.MakeParam(typeof(String), CustomerContactEmail));
                    cmd.Parameters.AddWithValue("@customer_contact_phone", DB.MakeParam(typeof(String), CustomerContactPhone));
                    cmd.Parameters.AddWithValue("@customer_contact_address", DB.MakeParam(typeof(String), CustomerContactAddress));
                    cmd.Parameters.AddWithValue("@approved_customer", DB.MakeParam(typeof(bool), ApprovedCustomer));
                    cmd.Parameters.AddWithValue("@approved_customer_by", DB.MakeParam(typeof(String), ApprovedCustomerBy));
                    cmd.Parameters.AddWithValue("@approved_laboratory", DB.MakeParam(typeof(bool), ApprovedLaboratory));
                    cmd.Parameters.AddWithValue("@approved_laboratory_by", DB.MakeParam(typeof(String), ApprovedLaboratoryBy));
                    cmd.Parameters.AddWithValue("@content_comment", DB.MakeParam(typeof(String), ContentComment));
                    cmd.Parameters.AddWithValue("@report_comment", DB.MakeParam(typeof(String), ReportComment));
                    cmd.Parameters.AddWithValue("@audit_comment", DB.MakeParam(typeof(String), AuditComment));
                    cmd.Parameters.AddWithValue("@workflow_status_id", DB.MakeParam(typeof(int), WorkflowStatusId));
                    cmd.Parameters.AddWithValue("@last_workflow_status_date", DB.MakeParam(typeof(DateTime), LastWorkflowStatusDate));
                    cmd.Parameters.AddWithValue("@last_workflow_status_by", DB.MakeParam(typeof(String), LastWorkflowStatusBy));
                    cmd.Parameters.AddWithValue("@analysis_report_version", DB.MakeParam(typeof(int), AnalysisReportVersion));
                    cmd.Parameters.AddWithValue("@instance_status_id", DB.MakeParam(typeof(int), InstanceStatusId));
                    cmd.Parameters.AddWithValue("@locked_by", DB.MakeParam(typeof(String), LockedBy));                    
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                    cmd.ExecuteNonQuery();

                    string json = Assignment.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "assignment", Id, AuditOperationType.Update, json, "");

                    Dirty = false;
                }
            }
            
            foreach (AssignmentSampleType ast in SampleTypes)
                ast.StoreToDB(conn, trans);

            // Remove deleted sample types from DB
            List<Guid> storedSampTypeIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_sample_type where assignment_id = @id", CommandType.Text,
                new SqlParameter("@id", Id)))
            {
                while (reader.Read())
                    storedSampTypeIds.Add(Guid.Parse(reader["id"].ToString()));
            }

            cmd.CommandText = "delete from assignment_sample_type where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid astId in storedSampTypeIds)
            {
                if (SampleTypes.FindIndex(x => x.Id == astId) == -1)
                {
                    string json = AssignmentSampleType.ToJSON(conn, trans, astId);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "assignment_sample_type", astId, AuditOperationType.Delete, json, "");

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", astId);
                    cmd.ExecuteNonQuery();
                }
            }
        }        
    }
}
