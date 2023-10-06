using CallAugger.Utilities;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CallAugger.Controllers.DataImporters
{
    internal class CallRecordDataImporter : DataImporter
    {
        ///////////////////////////////////////////////////////////////
        //  This class is responsible for reading in call record data
        //  from a file and creating a list of CallRecord objects. 
        //  This class is responsible for reading in call record data


        // open and read in CallRecord data into a list of lists
        public List<List<string>> ReadInCallRecordData(string path)
        {
            // get the path to the call records
            string callDirPath = path + @"\Data\Call Records";


            // get the names of all the files in the call records directory
            var callRecordFiles = Directory.EnumerateFiles(callDirPath);
            var data = new List<List<string>>();

            if (callRecordFiles.Count() == 0)
            {
                Console.WriteLine($"TempError: no files in the Call Records folder.");
                Console.Write("\nPress the AnyKey to Close the program...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            ///////////////////////////////////////////////
            // Read Each File in the CallRecords Directory
            foreach (string currentFile in callRecordFiles)
            {
                // get full file path
                string fileName = currentFile.Substring(callDirPath.Length);
                var filePath = callDirPath + fileName;


                if (fileName.Contains(".csv") || fileName.Contains(".xlsx"))
                {
                    // A whole lotta Bullshit
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
                            $"Error in CallRecordDataImporter:ReadInCallRecordData() ~ xReaderData is null.\n {e.Message}\n");
                    }
                }
                else
                {
                    throw new Exception($"Error: {filePath} is not a .csv or .xlsx file.");
                }
            }

            return data;
        }


        // Create a list of call records from a list of ExcelReaders
        public List<CallRecord> CreateCallRecords(List<List<string>> callData)
        {
            // Create Data Store
            List<CallRecord> callRecords = new List<CallRecord>();
            
            
            // the first row in call data contains the headers.
            // create a dictionary of headers(string key) and their index(value)
            var headers = new Dictionary<string, int>();
            var headerRow = callData[0];
            for (int i = 0; i < headerRow.Count; i++)
            {
                headers.Add(headerRow[i], i);
            }


            for (int rowNum = 1; rowNum < callData.Count; rowNum++)
            {
                ///////////////////////////////////////////////
                // Create a Call Record object
                
                var row = callData[rowNum];
                var callRecord = new CallRecord();
                
                // Store basic information from entire row instead of multiple cell accesses
                callRecord.CallType = row[headers["Call Type"]];
                callRecord.UserName = row[headers["User Name"]];
                callRecord.Duration = Convert.ToInt32(row[headers["Duration"]]);
                
                DateTime time = DateTime.Parse(row[headers["Time"]]);
                DateTime timeStripped = time.Date.AddHours(time.Hour).AddMinutes(time.Minute);
                callRecord.Time = time;


                // Transfer User is some times null, if Missing apply Outbound
                if (row[headers["Transfer User"]] == null)
                    callRecord.TransferUser = "Outbound";
                else
                    callRecord.TransferUser = row[headers["Transfer User"]];

                
                // these are the phone numbers used during this call
                var from = row[headers["From"]];
                var to = row[headers["To"]];

                // if both to and from are only 3 digits long disregard this row
                if (from.Length == 3 && to.Length == 3)
                    continue;

                // if the number is longer than 10 digits, remove the first digit
                if (from.Length > 10)
                    from = from.Substring(1);

                if (to.Length > 10)
                    to = to.Substring(1);


                // determine the caller from the call type and set correct caller info
                switch (callRecord.CallType)
                {
                    case "Inbound call":
                        callRecord.Caller = from;
                        callRecord.UserExtention = to;
                        break;
                    case "Outbound call":
                        callRecord.Caller = to;
                        callRecord.UserExtention = from;
                        break;
                    default:
                        throw new Exception($"Error: {callRecord.CallType} is not a valid call type.");
                }




                callRecords.Add(callRecord);
            }
                 
            Console.Write(" ~ {0} Total Call Records", callRecords.Count);
            return callRecords;
        }
    }
}
