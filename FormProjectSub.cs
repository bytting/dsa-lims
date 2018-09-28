using log4net;
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
    public partial class FormProjectSub : Form
    {
        private ILog mLog = null;

        public ProjectSubModel SubProject = new ProjectSubModel();

        public FormProjectSub(ILog log, string pname, Guid pid)
        {
            InitializeComponent();
            mLog = log;
            SubProject.ProjectMainId = pid;            
            Text = "Create new sub project";
            tbMainProjectName.Text = pname;
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;
            cboxInstanceStatus.SelectedValue = InstanceStatus.Active;
        }

        public FormProjectSub(ILog log, string pname, Guid pid, Guid spid)
        {
            InitializeComponent();
            mLog = log;
            SubProject.Id = spid;
            SubProject.ProjectMainId = pid;            
            Text = "Update sub project";
            tbMainProjectName.Text = pname;
            cboxInstanceStatus.DataSource = Common.InstanceStatusList;

            using (SqlConnection conn = DB.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("csp_select_project_sub", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SubProject.Id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        throw new Exception("Project with ID " + SubProject.Id.ToString() + " was not found");

                    reader.Read();
                    tbName.Text = reader["name"].ToString();
                    cboxInstanceStatus.SelectedValue = InstanceStatus.Eval(reader["instance_status_id"]);
                    tbComment.Text = reader["comment"].ToString();
                    
                    SubProject.CreateDate = Convert.ToDateTime(reader["create_date"]);
                    SubProject.CreatedBy = reader["created_by"].ToString();
                    SubProject.UpdateDate = Convert.ToDateTime(reader["update_date"]);
                    SubProject.UpdatedBy = reader["updated_by"].ToString();
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
                MessageBox.Show("Project name is mandatory");
                return;
            }

            SubProject.Name = tbName.Text.Trim();
            SubProject.InstanceStatusId = InstanceStatus.Eval(cboxInstanceStatus.SelectedValue);
            SubProject.Comment = tbComment.Text.Trim();

            bool success;
            if (SubProject.Id == Guid.Empty)
                success = InsertSubProject();
            else
                success = UpdateSubProject();

            DialogResult = success ? DialogResult.OK : DialogResult.Abort;
            Close();
        }

        private bool InsertSubProject()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SubProject.CreateDate = DateTime.Now;
                SubProject.CreatedBy = Common.Username;
                SubProject.UpdateDate = DateTime.Now;
                SubProject.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_insert_project_sub", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                SubProject.Id = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", SubProject.Id);
                cmd.Parameters.AddWithValue("@project_main_id", SubProject.ProjectMainId);
                cmd.Parameters.AddWithValue("@name", SubProject.Name);
                cmd.Parameters.AddWithValue("@instance_status_id", SubProject.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SubProject.Comment);
                cmd.Parameters.AddWithValue("@create_date", SubProject.CreateDate);
                cmd.Parameters.AddWithValue("@created_by", SubProject.CreatedBy);
                cmd.Parameters.AddWithValue("@update_date", SubProject.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SubProject.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "project_sub", SubProject.Id, AuditOperationType.Insert, JsonConvert.SerializeObject(SubProject));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }

        private bool UpdateSubProject()
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                SubProject.UpdateDate = DateTime.Now;
                SubProject.UpdatedBy = Common.Username;

                connection = DB.OpenConnection();
                transaction = connection.BeginTransaction();

                SqlCommand cmd = new SqlCommand("csp_update_project_sub", connection, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", SubProject.Id);
                cmd.Parameters.AddWithValue("@name", SubProject.Name);
                cmd.Parameters.AddWithValue("@instance_status_id", SubProject.InstanceStatusId);
                cmd.Parameters.AddWithValue("@comment", SubProject.Comment);
                cmd.Parameters.AddWithValue("@update_date", SubProject.UpdateDate);
                cmd.Parameters.AddWithValue("@updated_by", SubProject.UpdatedBy);
                cmd.ExecuteNonQuery();

                DB.AddAuditMessage(connection, transaction, "project_sub", SubProject.Id, AuditOperationType.Update, JsonConvert.SerializeObject(SubProject));

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                mLog.Error(ex);
                return false;
            }
            finally
            {
                connection?.Close();
            }

            return true;
        }
    }
}
