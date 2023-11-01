using CallAugger.Utilities;
using CallAugger.Utilities.DataBase;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Generators
{
    static class CallerReportWorksheet
    {
        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {
            ///////////////////////////////////////////////////////////////
            // This method will create the Call Report Worksheet. This
            // worksheet will contain all of the call records for the
            // specified time span. 
            ///////////////////////////////////////////////////////////////

            // set the worksheet name
            worksheet.Name = "Caller Report";

            worksheet = AddHeader(worksheet);

            // set the row counter
            int row = 2;

            // get all PhoneNumbers
            List<PhoneNumber> phoneNumbers = dbHandle.GetAllPhoneNumbers();

            // begin progress bar 
            Console.WriteLine("\nCreating Caller Report, {0} Callers:", phoneNumbers.Count());
            ProgressBarUtility.WriteProgressBar(0);


            // enumerate the list and add each record
            foreach (var phoneNumber in phoneNumbers.OrderByDescending(pn => pn.TotalDuration))
            {
                // skip phone numbers with no calls
                if (phoneNumber.TotalCalls == 0) continue;

                // add the record to the worksheet
                worksheet.Cells[row, 1] = phoneNumber.id;
                worksheet.Cells[row, 2] = phoneNumber.FormatedPhoneNumber();
                worksheet.Cells[row, 3] = phoneNumber.TotalCalls;
                worksheet.Cells[row, 4] = phoneNumber.FormatedDuration(phoneNumber.TotalDuration);
                worksheet.Cells[row, 5] = phoneNumber.FormatedDuration(Convert.ToInt32(phoneNumber.AverageDuration()));
                worksheet.Cells[row, 6] = phoneNumber.InboundCalls;
                worksheet.Cells[row, 7] = phoneNumber.FormatedDuration(phoneNumber.InboundDuration);
                worksheet.Cells[row, 8] = phoneNumber.OutboundCalls;
                worksheet.Cells[row, 9] = phoneNumber.FormatedDuration(phoneNumber.OutboundDuration);
                worksheet.Cells[row, 10] = dbHandle.GetPharmacyName(phoneNumber.PharmacyID);


                worksheet.Range["A" + row, "J" + row].Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                worksheet.Range["A" + row, "J" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;


                // move to the next row
                row++;


                // update the progress bar
                ProgressBarUtility.WriteProgressBar((row * 100) / phoneNumbers.Count, true);
            }
            
            worksheet = FormatWorksheet(worksheet, row);
            
            // finish progress bar
            ProgressBarUtility.WriteProgressBar(100, true);
            Console.WriteLine("\n");

            return worksheet;
        }


        internal static Worksheet AddHeader(Worksheet worksheet)
        {
            // add the header row to the worksheet
            worksheet.Cells[1, 1] = "id";
            worksheet.Cells[1, 2] = "Number";
            worksheet.Cells[1, 3] = "Total Calls";
            worksheet.Cells[1, 4] = "Duration ↓"; // list is sorted
            worksheet.Cells[1, 5] = "Average";
            worksheet.Cells[1, 6] = "Inbound";
            worksheet.Cells[1, 7] = "Duration";
            worksheet.Cells[1, 8] = "Outbound";
            worksheet.Cells[1, 9] = "Duration";
            worksheet.Cells[1, 10] = "Pharmacy Name";

            // format header row
            worksheet.Range["A1", "K1"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range["A1", "J1"].Interior.Color = XlRgbColor.rgbLightSteelBlue;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }


        internal static Worksheet FormatWorksheet(Worksheet worksheet,int row)
        {
            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "I" + row].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // make all duration and ph name colmns align left
            worksheet.Range["D1", "D" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["G1", "G" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["I1", "I" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["J1", "J" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // make phonenumber and average columns wider
            worksheet.Range["B1", "B" + row].ColumnWidth = 16;
            worksheet.Range["E1", "E" + row].ColumnWidth = 16;


            // alternate row colors with smoke
            for (int i = 2; i < row; i++)
            {
                if (i % 2 == 0)
                {
                    worksheet.Range["A" + i, "J" + i].Interior.Color = XlRgbColor.rgbWhiteSmoke;
                }
            }

            return worksheet;
        }

    }
}
