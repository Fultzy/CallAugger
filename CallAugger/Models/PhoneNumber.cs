using CallAugger.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger
{
    public class PhoneNumber
    {
        public int id = 0;
        public int PharmacyID { get; set; }
        public string Number { get; set; }
        public string State { get; set; }

        public bool IsPrimary = false;
        public bool IsFax = false;

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
            return TotalDuration / TotalCalls;
        }

        public string FormattedPhoneNumber()
        {
            return Formatter.PhoneNumber(Number);
        }

        public string FormattedAverageDuration(int requestedLenth = 0)
        {
            return Formatter.Duration(Convert.ToInt32(AverageDuration()));
        }

        public string FormattedTotalDuration(int requestedLenth = 0)
        {
            return Formatter.Duration(TotalDuration, requestedLenth);
        }

        public string FormattedInboundDuration(int requestedLenth = 0)
        {
            return Formatter.Duration(InboundDuration, requestedLenth);
        }

        public string FormattedOutboundDuration(int requestedLenth = 0)
        {
            return Formatter.Duration(OutboundDuration, requestedLenth);
        }

        public string InlineCallerStatString(int callLengthRequest = 0, int durationLengthRequest = 0)
        {
            string callPadding = "";
            for (int i = TotalCalls.ToString().Length; i < callLengthRequest; i++)
            {
                callPadding += " ";
            }

            return $"{FormattedPhoneNumber()}    ~    {callPadding}{TotalCalls}    ~    {FormattedTotalDuration(durationLengthRequest)}";
        }

        internal object InlineDetails()
        {
            return $"{Number} ID:{id} State:{State} IsFax?{IsFax} IsPrimary?{IsPrimary} PhID:{PharmacyID}";
        }

        public void WriteCallerStats(int topCallCount = 0)
        {
            if (Number == null) return;
            var pad = "       ";
            var isPrimaryFlag = IsPrimary == true ? " ~ Primary" : "";
            var isFaxFlag = IsFax == true ? " ~ FAX" : "";
            var noBitchesFlag = TotalCalls == 0 ? " ~ 0 Calls" : "";

            Console.WriteLine(pad + $"\nid: {id} ~~~~~~~~~ " + FormattedPhoneNumber() + isPrimaryFlag + isFaxFlag + noBitchesFlag + " ~~~~~~~~~~\n");

            int padding = FormattedTotalDuration().Length;

            if (noBitchesFlag == "")
            {
                Console.WriteLine(pad + "     All Calls : {0}  -  {1}", TotalCalls, FormattedTotalDuration());
                Console.WriteLine(pad + " Inbound Calls : {0}  -  {1}", InboundCalls, FormattedInboundDuration(padding));
                Console.WriteLine(pad + "Outbound Calls : {0}  -  {1}\n", OutboundCalls, FormattedOutboundDuration(padding));


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
        }

        public string ToCsv()
        {
            return $"{Number}:{State}";
        }
    }
}
