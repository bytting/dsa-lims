using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSA
{
    public class AnalysisParameters
    {
        public string FileName { get; set; }
        public string SampleName { get; set; }
        public string PreparationGeometry { get; set; }
        public string SpectrumReferenceRegEx { get; set; }
    }

    public class AnalysisResult
    {        
        public string SpectrumReference { get; set; }
        public DateTime MeasurementStart { get; set; }
        public DateTime MeasurementStop { get; set; }
        public DateTime ReferenceTime { get; set; }
        public double LiveTime { get; set; }
        public double RealTime { get; set; }
        public string NuclideLibrary { get; set; }
        public string DetectionLimitLibrary { get; set; }
        public Dictionary<string, List<double>> IdentifiedIsotopes
        {
            get { return mIdentifiedIsotopes; }
            set { mIdentifiedIsotopes = value; }
        }

        private Dictionary<string, List<double>> mIdentifiedIsotopes = new Dictionary<string, List<double>>();
    }

    public interface IAnalysisPlugin
    {
        string PluginName { get; }
        string FileTypeFilter { get; }
        AnalysisResult Execute(AnalysisParameters parameters);
    }
}
