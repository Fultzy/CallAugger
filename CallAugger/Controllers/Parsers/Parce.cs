using CallAugger.Utilities;
using CallAugger.Utilities.DataBase;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Parsers
{
    internal class Parse
    {

        public void ParseCallRecordData(List<CallRecord> callRecords)
        {
            var IParser = new ParseCallRecordData();
            var dbHandle = new SQLiteHandler();

            Console.WriteLine("Processing New Data...");

            // begin progress bar DEBUGGING w/TIMER
            var startTime = DateTime.Now;
            int progress = 0;
            ProgressBarUtility.WriteProgressBar(0);


            // Take Snapshot of Current db
            var phoneNumbers = dbHandle.GetAllPhoneNumbers();
            var users        = dbHandle.GetAllUsers();


            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                foreach (var callRecord in callRecords)
                {

                    // these two functions will return either a new object or
                    // the existing object from the db
                    PhoneNumber caller = IParser.ParseCallRecordCaller
                        (callRecord, phoneNumbers);

                    User user = IParser.ParseCallRecordUser
                        (callRecord, users);


                    if (PhoneNumberValidator.IsPhoneNumber(callRecord.Caller)) 
                    {
                        // if this phone number is not already in the db add it
                        if (!phoneNumbers.Any(pn => pn.Number == caller.Number))
                        {
                            caller = dbHandle.InsertPhoneNumber(connection, caller);
                            phoneNumbers.Add(caller);
                        }

                        // if this user is not already in the db add it
                        if (!users.Any(u => u.Name == user.Name))
                        {
                           user = dbHandle.InsertUser(connection, user);
                           users.Add(user);
                        }

                        var thisCall = dbHandle.GetCallRecord(connection, callRecord.id);

                        // assign the PhoneNumberID and UserID to the callRecord
                        if (thisCall.PhoneNumberID == 0 || thisCall.UserID == 0)
                        {
                            dbHandle.UpdateCallRecordIDs
                                (connection, callRecord, caller.id, user.id);
                        }

                    }
                    else if (caller.Number.Length == 3) // internal call 
                    {
                        //Console.WriteLine($"DEBUGGING: {callRecord.TransferUser} -> {user.Name}");

                        // only create the user if it doesnt already exist
                        if (!users.Any(u => u.Name == user.Name))
                        {
                            user = dbHandle.InsertUser(connection, user);
                            users.Add(user);
                        }


                        // assign the PhoneNumberID and UserID to the callRecord
                        if (callRecord.PhoneNumberID == 0 || callRecord.UserID == 0)
                        {
                            dbHandle.UpdateCallRecordIDs
                                (connection, callRecord, caller.id, user.id);
                        }
                    }

                    /*
                    // if the transfer user isnt Tech Support, Weekend Support or null
                    if (callRecord.TransferUser != "Tech Support" && callRecord.TransferUser != "Weekend Support" && callRecord.TransferUser != null)
                    {
                        // if this transfer user is not already in the list add it
                        if (!ParsedUsers.Any(u => u.Name == callRecord.TransferUser))
                        {
                            // the issues here is that the transfer user can be a new user and can have been forwarded a 
                            // inbound call such as : 

                            // Inbound call |Joshua Bowman|Matthew Doherty|7/5/2023|1424|Terminating|Yes|From|MI|18102157771|336|

                            // we cant assign this transfer user to the call record because we dont know the second users extention

                            // curently we are adding the call times to both the transfer user and the user that answered. 
                            // this is not fair but also there is no way of knowing at which point the call had split. 

                        
                            User transferUser = new User()
                            {
                                Name = callRecord.TransferUser
                            };

                            transferUser.AddCall(callRecord);
                            ParsedUsers.Add(transferUser);
                        }
                        else // find the transfer user and add this call record to it
                        {
                            ParsedUsers.Find(u => u.Name == callRecord.TransferUser).AddCall(callRecord);
                        }
                    }
                    */

                    // Update Progress bar
                    progress++;
                    ProgressBarUtility.WriteProgressBar((int)((progress * 100) / callRecords.Count()), true);
                }

                connection.Close();
            }

            // write how long it took to complete these tasks
            var endTime = DateTime.Now;
            var timeSpan = endTime - startTime;
            Console.WriteLine($" ~ Took {Math.Round(timeSpan.TotalSeconds, 2)} seconds..\n");
        }


        public void ParsePharmacyData()
        {
            var IParser = new ParsePharmacyData();
            var dbHandle = new SQLiteHandler();

            // we want to see all the pharmacies that are in the db, not only new ones
            List<Pharmacy> pharmacies = dbHandle.GetAllPharmacies();
            List<PhoneNumber> phoneNumbers = dbHandle.GetAllPhoneNumbers();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                foreach (PhoneNumber phoneNumber in phoneNumbers)
                {
                    if (IParser.IsRegisteredToPharmacy(phoneNumber, pharmacies))
                    {
                        // find the pharmacy that matches this phone number
                        var matchingPharmacy = pharmacies.Find(ph => ph.PhoneNumbers.Contains(phoneNumber));

                        phoneNumber.PharmacyID = matchingPharmacy.id;

                        dbHandle.UpdatePhoneNumberPharmacyID(connection, phoneNumber, matchingPharmacy.id);

                    }
                    else if (IParser.IsPrimaryPhoneNumber(phoneNumber, pharmacies))
                    {
                        // find the pharmacy that matches this phone number
                        var matchingPharmacy = pharmacies.Find(ph => ph.PrimaryPhoneNumber == phoneNumber.Number);

                        phoneNumber.PharmacyID = matchingPharmacy.id;

                        dbHandle.UpdatePhoneNumberPharmacyID(connection, phoneNumber, matchingPharmacy.id);
                    }
                }

                // get all pharmacies that dont have a primary phone number

                connection.Close();
            }
        }
    }
}
