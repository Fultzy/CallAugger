using CallAugger.Controllers.Generators.Worksheets;
using CallAugger.Controllers.Generators.WorkSheets;
using CallAugger.Settings;
using CallAugger.Utilities;
using CallAugger.Utilities.DataBase;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


// C:\code\CallAugger\CallAugger\bin\Debug\Reports
namespace CallAugger.Controllers.Generators
{
    internal class ExcelGenerator
    {
        ///////////////////////////////////////////////////////////////
        // This class is used to Create Excel files. this class will
        // direct the creation of the excel file and the placement of
        // values within it. Several Interfaces will assist in the 
        // creation of differently formatted worksheets. 

        private string FilePath;

        public ExcelGenerator(string filePath)
        {
            this.FilePath = filePath + @"\Reports";
        }


        public void CreateFullExcelReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;
           
            string fileName = @"\FullyAuggedReport_" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create each worksheet
            Worksheet worksheet5 = workbook.ActiveSheet;
            worksheet5 = SupportRepListingWorksheet.Create(dbHandle, worksheet5);

            Worksheet worksheet4 = workbook.Sheets.Add();
            worksheet4 = SupportMetricsWorksheet.Create(dbHandle, worksheet4);

            Worksheet worksheet3 = workbook.Sheets.Add();
            worksheet3 = CallerReportWorksheet.Create(dbHandle, worksheet3);

            Worksheet worksheet2 = workbook.Sheets.Add();
            worksheet2 = UnassignedPhoneNumberWorksheet.Create(dbHandle, worksheet2);

            Worksheet worksheet1 = workbook.Sheets.Add();
            worksheet1 = PharmacyReportWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();

            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(worksheet2);
            Marshal.ReleaseComObject(worksheet3);
            Marshal.ReleaseComObject(worksheet4);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }


        public void CreateDetailedCallerReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;
            
            string fileName = @"\CallerReport_" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create worksheet
            Worksheet worksheet1 = workbook.ActiveSheet;
            worksheet1 = CallerReportWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();

            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }


        public void CreateDetailedPharmacyReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            string fileName = @"\PharamcyReport_" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create worksheet
            Worksheet worksheet1 = workbook.ActiveSheet;
            worksheet1 = PharmacyReportWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();

            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }


        public void CreateSupportMetricsReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            

            // setup and create file
            string fileName = @"\SupportReport_" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create support metrics report worksheet
            Worksheet worksheet1 = workbook.ActiveSheet;
            worksheet1 = SupportMetricsWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();

            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);


            // splwow64.exe
        }


        public void CreateDetailedUserListingReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            string fileName = @"\SupportRepListing" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create support metrics report worksheet
            Worksheet worksheet1 = workbook.ActiveSheet;
            worksheet1 = SupportRepListingWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();
            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }


        public void CreateUnassignedPhoneNumberReport(SQLiteHandler dbHandle)
        {
            Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            string fileName = @"\UnassignedReport_" + UniqueTimeCode();
            Workbook workbook = excelApp.Workbooks.Add(Type.Missing);

            // create support metrics report worksheet
            Worksheet worksheet1 = workbook.ActiveSheet;
            worksheet1 = UnassignedPhoneNumberWorksheet.Create(dbHandle, worksheet1);

            // save the workbook
            workbook.SaveAs(FilePath + fileName + ".xlsx");
            workbook.Close(false);

            excelApp.Quit();

            Marshal.ReleaseComObject(worksheet1);
            Marshal.ReleaseComObject(workbook);
            Marshal.ReleaseComObject(excelApp);
        }


        static internal string UniqueTimeCode()
        {
            DateRange dateRange = new DateRange();
            var uniqueCode = DateTime.Now.ToString("_hhmmss");
            return dateRange.ToFileNameString() + uniqueCode;
        }
    }
}
