using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger
{
    public class Pharmacy
    {

        public int id { get; set; }
        public string Name { get; set; }
        public string Npi { get; set; }
        public string Dea { get; set; }
        public string Ncpdp { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ContactName1 { get; set; }
        public string ContactName2 { get; set; }
        public string PrimaryPhoneNumber { get; set; }

        // The Frist phone number in this is should always be the primary phone number. 
        public List<PhoneNumber> PhoneNumbers = new List<PhoneNumber>();
        public int TotalCalls = 0;
        public int TotalDuration = 0;
        public int InboundCalls = 0;
        public int OutboundCalls = 0;
        public int InboundDuration = 0;
        public int OutboundDuration = 0;

        // writes out all the info for the pharmacy to the console
        // if true is passed to this method, it will also write out the stats for each phone number
        public void WritePharmacyInfo(int callRecordCount = 0)
        {
            string pad = "     ";


            // write out all the info for the pharmacy to the console
            Console.WriteLine(pad + "  ~~~~~~~~~~ " + Name + " ~~~~~~~~~~\n");
                                   
            // write Pharmacy Medical Creds
            Console.WriteLine(pad + "        Npi        Ncpdp        Dea");
            Console.WriteLine(pad + "    " + Npi + " || " + Ncpdp + " || " + Dea);
            Console.WriteLine(pad + "    {0} {1}, {2} {3}\n", Address,  City, State, Zip);

            // write Pharmacy Contact Info
            Console.WriteLine(pad + "  Contact(s): {0} {1}", ContactName1, ContactName2);
            Console.WriteLine(pad + "  Main Phone: {0}\n", PrimaryPhoneNumber); // Install Null for missing

            if (PhoneNumbers.Count > 1)
            {
                // write phrmacy Total call states
                Console.WriteLine(pad + "   Total Calls : " + TotalCalls  +   "     Total Duration : " + FormatedDuration(TotalDuration));
                Console.WriteLine(pad + " Inbound Calls : " + InboundCalls  + "   Inbound Duration : " + FormatedDuration(InboundDuration));
                Console.WriteLine(pad + "Outbound Calls : " + OutboundCalls + "  Outbound Duration : " + FormatedDuration(OutboundDuration));
            }


            foreach (PhoneNumber phoneNumber in PhoneNumbers) 
            {
                phoneNumber.WriteCallerStats(callRecordCount);
            }
        }

        public void WriteInlinePharmacyInfo()
        {
            Console.WriteLine(PharmacyInfoString());
        }

        public string PharmacyInfoString()
        {
            return $" {Name} - {PrimaryPhoneNumber} - Npi {Npi}";
        }

        public void AddPhoneNumber(PhoneNumber newPhoneNumber)
        {
            // if the phone number already exists in the list replace it with the new phone number
            var foundPhoneNumber = PhoneNumbers.Find(pn => pn.Number == newPhoneNumber.Number);
            
            if (foundPhoneNumber != null)
            {
                
            }
            else // add the new phone number to the list of phone numbers
            {
                // add caller stats to pharmacy over-all stats
                TotalCalls += newPhoneNumber.TotalCalls;
                TotalDuration += newPhoneNumber.TotalDuration;
                InboundCalls += newPhoneNumber.InboundCalls;
                OutboundCalls += newPhoneNumber.OutboundCalls;
                InboundDuration += newPhoneNumber.InboundDuration;
                OutboundDuration += newPhoneNumber.OutboundDuration;

                PhoneNumbers.Add(newPhoneNumber);
            }
        }

        public void AddPhoneNumbers(List<PhoneNumber> phoneNumbers)
        {
            foreach (PhoneNumber phoneNumber in phoneNumbers)
            {
                AddPhoneNumber(phoneNumber);
            }
        }

        public void RemovePhoneNumber(string phoneNumber)
        {
            // remove a phone number from PhoneNumbers 
        }

        public string FormatedDuration(int duration)
        {
            string formattedDuration = duration.ToString();

            if (duration > 60 && duration < 3600)
            {
                formattedDuration = (duration / 60).ToString() + "m " + (duration % 60).ToString() + "s";
            }
            else if (duration >= 3600)
            {
                formattedDuration = (duration / 3600).ToString() + "h " + ((duration % 3600) / 60).ToString() + "m " + ((duration % 3600) % 60).ToString() + "s";
            }
            else
            {
                formattedDuration = duration.ToString() + "s";
            }

            return formattedDuration;
        }
    }
}
