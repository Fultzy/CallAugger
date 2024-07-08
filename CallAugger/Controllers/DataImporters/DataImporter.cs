using CallAugger.Utilities.Sqlite;
using CallAugger.Utilities;
using System;
using System.Collections.Generic;
using CallAugger.Utilities.CliInterface;

namespace CallAugger.DataImporters
{
    public class DataImporter
    {
        ///////////////////////////////////////////////////////////////
        // This class is responsible for reading in call record and
        // pharmacy data from a file and creating a list of CallRecord objects.

        public SQLiteHandler dbHandle;


        public List<CallRecord> ImportCallData()
        {
            var ICallImporter = new CallRecordDataImporter();
            List<CallRecord> callRecords = null;

            // Open Files and return the Data inside of it
            List<List<string>> data = ICallImporter.ReadInCallRecordData();

            if (data != null)
            {
                // Try to insert these CallRecords into the database
                callRecords = ICallImporter.ImportCallData(data, dbHandle);
            }

            ICallImporter = null;
            return callRecords;
        }


        public List<Pharmacy> ImportPharmacyData()
        {
            var IPharmImporter = new PharmacyDataImporter();
            List<Pharmacy> pharmacies = null;

            // Open Files and return the Data inside of it
            List<List<string>> data = IPharmImporter.ReadInPharmacyData();

            if (data != null)
            {
                // Try to insert these pharmacies into the database
                pharmacies = IPharmImporter.ImportPharmacyData(data, dbHandle);
            }

            IPharmImporter = null;
            return pharmacies;
        }
    }
}
