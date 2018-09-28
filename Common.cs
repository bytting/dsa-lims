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

using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace DSA_lims
{
    public static class Common
    {
        public static ILog Log = null;
        public static DSASettings Settings = new DSASettings();
        public static string Username { get; set; }

        public static List<Lemma<int, string>> InstanceStatusList = new List<Lemma<int, string>>();
        public static List<Lemma<int, string>> DecayTypeList = new List<Lemma<int, string>>();
        public static List<Lemma<int, string>> PreparationUnitList = new List<Lemma<int, string>>();
        public static List<Lemma<int, string>> UniformActivityUnitList = new List<Lemma<int, string>>();
        public static List<Lemma<int, string>> WorkflowStatusList = new List<Lemma<int, string>>();
        public static List<Lemma<int, string>> LocationTypeList = new List<Lemma<int, string>>();

        public static List<SampleTypeModel> SampleTypes = new List<SampleTypeModel>();
    }
}
