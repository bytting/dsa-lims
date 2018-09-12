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
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DSA_lims
{
    public static class DB
    {    
        public static string GetConnectionString()
        {
            return Properties.Settings.Default.DSADB;
        }

        public static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(Properties.Settings.Default.DSADB);
            connection.Open();
            return connection;
        }

        public static DataTable GetDataTable(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            adapter.SelectCommand.CommandType = queryType;
            adapter.SelectCommand.Parameters.AddRange(parameters);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static SqlDataReader GetDataReader(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader();
        }

        public static object GetScalar(SqlConnection conn, string query, CommandType queryType, params SqlParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.CommandType = queryType;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static void AddAuditMessage(SqlConnection conn, SqlTransaction trans, string tbl, Guid id, AuditOperation op, string msg)
        {
            SqlCommand cmd = new SqlCommand("csp_insert_audit_message", conn, trans);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("@source_table", tbl);
            cmd.Parameters.AddWithValue("@source_id", id);
            cmd.Parameters.AddWithValue("@operation", op.ToString());
            cmd.Parameters.AddWithValue("@value", msg);
            cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
            cmd.ExecuteNonQuery();
        }        
    }    
}
