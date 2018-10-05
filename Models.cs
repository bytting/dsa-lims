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
using System.Text;

namespace DSA_lims
{
    public abstract class ModelBase
    {
        public ModelBase() {}

        public ModelBase(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatedBy { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class NuclideModel : ModelBase
    {
        public NuclideModel() {}
        public NuclideModel(Guid id, string name) : base(id, name) {}

        public int ProtonCount { get; set; }
        public int NeutronCount { get; set; }
        public double HalfLife { get; set; }
        public double HalfLifeUncertainty { get; set; }
        public int DecayTypeId { get; set; }
        public double XRayEnergy { get; set; }
        public double FluorescenceYield { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
    }

    public class EnergyLineModel : ModelBase
    {
        public EnergyLineModel() { }
        public EnergyLineModel(Guid id, string name) : base(id, name) { }

        public Guid NuclideId { get; set; }
        public double TransmissionFrom { get; set; }
        public double TransmissionTo { get; set; }
        public double Energy { get; set; }
        public double EnergyUncertainty { get; set; }
        public double Intensity { get; set; }
        public double IntensityUncertainty { get; set; }
        public double ProbabilityOfDecay { get; set; }
        public double ProbabilityOfDecayUncertainty { get; set; }
        public double TotalInternalConversion { get; set; }
        public double KShellConversion { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        

        public override string ToString()
        {
            return NuclideId.ToString() + " : " + TransmissionFrom.ToString() + " -> " + TransmissionTo.ToString();
        }
    }

    public class GeometryModel : ModelBase
    {
        public GeometryModel() { }
        public GeometryModel(Guid id, string name) : base(id, name) { }

        public double MinFillHeight { get; set; }
        public double MaxFillHeight { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class CountyModel : ModelBase
    {
        public CountyModel() { }
        public CountyModel(Guid id, string name) : base(id, name) { }

        public int Number { get; set; }
        public int InstanceStatusId { get; set; }        
    }

    public class MunicipalityModel : ModelBase
    {
        public MunicipalityModel() { }
        public MunicipalityModel(Guid id, string name) : base(id, name) { }

        public Guid CountyId { get; set; }    
        public int Number { get; set; }
        public int InstanceStatusId { get; set; }        
    }

    public class StationModel : ModelBase
    {
        public StationModel() { }
        public StationModel(Guid id, string name) : base(id, name) { }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class SampleStorageModel : ModelBase
    {
        public SampleStorageModel() { }
        public SampleStorageModel(Guid id, string name) : base(id, name) { }

        public string Address { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class LaboratoryModel : ModelBase
    {
        public LaboratoryModel() { }
        public LaboratoryModel(Guid id, string name) : base(id, name) { }

        public string NamePrefix { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int AssignmentCounter { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class ProjectMainModel : ModelBase
    {
        public ProjectMainModel() { }
        public ProjectMainModel(Guid id, string name) : base(id, name) { }

        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class ProjectSubModel : ProjectMainModel
    {
        public ProjectSubModel() { }
        public ProjectSubModel(Guid id, string name) : base(id, name) { }

        public Guid ProjectMainId { get; set; }
    }

    public class SamplerModel : ModelBase
    {
        public SamplerModel() { }
        public SamplerModel(Guid id, string name) : base(id, name) { }

        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class PreparationMethodModel : ModelBase
    {
        public PreparationMethodModel() { }
        public PreparationMethodModel(Guid id, string name) : base(id, name) { }

        public string DescriptionLink { get; set; }
        public bool Destructive { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
    }

    public class AnalysisMethodModel : ModelBase
    {
        public AnalysisMethodModel() { }
        public AnalysisMethodModel(Guid id, string name) : base(id, name) { }

        public string DescriptionLink { get; set; }
        public string SpecterReferenceRegExp { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
    }

    public class SamplingMethodModel : ModelBase
    {
        public SamplingMethodModel() { }
        public SamplingMethodModel(Guid id, string name) : base(id, name) { }        

        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }
    }

    public class SampleComponentModel : ModelBase
    {
        public SampleComponentModel() { }
        public SampleComponentModel(Guid id, string name) : base(id, name) { }
    }

    public class SampleParameterModel : ModelBase
    {
        public SampleParameterModel() { }
        public SampleParameterModel(Guid id, string name) : base(id, name) { }

        public string Type { get; set; }
    }    

    public class SampleTypeModel : ModelBase
    {
        public SampleTypeModel() { }
        public SampleTypeModel(Guid id, string name) : base(id, name) { }
        
        public string ShortName { get; set; }

        public static string NameToLabel(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            string[] items = name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if(items.Length < 1)
                return String.Empty;

            return items[items.Length - 1] + " -> " + name;
        }

        public static string LabelToName(string label)
        {
            if (String.IsNullOrEmpty(label))
                return String.Empty;

            int idx = label.IndexOf(" -> ");
            if(idx < 0)
                return String.Empty;

            return label.Substring(idx + 4);
        }
    }    
}
