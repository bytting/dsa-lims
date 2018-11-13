using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IronPython.Hosting;
//using IronPython.Runtime;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

using PyDict = IronPython.Runtime.PythonDictionary;
using PyList = IronPython.Runtime.List;

namespace DSA_lims
{
    public partial class FormImportAnalysis : Form
    {
        private ScriptEngine Engine = Python.CreateEngine();
        private ScriptScope Scope = null;

        public FormImportAnalysis()
        {
            InitializeComponent();
        }

        private void FormImportAnalysis_Load(object sender, EventArgs e)
        {
            List<Plugin> plugins = new List<Plugin>();
            string[] pluginPaths = Directory.GetFiles(Common.Settings.PluginDirectory, "*.py");
            Array.ForEach(pluginPaths, path => plugins.Add(new Plugin(path, Path.GetFileNameWithoutExtension(path))));
            cboxPlugins.Items.AddRange(plugins.ToArray());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(cboxPlugins.Text))
            {
                MessageBox.Show("You must select a plugin first");
                return;
            }

            if (String.IsNullOrEmpty(tbFilename.Text))
            {
                MessageBox.Show("You must select a file to import first");
                return;
            }

            tbInfo.Text = "";

            Plugin plugin = cboxPlugins.SelectedItem as Plugin;            

            try
            {                
                Scope = Engine.CreateScope();
                Scope.SetVariable("filename", tbFilename.Text);
                string code = File.ReadAllText(plugin.Path);
                ScriptSource source = Engine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect);
                source.Execute(Scope);

                string filename = Scope.GetVariable("filename");
                tbInfo.Text += "filename: " + filename + Environment.NewLine;

                string specref = Scope.GetVariable("spectrum_reference");
                tbInfo.Text += "specref: " + specref + Environment.NewLine;

                string nuclib = Scope.GetVariable("nuclide_library");
                tbInfo.Text += "nuclib: " + nuclib + Environment.NewLine;

                string detlimlib = Scope.GetVariable("detection_limit_lib");
                tbInfo.Text += "detlimlib: " + detlimlib + Environment.NewLine;

                PyDict identifiedIsotopes = Scope.GetVariable("identified_isotopes");
                tbInfo.Text += "identified_isotopes:" + Environment.NewLine;
                foreach (KeyValuePair<object, object> kv in identifiedIsotopes)
                {
                    tbInfo.Text += kv.Key.ToString() + " ";

                    PyList lst = kv.Value as PyList;
                    foreach (double d in lst)
                        tbInfo.Text += d.ToString() + " ";

                    tbInfo.Text += Environment.NewLine;
                }
            }
            catch(Exception ex)
            {
                Common.Log.Error(ex);
                MessageBox.Show(ex.Message);
            }

            //DialogResult = DialogResult.OK;
            //Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            tbFilename.Text = dialog.FileName;
        }
    }

    public class Plugin
    {
        public Plugin(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
