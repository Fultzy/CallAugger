using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Parsers
{
    internal class ParseCallRecordData : Parse
    {
        public PhoneNumber ParseCallRecordCaller(CallRecord callRecord)
        {
            String phoneNumberString = callRecord.Caller;
            PhoneNumber thisPhoneNumber = ParsedPhoneNumbers.Find(pn => pn.Number == phoneNumberString);


            // Add the call record to the parced phone numbers list if it is already there
            if (thisPhoneNumber != null)
            {
                thisPhoneNumber.AddCall(callRecord);
            }
            else // Create a new PhoneNumber object
            {
                thisPhoneNumber = new PhoneNumber()
                {
                    Number = phoneNumberString,
                };

                thisPhoneNumber.AddCall(callRecord);
            }

            return thisPhoneNumber;
        }

        public User ParseCallRecordUser(CallRecord callRecord)
        {
            String userName = callRecord.UserName;
            User thisUser = null;


            // Add the call record to the parced users list if it is already there
            if (ParsedUsers.Any(pu => pu.Name == userName))
            {
                thisUser = ParsedUsers.Find(pu => pu.Name == userName);
                thisUser.AddCall(callRecord);
            }
            else // Create a new User object
            {
                thisUser = new User()
                {
                    Name = userName,
                    Extention = callRecord.UserExtention
                };

                thisUser.AddCall(callRecord);
            }

            return thisUser;
        }
    }
}
