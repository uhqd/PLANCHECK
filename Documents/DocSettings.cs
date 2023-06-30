using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanCheck
{
    public class DocSettings
    {
        public String HostName { get; private set; }
        public String Port { get; private set; }
        public String DocKey { get; private set; }
        public String ImportDir { get; private set; }

        public static DocSettings ReadSettings()//String settingsFilePath)
        {


            DocSettings docSettings = new DocSettings();
            docSettings.HostName = "srvaria15-web";
            docSettings.Port = "55051";
            docSettings.DocKey = "ce04163e-39dd-4be5-b3c1-da7154588c7a";
            docSettings.ImportDir = @"\\srvaria15-img\va_data$\Documents";

            return docSettings;
        }

        public static string RemoveWhitespace(string line)
        {
            return string.Join("", line.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
