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
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting;

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
            //List<string> pluginNames = new List<string>();
            string[] pluginPaths = Directory.GetFiles(Common.Settings.PluginDirectory, "*.py");
            //Array.ForEach(pluginPaths, path => pluginNames.Add(path));

            //foreach(string plugin in pluginPaths)
              //  Path.GetFileNameWithoutExtension()
            cboxPlugins.Items.AddRange(pluginPaths);
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

            try
            {
                Analysis analysis = new Analysis();
                Scope = Engine.CreateScope();
                Scope.SetVariable("analysis", analysis);
                string code = File.ReadAllText(cboxPlugins.Text);
                ScriptSource source = Engine.CreateScriptSourceFromString(code, SourceCodeKind.AutoDetect);
                source.Execute(Scope);
                var UpdatedAnalysis = Scope.GetVariable<Analysis>("analysis");
                MessageBox.Show("sigma: " + UpdatedAnalysis.sigma);
                foreach (KeyValuePair<string, string> kv in UpdatedAnalysis.found_nuclides)
                    MessageBox.Show(kv.Key + " " + kv.Value);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            DialogResult = DialogResult.OK;
            Close();
        }                
    }

    public class Analysis
    {
        public double sigma;
        public Dictionary<string, string> found_nuclides = new Dictionary<string, string>();
    }

    public class Plugin
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
