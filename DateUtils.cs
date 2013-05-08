using System;

namespace DailyEvents
{
  static public class DateUtils
  {
    static private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0);

    static public string CurrentTimeMillis()
    {
      return (DateTime.UtcNow - Jan1st1970).TotalMilliseconds.ToString();
    }

    static public string FormatTime(string timestamp)
    {
      DateTime dateTime = ToDateTime(timestamp);
      return dateTime.ToString("HH:mm");
    }

    static public string GetUtcOffsetInMinutes(string timestamp)
    {
      DateTime dateTime = ToDateTime(timestamp);
      TimeSpan timeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
      int offset = timeSpan.Negate().Hours * 60;
      return offset.ToString();
    }

    static private DateTime ToDateTime(string timestamp)
    {
      double seconds = Math.Round(Convert.ToDouble(timestamp) / 1000);
      return Jan1st1970.AddSeconds(seconds).ToLocalTime();
    }
  }
}
