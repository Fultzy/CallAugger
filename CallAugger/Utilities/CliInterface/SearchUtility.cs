using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Utilities
{
    public class SearchUtilities
    {
        // this method will take in an input and will try to match it against a list of pharmacy properties
        public List<Pharmacy> ListMatchingPharmacies(string input, List<Pharmacy> listToSearch)
        {
            if (input == "") 
                return listToSearch;
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
                ).ToList();
        }

        // this method will take in an input and will try to match it against a list of PhoneNumber properties
        public List<PhoneNumber> ListMatchingPhoneNumbers(string input, List<PhoneNumber> listToSearch)
        {
            List<PhoneNumber> matchingPhoneNumbers = new List<PhoneNumber>();

            foreach (PhoneNumber phoneNumber in listToSearch)
            {
                if (phoneNumber.Number.Contains(input.ToLower())) matchingPhoneNumbers.Add(phoneNumber);
            }

            return matchingPhoneNumbers;
        }

        internal List<User> ListMatchingUsers(string searchTerm, List<User> users)
        {
            List<User> matchingUsers = new List<User>();

            foreach (User user in users)
            {
                if (user.Name.ToLower().Contains(searchTerm.ToLower())) matchingUsers.Add(user);
            }

            return matchingUsers;
        }
    }
}
