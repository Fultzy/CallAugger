
namespace CallAugger.Utilities.Sqlite
{
    internal class QueryStore
    {
        // Creation Queries
        public static string CreateCallRecordsTable =
            "CREATE TABLE IF NOT EXISTS CallRecords (id INTEGER PRIMARY KEY AUTOINCREMENT, CallType TEXT, Duration INTEGER, Time TEXT, UserName TEXT, UserExtention TEXT, TransferUser TEXT, Caller TEXT, State TEXT, PhoneNumberID INTEGER, UserID INTEGER)";

        public static string CreatePhoneNumbersTable =
            "CREATE TABLE IF NOT EXISTS PhoneNumbers (id INTEGER PRIMARY KEY AUTOINCREMENT, Number TEXT, State TEXT, IsPrimary BOOLEAN, IsFax BOOLEAN, PharmacyID INTEGER)";

        public static string CreatePharmaciesTable =
            "CREATE TABLE IF NOT EXISTS Pharmacies (id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Npi TEXT, Dea Text, Ncpdp TEXT, Address TEXT, City TEXT, State TEXT, Zip TEXT, ContactName1 TEXT, ContactName2 TEXT, Anniversary TEXT, PrimaryPhoneNumber TEXT, FaxNumber TEXT)";

        public static string CreateUsersTable =
            "CREATE TABLE IF NOT EXISTS Users (id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Extention TEXT)";

        // Drop Queries
        public static string DropCallRecordsTable = "DROP TABLE IF EXISTS CallRecords";
        public static string DropPhoneNumbersTable = "DROP TABLE IF EXISTS PhoneNumbers";
        public static string DropPharmaciesTable = "DROP TABLE IF EXISTS Pharmacies";
        public static string DropUsersTable = "DROP TABLE IF EXISTS Users";


        // Insert Non-Queries
        public static string InsertCallRecord =
            "INSERT INTO CallRecords (CallType, Duration, Time, UserName, UserExtention, TransferUser, Caller, State, PhoneNumberID, UserID) VALUES (@CallType, @Duration, @Time, @UserName, @UserExtention, @TransferUser, @Caller, @State, @PhoneNumberID, @UserID)";

        public static string InsertPharmacy =
            "INSERT INTO Pharmacies (Name, Npi, Dea, Ncpdp, Address, City, State, Zip, ContactName1, ContactName2, Anniversary, PrimaryPhoneNumber, FaxNumber) VALUES (@Name, @Npi, @Dea, @Ncpdp, @Address, @City, @State, @Zip, @ContactName1, @ContactName2, @Anniversary, @PrimaryPhoneNumber, @FaxNumber)";

        public static string InsertUser =
            "INSERT INTO Users (Name, Extention) VALUES (@Name, @Extention)";

        public static string InsertPhoneNumber =
            "INSERT INTO PhoneNumbers (Number, State, IsPrimary, IsFax, PharmacyID) VALUES (@Number, @State, @IsPrimary, @IsFax, @PharmacyID)";


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
        public static string SelectAssignedPhoneNumbers =
            "SELECT * FROM PhoneNumbers WHERE PharmacyID IS NOT 0";


        // Update Non-Queries
        public static string UpdatePhoneNumber =
            "UPDATE PhoneNumbers SET Number = @Number, State = @State, IsPrimary = @IsPrimary, IsFax = @IsFax, PharmacyID = @PharmacyID WHERE id = @id";

        public static string UpdatePharmacy =
            "UPDATE Pharmacies SET Name = @Name, Npi = @Npi, Dea = @Dea, Ncpdp = @Ncpdp, Address = @Address, City = @City, State = @State, Zip = @Zip, ContactName1 = @ContactName1, ContactName2 = @ContactName2, PrimaryPhoneNumber = @PrimaryPhoneNumber, FaxNumber = @FaxNumber WHERE id = @id";

        public static string UpdateUser =
            "UPDATE Users SET Name = @Name, Extention = @Extention WHERE id = @id";

        // Special Update Non-Queries
        public static string UpdateCallRecordForeignIDs =
            "UPDATE CallRecords SET PhoneNumberID = @PhoneNumberID, UserID = @UserID WHERE id = @id";

        public static string UpdateCallRecordUserID =
            "UPDATE CallRecords SET UserID = @UserID WHERE id = @id";

        public static string UpdateCallRecordPhoneNumberID =
            "UPDATE CallRecords SET PhoneNumberID = @PhoneNumberID WHERE id = @id";

        public static string UpdatePhoneNumberPharmacyID =
            "UPDATE PhoneNumbers SET PharmacyID = @PharmacyID WHERE id = @id";

        // Delete Records
        public static string DeleteCallRecord = "DELETE FROM CallRecords WHERE id = @id";
        public static string DeletePhoneNumber = "DELETE FROM PhoneNumbers WHERE id = @id";
        public static string DeletePharmacy = "DELETE FROM Pharmacies WHERE id = @id";
        public static string DeleteUser = "DELETE FROM Users WHERE id = @id";
    }
}
