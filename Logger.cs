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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
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
        public static ILog CreateLogger(RichTextBox tb)
        {
            if(initialized)            
                throw new Exception("Logger is already created");

            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "%date [%thread] - %message%newline";
            patternLayout.ActivateOptions();

            RollingFileAppender roller = new RollingFileAppender();
            roller.AppendToFile = false;
            roller.File = DSAEnvironment.SettingsPath + Path.DirectorySeparatorChar + "dsa-lims.log";
            roller.Layout = patternLayout;
            roller.MaxSizeRollBackups = 3;
            roller.MaximumFileSize = "10MB";
            roller.RollingStyle = RollingFileAppender.RollingMode.Size;
            roller.StaticLogFileName = true;
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            TextBoxAppender textBoxAppender = new TextBoxAppender(tb);
            textBoxAppender.Threshold = Level.All;
            textBoxAppender.Layout = patternLayout;
            textBoxAppender.ActivateOptions();
            hierarchy.Root.AddAppender(textBoxAppender);

            hierarchy.Root.Level = Level.All;
            hierarchy.Configured = true;

            initialized = true;
            return LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
    }

    public class TextBoxAppender : AppenderSkeleton
    {
        private RichTextBox mTextBox;

        public TextBoxAppender(RichTextBox tb)
        {
            mTextBox = tb;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            mTextBox.BeginInvoke((MethodInvoker)delegate
            {
                mTextBox.Text += RenderLoggingEvent(loggingEvent);
            });
        }
    }
}
