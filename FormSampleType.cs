using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSampleType : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();
        private string SampleTypePath;

        public Guid SampleTypeId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string SampleTypeName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormSampleType(TreeNode tnode, bool edit)
        {
            InitializeComponent();

            if (edit)
            {
                lblCurrent.Text = "Name";
                tbCurrent.Text = tnode.Text + " -> " + tnode.FullPath;
                p["id"] = Guid.Parse(tnode.Name);
                SampleTypePath = tnode.Parent.FullPath;

                using (SqlConnection conn = DB.OpenConnection())
                {
                    SqlCommand cmd = new SqlCommand("csp_select_sample_type", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", p["id"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            throw new Exception("Sample type with ID " + p["id"] + " was not found");

                        reader.Read();
                        tbName.Text = reader["name"].ToString();
                        tbNameCommon.Text = reader["name_common"].ToString();
                        tbNameLatin.Text = reader["name_latin"].ToString();
                        p["parent_id"] = reader["parent_id"];
                        p["create_date"] = reader["create_date"];
                        p["created_by"] = reader["created_by"];
                        p["update_date"] = reader["update_date"];
                        p["updated_by"] = reader["updated_by"];
                    }
                }
            }
            else
            {
                lblCurrent.Text = "Parent name";

                if (tnode == null)
                {                    
                    tbCurrent.Text = "";
                    p["parent_id"] = Guid.Empty;
                    SampleTypePath = "";
                }
                else
                {                    
                    tbCurrent.Text = tnode.Text + " -> " + tnode.FullPath;
                    p["parent_id"] = Guid.Parse(tnode.Name);
                    SampleTypePath = tnode.FullPath;
                }
            }
        }                

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("Sample type name is mandatory");
                return;
            }

            p["name"] = tbName.Text.Trim();
            if(String.IsNullOrEmpty(SampleTypePath))
                p["path"] = p["name"];
            else p["path"] = SampleTypePath + "/" + p["name"];
            p["name_common"] = tbNameCommon.Text.Trim();
            p["name_latin"] = tbNameLatin.Text.Trim();

            bool success;
            if (!p.ContainsKey("id"))            
                success = InsertSampleType();            
            else            
                success = UpdateSampleType();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertSampleType()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {                                
                p["create_date"] = DateTime.Now;
                p["created_by"] = Common.Username;
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;                

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_sample_type", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@parent_id", p["parent_id"]);
                cmd.Parameters.AddWithValue("@path", p["path"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@name_common", p["name_common"], String.Empty);
                cmd.Parameters.AddWithValue("@name_latin", p["name_latin"], String.Empty);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
                cmd.Parameters.AddWithValue("@created_by", p["created_by"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sample_type", (Guid)p["id"], AuditOperationType.Insert, JsonConvert.SerializeObject(p));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateSampleType()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                p["update_date"] = DateTime.Now;
                p["updated_by"] = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_sample_type", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@path", p["path"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);                
                cmd.Parameters.AddWithValue("@name_common", p["name_common"], String.Empty);
                cmd.Parameters.AddWithValue("@name_latin", p["name_latin"], String.Empty);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@updated_by", p["updated_by"]);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "sample_type", (Guid)p["id"], AuditOperationType.Update, JsonConvert.SerializeObject(p));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                Common.Log.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private void tbName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Don't allow certain characters in XML
            if ("&<>'\"/".Contains(e.KeyChar))
                e.Handled = true;                
        }
    }
}
