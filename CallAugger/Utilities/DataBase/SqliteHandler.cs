using System.Data.Sqlite;

namespace CallAugger.Utilities.DataBase
{

    // no workie
    internal class SQLiteHandler
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SQLiteHandler(string path)
        {
            _dbPath = path + "CallAugger.db";
            _connectionString = ConnectionString();
        }

        private string ConnectionString()
        {
            return $"Data Source={_dbPath};Version=3;";
        }

        public void CreateDatabase()
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new System.Data.SQLite.SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS CallRecords (Id INTEGER PRIMARY KEY AUTOINCREMENT, CallType TEXT, Caller TEXT, Callee TEXT, Duration INTEGER, StartTime TEXT, EndTime TEXT, UserName TEXT, UserExtention TEXT, UserPhoneNumber TEXT, UserPhoneNumberType TEXT, UserPhoneNumberDescription TEXT, UserPhoneNumberId TEXT, UserPhoneNumberExtension TEXT, UserPhoneNumberExtensionType TEXT, UserPhoneNumberExtensionDescription TEXT, UserPhoneNumberExtensionId TEXT, UserPhoneNumberExtension2 TEXT, UserPhoneNumberExtension2Type TEXT, UserPhoneNumberExtension2Description TEXT, UserPhoneNumberExtension2Id TEXT, UserPhoneNumberExtension3 TEXT, UserPhoneNumberExtension3Type TEXT, UserPhoneNumberExtension3Description TEXT, UserPhoneNumberExtension3Id TEXT, UserPhoneNumberExtension4 TEXT, UserPhoneNumberExtension4Type TEXT, UserPhoneNumberExtension4Description TEXT, UserPhoneNumberExtension4Id TEXT, UserPhoneNumberExtension5 TEXT, UserPhoneNumberExtension5Type TEXT, UserPhoneNumberExtension5Description TEXT, UserPhoneNumberExtension5Id TEXT)";
                    command.ExecuteNonQuery();
                }

                using (var command = new System.Data.SQLite.SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS PhoneNumbers (Id INTEGER PRIMARY KEY AUTOINCREMENT, Number TEXT, Type TEXT, Description TEXT, Extension TEXT, ExtensionType TEXT, ExtensionDescription TEXT, Extension2 TEXT, Extension2Type TEXT, Extension2Description TEXT, Extension3 TEXT, Extension3Type TEXT, Extension3Description TEXT, Extension4 TEXT, Extension4Type TEXT, Extension4Description TEXT, Extension5 TEXT, Extension5Type TEXT, Extension5Description TEXT)";
                    command.ExecuteNonQuery();
                }

                using (var command = new System.Data.SQLite.SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Extention TEXT, TotalCalls INTEGER, TotalInboundCalls INTEGER, TotalOutboundCalls INTEGER, TotalCallTime INTEGER, TotalInboundTalkTime INTEGER, TotalOutboundTalkTime INTEGER)";
                    command.ExecuteNonQuery();
                }

                using (var command = new System.Data.SQLite.SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS UserPhoneNumbers (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId INTEGER, PhoneNumberId INTEGER)";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
