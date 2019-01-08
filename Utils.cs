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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DSA_lims
{
    public static class Utils
    {
        public static int MIN_PASSWORD_LENGTH { get { return 8; } }

        public static string DateTimeFormatISO = "yyyy-MM-dd HH:mm:ss";
        public static string DateFormatISO = "yyyy-MM-dd";
        public static string DateTimeFormatNorwegian = "dd.MM.yyyy HH:mm:ss";
        public static string DateFormatNorwegian = "dd.MM.yyyy";

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
    }
}
