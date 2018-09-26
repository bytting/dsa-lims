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
using System.Linq;
using System.Text;

namespace DSA_lims
{
    public abstract class ModelBase
    {
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
        public double MinFillHeight { get; set; }
        public double MaxFillHeight { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class CountyModel : ModelBase
    {        
        public int Number { get; set; }
        public int InstanceStatusId { get; set; }        
    }

    public class MunicipalityModel : ModelBase
    {        
        public Guid CountyId { get; set; }    
        public int Number { get; set; }
        public int InstanceStatusId { get; set; }        
    }

    public class StationModel : ModelBase
    {        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class SampleStorageModel : ModelBase
    {        
        public string Address { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class LaboratoryModel : ModelBase
    {        
        public string NamePrefix { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int AssignmentCounter { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class MainProjectModel : ModelBase
    {        
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class SubProjectModel : MainProjectModel
    {        
        public Guid MainProjectId { get; set; }
    }

    public class SamplerModel : ModelBase
    {        
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int InstanceStatusId { get; set; }
        public string Comment { get; set; }        
    }

    public class SampleComponentModel : ModelBase
    {        
    }

    public class SampleParameterModel : ModelBase
    {        
        public string Type { get; set; }
    }

    public class SampleTypeModel : ModelBase
    {
        public string ShortName { get; set; }

        public List<SampleTypeModel> SampleTypes = new List<SampleTypeModel>();
        public List<SampleComponentModel> SampleComponents = new List<SampleComponentModel>();
        public List<SampleParameterModel> SampleParameters = new List<SampleParameterModel>();
    }
}
