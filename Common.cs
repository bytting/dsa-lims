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
using System.Threading.Tasks;

namespace DSA_lims
{
    public static class Common
    {
        public static string Username { get; set; }

        public static List<Tag<int, string>> InstanceStatusList = new List<Tag<int, string>>();
        public static List<Tag<int, string>> DecayTypeList = new List<Tag<int, string>>();
        public static List<Tag<int, string>> PreparationUnitList = new List<Tag<int, string>>();
        public static List<Tag<int, string>> UniformActivityUnitList = new List<Tag<int, string>>();
        public static List<Tag<int, string>> WorkflowStatusList = new List<Tag<int, string>>();
        public static List<Tag<int, string>> LocationTypeList = new List<Tag<int, string>>();

        public static List<SampleTypeModel> SampleTypes = new List<SampleTypeModel>();
    }
}
