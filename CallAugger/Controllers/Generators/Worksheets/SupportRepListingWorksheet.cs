using CallAugger.Utilities.DataBase;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Generators.Worksheets
{
    internal class SupportRepListingWorksheet
    {
        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {

            Console.WriteLine("\nCreating Support Rep Listing:");

            // this report will list all support reps and their total metrics
            int row = 1;

            // create the header
            worksheet.Cells[row, 1] = "Support Rep";
            worksheet.Cells[row, 2] = "Total Calls";
            worksheet.Cells[row, 3] = "Total Duration";
            worksheet.Cells[row, 4] = "Inbound Calls";
            worksheet.Cells[row, 5] = "Inbound Duration";
            worksheet.Cells[row, 6] = "Outbound Calls";
            worksheet.Cells[row, 7] = "Outbound Duration";
            worksheet.Cells[row, 8] = "> 30m";
            worksheet.Cells[row, 9] = "> 30m %";
            worksheet.Cells[row, 10] = "> 60m";
            worksheet.Cells[row, 11] = "> 60m %";
            worksheet.Cells[row, 12] = "Weekend Calls";
            worksheet.Cells[row, 13] = "Internal Calls";

            // get the list of users
            List<User> users = dbHandle.GetAllUsers();

            // populate the worksheet
            foreach (User user in users.OrderByDescending(user => user.TotalCalls))
            {
                row++;

                worksheet.Cells[row, 1] = user.Name;
                worksheet.Cells[row, 2] = user.TotalCalls;
                worksheet.Cells[row, 3] = user.FormatedDuration(user.TotalDuration);
                worksheet.Cells[row, 4] = user.InboundCalls;
                worksheet.Cells[row, 5] = user.FormatedDuration(user.InboundDuration);
                worksheet.Cells[row, 6] = user.OutboundCalls;
                worksheet.Cells[row, 7] = user.FormatedDuration(user.OutboundDuration);
                worksheet.Cells[row, 8] = user.CallsOver30;
                worksheet.Cells[row, 9] = user.Over30Percentage();
                worksheet.Cells[row, 10] = user.CallsOver60;
                worksheet.Cells[row, 11] = user.Over60Percentage();
                worksheet.Cells[row, 12] = user.WeekendCalls;
                worksheet.Cells[row, 13] = user.InternalCalls;

                // alternate row colors with smoke
                if (row % 2 == 0)
                {
                    worksheet.Range["A" + row, "M" + row].Interior.Color = XlRgbColor.rgbWhiteSmoke;
                }
            }

            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "P" + row].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // set bottom border for all rows
            worksheet.Range["A1", "M" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            // format the header
            Range header = worksheet.Range["A1", "M1"];
            header.Font.Bold = true;
            header.Interior.Color = XlRgbColor.rgbLightSteelBlue;


            // format the data
            Range data = worksheet.Range["A2", "M" + (users.Count + 1)];


            Console.WriteLine("Done!");
            return worksheet;
        }
    }
}
