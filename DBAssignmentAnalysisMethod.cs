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
    public class AssignmentAnalysisMethod
    {
        public AssignmentAnalysisMethod()
        {
            Id = Guid.NewGuid();
            Dirty = false;
        }

        public Guid Id { get; set; }
        public Guid AssignmentPreparationMethodId { get; set; }
        public Guid AnalysisMethodId { get; set; }
        public int AnalysisMethodCount { get; set; }
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

        public string AnalysisMethodName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name_short from analysis_method where id = @aid", CommandType.Text, new SqlParameter("@aid", AnalysisMethodId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public string AnalysisMethodNameFull(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from analysis_method where id = @aid", CommandType.Text, new SqlParameter("@aid", AnalysisMethodId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid aamId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from assignment_analysis_method where id = @id", CommandType.Text, new SqlParameter("@id", aamId));
            return cnt > 0;
        }

        public void StoreToDB(SqlConnection conn, SqlTransaction trans)
        {
            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!AssignmentAnalysisMethod.IdExists(conn, trans, Id))
            {
                // Insert new aam
                cmd.CommandText = "csp_insert_assignment_analysis_method";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@assignment_preparation_method_id", AssignmentPreparationMethodId, Guid.Empty);
                cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId, Guid.Empty);
                cmd.Parameters.AddWithValue("@analysis_method_count", AnalysisMethodCount);
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
                    // Update existing aam
                    cmd.CommandText = "csp_update_assignment_analysis_method";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@assignment_preparation_method_id", AssignmentPreparationMethodId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@analysis_method_id", AnalysisMethodId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@analysis_method_count", AnalysisMethodCount);
                    cmd.Parameters.AddWithValue("@comment", Comment, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                    cmd.ExecuteNonQuery();

                    Dirty = false;
                }
            }
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid aamId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_assignment_analysis_method", CommandType.StoredProcedure,
                    new SqlParameter("@id", aamId)))
            {
                if(!reader.HasRows)
                    throw new Exception("Error: Assignment analysis method with id " + aamId.ToString() + " was not found");

                reader.Read();
                                
                Id = reader.GetGuid("id");
                AssignmentPreparationMethodId = reader.GetGuid("assignment_preparation_method_id");
                AnalysisMethodId = reader.GetGuid("analysis_method_id");
                AnalysisMethodCount = reader.GetInt32("analysis_method_count");
                Comment = reader.GetString("comment");
                CreateDate = reader.GetDateTime("create_date");
                CreateId = reader.GetGuid("create_id");
                UpdateDate = reader.GetDateTime("update_date");
                UpdateId = reader.GetGuid("update_id");
                Dirty = false;
            }
        }        
    }
}
