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
using log4net.Core;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace DSA_lims
{    
    public static class DSALogger
    {
        private static bool initialized = false;
        private static string mLogFile;
        public static string LogFile { get { return mLogFile; } }

        public static ILog CreateLogger(string logFile)
        {
            if(initialized)            
                throw new Exception("Logger is already created");

            mLogFile = logFile;
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = false;
            roller.File = LogFile;
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 3;
            roller.MaximumFileSize = "10MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);            

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            initialized = true;
            return LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }        
    }
}
