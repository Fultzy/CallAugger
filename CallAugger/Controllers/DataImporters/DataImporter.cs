using CallAugger.Utilities.DataBase;
using System;
using System.Collections.Generic;


namespace CallAugger.Controllers.DataImporters
{
    public class DataImporter
    {
        ///////////////////////////////////////////////////////////////
        // This class to help direct the CallRecord and Pharmacy object
        // creation process. It is responsible for passing data to some
        // Excel Readers and passing it to the appropriate object creation
        // class. It is not responsible for processing the data otherwise.


        // Turn a file path into a list of CallRecords
        public List<CallRecord> ImportCallData(string path)
        {
            var startTime = DateTime.Now;
            var ICallImporter = new CallRecordDataImporter();
            List<CallRecord> callRecords = null;

            // Open Files and return the Data inside of it
            List<List<string>> data = ICallImporter.ReadInCallRecordData(path);

            if (data != null)
            {
                // Create CallRecords from the data above
                callRecords = ICallImporter.CreateCallRecords(data);

                // write how long it took to complete these tasks
                var endTime = DateTime.Now;
                var timeSpan = endTime - startTime;
                Console.WriteLine($" ~ Took {Math.Round(timeSpan.TotalMinutes, 2)} minutes\n");
            }

            return callRecords;
        }


        // Turn a file path into a list of Pharmcaies
        public List<Pharmacy> ImportPharmacyData(string path)
        {
            var startTime = DateTime.Now;
            var IPharmImporter = new PharmacyDataImporter();
            List<Pharmacy> pharmacies = null;

            // Open Files and return the Data inside of it
            List<List<string>> data = IPharmImporter.ReadInPharmacyData(path);

            if (data != null)
            {
                // Create Pharmacies from the data above
                pharmacies = IPharmImporter.CreatePharmacyRecords(data);
                
                // write how long it took to complete these tasks
                var endTime = DateTime.Now;
                var timeSpan = endTime - startTime;
                Console.WriteLine($" ~ Took {Math.Round(timeSpan.TotalMinutes, 2)} minutes\n");
            }


            return pharmacies;
        }
    }
}
