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
using log4net;
using System.Drawing;

namespace DSA_lims
{
    public static class Common
    {
        public static ILog Log = null;

        public static DSASettings Settings = new DSASettings();

        public static Guid UserId { get; set; }
        public static string Username { get; set; }        

        public static Guid LabId { get; set; }
        public static Image LabLogo { get; set; }
        public static Image LabAccredLogo { get; set; }

        public static List<SampleTypeModel> SampleTypeList = new List<SampleTypeModel>();

        public static DateTime CurrentDate(bool inclusive)
        {
            DateTime n = DateTime.Now;

            if (inclusive)            
                return new DateTime(n.Year, n.Month, n.Day, 23, 59, 59);            
            else            
                return new DateTime(n.Year, n.Month, n.Day, 0, 0, 1);            
        }
    }
}
