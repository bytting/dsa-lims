using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSA_lims
{
    public class SampleParameter
    {
        public SampleParameter()
        {
            Id = Guid.NewGuid();
            Dirty = false;
        }

        public Guid Id { get; set; }
        public Guid SampleId { get; set; }
        public Guid SampleParameterNameId { get; set; }
        public string Value { get; set; }
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

        public string GetParameterName(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select name from sample_parameter_name where id = @spid", CommandType.Text, new SqlParameter("@spid", SampleParameterNameId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public string GetParameterType(SqlConnection conn, SqlTransaction trans)
        {
            object o = DB.GetScalar(conn, trans, "select type from sample_parameter_name where id = @spid", CommandType.Text, new SqlParameter("@spid", SampleParameterNameId));
            return !DB.IsValidField(o) ? "" : o.ToString();
        }

        public static string ToJSON(SqlConnection conn, SqlTransaction trans, Guid sampParamId)
        {
            string json = String.Empty;
            Dictionary<string, object> map = new Dictionary<string, object>();

            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample_parameter_flat", CommandType.StoredProcedure,
                new SqlParameter("@id", sampParamId)))
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

        public static bool IdExists(SqlConnection conn, SqlTransaction trans, Guid sampParamId)
        {
            int cnt = (int)DB.GetScalar(conn, trans, "select count(*) from sample_parameter where id = @id", CommandType.Text, new SqlParameter("@id", sampParamId));
            return cnt > 0;
        }

        public void LoadFromDB(SqlConnection conn, SqlTransaction trans, Guid sampParamId)
        {
            using (SqlDataReader reader = DB.GetDataReader(conn, trans, "csp_select_sample_parameter", CommandType.StoredProcedure,
                new SqlParameter("@id", sampParamId)))
            {
                if (!reader.HasRows)
                    throw new Exception("Error: Sample parameter with id " + sampParamId.ToString() + " was not found");

                reader.Read();

                Id = reader.GetGuid("id");
                SampleId = reader.GetGuid("sample_id");
                SampleParameterNameId = reader.GetGuid("sample_parameter_name_id");
                Value = reader.GetString("value");
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
                throw new Exception("Error: Can not store a sample parameter with an empty id");

            SqlCommand cmd = new SqlCommand("", conn, trans);

            if (!SampleParameter.IdExists(conn, trans, Id))
            {
                // insert new analysis result
                cmd.CommandText = "csp_insert_sample_parameter";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.Parameters.AddWithValue("@sample_id", SampleId, Guid.Empty);
                cmd.Parameters.AddWithValue("@sample_parameter_name_id", SampleParameterNameId, Guid.Empty);
                cmd.Parameters.AddWithValue("@value", Value, String.Empty);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@create_id", Common.UserId, Guid.Empty);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                cmd.ExecuteNonQuery();

                string json = SampleParameter.ToJSON(conn, trans, Id);
                if (!String.IsNullOrEmpty(json))
                    DB.AddAuditMessage(conn, trans, "sample_parameter", Id, AuditOperationType.Insert, json, "");

                Dirty = false;
            }
            else
            {
                if (Dirty)
                {
                    // update existing analysis result
                    cmd.CommandText = "csp_update_sample_parameter";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@sample_id", SampleId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@sample_parameter_name_id", SampleParameterNameId, Guid.Empty);
                    cmd.Parameters.AddWithValue("@value", Value, String.Empty);
                    cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@update_id", Common.UserId, Guid.Empty);

                    cmd.ExecuteNonQuery();

                    string json = SampleParameter.ToJSON(conn, trans, Id);
                    if (!String.IsNullOrEmpty(json))
                        DB.AddAuditMessage(conn, trans, "sample_parameter", Id, AuditOperationType.Update, json, "");

                    Dirty = false;
                }
            }
        }
    }
}
