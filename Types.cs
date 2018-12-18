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
    public enum StatusMessageType
    {
        Success,
        Warning,
        Error
    }

    public enum AuditOperationType
    {
        Insert,
        Update,
        Delete
    }

    public static class InstanceStatus
    {
        public const int Active = 1;
        public const int Inactive = 2;
        public const int Deleted = 3;
    }

    public static class WorkflowStatus
    {
        public const int Construction = 1;
        public const int Complete = 2;
        public const int Rejected = 3;
    }

    public class Lemma<I, N>
    {
        public Lemma() {}

        public Lemma(I id, N name)
        {
            Id = id;
            Name = name;
        }

        public I Id { get; set; }
        public N Name { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class SampleTypeModel
    {
        public SampleTypeModel() { }

        public SampleTypeModel(Guid id, Guid parentId, string name, string name_common, string name_latin)
        {
            Id = id;
            ParentId = parentId;
            Name = name;
            NameCommon = name_common;
            NameLatin = name_latin;
        }

        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Name { get; set; }        
        public string NameCommon { get; set; }
        public string NameLatin { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class CustomerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }        
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class AnalysisParameters
    {
        public string FileName { get; set; }
        public string SampleName { get; set; }
        public string PreparationGeometry { get; set; }
        public string SpectrumReferenceRegEx { get; set; }
        public List<string> AllNuclides { get; set; }
        public List<string> AnalMethNuclides { get; set; }
    }    

    public class AnalysisResult
    {
        public string SpectrumName, SampleName, SamplePlace, Geometry, Unit, ReferenceTime, NuclideLibrary, DetLimLib;
        public double Height, Weight, Volume, Density, SampleQuantity;
        public double SigmaAct, SigmaMDA, MDAFactor;        
        public List<Isotop> Isotopes = new List<Isotop>();

        public void Clear()
        {
            SpectrumName = SampleName = SamplePlace = Geometry = Unit = ReferenceTime = NuclideLibrary = DetLimLib = String.Empty;
            Height = Weight = Volume = Density = SampleQuantity = 0d;
            SigmaAct = SigmaMDA = MDAFactor = 0d;            
            Isotopes.Clear();
        }

        public class Isotop
        {
            public string NuclideName { get; set; }
            public double ConfidenceValue { get; set; }
            public double Activity { get; set; }
            public double Uncertainty { get; set; }
            public double MDA { get; set; }
            public bool ApprovedRES { get; set; }
            public bool ApprovedMDA { get; set; }
            public bool Accredited { get; set; }
            public bool Reportable { get; set; }
        }
    }    
}
