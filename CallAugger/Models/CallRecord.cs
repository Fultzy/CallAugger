using System;

namespace CallAugger
{
    public class CallRecord
    {
        public string CallType { get; set; }
        public int Duration { get; set; }
        public DateTime Time { get; set; }



        public string UserName { get; set; }
        public string UserExtention { get; set; }
        public string TransferUser { get; set; }
        public string Caller { get; set; }


        public void WriteInfo()
        {
            Console.WriteLine("Caller: " + Caller);
            Console.WriteLine("User: " + UserName);
            Console.WriteLine("Call Type: " + CallType);
            Console.WriteLine("Time: " + Time);
            Console.WriteLine("Duration: " + Duration);
        }
    }
}
