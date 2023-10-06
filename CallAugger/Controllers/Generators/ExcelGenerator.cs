using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// C:\code\CallAugger\CallAugger\bin\Debug\Reports
namespace CallAugger.Controllers.Generators
{
    internal class ExcelGenerator
    {
        ///////////////////////////////////////////////////////////////
        // This class is used to Create Exel files. this class will
        // direct the creation of the excel file and the placement of
        // values within it. Several Interfaces will assist in the 
        // creation of differently formatted worksheets. 

        public ExcelGenerator(string filePath)
        {
            this.FilePath = filePath + @"\Reports";
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;
        }

        private string FilePath;
        private Application excelApp = new Microsoft.Office.Interop.Excel.Application();

        public void CreateNewExcelFile()
        {
            string fileName = @"\CallerReport_" + DateTime.Now.Date.ToShortDateString() + ".xlsx";
            Console.WriteLine(FilePath + fileName);
            
            // Validate the file path
            //if (!System.IO.Directory.Exists(FilePath)) System.IO.Directory.CreateDirectory(FilePath);

            //Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            //var sheet = workbook.Worksheets.Add(Type.Missing);
            //sheet.Name = "Top Callers";

            //workbook.Close(true, FilePath + fileName);
            
            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(FilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                // Open the stream and read it back.
                using (StreamReader sr = File.OpenText(FilePath))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(s);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error in ExcelGenerator:CreateNewExcelFile() ~ Could Not Create New Excel File. \n{e.Message}\n");
            }
        }
    }
}
