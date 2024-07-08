using CallAugger.Utilities;
using CallAugger.Utilities.Sqlite;
using Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger.Generators.Worksheets
{
    internal class SupportRepListingWorksheet
    {
        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {
            // set the worksheet name
            worksheet.Name = "User List";

            int row = 1;

            // get the list of users
            List<User> users = dbHandle.GetAllUsers();

            // begin progress bar
            Console.WriteLine("\nCreating Support Rep Listing:", users.Count);
            ProgressBarUtility.WriteProgressBar(0);

            // create the header
            worksheet = CreateHeader(worksheet, row);

            // populate the worksheet
            foreach (User user in users.OrderByDescending(user => user.TotalDuration))
            {
                row++;

                worksheet.Cells[row, 1] = user.Extention;
                worksheet.Cells[row, 2] = user.Name;
                worksheet.Cells[row, 3] = user.TotalCalls;
                worksheet.Cells[row, 4] = user.FormatedDuration(user.TotalDuration);
                worksheet.Cells[row, 5] = user.InboundCalls;
                worksheet.Cells[row, 6] = user.FormatedDuration(user.InboundDuration);
                worksheet.Cells[row, 7] = user.OutboundCalls;
                worksheet.Cells[row, 8] = user.FormatedDuration(user.OutboundDuration);
                worksheet.Cells[row, 9] = user.CallsOver30;
                worksheet.Cells[row, 10] = user.Over30Percentage();
                worksheet.Cells[row, 11] = user.CallsOver60;
                worksheet.Cells[row, 12] = user.Over60Percentage();
                worksheet.Cells[row, 13] = user.WeekendCalls;
                worksheet.Cells[row, 14] = user.InternalCalls;

                // alternate row colors with smoke
                if (row % 2 == 0)
                {
                    worksheet.Range["A" + row, "N" + row].Interior.Color = XlRgbColor.rgbWhiteSmoke;
                }

                // update progress bar
                ProgressBarUtility.WriteProgressBar((row * 100) / users.Count, true);
            }

            // format the worksheet
            worksheet = FormatWorksheet(worksheet, row);

            ProgressBarUtility.WriteProgressBar(100, true);
            Console.WriteLine(" Done!");
            return worksheet;
        }

        private static Worksheet FormatWorksheet(Worksheet worksheet, int row)
        {
            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "P" + row].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // format the header
            Range header = worksheet.Range["A1", "N1"];
            header.Font.Bold = true;
            header.Interior.Color = XlRgbColor.rgbLightSteelBlue;

            return worksheet;
        }

        private static Worksheet CreateHeader(Worksheet worksheet, int row)
        {
            // create the header
            worksheet.Cells[row, 1] = "Ext.";
            worksheet.Cells[row, 2] = "Support Rep";
            worksheet.Cells[row, 3] = "Total Calls";
            worksheet.Cells[row, 4] = "Total Duration ↓"; // list is sorted
            worksheet.Cells[row, 5] = "Inbound Calls";
            worksheet.Cells[row, 6] = "Inbound Duration";
            worksheet.Cells[row, 7] = "Outbound Calls";
            worksheet.Cells[row, 8] = "Outbound Duration";
            worksheet.Cells[row, 9] = "> 30m";
            worksheet.Cells[row, 10] = "> 30m %";
            worksheet.Cells[row, 11] = "> 60m";
            worksheet.Cells[row, 12] = "> 60m %";
            worksheet.Cells[row, 13] = "Weekend Calls";
            worksheet.Cells[row, 14] = "Internal Calls";

            return worksheet;
        }
    }
}
