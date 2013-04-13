using System;

namespace DailyEvents
{
  public class DateUtils
  {
    static private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private DateUtils() {}

    static public string CurrentTimeMillis()
    {
      string millis = (DateTime.UtcNow - Jan1st1970).TotalMilliseconds.ToString();
      millis = millis.Substring(0, millis.IndexOf('.'));
      return millis;
    }
  }
}

