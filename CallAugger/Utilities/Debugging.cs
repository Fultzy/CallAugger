using CallAugger.Parsers;
using CallAugger.DataImporters;
using CallAugger.Utilities.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SQLite;

namespace CallAugger.Utilities
{
    static class Debugging
    {
        // Clears the db and recreates the tables
        public static void Cleardb(SQLiteHandler dbHandle)
        {
            if (ConfigurationManager.AppSettings["allow_db_clear"] == "true")
            {
                // Make sure the user is sure
                bool confirmClear = new CliMenu().ConfirmationPrompt($"Are You Sure You Want To Clear The Database? (y/n)");
                if (confirmClear != true) return;

                bool confirmAgain = new CliMenu().ConfirmationPrompt($"Really tho? (y/n)");
                if (confirmAgain != true) return;

                // Drop Tables !!!!!! DANGER !!!!!!
                dbHandle.DropTables();
                dbHandle.CreateTables();
            }
            else
            {
                Console.WriteLine("\nThis feature is disabled in the settings file.");
                Console.WriteLine("Modify the Settings.txt file to enable it.");
            }
        }

        // Simulates the assignment of phone numbers to pharmacies
        public static void SimulateAssignment(SQLiteHandler dbHandle)
        {
            if (ConfigurationManager.AppSettings["allow_simulation"] == "true")
            {
                // Make sure the user is sure
                bool confirmClear = new CliMenu().ConfirmationPrompt($"Are You Sure You Want Assign Random Phone Numbers To Random Pharmacies? (y/n)");
                if (confirmClear != true) return;

                bool confirmAgain = new CliMenu().ConfirmationPrompt($"Really tho? this will really mess ur stuff up.. (y/n)");
                if (confirmAgain != true) return;

                var unassignedPhoneNumbers = dbHandle.GetUnassignedPhoneNumbers();
                var allPharmacies = dbHandle.GetAllPharmacies();

                // assign half of the unassigned phone numbers to a random pharmacy
                foreach (PhoneNumber pn in unassignedPhoneNumbers.Take(unassignedPhoneNumbers.Count / 2))
                {
                    var randomPharmacy = allPharmacies[new Random().Next(allPharmacies.Count)];
                    pn.PharmacyID = randomPharmacy.id;
                    dbHandle.UpdatePhoneNumberPharmacyID(pn, randomPharmacy.id);
                }
            }
            else
            {
                Console.WriteLine("\nThis feature is disabled in the settings file.");
                Console.WriteLine("Modify the Settings.txt file to enable it.");
            }
        }

        // Rebuild the db from the archive using the newest files
        public static void RebuildDB(SQLiteHandler dbHandle)
        {
            if (ConfigurationManager.AppSettings["allow_db_rebuild"] == "true")
            {
                // Make sure the user is sure
                bool confirmRebuild = new CliMenu().ConfirmationPrompt($"Are You Sure You Want To Rebuild The Database? (y/n)");
                if (confirmRebuild != true) return;

                bool confirmRebuildAgain = new CliMenu().ConfirmationPrompt($"Really tho? (y/n)");
                if (confirmRebuildAgain != true) return;

                // Drop Tables !!!!!! DANGER !!!!!!
                dbHandle.DropTables();
                dbHandle.CreateTables();

                // Move the newest files from the archive to the data folders
                string path = Directory.GetCurrentDirectory();
                MoveNewestFiles(path);

                // Import the new data
                var IDataImporter = new DataImporter() { dbHandle = dbHandle };
                List<Pharmacy> newPharmacies = IDataImporter.ImportPharmacyData();
                List<CallRecord> newCallRecords = IDataImporter.ImportCallData();

                // Parce new Data (CallRecords need to be first)
                var IParser = new Parse() { dbHandle = dbHandle };
                IParser.ParseCallRecordData(newCallRecords);
                IParser.ParsePharmacyData();
            }
            else
            {
                Console.WriteLine("\nThis feature is disabled in the settings file.");
                Console.WriteLine("Modify the Settings.txt file to enable it.");
            }
        }

        // Move the newest files from the archive to the data folders (used in RebuildDB)
        private static void MoveNewestFiles(string path)
        {
            // move if there isnt already a file in the data folder
            if (Directory.EnumerateFiles(path + @"\data\Call Records").Count() == 0)
            {
                var callArchivePath = path + @"\Data\Call Records\Archive";
                var callFiles = Directory.EnumerateFiles(callArchivePath);

                var newestCallFile = callFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();

                File.Move(newestCallFile, path + @"\Data\Call Records\" + newestCallFile.Substring(callArchivePath.Length));
            }

            if (Directory.EnumerateFiles(path + @"\data\Pharmacy Info").Count() == 0)
            {
                var pharmacyArchivePath = path + @"\Data\Pharmacy Info\Archive";
                var pharmacyFiles = Directory.EnumerateFiles(pharmacyArchivePath);

                var newestPharmacyFile = pharmacyFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();

                File.Move(newestPharmacyFile, path + @"\Data\Pharmacy Info\" + newestPharmacyFile.Substring(pharmacyArchivePath.Length));
            }
        }

        // check two files, link them by pharmacy id and phonenumber pharmacy id
        public static void ImportAssignmentsFromv1_1db(SQLiteHandler dbHandle)
        {
            CliMenu menu = new CliMenu() 
            {
                OptionPadding = 3,
                MenuName = "Import Assignments from v1.1 db:\n",
                Prompt = "Enter the path to the v1.1 db: ",
                Exit = "Back to Main Menu"
            };

            string input = "";

            while (!File.Exists(input) && Path.GetExtension(input) != ".db")
            {
                // ask the user for the path to the v1.1 db
                input = menu.WriteMenu();

                // if the user wants to go back to the main menu
                if (input == "Back to Main Menu") return;

                // if the user entered a path make sure it exists
                if (!File.Exists(input))
                {
                    Console.WriteLine("\nFile not found.");
                    menu.AnyKey();
                    break;
                }
            }

            // create the connection string 
            string connectionString = $"Data Source={input};Version=3;";

            // connect to the v1.1 db
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // get all pharmacies from the pharmacy table
                List<Pharmacy> pharmacies = new List<Pharmacy>();

                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllPharmacies, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Pharmacy pharmacy = dbHandle.PharmacyRepo.CreateFromReader(reader);
                    pharmacies.Add(pharmacy);
                }

                // get all phone numbers from the phone number table
                List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

                command = new SQLiteCommand(QueryStore.SelectAllPhoneNumbers, connection);
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhoneNumber phoneNumber = dbHandle.PhoneNumberRepo.CreateFromReader(reader);
                    phoneNumbers.Add(phoneNumber);
                }

                // match the phone numbers to the pharmacies
                foreach (PhoneNumber pn in phoneNumbers)
                {
                    // find the pharmacy that matches the phone number
                    Pharmacy pharmacy = pharmacies.Find(p => p.id == pn.PharmacyID);

                    // if the pharmacy was found
                    if (pharmacy != null)
                    {
                        // update the phone number with the pharmacy id
                        dbHandle.UpdatePhoneNumberPharmacyID(pn, pharmacy.id);
                    }
                }

            }
        }
    }
}
