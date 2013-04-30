using NUnit.Framework;
using System;

namespace DailyEvents
{
  [TestFixture()]
  public class DateUtilsTest
  {

    [Test()]
    public void should_get_current_time_in_milliseconds()
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      Assert.IsNotNullOrEmpty(timestamp);
    }
    
    [Test()]
    public void should_format_current_time()
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      string time = DateUtils.FormatTime(timestamp);
      Assert.IsNotNullOrEmpty(time);
      Assert.AreEqual(5, time.Length);
    }
    
    [Test()]
    public void should_get_utc_offset_in_minutes()
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      int utcOffset = DateUtils.GetUtcOffsetInMinutes(timestamp);
      Assert.IsTrue(utcOffset != 0);
    }
  }
}
