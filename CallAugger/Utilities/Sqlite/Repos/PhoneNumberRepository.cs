using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Configuration;

namespace CallAugger.Utilities.Sqlite
{
    public class PhoneNumberRepository
    {
        // Instance property
        private static readonly object lockObject = new object();
        private static PhoneNumberRepository instance;

        public static PhoneNumberRepository Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new PhoneNumberRepository();
                    }
                    return instance;
                }
            }
        }

        // Cache
        public List<PhoneNumber> PhoneNumbers = new List<PhoneNumber>();

        private PhoneNumberRepository()
        {
            All();
        }


        ///////////// Fetching /////////////
        public List<PhoneNumber> All()
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllPhoneNumbers, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var phoneNumber = CreateFromReader(reader);

                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }

                connection.Close();
            }

            PhoneNumbers = phoneNumbers.OrderByDescending(pn => pn.TotalDuration).ToList();
            return phoneNumbers;
        }

        public List<PhoneNumber> GetUnassigned()
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllPhoneNumbers + " WHERE PharmacyID = 0", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var phoneNumber = CreateFromReader(reader);

                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }

                connection.Close();
            }

            return phoneNumbers.OrderByDescending(pn => pn.TotalDuration).ToList();
        }

        public List<PhoneNumber> GetAssigned()
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllPhoneNumbers + " WHERE PharmacyID != 0", connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var phoneNumber = CreateFromReader(reader);


                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }

                connection.Close();
            }

            return phoneNumbers;
        }

        public List<PhoneNumber> ByState(string state)
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE State = @State", connection);

                command.Parameters.AddWithValue("@State", state);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var phoneNumber = CreateFromReader(reader);

                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }
            }
            return phoneNumbers;
        }
        
        public string TotalUnassignedPhoneTime()
        {
            int totalDuration = 0;

            foreach (PhoneNumber phoneNumber in PhoneNumbers.Where(pn => pn.PharmacyID == 0))
            {
                totalDuration += phoneNumber.TotalDuration;
            }

            TimeSpan totalTimeSpan = TimeSpan.FromSeconds(totalDuration);
            string formattedDuration = $"{(int)totalTimeSpan.TotalHours}h {(int)totalTimeSpan.Minutes}m";

            return formattedDuration;
        }

        public List<PhoneNumber> ByPharmacy(SQLiteConnection connection, int pharmacyID)
        {
            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE PharmacyID = @PharmacyID", connection);

            command.Parameters.AddWithValue("@PharmacyID", pharmacyID);


            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var phoneNumber = CreateFromReader(reader);


                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                    phoneNumbers.Add(phoneNumber);
                }
            }

            return phoneNumbers;
        }

        public PhoneNumber Find(SQLiteConnection connection, string phoneNumberString)
        {
            PhoneNumber phoneNumber = new PhoneNumber();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE Number = @Number", connection);

            command.Parameters.AddWithValue("@Number", phoneNumberString);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    phoneNumber = CreateFromReader(reader);

                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                }
            }

            return phoneNumber;
        }

        public PhoneNumber ID(SQLiteConnection connection, int id)
        {
            PhoneNumber phoneNumber = new PhoneNumber();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPhoneNumbers + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    phoneNumber = CreateFromReader(reader);

                    phoneNumber.AddCalls(CallRecordRepository.Instance.ByPhoneNumber(connection, phoneNumber.id));
                }
            }

            return phoneNumber;
        }


        ///////////// CRUD /////////////
        public PhoneNumber CreateFromReader(SQLiteDataReader reader)
        {
            PhoneNumber phoneNumber = new PhoneNumber
            {
                id = Convert.ToInt32(reader["id"]),
                Number = reader["Number"].ToString(),
                State = reader["State"].ToString(),
                PharmacyID = Convert.ToInt32(reader["PharmacyID"]),
                IsPrimary = Convert.ToBoolean(reader["IsPrimary"]),
                IsFax = Convert.ToBoolean(reader["IsFax"])
            };

            return phoneNumber;
        }

        public PhoneNumber Insert(SQLiteConnection connection, PhoneNumber phoneNumber)
        {
            try
            {
                if (!this.Contains(phoneNumber))
                {
                    SQLiteCommand command = new SQLiteCommand
                        (QueryStore.InsertPhoneNumber + "; SELECT last_insert_rowid()", connection);

                    command.Parameters.AddWithValue("@Number", phoneNumber.Number);
                    command.Parameters.AddWithValue("@State", phoneNumber.State);
                    command.Parameters.AddWithValue("@PharmacyID", +phoneNumber.PharmacyID);
                    command.Parameters.AddWithValue("@IsPrimary", phoneNumber.IsPrimary);
                    command.Parameters.AddWithValue("@IsFax", phoneNumber.IsFax);

                    object id = command.ExecuteScalar();
                    phoneNumber.id = Convert.ToInt32(id);

                    // update cache
                    PhoneNumbers.Add(phoneNumber);

                    Logger.Database($"INSERTED PhoneNumber : {phoneNumber.InlineDetails()}");
                    return phoneNumber;
                }

                return Update(connection, PhoneNumbers.Find(pn => pn.Number == phoneNumber.Number));
            }
            catch (Exception ex)
            {
                string message = ex + $"INSERTING PhoneNumber FAILED : {phoneNumber.InlineDetails()}";
                throw new Exception(Logger.Error(Logger.Database(message)));
            }
        }

        public void UpdatePharmacyID(SQLiteConnection connection, PhoneNumber phoneNumber, int pharmacyID)
        {

            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdatePhoneNumberPharmacyID, connection))
            {
                command.Parameters.AddWithValue("@PharmacyID", pharmacyID);
                command.Parameters.AddWithValue("@id", phoneNumber.id);

                int rowsUpdated = command.ExecuteNonQuery();


                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE PhoneNumber PharmacyID:{pharmacyID} FAILED : {phoneNumber.InlineDetails()}");
                }

                // Update Cached Record
                PhoneNumber thisPhoneNumber = PhoneNumbers.Find(pn => pn.id == phoneNumber.id);

                thisPhoneNumber.PharmacyID = pharmacyID;

            }

        }

        public PhoneNumber Update(SQLiteConnection connection, PhoneNumber phoneNumber)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdatePhoneNumber, connection))
            {
                command.Parameters.AddWithValue("@Number", phoneNumber.Number);
                command.Parameters.AddWithValue("@State", phoneNumber.State);
                command.Parameters.AddWithValue("@PharmacyID", phoneNumber.PharmacyID);
                command.Parameters.AddWithValue("@IsPrimary", phoneNumber.IsPrimary);
                command.Parameters.AddWithValue("@IsFax", phoneNumber.IsFax);
                command.Parameters.AddWithValue("@id", phoneNumber.id);

                int rowsUpdated = command.ExecuteNonQuery();
                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE PhoneNumber FAILED : {phoneNumber.InlineDetails()}");
                }

                // Update Cached Record
                PhoneNumbers.Remove(PhoneNumbers.Find(pn => pn.id == phoneNumber.id));
                PhoneNumbers.Add(phoneNumber);
            
                return phoneNumber;
            }
        }
        
        public void Delete(SQLiteConnection connection, PhoneNumber phoneNumber)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.DeletePhoneNumber, connection))
            {
                command.Parameters.AddWithValue("@id", phoneNumber.id);

                int rowsDeleted = command.ExecuteNonQuery();

                if (rowsDeleted == 0)
                {
                    throw new Exception($"ATTEMPTING TO DELETE PhoneNumber FAILED : {phoneNumber.InlineDetails()}");
                }

            }
        }


        ///////////// Helpers /////////////
        public bool Contains(PhoneNumber newPhoneNumber)
        {
            if (PhoneNumbers.Any(pn => pn.Number == newPhoneNumber.Number))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void TimeToAccess()
        {
            var allStartTime = DateTime.Now;
            var all = All();
            var allEndTime = DateTime.Now;
            var allTimeSpan = allEndTime - allStartTime;

            var message1 = $"PhoneNumbers: {Math.Round(allTimeSpan.TotalSeconds, 2)}s for {all.Count()} records";
            Console.WriteLine(Logger.Database(message1));

            var unassignedStartTime = DateTime.Now;
            var unassigned = GetUnassigned();
            var unassignedEndTime = DateTime.Now;
            var unassignedTimeSpan = unassignedEndTime - unassignedStartTime;

            var message2 = $"Unassigned:   {Math.Round(unassignedTimeSpan.TotalSeconds, 2)}s for {unassigned.Count()} records";
            Console.WriteLine(Logger.Database(message2));

            var assignedStartTime = DateTime.Now;
            var assigned = GetAssigned();
            var assignedEndTime = DateTime.Now;
            var assignedTimeSpan = assignedEndTime - assignedStartTime;

            var message3 = $"Assigned:     {Math.Round(assignedTimeSpan.TotalSeconds, 2)}s for {assigned.Count()} records";
            Console.WriteLine(Logger.Database(message3));
        }

    }
}