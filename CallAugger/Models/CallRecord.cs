using System;

namespace CallAugger
{
    public class CallRecord
    {
        public int id = 0;
        public int PhoneNumberID { get; set; }
        public int UserID { get; set; }


        public string CallType { get; set; }
        public int Duration { get; set; }
        public DateTime Time { get; set; }
        public string State { get; set; }


        public string UserName { get; set; }
        public string UserExtention { get; set; }
        public string TransferUser { get; set; }
        public string Caller { get; set; }



        public string InLineInfo()
        {
            var type = CallType;
            if (type == "Inbound call") type = "Inbound ";
            if (type == "Outbound call") type = "Outbound";

            return $"{Time.ToString("MM/dd/yyyy hh:mm tt")}  ~  {type}  ~  {FormatedDuration(Duration)}  ~  {UserName}";
        }

        internal object InlineDetails()
        {
            return $"DateTime:{Time.ToString("MM/dd/yyyy hh:mm tt")}, Type:{CallType}, Duration:{Duration}, UserID:{UserID}, Username:{UserName}, UserExt:{UserExtention}, TransferUser:{TransferUser}, CallerID:{PhoneNumberID}, Caller:{Caller}, State:{State}";
        }

        public string ToCsv()
        {
            return $"{Time.ToString("MM/dd/yyyy hh:mm tt")},{CallType},{Duration},{UserName},{UserExtention},{TransferUser},{Caller},{State}";
        }

        public void WriteInfo()
        {
            Console.WriteLine(InLineInfo());
        }

        public string FormatedDuration(int duration)
        {
            TimeSpan time = TimeSpan.FromSeconds(duration);
            if (time.Hours == 0)
                return $"{time.Minutes}m {time.Seconds}s";
            else
                return $"{time.Hours + (time.Days * 24)}h {time.Minutes}m {time.Seconds}s";
        }

        public bool IsWeekend()
        {
            if (Time.DayOfWeek == DayOfWeek.Saturday || Time.DayOfWeek == DayOfWeek.Sunday)
                return true;
            else
                return false;
        }

        public bool IsInternal()
        {
            if (Caller.Length == UserExtention.Length || Caller == "16308690873")
                return true;
            else
                return false;
        }

    }
}
