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
      Assert.NotNull(timestamp);
      Assert.IsNotEmpty(timestamp);
    }
    
    [Test()]
    public void should_format_current_time()
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      string time = DateUtils.FormatTime(timestamp, "-120");
      Assert.NotNull(time);
      Assert.IsNotEmpty(time);
      Assert.AreEqual(5, time.Length);
    }

    [Test()]
    public void should_format_time_with_decimals()
    {
      Assert.NotNull(DateUtils.FormatTime("1373007551225.54", "-120"));
      Assert.IsNotEmpty(DateUtils.FormatTime("1373007551225,54", "-120"));
    }

    [Test()]
    public void should_get_utc_offset_in_minutes()
    {
      string timestamp = DateUtils.CurrentTimeMillis();
      string utcOffset = DateUtils.GetUtcOffsetInMinutes(timestamp);
      Assert.NotNull(utcOffset);
      Assert.IsNotEmpty(utcOffset);

      int minutes = 0;
      Int32.TryParse(utcOffset, out minutes);
      Assert.IsTrue(minutes != 0);
    }
  }
}
