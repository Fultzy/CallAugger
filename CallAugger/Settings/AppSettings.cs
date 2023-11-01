using CallAugger.Controllers.Generators.Add_ons;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Settings
{
    public class AppSettings
    {
        static AppSettings()
        {
            // set the console window size
            Console.SetWindowSize(120, 45);

            var settings = LoadSettings();

            // set the app settings
            ConfigurationManager.AppSettings["default_dateRange"] = settings["default_dateRange"];

            ConfigurationManager.AppSettings["header_color"] = settings["header_color"];
            ConfigurationManager.AppSettings["item_accent_color"] = settings["item_accent_color"];
            ConfigurationManager.AppSettings["mini_header_color"] = settings["mini_header_color"];
            ConfigurationManager.AppSettings["alternating_row_color1"] = settings["alternating_row_color1"];
            ConfigurationManager.AppSettings["alternating_row_color2"] = settings["alternating_row_color2"];

            ConfigurationManager.AppSettings["weekGraph_pharmacies"] = settings["weekGraph_pharmacies"];
            ConfigurationManager.AppSettings["monthGraph_pharmacies"] = settings["monthGraph_pharmacies"];

            ConfigurationManager.AppSettings["ignore_users"] = settings["ignore_users"];
        }


        static Dictionary<string, string> LoadSettings()
        {
            // load from file
            var path = Directory.GetCurrentDirectory();
            var settingsPath = path + @"\Settings.txt";

            // open the file and read the settings into a dictionary
            Dictionary<string, string> settings = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(settingsPath))
            { 
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // ignore anything after "//"
                    if (line.Contains("//")) line = line.Substring(0, line.IndexOf("//"));

                    // if the line is empty, skip it
                    if (line == "") continue;

                    // split the line into key and value
                    string[] splitLine = line.Split('=');

                    // add the key and value to the dictionary
                    settings.Add(splitLine[0].Trim(), splitLine[1].Trim());
                }

                return settings;
            }
        }
    }
}
