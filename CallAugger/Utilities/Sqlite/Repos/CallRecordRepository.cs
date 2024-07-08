using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Configuration;
using CallAugger.Settings;

namespace CallAugger.Utilities.Sqlite
{
    public class CallRecordRepository
    {
        // Instance property
        private static readonly object lockObject = new object();
        private static CallRecordRepository instance;

        public static CallRecordRepository Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new CallRecordRepository();
                    }
                    return instance;
                }
            }
        }

        // Cache
        public List<CallRecord> CallRecords = new List<CallRecord>();

        private CallRecordRepository()
        {
            All();
        }


        ///////////// Fetching /////////////
        public List<CallRecord> All()
        {
            DateRange dateRange = new DateRange();
            List<CallRecord> callRecords = new List<CallRecord>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand(QueryStore.SelectAllCallRecords, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    if (!dateRange.IsInRange(Convert.ToDateTime(reader["Time"]))) continue;

                    CallRecord callRecord = new CallRecord
                    {
                        id = Convert.ToInt32(reader["id"]),
                        UserID = Convert.ToInt32(reader["UserID"]),
                        PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]),
                        CallType = reader["CallType"].ToString(),
                        Duration = Convert.ToInt32(reader["Duration"]),
                        Time = Convert.ToDateTime(reader["Time"]),
                        UserName = reader["UserName"].ToString(),
                        UserExtention = reader["UserExtention"].ToString(),
                        TransferUser = reader["TransferUser"].ToString(),
                        Caller = reader["Caller"].ToString(),
                        State = reader["State"].ToString()
                    };

                    callRecords.Add(callRecord);
                }

                connection.Close();
            }

            CallRecords = callRecords.OrderBy(c => c.Time).ToList();
            return CallRecords;
        }

        public List<CallRecord> ByPhoneNumber(SQLiteConnection connection, int phoneNumberID)
        {
            DateRange dateRange = new DateRange().GetCurrentDateRange();
            List<CallRecord> theseCallRecords = new List<CallRecord>();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllCallRecords + " WHERE PhoneNumberID = @PhoneNumberID", connection);

            command.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!dateRange.IsInRange(Convert.ToDateTime(reader["Time"]))) continue;

                    CallRecord callRecord = new CallRecord
                    {
                        id = Convert.ToInt32(reader["id"]),
                        UserID = Convert.ToInt32(reader["UserID"]),
                        PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]),
                        CallType = reader["CallType"].ToString(),
                        Duration = Convert.ToInt32(reader["Duration"]),
                        Time = Convert.ToDateTime(reader["Time"]),
                        UserName = reader["UserName"].ToString(),
                        UserExtention = reader["UserExtention"].ToString(),
                        TransferUser = reader["TransferUser"].ToString(),
                        Caller = reader["Caller"].ToString(),
                        State = reader["State"].ToString()
                    };

                    theseCallRecords.Add(callRecord);
                }
            }

            return theseCallRecords;
        }

        public List<CallRecord> ByUser(SQLiteConnection connection, int userID)
        {
            DateRange dateRange = new DateRange();
            List<CallRecord> theseCallRecords = new List<CallRecord>();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllCallRecords + " WHERE UserID = @UserID", connection);

            command.Parameters.AddWithValue("@UserID", userID);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!dateRange.IsInRange(Convert.ToDateTime(reader["Time"]))) continue;

                    CallRecord callRecord = new CallRecord
                    {
                        id = Convert.ToInt32(reader["id"]),
                        UserID = Convert.ToInt32(reader["UserID"]),
                        PhoneNumberID = Convert.ToInt32(reader["PhoneNumberID"]),
                        CallType = reader["CallType"].ToString(),
                        Duration = Convert.ToInt32(reader["Duration"]),
                        Time = Convert.ToDateTime(reader["Time"]),
                        UserName = reader["UserName"].ToString(),
                        UserExtention = reader["UserExtention"].ToString(),
                        TransferUser = reader["TransferUser"].ToString(),
                        Caller = reader["Caller"].ToString(),
                        State = reader["State"].ToString()
                    };

                    theseCallRecords.Add(callRecord);
                }
            }

            return theseCallRecords;
        }

        public CallRecord ID(SQLiteConnection connection, int id)
        {
            CallRecord callRecord = new CallRecord();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllCallRecords + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            using (SQLiteDataReader reader = command.ExecuteReader())
            {
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
                    callRecord.State = reader["State"].ToString();
                }
            }

            return callRecord;
        }


        ///////////// CRUD /////////////
        public CallRecord Insert(SQLiteConnection connection, CallRecord callRecord)
        {
            if (!this.Contains(callRecord))
            {
                SQLiteCommand command = new SQLiteCommand(QueryStore.InsertCallRecord, connection);

                command.Parameters.AddWithValue("@CallType", callRecord.CallType);
                command.Parameters.AddWithValue("@Duration", callRecord.Duration);
                command.Parameters.AddWithValue("@Time", callRecord.Time);
                command.Parameters.AddWithValue("@UserName", callRecord.UserName);
                command.Parameters.AddWithValue("@UserExtention", callRecord.UserExtention);
                command.Parameters.AddWithValue("@TransferUser", callRecord.TransferUser);
                command.Parameters.AddWithValue("@Caller", callRecord.Caller);
                command.Parameters.AddWithValue("@PhoneNumberID", callRecord.PhoneNumberID);
                command.Parameters.AddWithValue("@UserID", callRecord.UserID);
                command.Parameters.AddWithValue("@State", callRecord.State);


                command.ExecuteNonQuery();

                command.CommandText = "SELECT last_insert_rowid()";
                object id = command.ExecuteScalar();
                callRecord.id = Convert.ToInt32(id);

                // Update Cache
                CallRecords.Add(callRecord);

                return callRecord;
            }

            return this.ID(connection, callRecord.id);
        }
        
        public void UpdateCallIDs(SQLiteConnection connection, CallRecord call, int phoneNumberID, int userID)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdateCallRecordForeignIDs, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@id", call.id);

                int rowsUpdated = command.ExecuteNonQuery();


                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE CallRecord UserID:{userID} && PhoneNumberID:{phoneNumberID} FAILED : CallRecord: {call.InlineDetails()}");
                }

                // Update Cached Record
                CallRecord thisCall = CallRecords.Find(cr => cr.id == call.id);

                thisCall.UserID = userID;
                thisCall.PhoneNumberID = phoneNumberID;

            }
        }

        public void UpdateUserID(SQLiteConnection connection, CallRecord call, int userID)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdateCallRecordUserID, connection))
            {
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@id", call.id);

                int rowsUpdated = command.ExecuteNonQuery();


                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE CallRecord UserID:{userID} FAILED : {call.InlineDetails()}");
                }

                // Update Cached Record
                CallRecord thisCall = CallRecords.Find(cr => cr.id == call.id);

                thisCall.UserID = userID;

            }
        }

        public void UpdatePhoneNumberID(SQLiteConnection connection, CallRecord call, int phoneNumberID)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdateCallRecordPhoneNumberID, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumberID", phoneNumberID);
                command.Parameters.AddWithValue("@id", call.id);

                int rowsUpdated = command.ExecuteNonQuery();

                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE CallRecord PhoneNumberID:{phoneNumberID} FAILED : {call.InlineDetails()}");
                }

                // Update Cached Record
                CallRecord thisCall = CallRecords.Find(cr => cr.id == call.id);

                thisCall.PhoneNumberID = phoneNumberID;

            }
        }

        public void Delete(SQLiteConnection connection, CallRecord callRecord)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.DeleteCallRecord, connection))
            {
                command.Parameters.AddWithValue("@id", callRecord.id);

                int rowsDeleted = command.ExecuteNonQuery();

                if (rowsDeleted == 0)
                {
                    throw new Exception($"ATTEMPTING TO DELETE CallRecord FAILED : {callRecord.InlineDetails()}");
                }

            }
        }


        ///////////// Helpers /////////////
        public bool Contains(CallRecord newCallRecord)
        {
            if (newCallRecord == null) return false;

            // checks cache for matching call record
            if (CallRecords.Any(cr => cr.Time == newCallRecord.Time &&
                cr.Caller == newCallRecord.Caller &&
                cr.TransferUser == newCallRecord.TransferUser &&
                cr.UserExtention == newCallRecord.UserExtention &&
                cr.UserName == newCallRecord.UserName &&
                cr.Duration == newCallRecord.Duration))
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

            var message = $"CallRecords:  {Math.Round(allTimeSpan.TotalSeconds, 2)}s for {all.Count()} records";
            Console.WriteLine(Logger.Database(message));
        }
    }
}