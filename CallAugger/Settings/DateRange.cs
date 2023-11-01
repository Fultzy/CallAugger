using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CallAugger.Utilities.Validators;

namespace CallAugger.Settings
{
    public class DateRange
    {
        public DateTime StartDate;
        public DateTime EndDate;


        public DateRange()
        {
            if (ConfigurationManager.AppSettings["Current_dateRange"] != "")
            {
                SetRange(GetCurrentDateRange().StartDate, GetCurrentDateRange().EndDate);
            }
            else
            {
                SetDefaultDateRange();
            }
        }

        public void SetDefaultDateRange()
        {
            switch (ConfigurationManager.AppSettings["default_dateRange"].ToLower())
            {
                case "last week":
                    SetToLastWeek();
                    break;
                case "last month":
                    SetToLastMonth();
                    break;
                case "last year":
                    SetToLastYear();
                    break;
                case "this week":
                    SetToThisWeek();
                    break;
                case "this month":
                    SetToThisMonth();
                    break;
                case "this year":
                    SetToThisYear();
                    break;
                case "all time":
                    SetToAllTime();
                    break;
                default:
                    break;
            }
        }

        public bool IsInRange(DateTime date)
        {
            if (date >= StartDate && date <= EndDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Write()
        {
            // write the date range to the console
            Console.WriteLine("Date Range: " + StartDate.ToString("MM/dd/yyyy") + " - " + EndDate.ToString("MM/dd/yyyy"));
        }

        public override string ToString()
        {
            return StartDate.ToString("MM/dd/yyyy") + " - " + EndDate.ToString("MM/dd/yyyy");
        }

        public string ToFileNameString()
        {
            return StartDate.ToString("MM.dd.yy") + "-" + EndDate.ToString("MM.dd.yy");
        }

        public DateRange GetCurrentDateRange()
        {
            string dateRange = ConfigurationManager.AppSettings["Current_dateRange"];
            
            if (dateRange == null) return null;

            return SetFromString(dateRange);
        }

        public DateRange SetFromString(string dateTimeString)
        {
            string[] splitString = dateTimeString.Split('-');
            if (splitString.Length != 2) throw new Exception("Invalid Date Range");
            if (DateValidator.IsDateTime(splitString[0]) == false) throw new Exception("Invalid Date Range Start");
            if (DateValidator.IsDateTime(splitString[1]) == false) throw new Exception("Invalid Date Range End");

            StartDate = DateTime.Parse(splitString[0]).Date;
            EndDate = DateTime.Parse(splitString[1]).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            ConfigurationManager.AppSettings["Current_dateRange"] = ToString();

            return this;
        }

        public DateRange SetRange(DateTime start, DateTime end)
        {

            StartDate = start.Date;
            EndDate = end.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            ConfigurationManager.AppSettings["Current_dateRange"] = ToString();

            return this;
        }


        public DateRange SetToAllTime()
        {
            StartDate = new DateTime(1992, 12, 23);
            EndDate = DateTime.Now;

            return SetRange(StartDate, EndDate);
        }


        public DateRange SetToThisWeek()
        {
            StartDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            EndDate = DateTime.Now;

            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToThisMonth()
        {
            StartDate = DateTime.Now.AddDays(-(int)DateTime.Now.Day + 1);
            EndDate = DateTime.Now;

            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToThisYear()
        {
            StartDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfYear + 1);
            EndDate = DateTime.Now;
            
            return SetRange(StartDate, EndDate);
        }


        public DateRange SetToLastWeek()
        {
            StartDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7);
            EndDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1);

            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToLastMonth()
        {
            StartDate = DateTime.Now.AddMonths(-1).AddDays(-DateTime.Now.Day + 1);
            EndDate = DateTime.Now.AddMonths(-1).AddDays(-DateTime.Now.Day + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month));

            return SetRange(StartDate, EndDate);
        }
        
        public DateRange SetToLastYear()
        {
            StartDate = DateTime.Now.AddYears(-1).AddDays(-DateTime.Now.DayOfYear + 1);

            int daysInYear = 0;
            for (int i = 1; i <= 12; i++)
            {
                daysInYear += DateTime.DaysInMonth(StartDate.Year, i);
            }

            EndDate = DateTime.Now.AddYears(-1).AddDays(-DateTime.Now.DayOfYear + daysInYear);

            return SetRange(StartDate, EndDate);
        }


        public DateRange SetToQuarter1()
        {
            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 3, 31);
        
            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToQuarter2()
        {
            StartDate = new DateTime(DateTime.Now.Year, 4, 1);
            EndDate = new DateTime(DateTime.Now.Year, 6, 30);
        
            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToQuarter3()
        {
            StartDate = new DateTime(DateTime.Now.Year, 7, 1);
            EndDate = new DateTime(DateTime.Now.Year, 9, 30);
            
            return SetRange(StartDate, EndDate);
        }

        public DateRange SetToQuarter4()
        {
            StartDate = new DateTime(DateTime.Now.Year, 10, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            return SetRange(StartDate, EndDate);
        }
    }
}
