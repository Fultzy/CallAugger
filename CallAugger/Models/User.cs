using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallAugger
{
    public class User
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Extention { get; set; }

        public int TotalCalls = 0;
        public int TotalInboundCalls = 0;
        public int TotalOutboundCalls = 0;

        public int TotalCallTime = 0;
        public int TotalInboundTalkTime = 0;
        public int TotalOutboundTalkTime = 0;

        public List<CallRecord> CallRecords { get; set; } = new List<CallRecord>();


        public void AddCall(CallRecord callRecord)
        {

            TotalCalls++;
            TotalCallTime += callRecord.Duration;

            if (callRecord.CallType == "Inbound call")
            {
                TotalInboundCalls++;
                TotalInboundTalkTime += callRecord.Duration;
            }
            else
            {
                TotalOutboundCalls++;
                TotalOutboundTalkTime += callRecord.Duration;
            }

            CallRecords.Add(callRecord);
        }
  
        public void AddCalls(List<CallRecord> callRecords)
        {
            foreach (CallRecord callRecord in callRecords)
            {
                AddCall(callRecord);
            }
        }

        public string FormatedAverageCallTime()
        {
            double averageTime = (double)TotalCallTime / (double)TotalCalls;
            int averageTimeInSeconds = Convert.ToInt32(averageTime);

            return FormatedDuration(averageTimeInSeconds);
        }

        public (double, double, double) OverSixtyMinutesCallsPercentages()
        {
            int overHourCalls = 0;
            int overHourInboundCalls = 0;
            int overHourOutboundCalls = 0;

            foreach (CallRecord call in CallRecords)
            {
                if (call.Duration > 3600)
                {
                    overHourCalls++;
                    if (call.CallType == "Inbound call")
                    {
                        overHourInboundCalls++;
                    }
                    else
                    {
                        overHourOutboundCalls++;
                    }
                }
            }
            return (
                Math.Round((double)overHourCalls / (double)TotalCalls * 100, 2),
                Math.Round((double)overHourInboundCalls / (double)TotalInboundCalls * 100, 2),
                Math.Round((double)overHourOutboundCalls / (double)TotalOutboundCalls * 100, 2)
                );
        }

        public (double, double, double) OverThirtyMinuteCallsPercentages()
        {
            int overThirtyMinuteCalls = 0;
            int overThirtyMinuteInboundCalls = 0;
            int overThirtyMinuteOutboundCalls = 0;

            foreach (CallRecord call in CallRecords)
            {
                if (call.Duration > 1800)
                {
                    overThirtyMinuteCalls++;
                    if (call.CallType == "Inbound call")
                    {
                        overThirtyMinuteInboundCalls++;
                    }
                    else
                    {
                        overThirtyMinuteOutboundCalls++;
                    }
                }
            }
            return (
                Math.Round((double)overThirtyMinuteCalls / (double)TotalCalls * 100, 2),
                Math.Round((double)overThirtyMinuteInboundCalls / (double)TotalInboundCalls * 100, 2),
                Math.Round((double)overThirtyMinuteOutboundCalls / (double)TotalOutboundCalls * 100, 2)
                );
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

        public void WriteUserStats()
        {
            var pad = "      ";

            Console.WriteLine($"{pad}               ~~~~~~~~~~ " + Name + " ~~~~~~~~~~\n");

            Console.WriteLine($"{pad}      Total Calls: {TotalCalls}  ~  {FormatedDuration(TotalCallTime)}  ~  Average: {FormatedAverageCallTime()}\n");
            
            Console.WriteLine($"{pad}      Inbound Calls: {TotalInboundCalls}  ~  {FormatedDuration(TotalInboundTalkTime)}");
            
            Console.WriteLine($"{pad}     Outbound Calls: {TotalOutboundCalls}  ~  {FormatedDuration(TotalOutboundTalkTime)}\n");
            
            Console.WriteLine($"{pad} Over 30 Minute Calls: {OverThirtyMinuteCallsPercentages().Item1}%  ~  Inbound: {OverThirtyMinuteCallsPercentages().Item2}%  ~  Outbound: {OverThirtyMinuteCallsPercentages().Item3}%");

            Console.WriteLine($"{pad} Over 60 Minute Calls: {OverSixtyMinutesCallsPercentages().Item1}%  ~  Inbound: {OverSixtyMinutesCallsPercentages().Item2}%  ~  Outbound: {OverSixtyMinutesCallsPercentages().Item3}%");
        }

        public string InlineUserInfo()
        {
            return $"{Name} - Total Calls: {TotalCalls} - Total Duration: {FormatedDuration(TotalCallTime)}";
        }
    }
}
