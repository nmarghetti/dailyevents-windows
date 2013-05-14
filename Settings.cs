using System;
using System.Collections.Generic;

namespace DailyEvents
{
  static public class Settings
  {
    static public string Username
    {
      get {
        string username = Properties.Settings.Default.Username;
        return username.Trim().Length > 0 ? username : Environment.UserName;
      }
      set {
        Properties.Settings.Default.Username = value;
        Properties.Settings.Default.Save();
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
    
    static public dynamic SortedGroups
    {
      get {
        List<string> groups = new List<string>();
        foreach (var group in Groups.Values)
        {
          groups.Add(group);
        }
        groups.Sort();
        return groups;
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