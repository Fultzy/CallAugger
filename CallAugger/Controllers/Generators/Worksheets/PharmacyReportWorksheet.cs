using CallAugger.Utilities;
using CallAugger.Utilities.Sqlite;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace CallAugger.Generators.Worksheets
{
    internal class PharmacyReportWorksheet
    {
        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {
            ///////////////////////////////////////////////////////////////
            // This method will create the Pharmacy Report Worksheet.
            // This worksheet will list out details about the call records
            // associated to all of the phonenumbers associated with each
            // pharmacy. 
            ///////////////////////////////////////////////////////////////

            // set the worksheet name
            worksheet.Name = "Pharmacy Report";

            worksheet = AddHeader(worksheet);

            // set the row counter
            int row = 2;
            int phs = 0;

            // get all Users
            List<Pharmacy> pharmacies = dbHandle.GetAllPharmacies();

            // remove pharmacies that have no calls
            pharmacies.RemoveAll(p => p.TotalCalls == 0);

            // begin progress bar
            Console.WriteLine("\nCreating Pharmacy Report, {0} Pharmacies:", pharmacies.Count);
            ProgressBarUtility.WriteProgressBar(0);

            // enumerate the list and add each record
            foreach (var pharmacy in pharmacies.OrderByDescending(pn => pn.TotalDuration))
            {
                // skip pharmacies with no calls
                if (pharmacy.TotalCalls == 0) continue;

                // change PhoneNumber row color and add borders to top and bottom
                // this color should be #D9E1F2 but thats red, the inverse is grey #F2E1D9 
                Color rgbBlueGreyColor = ColorTranslator.FromHtml("#F2E1D9"); // red color
                int argbBlueGreyColor = rgbBlueGreyColor.ToArgb();

                worksheet.Range["A" + row, "N" + row].Interior.Color = argbBlueGreyColor;

                // add borders to the top of this row
                worksheet.Range["A" + row, "N" + row].Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;

                // Add pharmacy details
                worksheet.Cells[row, 1] = pharmacy.id;
                worksheet.Cells[row, 2] = pharmacy.Name;


                // if there is more than 1 phone number 
                if (pharmacy.PhoneNumbers.Count() > 1)
                {
                    worksheet.Cells[row, 6] = "Total:";
                    worksheet.Cells[row, 7] = pharmacy.TotalCalls;
                    worksheet.Cells[row, 8] = pharmacy.FormattedTotalDuration();
                    worksheet.Cells[row, 9] = pharmacy.FormattedAverageDuration();
                    worksheet.Cells[row, 10] = pharmacy.InboundCalls;
                    worksheet.Cells[row, 11] = pharmacy.FormattedInboundDuration();
                    worksheet.Cells[row, 12] = pharmacy.OutboundCalls;
                    worksheet.Cells[row, 13] = pharmacy.FormattedOutboundDuration();
                }

                worksheet.Cells[row, 14] = pharmacy.Anniversary;
                
                // Go down a row and add more pharmacy details
                row++;
                
                worksheet.Cells[row, 3] = pharmacy.Npi;
                worksheet.Cells[row, 4] = pharmacy.Ncpdp;
                worksheet.Cells[row, 5] = pharmacy.Dea;

                foreach (PhoneNumber phoneNumber in pharmacy.PhoneNumbers.OrderByDescending(ph => ph.TotalDuration))
                {
                    if (phoneNumber.Number == null) continue;

                    // add the Phone number Details to the worksheet
                    worksheet.Cells[row, 6] = phoneNumber.FormattedPhoneNumber();
                    worksheet.Cells[row, 7] = phoneNumber.TotalCalls;
                    worksheet.Cells[row, 8] = phoneNumber.FormattedTotalDuration();
                    worksheet.Cells[row, 9] = phoneNumber.FormattedAverageDuration();

                    worksheet.Cells[row, 10] = phoneNumber.InboundCalls;
                    worksheet.Cells[row, 11] = phoneNumber.FormattedInboundDuration();
                    
                    worksheet.Cells[row, 12] = phoneNumber.OutboundCalls;
                    worksheet.Cells[row, 13] = phoneNumber.FormattedOutboundDuration();

                    row++;
                }
                phs++;


                // update the progress bar
                ProgressBarUtility.WriteProgressBar((phs * 100) / pharmacies.Count, true);
            }

            worksheet = FormatWorksheet(worksheet, row);

            // finish progress bar
            ProgressBarUtility.WriteProgressBar(100, true);
            Console.WriteLine(" Done!");
            return worksheet;
        }


        private static Worksheet AddHeader(Worksheet worksheet)
        {
            // add the header row to the worksheet
            worksheet.Cells[1, 1] = "id";
            worksheet.Cells[1, 2] = "Pharmacy Name";
            worksheet.Cells[1, 3] = "Npi";
            worksheet.Cells[1, 4] = "Ncpdp";
            worksheet.Cells[1, 5] = "Dea";
            worksheet.Cells[1, 6] = "Number";
            worksheet.Cells[1, 7] = "Total Calls";
            worksheet.Cells[1, 8] = "Duration ↓"; // List is sorted
            worksheet.Cells[1, 9] = "Average";
            worksheet.Cells[1, 10] = "Inbound";
            worksheet.Cells[1, 11] = "Duration";
            worksheet.Cells[1, 12] = "Outbound";
            worksheet.Cells[1, 13] = "Duration";
            worksheet.Cells[1, 14] = "Anniversary";

            // format header row
            worksheet.Range["A1", "N1"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range["A1", "N1"].Interior.Color = XlRgbColor.rgbLightSteelBlue;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }


        private static Worksheet FormatWorksheet(Worksheet worksheet, int row)
        {
            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "L" + row].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // left align the three duration and pharmacy name columns
            worksheet.Range["B1", "B" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["H1", "H" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["K1", "K" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;
            worksheet.Range["M1", "N" + row].HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // make the Pharmacy name column shorter
            worksheet.Range["B1", "B" + row].ColumnWidth = 14;

            // make the Phone number and Average columns wider
            worksheet.Range["F1", "F" + row].ColumnWidth = 18;
            worksheet.Range["I1", "I" + row].ColumnWidth = 12;


            return worksheet;
        }

    }
}
