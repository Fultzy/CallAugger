using System;
using System.Collections.Generic;
using System.Linq;

namespace CallAugger
{
    public class User
    {
        public int id = 0;
        public string Name { get; set; }
        public string Extention { get; set; }

        public int TotalCalls = 0;
        public int TotalDuration = 0;

        public int InboundCalls = 0;
        public int InboundDuration = 0;

        public int OutboundCalls = 0;
        public int OutboundDuration = 0;

        public int CallsOver30 = 0;
        public int CallsOver60 = 0;

        public int WeekendCalls = 0;
        public int InternalCalls = 0;


        public List<CallRecord> CallRecords { get; set; } = new List<CallRecord>();

        public void AddCall(CallRecord callRecord)
        {

            TotalCalls++;
            TotalDuration += callRecord.Duration;

            if (callRecord.Duration > 1800) CallsOver30++;
            if (callRecord.Duration > 3600) CallsOver60++;
            
            if (callRecord.IsWeekend() && callRecord.IsInternal() == false) WeekendCalls++;
            if (callRecord.IsInternal()) InternalCalls++;


            if (callRecord.CallType == "Inbound call")
            {
                InboundCalls++;
                InboundDuration += callRecord.Duration;
            }
            else
            {
                OutboundCalls++;
                OutboundDuration += callRecord.Duration;
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

        public string AverageCallTime()
        {
            if (TotalCalls == 0) return "0s";

            double averageTime = (double)TotalDuration / (double)TotalCalls;
            if (averageTime < 1) Console.WriteLine("OPPS!");
            int averageTimeInSeconds = Convert.ToInt32(averageTime);

            return FormatedDuration(averageTimeInSeconds);
        }

        public int AdjustedCalls()
        {
            return TotalCalls - InternalCalls - WeekendCalls;
        }

        public float AverageDuration()
        {
            if (TotalCalls == 0) return 0; // prevent divide by zero error
            return TotalDuration / TotalCalls;
        }

        public string Over30Percentage()
        {
            return (float)CallsOver30 / (float)TotalCalls * 100 + "%";
        }

        public string Over60Percentage()
        {
            return (float)CallsOver60 / (float)TotalCalls * 100 + "%";
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
                Math.Round((double)overHourInboundCalls / (double)InboundCalls * 100, 2),
                Math.Round((double)overHourOutboundCalls / (double)OutboundCalls * 100, 2)
                );
        }

        public (double, double, double) OverThirtyMinutesCallsPercentages()
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
                Math.Round((double)overThirtyMinuteInboundCalls / (double)InboundCalls * 100, 2),
                Math.Round((double)overThirtyMinuteOutboundCalls / (double)OutboundCalls * 100, 2)
                );
        }

        public string FormatedDuration(int duration)
        {
            TimeSpan time = TimeSpan.FromSeconds(duration);
            if (time.Hours == 0 && time.Days == 0)
                return $"{time.Minutes}m {time.Seconds}s";
            else
                return $"{time.Hours + (time.Days * 24)}h {time.Minutes}m {time.Seconds}s";
        }

        public void WriteUserStats(int callRecordCount = 0)
        {
            var pad = "      ";

            var over30 = OverThirtyMinutesCallsPercentages();
            var over60 = OverSixtyMinutesCallsPercentages();

            Console.WriteLine($"\n{pad}               ~~~~~~~~~~ " + Name + " ~~~~~~~~~~\n");

            Console.WriteLine($"{pad}      Total Calls: {TotalCalls}  ~  {FormatedDuration(TotalDuration)}  ~  Average: {AverageCallTime()}\n");
            
            Console.WriteLine($"{pad}      Inbound Calls: {InboundCalls}  ~  {FormatedDuration(InboundDuration)}");
            
            Console.WriteLine($"{pad}     Outbound Calls: {OutboundCalls}  ~  {FormatedDuration(OutboundDuration)}");
            Console.WriteLine($"{pad}      Weekend Calls: {WeekendCalls}  ~  Internal Calls: {InternalCalls}\n");
            
            Console.WriteLine($"{pad} Over 30 Minute Calls: {over30.Item1}%  ~  Inbound: {over30.Item2}%  ~  Outbound: {over30.Item3}%");

            Console.WriteLine($"{pad} Over 60 Minute Calls: {over60.Item1}%  ~  Inbound: {over60.Item2}%  ~  Outbound: {over60.Item3}%");
            

            Console.WriteLine($"\n{pad}      Top {callRecordCount} Calls:");
            foreach (CallRecord call in CallRecords.OrderByDescending(call => call.Duration).Take(callRecordCount))
            {
                Console.WriteLine($"{pad} {call.FormatedDuration(call.Duration)}  ~  {call.Time}  ~  {call.CallType.Split(' ')[0]}  ~  {call.Caller}");
            }
        }

        public string InlineStats()
        {
            return $"{Name} - Total Calls: {TotalCalls} - Total Duration: {FormatedDuration(TotalDuration)}";
        }

        internal object InlineDetails()
        {
            return $"{Name} ID:{id} Extention:{Extention}";
        }

        public string ToCsv()
        {
            return $"{Name},{Extention}";
        }

        public string NameL()
        {
            if (Name == null) return "Invalid Name";
            if (Name == "-- AVERAGE --") return Name;
            if (Name == "-- TOTAL --") return Name;

            string[] name = Name.Split(' ');
            return name[0] + " " + name[1][0];
        }

    }

}
