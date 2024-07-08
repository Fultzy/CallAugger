using CallAugger.Utilities;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CallAugger
{
    public class Pharmacy
    {

        public int id = 0;
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
        public string FaxNumber { get; set; }

        public List<PhoneNumber> PhoneNumbers = new List<PhoneNumber>();

        public int TotalCalls = 0;
        public int TotalDuration = 0;
        
        public int InboundCalls = 0;
        public int InboundDuration = 0;

        public int OutboundCalls = 0;
        public int OutboundDuration = 0;

        public void WritePharmacyInfo(int callRecordCount = 0)
        {
            string pad = "     ";
            
            // write out all the info for the pharmacy to the console
            Console.WriteLine(pad + "\n  ~~~~~~~~~~ " + Name.Trim() + " ~~~~~~~~~~");
            Console.WriteLine("id: " + id);

            // write Pharmacy Medical Creds
            Console.WriteLine(pad + "        Npi        Ncpdp        Dea");
            Console.WriteLine(pad + "    " + Npi + " || " + Ncpdp + " || " + Dea);
            Console.WriteLine(pad + "    {0} {1}, {2} {3}\n", Address,  City, State, Zip);

            Console.WriteLine(pad + "    Anniversary: {0}\n", Anniversary);

            // write Pharmacy Contact Info
            if (ContactName2 == "null") 
                Console.WriteLine(pad + "  Contact: {0}\n", ContactName1);
            else 
                Console.WriteLine(pad + "  Contact(s): {0}, {1}\n", ContactName1, ContactName2);


            if (PhoneNumbers.Count > 1)
            {
                // write phrmacy Total call states
                Console.WriteLine(pad + "   Total Calls : " + TotalCalls  +   "     Total Duration : " + FormattedTotalDuration());
                Console.WriteLine(pad + " Inbound Calls : " + InboundCalls  + "   Inbound Duration : " + FormattedInboundDuration());
                Console.WriteLine(pad + "Outbound Calls : " + OutboundCalls + "  Outbound Duration : " + FormattedOutboundDuration());
            }


            foreach (PhoneNumber phoneNumber in PhoneNumbers.OrderByDescending(pn => pn.TotalDuration)) 
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

            return $"{Name}{namePadding} ~ {FormattedPhoneNumber(PrimaryPhoneNumber)} ~ {Npi} ~   {callPadding}{TotalCalls}   ~  {FormattedTotalDuration()}";
        }

        public string InlineDetails()
        {

            return $"{Name} ID:{id} PrimaryPhone:{PrimaryPhoneNumber} Npi:{Npi} Ncpdp:{Ncpdp} Dea:{Dea} Address:{Address} City:{City} State:{State} ZipCode:{Zip} Contact1:{ContactName1} Contact2:{ContactName2} Anniversary:{Anniversary} Fax:{FaxNumber}";
        }

        public string ToCsv()
        {
            var name = Name;
            if (Name.Contains(",")) name = Name.Replace(",", " ");

            return $"{name},{Npi},{Dea},{Ncpdp},{Address},{City},{State},{Zip},{ContactName1},{ContactName2},{Anniversary},{PrimaryPhoneNumber},{FaxNumber}";
        }

        public Pharmacy FromCsv(string[] row)
        {
            Name = row[0];
            Npi = row[1];
            Dea = row[2];
            Ncpdp = row[3];
            Address = row[4];
            City = row[5];
            State = row[6];
            Zip = row[7];
            ContactName1 = row[8];
            ContactName2 = row[9];
            Anniversary = row[10];
            PrimaryPhoneNumber = row[11];
            FaxNumber = row[12];

            return this;
        }

        public void AddPhoneNumber(PhoneNumber newPhoneNumber)
        {
            // if the phone number already exists in the list replace it with the new phone number
            var foundPhoneNumber = PhoneNumbers.Find(pn => pn.Number == newPhoneNumber.Number);
            
            if (foundPhoneNumber == null)
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

        public string FormattedPhoneNumber(string number)
        {
            return Formatter.PhoneNumber(number);
        }
        
        public string FormattedTotalDuration()
        {
            return Formatter.Duration(TotalDuration);
        }

        public string FormattedInboundDuration()
        {
            return Formatter.Duration(InboundDuration);
        }

        public string FormattedOutboundDuration()
        {
            return Formatter.Duration(OutboundDuration);
        }

        public string FormattedAverageDuration()
        {
            return Formatter.Duration(Convert.ToInt32(AverageDuration()));
        }

        public float AverageDuration()
        {
            if (TotalDuration == 0) return 0;

            return TotalDuration / TotalCalls;
        }
    }
}
