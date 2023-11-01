using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using CallAugger.Utilities.Sqlite;
using CallAugger.Settings;
using System.Runtime.InteropServices;
using System.Linq;

namespace CallAugger.Utilities.DataBase
{

    public class SQLiteHandler
    {
        ///////////////////////////////////////////////////////////////
        // This class is responsible for handling all SQLite database
        // interactions. It is responsible for creating the database
        // and tables, inserting, retrieving, and deleting records

        public static readonly string ConnectionString = LoadConnectionString();
       
        public SQLiteHandler()
        {
            CreateTables();
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }


        // Create Tables
        public void CreateTables()
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                
                SQLiteCommand command = new SQLiteCommand(
                    QueryStore.CreateCallRecordsTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                    QueryStore.CreateUsersTable , connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                    QueryStore.CreatePharmaciesTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                    QueryStore.CreatePhoneNumbersTable, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }


        // Drop Tables
        public void DropTables()
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(
                                       QueryStore.DropCallRecordsTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                                       QueryStore.DropUsersTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                                       QueryStore.DropPharmaciesTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(
                                       QueryStore.DropPhoneNumbersTable, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

       
        // Insert Records
        // passing the connection to these method vastly decreases loading times

        public void InsertCallRecord(SQLiteConnection connection, CallRecord callRecord)
        {
            if (IsDuplicateCallRecord(connection, callRecord) == false)
            {
                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.InsertCallRecord, connection);
                
                command.Parameters.AddWithValue("@CallType", callRecord.CallType);
                command.Parameters.AddWithValue("@Duration", callRecord.Duration);
                command.Parameters.AddWithValue("@Time", callRecord.Time);
                command.Parameters.AddWithValue("@UserName", callRecord.UserName);
                command.Parameters.AddWithValue("@UserExtention", callRecord.UserExtention);
                command.Parameters.AddWithValue("@TransferUser", callRecord.TransferUser);
                command.Parameters.AddWithValue("@Caller", callRecord.Caller);
                command.Parameters.AddWithValue("@PhoneNumberID", callRecord.PhoneNumberID);
                command.Parameters.AddWithValue("@UserID", callRecord.UserID);


                command.ExecuteNonQuery();
            }
        }

        public void InsertPharmacy(SQLiteConnection connection, Pharmacy pharmacy)
        {
            if (IsDuplicatePharmacy(connection, pharmacy) == false)
            {
                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.InsertPharmacy, connection);

                command.Parameters.AddWithValue("@Name", pharmacy.Name);
                command.Parameters.AddWithValue("@Npi", pharmacy.Npi);
                command.Parameters.AddWithValue("@Dea", pharmacy.Dea);
                command.Parameters.AddWithValue("@Ncpdp", pharmacy.Ncpdp);
                command.Parameters.AddWithValue("@Address", pharmacy.Address);
                command.Parameters.AddWithValue("@City", pharmacy.City);
                command.Parameters.AddWithValue("@State", pharmacy.State);
                command.Parameters.AddWithValue("@Zip", pharmacy.Zip);
                command.Parameters.AddWithValue("@ContactName1", pharmacy.ContactName1);
                command.Parameters.AddWithValue("@ContactName2", pharmacy.ContactName2);
                command.Parameters.AddWithValue("@Anniversary", pharmacy.Anniversary);
                command.Parameters.AddWithValue("@PrimaryPhoneNumber", pharmacy.PrimaryPhoneNumber);

                command.ExecuteNonQuery();
            }


        }

        public User InsertUser(SQLiteConnection connection, User user)
        {
            User insertedUser = null;
            if (IsDuplicateUser(connection, user) == false)
            {
                SQLiteCommand command = new SQLiteCommand(
                    QueryStore.InsertUser, connection);

                command.Parameters.AddWithValue("@UserName", user.Name);
                command.Parameters.AddWithValue("@UserExtention", user.Extention);

                command.ExecuteNonQuery();

                command.CommandText = "SELECT last_insert_rowid()";
                object id = command.ExecuteScalar();
                insertedUser = user;
                insertedUser.id = Convert.ToInt32(id);
            }

            return insertedUser;
        }

        public PhoneNumber InsertPhoneNumber(SQLiteConnection connection, PhoneNumber phoneNumber)
        {
            PhoneNumber insertedPhoneNumber = null;
            if (IsDuplicatePhoneNumber(connection, phoneNumber) == false)
            {
                
                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.InsertPhoneNumber, connection);

                command.Parameters.AddWithValue("@Number", phoneNumber.Number);
                command.Parameters.AddWithValue("@PharmacyID", phoneNumber.PharmacyID);

                command.ExecuteNonQuery();

                command.CommandText = "SELECT last_insert_rowid()";
                object id = command.ExecuteScalar();
                insertedPhoneNumber = phoneNumber;
                insertedPhoneNumber.id = Convert.ToInt32(id);
            }

            return insertedPhoneNumber;
        }




        // List Record Getters
        public List<CallRecord> GetAllCallRecords()
        {
            DateRange dateRange = new DateRange();
            List<CallRecord> callRecords = new List<CallRecord>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(
                    QueryStore.SelectAllCallRecords, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (!dateRange.IsInRange(Convert.ToDateTime(reader["Time"]))) continue;

                    CallRecord callRecord = new CallRecord();

                    callRecord.id            = Convert.ToInt32(reader["id"]);
                    callRecord.UserID        = Convert.ToInt32(reader["UserID"]);
                    callRecord.PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]);
                    callRecord.CallType      = reader["CallType"].ToString();
                    callRecord.Duration      = Convert.ToInt32(reader["Duration"]);
                    callRecord.Time          = Convert.ToDateTime(reader["Time"]);
                    callRecord.UserName      = reader["UserName"].ToString();
                    callRecord.UserExtention = reader["UserExtention"].ToString();
                    callRecord.TransferUser  = reader["TransferUser"].ToString();
                    callRecord.Caller        = reader["Caller"].ToString();

                    callRecords.Add(callRecord);
                }

                connection.Close();
            }

            return callRecords;
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                var command = new SQLiteCommand
                    (QueryStore.SelectAllUsers, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user = new User();

                    user.id = Convert.ToInt32(reader["id"]);
                    user.Name = reader["UserName"].ToString();
                    user.Extention = reader["UserExtention"].ToString();

                    user.AddCalls(GetCallRecordsForUser(connection, user.id));
                    users.Add(user);
                }

                connection.Close();
            }

            
            return users;
        }

        public List<Pharmacy> GetAllPharmacies()
        {
            List<Pharmacy> pharmacies = new List<Pharmacy>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(
                    QueryStore.SelectAllPharmacies, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Pharmacy pharmacy = new Pharmacy();

                    pharmacy.id      = Convert.ToInt32(reader["id"]);
                    pharmacy.Name    = reader["Name"].ToString();
                    pharmacy.Npi     = reader["Npi"].ToString();
                    pharmacy.Dea     = reader["Dea"].ToString();
                    pharmacy.Ncpdp   = reader["Ncpdp"].ToString();
                    pharmacy.Address = reader["Address"].ToString();
                    pharmacy.City    = reader["City"].ToString();
                    pharmacy.State   = reader["State"].ToString();
                    pharmacy.Zip     = reader["Zip"].ToString();
                    pharmacy.ContactName1 = reader["ContactName1"].ToString();
                    pharmacy.ContactName2 = reader["ContactName2"].ToString();
                    pharmacy.Anniversary  = reader["Anniversary"].ToString();
                    pharmacy.PrimaryPhoneNumber = reader["PrimaryPhoneNumber"].ToString();

                    if (pharmacy.PrimaryPhoneNumber != null)
                    {
                        pharmacy.AddPhoneNumber(FindPhoneNumber(connection, pharmacy.PrimaryPhoneNumber));
                    }
                    pharmacy.AddPhoneNumbers(GetPhoneNumbersForPharmacy(connection, pharmacy.id));

                    pharmacies.Add(pharmacy);
                }

                connection.Close();
            }

            return pharmacies.OrderByDescending(pharmacy => pharmacy.TotalDuration).ToList();
        }

        public List<PhoneNumber> GetAllPhoneNumbers()
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.SelectAllPhoneNumbers, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhoneNumber phoneNumber = new PhoneNumber();

                    phoneNumber.id = Convert.ToInt32(reader["id"]);
                    phoneNumber.Number = reader["Number"].ToString();
                    phoneNumber.PharmacyID = Convert.ToInt32(reader["PharmacyID"]);

                    phoneNumber.AddCalls(GetCallRecordsForPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }

                connection.Close();
            }

            return phoneNumbers.OrderByDescending(number => number.TotalDuration).ToList();
        }
        
        public List<PhoneNumber> GetUnassignedPhoneNumbers()
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.SelectUnassignedPhoneNumbers, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    PhoneNumber phoneNumber = new PhoneNumber();
                    phoneNumber.id = Convert.ToInt32(reader["id"]);
                    phoneNumber.Number = reader["Number"].ToString();
                    phoneNumber.PharmacyID = Convert.ToInt32(reader["PharmacyID"]);

                    phoneNumber.AddCalls(GetCallRecordsForPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }

                connection.Close();
            }

            return phoneNumbers.OrderByDescending(number => number.TotalDuration).ToList();
        }


        // Special Getters
        private PhoneNumber FindPhoneNumber(SQLiteConnection connection, string primaryPhoneNumber)
        {
            PhoneNumber phoneNumber = new PhoneNumber();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE Number = @Number", connection);

            command.Parameters.AddWithValue("@Number", primaryPhoneNumber);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                phoneNumber.id = Convert.ToInt32(reader["id"]);
                phoneNumber.Number = reader["Number"].ToString();
                phoneNumber.PharmacyID = Convert.ToInt32(reader["PharmacyID"]);


                phoneNumber.AddCalls(GetCallRecordsForPhoneNumber(connection, phoneNumber.id));
            }

            return phoneNumber;
        }

        public List<PhoneNumber> GetPhoneNumbersForPharmacy(SQLiteConnection connection, int pharmacyID)
        {

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE PharmacyID = @PharmacyID", connection);

            command.Parameters.AddWithValue("@PharmacyID", pharmacyID);


            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    PhoneNumber phoneNumber = new PhoneNumber();

                    phoneNumber.id = Convert.ToInt32(reader["id"]);
                    phoneNumber.Number = reader["Number"].ToString();
                    phoneNumber.PharmacyID = Convert.ToInt32(reader["PharmacyID"]);

                    phoneNumber.AddCalls(GetCallRecordsForPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }
            }

            return phoneNumbers;
        }

        public List<CallRecord> GetCallRecordsForPhoneNumber(SQLiteConnection connection, int phonenumberID)
        {
            DateRange dateRange = new DateRange().GetCurrentDateRange();
            List<CallRecord> callRecords = new List<CallRecord>();

            using (SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllCallRecords + " WHERE PhoneNumberID = @PhoneNumberID", connection))
            {
                command.Parameters.AddWithValue("@PhoneNumberID", phonenumberID);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CallRecord callRecord = new CallRecord();
                        callRecord.Time = Convert.ToDateTime(reader["Time"]);

                        if (dateRange.IsInRange(callRecord.Time) == false) continue;

                        callRecord.id = Convert.ToInt32(reader["id"]);
                        callRecord.CallType = reader["CallType"].ToString();
                        callRecord.UserID = Convert.ToInt32(reader["UserID"]);
                        callRecord.PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]);
                        callRecord.Duration = Convert.ToInt32(reader["Duration"]);
                        callRecord.UserName = reader["UserName"].ToString();
                        callRecord.UserExtention = reader["UserExtention"].ToString();
                        callRecord.TransferUser = reader["TransferUser"].ToString();
                        callRecord.Caller = reader["Caller"].ToString();

                        callRecords.Add(callRecord);
                    }
                }
            }

            return callRecords;
        }

        public List<CallRecord> GetCallRecordsForUser(SQLiteConnection connection, int userID)
        {
            DateRange dateRange = new DateRange();
            List<CallRecord> callRecords = new List<CallRecord>();
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllCallRecords + " WHERE UserID = @UserID", connection))
            {
                command.Parameters.AddWithValue("@UserID", userID);

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // if the call is not in the current date range skip it
                        if (!dateRange.IsInRange(Convert.ToDateTime(reader["Time"]))) continue;

                        CallRecord callRecord = new CallRecord();

                        callRecord.id = Convert.ToInt32(reader["id"]);
                        callRecord.UserID = Convert.ToInt32(reader["UserID"]);
                        callRecord.PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]);
                        callRecord.CallType = reader["CallType"].ToString();
                        callRecord.Duration = Convert.ToInt32(reader["Duration"]);
                        callRecord.Time = Convert.ToDateTime(reader["Time"]);
                        callRecord.UserName = reader["UserName"].ToString();
                        callRecord.UserExtention = reader["UserExtention"].ToString();
                        callRecord.TransferUser = reader["TransferUser"].ToString();
                        callRecord.Caller = reader["Caller"].ToString();

                        callRecords.Add(callRecord);
                    }
                }
            }

            return callRecords;
        }



        // Single Record Getters
        public CallRecord GetCallRecord(SQLiteConnection connection, int callRecordID)
        {
            CallRecord callRecord = new CallRecord();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllCallRecords + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", callRecordID);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                callRecord.id = Convert.ToInt32(reader["id"]);
                callRecord.UserID = Convert.ToInt32(reader["UserID"]);
                callRecord.PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]);
                callRecord.CallType = reader["CallType"].ToString();
                callRecord.Duration = Convert.ToInt32(reader["Duration"]);
                callRecord.Time = Convert.ToDateTime(reader["Time"]);
                callRecord.UserName = reader["UserName"].ToString();
                callRecord.UserExtention = reader["UserExtention"].ToString();
                callRecord.TransferUser = reader["TransferUser"].ToString();
                callRecord.Caller = reader["Caller"].ToString();
            }

            return callRecord;
        }

        public User GetUser(SQLiteConnection connection, int userID)
        {
            User user = new User();
            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllUsers + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", userID);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                user.id = Convert.ToInt32(reader["id"]);
                user.Name = reader["UserName"].ToString();
                user.Extention = reader["UserExtention"].ToString();

                user.AddCalls(GetCallRecordsForUser(connection, user.id));
            }


            return user;
        }

        public Pharmacy GetPharmacy(SQLiteConnection connection, int pharmacyID)
        {
            Pharmacy pharmacy = new Pharmacy();
            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPharmacies + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", pharmacyID);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy.id = Convert.ToInt32(reader["id"]);
                pharmacy.Name = reader["Name"].ToString();
                pharmacy.Npi = reader["Npi"].ToString();
                pharmacy.Dea = reader["Dea"].ToString();
                pharmacy.Ncpdp = reader["Ncpdp"].ToString();
                pharmacy.Address = reader["Address"].ToString();
                pharmacy.City = reader["City"].ToString();
                pharmacy.State = reader["State"].ToString();
                pharmacy.Zip = reader["Zip"].ToString();
                pharmacy.ContactName1 = reader["ContactName1"].ToString();
                pharmacy.ContactName2 = reader["ContactName2"].ToString();
                pharmacy.Anniversary = reader["Anniversary"].ToString();
                pharmacy.PrimaryPhoneNumber = reader["PrimaryPhoneNumber"].ToString();
            }
           

            return pharmacy;
        }

        public String GetPharmacyName(int pharmacyID)
        {
            if (pharmacyID == 0)return "";
            string pharmacyName = "";

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.SelectAllPharmacies + " WHERE id = @id", connection);

                command.Parameters.AddWithValue("@id", pharmacyID);

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    pharmacyName = reader["Name"].ToString();
                }

                connection.Close();
            }

            return pharmacyName;
        }

        public PhoneNumber GetPhoneNumber(SQLiteConnection connection, int phoneNumberID)
        {
            PhoneNumber phoneNumber = new PhoneNumber();
            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", phoneNumberID);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                phoneNumber.id = Convert.ToInt32(reader["id"]);
                phoneNumber.Number = reader["Number"].ToString();
                phoneNumber.PharmacyID = Convert.ToInt32(reader["PharmacyID"]);


                phoneNumber.AddCalls(GetCallRecordsForPhoneNumber(connection, phoneNumber.id));
            }

            return phoneNumber;
        }
        


        // Duplicate Entry Checks
        // passing the connection to these methods vastly decreases loading times
        internal bool IsDuplicateCallRecord(SQLiteConnection connection, CallRecord newCallRecord)
        {
            string queryString = "SELECT COUNT(*) FROM CallRecords WHERE Time = @Time AND UserName = @UserName AND Duration = @Duration AND UserExtention = @UserExtention AND TransferUser = @TransferUser AND Caller = @Caller";
            
            using (SQLiteCommand command = new SQLiteCommand
                (queryString, connection))
            {
                command.Parameters.AddWithValue("@Time", newCallRecord.Time);
                command.Parameters.AddWithValue("@Duration", newCallRecord.Duration);
                command.Parameters.AddWithValue("@UserName", newCallRecord.UserName);
                command.Parameters.AddWithValue("@UserExtention", newCallRecord.UserExtention);
                command.Parameters.AddWithValue("@TransferUser", newCallRecord.TransferUser);
                command.Parameters.AddWithValue("@Caller", newCallRecord.Caller);

                var obj = command.ExecuteScalar();


                int i = Convert.ToInt32(obj);
                if (i == 0)
                {
                    return false;
                }
                else
                {   
                    return true;
                }
            }
        }

        internal bool IsDuplicateUser(SQLiteConnection connection, User newUser)
        {
            string queryString = "SELECT COUNT(*) FROM Users WHERE UserName = @UserName AND UserExtention = @UserExtention";

            using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@UserName", newUser.Name);
                command.Parameters.AddWithValue("@UserExtention", newUser.Extention);

                object obj = command.ExecuteScalar();

                int i = Convert.ToInt32(obj);
                if (i == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal bool IsDuplicatePharmacy(SQLiteConnection connection, Pharmacy newPharmacy)
        {
            string queryString = "SELECT COUNT(*) FROM Pharmacies WHERE Npi = @Npi AND Dea = @Dea AND Ncpdp = @Ncpdp";

            using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Npi", newPharmacy.Npi);
                command.Parameters.AddWithValue("@Dea", newPharmacy.Dea);
                command.Parameters.AddWithValue("@Ncpdp", newPharmacy.Ncpdp);
                    
                object obj = command.ExecuteScalar();
                    
                int i = Convert.ToInt32(obj);
                if (i == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        internal bool IsDuplicatePhoneNumber(SQLiteConnection connection, PhoneNumber newPhoneNumber)
        {
            string queryString = "SELECT COUNT(*) FROM PhoneNumbers WHERE Number = @Number";

            using (SQLiteCommand command = new SQLiteCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@Number", newPhoneNumber.Number);
                object obj = command.ExecuteScalar();


                int i = Convert.ToInt32(obj);
                if (i == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }




        // Update Records
        internal void UpdateCallRecordIDs(SQLiteConnection connection, CallRecord call, int phoneNumberID, int userID)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdateCallRecordForeignIDs, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@id", call.id);

                int rowsUpdated = command.ExecuteNonQuery();

                    
                if (rowsUpdated == 0)
                {
                    throw new Exception("No rows were updated");
                }
            }
        }

        internal void UpdatePhoneNumberPharmacyID(SQLiteConnection connection, PhoneNumber phoneNumber, int pharmacyID)
        {
            
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdatePhoneNumberPharmacyID, connection))
            {
                command.Parameters.AddWithValue("@PharmacyID", pharmacyID);
                command.Parameters.AddWithValue("@id", phoneNumber.id);

                command.ExecuteNonQuery();
            }

        }

        public void UpdatePhoneNumberPharmacyID(PhoneNumber phoneNumber, int pharmacyID)
        {
            // remove this phone number from the pharmacy and add it back to the list of unassigned
            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();
                UpdatePhoneNumberPharmacyID(connection, phoneNumber, pharmacyID);
                connection.Close();
            }
        }


        // Delete Record
        internal void DeleteCall(CallRecord call)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand
                    (QueryStore.DeleteCallRecord, connection))
                {
                    command.Parameters.AddWithValue("@id", call.id);

                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

    }
}
