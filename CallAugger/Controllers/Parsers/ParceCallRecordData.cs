using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger.Controllers.Parsers
{
    internal class ParseCallRecordData : Parse
    {

        // This method takes a call record and returns a PhoneNumber object
        public PhoneNumber ParseCallRecordCaller(CallRecord callRecord, List<PhoneNumber> phoneNumbers)
        {
            String phoneNumberstr = callRecord.Caller;
            PhoneNumber thisPhoneNumber = phoneNumbers.Find(pn => pn.Number == phoneNumberstr);

            // if number is not make one
            if (thisPhoneNumber == null)
            {
                thisPhoneNumber = new PhoneNumber()
                {
                    Number = phoneNumberstr,
                };
            }

            return thisPhoneNumber;
        }

        // This method takes a call record and returns a User object
        public User ParseCallRecordUser(CallRecord callRecord, List<User> users)
        {
            String userName = callRecord.UserName;
            User thisUser = users.Find(pu => pu.Name == userName);

            // if user exists add call to it, if not make one and add call
            if (thisUser == null)
            {
                thisUser = new User()
                {
                    Name = userName,
                    Extention = callRecord.UserExtention
                };
            }

            return thisUser;
        }
    }
}
