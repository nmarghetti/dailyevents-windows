using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DailyEvents
{
  static class DailyEvents
  {
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      DailyEventsContext context = new DailyEventsContext();
      Application.Run(context);
    }
  }
}
