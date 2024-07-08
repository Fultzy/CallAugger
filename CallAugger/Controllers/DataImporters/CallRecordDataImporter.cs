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
    internal class CallRecordDataImporter : DataImporter
    {
        ///////////////////////////////////////////////////////////////
        //  This class is responsible for reading in call record data
        //  from a file and creating a list of CallRecord objects. 

        public Dictionary<string, int> Headers = null;

        // open and read in CallRecord data into a list of lists of strings
        public List<List<string>> ReadInCallRecordData()
        {
            var callData = new List<List<string>>();

            // get the path to the call records
            string callDirPath = Directory.GetCurrentDirectory() + @"\Data\Call Records";
            var callRecordFiles = Directory.EnumerateFiles(callDirPath);

            if (callRecordFiles.Count() == 0) return null;

            ///////////////////////////////////////////////
            // Read Each File in the CallRecords Directory
            foreach (string filePath in callRecordFiles)
            {
                var newData = ProcessFile(filePath);
                callData.AddRange(newData);
            }

            return callData;
        }


        private List<List<string>> ProcessFile(string filePath)
        {
            string fileName = filePath.Substring(filePath.LastIndexOf(@"\") + 1);

            Console.WriteLine($"\nImporting CallRecord File : {fileName}...");
            Logger.Importing($"Importing CallRecord File : {fileName}...");

            if (fileName.Contains(".csv") || fileName.Contains(".xlsx"))
            {
                // A whole lotta Bullshit
                var excelReader = new ExcelReader(filePath);
                var rawData = excelReader.ReadData();

                // Standarize Data Formatting
                var thisHeader = HeaderHandler.GetHeaderFromList(rawData[0]);
                if (Headers == null && HeaderValidator.IsValidNextivaHeader(thisHeader))
                {
                    Headers = thisHeader;
                }

                // Sort the data to match the Global header
                var sortedData = HeaderHandler.SortDataToHeader(rawData, Headers);

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


        public List<CallRecord> ImportCallData(List<List<string>> callData, SQLiteHandler dbHandle)
        {
            var startTime = DateTime.Now;
            int max = callData.Count - 1;
            ProgressBarUtility.WriteProgressBar(0);
            
            List<CallRecord> callRecords = new List<CallRecord>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                for (int rowNum = 1; rowNum < max + 1; rowNum++)
                {
                    var row = callData[rowNum];

                    CallRecord newCallRecord = CreateCallRecord(row);
                    PhoneNumber newCaller = CreatePhoneNumber(connection, newCallRecord);
                    User newUser = CreateUser(connection, newCallRecord);

                    newUser = dbHandle.InsertUser(connection, newUser);
                    if (PhoneNumberValidator.IsPhoneNumber(newCaller.Number))
                    {
                        newCaller = dbHandle.InsertPhoneNumber(connection, newCaller);
                    }

                    newCallRecord.PhoneNumberID = newCaller.id;
                    newCallRecord.UserID = newUser.id;


                    // CallRecord is added to db here, no duplicates are saved
                    // we need to have inserted the user and caller before we 
                    // can insert the call record because of foreign key constraints
                    newCallRecord = dbHandle.InsertCallRecord(connection, newCallRecord);
                    callRecords.Add(newCallRecord);

                    Logger.Importing($"Successfully Added CallRecord : {newCallRecord.InlineDetails()}");

                    ProgressBarUtility.WriteProgressBar(rowNum * 100 / max, true);
                }

                connection.Close();
            }

            var endTime = DateTime.Now;
            var totalTime = endTime - startTime;
            var roundedSeconds = Math.Round(totalTime.TotalSeconds, 2);

            ProgressBarUtility.WriteProgressBar(100, true);
            Console.Write($" ~ {max} Call Records - Took {roundedSeconds}s\n");
            return callRecords;
        }

        private User CreateUser(SQLiteConnection connection, CallRecord callRecord)
        {
            try
            {
                User user = new User()
                {
                    Name = callRecord.UserName,
                    Extention = callRecord.UserExtention
                };

                return user;
            }
            catch 
            {
                var message1 = $"!!ERROR!! : Could not create User from CallRecord : {callRecord.InlineDetails()}";
                throw new Exception(Logger.Error(Logger.Importing(message1)));
            }
        }

        private PhoneNumber CreatePhoneNumber(SQLiteConnection connection, CallRecord callRecord)
        {
            try
            {
                PhoneNumber caller = new PhoneNumber()
                {
                    Number = callRecord.Caller,
                    State = callRecord.State
                };
                
                return caller;
            }
            catch
            {
                var message1 = $"!!ERROR!! : Could not create PhoneNumber from CallRecord : {callRecord.InlineDetails()}";
                throw new Exception(Logger.Error(Logger.Importing(message1)));
            }
        }


        // Create a CallRecord object from a row of data
        private CallRecord CreateCallRecord(List<string> row)
        {

            try
            {
                ///////////////////////////////////////////////
                // Create a Call Record object
                var callRecord = new CallRecord
                {
                    // Store basic information
                    CallType = row[Headers["Call Type"]],
                    UserName = row[Headers["User Name"]],
                    Duration = Convert.ToInt32(row[Headers["Duration"]]),
                    Time = DateTime.Parse(row[Headers["Time"]]),
                };


                // remove "From" or "To" from the state
                var state = row[Headers["State"]];
                if (state != "null")
                    callRecord.State = state.Split(' ')[1];
                else
                    callRecord.State = "null";


                // Transfer User is some times null, if Missing apply "Outbound"
                if (row[Headers["Transfer User"]] == null)
                    callRecord.TransferUser = "Outbound";
                else
                    callRecord.TransferUser = row[Headers["Transfer User"]];


                // these are the phone numbers used during this call
                var from = row[Headers["From"]];
                var to = row[Headers["To"]];

                // if the number is longer than 10 digits, remove the first digit
                if (from.Length > 10)
                    from = from.Substring(1);

                if (to.Length > 10)
                    to = to.Substring(1);


                // determine the caller by the call type and set correct caller info
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

                return callRecord;
            }
            catch (Exception e)
            {
                var message1 = $" Could not create CallRecord from row : ";
                var message2 = $" Row Values : {string.Join(", ", row)}";

                ErrorMenu.For(message1, message2, e.Message);

                throw new Exception(Logger.Error(Logger.Importing(message1 + message2 + e.Message)));
            }
            
        }
    }
}