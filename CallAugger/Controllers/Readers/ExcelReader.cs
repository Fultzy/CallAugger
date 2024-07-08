using CallAugger.Utilities;
using CallAugger.Utilities.Validators;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CallAugger.Readers
{
    public class ExcelReader
    {

        ///////////////////////////////////////////////////////////////
        // This class is used to read in data from an Excel file. It is used
        // by the DataImporter classes. this class is responsible for reading
        // and returning a data structure from an Excel file. It is not
        // responsible for processing the data in any way.

        public ExcelReader(string filePath)
        {
            this.FilePath = filePath;
        }

        public readonly string FilePath;
        private Application excelApp = null;


        // Open an Excel file from FilePath and return a Workbook
        private Workbook OpenFile()
        {
            try
            {
                excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                return excelApp.Workbooks.Open(FilePath);
            }
            catch (Exception e)
            {
                var message = $"Error in ExcelReader.OpenFile() ~ Could Not Open {Path.GetFileName(FilePath)}. {e.Message} \n";

                this.CloseReader();
                throw new Exception(Logger.Error(Logger.Importing(message)));
            }

        }



        // take all data from each worksheet and return a list (rows) of lists (string values)
        public List<List<string>> ReadData()
        {
            try
            {
                Workbook workbook = OpenFile();
                workbook.Activate();

                Worksheet sheet = workbook.Worksheets[1];
                sheet.Activate();
                
                List<List<string>> data = new List<List<string>>();

                var headers = HeaderHandler.GetHeaderFromWorksheet(sheet);
                int maxRows = sheet.UsedRange.Rows.Count;


                // iterate through each row
                for (int rowNum = 1; rowNum < maxRows; rowNum++)
                {
                    // get the range of the row
                    var range = sheet.Range["A" + rowNum + ":" + ConvertToLetter(headers.Count) + rowNum];

                    // take each value in this row and add it to a list
                    var row = RangeToList(range);

                    data.Add(row);

                    range = null;
                    row = null;
                }

                this.CloseReader();
                return data;
            }
            catch (Exception e)
            {
                var message = $"Error in ExcelReader.ReadData() ~ Could Not Read {Path.GetFileName(FilePath)}. {e.Message} \n";

                this.CloseReader();
                throw new Exception(Logger.Error(message));
            }

        }



        /////////////////////// Helper Methods ////////////////////////
        public List<string> RangeToList(Microsoft.Office.Interop.Excel.Range inputRange)
        {
            object[,] cellValues = (object[,])inputRange.Value;

            List<string> list = cellValues.Cast<object>().ToList().ConvertAll(x => x != null ? Convert.ToString(x) : "null");
            
            inputRange = null;
            return list;
        }


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

       
        public void CloseReader()
        {
            try
            {
                if (this.IsOpen())
                {
                    // release each worksheet in this work book
                    foreach (Worksheet worksheet in excelApp.Worksheets)
                    {
                        Marshal.ReleaseComObject(worksheet);
                    }

                    // close excel app
                    this.excelApp.Workbooks.Close();
                    this.excelApp.Quit();
                    this.excelApp = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                var message = $"Error in ExcelReader.CloseReader() ~ Could Not Close {Path.GetFileName(FilePath)}. {ex.Message} \n";

                throw new Exception(Logger.Error(message));
            }
        }

        public bool IsOpen()
        {
            return excelApp != null;
        }
    }
}
