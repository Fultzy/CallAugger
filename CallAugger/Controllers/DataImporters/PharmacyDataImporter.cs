using CallAugger.Utilities;
using CallAugger.Utilities.DataBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace CallAugger.Controllers.DataImporters
{
    internal class PharmacyDataImporter : DataImporter
    {
        ///////////////////////////////////////////////////////////////
        //  This class is responsible for reading in call record data
        //  from a file and creating a list of CallRecord objects.
        //  This class is responsible for reading in call record data


        // open and read in Pharmacy data into a list of lists
        public List<List<string>> ReadInPharmacyData(string path)
        {
            // get the path to the call records
            string pharmacyDirPath = path + @"\Data\Pharmacy Info";


            // get the names of all the files in the call records directory
            var PharmacyFiles = Directory.EnumerateFiles(pharmacyDirPath);
            var data = new List<List<string>>();

            if (PharmacyFiles.Count() == 0) return null;

            ///////////////////////////////////////////////
            // Read Each File in the CallRecords Directory
            foreach (string currentFile in PharmacyFiles)
            {
                string fileName = currentFile.Substring(pharmacyDirPath.Length);
                var fullFilePath = pharmacyDirPath + fileName;


                if (fileName.Contains(".csv") || fileName.Contains(".xlsx"))
                {
                    // a Whole lotta bullshit
                    var xReader = new ExcelReader(fullFilePath);
                    data.AddRange(xReader.ReadData());
                    xReader.CloseReader();


                    // move the file into the archive folder
                    var archivePath = path + @"\Data\Pharmacy Info\Archive";
                    File.Move(fullFilePath, archivePath + fileName);
                }
                else
                {
                    throw new Exception($"Error: {fileName} is not a .csv or .xlsx file.");
                }
            }
            
            return data;
        }


        // Create a list of pharmacies from the workbooks
        public List<Pharmacy> CreatePharmacyRecords(List<List<string>> pharmaData)
        {
            // Create Data store
            SQLiteHandler dbHandle = new SQLiteHandler();
            Console.Write(" Checking DB for new Entries...");


            // the first row in call data contains headers.
            var headers = new Dictionary<string, int>();
            var headerRow = pharmaData[0];
            for (int i = 0; i < headerRow.Count; i++)
            {
                headers.Add(headerRow[i], i);
            }

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                for (int rowNum = 1; rowNum < pharmaData.Count; rowNum++)
                {

                    ///////////////////////////////////////////////
                    // Create a Pharmacy object
                    var pharmacy = new Pharmacy();
                    {
                        pharmacy.Name = pharmaData[rowNum][headers["Pharmacy Name"]];
                        pharmacy.Npi = pharmaData[rowNum][headers["NPI #"]];
                        pharmacy.Dea = pharmaData[rowNum][headers["DEA #"]];
                        pharmacy.Ncpdp = pharmaData[rowNum][headers["NCPDP #"]];
                        pharmacy.Address = pharmaData[rowNum][headers["Address"]];
                        pharmacy.City = pharmaData[rowNum][headers["City"]];
                        pharmacy.State = pharmaData[rowNum][headers["State"]];
                        pharmacy.Zip = pharmaData[rowNum][headers["Zip"]];
                        pharmacy.ContactName1 = pharmaData[rowNum][headers["Contact 1 Name"]];
                        pharmacy.ContactName2 = pharmaData[rowNum][headers["Contact 2 Name"]];
                        pharmacy.PrimaryPhoneNumber = pharmaData[rowNum][headers["Phone # (no dashes)"]];
                    }

                    dbHandle.InsertPharmacy(connection, pharmacy);
                }

                connection.Close();
            }

            // Text For Progress Status Updates
            Console.Write("\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b");
            Console.Write(" ~ {0} Total Pharmacies in file", pharmaData.Count - 1);

            return dbHandle.GetAllPharmacies();
        }
    }
}
