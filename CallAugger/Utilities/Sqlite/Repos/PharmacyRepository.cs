using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Configuration;

namespace CallAugger.Utilities.Sqlite
{
    public class PharmacyRepository
    {
        // Instance property
        private static readonly object lockObject = new object();
        private static PharmacyRepository instance;

        public static PharmacyRepository Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new PharmacyRepository();
                    }
                    return instance;
                }
            }
        }

        // Cache
        public List<Pharmacy> Pharmacies = new List<Pharmacy>();

        private PharmacyRepository()
        {
            All();
        }


        ///////////// Fetching /////////////
        public List<Pharmacy> All()
        {
            List<Pharmacy> pharmacies = new List<Pharmacy>();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand
                    (QueryStore.SelectAllPharmacies, connection);

                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Pharmacy pharmacy = CreateFromReader(reader);

                    // get phone numbers
                    pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));

                    pharmacies.Add(pharmacy);
                }
            }
            
            Pharmacies = pharmacies.OrderByDescending(ph => ph.TotalDuration).ToList();
            return Pharmacies;
        }

        public Pharmacy Npi(SQLiteConnection connection, string Npi)
        {
            Pharmacy pharmacy = new Pharmacy();

            SQLiteCommand command = new SQLiteCommand
            (QueryStore.SelectAllPharmacies + " WHERE Npi = @Npi", connection);

            command.Parameters.AddWithValue("@Npi", Npi);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy = CreateFromReader(reader);

                pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));
            }

            return pharmacy;
        }

        public Pharmacy FindPrimaryNumber(SQLiteConnection connection, string primaryPhoneNumber)
        {
            Pharmacy pharmacy = new Pharmacy();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPharmacies + " WHERE PrimaryPhoneNumber = @PrimaryPhoneNumber", connection);

            command.Parameters.AddWithValue("@PrimaryPhoneNumber", primaryPhoneNumber);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy = CreateFromReader(reader);

                pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));
            }

            return pharmacy;
        }

        public Pharmacy FindFaxNumber(SQLiteConnection connection, string faxNumber)
        {
            Pharmacy pharmacy = new Pharmacy();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPharmacies + " WHERE FaxNumber = @FaxNumber", connection);

            command.Parameters.AddWithValue("@FaxNumber", faxNumber);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy = CreateFromReader(reader);

                pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));
            }

            return pharmacy;
        }

        public Pharmacy Find(SQLiteConnection connection, string name)
        {
            Pharmacy pharmacy = new Pharmacy();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPharmacies + " WHERE Name = @Name", connection);

            command.Parameters.AddWithValue("@Name", name);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy = CreateFromReader(reader);

                pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));
            }

            return pharmacy;
        }

        public Pharmacy ID(SQLiteConnection connection, int id)
        {
            Pharmacy pharmacy = new Pharmacy();

            SQLiteCommand command = new SQLiteCommand
                (QueryStore.SelectAllPharmacies + " WHERE id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                pharmacy = CreateFromReader(reader);

                pharmacy.AddPhoneNumbers(PhoneNumberRepository.Instance.ByPharmacy(connection, pharmacy.id));

            }

            return pharmacy;
        }

        
        ///////////// CRUD /////////////
        public Pharmacy CreateFromReader(SQLiteDataReader reader)
        {
            Pharmacy pharmacy = new Pharmacy
            {
                id = Convert.ToInt32(reader["id"]),
                Name = reader["Name"].ToString(),
                Npi = reader["Npi"].ToString(),
                Dea = reader["Dea"].ToString(),
                Ncpdp = reader["Ncpdp"].ToString(),
                Address = reader["Address"].ToString(),
                City = reader["City"].ToString(),
                State = reader["State"].ToString(),
                Zip = reader["Zip"].ToString(),
                ContactName1 = reader["ContactName1"].ToString(),
                ContactName2 = reader["ContactName2"].ToString(),
                Anniversary = reader["Anniversary"].ToString(),
                PrimaryPhoneNumber = reader["PrimaryPhoneNumber"].ToString(),
                FaxNumber = reader["FaxNumber"].ToString()
            };

            return pharmacy;
        }

        public Pharmacy Insert(SQLiteConnection connection, Pharmacy pharmacy)
        {
            try
            {
                if (!this.Contains(pharmacy))
                {
                    SQLiteCommand command = new SQLiteCommand
                        (QueryStore.InsertPharmacy + "; SELECT last_insert_rowid()", connection);

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
                    command.Parameters.AddWithValue("@FaxNumber", pharmacy.FaxNumber);

                    object id = command.ExecuteScalar();
                    pharmacy.id = Convert.ToInt32(id);

                    // update cache
                    Pharmacies.Add(pharmacy);

                    Logger.Database($"INSERTED Pharmacy : {pharmacy.InlineDetails()}");
                    return pharmacy;
                }

                var foundPharmacy = this.Find(connection, pharmacy.Name);

                Logger.Database($"INSERT ATTEMPT : Duplicate Pharmacys Found: \nAdding: {pharmacy.InlineDetails()}\nFound: {foundPharmacy.InlineDetails()}");

                return foundPharmacy;
            }
            catch (Exception ex)
            {
                throw new Exception(Logger.Database(ex + $"INSERTING Pharmacy FAILED : {pharmacy.InlineDetails()}"));
            }
        }

        public Pharmacy Update(SQLiteConnection connection, Pharmacy pharmacy)
        {
            using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdatePharmacy, connection))
            {

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
                command.Parameters.AddWithValue("@FaxNumber", pharmacy.FaxNumber);
                command.Parameters.AddWithValue("@id", pharmacy.id);

                int rowsUpdated = command.ExecuteNonQuery();
                if (rowsUpdated == 0)
                {
                    throw new Exception($"ATTEMPTING TO UPDATE PhoneNumber FAILED : {pharmacy.InlineDetails()}");
                }

                // update cache
                Pharmacies.Remove(Pharmacies.Find(ph => ph.id == pharmacy.id));
                Pharmacies.Add(pharmacy);

                Logger.Database($"UPDATED Pharmacy : {pharmacy.InlineDetails()}");
                return pharmacy;
            }
        }

        public Pharmacy Update(Pharmacy pharmacy)
        {
            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();  

                using (SQLiteCommand command = new SQLiteCommand(QueryStore.UpdatePharmacy, connection))
                {

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
                    command.Parameters.AddWithValue("@FaxNumber", pharmacy.FaxNumber);
                    command.Parameters.AddWithValue("@id", pharmacy.id);

                    int rowsUpdated = command.ExecuteNonQuery();
                    if (rowsUpdated == 0)
                    {
                        throw new Exception($"ATTEMPTING TO UPDATE PhoneNumber FAILED : {pharmacy.InlineDetails()}");
                    }

                    // update cache
                    Pharmacies.Remove(Pharmacies.Find(ph => ph.id == pharmacy.id));
                    Pharmacies.Add(pharmacy);

                    Logger.Database($"UPDATED Pharmacy : {pharmacy.InlineDetails()}");
                    return pharmacy;
                }
            }
        }

        public void Delete(Pharmacy pharmacy)
        {
            if (pharmacy.id == 0) return;

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(QueryStore.DeletePharmacy, connection))
                {
                    command.Parameters.AddWithValue("@id", pharmacy.id);

                    int rowsDeleted = command.ExecuteNonQuery();

                    if (rowsDeleted == 0)
                    {
                        throw new Exception($"ATTEMPTING TO DELETE Pharmacy FAILED : {pharmacy.InlineDetails()}");
                    }

                    // update cache
                    Pharmacies.Remove(Pharmacies.Find(ph => ph.id == pharmacy.id));

                    Logger.Database($"DELETED Pharmacy : {pharmacy.InlineDetails()}");
                }
            }
        }


        ///////////// Helpers /////////////
        public bool Contains(Pharmacy newPharmacy)
        {
            if (Pharmacies.Count == 0) return false;

            var pharmacy = Pharmacies.Find(ph => 
            ph.id != 0 && 
            ph.Name == newPharmacy.Name &&
            ph.Npi == newPharmacy.Npi &&
            ph.Ncpdp == newPharmacy.Ncpdp &&
            ph.Dea == newPharmacy.Dea);
            
            if (pharmacy != null)
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

            var message = $"Pharmacies:   {Math.Round(allTimeSpan.TotalSeconds, 2)}s for {all.Count()} records";

            Console.WriteLine(Logger.Database(message));
        }
    }
}