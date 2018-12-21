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

namespace DSA_lims
{
    public static class Role
    {        
        public const string LaboratoryAdministrator = "Laboratory Administrator";
        public const string LaboratoryOperator = "Laboratory Operator";
        public const string OrderAdministrator = "Order Administrator";
        public const string OrderOperator = "Order Operator";
        public const string SampleRegistration = "Sample Registration";
        public const string Spectator = "Spectator";
    }

    public static class Roles
    {        
        public static List<string> UserRoles = new List<string>();

        public static bool UserHasRole(string roleName)
        {
            return UserRoles.Exists(x => x == roleName.ToUpper());
        }

        public static bool UserIsAdmin()
        {
            return Common.Username.ToUpper() == "LIMSADMINISTRATOR";
        }
    }
}
