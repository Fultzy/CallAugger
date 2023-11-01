using CallAugger.Settings;
using CallAugger.Utilities;
using CallAugger.Utilities.DataBase;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CallAugger.Controllers.Generators
{
    internal class SupportMetricsWorksheet
    {
        internal static User AverageUser = null;
        internal static User TotalUser = null;

        public static Worksheet Create(SQLiteHandler dbHandle, Worksheet worksheet)
        {
            ///////////////////////////////////////////////////////////////
            // This method will create the Support Metric Report Worksheet.
            // This worksheet will list out all metrics for support within
            // the specified time span. it will list out company wide metrics
            // as well as metrics for each individual support agent.

            // This report will be very different from the other reports
            // it will show staff wide metrics and rankings of supprt reps
            // based on their metrics and showing several categories.
            ///////////////////////////////////////////////////////////////


            /////////////////////////////////////////// WorkSheet Setup

            // setup worksheet
            worksheet.Name = "Support Metric Report";

            // set the row counter
            int row = 1;

            // get the list of users
            List<User> users = dbHandle.GetAllUsers();
            DateRange dateRange = new DateRange();

            // Create average and total Users
            AverageUser = CreateAverageUser(dbHandle);
            TotalUser = CreateTotalUser(dbHandle);


            ///////////////////////////////////////////// get user input
            Console.WriteLine("\nCreating Support Metric Report:");

            int repCount = 0;
            Console.Write("\n  How many support reps to show?: ");
            while (!int.TryParse(Console.ReadLine(), out repCount) || repCount <= 0)
            {
                Console.Write("\nInvalid input. Please enter a number greater than 0: ");
            }

            int rankCount = 0;
            Console.Write("\n  How many ranked support reps?: ");
            while (!int.TryParse(Console.ReadLine(), out rankCount) || rankCount <= 0)
            {
                Console.Write("\nInvalid input. Please enter a number greater than 0: ");
            }


            // begin progress bar
            Console.WriteLine("\n");
            ProgressBarUtility.WriteProgressBar(0);


            /////////////////////////////////////////// Begin Data Entry
            // add date range to the worksheet
            worksheet.Cells[row, 1] = "Date Range:";
            worksheet.Cells[row, 2] = dateRange.ToString();

            // Format DateRange
            worksheet.Range["B1", "B2"].Font.Bold = true;
            worksheet.Range["B1"].HorizontalAlignment = XlHAlign.xlHAlignRight;
            worksheet.Range["B2"].HorizontalAlignment = XlHAlign.xlHAlignLeft;



            row++;
            row++;

            /////////// Table 1 - Company Wide Metrics
            // list the support wide metrics here
            worksheet = AddSupportHeader(worksheet, row);
            row++;
            worksheet = AddSupportWideMetrics(dbHandle, worksheet, row);


            row++;
            row++;
            row++;

            /////////// Table 2 - User Metrics
            // list each support agent and their metrics here
            worksheet = AddUserHeader(worksheet, row);
            row++;

            // sort then add total then average user to the front
            users = users.OrderByDescending(u => u.TotalCalls).ToList();
            users.Insert(0, TotalUser);
            users.Insert(1, AverageUser);

            List<string> ignoreList = ConfigurationManager.AppSettings["ignore_users"].Split(',').ToList();

            // remove all the ignored users
            users.RemoveAll(user => ignoreList.Any(ignored => ignored.Trim() == user.Name));


            foreach (var user in users.Take(repCount + 2)) // adding two for total and average users
            {

                worksheet = AddUserMetrics(user, worksheet, row);
                row++;

                // update the progress bar
                ProgressBarUtility.WriteProgressBar((row * 100) / users.Count, true);
            }


            row++;

            /////////// Table 3 - Average Lineup
            // list the average lineup here
            worksheet = AddAverageLineupHeader(worksheet, row);
            row++;

            worksheet = AddAverageLineupMetrics(users, worksheet, row, rankCount);



            // finish the progress bar
            ProgressBarUtility.WriteProgressBar(100, true);
            Console.WriteLine("\n");
            worksheet = FormatWorksheet(worksheet, row, rankCount);

            

            return worksheet;
        }

        ///////// FIRST TABLE
        private static Worksheet AddSupportHeader(Worksheet worksheet, int row)
        {
            // First header row on worksheet
            worksheet.Cells[row, 1] = "Tech Support";
            worksheet.Cells[row, 2] = "Tickets";
            worksheet.Cells[row, 3] = "Wkd Tickets";
            worksheet.Cells[row, 4] = "Adj Tickets";
            worksheet.Cells[row, 5] = "Calls ↓"; // list is sorted
            worksheet.Cells[row, 6] = "Wkd Calls";
            worksheet.Cells[row, 7] = "Internal Calls";
            worksheet.Cells[row, 8] = "Adj Calls";
            worksheet.Cells[row, 9] = "Calls/Tickets";
            worksheet.Cells[row, 10] = "Avg Call Time";
            worksheet.Cells[row, 11] = "Total Ph Time";
            worksheet.Cells[row, 12] = "Calls > 30m";
            worksheet.Cells[row, 13] = "> 30%";
            worksheet.Cells[row, 14] = "Calls > 1h";
            worksheet.Cells[row, 15] = "> 1h%";
            worksheet.Cells[row, 16] = "Notes";

            // format header row
            worksheet.Range[$"A{row}", $"P{row}"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range[$"A{row}", $"P{row}"].Interior.Color = XlRgbColor.rgbLightSteelBlue;
            worksheet.Range["A" + row, "P" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }

        private static Worksheet AddSupportWideMetrics(SQLiteHandler dbHandle, Worksheet worksheet, int row)
        {
            // there will be two rows added, one for total numbers across all support agents,
            // the other is for the averages across all support agents
            List<CallRecord> callRecords = dbHandle.GetAllCallRecords();
            List<User> users = dbHandle.GetAllUsers();

            AddUserMetrics(TotalUser, worksheet, row);
            row++;
            AddUserMetrics(AverageUser, worksheet, row);

            // alternate row colors with smoke
            if (row % 2 == 0)
            {
                worksheet.Range["A" + row, "P" + row].Interior.Color = XlRgbColor.rgbWhiteSmoke;
            }

            return worksheet;
        }

        


        ///////// SECOND TABLE
        private static Worksheet AddUserHeader(Worksheet worksheet, int row)
        {
            worksheet.Cells[row, 1] = "Tech Support";
            worksheet.Cells[row, 2] = "Tickets";
            worksheet.Cells[row, 3] = "Wkd Tickets";
            worksheet.Cells[row, 4] = "Adj Tickets";
            worksheet.Cells[row, 5] = "Calls ↓"; // list is sorted
            worksheet.Cells[row, 6] = "Wkd Calls";
            worksheet.Cells[row, 7] = "Internal Calls";
            worksheet.Cells[row, 8] = "Adj Calls";
            worksheet.Cells[row, 9] = "Calls/Tickets";
            worksheet.Cells[row, 10] = "Avg Call Time";
            worksheet.Cells[row, 11] = "Total Ph Time";
            worksheet.Cells[row, 12] = "Calls > 30m";
            worksheet.Cells[row, 13] = "> 30%";
            worksheet.Cells[row, 14] = "Calls > 1h";
            worksheet.Cells[row, 15] = "> 1h%";
            worksheet.Cells[row, 16] = "Notes";

            // format header row
            worksheet.Range[$"A{row}", $"P{row}"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range[$"A{row}", $"P{row}"].Interior.Color = XlRgbColor.rgbLightSteelBlue;

            // add bottom border to this header
            worksheet.Range["A" + row, "P" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }

        private static Worksheet AddUserMetrics(User user, Worksheet worksheet, int row)
        {
            // add the record to the worksheet
            worksheet.Cells[row, 1] = user.NameL();
            worksheet.Cells[row, 5] = user.TotalCalls;
            worksheet.Cells[row, 6] = user.WeekendCalls;
            worksheet.Cells[row, 7] = user.InternalCalls;
            worksheet.Cells[row, 8] = user.AdjustedCalls();
            worksheet.Cells[row, 10] = user.FormatedDuration(Convert.ToInt32(user.AverageDuration()));
            worksheet.Cells[row, 11] = user.FormatedDuration(user.TotalDuration);
            worksheet.Cells[row, 12] = user.CallsOver30;
            worksheet.Cells[row, 13] = user.Over30Percentage();
            worksheet.Cells[row, 14] = user.CallsOver60;
            worksheet.Cells[row, 15] = user.Over60Percentage();

            worksheet.Range["A" + row, "O" + row].Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            worksheet.Range["A" + row, "O" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            // format the row
            // if the username is average make the row bold
            if (user.Name == "-- AVERAGE --")
            {
                worksheet.Range["A" + row, "P" + row].Font.Bold = true;
            }

            // alternate row colors with smoke
            if (row % 2 == 0)
            {
                worksheet.Range["A" + row, "P" + row].Interior.Color = XlRgbColor.rgbWhiteSmoke;
            }

            return worksheet;
        }




        ///////// THIRD TABLE
        private static Worksheet AddAverageLineupHeader(Worksheet worksheet, int row)
        {
            worksheet.Cells[row, 1] = "Rankings";
            worksheet.Cells[row, 3] = "Adj Tickets";
            worksheet.Cells[row, 5] = "Adj Calls";
            worksheet.Cells[row, 7] = "Calls/Ticket";
            worksheet.Cells[row, 9] = "Avg Call Time";
            worksheet.Cells[row, 11] = "Total Ph Time";
            worksheet.Cells[row, 13] = " > 30m %";
            worksheet.Cells[row, 15] = " > 1h %";

            // format header row
            worksheet.Range[$"A{row}", $"P{row}"].Font.Bold = true;

            // Make the background color of the header light blue
            worksheet.Range[$"A{row}", $"O{row}"].Interior.Color = XlRgbColor.rgbLightSteelBlue;
            worksheet.Range["A" + row, "O" + row].Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

            // freeze the header row
            worksheet.Application.ActiveWindow.SplitRow = 1;
            worksheet.Application.ActiveWindow.FreezePanes = true;

            return worksheet;
        }

        private static Worksheet AddAverageLineupMetrics(List<User> users, Worksheet worksheet, int row, int rankCount)
        {
            // this table is going to display data in columns. each column will be a different metric and
            // will be sorted vertically by the metric with that users name to the left. 
            // there will be a sudo user added to the list that will be the average of all the users.

            // remove the total user
            users.Remove(TotalUser);




            // setup sorted lists for each metric, adjusted calls, average call time, total phone time, calls over 30, calls over 30 %, calls over 60, calls over 60 %
            List<User> adjCalls = users.OrderByDescending(u => u.AdjustedCalls()).ToList();
            List<User> avgCallTime = users.OrderBy(u => u.AverageDuration()).ToList();
            List<User> totalPhoneTime = users.OrderByDescending(u => u.TotalDuration).ToList();
            List<User> callsOver30Percent = users.OrderBy(u => u.Over30Percentage()).ToList();
            List<User> callsOver60Percent = users.OrderBy(u => u.Over60Percentage()).ToList();

            
            int rank = 0;
            foreach (User user in adjCalls.Take(rankCount))
            {
                if (user.Name == "-- AVERAGE --") worksheet.Range["D" + (row + rank), "E" + (row + rank)].Font.Bold = true;


                worksheet.Cells[row + rank, 4] = user.NameL();
                worksheet.Cells[row + rank, 5] = user.AdjustedCalls();
                rank++;

            }
            


            rank = 0;
            foreach (User user in avgCallTime.Take(rankCount))
            {
                if (user.Name == "-- AVERAGE --") worksheet.Range["H" + (row + rank), "I" + (row + rank)].Font.Bold = true;

                worksheet.Cells[row + rank, 8] = user.NameL();
                worksheet.Cells[row + rank, 9] = user.FormatedDuration(Convert.ToInt32(user.AverageDuration()));
                rank++;

            }


            rank = 0;
            foreach (User user in totalPhoneTime.Take(rankCount))
            {
                if (user.Name == "-- AVERAGE --") worksheet.Range["J" + (row + rank), "K" + (row + rank)].Font.Bold = true;

                worksheet.Cells[row + rank, 10] = user.NameL();
                worksheet.Cells[row + rank, 11] = user.FormatedDuration(user.TotalDuration);
                rank++;

            }


            rank = 0;
            foreach (User user in callsOver30Percent.Take(rankCount))
            {
                if (user.Name == "-- AVERAGE --") worksheet.Range["L" + (row + rank), "M" + (row + rank)].Font.Bold = true;

                worksheet.Cells[row + rank, 12] = user.NameL();
                worksheet.Cells[row + rank, 13] = user.Over30Percentage();
                rank++;

            }


            rank = 0;
            foreach (User user in callsOver60Percent.Take(rankCount))
            {
                if (user.Name == "-- AVERAGE --") worksheet.Range["N" + (row + rank), "O" + (row + rank)].Font.Bold = true;

                worksheet.Cells[row + rank, 14] = user.NameL();
                worksheet.Cells[row + rank, 15] = user.Over60Percentage();
                rank++;

            }


            worksheet = FormatLineupTable(worksheet, row, rankCount);
            
            return worksheet;
        }

        private static Worksheet FormatLineupTable(Worksheet worksheet, int row, int rankCount)
        {
            // format the rank table
            worksheet.Range["A" + row, "P" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // make the name columns align right
            worksheet.Range["D" + (row - 1), "D" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignRight;
            worksheet.Range["H" + (row - 1), "H" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignRight;
            worksheet.Range["J" + (row - 1), "J" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignRight;
            worksheet.Range["L" + (row - 1), "L" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignRight;
            worksheet.Range["N" + (row - 1), "N" + (row + rankCount)].HorizontalAlignment = XlHAlign.xlHAlignRight;


            // iterate through rows starting at row
            for (int i = 0; i < rankCount; i++)
            {
                // Ranking numbers
                worksheet.Cells[(i + row), 1] = i + 1;

                // alternate row colors with smoke
                if (i % 2 == 0)
                {
                    worksheet.Range["A" + (i + row), "P" + (i + row)].Interior.Color = XlRgbColor.rgbWhiteSmoke;
                }
            }

            return worksheet;
        }




        ///////// formatting and helpers
        private static User CreateTotalUser(SQLiteHandler dbHandle)
        {
            User totalUser = new User();
            List<User> users = dbHandle.GetAllUsers();
            List<CallRecord> callRecords = dbHandle.GetAllCallRecords();

            // add all the call records to the total user
            totalUser.Name = "-- TOTAL --";    
            totalUser.AddCalls(callRecords);

            /*
            // get the average metrics
            int totalCalls = 0;
            int inboundCalls = 0;
            int outboundCalls = 0;

            int totalPhoneTime = 0;
            int inboundPhoneTime = 0;
            int outboundPhoneTime = 0;

            int callsOver30 = 0;
            int callsOver60 = 0;

            int WeekendCalls = 0;
            int InternalCalls = 0;

            foreach (var user in users)
            {
                // add total calls
                totalCalls += user.TotalCalls;
                inboundCalls += user.InboundCalls;
                outboundCalls += user.OutboundCalls;

                // add timing
                totalPhoneTime += user.TotalDuration;
                inboundPhoneTime += user.InboundDuration;
                outboundPhoneTime += user.OutboundDuration;

                InternalCalls += user.InternalCalls;
                WeekendCalls += user.WeekendCalls;


                foreach (var call in user.CallRecords)
                {
                    if (call.Duration > 1800) callsOver30++;
                    if (call.Duration > 3600) callsOver60++;
                }
            }

            // set the Total user metrics

            totalUser.TotalCalls = totalCalls;
            totalUser.InboundCalls = inboundCalls;
            totalUser.OutboundCalls = outboundCalls;

            totalUser.TotalDuration = totalPhoneTime;
            totalUser.InboundDuration = inboundPhoneTime;
            totalUser.OutboundDuration = outboundPhoneTime;

            totalUser.WeekendCalls = WeekendCalls;

            totalUser.CallsOver30 = callsOver30;
            totalUser.CallsOver60 = callsOver60;
            */
            return totalUser;
        }

        private static User CreateAverageUser(SQLiteHandler dbHandle)
        {   
            List<string> ignoreList = ConfigurationManager.AppSettings["ignore_users"].Split(',').ToList();
            List<User> users = dbHandle.GetAllUsers();
            
            // create new user
            User avgUser = new User();
            avgUser.Name = "-- AVERAGE --";
            
            // get the average metrics
            int userCount = 0;

            int totalCalls = 0;
            int weekendCalls = 0;
            int internalCalls = 0;
            int adjustedCalls = 0;

            int totalPhoneTime = 0;
            int callsOver30 = 0;
            int callsOver60 = 0;

            // add from all users
            foreach (var user in users)
            {
                if (user.Name == "-- TOTAL --") continue; // skip the total user
                if (ignoreList.Any(ignored => ignored.Trim() == user.Name)) continue; // skip the ignored users

                userCount++;

                // add total calls
                totalCalls += user.TotalCalls;
                weekendCalls += user.WeekendCalls;
                internalCalls += user.InternalCalls;
                adjustedCalls += user.AdjustedCalls();

                // add timing
                totalPhoneTime += user.TotalDuration;

                // add calls over 30 and 60 minutes
                callsOver30 += user.CallsOver30;
                callsOver60 += user.CallsOver60;
            }


            avgUser.TotalCalls = totalCalls / userCount;
            avgUser.WeekendCalls = weekendCalls / userCount;
            avgUser.InternalCalls = internalCalls / userCount;
            avgUser.TotalDuration = totalPhoneTime / userCount;
            avgUser.CallsOver30 = callsOver30 / userCount;
            avgUser.CallsOver60 = callsOver60 / userCount;

            return avgUser;
        }

        private static Worksheet FormatWorksheet(Worksheet worksheet, int row, int rankCount)
        {
            // format columns to fit
            worksheet.Columns.AutoFit();

            // center align the entire sheet
            worksheet.Range["A1", "P" + (row - 1)].HorizontalAlignment = XlHAlign.xlHAlignCenter;

            // set the daterange column aligh left
            worksheet.Range["B1"].HorizontalAlignment = XlHAlign.xlHAlignLeft;

            // make all columns wider
            worksheet.Range["A1", "P1"].ColumnWidth = 14;

            // make the notes column wide enough for the user to make notes
            worksheet.Range["P1", "P" + (row - 1)].ColumnWidth = 70;

            // left alight the whole sheet
            worksheet.Columns["A"].HorizontalAlignment = XlHAlign.xlHAlignLeft;

            return worksheet;
        }

        private static string numToLetter(int num)
        {
            string letter = "";
            switch (num)
            {
                case 1:
                    letter = "A";
                    break;
                case 2:
                    letter = "B";
                    break;
                case 3:
                    letter = "C";
                    break;
                case 4:
                    letter = "D";
                    break;
                case 5:
                    letter = "E";
                    break;
                case 6:
                    letter = "F";
                    break;
                case 7:
                    letter = "G";
                    break;
                case 8:
                    letter = "H";
                    break;
                case 9:
                    letter = "I";
                    break;
                case 10:
                    letter = "J";
                    break;
                case 11:
                    letter = "K";
                    break;
                case 12:
                    letter = "L";
                    break;
                case 13:
                    letter = "M";
                    break;
                case 14:
                    letter = "N";
                    break;
                case 15:
                    letter = "O";
                    break;
                case 16:
                    letter = "P";
                    break;
                case 17:
                    letter = "Q";
                    break;
                case 18:
                    letter = "R";
                    break;
                case 19:
                    letter = "S";
                    break;
                case 20:
                    letter = "T";
                    break;
                case 21:
                    letter = "U";
                    break;
                case 22:
                    letter = "V";
                    break;
                case 23:
                    letter = "W";
                    break;
                case 24:
                    letter = "X";
                    break;
                case 25:
                    letter = "Y";
                    break;
                case 26:
                    letter = "Z";
                    break;
                default:
                    letter = "A";
                    break;
            }

            return letter;
        }
    }
}
