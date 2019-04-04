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
using System.Text.RegularExpressions;

namespace DSA_lims
{
    public static class UtilsGeo
    {
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

                if (Math.Abs(degree) > 90d)
                    throw new Exception("Latitude degree is out of range");

                if (Math.Abs(minutes) > 60d)
                    throw new Exception("Latitude minutes is out of range");

                if (Math.Abs(seconds) > 60d)
                    throw new Exception("Latitude seconds is out of range");

                lat = degree + (minutes / 60.0) + (seconds / 3600.0);

                if (match.Groups[4].Value == "S")
                    lat = -lat;

                return lat;
            }

            regex = new Regex("^(\\d{1,2})\\*?\\s+(\\d{1,2}" + Utils.NumberSeparator + "?\\d{0,6})'?\\s*([NS])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes
                if (Coords_GetGroupSuccesses(match) != 4)
                    throw new Exception("Invalid format on latitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);

                if (Math.Abs(degree) > 90d)
                    throw new Exception("Latitude degree is out of range");

                if (Math.Abs(minutes) > 60d)
                    throw new Exception("Latitude minutes is out of range");

                //degrees = degrees + minutes / 60
                lat = degree + minutes / 60.0;

                if (match.Groups[3].Value == "S")
                    lat = -lat;

                return lat;
            }

            regex = new Regex("^(\\d{1,2}" + Utils.NumberSeparator + "?\\d{0,32})\\*?\\s*([NS])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimals with direction
                if (Coords_GetGroupSuccesses(match) != 3)
                    throw new Exception("Invalid format on latitude");

                lat = Convert.ToDouble(match.Groups[1].Value);
                if (match.Groups[2].Value == "S")
                    lat = -lat;

                if (Math.Abs(lat) > 90d)
                    throw new Exception("Latitude is out of range");

                return lat;
            }

            regex = new Regex("^(-?\\d{1,2}" + Utils.NumberSeparator + "?\\d{0,32})$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimal
                if (Coords_GetGroupSuccesses(match) != 2)
                    throw new Exception("Invalid format on latitude");

                lat = Convert.ToDouble(match.Groups[1].Value);

                if (Math.Abs(lat) > 90d)
                    throw new Exception("Latitude is out of range");

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

                if (Math.Abs(degree) > 180d)
                    throw new Exception("Longitude degree is out of range");

                if (Math.Abs(minutes) > 60d)
                    throw new Exception("Longitude minutes is out of range");

                if (Math.Abs(seconds) > 60d)
                    throw new Exception("Longitude seconds is out of range");

                lon = degree + (minutes / 60.0) + (seconds / 3600.0);

                if (match.Groups[4].Value == "W")
                    lon = -lon;

                return lon;
            }

            regex = new Regex("^(\\d{1,3})\\*?\\s+(\\d{1,2}" + Utils.NumberSeparator + "?\\d{0,6})'?\\s*([EW])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Degrees, Minutes
                if (Coords_GetGroupSuccesses(match) != 4)
                    throw new Exception("Invalid format on longitude");

                double degree = Convert.ToDouble(match.Groups[1].Value);
                double minutes = Convert.ToDouble(match.Groups[2].Value);

                if (Math.Abs(degree) > 180d)
                    throw new Exception("Longitude degree is out of range");

                if (Math.Abs(minutes) > 60d)
                    throw new Exception("Longitude minutes is out of range");

                //degrees = degrees + minutes / 60
                lon = degree + minutes / 60.0;

                if (match.Groups[3].Value == "W")
                    lon = -lon;

                return lon;
            }

            regex = new Regex("^(\\d{1,3}" + Utils.NumberSeparator + "?\\d{0,32})\\*?\\s*([EW])$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Decimals with direction
                if (Coords_GetGroupSuccesses(match) != 3)
                    throw new Exception("Invalid format on longitude");

                lon = Convert.ToDouble(match.Groups[1].Value);
                if (match.Groups[2].Value == "W")
                    lon = -lon;

                if (Math.Abs(lon) > 180d)
                    throw new Exception("Longitude is out of range");

                return lon;
            }

            regex = new Regex("^(-?\\d{1,3}" + Utils.NumberSeparator + "?\\d{0,32})$");
            match = regex.Match(input);
            if (match.Success)
            {
                // Desimal
                if (Coords_GetGroupSuccesses(match) != 2)
                    throw new Exception("Invalid format on longitude");

                lon = Convert.ToDouble(match.Groups[1].Value);

                if (Math.Abs(lon) > 180d)
                    throw new Exception("Longitude out of range");

                return lon;
            }

            throw new Exception("Invalid format on longitude");
        }
    }
}
