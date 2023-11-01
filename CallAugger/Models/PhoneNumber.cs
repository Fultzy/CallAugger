using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CallAugger
{
    public class PhoneNumber
    {
        public int id { get; set; }
        public int PharmacyID { get; set; }

        public string Number { get; set; }
        
        public int TotalCalls = 0;
        public int InboundCalls = 0;
        public int OutboundCalls = 0;
        public int TotalDuration = 0;
        public int InboundDuration = 0;
        public int OutboundDuration = 0;

        public List<CallRecord> CallRecords = new List<CallRecord>();

        public void AddCall(CallRecord newCall)
        {
            TotalCalls++;
            TotalDuration += newCall.Duration;
            
            if (newCall.CallType == "Inbound call")
            {
                InboundCalls++;
                InboundDuration += newCall.Duration;
            }
            else
            {
                OutboundCalls++;
                OutboundDuration += newCall.Duration;
            }

            CallRecords.Add(newCall);
        }

        public void AddCalls(List<CallRecord> NewCallList)
        {
            foreach (CallRecord newCall in NewCallList)
            {
                AddCall(newCall);
            }
        }

        public float AverageDuration()
        {
            if (TotalCalls == 0) return 0;
            if (TotalDuration == 0) return 0;
            return TotalDuration / TotalCalls;
        }

        public string FormatedPhoneNumber()
        {
            if (Number == null) return "Invalid Number"; ;
            string formattedNumber = Number;

            if (Number.Length == 10)
            {
                formattedNumber = "(" + Number.Substring(0, 3) + ") " + Number.Substring(3, 3) + "-" + Number.Substring(6, 4);
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

        public void WriteCallerStats(int topCallCount = 0)
        {
            if (Number == null) return;
            var pad = "       ";
            Console.WriteLine(pad + $"\n    ~~~~~~~~~ " + FormatedPhoneNumber() + " ~~~~~~~~~~\n");

            Console.WriteLine(pad + "     All Calls : {0}  -  {1}", TotalCalls, FormatedDuration(TotalDuration));
            Console.WriteLine(pad + " Inbound Calls : {0}  -  {1}", InboundCalls, FormatedDuration(InboundDuration));
            Console.WriteLine(pad + "Outbound Calls : {0}  -  {1}\n", OutboundCalls, FormatedDuration(OutboundDuration));


            if (topCallCount > 0)
            {
                Console.WriteLine(pad + $"Top {topCallCount} calls:             Type:          Duration:        Rep:");

                int callCount = 0;
                foreach (CallRecord call in CallRecords.OrderByDescending(pn => pn.Duration))
                {
                    callCount++;
                    string idPadding = callCount > 9 ? "" : " ";

                    Console.WriteLine(pad + call.InLineInfo());
                    if (callCount >= topCallCount) break;
                }
            }
        }

        public string InlineCallerStatString(int paddingRequest = 0)
        {
            // add a space to callspadding for the paddingrequest
            string callsPadding = "";
            for (int i = 0; i < paddingRequest; i++)
            {
                callsPadding += " ";
            }

            return $"{FormatedPhoneNumber()}    -    {TotalCalls}" + callsPadding + $"      -      {FormatedDuration(TotalDuration)}";
        }

    }
}
