using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Parsers
{
    internal class Parse
    {
        ///////////////////////////////////////////////////////////////
        // This Parcing Class requries two Lists of objects to be passed in.
        // The first is a list of CallRecords and the second is a list of Pharmacies.
        // these lists are then used to create a list of ParsedPharmacies and a list of
        // ParsedPhoneNumbers to then be accessable by the rest of the program in 
        // addition to Users and Phonenumbers that do not belong to a pharmacy.

        public List<User>        ParsedUsers            = new List<User>();
        public List<Pharmacy>    ParsedPharmacies       = new List<Pharmacy>();
        public List<PhoneNumber> ParsedPhoneNumbers     = new List<PhoneNumber>();
        public List<PhoneNumber> UnassignedPhoneNumbers = new List<PhoneNumber>();

        public List<PhoneNumber> ParseCallRecordData(List<CallRecord> callRecords)
        {
            var IParser = new ParseCallRecordData();

            foreach (var callRecord in callRecords)
            {
                PhoneNumber caller = IParser.ParseCallRecordCaller(callRecord);
                User        user   = IParser.ParseCallRecordUser(callRecord);

                if (caller.Number.Length > 3)
                {
                    // if this phone number is not already in the list add it
                    if (!ParsedPhoneNumbers.Any(pn => pn.Number == caller.Number))
                    {
                        ParsedPhoneNumbers.Add(caller);
                    }
                    else // find the phone number and add this call record to it
                    {
                        ParsedPhoneNumbers.Find(pn => pn.Number == caller.Number).AddCall(callRecord);
                    }

                    // if this user is not already in the list add it
                    if (!ParsedUsers.Any(u => u.Name == user.Name))
                    {
                        ParsedUsers.Add(user);
                    }
                    else // find the user and add this call record to it
                    {
                        ParsedUsers.Find(u => u.Name == user.Name).AddCall(callRecord);
                    }
                }
                else // if both to and from numbers are user extention numbers then we need to find the other user
                {
                    
                }


                /*
                // if the transfer user isnt Tech Support, Weekend Support or null
                if (callRecord.TransferUser != "Tech Support" && callRecord.TransferUser != "Weekend Support" && callRecord.TransferUser != null)
                {
                    // if this transfer user is not already in the list add it
                    if (!ParsedUsers.Any(u => u.Name == callRecord.TransferUser))
                    {
                        // the issues here is that the transfer user can be a new user and can have been forwarded a 
                        // inbound call such as : 

                        // Inbound call |Joshua Bowman|Matthew Doherty|7/5/2023|1424|Terminating|Yes|From|MI|18102157771|336|

                        // we cant assign this transfer user to the call record because we dont know the second users extention

                        // curently we are adding the call times to both the transfer user and the user that answered. 
                        // this is not fair but also there is no way of knowing at which point the call had split. 

                        
                        User transferUser = new User()
                        {
                            Name = callRecord.TransferUser
                        };

                        transferUser.AddCall(callRecord);
                        ParsedUsers.Add(transferUser);
                    }
                    else // find the transfer user and add this call record to it
                    {
                        ParsedUsers.Find(u => u.Name == callRecord.TransferUser).AddCall(callRecord);
                    }
                }
                */


            }

            return ParsedPhoneNumbers;
        }

        public List<Pharmacy> ParsePharmacyData(List<Pharmacy> pharmacies)
        {
            var IParser = new ParsePharmacyData();
            
            foreach (PhoneNumber phoneNumber in ParsedPhoneNumbers)
            {
                if (IParser.IsRegisteredToPharmacy(phoneNumber, pharmacies))
                {
                    // find the pharmacy that matches this phone number
                    var matchingPharmacy = pharmacies.Find(ph => ph.PhoneNumbers.Contains(phoneNumber));
                    if (matchingPharmacy != null)
                    {
                        matchingPharmacy.AddPhoneNumber(phoneNumber);
                        ParsedPharmacies.Add(matchingPharmacy);
                    }
                }
                else if (IParser.IsPrimaryPhoneNumber(phoneNumber, pharmacies))
                {
                    // find the pharmacy that matches this phone number
                    var matchingPharmacy = pharmacies.Find(ph => ph.PrimaryPhoneNumber == phoneNumber.Number);

                    if (matchingPharmacy != null)
                    {
                        matchingPharmacy.AddPhoneNumber(phoneNumber);
                        ParsedPharmacies.Add(matchingPharmacy);
                    }
                }
                else // if the phone number is not a PrimaryPoneNumber and not registered under a pharmacy add it to unassigned
                {
                    UnassignedPhoneNumbers.Add(phoneNumber);
                }
            }

            return ParsedPharmacies;
        }
    }
}
