using CallAugger.Utilities.DataBase;
using CallAugger.Utilities;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data.SQLite;
using System.Configuration;

namespace CallAugger.Controllers.Generators.WorkSheets
{
    internal class UnassignedPhoneNumberWorksheet
    {
        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {
            ///////////////////////////////////////////////////////////////
            // This method will create the Call Report Worksheet. This
            // worksheet will contain all of the call records for the
            // specified time span. 
            ///////////////////////////////////////////////////////////////

            // set the worksheet name
            worksheet.Name = "Unassigned Number Report";

            // get all PhoneNumbers
            List<PhoneNumber> phoneNumbers = dbHandle.GetUnassignedPhoneNumbers();

            // begin progress bar 
            Console.WriteLine("\nCreating Unassigned Number Report, {0} Callers:", phoneNumbers.Count());
            ProgressBarUtility.WriteProgressBar(0);
            
            // add the header row
            worksheet = AddHeader(worksheet);
            
            // set counters
            int row = 2;
            int pnCount = 0;

            using (SQLiteConnection connection = new SQLiteConnection
                (ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                connection.Open();

                // enumerate the list and add each record
                foreach (var phoneNumber in phoneNumbers.OrderByDescending(pn => pn.TotalDuration))
                {
                    // skip phone numbers with no calls
                    if (phoneNumber.TotalCalls == 0) continue;

                    // add the record to the worksheet
                    worksheet = AddPhoneNumberRow(row, phoneNumber, worksheet);
                    worksheet = FormatPhoneNumberRow(row, phoneNumber, worksheet);

                    // move to the next row
                    row++;

                    // If there are call records for this phone number, list them
                    if (phoneNumber.CallRecords.Count > 0)
                    {
                        int crCount = 0;
                        worksheet = AddCallRecordTableHeader(row, phoneNumber, worksheet);
                        var callRecords = dbHandle.GetCallRecordsForPhoneNumber(connection, phoneNumber.id);

                        // 07/01/2023 - 07/20/2023

                        // List top 5 calls
                        foreach (var call in callRecords.OrderByDescending(cr => cr.Duration).Take(5))
                        {
                            row++;
                            crCount++;

                            worksheet.Cells[row, 5] = call.Time;
                            worksheet.Cells[row, 6] = call.FormatedDuration(call.Duration);
                            worksheet.Cells[row, 7] = call.CallType;
                            worksheet.Cells[row, 8] = call.UserName;

                            // make the row light grey if row is even
                            if (row % 2 == 0)
                            {
                                worksheet.Range["A" + row, "I" + row].Interior.Color = XlRgbColor.rgbWhiteSmoke;
                            }
                        }

                        // make the whole call record table all borders
                        worksheet.Range["E" + (row - crCount), "H" + (row)].Borders.LineStyle = XlLineStyle.xlContinuous;
                    }

                    pnCount++;
                    row += 2;


                    // update the progress bar
                    ProgressBarUtility.WriteProgressBar((pnCount * 100) / phoneNumbers.Count, true);
                }


            }

            worksheet = FormatWorksheet(worksheet, row);

            // finish progress bar
            ProgressBarUtility.WriteProgressBar(100, true);
            Console.WriteLine("\n");

            return worksheet;
        }

        private static Worksheet AddCallRecordTableHeader(int row, PhoneNumber phoneNumber, Worksheet worksheet)
        {
            // add header for call records
            worksheet.Cells[row, 5] = "DateTime";
            worksheet.Cells[row, 6] = "Duration ↓"; // list is sorted
            worksheet.Cells[row, 7] = "Type";
            worksheet.Cells[row, 8] = "User";

            // make this mini header light blue
            worksheet.Range["E" + row, "H" + row].Interior.Color = XlRgbColor.rgbAliceBlue;

            return worksheet;

        }

        private static Worksheet FormatPhoneNumberRow(int row, PhoneNumber phoneNumber, Worksheet worksheet)
        {
            // change PhoneNumber row color and add borders to top and bottom
            // this color should be #D9E1F2 but thats red, the inverse is grey #F2E1D9 
            Color rgbBlueGreyColor = ColorTranslator.FromHtml("#F2E1D9"); // red color
            int argbBlueGreyColor = rgbBlueGreyColor.ToArgb();

            worksheet.Range["A" + row, "I" + row].Interior.Color = argbBlueGreyColor;

            worksheet.Range["A" + row, "I" + row].Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            worksheet.Range["A" + row, "I" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            return worksheet;
        }


        private static Worksheet AddPhoneNumberRow(int row, PhoneNumber phoneNumber, Worksheet worksheet)
        {
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

            // format header row
            worksheet.Range["A1", "I1"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range["A1", "I1"].Interior.Color = XlRgbColor.rgbLightSteelBlue;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }


        internal static Worksheet FormatWorksheet(Worksheet worksheet, int row)
        {
            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "I" + row].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // make phoneNumber, average, and outbound duration column and make it wider
            worksheet.Range["B1", "B" + row].ColumnWidth = 16;
            worksheet.Range["E1", "E" + row].ColumnWidth = 16;
            worksheet.Range["I1", "I" + row].ColumnWidth = 15;

            return worksheet;
        }
    }
}
