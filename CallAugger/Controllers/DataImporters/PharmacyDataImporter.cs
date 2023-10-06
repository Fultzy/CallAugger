using CallAugger.Utilities;
using System;
using System.Collections.Generic;
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

            if (PharmacyFiles.Count() == 0)
            {
                Console.WriteLine($"TempError: no files in the Pharmacy Info folder.");
                Console.Write("\nPress the AnyKey to Close the program...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            ///////////////////////////////////////////////
            // Read Each File in the CallRecords Directory
            foreach (string currentFile in PharmacyFiles)
            {
                // get full file path
                string fileName = currentFile.Substring(pharmacyDirPath.Length);
                var filePath = pharmacyDirPath + fileName;


                if (fileName.Contains(".csv") || fileName.Contains(".xlsx"))
                {
                    // a Whole lotta bullshit
                    var xReader = new ExcelReader(filePath);
                    
                    List<List<string>> xReaderData = xReader.ReadData();

                    xReader.CloseReader();

                    try
                    {
                        // add all rows from xreaderdata to data
                        foreach (var row in xReaderData)
                        {
                            data.Add(row);
                        }
                    }
                    catch (Exception e) when (xReaderData == null)
                    {
                        throw new Exception(
                            $"Error in PharmacyDataImporter:ReadInPharmacyData() ~ xReaderData is null.\n {e.Message}\n");
                    }
                }
                else
                {
                    throw new Exception($"Error: {filePath} is not a .csv or .xlsx file.");
                }
            }
            
            return data;
        }


        // Create a list of pharmacies from the workbooks
        public List<Pharmacy> CreatePharmacyRecords(List<List<string>> pharmaData)
        {
            // Create Data store
            List<Pharmacy> pharmacies = new List<Pharmacy>();


            // the first row in call data contains the headers.
            // create a dictionary of headers(string key) and their index(value)
            var headers = new Dictionary<string, int>();
            var headerRow = pharmaData[0];
            for (int i = 0; i < headerRow.Count; i++)
            {
                headers.Add(headerRow[i], i);
            }


            for (int rowNum = 1; rowNum < pharmaData.Count; rowNum++)
            {

                ///////////////////////////////////////////////
                // Create a Pharmacy object
                var pharmacy = new Pharmacy();
                {
                    pharmacy.Name = pharmaData[rowNum][headers["Pharmacy Name"]];
                    pharmacy.ContactName = pharmaData[rowNum][headers["Contact 1 First Name"]] + " " + pharmaData[rowNum][headers["Contact 1 Last Name"]];
                    pharmacy.Npi = pharmaData[rowNum][headers["NPI #"]];
                    pharmacy.Dea = pharmaData[rowNum][headers["DEA #"]];
                    pharmacy.Ncpdp = pharmaData[rowNum][headers["NCPDP #"]];
                    pharmacy.Address = pharmaData[rowNum][headers["Address"]];
                    pharmacy.City = pharmaData[rowNum][headers["City"]];
                    pharmacy.State = pharmaData[rowNum][headers["State"]];
                    pharmacy.Zip = pharmaData[rowNum][headers["Zip"]];
                    pharmacy.PrimaryPhoneNumber = pharmaData[rowNum][headers["Phone # (no dashes)"]];
                }

               // pharmacy.WriteInlinePharmacyInfo();
                pharmacies.Add(pharmacy);

                /*
                // Loop through the rows of this sheet using parallelism

                object _lock = new object();
                Parallel.For(2, maxRows + 1, row =>
                {

                   
                    lock (_lock)
                    {
                        // Update Progress bar
                        int ProgressPercentage = (int)((completedRows * 100) / maxRows);
                        ProgressBarUtility.WriteProgressBar(ProgressPercentage, true);
                    }

                });
                */
            }

            Console.Write(" ~ {0} Total Pharmacies", pharmacies.Count);
            return pharmacies;
        }
    }
}
