using CallAugger.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities.Validators
{
    static class DateValidator
    {
        public static bool IsDateTime(string date)
        {
            DateTime dateTime;
            if (DateTime.TryParse(date, out dateTime)) return true;

            return false;
        }

        public static bool IsValidDateRangeString(string rangestring)
        {
            string[] splitString = rangestring.Split('-');
            
            if (splitString.Length != 2) return false;
            if (IsDateTime(splitString[0]) == false) return false;
            if (IsDateTime(splitString[1]) == false) return false;

            DateTime startDateTime;
            DateTime endDateTime;

            if (DateTime.TryParse(splitString[0], out startDateTime) && 
                DateTime.TryParse(splitString[1], out endDateTime))
            {
                if (startDateTime <= endDateTime) return true;
            }

            return false;
        }

        public static bool IsValidDateRange(DateRange dateRange)
        {
            if (dateRange.StartDate <= dateRange.EndDate) return true;

            return false;
        }
    }
}
