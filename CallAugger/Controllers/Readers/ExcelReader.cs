using CallAugger.Utilities;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CallAugger.Controllers
{
    public class ExcelReader
    {
        ///////////////////////////////////////////////////////////////
        // This class is used to read in data from an Excel file. It is used
        // by the DataImporter classes. this class is responsible for reading
        // and returning a data structure from an Excel file. It is not
        // responsible for processing the data in any way.

        public ExcelReader(string filePath)
        {                                 // Order of opperations:
            this.FilePath = filePath;     // set file path
        }                                 // open the file and workbook
                                          // Read the data from each worksheet
                                          // close the workbook

        private string FilePath;
        private List<List<string>> Data = new List<List<string>>();
        private Application excelApp = new Microsoft.Office.Interop.Excel.Application();



        // Open an Excel file from FilePath and return a Workbook
        private Workbook OpenFile()
        {
            try
            {
                return excelApp.Workbooks.Open(FilePath);
            }
            catch (Exception e)
            {
                throw new Exception($"Error in ExcelReader:OpenFile() ~ Opening File {Path.GetFileName(FilePath)}. {e.Message} \n");
            }

        }

        public bool IsOpen()
        {
            return excelApp.Workbooks.Count > 0;
        }

        public void CloseReader()
        {
            try
            {
                // release each worksheet in this work book by index
                foreach (Worksheet worksheet in excelApp.Worksheets)
                {
                    Marshal.ReleaseComObject(worksheet);
                }

                // close excel app
                excelApp.Workbooks.Close();
                excelApp.Quit();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in ExcelReader:CloseReader() ~ Closing File {Path.GetFileName(FilePath)}. {ex.Message} \n");
            }
        }


        // take all data from each worksheet and return a list (rows) of lists (string values)
        public List<List<string>> ReadData()
        {
            var workbook = OpenFile();
            workbook.Activate();

            try
            {
                Worksheet sheet = workbook.Worksheets[1];

                // Begin Progress bar
                Console.WriteLine($"Importing file: {Path.GetFileName(FilePath)}");
                ProgressBarUtility.WriteProgressBar(0);

                sheet.Activate();
                var headers = GetHeaders(sheet);
                int maxRows = sheet.UsedRange.Rows.Count;

                int StartingRow = Data.Count == 0 ? 1 : 2;

                // iterate through each row
                for (int rowNum = StartingRow; rowNum < maxRows + 1; rowNum++)
                {
                    // get the range of the row
                    var range = sheet.Range["A" + rowNum + ":" + ConvertToLetter(headers.Count) + rowNum];

                    // take each value in this row and add it to a list
                    var row = RangeToList(range);

                    // Update Progress bar
                    ProgressBarUtility.WriteProgressBar((int)((rowNum * 100) / maxRows), true);
                    Data.Add(row);

                    range = null;
                    row = null;
                }

                sheet = null;
            }
            catch (Exception e)
            {
                throw new Exception($"Error in ExcelReader:ReadData() ~ Could not read {Path.GetFileName(FilePath)}. {e.Message} \n");
            }


            return Data;
        }


        // take each value in this row and add it to a list uses null for missing
        public List<string> RangeToList(Microsoft.Office.Interop.Excel.Range inputRange)
        {
            object[,] cellValues = (object[,])inputRange.Value;
            List<string> list = 
                cellValues.Cast<object>().ToList().ConvertAll(x => x != null ? Convert.ToString(x) : "null");
            return list;
        }

        // convert a number to a letter
        private string ConvertToLetter(int num)
        {
            string letter = "";
            while (num > 0)
            {
                int temp = (num - 1) % 26;
                letter = (char)(temp + 65) + letter;
                num = (num - temp - 1) / 26;
            }
            return letter.ToUpper();
        }

        // get the headers from the first row of the sheet
        private Dictionary<string, int> GetHeaders(Worksheet sheet)
        {
            Dictionary<string, int> headers = new Dictionary<string, int>();

            try
            {
                int MaxColumns = sheet.UsedRange.Columns.Count;
                for (int col = 1; col <= MaxColumns; col++)
                {
                    string header = sheet.Cells[1, col].Value;
                    headers.Add(header, col);
                }
                return headers;

            }
            catch (Exception e)
            {
                throw new Exception($"Error in ExcelReader:GetHeaders() ~ Could not get headers from {sheet.Name}. {e.Message} \n");
                throw;
            }

        }
    }
}
