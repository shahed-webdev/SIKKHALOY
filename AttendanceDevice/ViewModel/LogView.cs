using System;

namespace AttendanceDevice.ViewModel
{
    public class LogView
    {
        public int DeviceID { get; set; }
        public TimeSpan Entry_Time { get; set; }
        public string Entry_Date { get; set; }
        public string Entry_Day { get; set; }
        public DateTime Entry_DateTime { get; set; }
    }
}
