using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var IImporter = new CallRecordDataImporter();
            List<CallRecord> callRecords = null;

            // Create a list of 
            var data = IImporter.ReadInCallRecordData(path);

            // use the list of ExcelReaders to create a list of CallRecords
            callRecords = IImporter.CreateCallRecords(data);
            
            // write how long it took to complete these tasks
            var endTime = DateTime.Now;
            var timeSpan = endTime - startTime;
            Console.WriteLine($" ~ Took {Math.Round(timeSpan.TotalMinutes, 2)} minutes\n");

            return callRecords;
        }


        // Turn a file path into a list of Pharmcaies
        public List<Pharmacy> ImportPharmacyData(string path)
        {
            var startTime = DateTime.Now;
            var IImporter = new PharmacyDataImporter();
            List<Pharmacy> pharmacies = null;

            // create a list of ExcelReaders
            var data = IImporter.ReadInPharmacyData(path);

            // use the list of ExcelReaders to create a list of Pharmacies
            pharmacies = IImporter.CreatePharmacyRecords(data);


            // write how long it took to complete these tasks
            var endTime = DateTime.Now;
            var timeSpan = endTime - startTime;
            Console.WriteLine($" ~ Took {Math.Round(timeSpan.TotalMinutes, 2)} minutes\n");

            return pharmacies;
        }
    }
}
