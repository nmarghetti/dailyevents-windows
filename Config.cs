using System;

namespace DailyEvents
{
  static public class Config
  {
    static public string ApiEntryPoint
    {
      get {
        return Properties.Settings.Default.ApiEntryPoint;
      }
    }
    
    static public dynamic Groups
    {
      get {
        string json = Properties.Settings.Default.Groups;
        return Json.Deserialize(json);
      }
      set {
        string json = Json.Serialize(value);
        Properties.Settings.Default.Groups = json;
        Properties.Settings.Default.Save();
      }
    }

    static public string CurrentGroup
    {
      get {
        return Properties.Settings.Default.CurrentGroup;
      }
      set {
        Properties.Settings.Default.CurrentGroup = value;
        Properties.Settings.Default.Save();
      }
    }
  }
}
