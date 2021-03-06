﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Pokemon3D.Common.FileSystem
{
    public class LogPathProvider : PathProvider
    {
        const string LOG_FILE_EXTENSION = ".txt";
        const string LOG_FILE_PREFIX = "log_";
        const string LOG_DIRECTORY = "Logs";

        /// <summary>
        /// The directory logs are placed in.
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                return Path.Combine(StartupPath, LOG_DIRECTORY);
            }
        }

        /// <summary>
        /// Returns the path to the current log file.
        /// </summary>
        public static string LogFile
        {
            get
            {
                /*
                  The log file will have this format:
                  log_yyyy-mm-dd.txt, corresponding to when the log file name is requested.
                */

                var now = DateTime.Now;
                StringBuilder sb = new StringBuilder(LOG_FILE_PREFIX);

                //D2 formats the number with a leading zero, if needed.
                sb.Append(now.Year.ToString());
                sb.Append("-");
                sb.Append(now.Month.ToString("D2"));
                sb.Append("-");
                sb.Append(now.Day.ToString("D2"));
                sb.Append(LOG_FILE_EXTENSION); //Append file ending.

                //Combine the startup path and the file name constructed by the string builder:
                return Path.Combine(LogDirectory, sb.ToString());
            }
        }
    }
}
