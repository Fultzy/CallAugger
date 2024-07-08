using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CallAugger.Settings
{
    public class AppSettings
    {
        static AppSettings()
        {
            // set the console window size
            Console.SetWindowSize(120, 45);
            Console.WriteLine("Loading Program:");

            var loadedSettings = LoadSettings();

            ConfigurationManager.AppSettings["program_version"] = "v1.5";

            // Basic Settings
            ConfigurationManager.AppSettings["db_path"] = Directory.GetCurrentDirectory() + @"\Data\Auggered.db";
            ConfigurationManager.AppSettings["default_dateRange"] = loadedSettings["default_dateRange"];
            ConfigurationManager.AppSettings["ignore_users"] = loadedSettings["ignore_users"];

            // Report Settings
            ConfigurationManager.AppSettings["header_color"] = loadedSettings["header_color"];
            ConfigurationManager.AppSettings["item_accent_color"] = loadedSettings["item_accent_color"];
            ConfigurationManager.AppSettings["mini_header_color"] = loadedSettings["mini_header_color"];
            ConfigurationManager.AppSettings["alternating_row_color1"] = loadedSettings["alternating_row_color1"];
            ConfigurationManager.AppSettings["alternating_row_color2"] = loadedSettings["alternating_row_color2"];

            // Report Add-ons
            ConfigurationManager.AppSettings["weekGraph_pharmacies"] = loadedSettings["weekGraph_pharmacies"];
            ConfigurationManager.AppSettings["monthGraph_pharmacies"] = loadedSettings["monthGraph_pharmacies"];


            // DEBUGGING
            ConfigurationManager.AppSettings["time_to_access"] = loadedSettings["time_to_access"];

            ConfigurationManager.AppSettings["allow_db_clear"] = loadedSettings["allow_db_clear"];
            ConfigurationManager.AppSettings["allow_db_rebuild"] = loadedSettings["allow_db_rebuild"];
            ConfigurationManager.AppSettings["allow_simulation"] = loadedSettings["allow_simulation"];
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
