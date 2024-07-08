using CallAugger.Utilities;
using CallAugger.Utilities.Sqlite;
using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;

namespace CallAugger.Readers
{
    public class CsvReader
    {
        ///////////////////////////////////////////////////////////////
        //

        public readonly string overwriteWarning = "If you continue Imporing assignments from file this will OVERRIDE any existing assignments.. Continue?(y/n): ";

        public void ImportAssignedPhoneNumbers(SQLiteHandler dbHandle)
        {
            // Make sure the user is sure
            bool confirmClear = new CliMenu().ConfirmationPrompt(overwriteWarning);

            if (confirmClear == false) return;

            // see if the file exists
            var csvFilePath = Directory.GetCurrentDirectory() + @"\Data\Exported Assignments.csv";

            if (CheckExportFile(csvFilePath) == false)
            {
                Console.WriteLine("No Exported Assignments File Found.");
                new CliMenu().AnyKey();
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                using (var Reader = new StreamReader(csvFilePath))
                {
                    string line;
                    while ((line = Reader.ReadLine()) != null)
                    {
                        var row = line.Split(',');
                        Pharmacy pharmacy = ProcessRow(connection, dbHandle, row);

                        pharmacy.InlineDetails();
                    }
                }

                connection.Close();
            }

            Console.WriteLine("\nImport Complete!");
        }


        private bool CheckExportFile(string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
            {
                Console.WriteLine("\n" + @" Data\Exported Assignments.csv does not exist, Export before Importing!");
                return false;
            }

            return true;
        }


        private Pharmacy ProcessRow(SQLiteConnection connection, SQLiteHandler dbHandle, string[] row)
        {
            // create a pharmacy object from the row
            var pharmacy = new Pharmacy().FromCsv(row);
            pharmacy = dbHandle.PharmacyRepo.Insert(connection, pharmacy);

            // create the primary phone number
            var primaryPhoneNumber = new PhoneNumber()
            {
                Number = pharmacy.PrimaryPhoneNumber,
                State = pharmacy.State,
                PharmacyID = pharmacy.id,
                IsPrimary = true
            };

            // find or create the phone number in the database
            primaryPhoneNumber = dbHandle.PhoneNumberRepo.Insert(connection, primaryPhoneNumber);

            // create the fax number
            var faxNumber = new PhoneNumber()
            {
                Number = pharmacy.FaxNumber,
                State = pharmacy.State,
                PharmacyID = pharmacy.id,
                IsFax = true
            };

            // find or create the phone number in the database
            faxNumber = dbHandle.PhoneNumberRepo.Insert(connection, faxNumber);

            // itterate through the rest of the row
            for (int i = 13; i < row.Length; i++)
            {
                var pn = row[i].Split(':');
                PhoneNumber phoneNumber = new PhoneNumber()
                {
                    Number = pn[0].ToString(),
                    State = pn[1].ToString(),
                    PharmacyID = pharmacy.id
                };


                // find or create the phone number in the database
                phoneNumber = dbHandle.PhoneNumberRepo.Insert(connection, phoneNumber);
                Console.WriteLine(phoneNumber.Number+ " " +phoneNumber.TotalCalls);
            }
            
            var message = $"Imported Pharmacy: {pharmacy.Name} with {pharmacy.PhoneNumbers.Count} phone numbers from Exported Assignments";

            Console.WriteLine(Logger.Importing(message));

            return pharmacy;
        }

    }
}