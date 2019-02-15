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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DSA_lims
{
    public static class Utils
    {        
        public static readonly string NumberSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;
        public static readonly char NumberSeparatorChar = Convert.ToChar(NumberSeparator);

        public static int MIN_USERNAME_LENGTH { get { return 3; } }
        public static int MIN_PASSWORD_LENGTH { get { return 8; } }

        public static readonly string DateTimeFormatISO = "yyyy-MM-dd HH:mm:ss";
        public static readonly string DateFormatISO = "yyyy-MM-dd";
        public static readonly string DateTimeFormatNorwegian = "dd.MM.yyyy HH:mm:ss";
        public static readonly string DateFormatNorwegian = "dd.MM.yyyy";

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

        public static Image CropImageToHeight(Image img, int height)
        {
            if (img.Height <= height)
                return img;

            double w = img.Width;
            double h = img.Height;
            double scaleFactor = (double)height / h;
            w = w * scaleFactor;
            h = h * scaleFactor;

            return (Image)(new Bitmap(img, (int)w, (int)h));
        }

        public static Image CropImageToWidth(Image img, int width)
        {
            if (img.Width <= width)
                return img;

            double w = img.Width;
            double h = img.Height;
            double scaleFactor = (double)width / w;
            w = w * scaleFactor;
            h = h * scaleFactor;

            return (Image)(new Bitmap(img, (int)w, (int)h));
        }
        
        private static int Coords_GetGroupSuccesses(Match m)
        {
            int nSuccesses = 0;
            foreach (Group g in m.Groups)
            {
                if (g.Success)
                    nSuccesses++;
            }
            return nSuccesses;
        }

        public static double GetLatitude(string input)
        {
            double lat = 0;
            input = input.Replace('°', '*');
            Regex regex = new Regex("^(\\d{1,2})\\*?\\s+(\\d{1,2})'?\\s+(\\d{1,2})\"?\\s*([NS])$");
            Match match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes, Seconds
                if (Coords_GetGroupSuccesses(match) != 5)
                    throw new Exception("Invalid format on latitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);
                double seconds = Convert.ToDouble(match.Groups[3].Value);

                lat = degree + (minutes / 60.0) + (seconds / 3600.0);

                if (match.Groups[4].Value == "S")
                    lat = -lat;

                return lat;
            }

            regex = new Regex("^(\\d{1,2})\\*?\\s+(\\d{1,2}" + NumberSeparator + "?\\d{0,6})'?\\s*([NS])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes
                if (Coords_GetGroupSuccesses(match) != 4)
                    throw new Exception("Invalid format on latitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);

                //degrees = degrees + minutes / 60
                lat = degree + minutes / 60.0;

                if (match.Groups[3].Value == "S")
                    lat = -lat;

                return lat;
            }

            regex = new Regex("^(\\d{1,2}" + NumberSeparator + "?\\d{0,32})\\*?\\s*([NS])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimals with direction
                if (Coords_GetGroupSuccesses(match) != 3)
                    throw new Exception("Invalid format on latitude");

                lat = Convert.ToDouble(match.Groups[1].Value);
                if (match.Groups[2].Value == "S")
                    lat = -lat;

                return lat;
            }

            regex = new Regex("^(-?\\d{1,2}" + NumberSeparator + "?\\d{0,32})$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimal
                if (Coords_GetGroupSuccesses(match) != 2)
                    throw new Exception("Invalid format on latitude");

                lat = Convert.ToDouble(match.Groups[1].Value);

                return lat;
            }

            throw new Exception("Invalid format on latitude");
        }

        public static double GetLongitude(string input)
        {
            double lon = 0;
            input = input.Replace('°', '*');
            Regex regex = new Regex("^(\\d{1,3})\\*?\\s+(\\d{1,2})'?\\s+(\\d{1,2})\"?\\s*([EW])$");
            Match match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes, Seconds
                if (Coords_GetGroupSuccesses(match) != 5)
                    throw new Exception("Invalid format on longitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);
                double seconds = Convert.ToDouble(match.Groups[3].Value);

                lon = degree + (minutes / 60.0) + (seconds / 3600.0);

                if (match.Groups[4].Value == "W")
                    lon = -lon;

                return lon;
            }

            regex = new Regex("^(\\d{1,3})\\*?\\s+(\\d{1,2}" + NumberSeparator + "?\\d{0,6})'?\\s*([EW])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes
                if (Coords_GetGroupSuccesses(match) != 4)
                    throw new Exception("Invalid format on longitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);

                //degrees = degrees + minutes / 60
                lon = degree + minutes / 60.0;

                if (match.Groups[3].Value == "W")
                    lon = -lon;

                return lon;
            }

            regex = new Regex("^(\\d{1,3}" + NumberSeparator + "?\\d{0,32})\\*?\\s*([EW])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Decimals with direction
                if (Coords_GetGroupSuccesses(match) != 3)
                    throw new Exception("Invalid format on longitude");

                lon = Convert.ToDouble(match.Groups[1].Value);
                if (match.Groups[2].Value == "W")
                    lon = -lon;

                return lon;
            }

            regex = new Regex("^(-?\\d{1,3}" + NumberSeparator + "?\\d{0,32})$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimal
                if (Coords_GetGroupSuccesses(match) != 2)
                    throw new Exception("Invalid format on longitude");

                lon = Convert.ToDouble(match.Groups[1].Value);
                return lon;
            }

            throw new Exception("Invalid format on longitude");
        }
    }
}
