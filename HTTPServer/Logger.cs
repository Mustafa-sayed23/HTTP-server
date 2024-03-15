using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        
        public static void LogException(Exception ex)
        {
            DateTime date = DateTime.Now;
            // TODO: Create log file named log.txt to log exception details in it
            //Datetime:
            //message:
            // for each exception write its details associated with datetime 
            Configuration.message[Configuration.count] = "DateTime: " + date.ToString() + "\nmessage: " + ex.Message;
            string s = "";
            Configuration.count++;
            for (int i = 0; i < Configuration.count; i++)
            { 
                s += Configuration.message[i];
                s += "\n";
            }
            File.WriteAllText("log.txt", s);
        }
    }
}
