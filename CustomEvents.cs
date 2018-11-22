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
using System.Windows.Forms;
using System.Globalization;

namespace DSA_lims
{        
    public static class CustomEvents
    {
        public static void Integer_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow numbers
            if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar))
                e.Handled = true;
        }

        public static void Numeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow decimals            
            char sep = Convert.ToChar(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);

            TextBox tb = (TextBox)sender;
            if (e.KeyChar == sep)
            {
                // Only allow one separator
                foreach (char c in tb.Text)
                {
                    if (c == sep)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }

            if (e.KeyChar == '-')
            {
                // Only allow one minus
                foreach (char c in tb.Text)
                {
                    if (c == '-')
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if(tb.SelectionStart != 0)
                {
                    e.Handled = true;
                    return;
                }
            }

            if (!Char.IsNumber(e.KeyChar) && !Char.IsControl(e.KeyChar) && e.KeyChar != sep && e.KeyChar != '-')
                e.Handled = true;
        }
    }    
}
