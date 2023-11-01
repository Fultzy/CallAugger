using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities.Sqlite
{
    internal class QueryStore
    {
        // Creation Queries
        public static string CreateCallRecordsTable =
            "CREATE TABLE IF NOT EXISTS CallRecords (id INTEGER PRIMARY KEY AUTOINCREMENT, CallType TEXT, Duration INTEGER, Time TEXT, UserName TEXT, UserExtention TEXT, TransferUser TEXT, Caller TEXT, PhoneNumberID INTEGER, UserID INTEGER)";

        public static string CreatePhoneNumbersTable =
            "CREATE TABLE IF NOT EXISTS PhoneNumbers (id INTEGER PRIMARY KEY AUTOINCREMENT, Number TEXT, PharmacyID INTEGER)";

        public static string CreatePharmaciesTable =
            "CREATE TABLE IF NOT EXISTS Pharmacies (id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Npi TEXT, Dea Text, Ncpdp TEXT, Address TEXT, City TEXT, State TEXT, Zip TEXT, ContactName1 TEXT, ContactName2 TEXT, Anniversary TEXT, PrimaryPhoneNumber TEXT)";

        public static string CreateUsersTable =
            "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY AUTOINCREMENT, UserName TEXT, UserExtention TEXT)";

        // Drop Queries
        public static string DropCallRecordsTable = "DROP TABLE IF EXISTS CallRecords";

        public static string DropPhoneNumbersTable = "DROP TABLE IF EXISTS PhoneNumbers";
        
        public static string DropPharmaciesTable = "DROP TABLE IF EXISTS Pharmacies";

        public static string DropUsersTable = "DROP TABLE IF EXISTS Users";


        // Insert Non-Queries
        public static string InsertCallRecord =
            "INSERT INTO CallRecords (CallType, Duration, Time, UserName, UserExtention, TransferUser, Caller, PhoneNumberID, UserID) VALUES (@CallType, @Duration, @Time, @UserName, @UserExtention, @TransferUser, @Caller, @PhoneNumberID, @UserID)";

        public static string InsertPharmacy =
            "INSERT INTO Pharmacies (Name, Npi, Dea, Ncpdp, Address, City, State, Zip, ContactName1, ContactName2, Anniversary, PrimaryPhoneNumber) VALUES (@Name, @Npi, @Dea, @Ncpdp, @Address, @City, @State, @Zip, @ContactName1, @ContactName2, @Anniversary, @PrimaryPhoneNumber)";

        public static string InsertUser =
            "INSERT INTO Users (UserName, UserExtention) VALUES (@UserName, @UserExtention)";

        public static string InsertPhoneNumber =
            "INSERT INTO PhoneNumbers (Number, PharmacyID) VALUES (@Number, @PharmacyID)";


        // Select Queries
        public static string SelectAllCallRecords = "SELECT * FROM CallRecords";
        public static string SelectAllUsers = "SELECT * FROM Users";
        public static string SelectAllPharmacies = "SELECT * FROM Pharmacies";
        public static string SelectAllPhoneNumbers = "SELECT * FROM PhoneNumbers";
        
        // Special Select Queries
        public static string SelectCallRecordByCaller =
            "SELECT * FROM CallRecords WHERE Caller = @Caller";
        public static string SelectUnassignedPhoneNumbers =
            "SELECT * FROM PhoneNumbers WHERE PharmacyID IS 0";
        public static string SelectOldestCallRecordDate = 
            "SELECT Time FROM CallRecords ORDER BY Time ASC LIMIT 1";


        // Update Non-Queries
        public static string UpdatePhoneNumbersPharmacy =
            "UPDATE PhoneNumbers SET PharmacyID = @PharmacyID WHERE id = @id";    

        public static string UpdatePharmacy =
            "UPDATE Pharmacies SET Name = @Name, Npi = @Npi, Dea = @Dea, Ncpdp = @Ncpdp, Address = @Address, City = @City, State = @State, Zip = @Zip, ContactName1 = @ContactName1, ContactName2 = @ContactName2, PrimaryPhoneNumber = @PrimaryPhoneNumber WHERE id = @id";

        public static string UpdateUser =
            "UPDATE Users SET UserName = @UserName, UserExtention = @UserExtention, TransferUser = @TransferUser WHERE id = @id";

        // Special Update Non-Queries
        public static string UpdateCallRecordForeignIDs =
            "UPDATE CallRecords SET PhoneNumberID = @PhoneNumberID, UserID = @UserID WHERE id = @id";

        public static string UpdatePhoneNumberPharmacyID =
            "UPDATE PhoneNumbers SET PharmacyID = @PharmacyID WHERE id = @id";

        // Delete Records
        public static string DeleteCallRecord =
            "DELETE FROM CallRecords WHERE id = @id";

    }
}
