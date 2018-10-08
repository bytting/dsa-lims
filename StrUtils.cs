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
using System.Linq;
using System.Text;

namespace DSA_lims
{
    public static class StrUtils
    {
        public static string DateFormatISO = "yyyy-MM-dd HH:mm:ss";
        public static string DateFormatNorwegian = "dd.MM.yyyy HH:mm:ss";

        public static string makeStatusMessage(string msg)
        {
            return DateTime.Now.ToString(DateFormatNorwegian) + " - " + msg;
        }

        public static string makeErrorMessage(string msg)
        {
            return msg + ". See log for more details";
        }

        public static string SampleTypeNameToLabel(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            string[] items = name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length < 1)
                return String.Empty;

            return items[items.Length - 1] + " -> " + name;
        }

        public static string LabelToSampleTypeName(string label)
        {
            if (String.IsNullOrEmpty(label))
                return String.Empty;

            int idx = label.IndexOf(" -> ");
            if (idx < 0)
                return String.Empty;

            return label.Substring(idx + 4);
        }
    }
}
