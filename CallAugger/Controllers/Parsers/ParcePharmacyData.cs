using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Parsers
{
    internal class ParsePharmacyData : Parse
    {
        public bool IsPrimaryPhoneNumber(PhoneNumber phoneNumber, List<Pharmacy> pharmacies)
        {
            // using the PrimaryPhoneNumber property in Pharmacy, check if the phone number belongs to a pharmacy
            if (pharmacies.Any(ph => ph.PrimaryPhoneNumber == phoneNumber.Number))
            {
                return true;
            }
            else // if the phone number is not a PrimaryPhoneNumber add it to unassigned
            {
                return false;
            }
        }

        public bool IsRegisteredToPharmacy(PhoneNumber phoneNumber, List<Pharmacy> pharmacies)
        {
            // using the PhoneNumbers property in Pharmacy, check if the phone number is already registered to a pharmacy
            if (pharmacies.Any(ph => ph.PhoneNumbers.Contains(phoneNumber)))
            {
                return true;
            }
            else // if the phone number is not a Registered phonenumber add it to unassigned
            {
                return false;
            }
        }
    }
}
