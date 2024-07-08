using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using CallAugger.Utilities.Sqlite;
using CallAugger.Settings;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CallAugger.Utilities.Sqlite
{

    public class SQLiteHandler
    {
        
        ///////////////////////////////////////////////////////////////
        // This class is responsible for handling all SQLite database
        // interactions. It is responsible for creating the database
        // and tables, inserting, retrieving, and deleting records

        public readonly string ConnectionString = LoadConnectionString();
        public readonly string dbPath = ConfigurationManager.AppSettings["db_path"];


        // Create Operation Repositories
        public PhoneNumberRepository PhoneNumberRepo = PhoneNumberRepository.Instance;
        public CallRecordRepository CallRecordRepo = CallRecordRepository.Instance;
        public PharmacyRepository PharmacyRepo = PharmacyRepository.Instance;
        public UserRepository UserRepo = UserRepository.Instance;

        // Instance property
        private static readonly object lockObject = new object();
        private static SQLiteHandler instance;

        public static SQLiteHandler Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new SQLiteHandler();
                    }
                    return instance;
                }
            }
        }
        
        private SQLiteHandler()
        {
            Console.WriteLine(Logger.Database("Loading db..."));

            // check if the db file exists and create it if it doesn't
            if (!File.Exists(dbPath))
            {
                Logger.Database("No db file found, creating new db file..");
                SQLiteConnection.CreateFile(dbPath);
            }

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
                
                SQLiteCommand command = new SQLiteCommand(QueryStore.CreateCallRecordsTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.CreateUsersTable , connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.CreatePharmaciesTable, connection);
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.CreatePhoneNumbersTable, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }

            Logger.Database("Tables Checked/Created Successfully!");
        }


        // Drop Tables
        public void DropTables()
        {
            Logger.Database("Dropping Tables!!");

            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(QueryStore.DropCallRecordsTable, connection);
                CallRecordRepo.CallRecords.Clear();
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.DropUsersTable, connection);
                UserRepo.Users.Clear();
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.DropPharmaciesTable, connection);
                PharmacyRepo.Pharmacies.Clear();
                command.ExecuteNonQuery();

                command = new SQLiteCommand(QueryStore.DropPhoneNumbersTable, connection);
                PhoneNumberRepo.PhoneNumbers.Clear();
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        
        // Check Tables
        public bool CheckReady()
        {
            // if the db is empty inform the user
            if (GetAllPharmacies().Count() == 0 && GetAllCallRecords().Count() == 0)
            {
                var nodbMenu = new CliMenu();
                nodbMenu.WriteProgramTitle();

                Console.WriteLine("\n\n !! EMPTY DATABASE !! CHECK THE README FOR IMPORTING INSTRUCTIONS\n");

                nodbMenu.AnyKey();
                return false;
            }
            else if (ConfigurationManager.AppSettings["time_to_access"] == "true")
            {
                RaceToAccess();
            }

            Logger.Database("db Ready!");
            return true;
        }


        public void RaceToAccess()
        {
            Logger.Database("Time to Access:");

            var menu = new CliMenu()
            {
                OptionPadding = 3,
                MenuName = "Time To Access",
            };

            menu.WriteMenuName();
            var dbStartTime = DateTime.Now;

            PharmacyRepo.TimeToAccess();
            CallRecordRepo.TimeToAccess();
            PhoneNumberRepo.TimeToAccess();
            UserRepo.TimeToAccess();

            var dbEndTime = DateTime.Now - dbStartTime;
            menu.WriteMessage(Logger.Database($"~ Total db Time: {Math.Round(dbEndTime.TotalSeconds, 2)} seconds..\n"));

            menu.AnyKey();

        }

        
        public void UpdateRepositories()
        {
            CallRecordRepo.All();
            UserRepo.All();
            PhoneNumberRepo.All();
            PharmacyRepo.All();
        }

       
        ///////////////////////// Repo Methods /////////////////////////
        // Insert Records
        public CallRecord InsertCallRecord(SQLiteConnection connection, CallRecord callRecord)
        {
            return CallRecordRepo.Insert(connection, callRecord);
        }

        public Pharmacy InsertPharmacy(SQLiteConnection connection, Pharmacy pharmacy)
        {
            return PharmacyRepo.Insert(connection, pharmacy);
        }

        public User InsertUser(SQLiteConnection connection, User user)
        {
           return UserRepo.Insert(connection, user);
        }

        public PhoneNumber InsertPhoneNumber(SQLiteConnection connection, PhoneNumber phoneNumber)
        {
            return PhoneNumberRepo.Insert(connection, phoneNumber);
        }


        // List Record Getters
        public List<CallRecord> GetAllCallRecords()
        {
            return CallRecordRepo.All();
        }

        public List<User> GetAllUsers()
        {
            return UserRepo.All();
        }

        public List<Pharmacy> GetAllPharmacies()
        {
            return PharmacyRepo.All();
        }

        public List<PhoneNumber> GetAllPhoneNumbers()
        {
            return PhoneNumberRepo.All();
        }
        
        public List<PhoneNumber> GetUnassignedPhoneNumbers()
        {
            return PhoneNumberRepo.GetUnassigned();
        }

        public List<PhoneNumber> GetAssignedPhoneNumbers()
        {
            return PhoneNumberRepo.GetAssigned();

        }

        // Special Getters
        public List<PhoneNumber> GetPhoneNumbersForPharmacy(SQLiteConnection connection, int pharmacyID)
        {
            return PhoneNumberRepo.ByPharmacy(connection, pharmacyID);
        }

        public List<CallRecord> GetCallRecordsForPhoneNumber(SQLiteConnection connection, int phoneNumberID)
        {
            return CallRecordRepo.ByPhoneNumber(connection, phoneNumberID);
        }
        public List<CallRecord> GetCallRecordsForUser(SQLiteConnection connection, int userID)
        {
            return CallRecordRepo.ByUser(connection, userID);
        }


        // Update Records
        internal void UpdateCallRecordIDs(SQLiteConnection connection, CallRecord call, int phoneNumberID, int userID)
        {
            CallRecordRepo.UpdateCallIDs(connection, call, phoneNumberID, userID);
        }

        public void UpdateCallRecordUserID(SQLiteConnection connection, CallRecord call, int userID)
        {
            CallRecordRepo.UpdateUserID(connection, call, userID);
        }

        internal void UpdatePhoneNumberPharmacyID(SQLiteConnection connection, PhoneNumber phoneNumber, int pharmacyID)
        {
            PhoneNumberRepo.UpdatePharmacyID(connection, phoneNumber, pharmacyID);
        }

        internal void UpdatePhoneNumberPharmacyID(PhoneNumber phoneNumber, int pharmacyID)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                PhoneNumberRepo.UpdatePharmacyID(connection, phoneNumber, pharmacyID);

                connection.Close();
            }

        }
    }
}
