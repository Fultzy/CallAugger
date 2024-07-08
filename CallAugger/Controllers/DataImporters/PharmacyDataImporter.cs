using CallAugger.Readers;
using CallAugger.Utilities;
using CallAugger.Utilities.CliInterface;
using CallAugger.Utilities.Sqlite;
using CallAugger.Utilities.Validators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace CallAugger.DataImporters
{
    internal class PharmacyDataImporter : DataImporter
    {
        ///////////////////////////////////////////////////////////////
        //  This class is responsible for reading in call record data
        //  from a file and creating a list of CallRecord objects.

        public Dictionary<string, int> Headers = null;

        public List<List<string>> ReadInPharmacyData()
        {
            var pharmaData = new List<List<string>>();

            // get the path to the Pharmacy Info
            string pharmacyDirPath = Directory.GetCurrentDirectory() + @"\Data\Pharmacy Info";
            var PharmacyFiles = Directory.EnumerateFiles(pharmacyDirPath);

            if (PharmacyFiles.Count() == 0) return null;

            foreach (string filePath in PharmacyFiles)
            {
                var newData = ProcessFile(filePath);
                pharmaData.AddRange(newData);
            }
            
            return pharmaData;
        }


        private List<List<string>> ProcessFile(string filePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);

            Console.WriteLine($"\nImporting Pharmacy File : {fileName}...");
            Logger.Importing($"Importing Pharmacy File : {fileName}...");

            if (fileName.Contains(".csv") || fileName.Contains(".xlsx"))
            {
                // a Whole lotta bullshit
                var excelReader = new ExcelReader(filePath);
                var rawdata = excelReader.ReadData();

                // Standarize Data Formatting
                var thisHeader = HeaderHandler.GetHeaderFromList(rawdata[0]);
                if (Headers == null && HeaderValidator.IsValidCallTrackerHeader(thisHeader))
                {
                    Headers = thisHeader;
                }

                // Sort the data to match the Global header
                var sortedData = HeaderHandler.SortDataToHeader(rawdata, Headers);

                excelReader.CloseReader();
                excelReader = null;

                // move the file into the archive folder
                var archivePath = Path.GetDirectoryName(filePath) + @"\Archive";
                File.Move(filePath, archivePath + fileName);
                
                return sortedData;
            }
            else
            {
                var message = $"!!ERROR!! : {fileName} is not a .csv or .xlsx file.";
                throw new Exception(Logger.Error(Logger.Importing(message)));
            }
        }

        
        public List<Pharmacy> ImportPharmacyData(List<List<string>> pharmaData, SQLiteHandler dbHandle)
        {
            var startTime = DateTime.Now;
            int max = pharmaData.Count - 1;
            ProgressBarUtility.WriteProgressBar(0);

            List<Pharmacy> pharmacies = new List<Pharmacy>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                for (int rowNum = 1; rowNum < pharmaData.Count - 1; rowNum++)
                {
                    var row = pharmaData[rowNum];

                    Pharmacy pharmacy = CreatePharmacy(row);
                    pharmacy = dbHandle.InsertPharmacy(connection, pharmacy);

                    Logger.Importing($"Successfully Added Pharmacy : {pharmacy.InlineDetails()}");

                    // create a Primary Phone Number object
                    if (pharmacy.PrimaryPhoneNumber == null)
                    {
                        var primaryPhoneNumber = CreatePrimaryPhoneNumber(connection, pharmacy);
                        primaryPhoneNumber = dbHandle.InsertPhoneNumber(connection, primaryPhoneNumber);
                        pharmacy.AddPhoneNumber(primaryPhoneNumber);

                        Logger.Importing($"Successfully Added Primary Phone Number : {primaryPhoneNumber.InlineDetails()}");
                    }

                    // create a Fax Number object
                    if (pharmacy.FaxNumber == null)
                    {
                        var faxNumber = CreateFaxNumber(connection, pharmacy);
                        faxNumber = dbHandle.InsertPhoneNumber(connection, faxNumber);
                        pharmacy.AddPhoneNumber(faxNumber);

                        Logger.Importing($"Successfully Added Fax Number : {faxNumber.InlineDetails()}");
                    }

                    pharmacies.Add(pharmacy);

                    ProgressBarUtility.WriteProgressBar(rowNum * 100 / max, true);
                }

                connection.Close();
            }

            var endTime = DateTime.Now;
            var totalTime = endTime - startTime;
            var roundedSeconds = Math.Round(totalTime.TotalSeconds, 2);

            ProgressBarUtility.WriteProgressBar(100, true);
            Console.Write($" ~ {max} Pharmacies - Took {roundedSeconds}s\n");
            return pharmacies;
        }


        private Pharmacy CreatePharmacy(List<string> row)
        {
            var pharmacy = new Pharmacy();
            try
            {
                // Remove any commas from the name
                string phName = row[Headers["Pharmacy Name"]];
                if (phName.Contains(",")) phName = phName.Replace(",", " ");

                pharmacy.Name = phName;
                pharmacy.Npi = row[Headers["NPI #"]];
                pharmacy.Dea = row[Headers["DEA #"]];
                pharmacy.Ncpdp = row[Headers["NCPDP #"]];
                pharmacy.Address = row[Headers["Address"]];
                pharmacy.City = row[Headers["City"]];
                pharmacy.State = row[Headers["State"]];
                pharmacy.Zip = row[Headers["Zip"]];

                // if contact names contains a coma replace it with &
                string name1 = row[Headers["Contact 1 Name"]];
                if (name1 == "" || name1 == null)
                    pharmacy.ContactName1 = "null";
                else if (name1.Contains(","))
                    pharmacy.ContactName1 = name1.Replace(",", "&");
                else
                    pharmacy.ContactName1 = name1;

                string name2 = row[Headers["Contact 2 Name"]];
                if (name2 == "" || name2 == null)
                    pharmacy.ContactName2 = "null";
                else if (name2.Contains(","))
                    pharmacy.ContactName2 = name2.Replace(",", "&");
                else
                    pharmacy.ContactName2 = name2;

                pharmacy.Anniversary = row[Headers["Rx Anniversary"]];
                pharmacy.PrimaryPhoneNumber = row[Headers["Phone # (no dashes)"]];
                pharmacy.FaxNumber = row[Headers["Fax # (no dashes)"]];

                return pharmacy;
            }
            catch (Exception ex)
            {
                var message1 = $"An error occurred while creating pharmacy: {ex.Message}";
                var message2 = $"Pharmacy: {pharmacy.InlineDetails()}";

                // ErrorMenu.For(message1, message2);
                throw new Exception(Logger.Error(Logger.Importing(message1 + "\n" + message2)));
            }
        }


        private PhoneNumber CreatePrimaryPhoneNumber(SQLiteConnection connection, Pharmacy pharmacy)
        {
            PhoneNumber primaryPhoneNumber = new PhoneNumber();

            try
            {
                if (PhoneNumberValidator.IsPhoneNumber(pharmacy.PrimaryPhoneNumber))
                {
                    primaryPhoneNumber.Number = pharmacy.PrimaryPhoneNumber;
                    primaryPhoneNumber.PharmacyID = pharmacy.id;
                    primaryPhoneNumber.State = pharmacy.State;
                    primaryPhoneNumber.IsPrimary = true;

                    Logger.Importing($"Added w/ Primary Phone Number : {primaryPhoneNumber.InlineDetails()}");

                    return primaryPhoneNumber;
                }
                else
                {
                    var message = $"Primary Phone Number is not a valid phone number: {pharmacy.PrimaryPhoneNumber}";
                    throw new Exception(Logger.Error(Logger.Importing(message)));
                }
            }
            catch (Exception ex)
            {
                var message1 = $"An error occurred while creating primary phone number: {ex.Message}";
                var message2 = $"Pharmacy: {pharmacy.InlineDetails()}";
                var message3 = $"Primary Phone Number: {primaryPhoneNumber.InlineDetails()}";

                ErrorMenu.For(message1, message2, message3);
                throw new Exception(Logger.Error(Logger.Importing(message1 + "\n" + message2 + "\n" + message3)));
            }
        }


        private PhoneNumber CreateFaxNumber(SQLiteConnection connection, Pharmacy pharmacy)
        {
            if (pharmacy.FaxNumber == "") return null;
            var faxNumber = new PhoneNumber();

            try
            {
                if (PhoneNumberValidator.IsPhoneNumber(pharmacy.FaxNumber))
                {
                    
                    faxNumber.Number = pharmacy.FaxNumber;
                    faxNumber.PharmacyID = pharmacy.id;
                    faxNumber.State = pharmacy.State;
                    faxNumber.IsFax = true;
                    
                    Logger.Importing($"Added w/ Fax Number : {faxNumber.InlineDetails()}");

                    return faxNumber;
                }
                else
                {
                    var message = $"Fax Number is not a valid phone number: {pharmacy.PrimaryPhoneNumber}";
                    throw new Exception(Logger.Error(Logger.Importing(message)));
                }
            }
            catch (Exception ex)
            {
                var message1 = $"An error occurred while creating Fax phone number: {ex.Message}";
                var message2 = $"Pharmacy: {pharmacy.InlineDetails()}";
                var message3 = $"Fax Phone Number: {faxNumber.InlineDetails()}";

                ErrorMenu.For(message1, message2, message3);
                throw new Exception(Logger.Error(Logger.Importing(message1 + "\n" + message2 + "\n" + message3)));
            }
        }

    }
}
