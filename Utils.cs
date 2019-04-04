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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DSA_lims
{
    public static class Utils
    {        
        public static readonly string NumberSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
        public static readonly char NumberSeparatorChar = Convert.ToChar(NumberSeparator);

        public static int MIN_USERNAME_LENGTH { get { return 3; } }
        public static int MIN_PASSWORD_LENGTH { get { return 8; } }

        public static readonly string DateFormatISO = "yyyy-MM-dd";
        public static readonly string TimeFormatISO = "HH:mm:ss";
        public static readonly string DateTimeFormatISO = DateFormatISO + " " + TimeFormatISO;
        public static readonly string DateFormatNorwegian = "dd.MM.yyyy";
        public static readonly string TimeFormatNorwegian = "HH:mm:ss";
        public static readonly string DateTimeFormatNorwegian = DateFormatNorwegian + " " + TimeFormatNorwegian;

        public static readonly string ScientificFormat = "0.###E+0";

        public static string makeStatusMessage(string msg)
        {            
            return DateTime.Now.ToString(DateTimeFormatNorwegian) + " - " + msg;
        }

        public static string makeErrorMessage(string msg)
        {
            return msg + ". See log for more details";
        }        

        public static bool IsValidGuid(object id)
        {
            if (id == null)
                return false;

            Guid g;
            if(!Guid.TryParse(id.ToString(), out g) || g == Guid.Empty)
                return false;

            return true;
        }

        public static Guid MakeGuid(object o)
        {
            if (o == null || o == DBNull.Value)
                return Guid.Empty;

            Guid g;
            if (!Guid.TryParse(o.ToString(), out g) || g == Guid.Empty)
                return Guid.Empty;

            return Guid.Parse(o.ToString());
        }

        public static int? ToInt32(object o)
        {
            if (o == null)
                return null;

            if (o.GetType() == typeof(String) && o.ToString().Trim() == "")
                return null;

            return Convert.ToInt32(o);
        }

        public static double? ToDouble(object o)
        {
            if (o == null)
                return null;

            if (o.GetType() == typeof(String) && o.ToString().Trim() == "")
                return null;

            return Convert.ToDouble(o);
        }

        public static string ToString(this double? self, string format)
        {
            if (self == null)
                return "";

            return self.Value.ToString(ScientificFormat);
        }

        public static byte[] MakePasswordHash(string password, string username)
        {
            return MakePasswordHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(username.ToUpper().Substring(0, 3)));
        }

        private static byte[] MakePasswordHash(byte[] value, byte[] salt)
        {
            byte[] saltedValue = value.Concat(salt).ToArray();
            return new SHA256Managed().ComputeHash(saltedValue);
        }

        public static bool PasswordHashEqual(byte[] hash1, byte[] hash2)
        {
            return hash1.SequenceEqual(hash2);
        }                
    }
}
