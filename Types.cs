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
using System.Drawing;
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

        public static Color GetStatusColor(int? status)
        {
            if (status == null)
                return SystemColors.ControlText;

            switch (status)
            {
                case Construction:
                    return Color.Firebrick;
                case Complete:
                    return Color.DarkGreen;
                default:
                    return SystemColors.ControlDark;
            }
        }
    }

    public static class SampleParameterType
    {
        public const string Integer = "Integer";
        public const string Decimal = "Decimal";
        public const string Text = "Text";
    }

    public class Lemma<I, N>
    {
        public Lemma() { }

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

    public class Customer
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyAddress { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactAddress { get; set; }
    }

    public class SampleParameterName
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class SampleImportEntry
    {
        public bool Importable { get; set; }
        public int? Number { get; set; }
        public string SampleType { get; set; }
        public string LIMSSampleType { get; set; }
        public Guid LIMSSampleTypeId { get; set; }
        public string SampleComponent { get; set; }
        public string LIMSSampleComponent { get; set; }
        public Guid LIMSSampleComponentId { get; set; }
        public string ExternalId { get; set; }
        public DateTime? SamplingDateFrom { get; set; }
        public DateTime? SamplingDateTo { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public string Location { get; set; }
        public int LIMSLocationTypeId { get; set; }
        public string LIMSLocationType { get; set; }
        //public string LIMSLocation { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public string Sampler { get; set; }
        public string LIMSSampler { get; set; }
        public Guid LIMSSamplerId { get; set; }
        public string SamplingMethod { get; set; }        
        public string LIMSSamplingMethod { get; set; }
        public Guid LIMSSamplingMethodId { get; set; }        
        public string Comment { get; set; }
        public int? WeekNumber { get; set; }
        public double? FlowVolume_m3 { get; set; }
        public double? Volume_l { get; set; }
        public double? Depth_m { get; set; }
        public double? DryWeight_g { get; set; }
        public double? WetWeight_g { get; set; }
        public double? AirSpeed_m3_t { get; set; }
        public double? Salinity { get; set; }
        public double? LayerFrom_cm { get; set; }
        public double? LayerTo_cm { get; set; }
        public double? Area_cm2 { get; set; }
        public double? Temperature_C { get; set; }
        public string TracerNuclide { get; set; }
        public string LIMSTracerNuclide { get; set; }
        public Guid LIMSTracerNuclideId { get; set; }
        public double? TracerWeight_g { get; set; }
        public double? TracerVolume_ml { get; set; }
        public string IntendedAnalysis { get; set; }
        public double? pH { get; set; }
        public string RequireFiltering { get; set; }
        public string ImportedFrom { get; set; }
        public string ImportedFromId { get; set; }
    }
}
