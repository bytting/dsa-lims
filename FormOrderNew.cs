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
    public partial class FormOrderNew : Form
    {
        public Guid OrderId = Guid.Empty;
        public string OrderName = String.Empty;

        public FormOrderNew()
        {
            InitializeComponent();
        }

        private void FormOrderNew_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateLaboratories(conn, InstanceStatus.Active, cboxLaboratory);
                UI.PopulateSigma(cboxRequestedSigma);
                UI.PopulateCustomers(conn, InstanceStatus.Active, cboxCustomer);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (cboxLaboratory.SelectedItem == null)
            {
                MessageBox.Show("Laboratory is mandatory");
                return;
            }

            if (cboxResponsible.SelectedItem == null)
            {
                MessageBox.Show("Responsible is mandatory");
                return;
            }

            if (String.IsNullOrEmpty(tbDeadline.Text))
            {
                MessageBox.Show("Deadline is mandatory");
                return;
            }

            if (cboxRequestedSigma.SelectedItem == null)
            {
                MessageBox.Show("Requested sigma is mandatory");
                return;
            }

            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = DB.OpenConnection();
                trans = conn.BeginTransaction();

                string customerName, customerAddress, customerEmail, customerPhone;
                Lemma<Guid, string> cust = cboxCustomer.SelectedItem as Lemma<Guid, string>;
                using (SqlDataReader reader = DB.GetDataReader(conn, trans, "select * from customer where id = @id", CommandType.Text, 
                    new SqlParameter("@id", cust.Id)))
                {
                    reader.Read();
                    customerName = reader["name"].ToString();
                    customerAddress = reader["address"].ToString();
                    customerEmail = reader["email"].ToString();
                    customerPhone = reader["phone"].ToString();
                }

                Lemma<Guid, string> lab = cboxLaboratory.SelectedItem as Lemma<Guid, string>;
                string labPrefix = DB.GetOrderPrefix(conn, trans, lab.Id);
                int orderCount = DB.GetNextOrderCount(conn, trans, lab.Id);
                OrderName = labPrefix + "-" + DateTime.Now.ToString("yyyy") + "-" + orderCount;

                SqlCommand cmd = new SqlCommand("csp_insert_assignment", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;
                OrderId = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", OrderId);
                cmd.Parameters.AddWithValue("@name", OrderName);
                cmd.Parameters.AddWithValue("@laboratory_id", lab.Id);
                Lemma<string, string> account = cboxResponsible.SelectedItem as Lemma<string, string>;
                cmd.Parameters.AddWithValue("@account_id", account.Id);
                cmd.Parameters.AddWithValue("@deadline", (DateTime)tbDeadline.Tag);
                cmd.Parameters.AddWithValue("@requested_sigma", Convert.ToDouble(cboxRequestedSigma.Text));
                cmd.Parameters.AddWithValue("@customer_name", customerName);
                cmd.Parameters.AddWithValue("@customer_address", customerAddress);
                cmd.Parameters.AddWithValue("@customer_email", customerEmail);
                cmd.Parameters.AddWithValue("@customer_phone", customerPhone);
                cmd.Parameters.AddWithValue("@customer_contact_name", DBNull.Value);
                cmd.Parameters.AddWithValue("@customer_contact_email", DBNull.Value);
                cmd.Parameters.AddWithValue("@customer_contact_phone", DBNull.Value);
                cmd.Parameters.AddWithValue("@approved_customer", 0);
                cmd.Parameters.AddWithValue("@approved_laboratory", 0);
                cmd.Parameters.AddWithValue("@content_comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@report_comment", DBNull.Value);
                cmd.Parameters.AddWithValue("@closed_date", DBNull.Value);
                cmd.Parameters.AddWithValue("@closed_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@instance_status_id", InstanceStatus.Active);
                cmd.Parameters.AddWithValue("@locked_by", DBNull.Value);
                cmd.Parameters.AddWithValue("@create_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@created_by", Common.Username);
                cmd.Parameters.AddWithValue("@update_date", DateTime.Now);
                cmd.Parameters.AddWithValue("@updated_by", Common.Username);

                cmd.ExecuteNonQuery();
                trans.Commit();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                trans?.Rollback();
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
                DialogResult = DialogResult.Abort;
            }
            finally
            {
                conn?.Close();
            }
            
            Close();
        }

        private void cboxLaboratory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxLaboratory.SelectedItem == null)
                return;

            Lemma<Guid, string> lab = cboxLaboratory.SelectedItem as Lemma<Guid, string>;
            using (SqlConnection conn = DB.OpenConnection())
            {
                UI.PopulateUsers(conn, lab.Id, InstanceStatus.Active, cboxResponsible);
            }
        }

        private void btnSelectDeadline_Click(object sender, EventArgs e)
        {
            FormSelectDate form = new FormSelectDate();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            DateTime selectedDate = form.SelectedDate;
            tbDeadline.Tag = selectedDate;
            tbDeadline.Text = selectedDate.ToString(StrUtils.DateFormatNorwegian);
        }
    }
}
