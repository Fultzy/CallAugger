using System;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using CallAugger.Utilities;
using CallAugger.Utilities.Sqlite;

namespace CallAugger.Generators
{
    internal class CsvGenerator
    {
        public readonly string overwriteWarning = "\nExporting Assigments will OVERWRITE the existing file.. Continue? (y/n)";


        internal void ExportAssignedPhoneNumberCSV(SQLiteHandler dbHandle)
        {
            var csvFilePath = Directory.GetCurrentDirectory() + @"\Data\Exported Assignments.csv";
            if (CheckExportFile(csvFilePath) == false) return;

            var allPharmacies = dbHandle.GetAllPharmacies();

            using (var writer = new StreamWriter(csvFilePath))
            {
                foreach (Pharmacy pharmacy in allPharmacies)
                {
                    if (pharmacy.PhoneNumbers.Any(pn => pn.IsFax == false && pn.IsPrimary == false))
                    {
                        WriteData(writer, pharmacy);
                    }
                }
            }

            Console.WriteLine("\nExported Assigned.csv Created in the Data directory");
        }


        private bool CheckExportFile(string csvFilePath)
        {
            if (File.Exists(csvFilePath))
            {
                // Make sure the user is sure
                return new CliMenu().ConfirmationPrompt(overwriteWarning);
            }

            return true;
        }


        private void WriteData(StreamWriter writer, Pharmacy pharmacy)
        {
            writer.Write(pharmacy.ToCsv());

            foreach (var phoneNumber in pharmacy.PhoneNumbers)
            {
                if (phoneNumber.IsPrimary) continue;
                if (phoneNumber.IsFax) continue;

                writer.Write($",{phoneNumber.ToCsv()}");
            }

            writer.Write("\n");
        }

    }
}