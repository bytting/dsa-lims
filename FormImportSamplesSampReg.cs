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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Data.SqlClient;

namespace DSA_lims
{    
    public partial class FormImportSamplesSampReg : Form
    {
        private TreeView mTreeSampleTypes = null;
        private Guid mFileId = Guid.Empty;        
        private string mProject = String.Empty;        

        private List<SampleImportEntry> mSamples = new List<SampleImportEntry>();
        public List<SampleImportEntry> ImportedSamples { get { return mSamples; } }

        private Guid mSelectedLaboratoryId { get; set; }
        public Guid SelectedLaboratoryId { get { return mSelectedLaboratoryId; } }

        private Guid mSelectedSubProjectId { get; set; }
        public Guid SelectedSubProjectId { get { return mSelectedSubProjectId; } }

        public FormImportSamplesSampReg(TreeView treeSampleTypes)
        {
            InitializeComponent();

            mTreeSampleTypes = treeSampleTypes;

            UI.PopulateSampleTypes(mTreeSampleTypes, cboxSampleTypes);
        }

        private void FormImportSamplesSampReg_Load(object sender, EventArgs e)
        {
            tableLayout_Resize(sender, e);

            SqlConnection conn = null;
            try
            {
                conn = DB.OpenConnection();

                UI.PopulateComboBoxes(conn, "csp_select_projects_main_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxProjectMain);

                UI.PopulateComboBoxes(conn, "csp_select_laboratories_short", new[] {
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxLaboratory);
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(!Utils.IsValidGuid(cboxLaboratory.SelectedValue))
            {
                MessageBox.Show("You must select a laboratory first");
                return;
            }

            if (!Utils.IsValidGuid(cboxProjectSub.SelectedValue))
            {
                MessageBox.Show("You must select a project first");
                return;
            }

            if(mSamples.Count < 1)
            {
                MessageBox.Show("No samples to import");
                return;
            }

            foreach (SampleImportEntry se in mSamples)
            {
                if(!Utils.IsValidGuid(se.LIMSSampleTypeId))
                {
                    MessageBox.Show("Sample " + se.Number + " must have a LIMS sample type");
                    return;
                }
            }

            mSelectedLaboratoryId = Utils.MakeGuid(cboxLaboratory.SelectedValue);
            mSelectedSubProjectId = Utils.MakeGuid(cboxProjectSub.SelectedValue);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            // import from sample registration
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            mFileId = Guid.Empty;
            mSamples.Clear();
            mProject = String.Empty;            

            String line = String.Empty;
            StreamReader reader = null;

            try
            {
                reader = new StreamReader(dialog.FileName);

                line = reader.ReadLine();
                if(line == null)
                {
                    MessageBox.Show("File " + dialog.FileName + " does not appear to be valid");
                    return;
                }

                try
                {
                    mFileId = Guid.Parse(line.Trim());
                }
                catch
                {
                    MessageBox.Show("File " + dialog.FileName + " does not appear to be valid");
                    return;
                }

                while ((line = reader.ReadLine()) != null)
                {
                    ParseLine(line);
                }

                Populate();                
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                reader?.Close();
            }            
        }

        private void Populate()
        {
            tbFileID.Text = mFileId.ToString();
            tbProject.Text = mProject;

            gridSamples.Rows.Clear();

            foreach (SampleImportEntry se in mSamples)
            {
                if (se.LIMSSampleTypeId == Guid.Empty)
                {
                    string st = se.SampleType.ToLower();
                    foreach (Lemma<Guid, string> l in cboxSampleTypes.Items)
                    {
                        string s = l.Name.ToLower();
                        if (s.StartsWith(st + " -> "))
                        {
                            se.LIMSSampleType = l.Name;
                            se.LIMSSampleTypeId = l.Id;
                        }
                    }
                }

                gridSamples.Rows.Add(se.Number, se.ExternalId, se.SamplingDate, se.Latitude, se.Longitude, se.Altitude, se.Location, se.SampleType, se.LIMSSampleType);
            }            

            gridSamples.Columns["ColumnSamplingDate"].DefaultCellStyle.Format = Utils.DateTimeFormatNorwegian;                            
        }

        private void ParseLine(string line)
        {
            string[] items = line.Split(new char[] { '|' });

            if (items.Length != 14)
                throw new Exception("Wrong number of items in line. Expected 14, got " + items.Length);

            SampleImportEntry s = new SampleImportEntry();
            s.Number = Convert.ToInt32(items[2]);
            s.ExternalId = items[0] + " - " + items[2];
            mProject = items[1].Trim();
            DateTimeOffset dto = DateTimeOffset.Parse(items[3], CultureInfo.InvariantCulture);
            s.SamplingDate = dto.DateTime + dto.Offset;
            s.Latitude = Convert.ToDouble(items[4].Trim());
            s.Longitude = Convert.ToDouble(items[5].Trim());
            s.Altitude = Convert.ToDouble(items[6].Trim());
            s.Location = items[7].Trim();
            s.SampleType = items[8].Trim();
            if(items.Length >= 14)
                s.Comment = items[13].Trim();

            mSamples.Add(s);
        }

        private void tableLayout_Resize(object sender, EventArgs e)
        {
            cboxProjectMain.Width = tbFileID.Width / 2;
        }

        private void btnSetSampleType_Click(object sender, EventArgs e)
        {
            if(gridSamples.SelectedRows.Count < 1)
            {
                MessageBox.Show("You must select one or more samples first");
                return;
            }

            foreach (DataGridViewRow row in gridSamples.SelectedRows)
            {
                int num = Convert.ToInt32(row.Cells["ColumnNumber"].Value);
                SampleImportEntry se = mSamples.Find(x => x.Number == num);
                if (se == null)
                    continue;

                se.LIMSSampleType = cboxSampleTypes.Text;
                se.LIMSSampleTypeId = Utils.MakeGuid(cboxSampleTypes.SelectedValue);
            }

            Populate();
        }

        private void cboxSampleTypes_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(cboxSampleTypes.Text.Trim()))
            {
                cboxSampleTypes.SelectedItem = Guid.Empty;
                return;
            }

            if (!Utils.IsValidGuid(cboxSampleTypes.SelectedValue))
            {
                cboxSampleTypes.SelectedValue = Guid.Empty;
            }
        }

        private void cboxProjectMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Utils.IsValidGuid(cboxProjectMain.SelectedValue))
            {
                cboxProjectSub.DataSource = null;
                return;
            }

            SqlConnection conn = null;
            try
            {
                Guid projectId = Utils.MakeGuid(cboxProjectMain.SelectedValue);

                conn = DB.OpenConnection();

                UI.PopulateComboBoxes(conn, "csp_select_projects_sub_short", new[] {
                    new SqlParameter("@project_main_id", projectId),
                    new SqlParameter("@instance_status_level", InstanceStatus.Active)
                }, cboxProjectSub);
            }
            catch (Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn?.Close();
            }
        }
    }    
}
