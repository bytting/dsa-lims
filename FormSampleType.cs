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
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DSA_lims
{
    public partial class FormSampleType : Form
    {
        private Dictionary<string, object> p = new Dictionary<string, object>();
        private string SampleTypePath;
        TreeView mTree = null;
        TreeNode mTreeNode = null;
        bool mEdit = false;

        public Guid SampleTypeId
        {
            get { return p.ContainsKey("id") ? (Guid)p["id"] : Guid.Empty; }
        }

        public string SampleTypeName
        {
            get { return p.ContainsKey("name") ? p["name"].ToString() : String.Empty; }
        }

        public FormSampleType(TreeView tree, TreeNode tnode, bool edit)
        {
            InitializeComponent();

            mTree = tree;
            mTreeNode = tnode;
            mEdit = edit;
        }

        private void FormSampleType_Load(object sender, EventArgs e)
        {
            if(mEdit)
            {
                lblCurrent.Text = "Name";
                tbCurrent.Text = mTreeNode.Text + " -> " + mTreeNode.FullPath;
                p["id"] = Guid.Parse(mTreeNode.Name);
                SampleTypePath = mTreeNode.Parent == null ? mTreeNode.FullPath : mTreeNode.Parent.FullPath;

                SqlConnection conn = null;
                try
                {
                    conn = DB.OpenConnection();
                    SqlCommand cmd = new SqlCommand("csp_select_sample_type", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", p["id"]);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            throw new Exception("Sample type with ID " + p["id"] + " was not found");

                        reader.Read();

                        tbName.Text = reader.GetString("name");
                        tbNameCommon.Text = reader.GetString("name_common");
                        tbNameLatin.Text = reader.GetString("name_latin");
                        p["parent_id"] = reader["parent_id"];
                        p["create_date"] = reader["create_date"];
                        p["create_id"] = reader["create_id"];
                        p["update_date"] = reader["update_date"];
                        p["update_id"] = reader["update_id"];
                    }
                }
                catch (Exception ex)
                {
                    Common.Log.Error(ex);
                    MessageBox.Show(ex.Message);
                    DialogResult = DialogResult.Abort;
                    Close();
                }
                finally
                {
                    conn?.Close();
                }
            }
            else
            {
                lblCurrent.Text = "Parent name";

                if (mTreeNode == null)
                {
                    tbCurrent.Text = "";
                    p["parent_id"] = Guid.Empty;
                    SampleTypePath = "";
                }
                else
                {
                    tbCurrent.Text = mTreeNode.Text + " -> " + mTreeNode.FullPath;
                    p["parent_id"] = Guid.Parse(mTreeNode.Name);
                    SampleTypePath = mTreeNode.FullPath;
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

            string name = tbName.Text.Trim();
            p["name"] = name;

            if (mTreeNode != null)
            {
                foreach (TreeNode tn in mTreeNode.Nodes)
                {
                    if (tn.Text.ToLower() == name.ToLower())
                    {
                        MessageBox.Show("Sample type " + name + " already exists");
                        return;
                    }
                }
            }
            else
            {
                foreach (TreeNode tn in mTree.Nodes)
                {
                    if (tn.Text.ToLower() == name.ToLower())
                    {
                        MessageBox.Show("Sample type " + name + " already exists");
                        return;
                    }
                }
            }

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

            try
            {                                
                p["create_date"] = DateTime.Now;
                p["create_id"] = Common.UserId;
                p["update_date"] = DateTime.Now;
                p["update_id"] = Common.UserId;                

                connection = DB.OpenConnection();            

                SqlCommand cmd = new SqlCommand("csp_insert_sample_type", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                p["id"] = Guid.NewGuid();
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@parent_id", p["parent_id"]);
                cmd.Parameters.AddWithValue("@path", p["path"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);
                cmd.Parameters.AddWithValue("@name_common", p["name_common"], String.Empty);
                cmd.Parameters.AddWithValue("@name_latin", p["name_latin"], String.Empty);
                cmd.Parameters.AddWithValue("@create_date", p["create_date"]);
                cmd.Parameters.AddWithValue("@create_id", p["create_id"]);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
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

            try
            {
                p["update_date"] = DateTime.Now;
                p["update_id"] = Common.UserId;

                connection = DB.OpenConnection();

                SqlCommand cmd = new SqlCommand("csp_update_sample_type", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", p["id"]);
                cmd.Parameters.AddWithValue("@path", p["path"]);
                cmd.Parameters.AddWithValue("@name", p["name"]);                
                cmd.Parameters.AddWithValue("@name_common", p["name_common"], String.Empty);
                cmd.Parameters.AddWithValue("@name_latin", p["name_latin"], String.Empty);
                cmd.Parameters.AddWithValue("@update_date", p["update_date"]);
                cmd.Parameters.AddWithValue("@update_id", p["update_id"]);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
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
