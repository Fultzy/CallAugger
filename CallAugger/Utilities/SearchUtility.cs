using CallAugger.Settings;
using CallAugger.Utilities.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger.Utilities
{
    public static class SearchUtilities
    {
        // this method will take in an input and will try to match it against a list of pharmacy properties
        public static List<Pharmacy> ListMatchingPharmacies(string input, List<Pharmacy> listToSearch, int resultCount)
        {
            if (input == "") 
                return listToSearch.OrderByDescending(ph => ph.TotalDuration).Take(resultCount).ToList();
            else
                return listToSearch.Where(pharmacy =>
                    pharmacy.Name.ToLower().Contains(input.ToLower()) ||
                    pharmacy.Npi.ToLower().Contains(input.ToLower()) ||
                    pharmacy.Dea.ToLower().Contains(input.ToLower()) ||
                    pharmacy.Ncpdp.ToLower().Contains(input.ToLower()) ||
                    pharmacy.Address.ToLower().Contains(input.ToLower()) ||
                    pharmacy.City.ToLower().Contains(input.ToLower()) ||
                    pharmacy.State.ToLower().Contains(input.ToLower()) ||
                    pharmacy.Zip.ToLower().Contains(input.ToLower()) ||
                    pharmacy.ContactName1.ToLower().Contains(input.ToLower()) ||
                    pharmacy.ContactName2.ToLower().Contains(input.ToLower()) ||
                    pharmacy.PrimaryPhoneNumber.ToLower().Contains(input.ToLower())
                ).OrderByDescending(ph => ph.TotalDuration).Take(resultCount).ToList();
        }

        // this method will take in an input and will try to match it against a list of PhoneNumber properties
        public static List<PhoneNumber> ListMatchingPhoneNumbers(string input, List<PhoneNumber> listToSearch, int resultCount)
        {
            if (input == "")
                return listToSearch.OrderByDescending(pn => pn.TotalDuration).Take(resultCount).ToList();
            else
                return listToSearch.Where(phoneNumber =>
                    phoneNumber.Number.ToLower().Contains(input.ToLower()) ||
                    phoneNumber.State.ToLower().Contains(input.ToLower())
                ).OrderByDescending(pn => pn.TotalDuration).Take(resultCount).ToList();
        }

        public static List<PhoneNumber> ListMatchingUnassignedPhoneNumbers(string input, List<PhoneNumber> listToSearch, int resultCount)
        {
            if (input == "")
                return listToSearch.OrderByDescending(pn => pn.TotalDuration).Take(resultCount).ToList();
            else
                return listToSearch.Where(phoneNumber =>
                    phoneNumber.Number.ToLower().Contains(input.ToLower()) ||
                    phoneNumber.State.ToLower().Contains(input.ToLower())
                ).OrderByDescending(pn => pn.TotalDuration).Take(resultCount).ToList();
        }

        internal static List<User> ListMatchingUsers(string searchTerm, List<User> listToSearch, int resultCount)
        {
            if (searchTerm == "")
                return listToSearch.OrderByDescending(usr => usr.TotalDuration).Take(resultCount).ToList();
            else
                return listToSearch.Where(user =>
                    user.Name.ToLower().Contains(searchTerm.ToLower()) ||
                    user.Extention.ToLower().Contains(searchTerm.ToLower()) 
                ).OrderByDescending(usr => usr.TotalDuration).Take(resultCount).ToList();
        }
    }
}
