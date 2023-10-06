using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers
{
    public class CsvReader
    {
        private string FilePath;
        private Workbook workbook;

        public CsvReader(string filePath) => FilePath = filePath;

        // Open a csv file from FilePath and return a Workbook
        public Workbook Read()
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook = excelApp.Workbooks.Open(FilePath);
            return workbook;
        }

        public void RollTheTape()
        {
            // get the first worksheet
            workbook.Worksheets[1].Activate();
            var sheet = workbook.ActiveSheet;

            // get the number of rows and columns
            int rows = sheet.UsedRange.Rows.Count;
            int cols = sheet.UsedRange.Columns.Count;

            // iterate through each row
            for (int row = 1; row <= rows; row++)
            {
                // iterate through each column
                for (int col = 1; col <= cols; col++)
                {
                    // get the value of the cell
                    var cell = sheet.Cells[row, col];
                    var value = cell.Value;


                }
            }
        }
    }
}
