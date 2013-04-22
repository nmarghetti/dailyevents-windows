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
      DateTime dateTime = Jan1st1970.AddSeconds(Math.Round(Convert.ToDouble(timestamp) / 1000)).ToLocalTime();
      return dateTime.ToString("HH:mm");
    }
  }
}
