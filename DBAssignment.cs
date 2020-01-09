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
        public DateTime? Deadline { get; set; }        
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
        public DateTime? LastWorkflowStatusDate { get; set; }        
        public string LastWorkflowStatusBy { get; set; }        
        public int AnalysisReportVersion { get; set; }        
        public int InstanceStatusId { get; set; }        
        public Guid LockedId { get; set; }        
        public DateTime CreateDate { get; set; }        
        public Guid CreateId { get; set; }        
        public DateTime UpdateDate { get; set; }        
        public Guid UpdateId { get; set; }
        public string Description { get; set; }

        public List<AssignmentSampleType> SampleTypes { get; set; }

        public bool Dirty;

        public bool IsDirty()
        {            
            if (Dirty)
                return true;

            foreach (AssignmentSampleType ast in SampleTypes)
                if (ast.IsDirty())
                    return true;

            return false;        
        }

        public void ClearDirty()
        {
            Dirty = false;
            foreach (AssignmentSampleType ast in SampleTypes)
                ast.ClearDirty();
        }

        public static Guid GetLaboratoryId(SqlConnection conn, SqlTransaction trans, Guid assignmentId)
        {
            object o = DB.GetScalar(conn, trans, "select laboratory_id from assignment where id = @aid", CommandType.Text, new SqlParameter("@aid", assignmentId));
            return !DB.IsValidField(o) ? Guid.Empty : Guid.Parse(o.ToString());
        }

        public static Guid GetCreatorId(SqlConnection conn, SqlTransaction trans, Guid assignmentId)
        {
            object o = DB.GetScalar(conn, trans, "select create_id from assignment where id = @aid", CommandType.Text, new SqlParameter("@aid", assignmentId));
            return !DB.IsValidField(o) ? Guid.Empty : Guid.Parse(o.ToString());
        }

        public string LaboratoryName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from laboratory where id = @lid", CommandType.Text, new SqlParameter("@lid", LaboratoryId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public string ResponsibleName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from cv_account where id = @aid", CommandType.Text, new SqlParameter("@aid", AccountId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid assId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from assignment where id = @id", CommandType.Text, new SqlParameter("@id", assId));
            return cnt > 0;
        }

        public List<Guid> GetSampleTypeIdsForSampleType(SqlConnection conn, SqlTransaction trans, string sampleTypeRoot)
        {
            List<Guid> ids = new List<Guid>();
            using (SqlDataReader astReader = DB.GetDataReader(conn, null, "csp_select_assignment_sample_types_for_sample_type_name", CommandType.StoredProcedure,
                    new SqlParameter("@assignment_id", Id),
                    new SqlParameter("@sample_type_name", sampleTypeRoot)))
            {
                while (astReader.Read())
                {
                    Guid stId = astReader.GetGuid("sample_type_id");
                    ids.Add(stId);
                }
            }

            return ids;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid assId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment", CommandType.StoredProcedure,
                new SqlParameter("@id", assId)))
            {
                if(!reader.HasRows)
                    throw new Exception("Error: Assignment with id " + assId.ToString() + " was not found");
                
                reader.Read();

                Id = reader.GetGuid("id");
                Name = reader.GetString("name");
                LaboratoryId = reader.GetGuid("laboratory_id");
                AccountId = reader.GetGuid("account_id");
                Deadline = reader.GetDateTimeNullable("deadline");
                RequestedSigmaAct = reader.GetDouble("requested_sigma_act");
                RequestedSigmaMDA = reader.GetDouble("requested_sigma_mda");
                CustomerCompanyName = reader.GetString("customer_company_name");
                CustomerCompanyEmail = reader.GetString("customer_company_email");
                CustomerCompanyPhone = reader.GetString("customer_company_phone");
                CustomerCompanyAddress = reader.GetString("customer_company_address");
                CustomerContactName = reader.GetString("customer_contact_name");
                CustomerContactEmail = reader.GetString("customer_contact_email");
                CustomerContactPhone = reader.GetString("customer_contact_phone");
                CustomerContactAddress = reader.GetString("customer_contact_address");
                ApprovedCustomer = reader.GetBoolean("approved_customer");
                ApprovedCustomerBy = reader.GetString("approved_customer_by");
                ApprovedLaboratory = reader.GetBoolean("approved_laboratory");
                ApprovedLaboratoryBy = reader.GetString("approved_laboratory_by");
                ContentComment = reader.GetString("content_comment");
                ReportComment = reader.GetString("report_comment");
                AuditComment = reader.GetString("audit_comment");
                WorkflowStatusId = reader.GetInt32("workflow_status_id");                
                LastWorkflowStatusDate = reader.GetDateTimeNullable("last_workflow_status_date");                
                LastWorkflowStatusBy = reader.GetString("last_workflow_status_by");
                AnalysisReportVersion = reader.GetInt32("analysis_report_version");
                InstanceStatusId = reader.GetInt32("instance_status_id");
                LockedId = reader.GetGuid("locked_id");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
                Description = reader.GetString("description");
            }

            SampleTypes.Clear();

            List<Guid> sampleTypeIds = new List<Guid>();
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select id from assignment_sample_type where assignment_id = @id", CommandType.Text,
                new SqlParameter("@id", assId)))
            {
                while (reader.Read())                    
                    sampleTypeIds.Add(reader.GetGuid("id"));
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
                cmd.Parameters.AddWithValue("@name", Name, String.Empty);
                cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                cmd.Parameters.AddWithValue("@account_id", AccountId, Guid.Empty);
                cmd.Parameters.AddWithValue("@deadline", Deadline, DateTime.MinValue);
                cmd.Parameters.AddWithValue("@requested_sigma_act", RequestedSigmaAct);
                cmd.Parameters.AddWithValue("@requested_sigma_mda", RequestedSigmaMDA);
                cmd.Parameters.AddWithValue("@customer_company_name", CustomerCompanyName, String.Empty);
                cmd.Parameters.AddWithValue("@customer_company_email", CustomerCompanyEmail, String.Empty);
                cmd.Parameters.AddWithValue("@customer_company_phone", CustomerCompanyPhone, String.Empty);
                cmd.Parameters.AddWithValue("@customer_company_address", CustomerCompanyAddress, String.Empty);
                cmd.Parameters.AddWithValue("@customer_contact_name", CustomerContactName, String.Empty);
                cmd.Parameters.AddWithValue("@customer_contact_email", CustomerContactEmail, String.Empty);
                cmd.Parameters.AddWithValue("@customer_contact_phone", CustomerContactPhone, String.Empty);
                cmd.Parameters.AddWithValue("@customer_contact_address", CustomerContactAddress, String.Empty);
                cmd.Parameters.AddWithValue("@approved_customer", ApprovedCustomer);
                cmd.Parameters.AddWithValue("@approved_customer_by", ApprovedCustomerBy, String.Empty);
                cmd.Parameters.AddWithValue("@approved_laboratory", ApprovedLaboratory);
                cmd.Parameters.AddWithValue("@approved_laboratory_by", ApprovedLaboratoryBy, String.Empty);
                cmd.Parameters.AddWithValue("@content_comment", ContentComment, String.Empty);
                cmd.Parameters.AddWithValue("@report_comment", ReportComment, String.Empty);
                cmd.Parameters.AddWithValue("@audit_comment", AuditComment, String.Empty);
                cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                cmd.Parameters.AddWithValue("@last_workflow_status_date", LastWorkflowStatusDate, DateTime.MinValue);
                cmd.Parameters.AddWithValue("@last_workflow_status_by", LastWorkflowStatusBy, String.Empty);
                cmd.Parameters.AddWithValue("@analysis_report_version", AnalysisReportVersion);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId);
                cmd.Parameters.AddWithValue("@locked_id", LockedId, Guid.Empty);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);
                cmd.Parameters.AddWithValue("@description", Description, String.Empty);

                cmd.ExecuteNonQuery();

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
                    cmd.Parameters.AddWithValue("@laboratory_id", LaboratoryId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@account_id", AccountId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@deadline", Deadline, DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@requested_sigma_act", RequestedSigmaAct);
                    cmd.Parameters.AddWithValue("@requested_sigma_mda", RequestedSigmaMDA);
                    cmd.Parameters.AddWithValue("@customer_company_name", CustomerCompanyName, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_company_email", CustomerCompanyEmail, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_company_phone", CustomerCompanyPhone, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_company_address", CustomerCompanyAddress, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_contact_name", CustomerContactName, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_contact_email", CustomerContactEmail, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_contact_phone", CustomerContactPhone, String.Empty);
                    cmd.Parameters.AddWithValue("@customer_contact_address", CustomerContactAddress, String.Empty);
                    cmd.Parameters.AddWithValue("@approved_customer", ApprovedCustomer);
                    cmd.Parameters.AddWithValue("@approved_customer_by", ApprovedCustomerBy, String.Empty);
                    cmd.Parameters.AddWithValue("@approved_laboratory", ApprovedLaboratory);
                    cmd.Parameters.AddWithValue("@approved_laboratory_by", ApprovedLaboratoryBy, String.Empty);
                    cmd.Parameters.AddWithValue("@content_comment", ContentComment, String.Empty);
                    cmd.Parameters.AddWithValue("@report_comment", ReportComment, String.Empty);
                    cmd.Parameters.AddWithValue("@audit_comment", AuditComment, String.Empty);
                    cmd.Parameters.AddWithValue("@workflow_status_id", WorkflowStatusId);
                    cmd.Parameters.AddWithValue("@last_workflow_status_date", LastWorkflowStatusDate, DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@last_workflow_status_by", LastWorkflowStatusBy, String.Empty);
                    cmd.Parameters.AddWithValue("@analysis_report_version", AnalysisReportVersion);
                    cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatusId);
                    cmd.Parameters.AddWithValue("@locked_id", LockedId, Guid.Empty);                    
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@description", Description, String.Empty);

                    cmd.ExecuteNonQuery();

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
                    storedSampTypeIds.Add(reader.GetGuid("id"));
            }

            cmd.CommandText = "delete from assignment_sample_type where id = @id";
            cmd.CommandType = CommandType.Text;
            foreach (Guid astId in storedSampTypeIds)
            {
                if (SampleTypes.FindIndex(x => x.Id == astId) == -1)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", astId);
                    cmd.ExecuteNonQuery();
                }
            }
        }        
    }
}
