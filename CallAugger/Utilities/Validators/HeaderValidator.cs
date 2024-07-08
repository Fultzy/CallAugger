using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger.Utilities.Validators
{
    class HeaderValidator
    {
        public static string[] RequiredNextivaHeaders = new string[]
        {
            "Call Type",
            "Transfer User",
            "User Name",
            "Time",
            "Duration",
            "State",
            "From",
            "To"
        };

        public static string[] RequiredCallTrackerHeaders = new string[]
        {
            "Pharmacy Name",
            "NPI #",
            "DEA #",
            "NCPDP #",
            "Address",
            "City",
            "State",
            "Zip",
            "Contact 1 Name",
            "Contact 2 Name",
            "Rx Anniversary",
            "Phone # (no dashes)",
            "Fax # (no dashes)"
        };

        public static void DetermineAndValidate(Dictionary<string, int> header)
        {
            var headerType = DetermineHeaderType(header);

            switch (headerType)
            {
                case "Nextiva":
                    IsValidNextivaHeader(header);
                    break;
                case "CallTracker":
                    IsValidCallTrackerHeader(header);
                    break;
                default:
                    throw new Exception(Logger.Error(Logger.Importing($"Error: Unknown Header Type: {headerType}")));
            }
        }

        public static string DetermineHeaderType(Dictionary<string, int> headers)
        {
            var nextivaHeaders = headers.Keys.Intersect(RequiredNextivaHeaders);
            var callTrackerHeaders = headers.Keys.Intersect(RequiredCallTrackerHeaders);

            if (nextivaHeaders.Count() == RequiredNextivaHeaders.Count())
                return "Nextiva";
            else if (callTrackerHeaders.Count() == RequiredCallTrackerHeaders.Count())
                return "CallTracker";
            else
                throw new Exception(Logger.Error(Logger.Importing("Error: Unknown Header Type")));
        }

        public static bool IsValidNextivaHeader(Dictionary<string, int> headers)
        {
            CheckNullHeaders(headers);
            CheckMissingHeaders(headers, RequiredNextivaHeaders);
            CheckDuplicateHeaders(headers);

            return true;
        }

        public static bool IsValidCallTrackerHeader(Dictionary<string, int> headers)
        {
            CheckNullHeaders(headers);
            CheckMissingHeaders(headers, RequiredCallTrackerHeaders);
            CheckDuplicateHeaders(headers);

            return true;
        }

        public static void CheckNullHeaders(Dictionary<string, int> headers)
        {
            if (headers == null) throw new Exception(Logger.Error(Logger.Importing("Error: Null headers")));
            if (headers.Count == 0) throw new Exception(Logger.Error(Logger.Importing("Error: Empty headers")));

            var nullHeaders = headers.Where(x => x.Key == null).Select(x => x.Value);
            if (nullHeaders.Any())
            {
                var errorMessage = $"Error: Null headers: {string.Join(", ", nullHeaders)}";
                throw new Exception(Logger.Error(Logger.Importing(errorMessage)));
            }
        }

        public static void CheckMissingHeaders(Dictionary<string, int> headers, string[] requiredHeaders)
        {
            var missingHeaders = requiredHeaders.Except(headers.Keys);
            if (missingHeaders.Any())
            {
                var errorMessage = $"Error: Missing required headers: {string.Join(", ", missingHeaders)}";
                throw new Exception(Logger.Error(Logger.Importing(errorMessage)));
            }
        }

        public static void CheckDuplicateHeaders(Dictionary<string, int> headers)
        {
            var duplicateHeaders = headers.GroupBy(x => x.Value).Where(x => x.Count() > 1).Select(x => x.Key);
            if (duplicateHeaders.Any())
            {
                var errorMessage = $"Error: Duplicate headers: {string.Join(", ", duplicateHeaders)}";
                throw new Exception(Logger.Error(Logger.Importing(errorMessage)));
            }
        }
    }
}
