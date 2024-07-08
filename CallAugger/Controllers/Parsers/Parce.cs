using CallAugger.Utilities;
using CallAugger.Utilities.Sqlite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;

namespace CallAugger.Parsers
{
    ///////////////////////////////////////////
    // This class should be used to parse call record data and pharmacy data and
    // insert them into the database. It is useful for managing phone numbers and
    // users in a database and for keeping track of pharmacy fax numbers.

    internal class Parse
    {
        public SQLiteHandler dbHandle;

        ///////////////////////////////////////////
        /// <summary>
        /// The ParseCallRecordData method takes a list of CallRecord objects and parses
        /// them to extract the phone number and user information. It then tries to add
        /// these new objects to the database. If the phone number is valid, it inserts
        /// the phone number and user into the database and updates the call record IDs.
        /// If the phone number is an internal call, it inserts the user into the database
        /// and updates the call record user ID. Finally, it updates the progress bar and
        /// writes how long it took to complete these tasks.
        /// </summary>
        public void ParseCallRecordData(List<CallRecord> callRecords)
        {

        }


        ///////////////////////////////////////////
        /// <summary>
        /// The ParsePharmacyData method retrieves all pharmacies and unassigned phone
        /// numbers from the database. It then iterates through each unassigned phone
        /// number and checks if it is a pharmacy fax number or primary phone number.
        /// If it is, it finds the pharmacy that has this fax number and updates the
        /// phone number's pharmacyID and sets its IsFax property to true. Finally,
        /// it updates the phone number's pharmacyID in the database.
        /// </summary>
        public void ParsePharmacyData()
        {
            // begin progress bar
            int progress = 0;
            Console.WriteLine("\nParcing to Pharmacies.. ");
            ProgressBarUtility.WriteProgressBar(progress);
            
            List<Pharmacy> pharmacies = dbHandle.GetAllPharmacies();
            List<PhoneNumber> unassignedPhoneNumbers = dbHandle.GetUnassignedPhoneNumbers();
            int totalUnassigned = unassignedPhoneNumbers.Count();

            using (SQLiteConnection connection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                foreach (PhoneNumber phoneNumber in unassignedPhoneNumbers)
                {
                    if (pharmacies.Any(ph => ph.FaxNumber == phoneNumber.Number))
                    {
                        // find the pharmacy that has this fax number
                        var matchingPharmacy = pharmacies.Find(ph => ph.FaxNumber == phoneNumber.Number);

                        phoneNumber.PharmacyID = matchingPharmacy.id;
                        phoneNumber.IsFax = true;

                        dbHandle.PhoneNumberRepo.Update(connection, phoneNumber);
                    }

                    if (pharmacies.Any(ph => ph.PrimaryPhoneNumber == phoneNumber.Number))
                    {
                        // find the pharmacy that has this Primary PhoneNumber
                        var matchingPharmacy = pharmacies.Find(ph => ph.PrimaryPhoneNumber == phoneNumber.Number);

                        phoneNumber.PharmacyID = matchingPharmacy.id;
                        phoneNumber.IsPrimary = true;

                        dbHandle.PhoneNumberRepo.Update(connection, phoneNumber); ;
                    }

                    // update progress bar
                    progress++;
                    ProgressBarUtility.WriteProgressBar(progress * 100 / totalUnassigned, true);
                }

                ProgressBarUtility.WriteProgressBar(100, true);
                Console.WriteLine();
                connection.Close();
            }
        }

    }
}
