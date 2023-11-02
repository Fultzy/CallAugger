using System;
using System.Collections.Generic;

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
        public string Anniversary { get; set; }
        public string PrimaryPhoneNumber { get; set; }

        // The Frist phone number in this is should always be the primary phone number. 
        public List<PhoneNumber> PhoneNumbers = new List<PhoneNumber>();
        public int TotalCalls = 0;
        public int TotalDuration = 0;
        
        public int InboundCalls = 0;
        public int InboundDuration = 0;

        public int OutboundCalls = 0;
        public int OutboundDuration = 0;

        // writes out all the info for the pharmacy to the console
        // if true is passed to this method, it will also write out the stats for each phone number
        public void WritePharmacyInfo(int callRecordCount = 0)
        {
            string pad = "     ";


            // write out all the info for the pharmacy to the console
            Console.WriteLine(pad + "\n  ~~~~~~~~~~ " + Name.Trim() + " ~~~~~~~~~~\n");
                                   
            // write Pharmacy Medical Creds
            Console.WriteLine(pad + "        Npi        Ncpdp        Dea");
            Console.WriteLine(pad + "    " + Npi + " || " + Ncpdp + " || " + Dea);
            Console.WriteLine(pad + "    {0} {1}, {2} {3}\n", Address,  City, State, Zip);

            Console.WriteLine(pad + "    Anniversary: {0}\n", Anniversary);

            // write Pharmacy Contact Info
            if (ContactName2 == null) Console.WriteLine(pad + "  Contact: {0}", ContactName1);
            else Console.WriteLine(pad + "  Contact(s): {0} {1}", ContactName1, ContactName2);


            Console.WriteLine(pad + "  Main Phone: {0}\n", FormatedPhoneNumber(PrimaryPhoneNumber));

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

        public string PharmacyInfoString(int nameLengthRequest = 0, int callLengthRequest = 0)
        {
            string namePadding = "";
            for (int i = Name.Length; i < nameLengthRequest; i++)
            {
                namePadding += " ";
            }

            string callPadding = "";
            for (int i = TotalCalls.ToString().Length; i < callLengthRequest; i++)
            {
                callPadding += " ";
            }

            return $"{Name}{namePadding} ~ {FormatedPhoneNumber(PrimaryPhoneNumber)} ~ {Npi} ~   {callPadding}{TotalCalls}   ~  {FormatedDuration(TotalDuration)}";
        }

        public void AddPhoneNumber(PhoneNumber newPhoneNumber)
        {
            // if the phone number already exists in the list replace it with the new phone number
            var foundPhoneNumber = PhoneNumbers.Find(pn => pn.Number == newPhoneNumber.Number);
            
            if (foundPhoneNumber != null)
            {
                // what is this for?!?!?!
            }
            else // add the new phone number to the list of phone numbers
            {
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


        public string FormatedPhoneNumber(string number)
        {
            string formattedNumber = number;
            if (number.Length == 10)
            {
                formattedNumber = "(" + number.Substring(0, 3) + ") " + number.Substring(3, 3) + "-" + number.Substring(6, 4);
            }

            return formattedNumber;
        }

        public string FormatedDuration(int duration)
        {
            TimeSpan time = TimeSpan.FromSeconds(duration);
            if (time.Hours == 0)
                return $"{time.Minutes}m {time.Seconds}s";
            else
                return $"{time.Hours + (time.Days * 24)}h {time.Minutes}m {time.Seconds}s";
        }

        public float AverageDuration()
        {
            if (TotalDuration == 0) return 0;

            return TotalDuration / TotalCalls;
        }
    }
}
