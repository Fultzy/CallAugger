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

        public float AverageDuration(int TotalDuration, int TotalCalls)
        {
            return TotalDuration / TotalCalls;
        }

        public string FormatedPhoneNumber()
        {
            string formattedNumber = Number;
            if (Number.Length == 10)
            {
                formattedNumber = "(" + Number.Substring(0, 3) + ") " + Number.Substring(3, 3) + "-" + Number.Substring(6, 4);
            }

            return formattedNumber;
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

        public void WriteCallerStats(int topCallCount = 0)
        {
            var pad = "       ";
            Console.WriteLine(pad + $"\n    ~~~~~~~~~ " + FormatedPhoneNumber() + " ~~~~~~~~~~\n");

            Console.WriteLine(pad + "     All Calls : {0}  -  {1}", TotalCalls, FormatedDuration(TotalDuration));
            Console.WriteLine(pad + " Inbound Calls : {0}  -  {1}", InboundCalls, FormatedDuration(InboundDuration));
            Console.WriteLine(pad + "Outbound Calls : {0}  -  {1}\n", OutboundCalls, FormatedDuration(OutboundDuration));


            if (topCallCount > 0)
            {
                Console.WriteLine(pad + $"Top {topCallCount} calls:");

                int callCount = 0;
                foreach (CallRecord call in CallRecords.OrderByDescending(pn => pn.Duration))
                {
                    
                    callCount++;
                    Console.WriteLine("   {0}: {1}  -  {2}  -  {3}  -  {4}", callCount, call.Time, call.CallType, FormatedDuration(call.Duration), call.UserName);
                    if (callCount >= topCallCount) break;
                }
            }
        }

        public string InlineCallerStatString()
        {
            return $"{FormatedPhoneNumber()} - Calls: {TotalCalls} : {FormatedDuration(TotalDuration)}";
        }

    }
}
