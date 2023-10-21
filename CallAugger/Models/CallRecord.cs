using Microsoft.Office.Interop.Word;
using System;

namespace CallAugger
{
    public class CallRecord
    {
        public int id { get; set; }
        public int PhoneNumberID { get; set; }
        public int UserID { get; set; }

        public string CallType { get; set; }
        public int Duration { get; set; }
        public DateTime Time { get; set; }

        public string UserName { get; set; }
        public string UserExtention { get; set; }
        public string TransferUser { get; set; }
        public string Caller { get; set; }



        public void WriteInfo()
        {
            Console.WriteLine("{0}:  {1}  -  {2}  -  {3} ~ {4}: {5} || {6}:{7}", id, Time, CallType, Duration, PhoneNumberID, Caller, UserID, UserName);
        }
    }
}
