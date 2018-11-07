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
using System.Data.SqlClient;
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

    public class Lemma<I, PI, N>
    {
        public Lemma() { }

        public Lemma(I id, PI parentId, N name)
        {
            Id = id;
            ParentId = parentId;
            Name = name;
        }

        public I Id { get; set; }
        public PI ParentId { get; set; }
        public N Name { get; set; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }    
}
