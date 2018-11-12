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

            tbInfo.Text = "";

            Plugin plugin = cboxPlugins.SelectedItem as Plugin;            

            try
            {                
                Scope = Engine.CreateScope();
                string code = File.ReadAllText(plugin.Path);
                ScriptSource source = Engine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect);
                source.Execute(Scope);

                string spectrum_reference = Scope.GetVariable("spectrum_reference");
                tbInfo.Text += "spectrum_reference: " + spectrum_reference + Environment.NewLine;

                double sigma = Scope.GetVariable("sigma");
                tbInfo.Text += "sigma: " + sigma + Environment.NewLine;

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

                PyDict detectionLimits = Scope.GetVariable("detection_limits");
                tbInfo.Text += "detection_limits:" + Environment.NewLine;
                foreach (KeyValuePair<object, object> kv in detectionLimits)
                {
                    tbInfo.Text += kv.Key.ToString() + " " + kv.Value.ToString() + Environment.NewLine;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //DialogResult = DialogResult.OK;
            //Close();
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
