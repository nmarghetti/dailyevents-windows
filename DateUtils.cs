using System;

namespace DailyEvents
{
  public class DateUtils
  {
    static private readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private DateUtils() {}

    static public string CurrentTimeMillis()
    {
      return (DateTime.UtcNow - Jan1st1970).TotalMilliseconds.ToString();
    }
  }
}
