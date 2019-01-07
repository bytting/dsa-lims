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
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DSA_lims
{
    // Class used to store application settings
    [Serializable()]
    public class DSASettings
    {
        public DSASettings()
        {
            UseActiveDirectoryCredentials = true;
        }        
        
        public bool UseActiveDirectoryCredentials { get; set; }
        public string ConnectionString { get; set; }
        public string LabelPrinterName { get; set; }
        public string LabelPrinterPaperName { get; set; }
        public bool LabelPrinterLandscape { get; set; }

        public string ScannerName { get; set; }
        public bool ScannerDuplex { get; set; }
        public string ScannerFlipType { get; set; }
        public string ScannerPixelType { get; set; }
    }
}
