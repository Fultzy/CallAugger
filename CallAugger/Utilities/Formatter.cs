using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities
{
    internal static class Formatter
    {
        public static string Duration(int duration, int requestedLength = 0)
        {
            var formatedDuration = "";

            TimeSpan time = TimeSpan.FromSeconds(duration);
            var totalHours = time.Days * 24 + time.Hours;

            // Create a duration string
            if (totalHours == 0)
            {
                formatedDuration = $"{time.Minutes}m {time.Seconds}s";
            }
            else
            {
                formatedDuration = $"{totalHours}h {time.Minutes}m {time.Seconds}s";
            }

            // pad the output according to the requested length
            if (requestedLength > 0)
            {
                formatedDuration = PadString(formatedDuration, requestedLength);
            }

            return formatedDuration;
        }


        public static string PhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null || phoneNumber.Length == 0) return "Missing Number";

            string formattedNumber = phoneNumber;

            if (phoneNumber.Length == 10)
            {
                formattedNumber = "(" + phoneNumber.Substring(0, 3) + ") " + phoneNumber.Substring(3, 3) + "-" + phoneNumber.Substring(6, 4);
            }

            return formattedNumber;
        }


        public static string PadString(string input, int requestedLength)
        {
            string padding = "";
            for (int i = input.Length; i < requestedLength; i++)
            {
                padding += " ";
            }

            return $"{padding}{input}";
        }
    }
}
